// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{
    public class BoneData
    {
        public float unk1;
        public float unk2;
        public float unk3;
        public float unk4;

        public BoneData(byte[] boneDataBlock, int num)
        {
            int offset = num * 0x10;
            unk1 = ReadFloat(boneDataBlock, offset + 0x00);
            unk2 = ReadFloat(boneDataBlock, offset + 0x04);
            unk3 = ReadFloat(boneDataBlock, offset + 0x08);
            unk4 = ReadFloat(boneDataBlock, offset + 0x0C);

            //unk4 = 0;
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x10];

            WriteFloat(outBytes, 0x00, unk1);
            WriteFloat(outBytes, 0x04, unk2);
            WriteFloat(outBytes, 0x08, unk3);
            WriteFloat(outBytes, 0x0C, unk4);

            return outBytes;
        }
    }
}
