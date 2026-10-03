using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LibReplanetizer.Models;
using Xunit;

namespace LibReplanetizer.Tests.Exporters
{
    public class GLTFExporterTests
    {
        [Fact]
        public void ExportModel_SparseBangleSlots_PreservesRootMesh()
        {
            MobyModel model = CreateTriangleModel();
            model.textureConfig.Add(new TextureConfig { id = 7, start = 0, size = 1 });
            model.bangles.Add(null);
            model.bangles.Add(null);

            using JsonDocument document = JsonDocument.Parse(Export(model, new ExporterModelSettings()));
            JsonElement meshes = document.RootElement.GetProperty("meshes");

            Assert.Equal(1, meshes.GetArrayLength());
            Assert.Single(meshes[0].GetProperty("primitives").EnumerateArray());
        }

        [Fact]
        public async Task ExportModel_AllZeroSkinWeights_ThrowsPromptlyWithModelAndVertex()
        {
            MobyModel model = new MobyModel
            {
                id = 42,
                boneCount = 1,
                vertexBuffer = new float[8],
                vertexBoneWeights = new uint[] { 0 },
                vertexBoneIds = new uint[] { 0 }
            };

            Task exportTask = Task.Run(() => Export(model, new ExporterModelSettings()));
            InvalidDataException exception = await Assert.ThrowsAsync<InvalidDataException>(
                async () => await exportTask.WaitAsync(TimeSpan.FromSeconds(5)));

            Assert.Contains("Model 42", exception.Message);
            Assert.Contains("vertex 0", exception.Message);
        }

        [Fact]
        public void ExportModel_MissingEmbeddedImage_KeepsPrimitiveWithTexturelessMaterial()
        {
            MobyModel model = CreateTriangleModel();
            model.textureConfig.Add(new TextureConfig { id = 99, start = 0, size = 1 });
            ExporterModelSettings settings = new ExporterModelSettings { embedTextures = true };

            using JsonDocument document = JsonDocument.Parse(Export(model, settings, new System.Collections.Generic.List<Texture>()));
            JsonElement root = document.RootElement;
            JsonElement primitive = root.GetProperty("meshes")[0].GetProperty("primitives")[0];
            JsonElement pbr = root.GetProperty("materials")[0].GetProperty("pbrMetallicRoughness");

            Assert.True(primitive.TryGetProperty("indices", out _));
            Assert.Equal(0, primitive.GetProperty("material").GetInt32());
            Assert.True(pbr.TryGetProperty("baseColorFactor", out _));
            Assert.False(pbr.TryGetProperty("baseColorTexture", out _));
            Assert.Equal(0, root.GetProperty("textures").GetArrayLength());
            Assert.Equal(0, root.GetProperty("images").GetArrayLength());
        }

        private static MobyModel CreateTriangleModel()
        {
            return new MobyModel
            {
                vertexBuffer = new float[]
                {
                    0, 0, 0, 0, 0, 1, 0, 0,
                    1, 0, 0, 0, 0, 1, 1, 0,
                    0, 1, 0, 0, 0, 1, 0, 1
                },
                indexBuffer = new ushort[] { 0, 1, 2 }
            };
        }

        private static string Export(MobyModel model, ExporterModelSettings settings, System.Collections.Generic.List<Texture>? textures = null)
        {
            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".gltf");
            try
            {
                new GLTFExporter(settings).ExportModel(fileName, null!, model, textures);
                return File.ReadAllText(fileName);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }
    }
}
