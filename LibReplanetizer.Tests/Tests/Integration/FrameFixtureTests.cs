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
    public class FrameFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        private static Frame? GetFirstFrame(EngineParser parser)
        {
            return parser.GetMobyModels()
                         .Cast<MobyModel>()
                         .SelectMany(m => m.animations)
                         .SelectMany(a => a.frames)
                         .FirstOrDefault();
        }

        [SkippableFact]
        public void Frame_CanBeRetrieved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var frame = GetFirstFrame(parser);
            Skip.If(frame == null, "No animation frames found in this level.");
            Assert.NotNull(frame);
        }

        [SkippableFact]
        public void Frame_Speed_NonNegative()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var frame = GetFirstFrame(parser);
            Skip.If(frame == null, "No animation frames found in this level.");
            Assert.True(frame!.speed >= 0, $"Frame speed should be non-negative, got {frame!.speed}.");
        }

        [SkippableFact]
        public void Frame_FrameIndex_NonNegative()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var frame = GetFirstFrame(parser);
            Skip.If(frame == null, "No animation frames found in this level.");
            Assert.True(frame!.frameIndex >= 0);
        }

        [SkippableFact]
        public void Frame_GetRotationMatrix_ReturnsSomething()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var frame = GetFirstFrame(parser);
            Skip.If(frame == null, "No animation frames found in this level.");
            // Bone 0 may or may not have a rotation, but should not throw.
            var mat = frame!.GetRotationMatrix(0);
        }
    }
}
