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
    public class CylinderFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetCylinders_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetCylinders());
        }

        [SkippableFact]
        public void Cylinder_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var cyl in parser.GetCylinders())
                Assert.Equal(Cylinder.ELEMENTSIZE, cyl.ToByteArray().Length);
        }

        [SkippableFact]
        public void Cylinder_IdMatchesIndex()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var cylinders = parser.GetCylinders();
            for (int i = 0; i < cylinders.Count; i++)
                Assert.Equal(i, cylinders[i].id);
        }

        [SkippableFact]
        public void Cylinder_PositionIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var cyl in parser.GetCylinders())
            {
                Assert.True(float.IsFinite(cyl.position.X));
                Assert.True(float.IsFinite(cyl.position.Y));
                Assert.True(float.IsFinite(cyl.position.Z));
            }
        }
    }
}
