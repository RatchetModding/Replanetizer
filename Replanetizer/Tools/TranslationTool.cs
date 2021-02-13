using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit.Tools
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

        public override void Render(Vector3 position, CustomGLControl control)
        {
            GetVBO();

            Matrix4 modelMatrix = Matrix4.CreateTranslation(position);
            var mvp = modelMatrix * control.worldView;
            GL.UniformMatrix4(control.matrixID, false, ref mvp);

            GL.Uniform4(control.colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.Uniform4(control.colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3);

            GL.Uniform4(control.colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);

            GL.Uniform4(control.colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 9, 3);


            GL.Uniform4(control.colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 12, 3);

            GL.Uniform4(control.colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 15, 3);

            GL.Uniform4(control.colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 18, 3);

            GL.Uniform4(control.colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 21, 3);


            GL.Uniform4(control.colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 24, 3);

            GL.Uniform4(control.colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 27, 3);

            GL.Uniform4(control.colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 30, 3);

            GL.Uniform4(control.colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 33, 3);
        }

        public override ToolType GetToolType()
        {
            return ToolType.Translate;
        }
    }
}
