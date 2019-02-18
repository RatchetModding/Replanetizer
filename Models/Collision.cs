using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Models
{
    class Collision
    {

        public Collision(FileStream fs, int collisionPointer)
        {
            float div = 1024f;

            int collisionVertCount = 0;

            int collisionStart = collisionPointer + ReadInt(ReadBlock(fs, collisionPointer + 0x00, 4), 0);
            int collisionLength = collisionPointer + collisionStart + ReadInt(ReadBlock(fs, collisionPointer + 0x04, 4), 0);
            byte[] collision = ReadBlock(fs, collisionStart, collisionLength);

            List<float> collisionVertexBuff = new List<float>();
            List<int> collisionIndiceBuff = new List<int>();

            ushort zShift = ReadUshort(collision, 0);
            ushort zCount = ReadUshort(collision, 2);

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
                        ushort faceCount = ReadUshort(collision, vOffset);
                        byte vertexCount = collision[vOffset + 2];
                        byte rCount = collision[vOffset + 3];

                        if (vOffset == 0) continue;

                        byte[] collisionType = new byte[vertexCount];
                        //Console.WriteLine("Facecount: " + faceCount.ToString("X2") + "VertCount: " + vertexCount.ToString("X2"));
                        for (int f = 0; f < faceCount; f++)
                        {
                            int fOffset = vOffset + (12 * vertexCount) + 4 + (f * 4);
                            byte[] fArray = new byte[4];
                            Array.Copy(collision, fOffset, fArray, 0, 4);

                            collisionType[fArray[0]] = fArray[3];
                            collisionType[fArray[1]] = fArray[3];
                            collisionType[fArray[2]] = fArray[3];

                            int f1 = collisionVertCount + fArray[0];
                            int f2 = collisionVertCount + fArray[1];
                            int f3 = collisionVertCount + fArray[2];
                            collisionIndiceBuff.Add(f1);
                            collisionIndiceBuff.Add(f2);
                            collisionIndiceBuff.Add(f3);
                            if (f < rCount)
                            {
                                int rOffset = vOffset + 4 + (12 * vertexCount) + (4 * faceCount) + f;
                                int f4 = collisionVertCount + collision[rOffset];
                                collisionIndiceBuff.Add(f1);
                                collisionIndiceBuff.Add(f3);
                                collisionIndiceBuff.Add(f4);
                                collisionType[collision[rOffset]] = fArray[3];
                            }

                            for (int v = 0; v < vertexCount; v++)
                            {
                                int pOffset = vOffset + (12 * v) + 4;
                                float pX = ReadFloat(collision, pOffset + 0) / div + 4 * (xShift + x + 0.5f);
                                float pY = ReadFloat(collision, pOffset + 4) / div + 4 * (yShift + y + 0.5f);
                                float pZ = ReadFloat(collision, pOffset + 8) / div + 4 * (zShift + z + 0.5f);
                                collisionVertexBuff.Add(pX);
                                collisionVertexBuff.Add(pY);
                                collisionVertexBuff.Add(pZ);
                                collisionVertCount++;
                            }
                        }
                    }
                }
            }
        }
    }
}
