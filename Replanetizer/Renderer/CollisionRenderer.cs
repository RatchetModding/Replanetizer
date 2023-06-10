// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Drawing;
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
        private List<int> indexCount = new List<int>();
        private int numCollisions = 0;

        public CollisionRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        public override void Include<T>(T obj) => throw new NotImplementedException();

        public override void Include<T>(List<T> list)
        {
            if (list.Count == 0) return;

            if (list is List<Collision> collisionChunks)
            {
                for (int i = 0; i < collisionChunks.Count; i++)
                {
                    uint[] indexBuffer = collisionChunks[i].indBuff;
                    float[] vertexBuffer = collisionChunks[i].vertexBuffer;

                    int vbo;
                    GL.GenBuffers(1, out vbo);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticDraw);

                    vbos.Add(vbo);

                    int ibo;
                    GL.GenBuffers(1, out ibo);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, indexBuffer.Length * sizeof(int), indexBuffer, BufferUsageHint.StaticDraw);

                    ibos.Add(ibo);
                    indexCount.Add(indexBuffer.Length);

                    numCollisions++;
                }
            }
        }

        public override void Render(RendererPayload payload)
        {
            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            Matrix4 modelToWorld = Matrix4.Identity;

            shaderTable.colorShader.UseShader();
            shaderTable.colorShader.SetUniform1("levelObjectType", (int) RenderedObjectType.Null);
            shaderTable.colorShader.SetUniform4("incolor", 1.0f, 1.0f, 1.0f, 1.0f);
            shaderTable.colorShader.SetUniformMatrix4("worldToView", false, ref worldToView);
            shaderTable.colorShader.SetUniformMatrix4("modelToWorld", false, ref modelToWorld);

            shaderTable.collisionShader.UseShader();
            shaderTable.collisionShader.SetUniformMatrix4("worldToView", false, ref worldToView);

            GLState.ChangeNumberOfVertexAttribArrays(2);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);

            for (int i = 0; i < numCollisions; i++)
            {
                if (!payload.visibility.chunks[i]) continue;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibos[i]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbos[i]);

                shaderTable.collisionShader.UseShader();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.DrawElements(PrimitiveType.Triangles, indexCount[i], DrawElementsType.UnsignedInt, 0);

                shaderTable.colorShader.UseShader();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, indexCount[i], DrawElementsType.UnsignedInt, 0);
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < numCollisions; i++)
            {
                GL.DeleteBuffer(ibos[i]);
                GL.DeleteBuffer(vbos[i]);
            }
        }
    }
}
