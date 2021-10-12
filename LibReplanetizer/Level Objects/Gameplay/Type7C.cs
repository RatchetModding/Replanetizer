// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type7C
    {
        public const int ELEMENTSIZE = 0x20;

        public float off00;
        public float off04;
        public float off08;
        public float off0C;

        public int off10;
        public int off14;
        public int off18;
        public int off1C;

        public Type7C(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            off00 = ReadFloat(block, offset + 0x00);
            off04 = ReadFloat(block, offset + 0x04);
            off08 = ReadFloat(block, offset + 0x08);
            off0C = ReadFloat(block, offset + 0x0C);

            off10 = ReadInt(block, offset + 0x10);
            off14 = ReadInt(block, offset + 0x14);
            off18 = ReadInt(block, offset + 0x18);
            off1C = ReadInt(block, offset + 0x1C);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteFloat(bytes, 0x00, off00);
            WriteFloat(bytes, 0x04, off04);
            WriteFloat(bytes, 0x08, off08);
            WriteFloat(bytes, 0x0C, off0C);

            WriteInt(bytes, 0x10, off10);
            WriteInt(bytes, 0x14, off14);
            WriteInt(bytes, 0x18, off18);
            WriteInt(bytes, 0x1C, off1C);

            return bytes;
        }
    }
}
