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
    public class WireframeRenderer : Renderer
    {
        public class WireframeCollection
        {
            public readonly BufferContainer container;
            public readonly List<LevelObject> levelObjects;
            public readonly RenderedObjectType type;
            public readonly bool isSpline;

            public WireframeCollection(BufferContainer container, List<LevelObject> levelObjects, RenderedObjectType type)
            {
                this.container = container;
                this.levelObjects = levelObjects;
                this.type = type;
                this.isSpline = (type == RenderedObjectType.Spline || type == RenderedObjectType.GrindPath);
            }

            public void Bind()
            {
                container.Bind();
            }
        }

        private static readonly Vector4 DEFAULT_COLOR = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private static readonly Vector4 SELECTED_COLOR = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);


        private readonly ShaderTable shaderTable;
        private List<WireframeCollection> wireframes = new List<WireframeCollection>();

        public WireframeRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        private static void VertexAttribPointerSetup()
        {
            GLUtil.ActivateNumberOfVertexAttribArrays(1);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }

        public override void Include<T>(T obj)
        {
            if (obj is LevelObject levelObject && obj is IRenderable renderable)
            {
                BufferContainer container = new BufferContainer(renderable, VertexAttribPointerSetup);
                RenderedObjectType type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(levelObject);

                List<LevelObject> listLevelObjects = new List<LevelObject>();
                listLevelObjects.Add(levelObject);

                wireframes.Add(new WireframeCollection(container, listLevelObjects, type));

                return;
            }

            throw new NotImplementedException();
        }

        public override void Include<T>(List<T> list)
        {
            if (list.Count == 0) return;

            BufferContainer? container = null;
            RenderedObjectType type = RenderedObjectType.Null;
            List<LevelObject> listLevelObjects = new List<LevelObject>();

            switch (list)
            {
                case List<Cuboid> cuboids:
                    container = new BufferContainer(cuboids[0], VertexAttribPointerSetup);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(cuboids[0]);
                    listLevelObjects.AddRange(cuboids);
                    break;
                case List<Sphere> spheres:
                    container = new BufferContainer(spheres[0], VertexAttribPointerSetup);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(spheres[0]);
                    listLevelObjects.AddRange(spheres);
                    break;
                case List<Cylinder> cylinders:
                    container = new BufferContainer(cylinders[0], VertexAttribPointerSetup);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(cylinders[0]);
                    listLevelObjects.AddRange(cylinders);
                    break;
                case List<Pill> pills:
                    container = new BufferContainer(pills[0], VertexAttribPointerSetup);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(pills[0]);
                    listLevelObjects.AddRange(pills);
                    break;
                case List<GameCamera> gameCameras:
                    container = new BufferContainer(gameCameras[0], VertexAttribPointerSetup);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(gameCameras[0]);
                    listLevelObjects.AddRange(gameCameras);
                    break;
                case List<Spline> splines:
                    container = new BufferContainer(splines[0], VertexAttribPointerSetup);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(splines[0]);
                    listLevelObjects.AddRange(splines);
                    break;
            }

            if (container != null)
            {
                wireframes.Add(new WireframeCollection(container, listLevelObjects, type));
                return;
            }

            throw new NotImplementedException();
        }

        public override void Render(RendererPayload payload)
        {
            /*shaderTable.colorShader.UseShader();

            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            shaderTable.colorShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            foreach (WireframeCollection wireframe in wireframes)
            {
                shaderTable.colorShader.SetUniform1(UniformName.levelObjectType, (int) wireframe.type);
                wireframe.Bind();

                if (wireframe.isSpline)
                {
                    foreach (LevelObject obj in wireframe.levelObjects)
                    {
                        shaderTable.colorShader.SetUniform1(UniformName.levelObjectNumber, obj.globalID);
                        shaderTable.colorShader.SetUniformMatrix4(UniformName.modelToWorld, ref obj.modelMatrix);
                        shaderTable.colorShader.SetUniform4(UniformName.incolor, payload.selection.Contains(obj) ? SELECTED_COLOR : DEFAULT_COLOR);
                        GL.DrawArrays(PrimitiveType.LineStrip, 0, wireframe.container.GetVertexBufferLength() / 3);
                    }
                }
                else
                {
                    foreach (LevelObject obj in wireframe.levelObjects)
                    {
                        shaderTable.colorShader.SetUniform1(UniformName.levelObjectNumber, obj.globalID);
                        shaderTable.colorShader.SetUniformMatrix4(UniformName.modelToWorld, ref obj.modelMatrix);
                        shaderTable.colorShader.SetUniform4(UniformName.incolor, payload.selection.Contains(obj) ? SELECTED_COLOR : DEFAULT_COLOR);
                        GL.DrawElements(PrimitiveType.Triangles, wireframe.container.GetIndexBufferLength(), DrawElementsType.UnsignedShort, 0);
                    }
                }
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);*/

            shaderTable.wireframeShader.UseShader();

            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            shaderTable.wireframeShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.wireframeShader.SetUniform2(UniformName.resolution, payload.width, payload.height);

            foreach (WireframeCollection wireframe in wireframes)
            {
                shaderTable.wireframeShader.SetUniform1(UniformName.levelObjectType, (int) wireframe.type);
                wireframe.Bind();

                foreach (LevelObject obj in wireframe.levelObjects)
                {
                    shaderTable.wireframeShader.SetUniform1(UniformName.levelObjectNumber, obj.globalID);
                    shaderTable.wireframeShader.SetUniformMatrix4(UniformName.modelToWorld, ref obj.modelMatrix);
                    shaderTable.wireframeShader.SetUniform4(UniformName.incolor, payload.selection.Contains(obj) ? SELECTED_COLOR : DEFAULT_COLOR);
                    GL.DrawElements(PrimitiveType.Triangles, wireframe.container.GetIndexBufferLength(), DrawElementsType.UnsignedShort, 0);
                }
            }

            GLUtil.CheckGlError("WireframeRenderer");
        }

        public override void Dispose()
        {
        }
    }
}
