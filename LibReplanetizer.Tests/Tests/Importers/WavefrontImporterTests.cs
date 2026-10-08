using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Serializers.Importers;
using OpenTK.Mathematics;
using Xunit;

namespace LibReplanetizer.Tests.Importers
{
    public class WavefrontImporterTests
    {
        [Fact]
        public void Import_StoresInternedMaterialNamesAndFaceIndices()
        {
            ParsedModelGeometry geometry = ImportText(
                "v 0 0 0\nv 1 0 0\nv 0 1 0\nf 1 2 3\n" +
                "usemtl mtl_42\nf 1 2 3\n" +
                "usemtl RC_11\nf 1 2 3\n" +
                "usemtl mtl_42\nf 1 2 3\n" +
                "usemtl no_digits\nf 1 2 3\n");

            Assert.Equal(new[] { "mtl_42", "RC_11", "no_digits" }, geometry.materialNames);
            Assert.Equal(new[] { -1, 0, 1, 0, 2 }, geometry.faces.ConvertAll(face => face.materialIndex));
            Assert.Equal(-1, geometry.faces[0].corners[0].uv);
            Assert.Equal(-1, geometry.faces[0].corners[0].norm);
        }

        [Fact]
        public void Import_TriangleWithInlineCommentsParsesOnlyObjTokens()
        {
            ParsedModelGeometry geometry = ImportText(
                "v 0 0 0 # origin\nv 1 0 0 # x\nv 0 1 0 # y\nf 1 2 3 # triangle\n");

            Assert.Single(geometry.faces);
            Assert.Equal(3, geometry.faces[0].corners.Length);
        }

        [Fact]
        public void Import_ParsesNumericAttributesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                ParsedModelGeometry geometry = ImportText(
                    "v 1.5 2.5 3.5 0.25 0.5 0.75\nv 2.5 2.5 3.5\nv 1.5 3.5 3.5\n" +
                    "vt 0.25 0.75\nvn 0.5 0.5 1\nf 1/1/1 2/1/1 3/1/1\n");

                Assert.Equal(1.5f, geometry.vertices[0].X);
                Assert.Equal(0.25f, geometry.colors[0].X);
                Assert.Equal(0.25f, geometry.uvs[0].X);
                Assert.Equal(0.5f, geometry.normals[0].X);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void ParseTextureId_UsesExportedMtlIdConvention()
        {
            Assert.Equal(42, TerrainBuilder.ParseTextureId("mtl_42"));
        }

        [Theory]
        [InlineData("RC_42", 42)]
        [InlineData("RC_42_Repeat_ClampEdge", 42)]
        [InlineData("RC_42_0_1", 42)]
        public void ParseTextureId_ParsesStructuredTerrainName(string materialName, int expected)
        {
            Assert.Equal(expected, TerrainBuilder.ParseTextureId(materialName));
        }

        [Fact]
        public void Import_TerrainMaterialMappingReachesTextureConfiguration()
        {
            const string obj =
                "v 0 0 0\nv 1 0 0\nv 0 1 0\n" +
                "vt 0 0\nvt 1 0\nvt 0 1\nvn 0 0 1\n" +
                "f 1/1/1 2/2/1 3/3/1\n" +
                "usemtl mtl_42\nf 1/1/1 2/2/1 3/3/1\n" +
                "usemtl mtl_17\nf 1/1/1 2/2/1 3/3/1\n";
            ParsedModelGeometry geometry = ImportText(obj);

            Terrain terrain = TerrainBuilder.Build(geometry, 0);

            TerrainModel model = Assert.IsType<TerrainModel>(terrain.fragments[0].model);
            Assert.Equal(new[] { 0, 17, 42 }, model.textureConfig.Select(config => config.id));
            Assert.All(model.textureConfig, config =>
            {
                Assert.Equal(TextureConfig.WrapMode.Repeat, config.wrapModeS);
                Assert.Equal(TextureConfig.WrapMode.Repeat, config.wrapModeT);
            });
        }

        [Fact]
        public void Import_TerrainMaterialWrapModesCreateSeparateConfigs()
        {
            const string obj =
                "v 0 0 0\nv 1 0 0\nv 0 1 0\n" +
                "vt 0 0\nvt 1 0\nvt 0 1\nvn 0 0 1\n" +
                "usemtl RC_42_Repeat_ClampEdge\nf 1/1/1 2/2/1 3/3/1\n" +
                "usemtl RC_42_ClampEdge_Repeat\nf 1/1/1 2/2/1 3/3/1\n";
            ParsedModelGeometry geometry = ImportText(obj);

            Terrain terrain = TerrainBuilder.Build(geometry, 0);

            TerrainModel model = Assert.IsType<TerrainModel>(terrain.fragments[0].model);
            Assert.Equal(2, model.textureConfig.Count);
            Assert.All(model.textureConfig, config => Assert.Equal(42, config.id));
            Assert.Contains(model.textureConfig, config =>
                config.wrapModeS == TextureConfig.WrapMode.Repeat && config.wrapModeT == TextureConfig.WrapMode.ClampEdge);
            Assert.Contains(model.textureConfig, config =>
                config.wrapModeS == TextureConfig.WrapMode.ClampEdge && config.wrapModeT == TextureConfig.WrapMode.Repeat);
        }

        private static ParsedModelGeometry ImportText(string contents)
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, contents);
                var importer = new WavefrontImporter();
                return importer.Import(path);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
