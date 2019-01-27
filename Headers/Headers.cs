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

            vertexPointer = ReadInt(levelBlock, (offset * TIEELEMSIZE) + 0x10);
            UVPointer = ReadInt(levelBlock, (offset * TIEELEMSIZE) + 0x14);
            indexPointer = ReadInt(levelBlock, (offset * TIEELEMSIZE) + 0x18);
            texturePointer = ReadInt(levelBlock, (offset * TIEELEMSIZE) + 0x1C);

            off_20 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x20);
            vertexCount = ReadInt(levelBlock, (offset * TIEELEMSIZE) + 0x24);
            textureCount = ReadShort(levelBlock, (offset * TIEELEMSIZE) + 0x28);
            off_2C = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x2C);

            modelID = ReadShort(levelBlock, (offset * TIEELEMSIZE) + 0x30);
            off_34 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x34);
            off_38 = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x38);
            off_3C = ReadUint(levelBlock, (offset * TIEELEMSIZE) + 0x3C);
        }
    }

    public class MobyModelHeader
    {
        const int MESHHEADERSIZE = 0x20;
        const int HEADERSIZE = 0x48;

        public int meshPointer { get; set; }
        public int null1 { get; set; }

        public byte boneCount { get; set; }
        public byte count2 { get; set; }
        public byte count3 { get; set; }
        public byte count4 { get; set; }

        public byte animationCount { get; set; }
        public byte type28Count { get; set; }
        public byte count7 { get; set; }
        public byte count8 { get; set; }

        public int type10Pointer { get; set; }
        public int boneMatrixPointer { get; set; }
        public int bonePointer { get; set; }
        public int type1CPointer { get; set; }

        public int null2 { get; set; }
        public float scale { get; set; }
        public int type28Pointer { get; set; }
        public int null3 { get; set; }

        public float unk1 { get; set; }
        public float unk2 { get; set; }
        public float unk3 { get; set; }
        public float unk4 { get; set; }

        public uint unk5 { get; set; }
        public uint unk6 { get; set; }

        public List<Animation> animations { get; set; }


        public int texCount { get; set; }
        public int otherCount { get; set; }
        public int texBlockPointer { get; set; }
        public int otherBlockPointer { get; set; }
        public int vertPointer { get; set; }
        public int indexPointer { get; set; }
        public ushort vertexCount { get; set; }


        public MobyModelHeader(FileStream fs, int offset)
        {
            byte[] headBlock = ReadBlock(fs, offset, HEADERSIZE);
            meshPointer = ReadInt(headBlock, 0x00);
            null1 = ReadInt(headBlock, 0x04);

            boneCount = headBlock[0x08];
            count2 = headBlock[0x09];
            count3 = headBlock[0x0A];
            count4 = headBlock[0x0B];
            animationCount = headBlock[0x0C];
            type28Count = headBlock[0x0D];
            count7 = headBlock[0x0E];
            count8 = headBlock[0x0F];

            type10Pointer = ReadInt(headBlock, 0x10);
            boneMatrixPointer = ReadInt(headBlock, 0x14);
            bonePointer = ReadInt(headBlock, 0x18);
            type1CPointer = ReadInt(headBlock, 0x1C);

            null2 = ReadInt(headBlock, 0x20);
            scale = ReadFloat(headBlock, 0x24);
            type28Pointer = ReadInt(headBlock, 0x28);
            null3 = ReadInt(headBlock, 0x2C);

            unk1 = ReadFloat(headBlock, 0x30);
            unk2 = ReadFloat(headBlock, 0x34);
            unk3 = ReadFloat(headBlock, 0x38);
            unk4 = ReadFloat(headBlock, 0x3C);

            unk5 = ReadUint(headBlock, 0x40);
            unk6 = ReadUint(headBlock, 0x44);

            byte[] animationPointerBlock = ReadBlock(fs, offset + 0x48, animationCount * 0x04);
            animations = new List<Animation>();
            for (int i = 0; i < animationCount; i++)
            {
                animations.Add(new Animation(fs, offset, ReadInt(animationPointerBlock, i * 0x04)));
            }

            byte[] meshHeader = ReadBlock(fs, offset + meshPointer, 0x20);
            texCount = ReadInt(meshHeader, 0x00);
            otherCount = ReadInt(meshHeader, 0x04);
            texBlockPointer = offset + ReadInt(meshHeader, 0x08);
            otherBlockPointer = offset + ReadInt(meshHeader, 0x0C);
            vertPointer = offset + ReadInt(meshHeader, 0x10);
            indexPointer = offset + ReadInt(meshHeader, 0x14);
            vertexCount = ReadUshort(meshHeader, 0x18);
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
            RGBAPointer = ReadInt(terrainBlock, offset + 0x18);
            UVPointer = ReadInt(terrainBlock, offset + 0x28);
            indexPointer = ReadInt(terrainBlock, offset + 0x38);
            heads = new List<TerrainFragHeader>();
        }
    }

    public class TerrainFragHeader
    {
        const int ELEMSIZE = 0x30;

        public float off_00;
        public float off_04;
        public float off_08;
        public float off_0C;

        public int off_1C;
        public short off_20;

        public int texturePointer;
        public int textureCount;
        public ushort vertexIndex;
        public ushort vertexCount;
        public ushort slotNum;

        public TerrainFragHeader(FileStream fs, byte[] terrainHeadBlock, int index)
        {
            int offset = index * ELEMSIZE;

            off_00 = ReadFloat(terrainHeadBlock, offset + 0x00);
            off_04 = ReadFloat(terrainHeadBlock, offset + 0x04);
            off_08 = ReadFloat(terrainHeadBlock, offset + 0x08);
            off_0C = ReadFloat(terrainHeadBlock, offset + 0x0C);

            off_1C = ReadInt(terrainHeadBlock, offset + 0x1C);
            off_20 = ReadShort(terrainHeadBlock, off_20 + 0x20);

            texturePointer = ReadInt(terrainHeadBlock, (offset) + 0x10);
            textureCount = ReadInt(terrainHeadBlock, (offset) + 0x14);
            vertexIndex = ReadUshort(terrainHeadBlock, (offset) + 0x18);
            vertexCount = ReadUshort(terrainHeadBlock, (offset) + 0x1A);
            slotNum = ReadUshort(terrainHeadBlock, (offset) + 0x22);
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
