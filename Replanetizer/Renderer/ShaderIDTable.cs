// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

namespace Replanetizer.Renderer
{
    public class ShaderTable
    {
        public Shader meshShader;
        public Shader colorShader;
        public Shader skyShader;
        public Shader billboardShader;
        public Shader collisionShader;

        public ShaderTable(string directory = "")
        {
            meshShader = new Shader("MeshShader", directory + "vs.glsl", directory + "fs.glsl");
            colorShader = new Shader("colorShader", directory + "colorshadervs.glsl", directory + "colorshaderfs.glsl");
            skyShader = new Shader("skyShader", directory + "skyvs.glsl", directory + "skyfs.glsl");
            billboardShader = new Shader("billboardShader", directory + "billboardvs.glsl", directory + "billboardfs.glsl");
            collisionShader = new Shader("collisionShader", directory + "collisionshadervs.glsl", directory + "collisionshaderfs.glsl");
        }
    }
}
