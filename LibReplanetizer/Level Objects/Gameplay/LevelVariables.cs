// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.ComponentModel;
using System.IO;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class LevelVariables
    {
        private uint byteSize = 0x0;

        [Category("Attributes"), DisplayName("Background Color")]
        public Rgb24 backgroundColor { get; set; }

        [Category("Attributes"), DisplayName("Fog Color")]
        public Rgb24 fogColor { get; set; }

        [Category("Attributes"), DisplayName("Fog Near Distance"), Description("Distance at which fog has its near intensity. The actual distance is given by [FogNearDistance]/1024.")]
        public float fogNearDistance { get; set; }

        [Category("Attributes"), DisplayName("Fog Far Distance"), Description("Distance at which fog has its far intensity. The actual distance is given by 1024/([FogFarDistance]-[FogNearDistance]). ")]
        public float fogFarDistance { get; set; }

        [Category("Attributes"), DisplayName("Fog Near Intensity"), Description("Intensity of the fog at its near distance. The actual intensity is given by 1-[FogNearIntensity]/255.")]
        public float fogNearIntensity { get; set; }

        [Category("Attributes"), DisplayName("Fog Far Intensity"), Description("Intensity of the fog at its far distance. The actual intensity is given by 1-[FogFarIntensity]/255.")]
        public float fogFarIntensity { get; set; }

        [Category("Attributes"), DisplayName("Deathplane Z")]
        public float deathPlaneZ { get; set; }

        private int _isSphericalWorld { get; set; }
        [Category("Attributes"), DisplayName("Is Spherical World?")]
        public bool isSphericalWorld
        {
            get { return _isSphericalWorld > 0; }
            set
            {
                _isSphericalWorld = (value) ? 1 : 0;
            }
        }

        [Category("Attributes"), DisplayName("Sphere Center")]
        public Vector3 sphereCentre { get; set; }

        [Category("Attributes"), DisplayName("Ship Position")]
        public Vector3 shipPosition { get; set; }

        [Category("Attributes"), DisplayName("Ship Rotation")]
        public float shipRotation { get; set; }
        [Category("Attributes"), DisplayName("Chunk 1 Plane")]
        public Vector3 chunk1Plane { get; set; }
        [Category("Attributes"), DisplayName("Chunk 2 Plane")]
        public Vector3 chunk2Plane { get; set; }
        [Category("Attributes"), DisplayName("Chunk 1 Plane Normal")]
        public Vector3 chunk1PlaneNormal { get; set; }
        [Category("Attributes"), DisplayName("Chunk 2 Plane Normal")]
        public Vector3 chunk2PlaneNormal { get; set; }

        // These seem to be 3 pointers
        // First pointer points to path that ship takes when leaving
        // Second pointer points to start of camera path when leaving
        // Third pointer points to end of camera path when leaving
        //
        // The pointers point to level local memory as the same pointer in
        // different levels yields different results.
        //
        // The first pointer cannot be reused for the other two, that makes sense,
        // I guess though that it may reference the same part in memory but it
        // interprets it differently (what do I know).
        // The second and third pointer can be interchanged.
        // In general it seems like it simply interpolates between the start and
        // end camera matrix found at the second and third pointer.
        // These matrices seem to be used in other cutscenes/camera paths as well,
        // i.e., changing the value here can result in giving you the camera path
        // of some cutscene.
        // Also note that while I say pointer I mean more like ID or offset or whatever.
        [Category("Attributes"), DisplayName("Ship Path Index")]
        public int shipPathID { get; set; }
        [Category("Attributes"), DisplayName("Ship Camera Start Index")]
        public int shipCameraStartID { get; set; }
        [Category("Attributes"), DisplayName("Ship Camera End Index")]
        public int shipCameraEndID { get; set; }
        [Category("Attributes"), DisplayName("Chunk Count")]
        public int chunkCount { get; set; }
        [Category("Unknown"), DisplayName("Chunk 2 Number")]
        public int unknownChunk2 { get; set; }

        [Category("Unknown"), DisplayName("OFF_48: Always 0")]
        public int off48 { get; set; }

        [Category("Unknown"), DisplayName("OFF_4C: Always 0")]
        public int off4C { get; set; }

        [Category("Unknown"), DisplayName("OFF_58: Always 0")]
        public int off58 { get; set; }

        [Category("Unknown"), DisplayName("OFF_5C")]
        public int off5C { get; set; }

        [Category("Unknown"), DisplayName("OFF_60")]
        public int off60 { get; set; }

        [Category("Unknown"), DisplayName("OFF_64")]
        public float off64 { get; set; }

        [Category("Unknown"), DisplayName("OFF_68")]
        public int off68 { get; set; }

        [Category("Unknown"), DisplayName("OFF_6C")]
        public int off6C { get; set; }

        [Category("Unknown"), DisplayName("OFF_70")]
        public int off70 { get; set; }

        [Category("Unknown"), DisplayName("OFF_74")]
        public float off74 { get; set; }

        [Category("Unknown"), DisplayName("OFF_78")]
        public int off78 { get; set; }

        /*
         * RaC 2/3:
         * OFF_7C is often non zero and probably corresponds to OFF_9C in case there are 2 chunks
         */
        [Category("Unknown"), DisplayName("OFF_7C")]
        public int off7C { get; set; }

        [Category("Unknown"), DisplayName("OFF_80")]
        public int off80 { get; set; }
        [Category("Unknown"), DisplayName("OFF_98")]
        public int off98 { get; set; }
        [Category("Unknown"), DisplayName("OFF_9C")]
        public int off9C { get; set; }
        [Category("Unknown"), DisplayName("OFF_100")]
        public int off100 { get; set; }

        [Category("Unknown"), DisplayName("UnknownBytes")]
        public byte[] unknownBytes { get; set; } = new byte[0];

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
            shipPathID = ReadInt(levelVarBlock, 0x3C);

            shipCameraStartID = ReadInt(levelVarBlock, 0x40);
            shipCameraEndID = ReadInt(levelVarBlock, 0x44);
            off48 = ReadInt(levelVarBlock, 0x48);
            off4C = ReadInt(levelVarBlock, 0x4C);

            backgroundColor = Color.FromRgb((byte) bgRed, (byte) bgGreen, (byte) bgBlue).ToPixel<Rgb24>();
            fogColor = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
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
            _isSphericalWorld = ReadInt(levelVarBlock, 0x2C);

            float sphereCentreX = ReadFloat(levelVarBlock, 0x30);
            float sphereCentreY = ReadFloat(levelVarBlock, 0x34);
            float sphereCentreZ = ReadFloat(levelVarBlock, 0x38);
            float shipPositionX = ReadFloat(levelVarBlock, 0x3C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x40);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x44);
            shipRotation = ReadFloat(levelVarBlock, 0x48);
            shipPathID = ReadInt(levelVarBlock, 0x4C);

            shipCameraStartID = ReadInt(levelVarBlock, 0x50);
            shipCameraEndID = ReadInt(levelVarBlock, 0x54);
            off58 = ReadInt(levelVarBlock, 0x58);
            float chunk1PlaneX = ReadFloat(levelVarBlock, 0x5C);

            float chunk1PlaneY = ReadFloat(levelVarBlock, 0x60);
            float chunk1PlaneZ = ReadFloat(levelVarBlock, 0x64);
            chunkCount = ReadInt(levelVarBlock, 0x68);
            float chunk1PlaneNormalX = ReadFloat(levelVarBlock, 0x6C);

            float chunk1PlaneNormalY = ReadFloat(levelVarBlock, 0x70);
            float chunk1PlaneNormalZ = ReadFloat(levelVarBlock, 0x74);
            off78 = ReadInt(levelVarBlock, 0x78);

            if (chunkCount > 1)
            {
                byteSize = 0x100;

                float chunk2LimitX = ReadFloat(levelVarBlock, 0x7C);

                float chunk2LimitY = ReadFloat(levelVarBlock, 0x80);
                float chunk2LimitZ = ReadFloat(levelVarBlock, 0x84);
                unknownChunk2 = ReadInt(levelVarBlock, 0x88);
                float chunk2LimitOrientX = ReadFloat(levelVarBlock, 0x8C);

                float chunk2LimitOrientY = ReadFloat(levelVarBlock, 0x90);
                float chunk2LimitOrientZ = ReadFloat(levelVarBlock, 0x94);
                off98 = ReadInt(levelVarBlock, 0x98);
                off9C = ReadInt(levelVarBlock, 0x9C);

                chunk2Plane = new Vector3(chunk2LimitX, chunk2LimitY, chunk2LimitZ);
                chunk2PlaneNormal = new Vector3(chunk2LimitOrientX, chunk2LimitOrientY, chunk2LimitOrientZ);
            }
            else
            {
                byteSize = 0x80;

                off7C = ReadInt(levelVarBlock, 0x7C);
            }

            backgroundColor = Color.FromRgb((byte) bgRed, (byte) bgGreen, (byte) bgBlue).ToPixel<Rgb24>();
            fogColor = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
            sphereCentre = new Vector3(sphereCentreX, sphereCentreY, sphereCentreZ);
            shipPosition = new Vector3(shipPositionX, shipPositionY, shipPositionZ);
            chunk1Plane = new Vector3(chunk1PlaneX, chunk1PlaneY, chunk1PlaneZ);
            chunk1PlaneNormal = new Vector3(chunk1PlaneNormalX, chunk1PlaneNormalY, chunk1PlaneNormalZ);
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
            _isSphericalWorld = ReadInt(levelVarBlock, 0x2C);

            float sphereCentreX = ReadFloat(levelVarBlock, 0x30);
            float sphereCentreY = ReadFloat(levelVarBlock, 0x34);
            float sphereCentreZ = ReadFloat(levelVarBlock, 0x38);
            float shipPositionX = ReadFloat(levelVarBlock, 0x3C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x40);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x44);
            shipRotation = ReadFloat(levelVarBlock, 0x48);
            shipPathID = ReadInt(levelVarBlock, 0x4C);

            shipCameraStartID = ReadInt(levelVarBlock, 0x50);
            shipCameraEndID = ReadInt(levelVarBlock, 0x54);
            off58 = ReadInt(levelVarBlock, 0x58);
            float chunk1PlaneX = ReadFloat(levelVarBlock, 0x5C);

            float chunk1PlaneY = ReadFloat(levelVarBlock, 0x60);
            float chunk1PlaneZ = ReadFloat(levelVarBlock, 0x64);
            chunkCount = ReadInt(levelVarBlock, 0x68);
            float chunk1PlaneNormalX = ReadFloat(levelVarBlock, 0x6C);

            float chunk1PlaneNormalY = ReadFloat(levelVarBlock, 0x70);
            float chunk1PlaneNormalZ = ReadFloat(levelVarBlock, 0x74);
            off78 = ReadInt(levelVarBlock, 0x78);

            if (chunkCount > 1)
            {
                byteSize = 0x104;

                float chunk2LimitX = ReadFloat(levelVarBlock, 0x7C);

                float chunk2LimitY = ReadFloat(levelVarBlock, 0x80);
                float chunk2LimitZ = ReadFloat(levelVarBlock, 0x84);
                unknownChunk2 = ReadInt(levelVarBlock, 0x88);
                float chunk2LimitOrientX = ReadFloat(levelVarBlock, 0x8C);

                float chunk2LimitOrientY = ReadFloat(levelVarBlock, 0x90);
                float chunk2LimitOrientZ = ReadFloat(levelVarBlock, 0x94);
                off98 = ReadInt(levelVarBlock, 0x98);
                off9C = ReadInt(levelVarBlock, 0x9C);

                if (levelVarBlock.Length < 0x104)
                {
                    byteSize = 0x100;
                }
                else
                {
                    off100 = ReadInt(levelVarBlock, 0x100);
                }

                chunk2Plane = new Vector3(chunk2LimitX, chunk2LimitY, chunk2LimitZ);
                chunk2PlaneNormal = new Vector3(chunk2LimitOrientX, chunk2LimitOrientY, chunk2LimitOrientZ);
            }
            else
            {
                byteSize = 0x84;

                off7C = ReadInt(levelVarBlock, 0x7C);
                off80 = ReadInt(levelVarBlock, 0x80);
            }

            backgroundColor = Color.FromRgb((byte) bgRed, (byte) bgGreen, (byte) bgBlue).ToPixel<Rgb24>();
            fogColor = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
            sphereCentre = new Vector3(sphereCentreX, sphereCentreY, sphereCentreZ);
            shipPosition = new Vector3(shipPositionX, shipPositionY, shipPositionZ);
            chunk1Plane = new Vector3(chunk1PlaneX, chunk1PlaneY, chunk1PlaneZ);
            chunk1PlaneNormal = new Vector3(chunk1PlaneNormalX, chunk1PlaneNormalY, chunk1PlaneNormalZ);
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
            _isSphericalWorld = ReadInt(levelVarBlock, 0x2C);

            float sphereCentreX = ReadFloat(levelVarBlock, 0x30);
            float sphereCentreY = ReadFloat(levelVarBlock, 0x34);
            float sphereCentreZ = ReadFloat(levelVarBlock, 0x38);
            float shipPositionX = ReadFloat(levelVarBlock, 0x3C);

            float shipPositionY = ReadFloat(levelVarBlock, 0x40);
            float shipPositionZ = ReadFloat(levelVarBlock, 0x44);
            shipRotation = ReadFloat(levelVarBlock, 0x48);
            shipPathID = ReadInt(levelVarBlock, 0x4C);

            shipCameraStartID = ReadInt(levelVarBlock, 0x50);
            shipCameraEndID = ReadInt(levelVarBlock, 0x54);
            off58 = ReadInt(levelVarBlock, 0x58);
            off5C = ReadInt(levelVarBlock, 0x5C);

            off60 = ReadInt(levelVarBlock, 0x60);
            off64 = ReadFloat(levelVarBlock, 0x64);
            off68 = ReadInt(levelVarBlock, 0x68);
            off6C = ReadInt(levelVarBlock, 0x6C);

            off70 = ReadInt(levelVarBlock, 0x70);
            off74 = ReadFloat(levelVarBlock, 0x74);
            off78 = ReadInt(levelVarBlock, 0x78);
            off7C = ReadInt(levelVarBlock, 0x7C);

            off80 = ReadInt(levelVarBlock, 0x80);

            unknownBytes = new byte[levelVarBlock.Length - 0x84];

            for (int i = 0; i < levelVarBlock.Length - 0x84; i++)
            {
                unknownBytes[i] = levelVarBlock[0x84 + i];
            }

            backgroundColor = Color.FromRgb((byte) bgRed, (byte) bgGreen, (byte) bgBlue).ToPixel<Rgb24>();
            fogColor = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
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
            byte[] bytes = new byte[0x50];

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
            WriteInt(bytes, 0x3C, shipPathID);

            WriteInt(bytes, 0x40, shipCameraStartID);
            WriteInt(bytes, 0x44, shipCameraEndID);
            WriteInt(bytes, 0x48, off48);
            WriteInt(bytes, 0x4C, off4C);

            return bytes;
        }

        private byte[] SerializeRC2()
        {
            byte[] bytes = new byte[byteSize];

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
            WriteInt(bytes, 0x2C, _isSphericalWorld);

            WriteFloat(bytes, 0x30, sphereCentre.X);
            WriteFloat(bytes, 0x34, sphereCentre.Y);
            WriteFloat(bytes, 0x38, sphereCentre.Z);
            WriteFloat(bytes, 0x3C, shipPosition.X);

            WriteFloat(bytes, 0x40, shipPosition.Y);
            WriteFloat(bytes, 0x44, shipPosition.Z);
            WriteFloat(bytes, 0x48, shipRotation);
            WriteInt(bytes, 0x4C, shipPathID);

            WriteInt(bytes, 0x50, shipCameraStartID);
            WriteInt(bytes, 0x54, shipCameraEndID);
            WriteInt(bytes, 0x58, off58);
            WriteFloat(bytes, 0x5C, chunk1Plane.X);

            WriteFloat(bytes, 0x60, chunk1Plane.Y);
            WriteFloat(bytes, 0x64, chunk1Plane.Z);
            WriteInt(bytes, 0x68, chunkCount);
            WriteFloat(bytes, 0x6C, chunk1PlaneNormal.X);

            WriteFloat(bytes, 0x70, chunk1PlaneNormal.Y);
            WriteFloat(bytes, 0x74, chunk1PlaneNormal.Z);
            WriteInt(bytes, 0x78, off78);

            if (chunkCount > 1)
            {
                WriteFloat(bytes, 0x7C, chunk2Plane.X);

                WriteFloat(bytes, 0x80, chunk2Plane.Y);
                WriteFloat(bytes, 0x84, chunk2Plane.Z);
                WriteInt(bytes, 0x88, unknownChunk2);
                WriteFloat(bytes, 0x8C, chunk2PlaneNormal.X);

                WriteFloat(bytes, 0x90, chunk2PlaneNormal.Y);
                WriteFloat(bytes, 0x94, chunk2PlaneNormal.Z);
                WriteInt(bytes, 0x98, off98);
                WriteInt(bytes, 0x9C, off9C);
            }
            else
            {
                WriteInt(bytes, 0x7C, off7C);
            }

            return bytes;
        }

        private byte[] SerializeRC3()
        {
            byte[] bytes = new byte[byteSize];

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
            WriteInt(bytes, 0x2C, _isSphericalWorld);

            WriteFloat(bytes, 0x30, sphereCentre.X);
            WriteFloat(bytes, 0x34, sphereCentre.Y);
            WriteFloat(bytes, 0x38, sphereCentre.Z);
            WriteFloat(bytes, 0x3C, shipPosition.X);

            WriteFloat(bytes, 0x40, shipPosition.Y);
            WriteFloat(bytes, 0x44, shipPosition.Z);
            WriteFloat(bytes, 0x48, shipRotation);
            WriteInt(bytes, 0x4C, shipPathID);

            WriteInt(bytes, 0x50, shipCameraStartID);
            WriteInt(bytes, 0x54, shipCameraEndID);
            WriteInt(bytes, 0x58, off58);
            WriteFloat(bytes, 0x5C, chunk1Plane.X);

            WriteFloat(bytes, 0x60, chunk1Plane.Y);
            WriteFloat(bytes, 0x64, chunk1Plane.Z);
            WriteInt(bytes, 0x68, chunkCount);
            WriteFloat(bytes, 0x6C, chunk1PlaneNormal.X);

            WriteFloat(bytes, 0x70, chunk1PlaneNormal.Y);
            WriteFloat(bytes, 0x74, chunk1PlaneNormal.Z);
            WriteInt(bytes, 0x78, off78);

            if (chunkCount > 1)
            {
                WriteFloat(bytes, 0x7C, chunk2Plane.X);

                WriteFloat(bytes, 0x80, chunk2Plane.Y);
                WriteFloat(bytes, 0x84, chunk2Plane.Z);
                WriteInt(bytes, 0x88, unknownChunk2);
                WriteFloat(bytes, 0x8C, chunk2PlaneNormal.X);

                WriteFloat(bytes, 0x90, chunk2PlaneNormal.Y);
                WriteFloat(bytes, 0x94, chunk2PlaneNormal.Z);
                WriteInt(bytes, 0x98, off98);
                WriteInt(bytes, 0x9C, off9C);

                WriteInt(bytes, 0x100, off100);
            }
            else
            {
                WriteInt(bytes, 0x7C, off7C);
                WriteInt(bytes, 0x80, off80);
            }

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
            WriteInt(bytes, 0x2C, _isSphericalWorld);

            WriteFloat(bytes, 0x30, sphereCentre.X);
            WriteFloat(bytes, 0x34, sphereCentre.Y);
            WriteFloat(bytes, 0x38, sphereCentre.Z);
            WriteFloat(bytes, 0x3C, shipPosition.X);

            WriteFloat(bytes, 0x40, shipPosition.Y);
            WriteFloat(bytes, 0x44, shipPosition.Z);
            WriteFloat(bytes, 0x48, shipRotation);
            WriteInt(bytes, 0x4C, shipPathID);

            WriteInt(bytes, 0x50, shipCameraStartID);
            WriteInt(bytes, 0x54, shipCameraEndID);
            WriteInt(bytes, 0x58, off58);
            WriteInt(bytes, 0x5C, off5C);

            WriteInt(bytes, 0x60, off60);
            WriteFloat(bytes, 0x64, off64);
            WriteInt(bytes, 0x68, off68);
            WriteInt(bytes, 0x6C, off6C);

            WriteInt(bytes, 0x70, off70);
            WriteFloat(bytes, 0x74, off74);
            WriteInt(bytes, 0x78, off78);
            WriteInt(bytes, 0x7C, off7C);

            WriteInt(bytes, 0x80, off80);

            unknownBytes.CopyTo(bytes, 0x84);

            return bytes;
        }
    }
}
