using System.Collections.Generic;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Serializers.Importers;
using OpenTK.Mathematics;
using Xunit;

namespace LibReplanetizer.Tests.Importers
{
    public class TerrainBuilderTests
    {
        [Fact]
        public void Build_UnmaterialedTriangleProducesExpectedBuffersAndDefaultConfig()
        {
            TerrainModel model = BuildModel(CreateTriangleGeometry());

            Assert.Equal(24, model.vertexBuffer.Length);
            Assert.Equal(3, model.indexBuffer.Length);
            Assert.Equal(12, model.rgbas.Length);
            Assert.Equal(1.0f, model.vertexBuffer[7]);
            Assert.Equal(128, model.rgbas[3]);
            Assert.All(model.indexBuffer, index => Assert.True(index < model.vertexBuffer.Length / 8));

            TextureConfig config = Assert.Single(model.textureConfig);
            Assert.Equal(0, config.id);
            Assert.Equal(0, config.start);
            Assert.Equal(3, config.size);
            Assert.Equal(TextureConfig.WrapMode.Repeat, config.wrapModeS);
            Assert.Equal(TextureConfig.WrapMode.Repeat, config.wrapModeT);
        }

        [Fact]
        public void Build_MissingUvAndNormalUsesZeroUvAndGeneratedFaceNormal()
        {
            TerrainModel model = BuildModel(CreateGeometryWithoutAttributes());

            Assert.Equal(3, model.vertexBuffer.Length / 8);
            for (int vertex = 0; vertex < 3; vertex++)
            {
                int offset = vertex * 8;
                Assert.Equal(0.0f, model.vertexBuffer[offset + 3]);
                Assert.Equal(0.0f, model.vertexBuffer[offset + 4]);
                Assert.Equal(1.0f, model.vertexBuffer[offset + 5]);
                Assert.Equal(0.0f, model.vertexBuffer[offset + 6]);
                Assert.Equal(1.0f, model.vertexBuffer[offset + 7]);
            }
        }

        [Fact]
        public void Build_GeneratedNormalsKeepSharedVerticesSeparateAcrossFaces()
        {
            var geometry = CreateGeometryWithoutAttributes();
            geometry.vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));
            geometry.colors.Add(new Vector3(0.5f, 0.5f, 0.5f));
            geometry.faces.Add(CreateFace((0, -1, -1), (1, -1, -1), (3, -1, -1)));

            TerrainModel model = BuildModel(geometry);

