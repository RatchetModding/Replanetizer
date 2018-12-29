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

    public class TieModelHeader
    {
        public const int TIEELEMSIZE = 0x40;

        public uint off_00 { get; set; }
        public uint off_04 { get; set; }
        public uint off_08 { get; set; }
        public uint off_0C { get; set; }

        public int vertexPointer { get; set; }
        public int UVPointer { get; set; }
        public int indexPointer { get; set; }
        public int texturePointer { get; set; }

        public uint off_20 { get; set; }
        public int vertexCount { get; set; }
        public short textureCount { get; set; }
        public uint off_2C { get; set; }

        public short modelID { get; set; }
        public uint off_34 { get; set; }
        public uint off_38 { get; set; }
        public uint off_3C { get; set; }


        public TieModelHeader(byte[] levelBlock, int offset)
        {
            off_00 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x00);
            off_04 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x04);
            off_08 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x08);
            off_0C = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x0C);

            vertexPointer =     ReadInt(levelBlock,    (offset * TIEELEMSIZE) + 0x10);
            UVPointer =         ReadInt(levelBlock,    (offset * TIEELEMSIZE) + 0x14);
            indexPointer =      ReadInt(levelBlock,    (offset * TIEELEMSIZE) + 0x18);
            texturePointer =    ReadInt(levelBlock,    (offset * TIEELEMSIZE) + 0x1C);

            off_20 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x20);
            vertexCount =       ReadInt(levelBlock,     (offset * TIEELEMSIZE) + 0x24);
            textureCount =      ReadShort(levelBlock,   (offset * TIEELEMSIZE) + 0x28);
            off_2C = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x2C);

            modelID =           ReadShort(levelBlock,   (offset * TIEELEMSIZE) + 0x30);
            off_34 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x34);
            off_38 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x38);
            off_3C = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x3C);
        }
    }

    public class MobyModelHeader
    {
        const int MOBYHEADERSIZE = 0x20;

        public float modelSize;
        public int headSize;
        public int texCount;
        public int otherCount;
        public int texBlockPointer;
        public int otherBlockPointer;
        public int vertPointer;
        public int indexPointer;
        public ushort vertexCount;


        public MobyModelHeader(FileStream fs, int offset)
        {
            headSize = ReadInt(ReadBlock(fs, offset, 4), 0);
            if(headSize > 0)
            {
                byte[] headBlock = ReadBlock(fs, offset, headSize + MOBYHEADERSIZE);

                //0x00 headSize
                //0x04 null
                //0x08 
                modelSize = ReadFloat(headBlock, 0x24);

                texCount =                  ReadInt(headBlock, headSize + 0x00);
                otherCount =                ReadInt(headBlock, headSize + 0x04);
                texBlockPointer = offset +  ReadInt(headBlock, headSize + 0x08);
                otherBlockPointer = offset+ ReadInt(headBlock, headSize + 0x0C);
                vertPointer = offset +      ReadInt(headBlock, headSize + 0x10);
                indexPointer = offset +     ReadInt(headBlock, headSize + 0x14);
                vertexCount =               ReadUshort(headBlock, headSize + 0x18);
            }
        }
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
            RGBAPointer =   ReadInt(terrainBlock, offset + 0x18);
            UVPointer =     ReadInt(terrainBlock, offset + 0x28);
            indexPointer =  ReadInt(terrainBlock, offset + 0x38);
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
            texturePointer =    ReadInt(terrainHeadBlock,      (index * ELEMSIZE) + 0x10);
            textureCount =      ReadInt(terrainHeadBlock,       (index * ELEMSIZE) + 0x14);
            vertexIndex =       ReadUshort(terrainHeadBlock,    (index * ELEMSIZE) + 0x18);
            vertexCount =       ReadUshort(terrainHeadBlock,    (index * ELEMSIZE) + 0x1A);
            slotNum =           ReadUshort(terrainHeadBlock,    (index * ELEMSIZE) + 0x22);
        }
    }
}






