// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Terrain
    {
        public ushort levelNumber;
        public List<TerrainFragment> fragments;

        public Terrain(List<TerrainFragment> fragments, ushort levelNumber)
        {
            this.fragments = fragments;
            this.levelNumber = levelNumber;
        }
    }

    public class TerrainFragment : ModelObject
    {

        [Category("Attributes"), DisplayName("Culling Center")]
        public Vector3 cullingCenter { get; set; }
        [Category("Attributes"), DisplayName("Culling Radius")]
        public float cullingSize { get; set; }

        // 0x10 = pointer to TextureConfig
        // 0x14 = TextureConfig count
        // 0x18 = vertex offset, vertex count
        [Category("Unknowns"), DisplayName("OFF_1C: Always 65535")]
        public ushort off_1C { get; set; }   // Always 0xffff
        [Category("Unknowns"), DisplayName("OFF_1E: Fragment ID")]
        public ushort off_1E { get; set; }   // 0 in rac1, index in rac2/3

        [Category("Unknowns"), DisplayName("OFF_1C: Always 65280")]
        public ushort off_20 { get; set; }   // Always 0xff00
        // 0x22 = which rgba, uv and index pointer to use (0 for the first, 1 for the second)
        [Category("Unknowns"), DisplayName("OFF_24: Always 0")]
        public uint off_24 { get; set; }     // Always 0
        [Category("Unknowns"), DisplayName("OFF_28: Always 0")]
        public uint off_28 { get; set; }     // Always 0
        [Category("Unknowns"), DisplayName("OFF_2C: Always 0")]
        public uint off_2C { get; set; }     // Always 0


        public TerrainFragment(FileStream fs, TerrainHead head, byte[] tfragBlock, int num)
        {
            int offset = num * 0x30;

            float cullingX = ReadFloat(tfragBlock, offset + 0x00);
            float cullingY = ReadFloat(tfragBlock, offset + 0x04);
            float cullingZ = ReadFloat(tfragBlock, offset + 0x08);
            cullingSize = ReadFloat(tfragBlock, offset + 0x0C);

            off_1C = ReadUshort(tfragBlock, offset + 0x1C);
            off_1E = ReadUshort(tfragBlock, offset + 0x1E);

            off_20 = ReadUshort(tfragBlock, offset + 0x20);
            off_24 = ReadUint(tfragBlock, offset + 0x24);
            off_28 = ReadUint(tfragBlock, offset + 0x28);
            off_2C = ReadUint(tfragBlock, offset + 0x2C);

            model = new TerrainModel(fs, head, tfragBlock, num);

            modelID = model.id;

            modelMatrix = Matrix4.Identity;

            cullingCenter = new Vector3(cullingX, cullingY, cullingZ);
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        // Some variables are not written since they have to be dynamically determined based on the underlying data
        public override byte[] ToByteArray()
        {
            byte[] head = new byte[0x30];

            WriteFloat(head, 0x00, cullingCenter.X);
            WriteFloat(head, 0x04, cullingCenter.Y);
            WriteFloat(head, 0x08, cullingCenter.Z);
            WriteFloat(head, 0x0C, cullingSize);

            WriteInt(head, 0x10, 0);
            WriteInt(head, 0x14, 0);
            WriteInt(head, 0x18, 0);
            WriteUshort(head, 0x1C, off_1C);
            WriteUshort(head, 0x1E, off_1E);

            WriteUshort(head, 0x20, off_20);
            WriteUshort(head, 0x22, 0);
            WriteUint(head, 0x24, off_24);
            WriteUint(head, 0x28, off_28);
            WriteUint(head, 0x2C, off_2C);

            return head;
        }

    }

}
