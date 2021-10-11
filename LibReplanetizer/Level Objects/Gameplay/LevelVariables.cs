﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.ComponentModel;
using System.Drawing;
using System.IO;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class LevelVariables
    {
        [Category("Attributes"), DisplayName("Background Color")]
        public Color backgroundColor { get; set; }

        [Category("Attributes"), DisplayName("Fog Color")]
        public Color fogColor { get; set; }

        [Category("Attributes"), DisplayName("Fog Near Distance")]
        public float fogNearDistance { get; set; }

        [Category("Attributes"), DisplayName("Fog Far Distance")]
        public float fogFarDistance { get; set; }

        [Category("Attributes"), DisplayName("Fog Near Intensity")]
        public float fogNearIntensity { get; set; }

        [Category("Attributes"), DisplayName("Fog Far Intensity")]
        public float fogFarIntensity { get; set; }

        [Category("Attributes"), DisplayName("Deathplane Z")]
        public float deathPlaneZ { get; set; }

        [Category("Attributes"), DisplayName("Is Spherical World?")]
        public int isSphericalWorld { get; set; }

        [Category("Attributes"), DisplayName("Sphere Center")]
        public Vector3 sphereCentre { get; set; }

        [Category("Attributes"), DisplayName("Ship Position")]
        public Vector3 shipPosition { get; set; }

        [Category("Attributes"), DisplayName("Ship Rotation")]
        public float shipRotation { get; set; }

        [Category("Unknown"), DisplayName("Unknown Color")]
        public Color unkColor { get; set; }

        [Category("Unknown"), DisplayName("OFF_48: Always 0")]
        public int off_48 { get; set; }

        [Category("Unknown"), DisplayName("OFF_4C: Always 0")]
        public int off_4c { get; set; }

        [Category("Unknown"), DisplayName("OFF_58: Always 0")]
        public int off_58 { get; set; }

        [Category("Unknown"), DisplayName("OFF_5C")]
        public int off_5C { get; set; }

        [Category("Unknown"), DisplayName("OFF_60")]
        public int off_60 { get; set; }

        [Category("Unknown"), DisplayName("OFF_64")]
        public float off_64 { get; set; }

        [Category("Unknown"), DisplayName("OFF_68")]
        public int off_68 { get; set; }

        [Category("Unknown"), DisplayName("OFF_6C")]
        public int off_6C { get; set; }

        [Category("Unknown"), DisplayName("OFF_70")]
        public int off_70 { get; set; }

        [Category("Unknown"), DisplayName("OFF_74")]
        public float off_74 { get; set; }

        [Category("Unknown"), DisplayName("OFF_78")]
        public int off_78 { get; set; }

        [Category("Unknown"), DisplayName("OFF_7C")]
        public int off_7C { get; set; }

        [Category("Unknown"), DisplayName("OFF_80")]
        public int off_80 { get; set; }

        [Category("Unknown"), DisplayName("UnknownBytes")]
        public byte[] unknownBytes { get; set; }

        public LevelVariables(GameType game, FileStream fileStream, int levelVarPointer, int length)
        {
            if (levelVarPointer == 0 || length == 0) return;

            byte[] levelVarBlock = ReadBlock(fileStream, levelVarPointer, length);

            switch (game.num)
            {
                case 1:
                    GetRC1Vals(levelVarBlock);
                    break;
                case 2:
                    GetRC2Vals(levelVarBlock);
                    break;
                case 3:
                    GetRC3Vals(levelVarBlock);
                    break;
                case 4:
                    GetDLVals(levelVarBlock);
                    break;
            }
        }

        private void GetRC1Vals(byte[] levelVarBlock)
        {
            int bgRed = ReadInt(levelVarBlock, 0x00);
            int bgGreen = ReadInt(levelVarBlock, 0x04);
            int bgBlue = ReadInt(levelVarBlock, 0x08);
            int r = ReadInt(levelVarBlock, 0x0c);

            int g = ReadInt(levelVarBlock, 0x10);
            int b = ReadInt(levelVarBlock, 0x14);
            fogNearDistance = ReadFloat(levelVarBlock, 0x18);
            fogFarDistance = ReadFloat(levelVarBlock, 0x1C);

            fogNearIntensity = ReadFloat(levelVarBlock, 0x20);
            fogFarIntensity = ReadFloat(levelVarBlock, 0x24);
            deathPlaneZ = ReadFloat(levelVarBlock, 0x28);
            float shipPositionX = ReadFloat(levelVarBlock, 0x2C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x30);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x34);
            shipRotation = ReadFloat(levelVarBlock, 0x38);
            int shipColorR = ReadInt(levelVarBlock, 0x3C);

            int shipColorG = ReadInt(levelVarBlock, 0x40);
            int shipColorB = ReadInt(levelVarBlock, 0x44);
            off_48 = ReadInt(levelVarBlock, 0x48);
            off_4c = ReadInt(levelVarBlock, 0x4C);

            unknownBytes = new byte[levelVarBlock.Length - 0x50];

            for (int i = 0; i < levelVarBlock.Length - 0x50; i++)
            {
                unknownBytes[i] = levelVarBlock[0x50 + i];
            }

            backgroundColor = Color.FromArgb(bgRed, bgGreen, bgBlue);
            fogColor = Color.FromArgb(r, g, b);
            if (shipColorR != -1)
            {
                unkColor = Color.FromArgb(shipColorR, shipColorG, shipColorB);
            }
            else
            {
                unkColor = Color.Empty;
            }

            sphereCentre = Vector3.Zero;
            shipPosition = new Vector3(shipPositionX, shipPositionY, shipPositionZ);
        }

        private void GetRC2Vals(byte[] levelVarBlock)
        {
            int bgRed = ReadInt(levelVarBlock, 0x00);
            int bgGreen = ReadInt(levelVarBlock, 0x04);
            int bgBlue = ReadInt(levelVarBlock, 0x08);
            int r = ReadInt(levelVarBlock, 0x0c);

            int g = ReadInt(levelVarBlock, 0x10);
            int b = ReadInt(levelVarBlock, 0x14);
            fogNearDistance = ReadFloat(levelVarBlock, 0x18);
            fogFarDistance = ReadFloat(levelVarBlock, 0x1C);

            fogNearIntensity = ReadFloat(levelVarBlock, 0x20);
            fogFarIntensity = ReadFloat(levelVarBlock, 0x24);
            deathPlaneZ = ReadFloat(levelVarBlock, 0x28);
            isSphericalWorld = ReadInt(levelVarBlock, 0x2C);

            float sphereCentreX = ReadFloat(levelVarBlock, 0x30);
            float sphereCentreY = ReadFloat(levelVarBlock, 0x34);
            float sphereCentreZ = ReadFloat(levelVarBlock, 0x38);
            float shipPositionX = ReadFloat(levelVarBlock, 0x3C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x40);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x44);
            shipRotation = ReadFloat(levelVarBlock, 0x48);
            int shipColorR = ReadInt(levelVarBlock, 0x4C);

            int shipColorG = ReadInt(levelVarBlock, 0x50);
            int shipColorB = ReadInt(levelVarBlock, 0x54);
            off_58 = ReadInt(levelVarBlock, 0x58);
            off_5C = ReadInt(levelVarBlock, 0x5C);

            off_60 = ReadInt(levelVarBlock, 0x60);
            off_64 = ReadFloat(levelVarBlock, 0x64);
            off_68 = ReadInt(levelVarBlock, 0x68);
            off_6C = ReadInt(levelVarBlock, 0x6C);

            off_70 = ReadInt(levelVarBlock, 0x70);
            off_74 = ReadFloat(levelVarBlock, 0x74);
            off_78 = ReadInt(levelVarBlock, 0x78);
            off_7C = ReadInt(levelVarBlock, 0x7C);

            unknownBytes = new byte[levelVarBlock.Length - 0x80];

            for (int i = 0; i < levelVarBlock.Length - 0x80; i++)
            {
                unknownBytes[i] = levelVarBlock[0x80 + i];
            }

            backgroundColor = Color.FromArgb(bgRed, bgGreen, bgBlue);
            fogColor = Color.FromArgb(r, g, b);
            if (shipColorR != -1)
            {
                unkColor = Color.FromArgb(shipColorR, shipColorG, shipColorB);
            }
            else
            {
                unkColor = Color.Empty;
            }


            sphereCentre = new Vector3(sphereCentreX, sphereCentreY, sphereCentreZ);
            shipPosition = new Vector3(shipPositionX, shipPositionY, shipPositionZ);
        }

        private void GetRC3Vals(byte[] levelVarBlock)
        {
            int bgRed = ReadInt(levelVarBlock, 0x00);
            int bgGreen = ReadInt(levelVarBlock, 0x04);
            int bgBlue = ReadInt(levelVarBlock, 0x08);
            int r = ReadInt(levelVarBlock, 0x0c);

            int g = ReadInt(levelVarBlock, 0x10);
            int b = ReadInt(levelVarBlock, 0x14);
            fogNearDistance = ReadFloat(levelVarBlock, 0x18);
            fogFarDistance = ReadFloat(levelVarBlock, 0x1C);

            fogNearIntensity = ReadFloat(levelVarBlock, 0x20);
            fogFarIntensity = ReadFloat(levelVarBlock, 0x24);
            deathPlaneZ = ReadFloat(levelVarBlock, 0x28);
            isSphericalWorld = ReadInt(levelVarBlock, 0x2C);

            float sphereCentreX = ReadFloat(levelVarBlock, 0x30);
            float sphereCentreY = ReadFloat(levelVarBlock, 0x34);
            float sphereCentreZ = ReadFloat(levelVarBlock, 0x38);
            float shipPositionX = ReadFloat(levelVarBlock, 0x3C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x40);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x44);
            shipRotation = ReadFloat(levelVarBlock, 0x48);
            int shipColorR = ReadInt(levelVarBlock, 0x4C);

            int shipColorG = ReadInt(levelVarBlock, 0x50);
            int shipColorB = ReadInt(levelVarBlock, 0x54);
            off_58 = ReadInt(levelVarBlock, 0x58);
            off_5C = ReadInt(levelVarBlock, 0x5C);

            off_60 = ReadInt(levelVarBlock, 0x60);
            off_64 = ReadFloat(levelVarBlock, 0x64);
            off_68 = ReadInt(levelVarBlock, 0x68);
            off_6C = ReadInt(levelVarBlock, 0x6C);

            off_70 = ReadInt(levelVarBlock, 0x70);
            off_74 = ReadFloat(levelVarBlock, 0x74);
            off_78 = ReadInt(levelVarBlock, 0x78);
            off_7C = ReadInt(levelVarBlock, 0x7C);

            off_80 = ReadInt(levelVarBlock, 0x80);

            unknownBytes = new byte[levelVarBlock.Length - 0x84];

            for (int i = 0; i < levelVarBlock.Length - 0x84; i++)
            {
                unknownBytes[i] = levelVarBlock[0x84 + i];
            }

            backgroundColor = Color.FromArgb(bgRed, bgGreen, bgBlue);
            fogColor = Color.FromArgb(r, g, b);
            if (shipColorR != -1)
            {
                unkColor = Color.FromArgb(shipColorR, shipColorG, shipColorB);
            }
            else
            {
                unkColor = Color.Empty;
            }


            sphereCentre = new Vector3(sphereCentreX, sphereCentreY, sphereCentreZ);
            shipPosition = new Vector3(shipPositionX, shipPositionY, shipPositionZ);
        }

        private void GetDLVals(byte[] levelVarBlock)
        {
            int bgRed = ReadInt(levelVarBlock, 0x00);
            int bgGreen = ReadInt(levelVarBlock, 0x04);
            int bgBlue = ReadInt(levelVarBlock, 0x08);
            int r = ReadInt(levelVarBlock, 0x0c);

            int g = ReadInt(levelVarBlock, 0x10);
            int b = ReadInt(levelVarBlock, 0x14);
            fogNearDistance = ReadFloat(levelVarBlock, 0x18);
            fogFarDistance = ReadFloat(levelVarBlock, 0x1C);

            fogNearIntensity = ReadFloat(levelVarBlock, 0x20);
            fogFarIntensity = ReadFloat(levelVarBlock, 0x24);
            deathPlaneZ = ReadFloat(levelVarBlock, 0x28);
            isSphericalWorld = ReadInt(levelVarBlock, 0x2C);

            float sphereCentreX = ReadFloat(levelVarBlock, 0x30);
            float sphereCentreY = ReadFloat(levelVarBlock, 0x34);
            float sphereCentreZ = ReadFloat(levelVarBlock, 0x38);
            float shipPositionX = ReadFloat(levelVarBlock, 0x3C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x40);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x44);
            shipRotation = ReadFloat(levelVarBlock, 0x48);
            int shipColorR = ReadInt(levelVarBlock, 0x4C);

            int shipColorG = ReadInt(levelVarBlock, 0x50);
            int shipColorB = ReadInt(levelVarBlock, 0x54);
            off_58 = ReadInt(levelVarBlock, 0x58);
            off_5C = ReadInt(levelVarBlock, 0x5C);

            off_60 = ReadInt(levelVarBlock, 0x60);
            off_64 = ReadFloat(levelVarBlock, 0x64);
            off_68 = ReadInt(levelVarBlock, 0x68);
            off_6C = ReadInt(levelVarBlock, 0x6C);

            off_70 = ReadInt(levelVarBlock, 0x70);
            off_74 = ReadFloat(levelVarBlock, 0x74);
            off_78 = ReadInt(levelVarBlock, 0x78);
            off_7C = ReadInt(levelVarBlock, 0x7C);

            off_80 = ReadInt(levelVarBlock, 0x80);

            unknownBytes = new byte[levelVarBlock.Length - 0x84];

            for (int i = 0; i < levelVarBlock.Length - 0x84; i++)
            {
                unknownBytes[i] = levelVarBlock[0x84 + i];
            }

            backgroundColor = Color.FromArgb(bgRed, bgGreen, bgBlue);
            fogColor = Color.FromArgb(r, g, b);
            if (shipColorR != -1)
            {
                unkColor = Color.FromArgb(shipColorR, shipColorG, shipColorB);
            }
            else
            {
                unkColor = Color.Empty;
            }


            sphereCentre = new Vector3(sphereCentreX, sphereCentreY, sphereCentreZ);
            shipPosition = new Vector3(shipPositionX, shipPositionY, shipPositionZ);
        }

        public byte[] Serialize(GameType game)
        {
            switch (game.num)
            {
                case 1:
                    return SerializeRC1();
                case 2:
                    return SerializeRC2();
                case 3:
                    return SerializeRC3();
                case 4:
                    return SerializeDL();
                default:
                    return SerializeRC3();
            }
        }

        private byte[] SerializeRC1()
        {
            byte[] bytes = new byte[0x50 + unknownBytes.Length];

            WriteUint(bytes, 0x00, backgroundColor.R);
            WriteUint(bytes, 0x04, backgroundColor.G);
            WriteUint(bytes, 0x08, backgroundColor.B);
            WriteUint(bytes, 0x0C, fogColor.R);

            WriteUint(bytes, 0x10, fogColor.G);
            WriteUint(bytes, 0x14, fogColor.B);
            WriteFloat(bytes, 0x18, fogNearDistance);
            WriteFloat(bytes, 0x1C, fogFarDistance);

            WriteFloat(bytes, 0x20, fogNearIntensity);
            WriteFloat(bytes, 0x24, fogFarIntensity);
            WriteFloat(bytes, 0x28, deathPlaneZ);
            WriteFloat(bytes, 0x2C, shipPosition.X);

            WriteFloat(bytes, 0x30, shipPosition.Y);
            WriteFloat(bytes, 0x34, shipPosition.Z);
            WriteFloat(bytes, 0x38, shipRotation);

            if (unkColor.IsEmpty)
            {
                WriteInt(bytes, 0x3C, -1);
                WriteInt(bytes, 0x40, 0);
                WriteInt(bytes, 0x44, 0);
            }
            else
            {
                WriteInt(bytes, 0x3C, unkColor.R);
                WriteInt(bytes, 0x40, unkColor.G);
                WriteInt(bytes, 0x44, unkColor.B);
            }

            WriteInt(bytes, 0x48, off_48);
            WriteInt(bytes, 0x4C, off_4c);

            unknownBytes.CopyTo(bytes, 0x50);

            return bytes;
        }

        private byte[] SerializeRC2()
        {
            byte[] bytes = new byte[0x80 + unknownBytes.Length];

            WriteUint(bytes, 0x00, backgroundColor.R);
            WriteUint(bytes, 0x04, backgroundColor.G);
            WriteUint(bytes, 0x08, backgroundColor.B);
            WriteUint(bytes, 0x0C, fogColor.R);

            WriteUint(bytes, 0x10, fogColor.G);
            WriteUint(bytes, 0x14, fogColor.B);
            WriteFloat(bytes, 0x18, fogNearDistance);
            WriteFloat(bytes, 0x1C, fogFarDistance);

            WriteFloat(bytes, 0x20, fogNearIntensity);
            WriteFloat(bytes, 0x24, fogFarIntensity);
            WriteFloat(bytes, 0x28, deathPlaneZ);
            WriteInt(bytes, 0x2C, isSphericalWorld);

            WriteFloat(bytes, 0x30, sphereCentre.X);
            WriteFloat(bytes, 0x34, sphereCentre.Y);
            WriteFloat(bytes, 0x38, sphereCentre.Z);
            WriteFloat(bytes, 0x3C, shipPosition.X);

            WriteFloat(bytes, 0x40, shipPosition.Y);
            WriteFloat(bytes, 0x44, shipPosition.Z);
            WriteFloat(bytes, 0x48, shipRotation);

            if (unkColor.IsEmpty)
            {
                WriteInt(bytes, 0x4C, -1);
                WriteInt(bytes, 0x50, 0);
                WriteInt(bytes, 0x54, 0);
            }
            else
            {
                WriteInt(bytes, 0x4C, unkColor.R);
                WriteInt(bytes, 0x50, unkColor.G);
                WriteInt(bytes, 0x54, unkColor.B);
            }

            WriteInt(bytes, 0x58, off_58);
            WriteInt(bytes, 0x5C, off_5C);

            WriteInt(bytes, 0x60, off_60);
            WriteFloat(bytes, 0x64, off_64);
            WriteInt(bytes, 0x68, off_68);
            WriteInt(bytes, 0x6C, off_6C);

            WriteInt(bytes, 0x70, off_70);
            WriteFloat(bytes, 0x74, off_74);
            WriteInt(bytes, 0x78, off_78);
            WriteInt(bytes, 0x7C, off_7C);

            unknownBytes.CopyTo(bytes, 0x80);

            return bytes;
        }

        private byte[] SerializeRC3()
        {
            byte[] bytes = new byte[0x84 + unknownBytes.Length];

            WriteUint(bytes, 0x00, backgroundColor.R);
            WriteUint(bytes, 0x04, backgroundColor.G);
            WriteUint(bytes, 0x08, backgroundColor.B);
            WriteUint(bytes, 0x0C, fogColor.R);

            WriteUint(bytes, 0x10, fogColor.G);
            WriteUint(bytes, 0x14, fogColor.B);
            WriteFloat(bytes, 0x18, fogNearDistance);
            WriteFloat(bytes, 0x1C, fogFarDistance);

            WriteFloat(bytes, 0x20, fogNearIntensity);
            WriteFloat(bytes, 0x24, fogFarIntensity);
            WriteFloat(bytes, 0x28, deathPlaneZ);
            WriteInt(bytes, 0x2C, isSphericalWorld);

            WriteFloat(bytes, 0x30, sphereCentre.X);
            WriteFloat(bytes, 0x34, sphereCentre.Y);
            WriteFloat(bytes, 0x38, sphereCentre.Z);
            WriteFloat(bytes, 0x3C, shipPosition.X);

            WriteFloat(bytes, 0x40, shipPosition.Y);
            WriteFloat(bytes, 0x44, shipPosition.Z);
            WriteFloat(bytes, 0x48, shipRotation);

            if (unkColor.IsEmpty)
            {
                WriteInt(bytes, 0x4C, -1);
                WriteInt(bytes, 0x50, 0);
                WriteInt(bytes, 0x54, 0);
            }
            else
            {
                WriteInt(bytes, 0x4C, unkColor.R);
                WriteInt(bytes, 0x50, unkColor.G);
                WriteInt(bytes, 0x54, unkColor.B);
            }

            WriteInt(bytes, 0x58, off_58);
            WriteInt(bytes, 0x5C, off_5C);

            WriteInt(bytes, 0x60, off_60);
            WriteFloat(bytes, 0x64, off_64);
            WriteInt(bytes, 0x68, off_68);
            WriteInt(bytes, 0x6C, off_6C);

            WriteInt(bytes, 0x70, off_70);
            WriteFloat(bytes, 0x74, off_74);
            WriteInt(bytes, 0x78, off_78);
            WriteInt(bytes, 0x7C, off_7C);

            WriteInt(bytes, 0x80, off_80);

            unknownBytes.CopyTo(bytes, 0x84);

            return bytes;
        }

        private byte[] SerializeDL()
        {
            byte[] bytes = new byte[0x84 + unknownBytes.Length];

            WriteUint(bytes, 0x00, backgroundColor.R);
            WriteUint(bytes, 0x04, backgroundColor.G);
            WriteUint(bytes, 0x08, backgroundColor.B);
            WriteUint(bytes, 0x0C, fogColor.R);

            WriteUint(bytes, 0x10, fogColor.G);
            WriteUint(bytes, 0x14, fogColor.B);
            WriteFloat(bytes, 0x18, fogNearDistance);
            WriteFloat(bytes, 0x1C, fogFarDistance);

            WriteFloat(bytes, 0x20, fogNearIntensity);
            WriteFloat(bytes, 0x24, fogFarIntensity);
            WriteFloat(bytes, 0x28, deathPlaneZ);
            WriteInt(bytes, 0x2C, isSphericalWorld);

            WriteFloat(bytes, 0x30, sphereCentre.X);
            WriteFloat(bytes, 0x34, sphereCentre.Y);
            WriteFloat(bytes, 0x38, sphereCentre.Z);
            WriteFloat(bytes, 0x3C, shipPosition.X);

            WriteFloat(bytes, 0x40, shipPosition.Y);
            WriteFloat(bytes, 0x44, shipPosition.Z);
            WriteFloat(bytes, 0x48, shipRotation);

            if (unkColor.IsEmpty)
            {
                WriteInt(bytes, 0x4C, -1);
                WriteInt(bytes, 0x50, 0);
                WriteInt(bytes, 0x54, 0);
            }
            else
            {
                WriteInt(bytes, 0x4C, unkColor.R);
                WriteInt(bytes, 0x50, unkColor.G);
                WriteInt(bytes, 0x54, unkColor.B);
            }

            WriteInt(bytes, 0x58, off_58);
            WriteInt(bytes, 0x5C, off_5C);

            WriteInt(bytes, 0x60, off_60);
            WriteFloat(bytes, 0x64, off_64);
            WriteInt(bytes, 0x68, off_68);
            WriteInt(bytes, 0x6C, off_6C);

            WriteInt(bytes, 0x70, off_70);
            WriteFloat(bytes, 0x74, off_74);
            WriteInt(bytes, 0x78, off_78);
            WriteInt(bytes, 0x7C, off_7C);

            WriteInt(bytes, 0x80, off_80);

            unknownBytes.CopyTo(bytes, 0x84);

            return bytes;
        }
    }
}
