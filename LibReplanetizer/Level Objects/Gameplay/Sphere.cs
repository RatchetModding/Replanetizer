// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Sphere : MatrixObject, IRenderable
    {
        public const int ELEMENTSIZE = 0x80;

        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;

        // Sphere with 80 verts made in Blender
        // the first vertex is a dummy so I didnt have to apply -1 to the triangle indices
        static readonly float[] SPHERE_VERTS = {
            0.0f, 0.0f, 0.0f,
            0.000000f, -1.000000f, 0.000000f,
            0.723607f, -0.447220f, 0.525725f,
            -0.276388f, -0.447220f, 0.850649f,
            -0.894426f, -0.447216f, 0.000000f,
            -0.276388f, -0.447220f, -0.850649f,
            0.723607f, -0.447220f, -0.525725f,
            0.276388f, 0.447220f, 0.850649f,
            -0.723607f, 0.447220f, 0.525725f,
            -0.723607f, 0.447220f, -0.525725f,
            0.276388f, 0.447220f, -0.850649f,
            0.894426f, 0.447216f, 0.000000f,
            0.000000f, 1.000000f, 0.000000f,
            -0.162456f, -0.850654f, 0.499995f,
            0.425323f, -0.850654f, 0.309011f,
            0.262869f, -0.525738f, 0.809012f,
            0.850648f, -0.525736f, 0.000000f,
            0.425323f, -0.850654f, -0.309011f,
            -0.525730f, -0.850652f, 0.000000f,
            -0.688189f, -0.525736f, 0.499997f,
            -0.162456f, -0.850654f, -0.499995f,
            -0.688189f, -0.525736f, -0.499997f,
            0.262869f, -0.525738f, -0.809012f,
            0.951058f, 0.000000f, 0.309013f,
            0.951058f, 0.000000f, -0.309013f,
            0.000000f, 0.000000f, 1.000000f,
            0.587786f, 0.000000f, 0.809017f,
            -0.951058f, 0.000000f, 0.309013f,
            -0.587786f, 0.000000f, 0.809017f,
            -0.587786f, 0.000000f, -0.809017f,
            -0.951058f, 0.000000f, -0.309013f,
            0.587786f, 0.000000f, -0.809017f,
            0.000000f, 0.000000f, -1.000000f,
            0.688189f, 0.525736f, 0.499997f,
            -0.262869f, 0.525738f, 0.809012f,
            -0.850648f, 0.525736f, 0.000000f,
            -0.262869f, 0.525738f, -0.809012f,
            0.688189f, 0.525736f, -0.499997f,
            0.162456f, 0.850654f, 0.499995f,
            0.525730f, 0.850652f, 0.000000f,
            -0.425323f, 0.850654f, 0.309011f,
            -0.425323f, 0.850654f, -0.309011f,
            0.162456f, 0.850654f, -0.499995f
        };

        public static readonly ushort[] SPHERE_TRIS = {
            1, 14, 13,
            2, 14, 16,
            1, 13, 18,
            1, 18, 20,
            1, 20, 17,
            2, 16, 23,
            3, 15, 25,
            4, 19, 27,
            5, 21, 29,
            6, 22, 31,
            2, 23, 26,
            3, 25, 28,
            4, 27, 30,
            5, 29, 32,
            6, 31, 24,
            7, 33, 38,
            8, 34, 40,
            9, 35, 41,
            10, 36, 42,
            11, 37, 39,
            39, 42, 12,
            39, 37, 42,
            37, 10, 42,
            42, 41, 12,
            42, 36, 41,
            36, 9, 41,
            41, 40, 12,
            41, 35, 40,
            35, 8, 40,
            40, 38, 12,
            40, 34, 38,
            34, 7, 38,
            38, 39, 12,
            38, 33, 39,
            33, 11, 39,
            24, 37, 11,
            24, 31, 37,
            31, 10, 37,
            32, 36, 10,
            32, 29, 36,
            29, 9, 36,
            30, 35, 9,
            30, 27, 35,
            27, 8, 35,
            28, 34, 8,
            28, 25, 34,
            25, 7, 34,
            26, 33, 7,
            26, 23, 33,
            23, 11, 33,
            31, 32, 10,
            31, 22, 32,
            22, 5, 32,
            29, 30, 9,
            29, 21, 30,
            21, 4, 30,
            27, 28, 8,
            27, 19, 28,
            19, 3, 28,
            25, 26, 7,
            25, 15, 26,
            15, 2, 26,
            23, 24, 11,
            23, 16, 24,
            16, 6, 24,
            17, 22, 6,
            17, 20, 22,
            20, 5, 22,
            20, 21, 5,
            20, 18, 21,
            18, 4, 21,
            18, 19, 4,
            18, 13, 19,
            13, 3, 19,
            16, 17, 6,
            16, 14, 17,
            14, 1, 17,
            13, 15, 3,
            13, 14, 15,
            14, 2, 15
        };

        public Sphere(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            mat1 = ReadMatrix4(block, offset + 0x00);
            mat2 = ReadMatrix4(block, offset + 0x40);

            modelMatrix = mat1;
            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();

            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, mat1);
            WriteMatrix4(bytes, 0x40, mat2);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public ushort[] GetIndices()
        {
            return SPHERE_TRIS;
        }

        public float[] GetVertices()
        {
            return SPHERE_VERTS;
        }

        public bool IsDynamic()
        {
            return false;
        }
    }
}
