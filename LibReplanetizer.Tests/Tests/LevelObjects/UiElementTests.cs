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
    public class UiElementTests
    {
        private static (byte[] headBlock, byte[] texBlock) BuildBlocks(int num, short id, int[] sprites)
        {
            // Header entry: 2-byte id, 2-byte count, 2-byte spriteOffset, 2-byte padding
            int headEntrySize = 8;
            byte[] head = new byte[(num + 1) * headEntrySize];
            int hOff = num * headEntrySize;
            WriteShort(head, hOff + 0x00, id);
            WriteShort(head, hOff + 0x02, (short) sprites.Length);
            WriteShort(head, hOff + 0x04, 0); // spriteOffset = 0 in texBlock

            // Tex block: one int per sprite
            byte[] tex = new byte[sprites.Length * 4];
            for (int i = 0; i < sprites.Length; i++)
                WriteInt(tex, i * 4, sprites[i]);

            return (head, tex);
        }

        [Fact]
        public void Constructor_ParsesId()
        {
            var (head, tex) = BuildBlocks(0, 42, new[] { 1, 2 });
            var ui = new UiElement(head, 0, tex);
            Assert.Equal(42, ui.id);
        }

        [Fact]
        public void Constructor_ParsesSpriteList()
        {
            var (head, tex) = BuildBlocks(0, 0, new[] { 100, 200, 300 });
            var ui = new UiElement(head, 0, tex);
            Assert.Equal(3, ui.sprites.Count);
            Assert.Equal(100, ui.sprites[0]);
            Assert.Equal(200, ui.sprites[1]);
            Assert.Equal(300, ui.sprites[2]);
        }

        [Fact]
        public void Constructor_EmptySpriteList()
        {
            var (head, tex) = BuildBlocks(0, 5, new int[0]);
            var ui = new UiElement(head, 0, tex);
            Assert.Empty(ui.sprites);
        }

        [Fact]
        public void Constructor_ParsesAtNonZeroIndex()
        {
            // Two entries in headBlock; we parse index 1
            var (_, tex) = BuildBlocks(0, 0, new[] { 99 });
            byte[] head = new byte[16];
            WriteShort(head, 0x00, 0);   // id at entry 0
            WriteShort(head, 0x02, 0);   // count = 0
            WriteShort(head, 0x04, 0);   // offset
            WriteShort(head, 0x08, 55);  // id at entry 1
            WriteShort(head, 0x0A, 1);   // count = 1
            WriteShort(head, 0x0C, 0);   // spriteOffset = 0

            var ui = new UiElement(head, 1, tex);
            Assert.Equal(55, ui.id);
            Assert.Single(ui.sprites);
            Assert.Equal(99, ui.sprites[0]);
        }
    }
}
