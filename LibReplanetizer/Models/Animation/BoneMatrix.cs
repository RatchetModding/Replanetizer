// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{

    //
    // RaC 1, 2 and 3:
    // Bonematrix contains the bind matrix rotation and scaling in the 3x3 part of the transformation and the bone to bone offset in
    // the translation part of the 3x4 transformation.
    // The total negative offset needed for the inverse bind matrix is stored as a separate vector.
    //
    // Deadlocked:
    // The 3x4 part of the inverse bind matrix is stored as is.
    //
    // All translations are scaled by 1024.
    //
    public class BoneMatrix
    {
        public Matrix3x4 transformation;
        private Vector3 cumulativeOffset;
        private short parent;
        public short id;
        private short unk0x3C;
        private Matrix4 inverseBindMatrix;
        private Matrix4 inverseBindMatrixTransposed;

        public BoneMatrix(GameType game, byte[] boneBlock, int num)
        {
            if (game == GameType.DL)
            {
                GetDLVals(boneBlock, num);
            }
            else
            {
                GetRC123Vals(boneBlock, num);
            }
        }

        private void GetRC123Vals(byte[] boneBlock, int num)
        {
            int offset = num * 0x40;
            id = (short) (offset / 0x40);

            transformation = ReadMatrix3x4(boneBlock, offset);

            float cumulativeOffsetX = ReadFloat(boneBlock, offset + 0x30);
            float cumulativeOffsetY = ReadFloat(boneBlock, offset + 0x34);
            float cumulativeOffsetZ = ReadFloat(boneBlock, offset + 0x38);

            cumulativeOffset = new Vector3(cumulativeOffsetX / 1024.0f, cumulativeOffsetY / 1024.0f, cumulativeOffsetZ / 1024.0f);

            //0 for root and some constant else (0b0111000000000000 = 0x7000 = 28672)
            unk0x3C = ReadShort(boneBlock, offset + 0x3C);
            parent = (short) (ReadShort(boneBlock, offset + 0x3E) / 0x40);

            inverseBindMatrix = new Matrix4();
            inverseBindMatrix.M11 = transformation.M11;
            inverseBindMatrix.M12 = transformation.M21;
            inverseBindMatrix.M13 = transformation.M31;
            inverseBindMatrix.M14 = cumulativeOffset.X;
            inverseBindMatrix.M21 = transformation.M12;
            inverseBindMatrix.M22 = transformation.M22;
            inverseBindMatrix.M23 = transformation.M32;
            inverseBindMatrix.M24 = cumulativeOffset.Y;
            inverseBindMatrix.M31 = transformation.M13;
            inverseBindMatrix.M32 = transformation.M23;
            inverseBindMatrix.M33 = transformation.M33;
            inverseBindMatrix.M34 = cumulativeOffset.Z;
            inverseBindMatrix.M41 = 0.0f;
            inverseBindMatrix.M42 = 0.0f;
            inverseBindMatrix.M43 = 0.0f;
            inverseBindMatrix.M44 = 1.0f;

            inverseBindMatrixTransposed = inverseBindMatrix;
            inverseBindMatrixTransposed.Transpose();
        }

        private void GetDLVals(byte[] boneBlock, int num)
        {
            int offset = num * 0x30;
            id = (short) (offset / 0x30);

            transformation = ReadMatrix3x4(boneBlock, offset);

            inverseBindMatrix = new Matrix4();
            inverseBindMatrix.M11 = transformation.M11;
            inverseBindMatrix.M12 = transformation.M12;
            inverseBindMatrix.M13 = transformation.M13;
            inverseBindMatrix.M14 = transformation.M14 / 1024.0f;
            inverseBindMatrix.M21 = transformation.M21;
            inverseBindMatrix.M22 = transformation.M22;
            inverseBindMatrix.M23 = transformation.M23;
            inverseBindMatrix.M24 = transformation.M24 / 1024.0f;
            inverseBindMatrix.M31 = transformation.M31;
            inverseBindMatrix.M32 = transformation.M32;
            inverseBindMatrix.M33 = transformation.M33;
            inverseBindMatrix.M34 = transformation.M34 / 1024.0f;
            inverseBindMatrix.M41 = 0.0f;
            inverseBindMatrix.M42 = 0.0f;
            inverseBindMatrix.M43 = 0.0f;
            inverseBindMatrix.M44 = 1.0f;

            inverseBindMatrixTransposed = inverseBindMatrix;
            inverseBindMatrixTransposed.Transpose();
        }

        public Matrix4 GetInvBindMatrix(bool transposed = false)
        {
            return (transposed) ? inverseBindMatrixTransposed : inverseBindMatrix;
        }

        public Vector3 GetTranslation()
        {
            return -inverseBindMatrix.ExtractTranslation();
        }

        public Quaternion GetRotation()
        {
            return inverseBindMatrix.ExtractRotation().Inverted();
        }

        public Vector3 GetScale()
        {
            Vector3 scale = inverseBindMatrix.ExtractScale();

            scale.X = 1.0f / scale.X;
            scale.Y = 1.0f / scale.Y;
            scale.Z = 1.0f / scale.Z;

            return scale;
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x40];
            WriteMatrix3x4(outBytes, 0x00, transformation);
            WriteFloat(outBytes, 0x30, cumulativeOffset.X * 1024.0f);
            WriteFloat(outBytes, 0x34, cumulativeOffset.Y * 1024.0f);
            WriteFloat(outBytes, 0x38, cumulativeOffset.Z * 1024.0f);
            WriteShort(outBytes, 0x3C, unk0x3C);
            WriteShort(outBytes, 0x3E, (short) (parent * 0x40));

            return outBytes;
        }
    }
}
