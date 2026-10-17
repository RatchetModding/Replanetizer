using System;
using System.IO;
using System.Linq;
using LibReplanetizer.Models;
using LibReplanetizer.Serializers.Importers;
using Xunit;

namespace LibReplanetizer.Tests.Exporters
{
    public class WavefrontExporterTests
    {
        [Fact]
        public void ExportCollision_RoundTripsTypesCategoriesAndWinding()
        {
            const string input =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "v 2 0.5 0.5\nv 3 0.5 0.5\nv 2 1.5 0.5\n" +
                "v 0.5 2 0.5\nv 1.5 2 0.5\nv 0.5 3 0.5\n" +
                "usemtl RC_11_2_true_Standard\nf 1 2 3\n" +
                "usemtl RC_0_0_true_Hero\nf 4 5 6\n" +
                "usemtl RC_0_0_true_Unknown\nf 7 8 9\n";

            string firstPath = Path.GetTempFileName();
            string secondPath = Path.GetTempFileName();
            try
            {
                var importer = new WavefrontImporter();
                var exporter = new WavefrontExporter();
                Collision source = CollisionBuilder.BuildPerFace(Import(input));

                exporter.ExportCollision(firstPath, source);
                string firstExport = File.ReadAllText(firstPath);
                Assert.Contains("usemtl RC_11_2_true_Standard", firstExport);
                Assert.Contains("usemtl RC_0_0_true_Hero", firstExport);
                Assert.Contains("usemtl RC_0_0_true_Unknown", firstExport);
                Assert.Contains("mtllib " + Path.GetFileName(Path.ChangeExtension(firstPath, ".mtl")), firstExport);
                AssertPositionOnlyVertices(firstExport);
                var materials = ReadCollisionMaterials(firstPath);
                Assert.Equal(
                    new OpenTK.Mathematics.Vector3(0.75f, 0.25f, 0.25f),
                    materials["RC_11_2_true_Standard"]);
                Assert.Equal(
                    new OpenTK.Mathematics.Vector3(0.5f, 0.5f, 1.0f),
                    materials["RC_0_0_true_Hero"]);
                Assert.Equal(
                    new OpenTK.Mathematics.Vector3(1.0f, 0.25f, 0.75f),
                    materials["RC_0_0_true_Unknown"]);

                Collision imported = CollisionBuilder.BuildPerFace(importer.Import(firstPath));
                Assert.Equal(
                    new[]
                    {
                        ((byte) 0xCB, CollisionGeometryCategory.Standard),
                        ((byte) 0x80, CollisionGeometryCategory.Hero),
                        ((byte) 0x80, CollisionGeometryCategory.Unknown)
                    },
                    GetFaceMetadata(imported));

                exporter.ExportCollision(secondPath, imported);
                Assert.Equal(ReadObjWithoutMtlReference(firstPath), ReadObjWithoutMtlReference(secondPath));
                Assert.Equal(File.ReadAllText(Path.ChangeExtension(firstPath, ".mtl")), File.ReadAllText(Path.ChangeExtension(secondPath, ".mtl")));
            }
            finally
            {
                DeleteCollisionExportFiles(firstPath);
                DeleteCollisionExportFiles(secondPath);
            }
        }

        [Fact]
        public void ExportCollision_UsesBlackForUndefinedAndYellowForDefaultMaterials()
        {
            const string input =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "v 2.5 0.5 0.5\nv 3.5 0.5 0.5\nv 2.5 1.5 0.5\n" +
                "v 4.5 0.5 0.5\nv 5.5 0.5 0.5\nv 4.5 1.5 0.5\n" +
                "usemtl RC_6_0_false_Standard\nf 1 2 3\n" +
                "usemtl RC_14_0_false_Standard\nf 4 5 6\n" +
                "usemtl RC_31_0_false_Standard\nf 7 8 9\n";
            string path = Path.GetTempFileName();

            try
            {
                Collision source = CollisionBuilder.BuildPerFace(Import(input));
                new WavefrontExporter().ExportCollision(path, source);

                var materials = ReadCollisionMaterials(path);
                Assert.Equal(
                    new OpenTK.Mathematics.Vector3(0.0f, 0.0f, 0.0f),
                    materials["RC_6_0_false_Standard"]);
                Assert.Equal(
                    new OpenTK.Mathematics.Vector3(0.0f, 0.0f, 0.0f),
                    materials["RC_14_0_false_Standard"]);
                Assert.Equal(
                    new OpenTK.Mathematics.Vector3(1.0f, 1.0f, 0.0f),
                    materials["RC_31_0_false_Standard"]);
                AssertPositionOnlyVertices(File.ReadAllText(path));
            }
            finally
            {
                DeleteCollisionExportFiles(path);
            }
        }

