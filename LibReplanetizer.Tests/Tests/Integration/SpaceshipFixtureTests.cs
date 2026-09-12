using System.Linq;
using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class SpaceshipFixtureTests
    {
        private static readonly string? EngineFile = FixtureConfig.GetFixtureFile("engine.ps3");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a RaC2 fixture directory containing engine.ps3.";

        [SkippableFact]
        public void RaC2Spaceships_LoadBodiesAndTextures()
        {
            Skip.If(EngineFile == null, SkipMsg);

            var (models, textures) = SpaceshipParser.GetAllSpaceshipData(GameType.RaC2, EngineFile!);

            Assert.Equal(new short[] { 0x0E0D, 0x0E29, 0x1210 }, models.Select(model => model.id).ToArray());
            Assert.Equal(18, textures.Count);
            for (int i = 0; i < models.Count; i++)
            {
                MobyModel model = models[i];
                Assert.True(model.isModel);
                Assert.NotEmpty(model.vertexBuffer);
                Assert.NotEmpty(model.indexBuffer);
                Assert.NotEmpty(model.textureConfig);
                Assert.NotEmpty(model.metalTextureConfig);
                Assert.All(model.textureConfig, config => Assert.Equal(i * 6, config.id));
                Assert.All(model.metalTextureConfig, config => Assert.Equal(-2, config.id));
            }
            Assert.All(textures, texture => Assert.NotEmpty(texture.data));
        }
    }
}
