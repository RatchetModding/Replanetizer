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
    class RotationTool : BasicTransformTool
    {
        public override ToolType toolType => ToolType.Rotation;

        public RotationTool(Toolbox toolbox) : base(toolbox)
        {
            const float length = 1.5f;
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

        public override void Transform(LevelObject obj, Vector3 pivot, TransformToolData data)
        {
            var mat = obj.modelMatrix;
            var rotPivot = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(data.vec));

            if (toolbox.transformSpace == TransformSpace.Global)
            {
                var transPivot = Matrix4.CreateTranslation(pivot);
                mat = mat * transPivot.Inverted() * rotPivot * transPivot;
            }
            else if (toolbox.transformSpace == TransformSpace.Local)
            {
                var transPivotOffset = Matrix4.CreateTranslation(obj.position - pivot);
                var transObj = Matrix4.CreateTranslation(obj.position);
                var rotObj = Matrix4.CreateFromQuaternion(obj.rotation);
                mat =
                    mat *
                    // Move to origin and remove rotation
                    transObj.Inverted() * rotObj.Inverted() *
                    // Offset by the pivot, rotate, and undo pivot offset
                    transPivotOffset * rotPivot * transPivotOffset.Inverted() *
                    // Add back object rotation and position
                    rotObj * transObj;
            }

            obj.SetFromMatrix(mat);
        }
    }
}
