// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Xunit;
using LibReplanetizer.LevelObjects;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.LevelObjects
{
    public class LightConfigTests
    {
        private static byte[] BuildLightConfigBlock(float[] values)
        {
            // values must have 12 elements, written at offsets 0x00..0x2C
            byte[] block = new byte[0x30];
            int[] offsets = { 0x00, 0x04, 0x08, 0x0C, 0x10, 0x14, 0x18, 0x1C, 0x20, 0x24, 0x28, 0x2C };
            for (int i = 0; i < 12; i++)
            {
                WriteFloat(block, offsets[i], values[i]);
            }
            return block;
        }

        [Fact]
        public void Constructor_ParsesAllFloatsCorrectly()
        {
            float[] values = { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f };
            byte[] block = BuildLightConfigBlock(values);
            var config = new LightConfig(block, block.Length);

            Assert.Equal(values[0],  config.off00);
            Assert.Equal(values[1],  config.off04);
            Assert.Equal(values[2],  config.off08);
            Assert.Equal(values[3],  config.off0C);
            Assert.Equal(values[4],  config.off10);
            Assert.Equal(values[5],  config.off14);
            Assert.Equal(values[6],  config.off18);
            Assert.Equal(values[7],  config.off1C);
            Assert.Equal(values[8],  config.off20);
            Assert.Equal(values[9],  config.off24);
            Assert.Equal(values[10], config.off28);
            Assert.Equal(values[11], config.off2C);
        }

        [Fact]
        public void Serialize_RoundTrip_MatchesOriginalBytes()
        {
            float[] values = { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f };
            byte[] original = BuildLightConfigBlock(values);
            var config = new LightConfig(original, original.Length);
            byte[] serialized = config.Serialize();

            Assert.Equal(original, serialized);
        }
    }
}
