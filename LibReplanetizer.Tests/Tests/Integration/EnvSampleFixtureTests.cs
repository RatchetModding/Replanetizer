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
    public class EnvSampleFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetEnvSamples_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetEnvSamples());
        }

        [SkippableFact]
        public void EnvSample_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            int expectedSize = EnvSample.GetElementSize(GameType.RaC1);
            foreach (var sample in parser.GetEnvSamples())
                Assert.Equal(expectedSize, sample.ToByteArray().Length);
        }

        [SkippableFact]
        public void EnvSample_IdMatchesIndex()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var samples = parser.GetEnvSamples();
            for (int i = 0; i < samples.Count; i++)
                Assert.Equal(i, samples[i].id);
        }

        [SkippableFact]
        public void EnvSample_PositionIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var sample in parser.GetEnvSamples())
            {
                Assert.True(float.IsFinite(sample.position.X));
                Assert.True(float.IsFinite(sample.position.Y));
                Assert.True(float.IsFinite(sample.position.Z));
            }
        }

        [SkippableFact]
        public void EnvSample_SerializeRoundTrip_HeroLightPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var sample in parser.GetEnvSamples())
            {
                byte[] bytes = sample.ToByteArray();
                Assert.Equal(sample.heroLight, DataFunctions.ReadInt(bytes, 0x1C));
            }
        }
    }
}
