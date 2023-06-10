﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Frames;
using Replanetizer.Renderer;
using Replanetizer.Utils;

namespace Replanetizer.Tools
{
    class TranslationTool : BasicTransformTool
    {
        public override ToolType toolType => ToolType.Translation;

        public TranslationTool(Toolbox toolbox) : base(toolbox)
        {
            const float length = 2.0f;
            const float thickness = length / 2.0f;
            const float thickness2 = length / 3.0f;

            vb = new[]{
                thickness,     -thickness2,   0,
                thickness,     thickness2,     0,
                length,         0,              0,

                -thickness,     - thickness2,   0,
                -thickness,     thickness2,     0,
                -length,         0,              0,


                thickness,     0,   - thickness2,
                thickness,     0,     thickness2,
                length,         0,              0,

                -thickness,     0,   - thickness2,
                -thickness,     0,     thickness2,
                -length,         0,              0,


                -thickness2,    thickness,     0,
                thickness2,     thickness,     0,
                0,              length,         0,

                -thickness2,    -thickness,    0,
                thickness2,     -thickness,    0,
                0,              -length,        0,

                0,    thickness,     -thickness2,
                0,     thickness,     thickness2,
                0,              length,         0,

                0,    -thickness,    -thickness2,
                0,     -thickness,    thickness2,
                0,              -length,        0,


                -thickness2,    0,              -thickness,
                thickness2,     0,              -thickness,
                0,              0,              -length,

                -thickness2,    0,              thickness,
                thickness2,     0,              thickness,
                0,              0,              length,

                0,    -thickness2,              -thickness,
                0,     thickness2,              -thickness,
                0,              0,              -length,

                0,    -thickness2,              thickness,
                0,     thickness2,              thickness,
                0,              0,              length,
            };
        }

        public override void Render(Matrix4 mat, ShaderTable table)
        {
            table.colorShader.UseShader();

            GetVbo();

            table.colorShader.SetUniformMatrix4("modelToWorld", false, ref mat);

            table.colorShader.SetUniform1("levelObjectNumber", 0);
            table.colorShader.SetUniform4("incolor", 1.0f, 0.0f, 0.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 9, 3);

            table.colorShader.SetUniform1("levelObjectNumber", 1);
            table.colorShader.SetUniform4("incolor", 0.0f, 1.0f, 0.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 12, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 15, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 18, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 21, 3);

            table.colorShader.SetUniform1("levelObjectNumber", 2);
            table.colorShader.SetUniform4("incolor", 0.0f, 0.0f, 1.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 24, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 27, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 30, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 33, 3);
        }

        public override void Transform(LevelObject obj, Vector3 pivot, TransformToolData data)
        {
            Matrix4 mat = obj.modelMatrix;

            if (toolbox.transformSpace == TransformSpace.Global)
            {
                float startDist = getLineIntersectionDist(data.cameraPos, data.mousePrevDir, pivot, data.axisDir);
                Vector3 startPos = data.cameraPos + startDist * data.mousePrevDir;

                float finalDist = getLineIntersectionDist(startPos, data.axisDir, data.cameraPos, data.mouseCurrDir);

                Matrix4 trans = Matrix4.CreateTranslation(finalDist * data.axisDir);
                mat = mat * trans;
            }
            else if (toolbox.transformSpace == TransformSpace.Local)
            {
                Vector3 aDir = (mat.Inverted() * new Vector4(data.axisDir, 0.0f)).Xyz;

                float startDist = getLineIntersectionDist(data.cameraPos, data.mousePrevDir, pivot, aDir);
                Vector3 startPos = data.cameraPos + startDist * data.mousePrevDir;

                float finalDist = getLineIntersectionDist(startPos, aDir, data.cameraPos, data.mouseCurrDir);

                Matrix4 trans = Matrix4.CreateTranslation(finalDist * aDir);
                mat = mat * trans;
            }

            obj.SetFromMatrix(mat);
        }
    }
}
