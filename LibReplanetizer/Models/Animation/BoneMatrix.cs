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
        public Matrix4 transformation;
        public float unk1, unk2, unk3;
        public short parent;
        public short id;
        public short unk0x3C;

        //The last 4 bytes and the translation are also present in the BoneData.

        public BoneMatrix(byte[] boneBlock, int num)
        {
            int offset = num * 0x40;
            id = (short) (offset / 0x40);

            transformation = ReadMatrix4(boneBlock, offset);

            transformation.M41 = 0;
            transformation.M42 = 0;
            transformation.M43 = 0;
            transformation.M44 = 1;

            unk1 = ReadFloat(boneBlock, offset + 0x30);
            unk2 = ReadFloat(boneBlock, offset + 0x34);
            unk3 = ReadFloat(boneBlock, offset + 0x38);

            //0 for root and some constant else (0b0111000000000000 = 0x7000 = 28672)
            unk0x3C = ReadShort(boneBlock, offset + 0x3C);
            parent = (short) (ReadShort(boneBlock, offset + 0x3E) / 0x40);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x40];
            /*Matrix4 mat = transformation;
            mat.Column3 = col3;
            WriteMatrix4(outBytes, 0, mat);*/

            return outBytes;
        }
    }
}
