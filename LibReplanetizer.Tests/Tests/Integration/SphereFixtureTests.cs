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
    public class SphereFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetSpheres_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetSpheres());
        }

        [SkippableFact]
        public void Sphere_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var sphere in parser.GetSpheres())
                Assert.Equal(Sphere.ELEMENTSIZE, sphere.ToByteArray().Length);
        }

        [SkippableFact]
        public void Sphere_IdMatchesIndex()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var spheres = parser.GetSpheres();
            for (int i = 0; i < spheres.Count; i++)
                Assert.Equal(i, spheres[i].id);
        }

        [SkippableFact]
        public void Sphere_PositionIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var sphere in parser.GetSpheres())
            {
                Assert.True(float.IsFinite(sphere.position.X));
                Assert.True(float.IsFinite(sphere.position.Y));
                Assert.True(float.IsFinite(sphere.position.Z));
            }
        }
    }
}
