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

namespace Replanetizer.Utils
{
    public class BillboardRenderer : IDisposable
    {
        private static float[] VERTICES = new float[] {
            -1.0f, -1.0f,
            1.0f, -1.0f,
            -1.0f, 1.0f,
            1.0f, 1.0f
        };

        private static readonly ushort[] INDICES = {
            0, 1, 2,
            1, 2, 3
        };

        private ShaderIDTable shaders;
        private int ibo;
        private int vbo;

        public BillboardRenderer(ShaderIDTable table)
        {
            shaders = table;

            int iboLength = INDICES.Length * sizeof(ushort);
            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, iboLength, INDICES, BufferUsageHint.StaticDraw);

            int vboLength = VERTICES.Length * sizeof(float);
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vboLength, VERTICES, BufferUsageHint.StaticDraw);
        }

        public void UpdateCameraMatrix(ref Matrix4 view)
        {
            Vector3 right = new Vector3(view[0, 0], view[1, 0], view[2, 0]).Normalized();
            Vector3 up = new Vector3(view[0, 1], view[1, 1], view[2, 1]).Normalized();

            int[] previousProgram = new int[1];
            GL.GetInteger(GetPName.CurrentProgram, previousProgram);
            GL.UseProgram(shaders.shaderBillboard);
            GL.UniformMatrix4(shaders.uniformBillboardWorldToViewMatrix, false, ref view);
            GL.Uniform3(shaders.uniformBillboardRightBase, right.X, right.Y, right.Z);
            GL.Uniform3(shaders.uniformBillboardUpBase, up.X, up.Y, up.Z);
            GL.UseProgram(previousProgram[0]);
        }

        public void SetObjectType(RenderedObjectType type)
        {
            int[] previousProgram = new int[1];
            GL.GetInteger(GetPName.CurrentProgram, previousProgram);
            GL.UseProgram(shaders.shaderBillboard);
            GL.Uniform1(shaders.uniformBillboardLevelObjectType, (int) type);
            GL.UseProgram(previousProgram[0]);
        }

        public void RenderObject(LevelObject obj, int number)
        {
            GL.UseProgram(shaders.shaderBillboard);

            GL.Uniform3(shaders.uniformBillboardPosition, obj.position.X, obj.position.Y, obj.position.Z);
            GL.Uniform1(shaders.uniformBillboardLevelObjectNumber, number);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
            GL.DrawElements(PrimitiveType.Triangles, INDICES.Length, DrawElementsType.UnsignedShort, 0);
        }

        public void RenderObjects<T>(List<T> list, RenderedObjectType type) where T : LevelObject
        {
            SetObjectType(type);

            for (int i = 0; i < list.Count; i++)
            {
                RenderObject(list[i], i);
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ibo);
            GL.DeleteBuffer(vbo);
        }
    }
}
