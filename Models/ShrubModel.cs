using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    class ShrubModel : Model
    {
        const int SHRUBTEXELEMSIZE = 0x10;
        const int SHRUBVERTELEMSIZE = 0x18;
        const int SHRUBUVELEMSIZE = 0x08;

        public float off_00 { get; set; }
        public float off_04 { get; set; }
        public float off_08 { get; set; }
        public float off_0C { get; set; }

        public uint off_20 { get; set; }
        public short off_2A { get; set; }
        public uint off_2C { get; set; }

        public uint off_34 { get; set; }
        public uint off_38 { get; set; }
        public uint off_3C { get; set; }


        public ShrubModel(FileStream fs, byte[] tieBlock, int num)
        {
            int offset = num * 0x40;
            off_00 = ReadFloat(tieBlock, offset + 0x00);
            off_04 = ReadFloat(tieBlock, offset + 0x04);
            off_08 = ReadFloat(tieBlock, offset + 0x08);
            off_0C = ReadFloat(tieBlock, offset + 0x0C);

            int vertexPointer = ReadInt(tieBlock, offset + 0x10);
            int UVPointer = ReadInt(tieBlock, offset + 0x14);
            int indexPointer = ReadInt(tieBlock, offset + 0x18);
            int texturePointer = ReadInt(tieBlock, offset + 0x1C);

            off_20 = ReadUint(tieBlock, offset + 0x20);
            int vertexCount = ReadInt(tieBlock, offset + 0x24);
            short textureCount = ReadShort(tieBlock, offset + 0x28);
            off_2A = ReadShort(tieBlock, offset + 0x2A);
            off_2C = ReadUint(tieBlock, offset + 0x2C);

            id = ReadShort(tieBlock, offset + 0x30);
            off_34 = ReadUint(tieBlock, offset + 0x34);
            off_38 = ReadUint(tieBlock, offset + 0x38);
            off_3C = ReadUint(tieBlock, offset + 0x3C);

            size = 1.0f;

            textureConfig = GetTextureConfigs(fs, texturePointer, textureCount, SHRUBTEXELEMSIZE);
            int faceCount = GetFaceCount();

            //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ] and UV array float[U, V] * vertexCount
            vertexBuffer = GetVertices(fs, vertexPointer, UVPointer, vertexCount, SHRUBVERTELEMSIZE, SHRUBUVELEMSIZE);

            //Get index buffer ushort[i] * faceCount
            indexBuffer = GetIndices(fs, indexPointer, faceCount);
        }

        public byte[] SerializeHead(int offStart)
        {
            byte[] outBytes = new byte[0x40];

            WriteFloat(ref outBytes, 0x00, off_00);
            WriteFloat(ref outBytes, 0x04, off_04);
            WriteFloat(ref outBytes, 0x08, off_08);
            WriteFloat(ref outBytes, 0x0C, off_0C);

            int texturePointer = GetLength(offStart);
            int vertexPointer = GetLength(texturePointer + textureConfig.Count * SHRUBTEXELEMSIZE); //+ 0x70
            int UVPointer = GetLength(vertexPointer + (vertexBuffer.Length / 8) * SHRUBVERTELEMSIZE);
            int indexPointer = GetLength(UVPointer + (vertexBuffer.Length / 8) * SHRUBUVELEMSIZE);

            WriteInt(ref outBytes, 0x10, vertexPointer);
            WriteInt(ref outBytes, 0x14, UVPointer);
            WriteInt(ref outBytes, 0x18, indexPointer);
            WriteInt(ref outBytes, 0x1C, texturePointer);

            WriteUint(ref outBytes, 0x20, off_20);
            WriteInt(ref outBytes, 0x24, vertexBuffer.Length / 8);
            WriteShort(ref outBytes, 0x28, (short)textureConfig.Count);
            WriteShort(ref outBytes, 0x2A, off_2A);
            WriteUint(ref outBytes, 0x2C, off_2C);

            WriteShort(ref outBytes, 0x30, id);
            WriteUint(ref outBytes, 0x34, off_34);
            WriteUint(ref outBytes, 0x38, off_38);
            WriteUint(ref outBytes, 0x3C, off_3C);

            return outBytes;
        }

        public byte[] SerializeBody(int offStart)
        {
            int texturePointer = 0;
            int vertexPointer = GetLength(texturePointer + textureConfig.Count * SHRUBTEXELEMSIZE); //+ 0x70
            int UVPointer = GetLength(vertexPointer + (vertexBuffer.Length / 8) * SHRUBVERTELEMSIZE);
            int indexPointer = GetLength(UVPointer + (vertexBuffer.Length / 8) * SHRUBUVELEMSIZE);
            int length = GetLength(indexPointer + indexBuffer.Length * 2);

            byte[] outBytes = new byte[length];
            SerializeTieVertices().CopyTo(outBytes, vertexPointer);
            GetFaceBytes().CopyTo(outBytes, indexPointer);
            SerializeUVs().CopyTo(outBytes, UVPointer);

            for (int i = 0; i < textureConfig.Count; i++)
            {
                WriteInt(ref outBytes, texturePointer + i * 0x10 + 0x00, textureConfig[i].ID);
                WriteInt(ref outBytes, texturePointer + i * 0x10 + 0x04, textureConfig[i].start);
                WriteInt(ref outBytes, texturePointer + i * 0x10 + 0x08, textureConfig[i].size);
                WriteInt(ref outBytes, texturePointer + i * 0x10 + 0x0C, textureConfig[i].mode);
            }

            return outBytes;
        }
    }
}
