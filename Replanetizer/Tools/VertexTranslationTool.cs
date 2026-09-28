// Copyright (C) 2018-2021, The Replanetizer Contributors.
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
    class VertexTranslationTool : SpecialTransformTool
    {
        public override ToolType toolType => ToolType.VertexTranslation;

        public int currentVertex { get; set; }


        public VertexTranslationTool(Toolbox toolbox) : base(toolbox)
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
            BindVao();

            table.colorShader.UseShader();

            table.colorShader.SetUniformMatrix4(UniformName.modelToWorld, ref mat);

            table.colorShader.SetUniform1(UniformName.levelObjectNumber, 0);
            table.colorShader.SetUniform4(UniformName.incolor, 1.0f, 0.0f, 0.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 9, 3);

            table.colorShader.SetUniform1(UniformName.levelObjectNumber, 1);
            table.colorShader.SetUniform4(UniformName.incolor, 0.0f, 1.0f, 0.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 12, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 15, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 18, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 21, 3);

            table.colorShader.SetUniform1(UniformName.levelObjectNumber, 2);
            table.colorShader.SetUniform4(UniformName.incolor, 0.0f, 0.0f, 1.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 24, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 27, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 30, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 33, 3);
        }

        public void Render(Spline spline, Camera camera, ShaderTable table)
        {
            Render(spline.GetVertex(currentVertex), camera, table);
        }

        public override void Reset()
        {
            currentVertex = 0;
        }

        public void Transform(LevelObject obj, Vector3 vec, int vertexIndex)
        {
            if (obj is not Spline spline)
                return;

            spline.TranslateVertex(vertexIndex, vec);
        }

        public void Transform(
            Selection selection, Vector3 direction, Vector3 magnitude, int vertexIndex)
        {
            Vector3 vec = ProcessVec(direction, magnitude);
            if (selection.TryGetOne(out var obj))
                Transform(obj, vec, vertexIndex);
        }

        public void Transform(LevelObject obj, Vector3 vec)
        {
            Transform(obj, vec, currentVertex);
        }

        public void Transform(
            Selection selection, Vector3 direction, Vector3 magnitude)
        {
            Vector3 vec = ProcessVec(direction, magnitude);
            if (selection.TryGetOne(out var obj))
                Transform(obj, vec);
        }
    }
}
