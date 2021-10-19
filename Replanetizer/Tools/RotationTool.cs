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
    class RotationTool : BasicTransformTool
    {
        public override ToolType toolType => ToolType.Rotation;

        public RotationTool(Toolbox toolbox) : base(toolbox)
        {
            const float length = 1.5f;

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

        public override void Transform(LevelObject obj, Vector3 vec, Vector3 pivot)
        {
            var mat = obj.modelMatrix;
            var rotPivot = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(vec));

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
