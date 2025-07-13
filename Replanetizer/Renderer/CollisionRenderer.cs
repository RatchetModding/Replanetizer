// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class CollisionRenderer : Renderer
    {
        private readonly ShaderTable shaderTable;
        private List<int> ibos = new List<int>();
        private List<int> vbos = new List<int>();
        private List<int> vaos = new List<int>();
        private List<int> indexCount = new List<int>();
        private int numCollisions = 0;

        public CollisionRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        public override void Include<T>(T obj)
        {
            if (obj is Collision collision)
            {
                uint[] indexBuffer = collision.indBuff;
                float[] vertexBuffer = collision.vertexBuffer;

                int vao;
                GL.GenVertexArrays(1, out vao);
                GL.BindVertexArray(vao);

                vaos.Add(vao);

                int vbo;
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticDraw);

                vbos.Add(vbo);

                int ibo;
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indexBuffer.Length * sizeof(int), indexBuffer, BufferUsageHint.StaticDraw);

                GLUtil.ActivateNumberOfVertexAttribArrays(2);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);

                ibos.Add(ibo);
                indexCount.Add(indexBuffer.Length);

                numCollisions++;

                return;
            }

            throw new NotImplementedException();
        }

        public override void Include<T>(List<T> list)
        {
            if (list.Count == 0) return;

            if (list is List<Collision> collisionChunks)
            {
                for (int i = 0; i < collisionChunks.Count; i++)
                {
                    Include(collisionChunks[i]);
                }

                return;
            }

            throw new NotImplementedException();
        }

        public override void Render(RendererPayload payload)
        {
            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            Matrix4 modelToWorld = Matrix4.Identity;

            shaderTable.colorShader.UseShader();
            shaderTable.colorShader.SetUniform1(UniformName.levelObjectType, (int) RenderedObjectType.Null);
            shaderTable.colorShader.SetUniform4(UniformName.incolor, 1.0f, 1.0f, 1.0f, 1.0f);
            shaderTable.colorShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.colorShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);

            shaderTable.collisionShader.UseShader();
            shaderTable.collisionShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);

            shaderTable.colorShader.UseShader();
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            for (int i = 0; i < numCollisions; i++)
            {
                if (!payload.visibility.chunks[i]) continue;

                GL.BindVertexArray(vaos[i]);
                GL.DrawElements(PrimitiveType.Triangles, indexCount[i], DrawElementsType.UnsignedInt, 0);
            }

            shaderTable.collisionShader.UseShader();
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

            for (int i = 0; i < numCollisions; i++)
            {
                if (!payload.visibility.chunks[i]) continue;

                GL.BindVertexArray(vaos[i]);
                GL.DrawElements(PrimitiveType.Triangles, indexCount[i], DrawElementsType.UnsignedInt, 0);
            }

            GLUtil.CheckGlError("CollisionRenderer");
        }

        public override void Dispose()
        {
            for (int i = 0; i < numCollisions; i++)
            {
                GL.DeleteBuffer(ibos[i]);
                GL.DeleteBuffer(vbos[i]);
                GL.DeleteVertexArray(vaos[i]);
            }
        }
    }
}
