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
    public class SoundInstanceFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetSoundInstances_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetSoundInstances());
        }

        [SkippableFact]
        public void SoundInstance_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var snd in parser.GetSoundInstances())
                Assert.Equal(SoundInstance.ELEMENTSIZE, snd.ToByteArray().Length);
        }

        [SkippableFact]
        public void SoundInstance_SerializeRoundTrip_IdPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var snd in parser.GetSoundInstances())
            {
                byte[] bytes = snd.ToByteArray();
                Assert.Equal(snd.id, DataFunctions.ReadUshort(bytes, 0x00));
            }
        }

        [SkippableFact]
        public void SoundInstance_SerializeRoundTrip_PvarIndexPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var snd in parser.GetSoundInstances())
            {
                byte[] bytes = snd.ToByteArray();
                Assert.Equal(snd.pvarIndex, DataFunctions.ReadInt(bytes, 0x08));
            }
        }

        [SkippableFact]
        public void SoundInstance_SerializeRoundTrip_UpdateDistancePreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var snd in parser.GetSoundInstances())
            {
                byte[] bytes = snd.ToByteArray();
                Assert.Equal(snd.updateDistance, DataFunctions.ReadFloat(bytes, 0x0C), 5);
            }
        }

        [SkippableFact]
        public void SoundInstance_PositionIsFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var snd in parser.GetSoundInstances())
            {
                Assert.True(float.IsFinite(snd.position.X));
                Assert.True(float.IsFinite(snd.position.Y));
                Assert.True(float.IsFinite(snd.position.Z));
            }
        }
    }
}
