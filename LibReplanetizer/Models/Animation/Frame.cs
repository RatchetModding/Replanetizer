// Copyright (C) 2018-2023, The Replanetizer Contributors.
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

        private struct FrameBoneRotation
        {
            public FrameBoneRotation(int x, int y, int z, int w)
            {
                this.rotation = new Quaternion(x / 32767f, y / 32767f, z / 32767f, w / 32767f);
            }

            public Quaternion rotation;
        }

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

        private List<FrameBoneRotation> rotations { get; set; }
        private List<FrameBoneScaling> scalings { get; set; }
        private List<FrameBoneTranslation> translations { get; set; }

        public Quaternion? GetRotationQuaternion(int bone)
        {
            if (bone >= rotations.Count)
            {
                return null;
            }

            return rotations[bone].rotation;
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

            rot1 *= (1.0f - blend);
            rot2 *= blend;

            float dotProduct = rot1.X * rot2.X + rot1.Y * rot2.Y + rot1.Z * rot2.Z + rot1.W * rot2.W;

            if (dotProduct >= 0.0f)
            {
                rot1 += rot2;
            }
            else
            {
                rot1 -= rot2;
            }

            float normSquared = rot1.X * rot1.X + rot1.Y * rot1.Y + rot1.Z * rot1.Z + rot1.W * rot1.W;

            if (normSquared > 0.0f)
            {
                rot1 *= 1.0f / MathF.Sqrt(normSquared);
            }

            return Matrix4.CreateFromQuaternion(rot1);
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
            rotations = new List<FrameBoneRotation>();
            for (int i = 0; i < boneCount; i++)
            {
                short[] rot = new short[4];
                int x = ReadShort(frameBlock, i * 8 + 0x00);
                int y = ReadShort(frameBlock, i * 8 + 0x02);
                int z = ReadShort(frameBlock, i * 8 + 0x04);
                int w = ReadShort(frameBlock, i * 8 + 0x06);
                rotations.Add(new FrameBoneRotation(x, y, z, -w));
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
        public Frame(byte[] frameBlock, int offset, int numRotations, int numScalings, int numTranslations)
        {
            // This code is not correct and is causing crashes.
            // TODO: (Milch) Implement support for Deadlocked animations.
#if true
            rotations = new List<FrameBoneRotation>();
            scalings = new List<FrameBoneScaling>();
            translations = new List<FrameBoneTranslation>();
#else
            int rotationOffset = 0;
            rotations = new List<FrameBoneRotation>();
            for (int i = 0; i < numRotations; i++)
            {
                short[] rot = new short[4];
                int x = ReadShort(frameBlock, offset + i * 8 + 0x00);
                int y = ReadShort(frameBlock, offset + i * 8 + 0x02);
                int z = ReadShort(frameBlock, offset + i * 8 + 0x04);
                int w = ReadShort(frameBlock, offset + i * 8 + 0x06);
                rotations.Add(new FrameBoneRotation(x, y, z, -w));
            }

            int scalingOffset = rotationOffset + numRotations * 0x08;
            scalings = new List<FrameBoneScaling>();
            for (int i = 0; i < numScalings; i++)
            {
                float x = ReadShort(frameBlock, offset + scalingOffset + i * 8 + 0x00) / 32767.0f;
                float y = ReadShort(frameBlock, offset + scalingOffset + i * 8 + 0x02) / 32767.0f;
                float z = ReadShort(frameBlock, offset + scalingOffset + i * 8 + 0x04) / 32767.0f;
                byte unk = frameBlock[offset + scalingOffset + i * 8 + 0x06];
                byte bone = frameBlock[offset + scalingOffset + i * 8 + 0x07];
                scalings.Add(new FrameBoneScaling(x, y, z, bone, unk));
            }

            int translationOffset = scalingOffset + numScalings * 0x08;
            translations = new List<FrameBoneTranslation>();
            for (int i = 0; i < numTranslations; i++)
            {
                float x = ReadShort(frameBlock, offset + translationOffset + i * 8 + 0x00) / 32767.0f;
                float y = ReadShort(frameBlock, offset + translationOffset + i * 8 + 0x02) / 32767.0f;
                float z = ReadShort(frameBlock, offset + translationOffset + i * 8 + 0x04) / 32767.0f;
                byte unk = frameBlock[offset + translationOffset + i * 8 + 0x06];
                byte bone = frameBlock[offset + translationOffset + i * 8 + 0x07];
                translations.Add(new FrameBoneTranslation(x, y, z, bone, unk));
            }
#endif
        }

        public byte[] Serialize()
        {

            byte[] rotationBytes = new byte[rotations.Count * 0x08];
            for (int i = 0; i < rotations.Count; i++)
            {
                WriteShort(rotationBytes, i * 8 + 0x00, (short) (rotations[i].rotation.X * 32767.0f));
                WriteShort(rotationBytes, i * 8 + 0x02, (short) (rotations[i].rotation.Y * 32767.0f));
                WriteShort(rotationBytes, i * 8 + 0x04, (short) (rotations[i].rotation.Z * 32767.0f));
                WriteShort(rotationBytes, i * 8 + 0x06, (short) (-rotations[i].rotation.W * 32767.0f));
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
