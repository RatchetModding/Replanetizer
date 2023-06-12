// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class GlobalPvarBlock : LevelObject
    {
        public const int ELEMENTSIZE = 0x08;

        public ushort off00;
        public ushort off02;
        public ushort off04;
        public ushort off06;

        public GlobalPvarBlock(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            off00 = ReadUshort(block, offset + 0x00);
            off02 = ReadUshort(block, offset + 0x02);
            off04 = ReadUshort(block, offset + 0x04);
            off06 = ReadUshort(block, offset + 0x06);
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteUshort(bytes, 0x00, off00);
            WriteUshort(bytes, 0x02, off02);
            WriteUshort(bytes, 0x04, off04);
            WriteUshort(bytes, 0x06, off06);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

    }
}
