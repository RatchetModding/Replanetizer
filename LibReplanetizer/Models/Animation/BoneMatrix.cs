// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{
    public class BoneMatrix
    {
        public Matrix3x4 transformation;
        public float cumulativeOffsetX, cumulativeOffsetY, cumulativeOffsetZ;
        public short parent;
        public short id;
        public short unk0x3C;

        //The last 4 bytes and the translation are also present in the BoneData.

        public BoneMatrix(byte[] boneBlock, int num)
        {
            int offset = num * 0x40;
            id = (short) (offset / 0x40);

            transformation = ReadMatrix3x4(boneBlock, offset);

            cumulativeOffsetX = ReadFloat(boneBlock, offset + 0x30);
            cumulativeOffsetY = ReadFloat(boneBlock, offset + 0x34);
            cumulativeOffsetZ = ReadFloat(boneBlock, offset + 0x38);

            //0 for root and some constant else (0b0111000000000000 = 0x7000 = 28672)
            unk0x3C = ReadShort(boneBlock, offset + 0x3C);
            parent = (short) (ReadShort(boneBlock, offset + 0x3E) / 0x40);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x40];
            WriteMatrix3x4(outBytes, 0x00, transformation);
            WriteFloat(outBytes, 0x30, cumulativeOffsetX);
            WriteFloat(outBytes, 0x34, cumulativeOffsetY);
            WriteFloat(outBytes, 0x38, cumulativeOffsetZ);
            WriteShort(outBytes, 0x3C, unk0x3C);
            WriteShort(outBytes, 0x3E, (short) (parent * 0x40));

            return outBytes;
        }
    }
}
