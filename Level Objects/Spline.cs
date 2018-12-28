using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit
{
    public class Spline : LevelObject
    {
        public int name;
        public float[] vertexBuffer;

        int VBO;

        public Spline(byte[] splineBlock, int offset)
        {
            name = offset;
            int count = ReadInt(splineBlock, offset);
            vertexBuffer = new float[count * 3];
            for(int i = 0; i < count; i++)
            {
                vertexBuffer[(i * 3) + 0] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x00);
                vertexBuffer[(i * 3) + 1] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x04);
                vertexBuffer[(i * 3) + 2] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x08);
            }

            if(count > 0) {
                position = new OpenTK.Vector3(vertexBuffer[0], vertexBuffer[1], vertexBuffer[2]);
            }
        }

        public byte[] serialize()
        {
            int count = vertexBuffer.Length / 3;
            byte[] bytes = new byte[count * 0x10 + 0x10];
            WriteUint(ref bytes, 0, (uint)count);
            for(int i = 0; i < count; i++)
            {
                WriteFloat(ref bytes, (i * 0x10) + 0x10, vertexBuffer[(i * 3) + 0]);
                WriteFloat(ref bytes, (i * 0x10) + 0x14, vertexBuffer[(i * 3) + 1]);
                WriteFloat(ref bytes, (i * 0x10) + 0x18, vertexBuffer[(i * 3) + 2]);
                WriteFloat(ref bytes, (i * 0x10) + 0x1C, -1);
            }
            return bytes;
        }

        public void getVBO()
        {
            //Get the vertex buffer object, or create one if one doesn't exist
            if (VBO == 0)
            {
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticDraw);
                Console.WriteLine("Generated VBO with ID: " + VBO.ToString());
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticDraw);
            }
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }
    }
}
