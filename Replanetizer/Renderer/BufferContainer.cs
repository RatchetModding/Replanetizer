// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;

namespace Replanetizer.Renderer
{
    /*
     * A container to store IBO and VBO references for a Model
     */
    public class BufferContainer : IDisposable
    {
        private int ibo = 0;
        private int vbo = 0;
        private int vao = 0;

        private int iboLength = 0;
        private int vboLength = 0;

        public BufferContainer(IRenderable renderable, Action action)
        {
            BufferUsageHint hint = BufferUsageHint.StaticDraw;
            if (renderable.IsDynamic())
            {
                hint = BufferUsageHint.DynamicDraw;
            }

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // IBO
            ushort[] iboData = renderable.GetIndices();
            if (iboData.Length > 0)
            {
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboData.Length * sizeof(ushort), iboData, hint);
                iboLength = iboData.Length;
            }

            // VBO
            float[] vboData = renderable.GetVertices();
            if (vboData.Length > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboData.Length * sizeof(float), vboData, hint);
                vboLength = vboData.Length;
            }

            action();
        }

        public void Bind()
        {
            if (vao != 0)
                GL.BindVertexArray(vao);
        }

        public int GetIndexBufferLength()
        {
            return iboLength;
        }

        public int GetVertexBufferLength()
        {
            return vboLength;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ibo);
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }
    }
}