            Assert.Equal(6, model.vertexBuffer.Length / 8);
            for (int vertex = 0; vertex < 3; vertex++)
            {
                Assert.Equal(0.0f, model.vertexBuffer[vertex * 8 + 3]);
                Assert.Equal(0.0f, model.vertexBuffer[vertex * 8 + 4]);
                Assert.Equal(1.0f, model.vertexBuffer[vertex * 8 + 5]);
                Assert.Equal(0.0f, model.vertexBuffer[(vertex + 3) * 8 + 3]);
                Assert.Equal(-1.0f, model.vertexBuffer[(vertex + 3) * 8 + 4]);
                Assert.Equal(0.0f, model.vertexBuffer[(vertex + 3) * 8 + 5]);
            }
        }

        [Fact]
        public void Build_PreservesUvAndNormalSeamsWhenDeduplicatingVertices()
        {
            ParsedModelGeometry geometry = CreateTriangleGeometry();
            geometry.vertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
            geometry.colors.Add(new Vector3(0.5f, 0.5f, 0.5f));
            geometry.uvs.Add(new Vector2(0.5f, 0.5f));
            geometry.normals.Add(new Vector3(0.0f, 0.0f, -1.0f));
            geometry.faces.Add(CreateFace((0, 3, 1), (1, 1, 0), (3, 3, 0)));

            TerrainModel model = BuildModel(geometry);

            Assert.Equal(5, model.vertexBuffer.Length / 8);
            Assert.Equal(6, model.indexBuffer.Length);
            Assert.NotEqual(model.indexBuffer[0], model.indexBuffer[3]);
        }

        [Fact]
        public void Build_SortsMaterialConfigsIntoContiguousIndexRanges()
        {
            ParsedModelGeometry geometry = CreateTriangleGeometry();
            geometry.faces.Add(CreateFace((0, 0, 0), (1, 1, 0), (2, 2, 0)));
            geometry.faces.Add(CreateFace((0, 0, 0), (1, 1, 0), (2, 2, 0)));
            SetMaterialName(geometry, geometry.faces[1], "RC_42");
            SetMaterialName(geometry, geometry.faces[2], "RC_17");

            TerrainModel model = BuildModel(geometry);

            Assert.Equal(new[] { 0, 17, 42 }, model.textureConfig.ConvertAll(config => config.id));
            int nextIndex = 0;
            foreach (TextureConfig config in model.textureConfig)
            {
                Assert.Equal(nextIndex, config.start);
                Assert.Equal(3, config.size);
                nextIndex += config.size;
            }
            Assert.Equal(model.indexBuffer.Length, nextIndex);
        }

        [Fact]
        public void Build_AssignsFacesToCellsByCentroid()
        {
            ParsedModelGeometry geometry = CreateTriangleGeometry();
            AddTriangle(geometry,
                new Vector3(32.0f, 0.0f, 0.0f),
                new Vector3(33.0f, 0.0f, 0.0f),
                new Vector3(32.0f, 1.0f, 0.0f));

            Terrain terrain = TerrainBuilder.Build(geometry, 0);

            Assert.Equal(2, terrain.fragments.Count);
        }

        [Fact]
        public void Build_SplitsCellAfter83Faces()
        {
            ParsedModelGeometry geometry = CreateTriangleGeometry();
            for (int i = 1; i < 84; i++)
                geometry.faces.Add(CreateFace((0, 0, 0), (1, 1, 0), (2, 2, 0)));

            Terrain terrain = TerrainBuilder.Build(geometry, 0);

            Assert.Equal(2, terrain.fragments.Count);
            TerrainModel first = Assert.IsType<TerrainModel>(terrain.fragments[0].model);
            TerrainModel second = Assert.IsType<TerrainModel>(terrain.fragments[1].model);
            Assert.Equal(83 * 3, first.indexBuffer.Length);
            Assert.Equal(3, second.indexBuffer.Length);
        }

        private static TerrainModel BuildModel(ParsedModelGeometry geometry)
        {
            Terrain terrain = TerrainBuilder.Build(geometry, 0);
            return Assert.IsType<TerrainModel>(Assert.Single(terrain.fragments).model);
        }

        private static ParsedModelGeometry CreateTriangleGeometry()
        {
            var geometry = new ParsedModelGeometry();
            geometry.vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            geometry.vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            geometry.vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            geometry.colors.AddRange(new[]
            {
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f)
            });
            geometry.normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
            geometry.uvs.Add(new Vector2(0.0f, 0.0f));
            geometry.uvs.Add(new Vector2(1.0f, 0.0f));
            geometry.uvs.Add(new Vector2(0.0f, 1.0f));
            geometry.faces.Add(CreateFace((0, 0, 0), (1, 1, 0), (2, 2, 0)));
            return geometry;
        }

        private static ParsedModelGeometry CreateGeometryWithoutAttributes()
        {
            var geometry = new ParsedModelGeometry();
            geometry.vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            geometry.vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            geometry.vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            geometry.colors.AddRange(new[]
            {
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f)
            });
            geometry.faces.Add(CreateFace((0, -1, -1), (1, -1, -1), (2, -1, -1)));
            return geometry;
        }

        private static void AddTriangle(ParsedModelGeometry geometry, Vector3 a, Vector3 b, Vector3 c)
        {
            int firstVertex = geometry.vertices.Count;
            int firstUv = geometry.uvs.Count;
            geometry.vertices.AddRange(new[] { a, b, c });
            geometry.colors.AddRange(new[]
            {
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f)
            });
            geometry.uvs.AddRange(new[] { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f) });
            geometry.faces.Add(CreateFace(
                (firstVertex, firstUv, 0),
                (firstVertex + 1, firstUv + 1, 0),
                (firstVertex + 2, firstUv + 2, 0)));
        }

        private static Face CreateFace((int pos, int uv, int norm) a, (int pos, int uv, int norm) b, (int pos, int uv, int norm) c)
        {
            return new Face { corners = new[] { a, b, c } };
        }

        private static void SetMaterialName(ParsedModelGeometry geometry, Face face, string materialName)
        {
            int index = geometry.materialNames.IndexOf(materialName);
            if (index < 0)
            {
                index = geometry.materialNames.Count;
                geometry.materialNames.Add(materialName);
            }
            face.materialIndex = index;
        }
    }
}
