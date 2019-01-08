using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit.Tools
{
    class TranslationTool : Tool
    {
        public static void Render(Vector3 position, CustomGLControl glControl)
        {
            float[] test = new float[18];
            float length = 2;
            test[0] = position.X - length;
            test[1] = position.Y;
            test[2] = position.Z;

            test[3] = position.X + length;
            test[4] = position.Y;
            test[5] = position.Z;

            test[6] = position.X;
            test[7] = position.Y - length;
            test[8] = position.Z;

            test[9] = position.X;
            test[10] = position.Y + length;
            test[11] = position.Z;

            test[12] = position.X;
            test[13] = position.Y;
            test[14] = position.Z - length;

            test[15] = position.X;
            test[16] = position.Y;
            test[17] = position.Z + length;

            GL.UseProgram(glControl.colorShaderID);
            var worldView = glControl.worldView;
            GL.UniformMatrix4(glControl.matrixID, false, ref worldView);
            GL.GenBuffers(1, out int VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.BufferData(BufferTarget.ArrayBuffer, test.Length * sizeof(float), test, BufferUsageHint.DynamicDraw);

            GL.Uniform4(glControl.colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 0, 2);

            GL.Uniform4(glControl.colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 2, 2);

            GL.Uniform4(glControl.colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 4, 2);
        }
    }
}
