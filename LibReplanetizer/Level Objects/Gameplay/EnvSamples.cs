// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.ComponentModel;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class EnvSample : LevelObject
    {
        private GameType game = GameType.RaC1;

        private const int ELEMENTSIZE_RAC1 = 0x30;
        private const int ELEMENTSIZE_RAC23DL = 0x20;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }
        [Category("Attributes"), DisplayName("Ratchet Light ID")]
        public int heroLight { get; set; }
        [Category("Attributes"), DisplayName("Ratchet Ambient Color")]
        public Rgb24 heroColor { get; set; }
        [Category("Unknowns"), DisplayName("Reverb Depth")]
        public int reverbDepth { get; set; }
        [Category("Unknowns"), DisplayName("Reverb Type")]
        public byte reverbType { get; set; }
        [Category("Unknowns"), DisplayName("Reverb Delay")]
        public byte reverbDelay { get; set; }
        [Category("Unknowns"), DisplayName("Reverb Feedback")]
        public byte reverbFeedback { get; set; }
        [Category("Unknowns"), DisplayName("Enable Reverb")]
        public bool enableReverbParams { get; set; }
        [Category("Attributes"), DisplayName("Music Track ID")]
        public int musicTrack { get; set; }
        [Category("Attributes"), DisplayName("Fog Color")]
        public Rgb24 fogColor { get; set; }
        [Category("Attributes"), DisplayName("Fog Near Intensity")]
        public float fogNearIntensity { get; set; }
        [Category("Attributes"), DisplayName("Fog Far Intensity")]
        public float fogFarIntensity { get; set; }
        [Category("Attributes"), DisplayName("Fog Near Distance")]
        public float fogNearDist { get; set; }
        [Category("Attributes"), DisplayName("Fog Far Distance")]
        public float fogFarDist { get; set; }

        public EnvSample(GameType game, byte[] block, int num)
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

        private void GetRC1Vals(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE_RAC1;

            float posX = ReadFloat(block, offset + 0x00);
            float posY = ReadFloat(block, offset + 0x04);
            float posZ = ReadFloat(block, offset + 0x08);
            // ReadFloat(block, offset + 0x0C); Always 1.0f (This is because this is a position and not a direction)

            int heroR = ReadInt(block, offset + 0x10);
            int heroG = ReadInt(block, offset + 0x14);
            int heroB = ReadInt(block, offset + 0x18);
            heroLight = ReadInt(block, offset + 0x1C);

            reverbDepth = ReadInt(block, offset + 0x20);
            reverbType = block[offset + 0x24];
            reverbDelay = block[offset + 0x25];
            reverbFeedback = block[offset + 0x26];
            enableReverbParams = block[offset + 0x27] > 0;
            musicTrack = ReadInt(block, offset + 0x28);

            position = new Vector3(posX, posY, posZ);
            heroColor = Color.FromRgb((byte) heroR, (byte) heroG, (byte) heroB).ToPixel<Rgb24>();
        }

        private void GetRC23DLVals(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE_RAC23DL;

            heroLight = ReadInt(block, offset + 0x00);
            short posX = ReadShort(block, offset + 0x04);
            short posY = ReadShort(block, offset + 0x06);
            short posZ = ReadShort(block, offset + 0x08);
            reverbDepth = ReadShort(block, offset + 0x0A);
            musicTrack = ReadShort(block, offset + 0x0C);
            byte fogNI = block[offset + 0x0E];
            byte fogFI = block[offset + 0x0F];

            byte heroR = block[offset + 0x10];
            byte heroG = block[offset + 0x11];
            byte heroB = block[offset + 0x12];
            reverbType = block[offset + 0x13];
            reverbDelay = block[offset + 0x14];
            reverbFeedback = block[offset + 0x15];
            enableReverbParams = block[offset + 0x16] > 0;
            byte fogR = block[offset + 0x17];
            byte fogG = block[offset + 0x18];
            byte fogB = block[offset + 0x19];
            short fogND = ReadShort(block, offset + 0x1A);
            short fogFD = ReadShort(block, offset + 0x1C);

            position = new Vector3(posX * 0.25f, posY * 0.25f, posZ * 0.25f);
            heroColor = Color.FromRgb((byte) heroR, (byte) heroG, (byte) heroB).ToPixel<Rgb24>();
            fogColor = Color.FromRgb((byte) fogR, (byte) fogG, (byte) fogB).ToPixel<Rgb24>();

            // TODO: How to correctly interpret these values
            fogNearDist = (float) fogND;
            fogFarDist = (float) fogFD;
            fogNearIntensity = (float) fogNI;
            fogFarIntensity = (float) fogFI;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsDynamic()
        {
            return false;
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
            WriteFloat(bytes, 0x0C, 1.0f);

            WriteInt(bytes, 0x10, heroColor.R);
            WriteInt(bytes, 0x14, heroColor.G);
            WriteInt(bytes, 0x18, heroColor.B);
            WriteInt(bytes, 0x1C, heroLight);

            WriteInt(bytes, 0x20, reverbDepth);
            bytes[0x24] = reverbType;
            bytes[0x25] = reverbDelay;
            bytes[0x26] = reverbFeedback;
            bytes[0x27] = (byte) ((enableReverbParams) ? 1 : 0);
            WriteInt(bytes, 0x28, musicTrack);
            WriteInt(bytes, 0x2C, 0);

            return bytes;
        }

        private byte[] ToByteArrayRC23DL()
        {
            byte[] bytes = new byte[ELEMENTSIZE_RAC23DL];

            WriteInt(bytes, 0x00, heroLight);
            WriteShort(bytes, 0x04, (short) MathF.Round(position.X * 4.0f));
            WriteShort(bytes, 0x06, (short) MathF.Round(position.Y * 4.0f));
            WriteShort(bytes, 0x08, (short) MathF.Round(position.Z * 4.0f));
            WriteShort(bytes, 0x0A, (short) reverbDepth);
            WriteShort(bytes, 0x0C, (short) musicTrack);
            bytes[0x0E] = (byte) fogNearIntensity;
            bytes[0x0F] = (byte) fogFarIntensity;

            bytes[0x10] = (byte) heroColor.R;
            bytes[0x11] = (byte) heroColor.G;
            bytes[0x12] = (byte) heroColor.B;
            bytes[0x13] = (byte) reverbType;
            bytes[0x14] = (byte) reverbDelay;
            bytes[0x15] = (byte) reverbFeedback;
            bytes[0x16] = (byte) ((enableReverbParams) ? 1 : 0);
            bytes[0x17] = (byte) fogColor.R;
            bytes[0x18] = (byte) fogColor.G;
            bytes[0x19] = (byte) fogColor.B;
            WriteShort(bytes, 0x1A, (short) fogNearDist);
            WriteShort(bytes, 0x1C, (short) fogFarDist);
            WriteShort(bytes, 0x1E, 0);

            return bytes;
        }
    }
}
