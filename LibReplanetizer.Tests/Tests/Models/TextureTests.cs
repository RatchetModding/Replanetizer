// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Xunit;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.Models
{
    public class TextureTests
    {
        private static byte[] BuildTextureBlock(int index,
            int vramPointer, byte unk04, byte mipMaps, byte format, byte unk07,
            int off08, int off0C, int gtfFlags, int off14,
            short width, short height, int off1C, int off20)
        {
            byte[] block = new byte[(index + 1) * Texture.TEXTUREELEMSIZE];
            int offset = index * Texture.TEXTUREELEMSIZE;
            WriteInt(block, offset + 0x00, vramPointer);
            block[offset + 0x04] = unk04;
            block[offset + 0x05] = mipMaps;
            block[offset + 0x06] = format;
            block[offset + 0x07] = unk07;
            WriteInt(block, offset + 0x08, off08);
            WriteInt(block, offset + 0x0C, off0C);
            WriteInt(block, offset + 0x10, gtfFlags);
            WriteInt(block, offset + 0x14, off14);
            WriteShort(block, offset + 0x18, width);
            WriteShort(block, offset + 0x1A, height);
            WriteInt(block, offset + 0x1C, off1C);
            WriteInt(block, offset + 0x20, off20);
            return block;
        }

        [Fact]
        public void Constructor_ParsesWidthHeight()
        {
            byte[] block = BuildTextureBlock(0, 0, 0, 1, 0x88, 0x29, 0, 0, 0, 0, 256, 128, 0, 0);
            var tex = new Texture(block, 0);

            Assert.Equal(256, tex.width);
            Assert.Equal(128, tex.height);
        }

        [Fact]
        public void Constructor_ParsesCompressionFormat()
        {
            byte[] block = BuildTextureBlock(0, 0, 0, 1, (byte) Texture.CompressionFormat.BC1, 0, 0, 0, 0, 0, 64, 64, 0, 0);
            var tex = new Texture(block, 0);

            Assert.Equal(Texture.CompressionFormat.BC1, tex.compressionFormat);
        }

        [Fact]
        public void Constructor_SetsIndexAsId()
        {
            byte[] block = BuildTextureBlock(2, 0, 0, 1, 0x88, 0, 0, 0, 0, 0, 64, 64, 0, 0);
            var tex = new Texture(block, 2);
            Assert.Equal(2, tex.id);
        }

        [Fact]
        public void Serialize_RoundTrip_MatchesOriginalBlock()
        {
            int vramOffset = 0x1000;
            byte[] block = BuildTextureBlock(0, vramOffset, 0x01, 3, 0x88, 0x29,
                0x00010101, unchecked((int) 0x80030000), 0x0000AAE4, 0x02063E80,
                512, 256, 0x00100000, 0x00FF0000);

            var tex = new Texture(block, 0);
            byte[] serialized = tex.Serialize(vramOffset);

            Assert.Equal(block, serialized);
        }

        [Fact]
        public void Serialize_WritesSuppliedVramOffset()
        {
            byte[] block = BuildTextureBlock(0, 0, 0, 1, 0x88, 0, 0, 0, 0, 0, 64, 64, 0, 0);
            var tex = new Texture(block, 0);
            byte[] serialized = tex.Serialize(0xABCD);

            Assert.Equal(0xABCD, ReadInt(serialized, 0x00));
        }

        [Fact]
        public void Constructor_FromValues_SetsCorrectDefaults()
        {
            byte[] data = new byte[64];
            var tex = new Texture(0, 64, 64, data, Texture.CompressionFormat.BC3);

            Assert.Equal(64, tex.width);
            Assert.Equal(64, tex.height);
            Assert.Equal(Texture.CompressionFormat.BC3, tex.compressionFormat);
            Assert.Equal(1, tex.mipMapCount);
        }
    }
}
