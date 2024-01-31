// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class DirectionalLight : LevelObject
    {
        // This is the implementation for the DirectionalLights in the gameplay_ntsc file.
        // These are unused and in parts still in little endian.
        // This here is merely for documentation and serialization.
        // Do not mistake this for the actual directional lights which are stored in the engine file.

        public const int ELEMENTSIZE = 0x40;

        public Vector4 colorA;
        public Vector4 directionA;
        public Vector4 colorB;
        public Vector4 directionB;

        public DirectionalLight(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            float colorARed = ReadFloat(block, offset + 0x00);
            float colorAGreen = ReadFloat(block, offset + 0x04);
            float colorABlue = ReadFloat(block, offset + 0x08);
            float colorAAlpha = ReadFloat(block, offset + 0x0C);

            float dirAx = ReadFloat(block, offset + 0x10);
            float dirAy = ReadFloat(block, offset + 0x14);
            float dirAz = ReadFloat(block, offset + 0x18);
            float dirAw = ReadFloat(block, offset + 0x1C);

            float colorBRed = ReadFloat(block, offset + 0x20);
            float colorBGreen = ReadFloat(block, offset + 0x24);
            float colorBBlue = ReadFloat(block, offset + 0x28);
            float colorBAlpha = ReadFloat(block, offset + 0x2C);

            float dirBx = ReadFloat(block, offset + 0x30);
            float dirBy = ReadFloat(block, offset + 0x34);
            float dirBz = ReadFloat(block, offset + 0x38);
            float dirBw = ReadFloat(block, offset + 0x3C);

            colorA = new Vector4(colorARed, colorAGreen, colorABlue, colorAAlpha);
            directionA = new Vector4(dirAx, dirAy, dirAz, dirAw);
            colorB = new Vector4(colorBRed, colorBGreen, colorBBlue, colorBAlpha);
            directionB = new Vector4(dirBx, dirBy, dirBz, dirBw);
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteFloat(bytes, 0x00, colorA.X);
            WriteFloat(bytes, 0x04, colorA.Y);
            WriteFloat(bytes, 0x08, colorA.Z);
            WriteFloat(bytes, 0x0C, colorA.W);

            WriteFloat(bytes, 0x10, directionA.X);
            WriteFloat(bytes, 0x14, directionA.Y);
            WriteFloat(bytes, 0x18, directionA.Z);
            WriteFloat(bytes, 0x1C, directionA.W);

            WriteFloat(bytes, 0x20, colorB.X);
            WriteFloat(bytes, 0x24, colorB.Y);
            WriteFloat(bytes, 0x28, colorB.Z);
            WriteFloat(bytes, 0x2C, colorB.W);

            WriteFloat(bytes, 0x30, directionB.X);
            WriteFloat(bytes, 0x34, directionB.Y);
            WriteFloat(bytes, 0x38, directionB.Z);
            WriteFloat(bytes, 0x3C, directionB.W);

            return bytes;
        }


        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

    }
}
