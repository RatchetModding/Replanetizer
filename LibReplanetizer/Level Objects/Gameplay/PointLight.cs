// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.ComponentModel;
using OpenTK.Mathematics;
using System.Drawing;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class PointLight : LevelObject
    {
        public const int ELEMENTSIZE = 0x10;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }

        [Category("Attributes"), DisplayName("Radius")]
        public float radius { get; set; }

        [Category("Attributes"), DisplayName("Color")]
        public Vector3 color { get; set; }

        public PointLight(byte[] block, int num)
        {
            id = num;
            int offset = num * ELEMENTSIZE;

            float x = ReadShort(block, offset + 0x00) * (1.0f / 64.0f);
            float y = ReadShort(block, offset + 0x02) * (1.0f / 64.0f);
            float z = ReadShort(block, offset + 0x04) * (1.0f / 64.0f);
            radius = ReadShort(block, offset + 0x06) * (1.0f / 64.0f);
            ushort red = ReadUshort(block, offset + 0x08);
            ushort green = ReadUshort(block, offset + 0x0A);
            ushort blue = ReadUshort(block, offset + 0x0C);

            position = new Vector3(x, y, z);
            color = new Vector3(red / 65535.0f, green / 65535.0f, blue / 65535.0f);

            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

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
