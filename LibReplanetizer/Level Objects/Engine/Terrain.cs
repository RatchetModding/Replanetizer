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
using System.Linq;
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
        private float baseCullingSize;

        // 0x10 = pointer to TextureConfig
        // 0x14 = TextureConfig count
        // 0x18 = vertex offset, vertex count
        [Category("Unknowns"), DisplayName("OFF_1C: Always 65535")]
        public ushort off1C { get; set; }   // Always 0xffff
        [Category("Attributes"), DisplayName("Fragment ID")]
        public ushort fragmentId { get; set; }   // 0 in rac1, index in rac2/3

        [Category("Unknowns"), DisplayName("OFF_1C: Always 65280")]
        public ushort off20 { get; set; }   // Always 0xff00
        // 0x22 = which rgba, uv and index pointer to use (0 for the first, 1 for the second)
        [Category("Unknowns"), DisplayName("OFF_24: Always 0")]
        public uint off24 { get; set; }     // Always 0
        [Category("Unknowns"), DisplayName("OFF_28: Always 0")]
        public uint off28 { get; set; }     // Always 0
        [Category("Unknowns"), DisplayName("OFF_2C: Always 0")]
        public uint off2C { get; set; }     // Always 0

        public TerrainFragment(TerrainFragment referenceTfrag)
        {
            this.position = referenceTfrag.position;
            this.rotation = referenceTfrag.rotation;
            this.scale = referenceTfrag.scale;
            this.reflection = referenceTfrag.reflection;
            this.modelID = referenceTfrag.modelID;

            this.cullingCenter = referenceTfrag.cullingCenter;
            this.cullingSize = referenceTfrag.cullingSize;
            this.baseCullingSize = referenceTfrag.baseCullingSize;

            this.off1C = referenceTfrag.off1C;
            this.fragmentId = referenceTfrag.fragmentId;
            this.off20 = referenceTfrag.off20;
            this.off24 = referenceTfrag.off24;
            this.off28 = referenceTfrag.off28;
            this.off2C = referenceTfrag.off2C;

            this.model = referenceTfrag.model;

            UpdateTransformMatrix();
        }
        public TerrainFragment(TerrainModel terrainModel, Vector3 cullingCenter, float cullingSize, ushort fragmentId)
        {
            this.model = terrainModel;
            this.modelID = terrainModel.id;
            this.cullingCenter = cullingCenter;
            this.cullingSize = cullingSize;
            this.baseCullingSize = cullingSize;

            off1C = 0xFFFF;
            this.fragmentId = fragmentId;
            off20 = 0xFF00;
        }


        public TerrainFragment(FileStream fs, TerrainHead head, byte[] tfragBlock, int num)
        {
            int offset = num * 0x30;

            float cullingX = ReadFloat(tfragBlock, offset + 0x00);
            float cullingY = ReadFloat(tfragBlock, offset + 0x04);
            float cullingZ = ReadFloat(tfragBlock, offset + 0x08);
            cullingSize = ReadFloat(tfragBlock, offset + 0x0C);
            baseCullingSize = cullingSize;

            off1C = ReadUshort(tfragBlock, offset + 0x1C);
            fragmentId = ReadUshort(tfragBlock, offset + 0x1E);

            off20 = ReadUshort(tfragBlock, offset + 0x20);
            off24 = ReadUint(tfragBlock, offset + 0x24);
            off28 = ReadUint(tfragBlock, offset + 0x28);
            off2C = ReadUint(tfragBlock, offset + 0x2C);

            model = new TerrainModel(fs, head, tfragBlock, num);

            modelID = model.id;

            modelMatrix = Matrix4.Identity;

            cullingCenter = new Vector3(cullingX, cullingY, cullingZ);
        }

        public override LevelObject Clone()
        {
            return new TerrainFragment(this);
        }

        public override void SetFromMatrix(Matrix4 mat)
        {
            if (modelMatrix != mat)
            {
                Matrix4 delta = modelMatrix.Inverted() * mat;
                cullingCenter = (new Vector4(cullingCenter, 1.0f) * delta).Xyz;
            }
            base.SetFromMatrix(mat);
        }

        public override void UpdateTransformMatrix()
        {
            // same as the original method from LevelObject
            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);

            // So that it doesn't yeet to outer space when scaling
            Matrix4 toPivot = Matrix4.CreateTranslation(-cullingCenter);
            Matrix4 fromPivot = Matrix4.CreateTranslation(cullingCenter);
            cullingSize = baseCullingSize * (scale.X + scale.Y + scale.Z) / 3f;

            modelMatrix = toPivot * reflection * scaleMatrix * rot * fromPivot * translationMatrix;
        }
        public override Vector3 GetPosition()
        {
            return cullingCenter;
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
            WriteUshort(head, 0x1C, off1C);
            WriteUshort(head, 0x1E, fragmentId);

            WriteUshort(head, 0x20, off20);
            WriteUshort(head, 0x22, 0);
            WriteUint(head, 0x24, off24);
            WriteUint(head, 0x28, off28);
            WriteUint(head, 0x2C, off2C);

            return head;
        }

    }

}
