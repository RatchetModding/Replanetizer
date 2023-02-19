// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{
    public class Frame
    {
        private struct FrameBoneScaling
        {
            public FrameBoneScaling(float x, float y, float z, byte bone, byte unk)
            {
                this.scale = new Vector3(x, y, z);
                this.bone = bone;
                this.unk = unk;
            }
            public Vector3 scale;
            public byte bone;
            /*
             * This value is either 0 or 128
             * Setting this to always 128 seems to work just fine
             * Setting this to always 0 causes Clanks rotors to not be scaled correctly
             */
            public byte unk;
        }

        private struct FrameBoneTranslation
        {
            public FrameBoneTranslation(float x, float y, float z, byte bone, byte unk)
            {
                this.translation = new Vector3(x, y, z);
                this.bone = bone;
                this.unk = unk;
            }
            public Vector3 translation;
            public byte bone;
            public byte unk;
        }

        public float speed { get; set; }
        public ushort frameIndex { get; set; }
        public ushort frameLength { get; set; }

        public List<short[]> rotations { get; set; }
        private List<FrameBoneScaling> sec0s { get; set; }
        private List<FrameBoneTranslation> translations { get; set; }

        public Matrix4 GetRotationMatrix(int bone)
        {
            short[] rots = rotations[bone];
            Quaternion rot = new Quaternion((rots[0] / 32767f) * 180f, (rots[1] / 32767f) * 180f, (rots[2] / 32767f) * 180f, (-rots[3] / 32767f) * 180f);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rot);

            return rotationMatrix;
        }

        public Vector3? GetScaling(int bone)
        {
            bool exists = sec0s.Exists(s => s.bone == bone);

            if (exists)
            {
                FrameBoneScaling scale = sec0s.First(s => s.bone == bone);
                Vector3 s = scale.scale;

                /*if (scale.unk == 128)
                {
                    s.X = MathF.Exp(s.X);
                    s.Y = MathF.Exp(s.Y);
                    s.Z = MathF.Exp(s.Z);
                }*/

                return s;
            } 

            return null;
        }

        public bool GetScalingUnk(int bone)
        {
            bool exists = sec0s.Exists(s => s.bone == bone);

            if (exists)
            {
                FrameBoneScaling scale = sec0s.First(s => s.bone == bone);

                return scale.unk == 128;
            }

            return false;
        }

        public Vector3? GetTranslation(int bone)
        {
            bool exists = translations.Exists(t => t.bone == bone);

            if (exists)
            {
                return translations.First(t => t.bone == bone).translation;
            }

            return null;
        }

        public Frame(FileStream fs, int offset, int boneCount)
        {
            byte[] header = ReadBlock(fs, offset, 0x10);
            speed = ReadFloat(header, 0x00);
            frameIndex = ReadUshort(header, 0x04);
            frameLength = ReadUshort(header, 0x06);
            ushort sec0Pointer = ReadUshort(header, 0x08);
            ushort sec0Count = ReadUshort(header, 0x0A);
            ushort translationPointer = ReadUshort(header, 0x0C);
            ushort translationCount = ReadUshort(header, 0x0E);

            byte[] frameBlock = ReadBlock(fs, offset + 0x10, frameLength * 0x10);
            rotations = new List<short[]>();
            for (int i = 0; i < boneCount; i++)
            {
                short[] rot = new short[4];
                rot[0] = ReadShort(frameBlock, i * 8 + 0x00);
                rot[1] = ReadShort(frameBlock, i * 8 + 0x02);
                rot[2] = ReadShort(frameBlock, i * 8 + 0x04);
                rot[3] = ReadShort(frameBlock, i * 8 + 0x06);
                rotations.Add(rot);
            }

            sec0s = new List<FrameBoneScaling>();
            for (int i = 0; i < sec0Count; i++)
            {
                if (sec0Pointer + (i + 1) * 0x08 > frameBlock.Length)
                {
                    // Custom MP levels may have a too large sec0Count
                    break;
                }
                float x = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x00) / 4096.0f;
                float y = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x02) / 4096.0f;
                float z = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x04) / 4096.0f;
                byte bone = frameBlock[sec0Pointer + i * 8 + 0x06];
                byte unk = frameBlock[sec0Pointer + i * 8 + 0x07];
                sec0s.Add(new FrameBoneScaling(x, y, z, bone, unk));
            }

            translations = new List<FrameBoneTranslation>();
            for (int i = 0; i < translationCount; i++)
            {
                if (translationPointer + (i + 1) * 0x08 > frameBlock.Length)
                {
                    // Custom MP levels may have a too large translationCount
                    break;
                }
                float x = ReadShort(frameBlock, translationPointer + i * 8 + 0x00) / 1024.0f;
                float y = ReadShort(frameBlock, translationPointer + i * 8 + 0x02) / 1024.0f;
                float z = ReadShort(frameBlock, translationPointer + i * 8 + 0x04) / 1024.0f;
                byte unk = frameBlock[translationPointer + i * 8 + 0x06];
                byte unk2 = frameBlock[translationPointer + i * 8 + 0x07];
                translations.Add(new FrameBoneTranslation(x, y, z, unk, unk2));
            }
        }

        public byte[] Serialize()
        {

            byte[] rotationBytes = new byte[rotations.Count * 0x08];
            for (int i = 0; i < rotations.Count; i++)
            {
                WriteShort(rotationBytes, i * 8 + 0x00, rotations[i][0]);
                WriteShort(rotationBytes, i * 8 + 0x02, rotations[i][1]);
                WriteShort(rotationBytes, i * 8 + 0x04, rotations[i][2]);
                WriteShort(rotationBytes, i * 8 + 0x06, rotations[i][3]);
            }

            byte[] sec0Bytes = new byte[sec0s.Count * 0x08];
            for (int i = 0; i < sec0s.Count; i++)
            {
                WriteShort(sec0Bytes, i * 8 + 0x00, (short) (sec0s[i].scale.X * 4096.0f));
                WriteShort(sec0Bytes, i * 8 + 0x02, (short) (sec0s[i].scale.Y * 4096.0f));
                WriteShort(sec0Bytes, i * 8 + 0x04, (short) (sec0s[i].scale.Z * 4096.0f));
                sec0Bytes[i * 8 + 0x06] = sec0s[i].bone;
                sec0Bytes[i * 8 + 0x07] = sec0s[i].unk;
            }

            byte[] translationBytes = new byte[translations.Count * 0x08];
            for (int i = 0; i < translations.Count; i++)
            {
                WriteShort(translationBytes, i * 8 + 0x00, (short) (translations[i].translation.X * 1024.0f));
                WriteShort(translationBytes, i * 8 + 0x02, (short) (translations[i].translation.Y * 1024.0f));
                WriteShort(translationBytes, i * 8 + 0x04, (short) (translations[i].translation.Z * 1024.0f));
                translationBytes[i * 8 + 0x06] = translations[i].bone;
                translationBytes[i * 8 + 0x07] = translations[i].unk;
            }

            ushort sec0Pointer = (ushort) rotationBytes.Length;
            ushort translationPointer = (ushort) (rotationBytes.Length + sec0Bytes.Length);

            byte[] header = new byte[0x10];
            WriteFloat(header, 0x00, speed);
            WriteUshort(header, 0x04, frameIndex);
            WriteUshort(header, 0x06, frameLength);
            WriteUshort(header, 0x08, sec0Pointer);
            WriteUshort(header, 0x0A, (ushort) sec0s.Count);
            WriteUshort(header, 0x0C, translationPointer);
            WriteUshort(header, 0x0E, (ushort) translations.Count);

            byte[] outBytes = new byte[GetLength(0x10 + rotationBytes.Length + sec0Bytes.Length + translationBytes.Length)];
            header.CopyTo(outBytes, 0);
            rotationBytes.CopyTo(outBytes, 0x10);
            sec0Bytes.CopyTo(outBytes, 0x10 + rotationBytes.Length);
            translationBytes.CopyTo(outBytes, 0x10 + rotationBytes.Length + sec0Bytes.Length);

            return outBytes;
        }
    }
}
