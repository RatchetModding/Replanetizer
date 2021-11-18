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
    class VertexTranslationTool : SpecialTransformTool
    {
        public override ToolType toolType => ToolType.VertexTranslation;

        public int currentVertex { get; set; }

        public VertexTranslationTool(Toolbox toolbox) : base(toolbox)
        {
            const float length = 0.7f;

            vb = new[]{
                -length,    0,          0,
                length,     0,          0,
                0,          -length,    0,
                0,          length,     0,
                0,          0,          -length,
                0,          0,          length,
            };
        }

        public override void Render(Matrix4 mat, LevelFrame frame)
        {
            GetVbo();

            GL.UniformMatrix4(frame.shaderIDTable.uniformModelToWorldMatrix, false, ref mat);

            GL.Uniform1(frame.shaderIDTable.uniformColorLevelObjectNumber, 0);
            GL.Uniform4(frame.shaderIDTable.uniformColor, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 0, 2);

            GL.Uniform1(frame.shaderIDTable.uniformColorLevelObjectNumber, 1);
            GL.Uniform4(frame.shaderIDTable.uniformColor, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 2, 2);

            GL.Uniform1(frame.shaderIDTable.uniformColorLevelObjectNumber, 2);
            GL.Uniform4(frame.shaderIDTable.uniformColor, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 4, 2);
        }

        public void Render(Spline spline, LevelFrame frame)
        {
            Render(spline.GetVertex(currentVertex), frame);
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
