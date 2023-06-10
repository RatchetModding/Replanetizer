// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;

namespace Replanetizer.Renderer
{
    /*
     * A container to store IBO and VBO references for a Model
     */
    public class BufferContainer
    {
        private int ibo = 0;
        private int vbo = 0;

        private int iboLength = 0;
        private int vboLength = 0;

        public BufferContainer() { }

        public static BufferContainer FromRenderable(IRenderable renderable)
        {
            BufferContainer container = new BufferContainer();

            BufferUsageHint hint = BufferUsageHint.StaticDraw;
            if (renderable.IsDynamic())
            {
                hint = BufferUsageHint.DynamicDraw;
            }

            // IBO
            ushort[] iboData = renderable.GetIndices();
            if (iboData.Length > 0)
            {
                GL.GenBuffers(1, out container.ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, container.ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboData.Length * sizeof(ushort), iboData, hint);
                container.iboLength = iboData.Length;
            }

            // VBO
            float[] vboData = renderable.GetVertices();
            if (vboData.Length > 0)
            {
                GL.GenBuffers(1, out container.vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, container.vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboData.Length * sizeof(float), vboData, hint);
                container.vboLength = vboData.Length;
            }

            return container;
        }

        public void Bind()
        {
            // IBO
            if (ibo != 0)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ibo);

            // VBO
            if (vbo != 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
            }
        }

        public int GetIndexBufferLength()
        {
            return iboLength;
        }

        public int GetVertexBufferLength()
        {
            return vboLength;
        }
    }
}
