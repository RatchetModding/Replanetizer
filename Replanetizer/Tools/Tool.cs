using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit.Tools
{
    public abstract class Tool
    {
        public enum ToolType
        {
            None,
            Translate,
            Rotate,
            Scale,
            VertexTranslator
        }

        protected int VBO;
        protected float[] vb;

        protected void GetVBO()
        {
            if (VBO == 0)
            {
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vb.Length * sizeof(float), vb, BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            }

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }

        public abstract ToolType GetToolType();
        public abstract void Render(Vector3 position, CustomGLControl control);
    }
}
