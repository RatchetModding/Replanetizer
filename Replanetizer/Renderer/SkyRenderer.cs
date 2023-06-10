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
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Tools;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class SkyRenderer : Renderer
    {
        private SkyboxModel? sky;
        private BufferContainer? container;
        private readonly ShaderTable shaderTable;
        private List<Texture> textures;
        private Dictionary<Texture, int> textureIds;

        public SkyRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, int> textureIds)
        {
            this.shaderTable = shaderTable;
            this.textureIds = textureIds;
            this.textures = textures;
        }

        public override void Include<T>(T obj)
        {
            if (obj is SkyboxModel sky)
            {
                this.sky = sky;
                container = BufferContainer.FromRenderable(sky);
                return;
            }
            throw new NotImplementedException();
        }

        public override void Include<T>(List<T> list) => throw new NotImplementedException();

        public override void Render(RendererPayload payload)
        {
            if (sky == null || container == null) return;

            shaderTable.skyShader.UseShader();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.Disable(EnableCap.DepthTest);

            Matrix4 mvp = payload.camera.GetWorldViewMatrix();
            shaderTable.skyShader.SetUniformMatrix4("worldToView", false, ref mvp);
            container.Bind();
            GLState.ChangeNumberOfVertexAttribArrays(3);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 6, 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 6, sizeof(float) * 3);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 6, sizeof(float) * 5);

            for (int i = 0; i < sky.textureConfig.Count; i++)
            {
                TextureConfig conf = sky.textureConfig[i];
                shaderTable.skyShader.SetUniform1("texAvailable", (conf.id > 0) ? 1.0f : 0.0f);
                GL.BindTexture(TextureTarget.Texture2D, (conf.id > 0) ? textureIds[textures[conf.id]] : 0);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }

        public override void Dispose()
        {

        }
    }
}
