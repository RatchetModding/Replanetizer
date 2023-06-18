// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.ComponentModel;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class PointLight : LevelObject
    {
        private const int ELEMENTSIZE_RAC1 = 0x20;
        private const int ELEMENTSIZE_RAC23DL = 0x10;
        private GameType game;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }

        [Category("Attributes"), DisplayName("Radius")]
        public float radius { get; set; }

        [Category("Attributes"), DisplayName("Color")]
        public Vector3 color { get; set; }

        public static int GetElementSize(GameType game)
        {
            switch (game.num)
            {
                case 1:
                    return ELEMENTSIZE_RAC1;
                case 2:
                case 3:
                case 4:
                default:
                    return ELEMENTSIZE_RAC23DL;
            }
        }

        public PointLight(GameType game, byte[] block, int num)
        {
            this.game = game;
            id = num;

            switch (game.num)
            {
                case 1:
                    GetRC1Vals(block, num);
                    break;
                case 2:
                case 3:
                case 4:
                default:
                    GetRC23DLVals(block, num);
                    break;
            }

            UpdateTransformMatrix();
        }

        private void GetRC1Vals(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE_RAC1;

            float posX = ReadFloat(block, offset + 0x00);
            float posY = ReadFloat(block, offset + 0x04);
            float posZ = ReadFloat(block, offset + 0x08);
            radius = ReadFloat(block, offset + 0x0C);

            byte red = block[offset + 0x10];
            byte green = block[offset + 0x11];
            byte blue = block[offset + 0x12];

            position = new Vector3(posX, posY, posZ);
            color = new Vector3(red / 255.0f, green / 255.0f, blue / 255.0f);
        }

        private void GetRC23DLVals(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE_RAC23DL;

            float x = ReadShort(block, offset + 0x00) * (1.0f / 64.0f);
            float y = ReadShort(block, offset + 0x02) * (1.0f / 64.0f);
            float z = ReadShort(block, offset + 0x04) * (1.0f / 64.0f);
            radius = ReadShort(block, offset + 0x06) * (1.0f / 64.0f);
            ushort red = ReadUshort(block, offset + 0x08);
            ushort green = ReadUshort(block, offset + 0x0A);
            ushort blue = ReadUshort(block, offset + 0x0C);

            position = new Vector3(x, y, z);
            color = new Vector3(red / 65535.0f, green / 65535.0f, blue / 65535.0f);
        }

        public override byte[] ToByteArray()
        {
            switch (game.num)
            {
                case 1:
                    return ToByteArrayRC1();
                case 2:
                case 3:
                case 4:
                default:
                    return ToByteArrayRC23DL();
            }
        }

        private byte[] ToByteArrayRC1()
        {
            byte[] bytes = new byte[ELEMENTSIZE_RAC1];

            WriteFloat(bytes, 0x00, position.X);
            WriteFloat(bytes, 0x04, position.Y);
            WriteFloat(bytes, 0x08, position.Z);
            WriteFloat(bytes, 0x0C, radius);

            bytes[0x10] = (byte) MathF.Round(color.X * 255.0f);
            bytes[0x11] = (byte) MathF.Round(color.Y * 255.0f);
            bytes[0x12] = (byte) MathF.Round(color.Z * 255.0f);

            return bytes;
        }

        private byte[] ToByteArrayRC23DL()
        {
            byte[] bytes = new byte[ELEMENTSIZE_RAC23DL];

            WriteShort(bytes, 0x00, (short) MathF.Round(position.X * 64.0f));
            WriteShort(bytes, 0x02, (short) MathF.Round(position.Y * 64.0f));
            WriteShort(bytes, 0x04, (short) MathF.Round(position.Z * 64.0f));
            WriteShort(bytes, 0x06, (short) MathF.Round(radius * 64.0f));
            WriteUshort(bytes, 0x08, (ushort) MathF.Round(color.X * 64.0f));
            WriteUshort(bytes, 0x0A, (ushort) MathF.Round(color.Y * 64.0f));
            WriteUshort(bytes, 0x0C, (ushort) MathF.Round(color.Z * 64.0f));

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsDynamic()
        {
            return false;
        }
    }
}
