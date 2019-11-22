using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Models
{
    [StructLayout(LayoutKind.Explicit)]
    struct FloatColor
    {
        [FieldOffset(0)]
        public byte r;
        [FieldOffset(1)]
        public byte g;
        [FieldOffset(2)]
        public byte b;
        [FieldOffset(3)]
        public byte a;

        [FieldOffset(0)]
        public float value;
    }

    public class Collision : Model
    {
        uint[] indBuff = { };
        uint[] colorBuff = { };

        public Collision(FileStream fs, int collisionPointer)
        {
            float div = 1024f;

            uint totalVertexCount = 0;

            byte[] headBlock = ReadBlock(fs, collisionPointer, 8);
            int collisionStart = collisionPointer + ReadInt(headBlock, 0);
            int collisionLength = ReadInt(headBlock, 4);
            byte[] collision = ReadBlock(fs, collisionStart, collisionLength);

            var vertexList = new List<float>();
            var indexList = new List<uint>();
            var colorList = new List<uint>();

            ushort zShift = ReadUshort(collision, 0);
            ushort zCount = ReadUshort(collision, 2);

            FloatColor fc = new FloatColor { r = 255, g = 0, b = 255, a = 255 };

            for (int z = 0; z < zCount; z++)
            {
                int yOffset = ReadInt(collision, (z * 4) + 0x04);
                if (yOffset == 0) continue;

                ushort yShift = ReadUshort(collision, yOffset + 0);
                ushort yCount = ReadUshort(collision, yOffset + 2);

                for (int y = 0; y < yCount; y++)
                {
                    int xOffset = ReadInt(collision, yOffset + (y * 4) + 0x04);
                    if (xOffset == 0) continue;

                    ushort xShift = ReadUshort(collision, xOffset + 0);
                    ushort xCount = ReadUshort(collision, xOffset + 2);

                    for (int x = 0; x < xCount; x++)
                    {
                        int vOffset = ReadInt(collision, xOffset + (x * 4) + 4);
                        if (vOffset == 0) { continue; }

                        ushort faceCount = ReadUshort(collision, vOffset);
                        byte vertexCount = collision[vOffset + 2];
                        byte rCount = collision[vOffset + 3];

                        byte[] collisionType = new byte[vertexCount];
                        for (int f = 0; f < faceCount; f++)
                        {

                            int fOffset = vOffset + 4 + (12 * vertexCount) + (f * 4);

                            byte b0 = collision[fOffset];
                            byte b1 = collision[fOffset + 1];
                            byte b2 = collision[fOffset + 2];
                            byte b3 = collision[fOffset + 3];

                            collisionType[b0] = b3;
                            collisionType[b1] = b3;
                            collisionType[b2] = b3;

                            uint f1 = totalVertexCount + b0;
                            uint f2 = totalVertexCount + b1;
                            uint f3 = totalVertexCount + b2;
                            indexList.Add(f1);
                            indexList.Add(f2);
                            indexList.Add(f3);

                            if (f < rCount)
                            {
                                int rOffset = vOffset + 4 + (12 * vertexCount) + (faceCount * 4) + f;
                                uint f4 = totalVertexCount + collision[rOffset];
                                indexList.Add(f1);
                                indexList.Add(f3);
                                indexList.Add(f4);
                                collisionType[collision[rOffset]] = b3;
                            }

                            for (int v = 0; v < vertexCount; v++)
                            {                                
                                int pOffset = vOffset + (12 * v) + 4;
                                vertexList.Add(ReadFloat(collision, pOffset + 0) / div + 4 * (xShift + x + 0.5f));  //Vertex X
                                vertexList.Add(ReadFloat(collision, pOffset + 4) / div + 4 * (yShift + y + 0.5f));  //Vertex Y
                                vertexList.Add(ReadFloat(collision, pOffset + 8) / div + 4 * (zShift + z + 0.5f));  //Vertex Z

                                /*
                                switch (collisionType[v])
                                {
                                    case 0x1F:
                                        fc.r = 255; fc.g = 0; fc.b = 0;
                                        break;

                                    default:
                                        fc.r = 0;
                                        fc.g = 0;
                                        fc.b = 0;
                                        break;
                                }
                                */

                                // Colorize different types of collision without knowing what they are
                                fc.r = (byte)((collisionType[v] & 0x03) << 6);
                                fc.g = (byte)((collisionType[v] & 0x0C) << 4);
                                fc.b = (byte)(collisionType[v] & 0xF0);

                                vertexList.Add(fc.value);
                                totalVertexCount++;
                            }
                        }
                    }
                }
            }
            vertexBuffer = vertexList.ToArray();
            indBuff = indexList.ToArray();
        }

        public void DrawCol(CustomGLControl glControl)
        {
            Matrix4 worldView = glControl.worldView;
            GL.UniformMatrix4(glControl.matrixID, false, ref worldView);
            GL.Uniform4(glControl.colorID, new Vector4(1, 1, 1, 1));
            GetUBO();
            GetBBO();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.DrawElements(PrimitiveType.Triangles, indBuff.Length, DrawElementsType.UnsignedInt, 0);
            GL.UseProgram(glControl.collisionShaderID);
            GL.UniformMatrix4(glControl.matrixID, false, ref worldView);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawElements(PrimitiveType.Triangles, indBuff.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void GetBBO()
        {
            //Get the index buffer object, or create one if one doesn't exist
            if (IBO == 0)
            {
                GL.GenBuffers(1, out IBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indBuff.Length * sizeof(int), indBuff, BufferUsageHint.StaticDraw);
                //Console.WriteLine("Generated IBO with ID: " + IBO.ToString());
            }
            else
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            }
        }


        public void GetUBO()
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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);
        }
    }
}
