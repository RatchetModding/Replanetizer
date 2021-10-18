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

namespace Replanetizer.Tools
{
    class TranslationTool : BasicTransformTool
    {
        public override ToolType toolType => ToolType.Translation;

        public TranslationTool(Toolbox toolbox) : base(toolbox)
        {
            const float length = 2.0f;

            vb = new[]{
                length / 2,     - length / 3,   0,
                length / 2,     length / 3,     0,
                length,         0,              0,

                -length / 2,     - length / 3,   0,
                -length / 2,     length / 3,     0,
                -length,         0,              0,


                length / 2,     0,   - length / 3,
                length / 2,     0,     length / 3,
                length,         0,              0,

                -length / 2,     0,   - length / 3,
                -length / 2,     0,     length / 3,
                -length,         0,              0,


                -length / 3,    length / 2,     0,
                length / 3,     length / 2,     0,
                0,              length,         0,

                -length / 3,    -length / 2,    0,
                length / 3,     -length / 2,    0,
                0,              -length,        0,

                0,    length / 2,     -length / 3,
                0,     length / 2,     length / 3,
                0,              length,         0,

                0,    -length / 2,    -length / 3,
                0,     -length / 2,    length / 3,
                0,              -length,        0,


                -length / 3,    0,              -length / 2,
                length / 3,     0,              -length / 2,
                0,              0,              -length,

                -length / 3,    0,              length / 2,
                length / 3,     0,              length / 2,
                0,              0,              length,

                0,    -length / 3,              -length / 2,
                0,     length / 3,              -length / 2,
                0,              0,              -length,

                0,    -length / 3,              length / 2,
                0,     length / 3,              length / 2,
                0,              0,              length,
            };
        }

        public override void Render(Vector3 position, LevelFrame frame)
        {
            GetVbo();

            var modelMatrix = GetModelMatrix(position, frame);
            GL.UniformMatrix4(frame.shaderIDTable.uniformModelToWorldMatrix, false, ref modelMatrix);

            GL.Uniform1(frame.shaderIDTable.uniformColorLevelObjectNumber, 0);
            GL.Uniform4(frame.shaderIDTable.uniformColor, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 9, 3);

            GL.Uniform1(frame.shaderIDTable.uniformColorLevelObjectNumber, 1);
            GL.Uniform4(frame.shaderIDTable.uniformColor, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 12, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 15, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 18, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 21, 3);

            GL.Uniform1(frame.shaderIDTable.uniformColorLevelObjectNumber, 2);
            GL.Uniform4(frame.shaderIDTable.uniformColor, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 24, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 27, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 30, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 33, 3);
        }

        public override void Transform(LevelObject obj, Vector3 vec, Vector3 pivot)
        {
            obj.Translate(vec);
        }
    }
}
