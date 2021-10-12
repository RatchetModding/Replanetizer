// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type80 : MatrixObject
    {
        public const int HEADSIZE = 0x10;
        public const int DATASIZE = 0x80;

        public float off00;
        public float off04;
        public float off08;
        public float off0C;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type80(byte[] block, byte[] dataBlock, int num)
        {
            int headOffset = num * HEADSIZE;
            int dataOffset = num * DATASIZE;

            off00 = ReadFloat(block, headOffset + 0x00);
            off04 = ReadFloat(block, headOffset + 0x04);
            off08 = ReadFloat(block, headOffset + 0x08);
            off0C = ReadFloat(block, headOffset + 0x0C);

            mat1 = ReadMatrix4(dataBlock, dataOffset + 0x00); //May be matrix data, but i'm not sure.
            mat2 = ReadMatrix4(dataBlock, dataOffset + 0x40); //Don't think this is a matrix.

            rotation = mat1.ExtractRotation();
            position = mat1.ExtractTranslation();
            UpdateTransformMatrix();
        }

        public byte[] SerializeHead()
        {
            byte[] bytes = new byte[HEADSIZE];

            WriteFloat(bytes, 0x00, off00);
            WriteFloat(bytes, 0x04, off04);
            WriteFloat(bytes, 0x08, off08);
            WriteFloat(bytes, 0x0C, off0C);

            return bytes;
        }

        public byte[] SerializeData()
        {
            byte[] bytes = new byte[DATASIZE];

            WriteMatrix4(bytes, 0x00, mat1);
            WriteMatrix4(bytes, 0x40, mat2);

            return bytes;
        }

        public override byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }
    }
}
