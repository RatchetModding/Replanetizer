using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Frames;

namespace Replanetizer.Tools
{
    class TranslationTool : Tool
    {
        public TranslationTool()
        {
            float length = 2.0f;

            vb = new float[]{
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
            GetVBO();

            Matrix4 modelMatrix = Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(frame.shaderIDTable.UniformModelToWorldMatrix, false, ref modelMatrix);

            GL.Uniform1(frame.shaderIDTable.UniformColorLevelObjectNumber, 0);
            GL.Uniform4(frame.shaderIDTable.UniformColor, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 9, 3);

            GL.Uniform1(frame.shaderIDTable.UniformColorLevelObjectNumber, 1);
            GL.Uniform4(frame.shaderIDTable.UniformColor, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 12, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 15, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 18, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 21, 3);

            GL.Uniform1(frame.shaderIDTable.UniformColorLevelObjectNumber, 2);
            GL.Uniform4(frame.shaderIDTable.UniformColor, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 24, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 27, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 30, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 33, 3);
        }

        public override ToolType GetToolType()
        {
            return ToolType.Translate;
        }
    }
}
