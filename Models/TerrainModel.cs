using System;
using System.Collections.Generic;
using System.IO;
using RatchetEdit.Headers;
using static RatchetEdit.DataFunctions;


namespace RatchetEdit.Models
{
    public class TerrainModel : Model
    {
        public TerrainModel(FileStream fs, TerrainHead head, byte[] tfragBlock, int num)
        {
            int offset = num * 0x30;
            int texturePointer = ReadInt(tfragBlock, offset + 0x10);
            int textureCount = ReadInt(tfragBlock, offset + 0x14);
            ushort vertexIndex = ReadUshort(tfragBlock, offset + 0x18);
            ushort vertexCount = ReadUshort(tfragBlock, offset + 0x1A);
            ushort slotNum = ReadUshort(tfragBlock, offset + 0x22);

            // Oh yes, we are hacking
            int faceStart = ReadInt(ReadBlock(fs, texturePointer + 4, 4), 0);

            textureConfig = GetTextureConfigs(fs, texturePointer, textureCount, 0x10, true);
            int faceCount = GetFaceCount();

            vertexBuffer = GetVertices(fs, head.vertexPointers[slotNum] + vertexIndex * 0x1C, head.UVpointers[slotNum] + vertexIndex * 0x08, vertexCount, 0x1C, 0x08);
            indexBuffer = GetIndices(fs, head.indexPointers[slotNum] + faceStart * 2, faceCount, vertexIndex);
        }
    }
}