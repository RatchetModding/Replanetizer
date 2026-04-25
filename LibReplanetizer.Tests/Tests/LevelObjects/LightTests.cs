// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using Xunit;
using LibReplanetizer.LevelObjects;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.LevelObjects
{
    public class LightTests
    {
        private static byte[] BuildLightBlock(
            Vector4 color1, Vector4 dir1,
            Vector4 color2, Vector4 dir2)
        {
            byte[] block = new byte[0x40];
            WriteFloat(block, 0x00, color1.X);
            WriteFloat(block, 0x04, color1.Y);
            WriteFloat(block, 0x08, color1.Z);
            WriteFloat(block, 0x0C, color1.W);
            WriteFloat(block, 0x10, dir1.X);
            WriteFloat(block, 0x14, dir1.Y);
            WriteFloat(block, 0x18, dir1.Z);
            WriteFloat(block, 0x1C, dir1.W);
            WriteFloat(block, 0x20, color2.X);
            WriteFloat(block, 0x24, color2.Y);
            WriteFloat(block, 0x28, color2.Z);
            WriteFloat(block, 0x2C, color2.W);
            WriteFloat(block, 0x30, dir2.X);
            WriteFloat(block, 0x34, dir2.Y);
            WriteFloat(block, 0x38, dir2.Z);
            WriteFloat(block, 0x3C, dir2.W);
            return block;
        }

        [Fact]
        public void Constructor_ParsesAllChannelsCorrectly()
        {
            var c1 = new Vector4(0.1f, 0.2f, 0.3f, 1.0f);
            var d1 = new Vector4(0.4f, 0.5f, 0.6f, 0.0f);
            var c2 = new Vector4(0.7f, 0.8f, 0.9f, 1.0f);
            var d2 = new Vector4(-1.0f, -2.0f, -3.0f, 0.0f);

            byte[] block = BuildLightBlock(c1, d1, c2, d2);
            var light = new Light(block, 0);

            Assert.Equal(c1, light.color1);
            Assert.Equal(d1, light.direction1);
            Assert.Equal(c2, light.color2);
            Assert.Equal(d2, light.direction2);
        }

        [Fact]
        public void Serialize_ProducesExpectedBytes()
        {
            var c1 = new Vector4(0.1f, 0.2f, 0.3f, 1.0f);
            var d1 = new Vector4(0.4f, 0.5f, 0.6f, 0.0f);
            var c2 = new Vector4(0.7f, 0.8f, 0.9f, 1.0f);
            var d2 = new Vector4(-1.0f, -2.0f, -3.0f, 0.0f);

            byte[] original = BuildLightBlock(c1, d1, c2, d2);
            var light = new Light(original, 0);
            byte[] serialized = light.Serialize();

            Assert.Equal(original, serialized);
        }

        [Fact]
        public void RoundTrip_MultipleElements_UsesCorrectOffset()
        {
            // Build a block with two lights; verify num=1 reads from offset 0x40.
            byte[] block = new byte[0x80];
            var c1 = new Vector4(1f, 0f, 0f, 1f);
            var d1 = new Vector4(0f, 1f, 0f, 0f);
            var c2 = new Vector4(0f, 0f, 1f, 1f);
            var d2 = new Vector4(0f, 0f, 1f, 0f);

            byte[] second = BuildLightBlock(c1, d1, c2, d2);
            second.CopyTo(block, 0x40);

            var light = new Light(block, 1);
            byte[] serialized = light.Serialize();

            Assert.Equal(second, serialized);
        }
    }
}
