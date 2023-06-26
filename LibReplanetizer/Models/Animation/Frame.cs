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

        private List<short[]> rotations { get; set; }
        private List<FrameBoneScaling> scalings { get; set; }
        private List<FrameBoneTranslation> translations { get; set; }

        public Quaternion? GetRotationQuaternion(int bone)
        {
            if (bone >= rotations.Count)
            {
                return null;
            }

            short[] rots = rotations[bone];
            return new Quaternion((rots[0] / 32767f) * 180f, (rots[1] / 32767f) * 180f, (rots[2] / 32767f) * 180f, (-rots[3] / 32767f) * 180f);
        }

        public Matrix4 GetRotationMatrix(int bone)
        {
            Quaternion? rotation = GetRotationQuaternion(bone);

            return (rotation != null) ? Matrix4.CreateFromQuaternion((Quaternion) rotation) : Matrix4.Identity;
        }

        public Vector3? GetScaling(int bone)
        {
            bool exists = scalings.Exists(s => s.bone == bone);

            if (exists)
            {
                return scalings.First(s => s.bone == bone).scale;
            }

            return null;
        }

        public bool GetScalingUnk(int bone)
        {
            bool exists = scalings.Exists(s => s.bone == bone);

            if (exists)
            {
                return scalings.First(s => s.bone == bone).unk == 128;
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

        public Matrix4 GetRotationMatrix(int bone, Frame nextFrame, float blend)
        {
            Quaternion? baseRotation = GetRotationQuaternion(bone);
            Quaternion? nextRotation = nextFrame.GetRotationQuaternion(bone);

            if (baseRotation == null || nextRotation == null)
            {
                if (baseRotation != null)
                {
                    return Matrix4.CreateFromQuaternion((Quaternion) baseRotation);
                }
                else if (nextRotation != null)
                {
                    return Matrix4.CreateFromQuaternion((Quaternion) nextRotation);
                }
                else
                {
                    return Matrix4.Identity;
                }
            }

            Quaternion rot1 = (Quaternion) baseRotation;
            Quaternion rot2 = (Quaternion) nextRotation;

            // Quaternion.Slerp does not work for some reason, it always returns the first operand.
            Quaternion rotation = new Quaternion(
                rot1.X + (rot2.X - rot1.X) * blend,
                rot1.Y + (rot2.Y - rot1.Y) * blend,
                rot1.Z + (rot2.Z - rot1.Z) * blend,
                rot1.W + (rot2.W - rot1.W) * blend);

            return Matrix4.CreateFromQuaternion(rotation);
        }

        public Vector3? GetScaling(int bone, Frame nextFrame, float blend)
        {
            Vector3? baseScale = GetScaling(bone);
            Vector3? nextScale = nextFrame.GetScaling(bone);

            if (baseScale == null || nextScale == null) return null;

            return (1.0f - blend) * baseScale + blend * nextScale;
        }

        public Vector3? GetTranslation(int bone, Frame nextFrame, float blend)
        {
            Vector3? baseTranslation = GetTranslation(bone);
            Vector3? nextTranslation = nextFrame.GetTranslation(bone);

            if (baseTranslation == null || nextTranslation == null) return null;

            return (1.0f - blend) * baseTranslation + blend * nextTranslation;
        }

        // Constructor for RaC 1, 2 and 3
        public Frame(FileStream fs, GameType game, int offset, int boneCount)
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

            scalings = new List<FrameBoneScaling>();
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
                scalings.Add(new FrameBoneScaling(x, y, z, bone, unk));
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
                byte bone = frameBlock[translationPointer + i * 8 + 0x06];
                byte unk = frameBlock[translationPointer + i * 8 + 0x07];
                translations.Add(new FrameBoneTranslation(x, y, z, bone, unk));
            }
        }

        // Constructor for Deadlocked
        public Frame(byte[] frameBlock, int numRotations, int numScalings, int numTranslations)
        {
            int rotationOffset = 0;
            rotations = new List<short[]>();
            for (int i = 0; i < numRotations; i++)
            {
                short[] rot = new short[4];
                rot[0] = ReadShort(frameBlock, i * 8 + 0x00);
                rot[1] = ReadShort(frameBlock, i * 8 + 0x02);
                rot[2] = ReadShort(frameBlock, i * 8 + 0x04);
                rot[3] = ReadShort(frameBlock, i * 8 + 0x06);
                rotations.Add(rot);
            }

            int scalingOffset = rotationOffset + numRotations * 0x08;
            scalings = new List<FrameBoneScaling>();
            for (int i = 0; i < numScalings; i++)
            {
                float x = ReadShort(frameBlock, scalingOffset + i * 8 + 0x00) / 32767.0f;
                float y = ReadShort(frameBlock, scalingOffset + i * 8 + 0x02) / 32767.0f;
                float z = ReadShort(frameBlock, scalingOffset + i * 8 + 0x04) / 32767.0f;
                byte unk = frameBlock[scalingOffset + i * 8 + 0x06];
                byte bone = frameBlock[scalingOffset + i * 8 + 0x07];
                scalings.Add(new FrameBoneScaling(x, y, z, bone, unk));
            }

            int translationOffset = scalingOffset + numScalings * 0x08;
            translations = new List<FrameBoneTranslation>();
            for (int i = 0; i < numTranslations; i++)
            {
                float x = ReadShort(frameBlock, translationOffset + i * 8 + 0x00) / 32767.0f;
                float y = ReadShort(frameBlock, translationOffset + i * 8 + 0x02) / 32767.0f;
                float z = ReadShort(frameBlock, translationOffset + i * 8 + 0x04) / 32767.0f;
                byte unk = frameBlock[translationOffset + i * 8 + 0x06];
                byte bone = frameBlock[translationOffset + i * 8 + 0x07];
                translations.Add(new FrameBoneTranslation(x, y, z, bone, unk));
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

            byte[] sec0Bytes = new byte[scalings.Count * 0x08];
            for (int i = 0; i < scalings.Count; i++)
            {
                WriteShort(sec0Bytes, i * 8 + 0x00, (short) (scalings[i].scale.X * 4096.0f));
                WriteShort(sec0Bytes, i * 8 + 0x02, (short) (scalings[i].scale.Y * 4096.0f));
                WriteShort(sec0Bytes, i * 8 + 0x04, (short) (scalings[i].scale.Z * 4096.0f));
                sec0Bytes[i * 8 + 0x06] = scalings[i].bone;
                sec0Bytes[i * 8 + 0x07] = scalings[i].unk;
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
            WriteUshort(header, 0x0A, (ushort) scalings.Count);
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
