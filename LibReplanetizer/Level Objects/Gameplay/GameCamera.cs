// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System;
using System.ComponentModel;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class GameCamera : MatrixObject, IRenderable
    {
        public const int ELEMENTSIZE = 0x20;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }
        [Category("Attributes"), DisplayName("Pvar Index")]
        public int pvarIndex { get; set; }

        static readonly float[] CAM = {
             0.000000f,-1.000000f, 1.333333f,
             -1.000000f,-1.000000f, 0.600000f,
             0.500000f,-1.000000f, 0.833333f,
             1.000000f,-1.000000f, 0.600000f,
             -0.500000f,-1.000000f, 0.833333f,
             -1.000000f,-1.000000f, -0.600000f,
              -0.001304f,2.050611f,-0.001558f,
             1.000000f,-1.000000f, -0.600000f
        };

        public static readonly ushort[] CAM_ELEMENTS = {
            2, 4, 0,
            3, 1, 7,
            1, 5, 7,
            6, 7, 5,
            7, 6, 3,
            6, 1, 3,
            5, 1, 6
        };

        public GameCamera(byte[] cameraBlock, int num)
        {
            int offset = num * ELEMENTSIZE;

            id = ReadInt(cameraBlock, offset + 0x00);
            float x = ReadFloat(cameraBlock, offset + 0x04);
            float y = ReadFloat(cameraBlock, offset + 0x08);
            float z = ReadFloat(cameraBlock, offset + 0x0C);
            float rx = ReadFloat(cameraBlock, offset + 0x10);
            float ry = ReadFloat(cameraBlock, offset + 0x14);
            float rz = ReadFloat(cameraBlock, offset + 0x18);
            pvarIndex = ReadInt(cameraBlock, offset + 0x1C);

            position = new Vector3(x, y, z);
            rotation = Quaternion.FromEulerAngles(rx, ry, rz);
            scale = new Vector3(1.0f, 1.0f, 1.0f);

            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            Vector3 rot = rotation.ToEulerAngles();

            WriteInt(bytes, 0x00, id);
            WriteFloat(bytes, 0x04, position.X);
            WriteFloat(bytes, 0x08, position.Y);
            WriteFloat(bytes, 0x0C, position.Z);
            WriteFloat(bytes, 0x10, rot.X);
            WriteFloat(bytes, 0x14, rot.Y);
            WriteFloat(bytes, 0x18, rot.Z);
            WriteInt(bytes, 0x1C, pvarIndex);

            return bytes;
        }

        public override void UpdateTransformMatrix()
        {
            Vector3 euler = rotation.ToEulerAngles();
            Matrix4 rotZ = Matrix4.CreateFromAxisAngle(Vector3.UnitZ, euler.Z);
            Matrix4 rotY = Matrix4.CreateFromAxisAngle(Vector3.UnitY, euler.Y);
            Matrix4 rotX = Matrix4.CreateFromAxisAngle(Vector3.UnitX, euler.X);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = rotX * rotY * rotZ * translationMatrix;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public ushort[] GetIndices()
        {
            return CAM_ELEMENTS;
        }

        public float[] GetVertices()
        {
            return CAM;
        }

        public bool IsDynamic()
        {
            return false;
        }
    }
}
