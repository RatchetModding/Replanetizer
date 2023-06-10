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

            public WireframeCollection(BufferContainer container, List<LevelObject> levelObjects, RenderedObjectType type)
            {
                this.container = container;
                this.levelObjects = levelObjects;
                this.type = type;
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

        public override void Include<T>(T obj)
        {
            if (obj is LevelObject levelObject && obj is IRenderable renderable)
            {
                BufferContainer container = BufferContainer.FromRenderable(renderable);
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
                    BufferContainer.FromRenderable(cuboids[0]);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(cuboids[0]);
                    listLevelObjects.AddRange(cuboids);
                    return;
                case List<Sphere> spheres:
                    BufferContainer.FromRenderable(spheres[0]);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(spheres[0]);
                    listLevelObjects.AddRange(spheres);
                    return;
                case List<Cylinder> cylinders:
                    BufferContainer.FromRenderable(cylinders[0]);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(cylinders[0]);
                    listLevelObjects.AddRange(cylinders);
                    return;
                case List<Pill> pills:
                    BufferContainer.FromRenderable(pills[0]);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(pills[0]);
                    listLevelObjects.AddRange(pills);
                    return;
                case List<GameCamera> gameCameras:
                    BufferContainer.FromRenderable(gameCameras[0]);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(gameCameras[0]);
                    listLevelObjects.AddRange(gameCameras);
                    return;
                case List<Spline> splines:
                    BufferContainer.FromRenderable(splines[0]);
                    type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(splines[0]);
                    listLevelObjects.AddRange(splines);
                    return;
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
            shaderTable.colorShader.UseShader();

            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            shaderTable.colorShader.SetUniformMatrix4("worldToView", false, ref worldToView);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            foreach (WireframeCollection wireframe in wireframes)
            {
                shaderTable.colorShader.SetUniform1("levelObjectType", (int) wireframe.type);
                wireframe.container.Bind();
                GLState.ChangeNumberOfVertexAttribArrays(1);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                foreach (LevelObject obj in wireframe.levelObjects)
                {
                    shaderTable.colorShader.SetUniform1("levelObjectNumber", obj.globalID);
                    shaderTable.colorShader.SetUniformMatrix4("modelToWorld", false, ref obj.modelMatrix);
                    shaderTable.colorShader.SetUniform4("incolor", payload.selection.Contains(obj) ? SELECTED_COLOR : DEFAULT_COLOR);
                    GL.DrawElements(PrimitiveType.Triangles, Cuboid.CUBE_ELEMENTS.Length, DrawElementsType.UnsignedShort, 0);
                }
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public override void Dispose()
        {
        }
    }
}