        [Fact]
        public void ExportCollision_DisablesMtlWhenSettingIsOff()
        {
            Collision collision = CollisionBuilder.Build(Import(
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\n"));
            string path = Path.GetTempFileName();

            try
            {
                var settings = new ExporterLevelSettings { exportMtlFile = false };
                new WavefrontExporter(settings).ExportCollision(path, collision);

                string exported = File.ReadAllText(path);
                Assert.DoesNotContain("mtllib", exported);
                AssertPositionOnlyVertices(exported);
                Assert.False(File.Exists(Path.ChangeExtension(path, ".mtl")));
            }
            finally
            {
                DeleteCollisionExportFiles(path);
            }
        }

        [Fact]
        public void ExportCollision_PreservesOppositeWindingCoincidentFaces()
        {
            const string input =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\nf 1 3 2\n";
            string path = Path.GetTempFileName();

            try
            {
                Collision source = CollisionBuilder.BuildPerFace(Import(input));
                new WavefrontExporter().ExportCollision(path, source);
                ParsedModelGeometry exported = Import(File.ReadAllText(path));

                Assert.Equal(2, exported.faces.Count);
                double[] signedAreas = exported.faces.Select(face =>
                {
                    var a = exported.vertices[face.corners[0].pos];
                    var b = exported.vertices[face.corners[1].pos];
                    var c = exported.vertices[face.corners[2].pos];
                    return (double) (b.X - a.X) * (c.Y - a.Y) - (double) (b.Y - a.Y) * (c.X - a.X);
                }).ToArray();
                Assert.Contains(signedAreas, area => area > 0.0);
                Assert.Contains(signedAreas, area => area < 0.0);
            }
            finally
            {
                DeleteCollisionExportFiles(path);
            }
        }

        [Fact]
        public void ExportCollision_DeduplicatesCyclicRotationsAndPreservesMultiplicity()
        {
            const string input =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\nf 2 3 1\n";
            string path = Path.GetTempFileName();

            try
            {
                Collision source = CollisionBuilder.BuildPerFace(Import(input));
                new WavefrontExporter().ExportCollision(path, source);
                ParsedModelGeometry exported = Import(File.ReadAllText(path));

                Assert.Equal(2, exported.faces.Count);
                Assert.Equal(1, File.ReadLines(Path.ChangeExtension(path, ".mtl")).Count(line => line.StartsWith("newmtl ", StringComparison.Ordinal)));
                var firstPositions = exported.faces[0].corners.Select(corner => exported.vertices[corner.pos]).ToArray();
                var secondPositions = exported.faces[1].corners.Select(corner => exported.vertices[corner.pos]).ToArray();
                Assert.Equal(firstPositions, secondPositions);
            }
            finally
            {
                DeleteCollisionExportFiles(path);
            }
        }

        [Fact]
        public void ExportCollision_RejectsInconsistentFaceMetadata()
        {
            Collision collision = CollisionBuilder.Build(Import(
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\nf 1 2 3\n"));
            int vertex = checked((int) collision.indBuff[0]);
            collision.vertexBuffer[vertex * collision.vertexStride + 3] =
                CollisionVertexMetadata.Pack(new CollisionType(0x02), CollisionGeometryCategory.Standard);
            string path = Path.GetTempFileName();

            try
            {
                Assert.Throws<InvalidOperationException>(() => new WavefrontExporter().ExportCollision(path, collision));
            }
            finally
            {
                DeleteCollisionExportFiles(path);
            }
        }

        [Fact]
        public void ExportCollision_MultiCellRepeatedQuadWritesOnlyLogicalFaces()
        {
            const string input =
                "v 3.5 0.5 0.5\nv 4.5 0.5 0.5\nv 4.5 1.5 0.5\nv 3.5 1.5 0.5\n" +
                "usemtl RC_11_2_true_Standard\nf 1 2 3 4\nf 1 2 3 4\n";
            string firstPath = Path.GetTempFileName();
            string secondPath = Path.GetTempFileName();
            try
            {
                var exporter = new WavefrontExporter();
                Collision source = CollisionBuilder.BuildPerFace(Import(input));

                exporter.ExportCollision(firstPath, source);

                string firstExport = File.ReadAllText(firstPath);
                Assert.Equal(4, File.ReadAllLines(firstPath).Count(line => line.StartsWith("f ", StringComparison.Ordinal)));
                Collision imported = CollisionBuilder.BuildPerFace(Import(firstExport));
                exporter.ExportCollision(secondPath, imported);
                Assert.Equal(ReadObjWithoutMtlReference(firstPath), ReadObjWithoutMtlReference(secondPath));
                Assert.Equal(File.ReadAllText(Path.ChangeExtension(firstPath, ".mtl")), File.ReadAllText(Path.ChangeExtension(secondPath, ".mtl")));
            }
            finally
            {
                DeleteCollisionExportFiles(firstPath);
                DeleteCollisionExportFiles(secondPath);
            }
        }

        [Fact]
        public void ExportCollision_NonBinaryMultiCellTriangleWritesOneLogicalFaceAfterSerialization()
        {
            const string input =
                "v 0.1234567 0.5 0.5\nv 4.5 0.5 0.5\nv 4.5 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\n";
            string path = Path.GetTempFileName();
            string reparsedPath = Path.GetTempFileName();

            try
            {
                Collision collision = CollisionBuilder.BuildPerFace(Import(input));
                Collision reparsed = RoundTrip(collision);
                var exporter = new WavefrontExporter();
                exporter.ExportCollision(path, collision);
                exporter.ExportCollision(reparsedPath, reparsed);

                Assert.Equal(1, File.ReadAllLines(path).Count(line => line.StartsWith("f ", StringComparison.Ordinal)));
                Assert.Equal(1, File.ReadAllLines(reparsedPath).Count(line => line.StartsWith("f ", StringComparison.Ordinal)));
            }
            finally
            {
                DeleteCollisionExportFiles(path);
                DeleteCollisionExportFiles(reparsedPath);
            }
        }

        [Fact]
        public void ExportCollision_NearbyDistinctMultiCellTrianglesRemainDistinct()
        {
            const string input =
                "v 0.1234567 0.5 0.5\nv 4.5 0.5 0.5\nv 4.5 1.5 0.5\n" +
                "v 0.1334567 0.5 0.5\nv 4.51 0.5 0.5\nv 4.51 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\nf 4 5 6\n";
            string path = Path.GetTempFileName();

            try
            {
                Collision collision = CollisionBuilder.BuildPerFace(Import(input));
                new WavefrontExporter().ExportCollision(path, collision);

                Assert.Equal(2, File.ReadAllLines(path).Count(line => line.StartsWith("f ", StringComparison.Ordinal)));
            }
            finally
            {
                DeleteCollisionExportFiles(path);
            }
        }

        private static ParsedModelGeometry Import(string contents)
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, contents);
                return new WavefrontImporter().Import(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static Collision RoundTrip(Collision collision)
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, collision.Serialize());
                using var stream = File.OpenRead(path);
                return new Collision(stream, 0, import: true);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static (byte collisionType, CollisionGeometryCategory category)[] GetFaceMetadata(Collision collision)
        {
            return Enumerable.Range(0, collision.indBuff.Length / 3)
                .Select(face =>
                {
                    int vertex = checked((int) collision.indBuff[face * 3]);
                    float metadata = collision.vertexBuffer[vertex * collision.vertexStride + 3];
                    return (
                        CollisionVertexMetadata.GetCollisionType(metadata).data,
                        CollisionVertexMetadata.GetCategory(metadata));
                })
                .ToArray();
        }

        private static System.Collections.Generic.Dictionary<string, OpenTK.Mathematics.Vector3> ReadCollisionMaterials(string objPath)
        {
            var materials = new System.Collections.Generic.Dictionary<string, OpenTK.Mathematics.Vector3>(StringComparer.Ordinal);
            string? currentMaterial = null;
            foreach (string line in File.ReadLines(Path.ChangeExtension(objPath, ".mtl")))
            {
                string[] fields = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length == 2 && fields[0] == "newmtl")
                {
                    currentMaterial = fields[1];
                }
                else if (fields.Length == 4 && fields[0] == "Kd" && currentMaterial != null)
                {
                    materials.Add(currentMaterial, new OpenTK.Mathematics.Vector3(
                        float.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(fields[3], System.Globalization.CultureInfo.InvariantCulture)));
                }
            }

            return materials;
        }

        private static void AssertPositionOnlyVertices(string objContents)
        {
            string[] vertexLines = objContents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.StartsWith("v ", StringComparison.Ordinal))
                .ToArray();
            Assert.NotEmpty(vertexLines);
            Assert.All(vertexLines, line => Assert.Equal(4, line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length));
        }

        private static string[] ReadObjWithoutMtlReference(string objPath)
        {
            return File.ReadAllLines(objPath)
                .Where(line => !line.StartsWith("mtllib ", StringComparison.Ordinal))
                .ToArray();
        }

        private static void DeleteCollisionExportFiles(string objPath)
        {
            File.Delete(objPath);
            File.Delete(Path.ChangeExtension(objPath, ".mtl"));
        }
    }
}
