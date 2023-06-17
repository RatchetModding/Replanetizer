// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class DirectionalLightsBuffer : IDisposable
    {
        private const int ALLOCATED_LIGHTS = 20;

        private float[][] data;
        private int buffer;

        public DirectionalLightsBuffer(ShaderTable shaderTable)
        {
            int loc = GL.GetUniformBlockIndex(shaderTable.meshShader.program, "lights");
            GL.UniformBlockBinding(shaderTable.meshShader.program, loc, 0);

            buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, buffer);
            GL.BufferData(BufferTarget.UniformBuffer, sizeof(float) * 16 * ALLOCATED_LIGHTS, IntPtr.Zero, BufferUsageHint.StaticRead);

            data = new float[ALLOCATED_LIGHTS][];

            for (int i = 0; i < ALLOCATED_LIGHTS; i++)
            {
                data[i] = new float[16];

                // Upload black lights, all unused ones will remain black.
                GL.BufferSubData(BufferTarget.UniformBuffer, new IntPtr(sizeof(float) * 16 * i), sizeof(float) * 16, data[i]);
            }
        }

        public void Update(List<Light>? lights)
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, buffer);

            if (lights == null) return;

            int lightSize = sizeof(float) * 16;

            for (int i = 0; i < lights.Count; i++)
            {
                data[i][0] = lights[i].color1.X;
                data[i][1] = lights[i].color1.Y;
                data[i][2] = lights[i].color1.Z;
                data[i][3] = lights[i].color1.W;
                data[i][4] = lights[i].direction1.X;
                data[i][5] = lights[i].direction1.Y;
                data[i][6] = lights[i].direction1.Z;
                data[i][7] = lights[i].direction1.W;
                data[i][8] = lights[i].color2.X;
                data[i][9] = lights[i].color2.Y;
                data[i][10] = lights[i].color2.Z;
                data[i][11] = lights[i].color2.W;
                data[i][12] = lights[i].direction2.X;
                data[i][13] = lights[i].direction2.Y;
                data[i][14] = lights[i].direction2.Z;
                data[i][15] = lights[i].direction2.W;
                GL.BufferSubData(BufferTarget.UniformBuffer, new IntPtr(sizeof(float) * 16 * i), lightSize, data[i]);
            }
        }

        public void Bind()
        {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, buffer);
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
