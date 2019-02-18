using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit
{
    class Headers
    {
    }

    public class TerrainHeader
    {
        public int vertexPointer;
        public int RGBAPointer;
        public int UVPointer;
        public int indexPointer;
        public int vertexCount;
        public List<TerrainFragHeader> heads;

        public TerrainHeader(byte[] terrainBlock, int offset)
        {
            vertexPointer = ReadInt(terrainBlock, offset + 0x08);
            RGBAPointer = ReadInt(terrainBlock, offset + 0x18);
            UVPointer = ReadInt(terrainBlock, offset + 0x28);
            indexPointer = ReadInt(terrainBlock, offset + 0x38);
            heads = new List<TerrainFragHeader>();
        }
    }



    public class TerrainFragHeader
    {
        const int ELEMSIZE = 0x30;

        public int texturePointer;
        public int textureCount;
        public ushort vertexIndex;
        public ushort vertexCount;
        public ushort slotNum;

        public TerrainFragHeader(FileStream fs, byte[] terrainHeadBlock, int index)
        {
            texturePointer = ReadInt(terrainHeadBlock, (index * ELEMSIZE) + 0x10);
            textureCount = ReadInt(terrainHeadBlock, (index * ELEMSIZE) + 0x14);
            vertexIndex = ReadUshort(terrainHeadBlock, (index * ELEMSIZE) + 0x18);
            vertexCount = ReadUshort(terrainHeadBlock, (index * ELEMSIZE) + 0x1A);
            slotNum = ReadUshort(terrainHeadBlock, (index * ELEMSIZE) + 0x22);
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
