// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Cuboid : MatrixObject, IRenderable
    {
        public const int ELEMENTSIZE = 0x80;

        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;


        static readonly float[] CUBE = {
            -1.0f, -1.0f,  1.0f,
            1.0f, -1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            // back
            -1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f
        };

        public static readonly ushort[] CUBE_ELEMENTS = {
            0, 1, 2,
            2, 3, 0,
            1, 5, 6,
            6, 2, 1,
            7, 6, 5,
            5, 4, 7,
            4, 0, 3,
            3, 7, 4,
            4, 5, 1,
            1, 0, 4,
            3, 2, 6,
            6, 7, 3
        };

        public Cuboid(byte[] block, int index)
        {
            id = index;
            int offset = index * ELEMENTSIZE;

            mat1 = ReadMatrix4(block, offset + 0x00);
            mat2 = ReadMatrix4(block, offset + 0x40);

            modelMatrix = mat1;
            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();

            UpdateTransformMatrix();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[0x80];

            WriteMatrix4(bytes, 0x00, mat1);
            WriteMatrix4(bytes, 0x40, mat2);

            return bytes;
        }

        public ushort[] GetIndices()
        {
            return CUBE_ELEMENTS;
        }

        public float[] GetVertices()
        {
            return CUBE;
        }

        public bool IsDynamic()
        {
            return false;
        }
    }
}
