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
    public class BangleFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        private static MobyModel? GetFirstModelWithBangles(EngineParser parser)
        {
            return parser.GetMobyModels()
                         .Cast<MobyModel>()
                         .FirstOrDefault(m => m.bangles.Count > 0);
        }

        [SkippableFact]
        public void AtLeastOneModel_HasBangles()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithBangles(parser);
            Skip.If(model == null, "No moby model with bangles found in this level.");
            Assert.NotEmpty(model!.bangles);
        }

        [SkippableFact]
        public void Bangle_VertexBuffer_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithBangles(parser);
            Skip.If(model == null, "No moby model with bangles found in this level.");
            foreach (var bangle in model!.bangles)
                Assert.True(bangle.vertexBuffer.Length > 0 || bangle.metalVertexBuffer.Length > 0,
                    "Bangle should have vertices in at least one buffer.");
        }

        [SkippableFact]
        public void Bangle_IsMetalModel()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithBangles(parser);
            Skip.If(model == null, "No moby model with bangles found in this level.");
            foreach (var bangle in model!.bangles)
                Assert.IsType<Bangle>(bangle);
        }
    }
}
