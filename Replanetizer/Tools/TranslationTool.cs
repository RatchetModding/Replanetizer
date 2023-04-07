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
using Replanetizer.Utils;

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

        public override void Render(Matrix4 mat, LevelFrame frame)
        {
            GetVbo();

            GL.UniformMatrix4(frame.shaderIDTable.uniformModelToWorldMatrix, false, ref mat);

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

        /// <summary>
        /// Computes the intersection of the lines x + a * dx and y + b * dy. The returned float f is such that x + f * dx is the intersection.
        /// The function returns 0.0f is no intersection was found.
        /// </summary>
        private float getLineIntersectionDist(Vector3 x, Vector3 dx, Vector3 y, Vector3 dy)
        {
            Vector3 g = y - x;
            Vector3 h = Vector3.Cross(dy, g);
            Vector3 k = Vector3.Cross(dy, dx);

            float ha = h.Length;
            float ka = k.Length;

            if (ha == 0.0f || ka == 0.0f)
            {
                return 0.0f;
            }

            float sign = (Vector3.Dot(h, k) >= 0.0f) ? 1.0f : -1.0f;

            return (ha / ka) * sign;
        }

        public override void Transform(LevelObject obj, Vector3 pivot, TransformToolData data)
        {
            Matrix4 mat = obj.modelMatrix;

            if (toolbox.transformSpace == TransformSpace.Global)
            {
                float startDist = getLineIntersectionDist(data.cameraPos, data.mousePrevDir, obj.position, data.axisDir);
                Vector3 startPos = data.cameraPos + startDist * data.mousePrevDir;

                float finalDist = getLineIntersectionDist(startPos, data.axisDir, data.cameraPos, data.mouseCurrDir);

                Matrix4 trans = Matrix4.CreateTranslation(finalDist * data.axisDir);
                mat = mat * trans;
            }
            else if (toolbox.transformSpace == TransformSpace.Local)
            {
                Vector3 aDir = (mat.Inverted() * new Vector4(data.axisDir, 0.0f)).Xyz;

                float startDist = getLineIntersectionDist(data.cameraPos, data.mousePrevDir, obj.position, aDir);
                Vector3 startPos = data.cameraPos + startDist * data.mousePrevDir;

                float finalDist = getLineIntersectionDist(startPos, aDir, data.cameraPos, data.mouseCurrDir);

                Matrix4 trans = Matrix4.CreateTranslation(finalDist * aDir);
                mat = mat * trans;
            }

            obj.SetFromMatrix(mat);
        }
    }
}
