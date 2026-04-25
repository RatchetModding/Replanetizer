// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class GrindPathFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetGrindPaths_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetGrindPaths());
        }

        [SkippableFact]
        public void GrindPath_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var path in parser.GetGrindPaths())
                Assert.Equal(GrindPath.ELEMENTSIZE, path.ToByteArray().Length);
        }

        [SkippableFact]
        public void GrindPath_RadiusIsNonNegative()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var path in parser.GetGrindPaths())
                Assert.True(path.radius >= 0f, $"GrindPath id={path.id} has negative radius.");
        }

        [SkippableFact]
        public void GrindPath_SplineIsNotNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var path in parser.GetGrindPaths())
                Assert.NotNull(path.spline);
        }

        [SkippableFact]
        public void GrindPath_SerializeRoundTrip_RadiusPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var path in parser.GetGrindPaths())
            {
                byte[] bytes = path.ToByteArray();
                Assert.Equal(path.radius, DataFunctions.ReadFloat(bytes, 0x0C), 5);
            }
        }

        [SkippableFact]
        public void GrindPath_SerializeRoundTrip_PositionPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var path in parser.GetGrindPaths())
            {
                byte[] bytes = path.ToByteArray();
                Assert.Equal(path.position.X, DataFunctions.ReadFloat(bytes, 0x00), 5);
                Assert.Equal(path.position.Y, DataFunctions.ReadFloat(bytes, 0x04), 5);
                Assert.Equal(path.position.Z, DataFunctions.ReadFloat(bytes, 0x08), 5);
            }
        }
    }
}
