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
    public class LevelVariablesFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetLevelVariables_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetLevelVariables());
        }

        [SkippableFact]
        public void LevelVariables_FogNearDistanceIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var vars = parser.GetLevelVariables();
            Assert.True(float.IsFinite(vars.fogNearDistance));
        }

        [SkippableFact]
        public void LevelVariables_FogFarDistanceIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var vars = parser.GetLevelVariables();
            Assert.True(float.IsFinite(vars.fogFarDistance));
        }

        [SkippableFact]
        public void LevelVariables_DeathPlaneZIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var vars = parser.GetLevelVariables();
            Assert.True(float.IsFinite(vars.deathPlaneZ));
        }

        [SkippableFact]
        public void LevelVariables_ShipPositionIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var vars = parser.GetLevelVariables();
            Assert.True(float.IsFinite(vars.shipPosition.X));
            Assert.True(float.IsFinite(vars.shipPosition.Y));
            Assert.True(float.IsFinite(vars.shipPosition.Z));
        }
    }
}
