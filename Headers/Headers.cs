using System.IO;
using System.Collections.Generic;
using static RatchetEdit.DataFunctions;
using System;

namespace RatchetEdit.Headers
{
    class Headers
    {
    }
    public class TerrainHead
    {
        public ushort headCount;
        public List<int> vertexPointers = new List<int>();
        public List<int> rgbaPointers = new List<int>();
        public List<int> UVpointers = new List<int>();
        public List<int> indexPointers = new List<int>();

        public TerrainHead(byte[] terrainBlock)
        {
            headCount = ReadUshort(terrainBlock, 0x06);

            for (int i = 0; i < 4; i++)
            {
                vertexPointers.Add(ReadInt(terrainBlock, 0x08 + i * 4));
                rgbaPointers.Add(ReadInt(terrainBlock, 0x18 + i * 4));
                UVpointers.Add(ReadInt(terrainBlock, 0x28 + i * 4));
                indexPointers.Add(ReadInt(terrainBlock, 0x38 + i * 4));
            }
        }
    }

    public class OcclusionDataHeader
    {
        public int mobyCount;
        public int tieCount;
        public int shrubCount;
        public int totalCount;

        public OcclusionDataHeader(byte[] headBlock)
        {
            mobyCount = ReadInt(headBlock, 0x00);
            tieCount = ReadInt(headBlock, 0x04);
            shrubCount = ReadInt(headBlock, 0x08);
            totalCount = mobyCount + tieCount + shrubCount;
        }
    }
}
