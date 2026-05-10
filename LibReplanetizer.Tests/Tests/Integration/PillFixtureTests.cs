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
    public class PillFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetPills_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetPills());
        }

        [SkippableFact]
        public void Pill_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var pill in parser.GetPills())
                Assert.Equal(Pill.ELEMENTSIZE, pill.ToByteArray().Length);
        }

        [SkippableFact]
        public void Pill_IdMatchesIndex()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var pills = parser.GetPills();
            for (int i = 0; i < pills.Count; i++)
                Assert.Equal(i, pills[i].id);
        }

        [SkippableFact]
        public void Pill_PositionIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var pill in parser.GetPills())
            {
                Assert.True(float.IsFinite(pill.position.X));
                Assert.True(float.IsFinite(pill.position.Y));
                Assert.True(float.IsFinite(pill.position.Z));
            }
        }
    }
}
