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
    public class EnvTransitionFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetEnvTransitions_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetEnvTransitions());
        }

        [SkippableFact]
        public void EnvTransition_ToByteArrayHead_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var et in parser.GetEnvTransitions())
                Assert.Equal(EnvTransition.HEADSIZE, et.ToByteArrayHead().Length);
        }

        [SkippableFact]
        public void EnvTransition_ToByteArrayMain_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var et in parser.GetEnvTransitions())
                Assert.Equal(EnvTransition.ELEMENTSIZE, et.ToByteArrayMain().Length);
        }

        [SkippableFact]
        public void EnvTransition_RadiusIsNonNegative()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var et in parser.GetEnvTransitions())
                Assert.True(et.radius >= 0f, $"EnvTransition id={et.id} has negative radius.");
        }

        [SkippableFact]
        public void EnvTransition_HeadRoundTrip_RadiusPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var et in parser.GetEnvTransitions())
            {
                byte[] head = et.ToByteArrayHead();
                Assert.Equal(et.radius, DataFunctions.ReadFloat(head, 0x0C), 5);
            }
        }

        [SkippableFact]
        public void EnvTransition_HeadRoundTrip_PositionPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var et in parser.GetEnvTransitions())
            {
                byte[] head = et.ToByteArrayHead();
                Assert.Equal(et.position.X, DataFunctions.ReadFloat(head, 0x00), 5);
                Assert.Equal(et.position.Y, DataFunctions.ReadFloat(head, 0x04), 5);
                Assert.Equal(et.position.Z, DataFunctions.ReadFloat(head, 0x08), 5);
            }
        }

        [SkippableFact]
        public void EnvTransition_MainRoundTrip_HeroLight1Preserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var et in parser.GetEnvTransitions())
            {
                byte[] main = et.ToByteArrayMain();
                Assert.Equal(et.heroLight1, DataFunctions.ReadInt(main, 0x48));
            }
        }
    }
}
