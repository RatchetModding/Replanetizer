using System;
using System.IO;
using System.Linq;
using LibReplanetizer.Models;
using LibReplanetizer.Serializers.Importers;
using OpenTK.Mathematics;
using Xunit;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.Models
{
    public class CollisionBuilderTests
    {
        [Fact]
        public void Build_EmptyGeometry_ReturnsEmptyCollision()
        {
            Collision collision = CollisionBuilder.Build(new ParsedModelGeometry());

            Assert.Empty(collision.vertexBuffer);
            Assert.Empty(collision.indBuff);
            Assert.Empty(collision.Serialize());
        }

        [Fact]
        public void Build_SerializesAndReparsesTriangleWithCollisionType()
        {
            Collision collision = CollisionBuilder.Build(CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.5f, 0.5f)), 0x0B);

            Assert.Equal(3, collision.vertexCount);
            Assert.Equal(3, collision.indBuff.Length);
            for (int i = 3; i < collision.vertexBuffer.Length; i += collision.vertexStride)
            {
                Assert.Equal(0x0B, CollisionVertexMetadata.GetCollisionType(collision.vertexBuffer[i]).data);
                Assert.Equal(CollisionGeometryCategory.Standard, CollisionVertexMetadata.GetCategory(collision.vertexBuffer[i]));
            }

            Collision reparsed = RoundTrip(collision);

            Assert.Equal(3, reparsed.vertexCount);
            Assert.Equal(3, reparsed.indBuff.Length);
            Assert.Equal(collision.vertexBuffer, reparsed.vertexBuffer);
            for (int i = 3; i < reparsed.vertexBuffer.Length; i += reparsed.vertexStride)
                Assert.Equal(0x0B, CollisionVertexMetadata.GetCollisionType(reparsed.vertexBuffer[i]).data);
        }

        [Fact]
        public void Build_TriangleAcrossCellBoundary_IsStoredInBothCells()
        {
            Collision collision = CollisionBuilder.Build(CreateGeometry(
                new Vector3(3.5f, 0.5f, 0.5f),
                new Vector3(4.5f, 0.5f, 0.5f),
                new Vector3(3.5f, 1.5f, 0.5f)));

            Assert.Equal(6, collision.indBuff.Length);
            Assert.Equal(6, collision.vertexCount);

            Collision reparsed = RoundTrip(collision);
            Assert.Equal(6, reparsed.indBuff.Length);
            Assert.Equal(6, reparsed.vertexCount);
        }

        [Fact]
        public void Build_DenseCellRendersOnlyReferencedFaceVertices()
        {
            var geometry = new ParsedModelGeometry();
            geometry.vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
            geometry.vertices.Add(new Vector3(1.5f, 0.5f, 0.5f));
            geometry.vertices.Add(new Vector3(1.5f, 1.5f, 0.5f));
            geometry.vertices.Add(new Vector3(0.5f, 1.5f, 0.5f));
            geometry.faces.Add(new Face
            {
                corners = new (int pos, int uv, int norm)[] { (0, -1, -1), (1, -1, -1), (2, -1, -1) }
            });
            geometry.faces.Add(new Face
            {
                corners = new (int pos, int uv, int norm)[] { (0, -1, -1), (2, -1, -1), (3, -1, -1) }
            });

            Collision collision = CollisionBuilder.Build(geometry);

            Assert.Equal(6, collision.indBuff.Length);
            Assert.Equal(6, collision.vertexCount);
            Assert.True(collision.vertexCount < geometry.faces.Count * geometry.vertices.Count);
            Assert.Equal(6, RoundTrip(collision).vertexCount);
        }

        [Fact]
        public void Build_CellVertexLimitStopsBeforeFirstOverflowingTriangle()
        {
            var geometry = new ParsedModelGeometry();

            void AddTriangle(Vector3 a, Vector3 b, Vector3 c, string materialName)
            {
                int firstPosition = geometry.vertices.Count;
                geometry.vertices.Add(a);
                geometry.vertices.Add(b);
                geometry.vertices.Add(c);
                var face = new Face
                {
                    corners = new (int pos, int uv, int norm)[]
                    {
                        (firstPosition, -1, -1),
                        (firstPosition + 1, -1, -1),
                        (firstPosition + 2, -1, -1)
                    }
                };
                SetMaterialName(geometry, face, materialName);
                geometry.faces.Add(face);
            }

            for (int triangle = 0; triangle < 85; triangle++)
            {
                Vector3[] positions = new Vector3[3];
                for (int corner = 0; corner < positions.Length; corner++)
                {
                    int vertex = triangle * 3 + corner;
                    positions[corner] = new Vector3(
                        0.1f + (vertex % 16) * 0.2f,
                        0.1f + (vertex / 16) * 0.2f,
                        0.5f);
                }
                AddTriangle(positions[0], positions[1], positions[2], "RC_1");
            }

            AddTriangle(geometry.vertices[0], geometry.vertices[1], geometry.vertices[2], "RC_2");
            AddTriangle(
                new Vector3(0.25f, 3.5f, 0.5f),
                new Vector3(0.5f, 3.5f, 0.5f),
                new Vector3(0.75f, 3.5f, 0.5f),
                "RC_3");
            AddTriangle(geometry.vertices[0], geometry.vertices[1], geometry.vertices[2], "RC_4");

            Collision collision = CollisionBuilder.BuildPerFace(geometry);

            Assert.Equal(86 * 3, collision.indBuff.Length);
            Assert.Equal((byte) 2, GetTriangleType(collision, 85));
            Assert.DoesNotContain((byte) 3, GetTriangleTypes(collision));
            Assert.DoesNotContain((byte) 4, GetTriangleTypes(collision));

            Collision reparsed = RoundTrip(collision);
            Assert.Equal(86 * 3, reparsed.indBuff.Length);
            Assert.Equal((byte) 2, GetTriangleType(reparsed, 85));
            Assert.DoesNotContain((byte) 3, GetTriangleTypes(reparsed));
            Assert.DoesNotContain((byte) 4, GetTriangleTypes(reparsed));
        }

        [Fact]
        public void CollisionCell_QuadRendersAndRoundTripsWithFourVertices()
        {
            Collision source = ParseCollision(CreateStandardCollisionWithQuad());

            Assert.Equal(6, source.indBuff.Length);
            Assert.Equal(4, source.vertexCount);
            Assert.Equal((byte) 0x0B, GetTriangleType(source, 0));
            Assert.Equal((byte) 0x0B, GetTriangleType(source, 1));

            Collision reparsed = RoundTrip(source);

            Assert.Equal(6, reparsed.indBuff.Length);
            Assert.Equal(4, reparsed.vertexCount);
            Assert.Equal((byte) 0x0B, GetTriangleType(reparsed, 0));
            Assert.Equal((byte) 0x0B, GetTriangleType(reparsed, 1));
        }

        [Fact]
        public void Build_NegativeGridCoordinate_Throws()
        {
            ParsedModelGeometry geometry = CreateGeometry(
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.5f, 0.5f));

            Assert.Throws<ArgumentOutOfRangeException>(() => CollisionBuilder.Build(geometry));
        }

        [Fact]
        public void Concatenate_MergesFacesAndKeepsCollisionTypes()
        {
            Collision first = CollisionBuilder.Build(CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.0f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.0f, 0.5f)), 0x03);
            Collision second = CollisionBuilder.Build(CreateGeometry(
                new Vector3(1.5f, 0.5f, 0.5f),
                new Vector3(2.0f, 0.5f, 0.5f),
                new Vector3(1.5f, 1.0f, 0.5f)), 0x0B);

            Collision merged = Collision.Concatenate(first, second);

            Assert.Equal(6, merged.indBuff.Length);
            Assert.Equal(6, merged.vertexCount);
            var types = new System.Collections.Generic.HashSet<byte>();
            foreach (uint index in merged.indBuff)
            {
                float metadata = merged.vertexBuffer[checked((int) index) * merged.vertexStride + 3];
                types.Add(CollisionVertexMetadata.GetCollisionType(metadata).data);
            }
            Assert.Contains((byte) 0x03, types);
            Assert.Contains((byte) 0x0B, types);

            Collision reparsed = RoundTrip(merged);
            Assert.Equal(merged.indBuff.Length, reparsed.indBuff.Length);
            Assert.Equal(6, reparsed.vertexCount);
        }

        [Fact]
        public void Concatenate_DoesNotDuplicateFacesAcrossGridCells()
        {
            Collision first = CollisionBuilder.Build(CreateGeometry(
                new Vector3(3.5f, 0.5f, 0.5f),
                new Vector3(4.5f, 0.5f, 0.5f),
                new Vector3(3.5f, 1.5f, 0.5f)));
            Collision empty = CollisionBuilder.Build(new ParsedModelGeometry());

            Collision merged = Collision.Concatenate(first, empty);

            Assert.Equal(first.indBuff.Length, merged.indBuff.Length);
            Assert.Equal(first.vertexCount, merged.vertexCount);
        }

        [Fact]
        public void Concatenate_PreservesFaceStoredOutsideMinimumCell()
        {
            Collision source = ParseCollision(CreateStandardCollisionWithFaceInNonMinimumCell());
            Collision empty = CollisionBuilder.Build(new ParsedModelGeometry());
            Assert.Equal(3, source.indBuff.Length);

            Collision merged = Collision.Concatenate(source, empty);

            Assert.Equal(3, merged.indBuff.Length);
            Assert.Equal((byte) 0x0B, GetTriangleType(merged, 0));
            Collision reparsed = RoundTrip(merged);
            Assert.Equal(3, reparsed.indBuff.Length);
            Assert.Equal((byte) 0x0B, GetTriangleType(reparsed, 0));
        }

        [Fact]
        public void Concatenate_PreservesRepeatedFacesWithinOneCell()
        {
            ParsedModelGeometry geometry = CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.5f, 0.5f));
            geometry.faces.Add(new Face
            {
                corners = new (int pos, int uv, int norm)[] { (0, -1, -1), (1, -1, -1), (2, -1, -1) }
            });
            Collision source = CollisionBuilder.Build(geometry, 0x0B);
            Collision empty = CollisionBuilder.Build(new ParsedModelGeometry());

            Collision merged = Collision.Concatenate(source, empty);

            Assert.Equal(6, merged.indBuff.Length);
            Assert.Equal((byte) 0x0B, GetTriangleType(merged, 0));
            Assert.Equal((byte) 0x0B, GetTriangleType(merged, 1));
        }

        [Fact]
        public void BuildPerFace_PreservesDifferentCollisionTypesAfterRoundTrip()
        {
            ParsedModelGeometry geometry = CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.0f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.0f, 0.5f));
            SetMaterialName(geometry, geometry.faces[0], "RC_3");
            geometry.faces.Add(new Face
            {
                corners = new (int pos, int uv, int norm)[] { (0, -1, -1), (1, -1, -1), (2, -1, -1) }
            });
            SetMaterialName(geometry, geometry.faces[1], "RC_11");

            Collision collision = CollisionBuilder.BuildPerFace(geometry);
            Assert.Equal((byte) 3, GetTriangleType(collision, 0));
            Assert.Equal((byte) 11, GetTriangleType(collision, 1));

            Collision reparsed = RoundTrip(collision);
            Assert.Equal((byte) 3, GetTriangleType(reparsed, 0));
            Assert.Equal((byte) 11, GetTriangleType(reparsed, 1));
        }

        [Fact]
        public void BuildPerFace_NoMaterialUsesDefaultCollisionType()
        {
            ParsedModelGeometry geometry = CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.0f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.0f, 0.5f));

            Assert.Equal((byte) 0x1F, GetTriangleType(CollisionBuilder.BuildPerFace(geometry), 0));
        }

        [Theory]
        [InlineData("mtl_42")]
        [InlineData("no_digits")]
        [InlineData("rc_3")]
        public void BuildPerFace_NonRcMaterialUsesDefaultCollisionType(string materialName)
        {
            ParsedModelGeometry geometry = CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.0f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.0f, 0.5f));
            SetMaterialName(geometry, geometry.faces[0], materialName);

            Assert.Equal((byte) 0x1F, GetTriangleType(CollisionBuilder.BuildPerFace(geometry), 0));
        }

        [Fact]
        public void BuildPerFace_SkipsTrianglesWithNegativeCoordinatesOnly()
        {
            const string obj =
                "v 0 0 0\nv 1 0 0\nv 0 1 0\n" +
                "v -1 0.5 0.5\nv 0.5 0.5 0.5\nv 0.5 1 0.5\n" +
                "v 0.5 -1 0.5\nv 0.5 0.5 0.5\nv 1 0.5 0.5\n" +
                "v 0.5 0.5 -1\nv 0.5 0.5 0.5\nv 1 0.5 0.5\n" +
                "usemtl RC_5\nf 1 2 3\n" +
                "usemtl RC_11\nf 4 5 6\nf 7 8 9\nf 10 11 12\n";

            Assert.Throws<InvalidOperationException>(() => CollisionBuilder.BuildPerFace(ImportObj(obj)));
        }

        [Fact]
        public void BuildPerFace_AllTrianglesWithNegativeCoordinates_RejectsImport()
        {
            ParsedModelGeometry geometry = ImportObj(
                "v -1 0 0\nv 0 1 0\nv 0 0 1\nf 1 2 3\n");

            Assert.Throws<InvalidOperationException>(() => CollisionBuilder.BuildPerFace(geometry));
        }

        [Fact]
        public void BuildPerFace_QuadWithOneNegativeCornerRejectsBothTriangles()
        {
            ParsedModelGeometry geometry = ImportObj(
                "v 1 1 1\nv -1 1 1\nv 1 3 1\nv 2 2 1\nf 1 2 3 4\n");

            Assert.Equal(2, geometry.faces.Count);
            Assert.Throws<InvalidOperationException>(() => CollisionBuilder.BuildPerFace(geometry));
        }

        [Fact]
        public void BuildPerFace_MultiCellQuadKeepsBothTrianglesAfterConcatenate()
        {
            ParsedModelGeometry geometry = ImportObj(
                "v 0.5 0.5 0.5\nv 4.5 0.5 0.5\nv 4.5 4.5 0.5\nv 0.5 4.5 0.5\nf 1 2 3 4\n");
            Collision imported = CollisionBuilder.BuildPerFace(geometry);
            Collision empty = CollisionBuilder.Build(new ParsedModelGeometry());

            Assert.Equal(24, imported.indBuff.Length);
            Collision merged = Collision.Concatenate(empty, imported);

            Assert.Equal(imported.indBuff.Length, merged.indBuff.Length);
        }

        [Theory]
        [InlineData("RC_0", 0)]
        [InlineData("RC_31", 31)]
        [InlineData("RC_3_2", 67)]
        [InlineData("RC_31_3_true", 255)]
        [InlineData("RC_5_1_false", 37)]
        [InlineData("RC_7_2_1", 199)]
        public void BuildPerFace_PacksStructuredMaterialFields(string materialName, int expected)
        {
            ParsedModelGeometry geometry = CreateGeometry(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.0f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.0f, 0.5f));
            SetMaterialName(geometry, geometry.faces[0], materialName);

            Assert.Equal((byte) expected, GetTriangleType(CollisionBuilder.BuildPerFace(geometry), 0));
        }

        [Fact]
        public void BuildPerFace_ImportsObjMaterialTypesAndRoundTrips()
        {
            const string obj =
                "v 0.5 0.5 0.5\nv 1.5 0.5 0.5\nv 0.5 1.5 0.5\n" +
                "v 2 0.5 0.5\nv 2.5 0.5 0.5\nv 2 1 0.5\n" +
                "v 3 0.5 0.5\nv 3.5 0.5 0.5\nv 3 1 0.5\n" +
                "f 1 2 3\n" +
                "usemtl RC_11_2_true\nf 4 5 6\n" +
                "usemtl RC_3_1_false\nf 7 8 9\n";
            ParsedModelGeometry geometry = ImportObj(obj);

            Collision collision = CollisionBuilder.BuildPerFace(geometry);
            Assert.Equal(new byte[] { 0x1F, 0x23, 0xCB }.OrderBy(type => type), GetTriangleTypes(collision).OrderBy(type => type));

            Collision reparsed = RoundTrip(collision);
            Assert.Equal(new byte[] { 0x1F, 0x23, 0xCB }.OrderBy(type => type), GetTriangleTypes(reparsed).OrderBy(type => type));
        }

        [Fact]
        public void BuildPerFace_RejectsNegativeBoundsFromUnusedVertex()
        {
            ParsedModelGeometry geometry = ImportObj(
                "v 0.5 0.5 0.5\nv 1 0.5 0.5\nv 0.5 1 0.5\nv -1 2 3\nf 1 2 3\n");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => CollisionBuilder.BuildPerFace(geometry));

            Assert.Contains("bounds", exception.Message);
        }

        [Fact]
        public void Concatenate_PreservesHeroAndUnknownSections()
        {
            Collision hero = ParseCollision(CreateSpecialCollisionBytes(hero: true));
            Collision unknown = ParseCollision(CreateSpecialCollisionBytes(hero: false));

            Collision merged = Collision.Concatenate(hero, unknown);

            Assert.Equal(6, merged.indBuff.Length);
            Assert.Contains(CollisionGeometryCategory.Hero, GetCategories(merged));
            Assert.Contains(CollisionGeometryCategory.Unknown, GetCategories(merged));

            Collision reparsed = RoundTrip(merged);
            Assert.Equal(6, reparsed.indBuff.Length);
            Assert.Contains(CollisionGeometryCategory.Hero, GetCategories(reparsed));
            Assert.Contains(CollisionGeometryCategory.Unknown, GetCategories(reparsed));
        }

        private static ParsedModelGeometry CreateGeometry(Vector3 a, Vector3 b, Vector3 c)
        {
            var geometry = new ParsedModelGeometry();
            geometry.vertices.Add(a);
            geometry.vertices.Add(b);
            geometry.vertices.Add(c);
            geometry.faces.Add(new Face
            {
                corners = new (int pos, int uv, int norm)[] { (0, -1, -1), (1, -1, -1), (2, -1, -1) }
            });
            return geometry;
        }

        private static ParsedModelGeometry ImportObj(string contents)
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

        private static void SetMaterialName(ParsedModelGeometry geometry, Face face, string materialName)
        {
            int materialIndex = geometry.materialNames.IndexOf(materialName);
            if (materialIndex == -1)
            {
                materialIndex = geometry.materialNames.Count;
                geometry.materialNames.Add(materialName);
            }
            face.materialIndex = materialIndex;
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

        private static Collision ParseCollision(byte[] bytes)
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, bytes);
                using var stream = File.OpenRead(path);
                return new Collision(stream, 0, import: true);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static byte[] CreateSpecialCollisionBytes(bool hero)
        {
            const int sectionOffset = 0x10;
            const int dataOffset = 0x20;
            var section = new byte[0x10 + 0x10 + 0x1C];
            WriteInt(section, 0x00, 1);
            WriteUshort(section, 0x10 + 0x08, 1);
            WriteUshort(section, 0x10 + 0x0A, 3);
            WriteInt(section, 0x10 + 0x0C, dataOffset);

            WriteUshort(section, dataOffset + 0x00, 32);
            WriteUshort(section, dataOffset + 0x02, 32);
            WriteUshort(section, dataOffset + 0x04, 32);
            WriteUshort(section, dataOffset + 0x08, 96);
            WriteUshort(section, dataOffset + 0x0A, 32);
            WriteUshort(section, dataOffset + 0x0C, 32);
            WriteUshort(section, dataOffset + 0x10, 32);
            WriteUshort(section, dataOffset + 0x12, 96);
            WriteUshort(section, dataOffset + 0x14, 32);
            section[dataOffset + 0x18] = 0;
            section[dataOffset + 0x19] = 1;
            section[dataOffset + 0x1A] = 2;

            var bytes = new byte[sectionOffset + section.Length];
            WriteInt(bytes, hero ? 0x04 : 0x08, sectionOffset);
            Buffer.BlockCopy(section, 0, bytes, sectionOffset, section.Length);
            return bytes;
        }

        private static byte[] CreateStandardCollisionWithFaceInNonMinimumCell()
        {
            const int standardOffset = 0x10;
            const int cellOffset = 0x1C;
            var bytes = new byte[standardOffset + cellOffset + 0x2C];

            WriteInt(bytes, 0x00, standardOffset);
            WriteUshort(bytes, standardOffset + 0x00, 0);
            WriteUshort(bytes, standardOffset + 0x02, 1);
            WriteInt(bytes, standardOffset + 0x04, 8);
            WriteUshort(bytes, standardOffset + 0x08, 0);
            WriteUshort(bytes, standardOffset + 0x0A, 1);
            WriteInt(bytes, standardOffset + 0x0C, 16);
            WriteUshort(bytes, standardOffset + 0x10, 0);
            WriteUshort(bytes, standardOffset + 0x12, 2);
            WriteInt(bytes, standardOffset + 0x14, 0);
            WriteInt(bytes, standardOffset + 0x18, cellOffset);

            int cell = standardOffset + cellOffset;
            WriteUshort(bytes, cell + 0x00, 1);
            bytes[cell + 0x02] = 3;
            bytes[cell + 0x03] = 0;

            Vector3[] positions =
            {
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 1.5f, 0.5f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                WriteFloat(bytes, cell + 0x04 + i * 0x0C + 0x00, (positions[i].X - 6.0f) * 1024.0f);
                WriteFloat(bytes, cell + 0x04 + i * 0x0C + 0x04, (positions[i].Y - 2.0f) * 1024.0f);
                WriteFloat(bytes, cell + 0x04 + i * 0x0C + 0x08, (positions[i].Z - 2.0f) * 1024.0f);
            }

            bytes[cell + 0x04 + 3 * 0x0C + 0x00] = 0;
            bytes[cell + 0x04 + 3 * 0x0C + 0x01] = 1;
            bytes[cell + 0x04 + 3 * 0x0C + 0x02] = 2;
            bytes[cell + 0x04 + 3 * 0x0C + 0x03] = 0x0B;
            return bytes;
        }

        private static byte[] CreateStandardCollisionWithQuad()
        {
            const int standardOffset = 0x10;
            const int cellOffset = 0x18;
            const int vertexCount = 4;
            const int faceCount = 1;
            const int quadCount = 1;
            int cellSize = 0x04 + vertexCount * 0x0C + faceCount * 0x04 + quadCount;
            var bytes = new byte[standardOffset + cellOffset + cellSize];

            WriteInt(bytes, 0x00, standardOffset);
            WriteUshort(bytes, standardOffset + 0x00, 0);
            WriteUshort(bytes, standardOffset + 0x02, 1);
            WriteInt(bytes, standardOffset + 0x04, 8);
            WriteUshort(bytes, standardOffset + 0x08, 0);
            WriteUshort(bytes, standardOffset + 0x0A, 1);
            WriteInt(bytes, standardOffset + 0x0C, 16);
            WriteUshort(bytes, standardOffset + 0x10, 0);
            WriteUshort(bytes, standardOffset + 0x12, 1);
            WriteInt(bytes, standardOffset + 0x14, cellOffset);

            int cell = standardOffset + cellOffset;
            WriteUshort(bytes, cell + 0x00, faceCount);
            bytes[cell + 0x02] = vertexCount;
            bytes[cell + 0x03] = quadCount;

            Vector3[] positions =
            {
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.5f, 0.5f, 0.5f),
                new Vector3(1.5f, 1.5f, 0.5f),
                new Vector3(0.5f, 1.5f, 0.5f)
            };
            for (int vertex = 0; vertex < positions.Length; vertex++)
            {
                WriteFloat(bytes, cell + 0x04 + vertex * 0x0C + 0x00, (positions[vertex].X - 2.0f) * 1024.0f);
                WriteFloat(bytes, cell + 0x04 + vertex * 0x0C + 0x04, (positions[vertex].Y - 2.0f) * 1024.0f);
                WriteFloat(bytes, cell + 0x04 + vertex * 0x0C + 0x08, (positions[vertex].Z - 2.0f) * 1024.0f);
            }

            int faceEntry = cell + 0x04 + vertexCount * 0x0C;
            bytes[faceEntry + 0x00] = 0;
            bytes[faceEntry + 0x01] = 1;
            bytes[faceEntry + 0x02] = 2;
            bytes[faceEntry + 0x03] = 0x0B;
            bytes[faceEntry + faceCount * 0x04] = 3;
            return bytes;
        }

        private static System.Collections.Generic.HashSet<CollisionGeometryCategory> GetCategories(Collision collision)
        {
            var categories = new System.Collections.Generic.HashSet<CollisionGeometryCategory>();
            for (int i = 3; i < collision.vertexBuffer.Length; i += collision.vertexStride)
                categories.Add(CollisionVertexMetadata.GetCategory(collision.vertexBuffer[i]));
            return categories;
        }

        private static byte GetTriangleType(Collision collision, int triangleIndex)
        {
            var types = new System.Collections.Generic.HashSet<byte>();
            int start = triangleIndex * 3;
            for (int i = start; i < start + 3; i++)
            {
                uint vertexIndex = collision.indBuff[i];
                float metadata = collision.vertexBuffer[checked((int) vertexIndex) * collision.vertexStride + 3];
                types.Add(CollisionVertexMetadata.GetCollisionType(metadata).data);
            }
            return Assert.Single(types);
        }

        private static System.Collections.Generic.HashSet<byte> GetTriangleTypes(Collision collision)
        {
            var types = new System.Collections.Generic.HashSet<byte>();
            for (int i = 0; i < collision.indBuff.Length / 3; i++)
                types.Add(GetTriangleType(collision, i));
            return types;
        }
    }
}
