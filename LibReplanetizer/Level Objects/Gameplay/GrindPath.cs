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
    public class GrindPath : LevelObject
    {
        public const int ELEMENTSIZE = 0x20;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }
        [Category("Attributes"), DisplayName("Radius")]
        public float radius { get; set; }
        [Category("Attributes"), DisplayName("Wrap")]
        public int wrap { get; set; }
        [Category("Attributes"), DisplayName("Inactive")]
        public int inactive { get; set; }
        [Category("Unknowns"), DisplayName("Offset 0x10")]
        public int unk0x10 { get; set; }
        [Category("Attributes"), DisplayName("Spline")]
        public Spline spline { get; set; }

        public GrindPath(byte[] block, int num, Spline spline)
        {
            id = num;
            int offset = num * ELEMENTSIZE;

            float x = ReadFloat(block, offset + 0x00);
            float y = ReadFloat(block, offset + 0x04);
            float z = ReadFloat(block, offset + 0x08);
            radius = ReadFloat(block, offset + 0x0C);

            unk0x10 = ReadInt(block, offset + 0x10);
            wrap = ReadInt(block, offset + 0x14);
            inactive = ReadInt(block, offset + 0x18);

            position = new Vector3(x, y, z);

            this.spline = spline;

            UpdateTransformMatrix();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public override byte[] ToByteArray()
        {
            byte[] block = new byte[ELEMENTSIZE];

            WriteFloat(block, 0x00, position.X);
            WriteFloat(block, 0x04, position.X);
            WriteFloat(block, 0x08, position.X);
            WriteFloat(block, 0x0C, position.X);

            WriteInt(block, 0x10, unk0x10);
            WriteInt(block, 0x14, wrap);
            WriteInt(block, 0x18, inactive);

            return block;
        }
    }
}
