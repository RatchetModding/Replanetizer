using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit
{
    public class Spline
    {
        public float[] vertexBuffer;
        int VBO;

        public Spline(byte[] splineBlock, int offset)
        {
            int count = ReadInt(splineBlock, offset);
            vertexBuffer = new float[count * 3];
            for(int i = 0; i < count; i++)
            {
                vertexBuffer[(i * 3) + 0] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x00);
                vertexBuffer[(i * 3) + 1] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x04);
                vertexBuffer[(i * 3) + 2] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x08);
                //vertexBuffer[i] = ReadFloat(splineBlock, offset + (i * 0x10));
            }
        }

        public void getVBO()
        {
            //Get the vertex buffer object, or create one if one doesn't exist
            if (VBO == 0)
            {
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticDraw);
                //Console.WriteLine("Generated VBO with ID: " + VBO.ToString());
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            }
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }
    }
}
