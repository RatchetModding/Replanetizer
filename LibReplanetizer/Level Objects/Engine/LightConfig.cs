// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using static LibReplanetizer.DataFunctions;


namespace LibReplanetizer.LevelObjects
{
    public class LightConfig
    {
        public float off00 { get; set; }
        public float off04 { get; set; }
        public float off08 { get; set; }
        public float off0C { get; set; }

        public float off10 { get; set; }
        public float off14 { get; set; }
        public float off18 { get; set; }
        public float off1C { get; set; }

        public float off20 { get; set; }
        public float off24 { get; set; }
        public float off28 { get; set; }
        public float off2C { get; set; }

        int length;

        public LightConfig(byte[] block, int len)
        {
            length = len;

            off00 = ReadFloat(block, 0x00);
            off04 = ReadFloat(block, 0x04);
            off08 = ReadFloat(block, 0x08);
            off0C = ReadFloat(block, 0x0C);

            off10 = ReadFloat(block, 0x10);
            off14 = ReadFloat(block, 0x14);
            off18 = ReadFloat(block, 0x18);
            off1C = ReadFloat(block, 0x1C);

            off20 = ReadFloat(block, 0x20);
            off24 = ReadFloat(block, 0x24);
            off28 = ReadFloat(block, 0x28);
            off2C = ReadFloat(block, 0x2C);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[length];

            WriteFloat(bytes, 0x00, off00);
            WriteFloat(bytes, 0x04, off04);
            WriteFloat(bytes, 0x08, off08);
            WriteFloat(bytes, 0x0C, off0C);

            WriteFloat(bytes, 0x10, off10);
            WriteFloat(bytes, 0x14, off14);
            WriteFloat(bytes, 0x18, off18);
            WriteFloat(bytes, 0x1C, off1C);

            WriteFloat(bytes, 0x20, off20);
            WriteFloat(bytes, 0x24, off24);
            WriteFloat(bytes, 0x28, off28);
            WriteFloat(bytes, 0x2C, off2C);

            return bytes;
        }
    }
}
