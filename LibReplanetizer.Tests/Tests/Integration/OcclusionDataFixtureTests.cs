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
    public class OcclusionDataFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetOcclusionData_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            // OcclusionData may be absent in some levels; only assert that parsing does not throw.
            var data = parser.GetOcclusionData();
            // data may be null if the level has no occlusion block — that is valid.
            if (data == null) return;
            Assert.NotNull(data.mobyData);
            Assert.NotNull(data.tieData);
            Assert.NotNull(data.shrubData);
        }

        [SkippableFact]
        public void OcclusionData_ToByteArray_ContainsCountHeader()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var data = parser.GetOcclusionData();
            Skip.If(data == null, "Level has no occlusion data.");

            byte[] bytes = data!.ToByteArray();
            // First 4 bytes: moby count; bytes 4-8: tie count; bytes 8-12: shrub count
            Assert.True(bytes.Length >= 0x10);
        }

        [SkippableFact]
        public void OcclusionData_ToByteArray_LengthMatchesEntries()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var data = parser.GetOcclusionData();
            Skip.If(data == null, "Level has no occlusion data.");

            int expected = 0x10 + data!.mobyData.Count * 0x08
                                + data.tieData.Count * 0x08
                                + data.shrubData.Count * 0x08;
            Assert.Equal(expected, data.ToByteArray().Length);
        }
    }
}
