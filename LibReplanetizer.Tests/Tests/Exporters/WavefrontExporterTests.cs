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
                Assert.DoesNotContain("mtllib", firstExport);

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
                Assert.Equal(firstExport, File.ReadAllText(secondPath));
            }
            finally
            {
                File.Delete(firstPath);
                File.Delete(secondPath);
            }
        }

        [Fact]
        public void ExportCollision_ConcatenatePreservesOppositeWindingCoincidentFaces()
        {
            const string input =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\nf 1 3 2\n";
            string path = Path.GetTempFileName();

            try
            {
                Collision source = CollisionBuilder.BuildPerFace(Import(input));
                Collision empty = CollisionBuilder.Build(new ParsedModelGeometry());
                Collision merged = Collision.Concatenate(source, empty);

                new WavefrontExporter().ExportCollision(path, merged);
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
                File.Delete(path);
            }
        }

        [Fact]
        public void ExportCollision_ConcatenateDeduplicatesCyclicRotationsAndPreservesMultiplicity()
        {
            const string input =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "usemtl RC_11_0_false_Standard\nf 1 2 3\nf 2 3 1\n";
            string path = Path.GetTempFileName();

            try
            {
                Collision source = CollisionBuilder.BuildPerFace(Import(input));
                Collision empty = CollisionBuilder.Build(new ParsedModelGeometry());
                Collision merged = Collision.Concatenate(source, empty);

                new WavefrontExporter().ExportCollision(path, merged);
                ParsedModelGeometry exported = Import(File.ReadAllText(path));

                Assert.Equal(2, exported.faces.Count);
                var firstPositions = exported.faces[0].corners.Select(corner => exported.vertices[corner.pos]).ToArray();
                var secondPositions = exported.faces[1].corners.Select(corner => exported.vertices[corner.pos]).ToArray();
                Assert.Equal(firstPositions, secondPositions);
            }
            finally
            {
                File.Delete(path);
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
                File.Delete(path);
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
                Assert.Equal(firstExport, File.ReadAllText(secondPath));
            }
            finally
            {
                File.Delete(firstPath);
                File.Delete(secondPath);
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
    }
}
