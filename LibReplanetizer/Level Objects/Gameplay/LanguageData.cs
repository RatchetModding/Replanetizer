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
    public class LanguageData
    {
        public const int ELEMENTSIZE = 0x10;
        public int id;
        public int secondId; // sound id?
        public byte[] text;

        public LanguageData(byte[] block, int index)
        {
            int headOffset = 8 + index * ELEMENTSIZE;

            int offset = ReadInt(block, headOffset + 0x00);
            id = ReadInt(block, headOffset + 0x04);
            secondId = ReadInt(block, headOffset + 0x08);

            List<byte> output = new();

            output.AddRange(block[offset..(offset + 4)]);
            while (block[offset + 3] != 0)
            {
                offset += 4;
                output.AddRange(block[offset..(offset + 4)]);
            }

            output.RemoveAll(item => item == 0);
            text = output.ToArray();
        }

    }
}
