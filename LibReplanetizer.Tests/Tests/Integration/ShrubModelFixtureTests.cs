// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Linq;
using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class ShrubModelFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetShrubModels_ReturnsNonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotEmpty(parser.GetShrubModels());
        }

        [SkippableFact]
        public void ShrubModel_VertexBuffer_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetShrubModels().Cast<ShrubModel>())
                Assert.NotEmpty(model.vertexBuffer);
        }

        [SkippableFact]
        public void ShrubModel_CullingRadius_Positive()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetShrubModels().Cast<ShrubModel>())
                Assert.True(model.cullingRadius > 0.0f);
        }

        [SkippableFact]
        public void ShrubModel_TextureConfig_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetShrubModels().Cast<ShrubModel>())
                Assert.NotEmpty(model.textureConfig);
        }

        [SkippableFact]
        public void ShrubModel_SerializeHead_Has64Bytes()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetShrubModels().Cast<ShrubModel>())
                Assert.Equal(0x40, model.SerializeHead(0).Length);
        }

        [SkippableFact]
        public void ShrubModel_IndexBuffer_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetShrubModels().Cast<ShrubModel>())
                Assert.NotEmpty(model.indexBuffer);
        }
    }
}
