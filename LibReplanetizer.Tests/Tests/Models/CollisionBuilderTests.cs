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
            Assert.Equal(12, merged.vertexCount);
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
            Assert.Equal(12, reparsed.vertexCount);
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

            Collision collision = CollisionBuilder.BuildPerFace(ImportObj(obj));

            Assert.Equal(3, collision.vertexCount);
            Assert.Equal(3, collision.indBuff.Length);
            Assert.Equal((byte) 5, GetTriangleType(collision, 0));
        }

        [Fact]
        public void BuildPerFace_AllTrianglesWithNegativeCoordinates_ReturnsEmptyCollision()
        {
            ParsedModelGeometry geometry = ImportObj(
                "v -1 0 0\nv 0 1 0\nv 0 0 1\nf 1 2 3\n");

            Collision collision = CollisionBuilder.BuildPerFace(geometry);

            Assert.Empty(collision.vertexBuffer);
            Assert.Empty(collision.indBuff);
            Assert.Empty(collision.Serialize());
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
