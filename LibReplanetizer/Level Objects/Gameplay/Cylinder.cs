// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.ComponentModel;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Cylinder : MatrixObject, IRenderable
    {
        public const int ELEMENTSIZE = 0x80;

        [Category("Attributes"), DisplayName("ID")]
        public int id { get; set; }

        static readonly float[] CYLINDER_VERTS = {
            0.0f, 0.0f, 0.0f,
            0.000000f, -1.000000f, -1.000000f,
            0.000000f, -1.000000f, 1.000000f,
            0.500000f, -0.866025f, -1.000000f,
            0.500000f, -0.866025f, 1.000000f,
            0.866025f, -0.500000f, -1.000000f,
            0.866025f, -0.500000f, 1.000000f,
            1.000000f, 0.000000f, -1.000000f,
            1.000000f, 0.000000f, 1.000000f,
            0.866025f, 0.500000f, -1.000000f,
            0.866025f, 0.500000f, 1.000000f,
            0.500000f, 0.866025f, -1.000000f,
            0.500000f, 0.866025f, 1.000000f,
            0.000000f, 1.000000f, -1.000000f,
            0.000000f, 1.000000f, 1.000000f,
            -0.500000f, 0.866026f, -1.000000f,
            -0.500000f, 0.866026f, 1.000000f,
            -0.866025f, 0.500000f, -1.000000f,
            -0.866025f, 0.500000f, 1.000000f,
            -1.000000f, 0.000000f, -1.000000f,
            -1.000000f, 0.000000f, 1.000000f,
            -0.866026f, -0.499999f, -1.000000f,
            -0.866026f, -0.499999f, 1.000000f,
            -0.500001f, -0.866025f, -1.000000f,
            -0.500001f, -0.866025f, 1.000000f
        };

        public static readonly ushort[] CYLINDER_TRIS = {
            2, 3, 1,
            4, 5, 3,
            6, 7, 5,
            8, 9, 7,
            10, 11, 9,
            12, 13, 11,
            14, 15, 13,
            16, 17, 15,
            18, 19, 17,
            20, 21, 19,
            22, 14, 6,
            22, 23, 21,
            24, 1, 23,
            19, 23, 7,
            2, 4, 3,
            4, 6, 5,
            6, 8, 7,
            8, 10, 9,
            10, 12, 11,
            12, 14, 13,
            14, 16, 15,
            16, 18, 17,
            18, 20, 19,
            20, 22, 21,
            6, 4, 2,
            2, 24, 22,
            22, 20, 18,
            18, 16, 14,
            14, 12, 10,
            10, 8, 6,
            6, 2, 22,
            22, 18, 14,
            14, 10, 6,
            22, 24, 23,
            24, 2, 1,
            23, 1, 3,
            3, 5, 7,
            7, 9, 11,
            11, 13, 15,
            15, 17, 19,
            19, 21, 23,
            23, 3, 7,
            7, 11, 15,
            15, 19, 7
        };

        // May need changes
        // Needs to be made renderable
        public Cylinder(byte[] block, int num)
        {
            id = num;
            int offset = num * ELEMENTSIZE;

            Matrix4 transformMatrix = ReadMatrix4(block, offset + 0x00);
            Matrix4 inverseRotationMatrix = ReadMatrix4(block, offset + 0x40);

            modelMatrix = transformMatrix;
            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();

            UpdateTransformMatrix();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public ushort[] GetIndices()
        {
            return CYLINDER_TRIS;
        }

        public float[] GetVertices()
        {
            return CYLINDER_VERTS;
        }

        public bool IsDynamic()
        {
            return false;
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, modelMatrix);
            WriteMatrix4(bytes, 0x40, Matrix4.CreateFromQuaternion(rotation).Inverted());

            return bytes;
        }
    }
}
