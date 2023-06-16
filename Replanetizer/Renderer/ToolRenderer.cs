// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Tools;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class ToolRenderer : Renderer
    {
        private readonly ShaderTable shaderTable;

        public ToolRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        public override void Dispose() => throw new NotImplementedException();
        public override void Include<T>(T obj) => throw new NotImplementedException();
        public override void Include<T>(List<T> list) => throw new NotImplementedException();
        public override void Render(RendererPayload payload)
        {
            if (payload.toolbox.tool == null || payload.selection.Count == 0)
                return;

            GL.Disable(EnableCap.DepthTest);
            shaderTable.colorShader.UseShader();
            shaderTable.colorShader.SetUniform1(UniformName.levelObjectType, (int) RenderedObjectType.Tool);

            if (payload.selection.TryGetOne(out var obj) && obj is Spline spline &&
                payload.toolbox.tool is VertexTranslationTool vertexTranslationTool)
            {
                vertexTranslationTool.Render(spline, payload.camera, shaderTable);
            }
            else
                payload.toolbox.tool.Render(payload.selection, payload.camera, shaderTable);

            GL.Enable(EnableCap.DepthTest);
            GLUtil.CheckGlError("ToolRenderer");
        }
    }
}
