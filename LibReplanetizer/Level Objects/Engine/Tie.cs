// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using static LibReplanetizer.DataFunctions;


namespace LibReplanetizer.LevelObjects
{
    public class Tie : ModelObject
    {
        const int ELEMENTSIZE = 0x70;

        [Category("Unknowns"), DisplayName("OFF_54: Always 4000 in RaC 2/3")]
        public uint off54 { get; set; }
        [Category("Unknowns"), DisplayName("OFF_58: Tie ID")]
        public uint off58 { get; set; }
        [Category("Unknowns"), DisplayName("OFF_5C: Always 0")]
        public uint off5C { get; set; }

        [Category("Unknowns"), DisplayName("OFF_64: Always 0")]
        public uint off64 { get; set; }
        [Category("Attributes"), DisplayName("Light")]
        public ushort light { get; set; }
        [Category("Unknowns"), DisplayName("OFF_6C: Always 0")]
        public uint off6C { get; set; }

        public byte[] colorBytes;

        public Tie(Tie referenceTie)
        {
            this.position = referenceTie.position;
            this.rotation = referenceTie.rotation;
            this.scale = referenceTie.scale;
            this.reflection = referenceTie.reflection;
            this.modelID = referenceTie.modelID;
            this.off54 = referenceTie.off54;
            this.off58 = referenceTie.off58;
            this.off5C = referenceTie.off5C;
            this.off64 = referenceTie.off64;
            this.light = referenceTie.light;
            this.off6C = referenceTie.off6C;
            this.model = referenceTie.model;
            this.colorBytes = (byte[]) referenceTie.colorBytes.Clone();

            UpdateTransformMatrix();
        }

        public Tie(byte[] levelBlock, int num, List<Model> tieModels, FileStream fs)
        {
            int offset = num * ELEMENTSIZE;

            modelMatrix = ReadMatrix4(levelBlock, offset + 0x00);

            /* These offsets are just placeholders for the render distance quaternion which is set in-game
            off_40 =    BAToUInt32(levelBlock, offset + 0x40);
            off_44 =    BAToUInt32(levelBlock, offset + 0x44);
            off_48 =    BAToUInt32(levelBlock, offset + 0x48);
            off_4C =    BAToUInt32(levelBlock, offset + 0x4C);
            */

            modelID = ReadInt(levelBlock, offset + 0x50);
            off54 = ReadUint(levelBlock, offset + 0x54);
            off58 = ReadUint(levelBlock, offset + 0x58);
            off5C = ReadUint(levelBlock, offset + 0x5C);

            int colorOffset = ReadInt(levelBlock, offset + 0x60);
            off64 = ReadUint(levelBlock, offset + 0x64);
            light = ReadUshort(levelBlock, offset + 0x68);
            off6C = ReadUint(levelBlock, offset + 0x6C);

            model = tieModels.Find(tieModel => tieModel.id == modelID);

            if (model == null)
            {
                colorBytes = new byte[0];
            }
            else
            {
                colorBytes = ReadBlock(fs, colorOffset, (model.vertexBuffer.Length / 8) * 4);
            }

            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();

            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 attributes = scaleMatrix * rot * translationMatrix;
            try
            {
                attributes.Invert();
            }
            catch
            {
                attributes = Matrix4.Identity;
            }

            reflection = modelMatrix * attributes;
        }

        public byte[] ToByteArray(int colorOffset)
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, modelMatrix);

            WriteInt(bytes, 0x50, modelID);
            WriteUint(bytes, 0x54, off54);
            WriteUint(bytes, 0x58, off58);
            WriteUint(bytes, 0x5C, off5C);

            WriteInt(bytes, 0x60, colorOffset);
            WriteUint(bytes, 0x64, off64);
            WriteUshort(bytes, 0x68, light);
            WriteUshort(bytes, 0x6A, 0xffff);
            WriteUint(bytes, 0x6C, off6C);

            return bytes;
        }

        // this may cause issues since the colorOffset is not given
        public override byte[] ToByteArray()
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, modelMatrix);

            WriteInt(bytes, 0x50, modelID);
            WriteUint(bytes, 0x54, off54);
            WriteUint(bytes, 0x58, off58);
            WriteUint(bytes, 0x5C, off5C);

            WriteInt(bytes, 0x60, 0);
            WriteUint(bytes, 0x64, off64);
            WriteUshort(bytes, 0x68, light);
            WriteUshort(bytes, 0x6A, 0xffff);
            WriteUint(bytes, 0x6C, off6C);

            return bytes;
        }

        public override LevelObject Clone()
        {
            return new Tie(this);
        }
    }
}
