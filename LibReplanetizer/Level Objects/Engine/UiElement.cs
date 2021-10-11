// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class UiElement
    {
        public short id;
        public List<int> sprites;

        public UiElement(byte[] headBlock, int num, byte[] texBlock)
        {

            int offset = num * 8;
            id = ReadShort(headBlock, offset + 0x00);

            short spriteCount = ReadShort(headBlock, offset + 0x02);
            short spriteOffset = ReadShort(headBlock, offset + 0x04);

            sprites = new List<int>();
            for (int i = 0; i < spriteCount; i++)
            {
                sprites.Add(ReadInt(texBlock, (spriteOffset + i) * 4));
            }
        }

        /*public byte[] ToByteArray()
        {

        }*/
    }
}
