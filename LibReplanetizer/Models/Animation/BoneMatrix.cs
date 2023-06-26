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
        private Vector3 cumulativeOffset;    // Private because this data is not available in Deadlocked.
        private short parent;   // Private because this data is not available in Deadlocked.
        public short id;
        public short unk0x3C;

        //The last 4 bytes and the translation are also present in the BoneData.

        public BoneMatrix(GameType game, byte[] boneBlock, int num)
        {
            if (game == GameType.DL)
            {
                GetDLVals(boneBlock, num);
            }
            else
            {
                GetRC123Vals(boneBlock, num);
            }
        }

        private void GetRC123Vals(byte[] boneBlock, int num)
        {
            int offset = num * 0x40;
            id = (short) (offset / 0x40);

            transformation = ReadMatrix3x4(boneBlock, offset);

            float cumulativeOffsetX = ReadFloat(boneBlock, offset + 0x30);
            float cumulativeOffsetY = ReadFloat(boneBlock, offset + 0x34);
            float cumulativeOffsetZ = ReadFloat(boneBlock, offset + 0x38);

            cumulativeOffset = new Vector3(cumulativeOffsetX / 1024.0f, cumulativeOffsetY / 1024.0f, cumulativeOffsetZ / 1024.0f);

            //0 for root and some constant else (0b0111000000000000 = 0x7000 = 28672)
            unk0x3C = ReadShort(boneBlock, offset + 0x3C);
            parent = (short) (ReadShort(boneBlock, offset + 0x3E) / 0x40);
        }

        private void GetDLVals(byte[] boneBlock, int num)
        {
            int offset = num * 0x30;
            id = (short) (offset / 0x30);

            transformation = ReadMatrix3x4(boneBlock, offset);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x40];
            WriteMatrix3x4(outBytes, 0x00, transformation);
            WriteFloat(outBytes, 0x30, cumulativeOffset.X * 1024.0f);
            WriteFloat(outBytes, 0x34, cumulativeOffset.Y * 1024.0f);
            WriteFloat(outBytes, 0x38, cumulativeOffset.Z * 1024.0f);
            WriteShort(outBytes, 0x3C, unk0x3C);
            WriteShort(outBytes, 0x3E, (short) (parent * 0x40));

            return outBytes;
        }
    }
}
