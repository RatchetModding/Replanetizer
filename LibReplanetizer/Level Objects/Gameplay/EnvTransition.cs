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
    public class EnvTransition : MatrixObject
    {
        public const int ELEMENTSIZE = 0x80;
        public const int HEADSIZE = 0x10;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }
        [Category("Attributes"), DisplayName("Inverse Matrix")]
        public Matrix4 inverseMatrix { get; set; }
        [Category("Attributes"), DisplayName("Ratchet Ambient Color 1")]
        public Color heroColor1 { get; set; }
        [Category("Attributes"), DisplayName("Ratchet Ambient Color 2")]
        public Color heroColor2 { get; set; }
        [Category("Attributes"), DisplayName("Ratchet Light ID 1")]
        public int heroLight1 { get; set; }
        [Category("Attributes"), DisplayName("Ratchet Light ID 2")]
        public int heroLight2 { get; set; }
        [Category("Attributes"), DisplayName("Flags")]
        public int flags { get; set; }
        [Category("Attributes"), DisplayName("Fog Color 1")]
        public Color fogColor1 { get; set; }
        [Category("Attributes"), DisplayName("Fog Color 2")]
        public Color fogColor2 { get; set; }
        [Category("Attributes"), DisplayName("Fog Near Distance 1")]
        public float fogNearDist1 { get; set; }
        [Category("Attributes"), DisplayName("Fog Far Distance 1")]
        public float fogFarDist1 { get; set; }
        [Category("Attributes"), DisplayName("Fog Near Intensity 1")]
        public float fogNearIntensity1 { get; set; }
        [Category("Attributes"), DisplayName("Fog Far Intensity 1")]
        public float fogFarIntensity1 { get; set; }
        [Category("Attributes"), DisplayName("Fog Near Distance 2")]
        public float fogNearDist2 { get; set; }
        [Category("Attributes"), DisplayName("Fog Far Distance 2")]
        public float fogFarDist2 { get; set; }
        [Category("Attributes"), DisplayName("Fog Near Intensity 2")]
        public float fogNearIntensity2 { get; set; }
        [Category("Attributes"), DisplayName("Fog Far Intensity 2")]
        public float fogFarIntensity2 { get; set; }

        // Probably related to how a transition is triggered
        [Category("Unknowns"), DisplayName("Position")]
        public Vector4 unkPos { get; set; }

        public EnvTransition(byte[] headBlock, byte[] mainBlock, int num)
        {
            id = num;
            int offsetHead = num * HEADSIZE;

            float x = ReadFloat(headBlock, offsetHead + 0x00);
            float y = ReadFloat(headBlock, offsetHead + 0x04);
            float z = ReadFloat(headBlock, offsetHead + 0x08);
            float w = ReadFloat(headBlock, offsetHead + 0x0C);

            unkPos = new Vector4(x, y, z, w);

            int offset = num * ELEMENTSIZE;

            inverseMatrix = ReadMatrix4(mainBlock, offset + 0x00);

            byte heroR1 = mainBlock[offset + 0x40];
            byte heroG1 = mainBlock[offset + 0x41];
            byte heroB1 = mainBlock[offset + 0x42];
            byte heroR2 = mainBlock[offset + 0x44];
            byte heroG2 = mainBlock[offset + 0x45];
            byte heroB2 = mainBlock[offset + 0x46];
            heroLight1 = ReadInt(mainBlock, offset + 0x48);
            heroLight2 = ReadInt(mainBlock, offset + 0x4C);

            flags = ReadInt(mainBlock, offset + 0x50);
            byte fogR1 = mainBlock[offset + 0x54];
            byte fogG1 = mainBlock[offset + 0x55];
            byte fogB1 = mainBlock[offset + 0x56];
            byte fogR2 = mainBlock[offset + 0x58];
            byte fogG2 = mainBlock[offset + 0x59];
            byte fogB2 = mainBlock[offset + 0x5A];
            fogNearDist1 = ReadFloat(mainBlock, offset + 0x5C);

            fogNearIntensity1 = ReadFloat(mainBlock, offset + 0x60);
            fogFarDist1 = ReadFloat(mainBlock, offset + 0x64);
            fogFarIntensity1 = ReadFloat(mainBlock, offset + 0x68);
            fogNearDist2 = ReadFloat(mainBlock, offset + 0x6C);

            fogNearIntensity2 = ReadFloat(mainBlock, offset + 0x70);
            fogFarDist2 = ReadFloat(mainBlock, offset + 0x74);
            fogFarIntensity2 = ReadFloat(mainBlock, offset + 0x78);

            heroColor1 = Color.FromArgb(heroR1, heroG1, heroB1);
            heroColor2 = Color.FromArgb(heroR2, heroG2, heroB2);
            fogColor1 = Color.FromArgb(fogR1, fogG1, fogB1);
            fogColor2 = Color.FromArgb(fogR2, fogG2, fogB2);

            UpdateTransformMatrix();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArrayHead()
        {
            byte[] block = new byte[HEADSIZE];

            WriteFloat(block, 0x00, unkPos.X);
            WriteFloat(block, 0x04, unkPos.Y);
            WriteFloat(block, 0x08, unkPos.Z);
            WriteFloat(block, 0x0C, unkPos.W);

            return block;
        }

        public byte[] ToByteArrayMain()
        {
            byte[] block = new byte[ELEMENTSIZE];

            WriteMatrix4(block, 0x00, inverseMatrix);

            block[0x40] = (byte) heroColor1.R;
            block[0x41] = (byte) heroColor1.G;
            block[0x42] = (byte) heroColor1.B;
            block[0x43] = (byte) 0;
            block[0x44] = (byte) heroColor2.R;
            block[0x45] = (byte) heroColor2.G;
            block[0x46] = (byte) heroColor2.B;
            block[0x47] = (byte) 0;
            WriteInt(block, 0x48, heroLight1);
            WriteInt(block, 0x4C, heroLight2);

            WriteInt(block, 0x50, flags);
            block[0x54] = (byte) fogColor1.R;
            block[0x55] = (byte) fogColor1.G;
            block[0x56] = (byte) fogColor1.B;
            block[0x57] = (byte) 0;
            block[0x58] = (byte) fogColor2.R;
            block[0x59] = (byte) fogColor2.G;
            block[0x5A] = (byte) fogColor2.B;
            block[0x5B] = (byte) 0;
            WriteFloat(block, 0x5C, fogNearDist1);

            WriteFloat(block, 0x6C, fogNearIntensity1);
            WriteFloat(block, 0x6C, fogFarDist1);
            WriteFloat(block, 0x6C, fogFarIntensity1);
            WriteFloat(block, 0x6C, fogNearDist2);

            WriteFloat(block, 0x7C, fogNearIntensity2);
            WriteFloat(block, 0x7C, fogFarDist2);
            WriteFloat(block, 0x7C, fogFarIntensity2);

            return block;
        }

        public override byte[] ToByteArray()
        {
            // The data structure of EnvTransitions does not fit this design
            throw new NotImplementedException();
        }
    }
}
