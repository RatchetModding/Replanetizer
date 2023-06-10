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
    public class BillboardRenderer : Renderer
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

        private readonly ShaderTable shaderTable;
        private int ibo;
        private int vbo;
        private List<LevelObject> objects = new List<LevelObject>();
        private List<RenderedObjectType> types = new List<RenderedObjectType>();

        public BillboardRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;

            int iboLength = INDICES.Length * sizeof(ushort);
            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, iboLength, INDICES, BufferUsageHint.StaticDraw);

            int vboLength = VERTICES.Length * sizeof(float);
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vboLength, VERTICES, BufferUsageHint.StaticDraw);
        }

        public override void Include<T>(T obj)
        {
            if (obj is LevelObject levelObject)
            {
                List<LevelObject> list = new List<LevelObject>();
                list.Add(levelObject);

                Include(list);
            }
        }

        public override void Include<T>(List<T> list)
        {
            if (list is List<LevelObject> levelObjectList)
            {
                foreach (LevelObject obj in levelObjectList)
                {
                    objects.Add(obj);
                    types.Add(RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(obj));
                }
            }
        }

        public override void Render(RendererPayload payload)
        {
            shaderTable.billboardShader.UseShader();

            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            Vector3 right = new Vector3(worldToView[0, 0], worldToView[1, 0], worldToView[2, 0]).Normalized();
            Vector3 up = new Vector3(worldToView[0, 1], worldToView[1, 1], worldToView[2, 1]).Normalized();
            shaderTable.billboardShader.SetUniformMatrix4("worldToView", false, ref worldToView);
            shaderTable.billboardShader.SetUniform3("right", right.X, right.Y, right.Z);
            shaderTable.billboardShader.SetUniform3("up", up.X, up.Y, up.Z);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GLState.ChangeNumberOfVertexAttribArrays(1);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);

            for (int i = 0; i < objects.Count; i++)
            {
                LevelObject obj = objects[i];
                shaderTable.billboardShader.SetUniform3("position", obj.position.X, obj.position.Y, obj.position.Z);
                shaderTable.billboardShader.SetUniform1("levelObjectNumber", i);
                shaderTable.billboardShader.SetUniform1("levelObjectType", (int) types[i]);
                GL.DrawElements(PrimitiveType.Triangles, INDICES.Length, DrawElementsType.UnsignedShort, 0);
            }
        }

        public override void Dispose()
        {
            GL.DeleteBuffer(ibo);
            GL.DeleteBuffer(vbo);
        }
    }
}
