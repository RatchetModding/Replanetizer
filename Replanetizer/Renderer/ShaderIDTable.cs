// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;

namespace Replanetizer.Renderer
{
    public class ShaderTable
    {
        public Shader meshShader;
        public Shader colorShader;
        public Shader skyShader;
        public Shader billboardShader;
        public Shader collisionShader;

        public ShaderTable(string directory)
        {
            meshShader = Shader.GetShaderFromFiles("MeshShader", Path.Join(directory, "vs.glsl"), Path.Join(directory, "fs.glsl"));
            colorShader = Shader.GetShaderFromFiles("colorShader", Path.Join(directory, "colorshadervs.glsl"), Path.Join(directory, "colorshaderfs.glsl"));
            skyShader = Shader.GetShaderFromFiles("skyShader", Path.Join(directory, "skyvs.glsl"), Path.Join(directory, "skyfs.glsl"));
            billboardShader = Shader.GetShaderFromFiles("billboardShader", Path.Join(directory, "billboardvs.glsl"), Path.Join(directory, "billboardfs.glsl"));
            collisionShader = Shader.GetShaderFromFiles("collisionShader", Path.Join(directory, "collisionshadervs.glsl"), Path.Join(directory, "collisionshaderfs.glsl"));
        }
    }
}
