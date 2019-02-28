using OpenTK;
using OpenTK.Graphics.OpenGL;
using RatchetEdit.LevelObjects;

namespace RatchetEdit
{
    class Cube
    {
        public int VBO;
        public int IBO;

        public float[] cube;
        public ushort[] cubeElements;

        Matrix4 scaleMatrix = Matrix4.CreateScale(1000f);

        public Cube()
        {
            cube = new float[]{
                    -1.0f, -1.0f,  1.0f,
                    1.0f, -1.0f,  1.0f,
                    1.0f,  1.0f,  1.0f,
                    -1.0f,  1.0f,  1.0f,
                    // back
                    -1.0f, -1.0f, -1.0f,
                    1.0f, -1.0f, -1.0f,
                    1.0f,  1.0f, -1.0f,
                    -1.0f,  1.0f, -1.0f
            };

            cubeElements = new ushort[] { 0, 1, 2, 2, 3, 0, 1, 5, 6, 6, 2, 1, 7, 6, 5, 5, 4, 7, 4, 0, 3, 3, 7, 4, 4, 5, 1, 1, 0, 4, 3, 2, 6, 6, 7, 3 };


            GL.GenBuffers(1, out VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, cube.Length * sizeof(float), cube, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out IBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, cubeElements.Length * sizeof(ushort), cubeElements, BufferUsageHint.StaticDraw);
        }

        public void Render(Cuboid sp, CustomGLControl glControl)
        {
            GL.UseProgram(glControl.colorShaderID);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                Matrix4 mvp = sp.modelMatrix * glControl.worldView;
                GL.UniformMatrix4(glControl.matrixID, false, ref mvp);
                GL.Uniform4(glControl.colorID, new Vector4(1, 1, 1, 1));

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawElements(PrimitiveType.Triangles, cubeElements.Length, DrawElementsType.UnsignedShort, 0);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }
    }
}
