// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class SkyboxModelFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetSkyboxModel_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotNull(parser.GetSkyboxModel());
        }

        [SkippableFact]
        public void SkyboxModel_VertexBuffer_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var skybox = parser.GetSkyboxModel();
            Assert.NotEmpty(skybox.vertexBuffer);
        }

        [SkippableFact]
        public void SkyboxModel_TextureConfigs_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var skybox = parser.GetSkyboxModel();
            Assert.NotNull(skybox.textureConfigs);
        }

        [SkippableFact]
        public void SkyboxModel_VertexStride_Is6()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var skybox = parser.GetSkyboxModel();
            // Skybox has no normals → stride 6 instead of the default 8.
            Assert.Equal(6, skybox.vertexStride);
        }

        [SkippableFact]
        public void SkyboxModel_VertexCount_ConsistentWithBuffer()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var skybox = parser.GetSkyboxModel();
            Assert.Equal(skybox.vertexBuffer.Length / skybox.vertexStride, skybox.vertexCount);
        }
    }
}
