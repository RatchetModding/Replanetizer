// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Linq;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class AnimationFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        private static MobyModel? GetFirstModelWithAnimations(EngineParser parser)
        {
            return parser.GetMobyModels()
                         .Cast<MobyModel>()
                         .FirstOrDefault(m => m.animations.Count > 0);
        }

        [SkippableFact]
        public void AtLeastOneModel_HasAnimations()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAnimations(parser);
            Skip.If(model == null, "No moby model with animations found in this level.");
            Assert.NotEmpty(model!.animations);
        }

        [SkippableFact]
        public void Animation_Frames_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAnimations(parser);
            Skip.If(model == null, "No moby model with animations found in this level.");
            foreach (var anim in model!.animations)
                Assert.NotNull(anim.frames);
        }

        [SkippableFact]
        public void Animation_Speed_NonNegative()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAnimations(parser);
            Skip.If(model == null, "No moby model with animations found in this level.");
            foreach (var anim in model!.animations)
                Assert.True(anim.speed >= 0, $"Animation speed should be non-negative, got {anim.speed}.");
        }

        [SkippableFact]
        public void Animation_HasAtLeastOneFrame()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAnimations(parser);
            Skip.If(model == null, "No moby model with animations found in this level.");
            var animWithFrames = model!.animations.FirstOrDefault(a => a.frames.Count > 0);
            Skip.If(animWithFrames == null, "No animation with frames found.");
            Assert.NotEmpty(animWithFrames!.frames);
        }
    }
}
