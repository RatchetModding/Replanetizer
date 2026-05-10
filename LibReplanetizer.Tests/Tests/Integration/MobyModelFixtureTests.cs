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
    public class MobyModelFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetMobyModels_ReturnsNonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotEmpty(parser.GetMobyModels());
        }

        [SkippableFact]
        public void MobyModel_VertexBuffer_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.NotEmpty(model.vertexBuffer);
        }

        [SkippableFact]
        public void MobyModel_CullingRadius_Positive()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.True(model.cullingRadius > 0.0f);
        }

        [SkippableFact]
        public void MobyModel_BoneMatrices_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.NotNull(model.boneMatrices);
        }

        [SkippableFact]
        public void MobyModel_BoneDatas_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.NotNull(model.boneDatas);
        }

        [SkippableFact]
        public void MobyModel_Animations_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.NotNull(model.animations);
        }

        [SkippableFact]
        public void MobyModel_ModelSounds_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.NotNull(model.modelSounds);
        }

        [SkippableFact]
        public void MobyModel_BoneCount_MatchesBoneMatricesList()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var model in parser.GetMobyModels().Cast<MobyModel>())
                Assert.Equal(model.boneCount, model.boneMatrices.Count);
        }
    }
}
