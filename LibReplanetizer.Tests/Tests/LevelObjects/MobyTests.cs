// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Xunit;
using LibReplanetizer.LevelObjects;

namespace LibReplanetizer.Tests.LevelObjects
{
    /// <summary>
    /// Tests for the Moby spawn bitmask computed properties.
    /// These are pure logic tests — no binary data parsing required.
    /// </summary>
    public class MobySpawnBitmaskTests
    {
        private static Moby MobyWithSpawnType(int spawnType)
        {
            // Use the protected-setter via the public property.
            // Moby cannot be constructed without a byte block and model list, so
            // we rely on the object initialiser with default values.
            var moby = new Moby(GameType.RaC1);
            moby.spawnType = spawnType;
            return moby;
        }

        [Theory]
        [InlineData(0b00001, true)]
        [InlineData(0b00000, false)]
        [InlineData(0b11110, false)]
        public void SpawnBeforeMissionCompletion_ReflectsBit0(int spawnType, bool expected)
        {
            var moby = MobyWithSpawnType(spawnType);
            Assert.Equal(expected, moby.spawnBeforeMissionCompletion);
        }

        [Theory]
        [InlineData(0b00010, true)]
        [InlineData(0b00000, false)]
        [InlineData(0b11101, false)]
        public void SpawnAfterMissionCompletion_ReflectsBit1(int spawnType, bool expected)
        {
            var moby = MobyWithSpawnType(spawnType);
            Assert.Equal(expected, moby.spawnAfterMissionCompletion);
        }

        [Theory]
        [InlineData(0b00100, true)]
        [InlineData(0b00000, false)]
        public void IsCrate_ReflectsBit2(int spawnType, bool expected)
        {
            var moby = MobyWithSpawnType(spawnType);
            Assert.Equal(expected, moby.isCrate);
        }

        [Theory]
        [InlineData(0b01000, true)]
        [InlineData(0b00000, false)]
        public void SpawnBeforeDeath_ReflectsBit3(int spawnType, bool expected)
        {
            var moby = MobyWithSpawnType(spawnType);
            Assert.Equal(expected, moby.spawnBeforeDeath);
        }

        [Fact]
        public void SetSpawnBeforeMissionCompletion_True_SetsBit0()
        {
            var moby = MobyWithSpawnType(0);
            moby.spawnBeforeMissionCompletion = true;
            Assert.True((moby.spawnType & 0b00001) != 0);
        }

        [Fact]
        public void SetSpawnBeforeMissionCompletion_False_ClearsBit0()
        {
            var moby = MobyWithSpawnType(0b11111);
            moby.spawnBeforeMissionCompletion = false;
            Assert.False((moby.spawnType & 0b00001) != 0);
            // Other bits must remain intact
            Assert.True((moby.spawnType & 0b11110) == 0b11110);
        }

        [Fact]
        public void SetIsCrate_DoesNotAffectOtherBits()
        {
            var moby = MobyWithSpawnType(0b11111);
            moby.isCrate = false;
            Assert.Equal(0b11011, (int) moby.spawnType);
        }
    }
}
