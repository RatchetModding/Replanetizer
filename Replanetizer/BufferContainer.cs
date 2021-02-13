using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit
{
    /*
     * A container to store IBO and VBO references for a Model
     */
    public class BufferContainer
    {
        public int ibo = 0;
        public int vbo = 0;

        public BufferContainer() { }

        public static BufferContainer FromRenderable(IRenderable renderable)
        {
            BufferContainer container = new BufferContainer();

            BufferUsageHint hint = BufferUsageHint.StaticDraw;
            if (renderable.IsDynamic())
            {
                hint = BufferUsageHint.DynamicDraw;
            }

            // IBO
            ushort[] iboData = renderable.GetIndices();
            if (iboData.Length > 0)
            {
                GL.GenBuffers(1, out container.ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, container.ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboData.Length * sizeof(ushort), iboData, hint);
            }

            // VBO 
            float[] vboData = renderable.GetVertices();
            if (vboData.Length > 0)
            {
                GL.GenBuffers(1, out container.vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, container.vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboData.Length * sizeof(float), vboData, hint);
            }

            return container;
        }

        public void Bind()
        {
            // IBO
            if (ibo != 0)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ibo);

            // VBO
            if (vbo != 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
            }
        }
    }
}
