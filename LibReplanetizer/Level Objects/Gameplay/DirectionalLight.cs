using OpenTK;
using System;
using System.Drawing;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class DirectionalLight : LevelObject
    {
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

            float dirAX = ReadFloat(block, offset + 0x10);
            float dirAY = ReadFloat(block, offset + 0x14);
            float dirAZ = ReadFloat(block, offset + 0x18);
            float dirAW = ReadFloat(block, offset + 0x1C);

            float colorBRed = ReadFloat(block, offset + 0x20);
            float colorBGreen = ReadFloat(block, offset + 0x24);
            float colorBBlue = ReadFloat(block, offset + 0x28);
            float colorBAlpha = ReadFloat(block, offset + 0x2C);

            float dirBX = ReadFloat(block, offset + 0x30);
            float dirBY = ReadFloat(block, offset + 0x34);
            float dirBZ = ReadFloat(block, offset + 0x38);
            float dirBW = ReadFloat(block, offset + 0x3C);

            colorA = new Vector4(colorARed, colorAGreen, colorABlue, colorAAlpha);
            directionA = new Vector4(dirAX, dirAY, dirAZ, dirAW);
            colorB = new Vector4(colorBRed, colorBGreen, colorBBlue, colorBAlpha);
            directionB = new Vector4(dirBX, dirBY, dirBZ, dirBW);
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
