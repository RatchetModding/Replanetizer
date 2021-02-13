using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models
{
    public class TieModel : Model
    {
        const int TIETEXELEMSIZE = 0x18;
        const int TIEVERTELEMSIZE = 0x18;
        const int TIEUVELEMSIZE = 0x08;

        public float cullingX { get; set; }
        public float cullingY { get; set; }
        public float cullingZ { get; set; }
        public float cullingRadius { get; set; }

        public uint off_20 { get; set; }
        public short wiggleMode { get; set; }
        public float off_2C { get; set; }

        public uint off_34 { get; set; }
        public uint off_38 { get; set; }
        public uint off_3C { get; set; }


        public TieModel(FileStream fs, byte[] tieBlock, int num)
        {
            int offset = num * 0x40;
            cullingX = ReadFloat(tieBlock, offset + 0x00);
            cullingY = ReadFloat(tieBlock, offset + 0x04);
            cullingZ = ReadFloat(tieBlock, offset + 0x08);
            cullingRadius = ReadFloat(tieBlock, offset + 0x0C);

            int vertexPointer = ReadInt(tieBlock, offset + 0x10);
            int UVPointer = ReadInt(tieBlock, offset + 0x14);
            int indexPointer = ReadInt(tieBlock, offset + 0x18);
            int texturePointer = ReadInt(tieBlock, offset + 0x1C);

            off_20 = ReadUint(tieBlock, offset + 0x20);                 //null
            int vertexCount = ReadInt(tieBlock, offset + 0x24);
            short textureCount = ReadShort(tieBlock, offset + 0x28);
            wiggleMode = ReadShort(tieBlock, offset + 0x2A);
            off_2C = ReadFloat(tieBlock, offset + 0x2C);

            id = ReadShort(tieBlock, offset + 0x30);
            off_34 = ReadUint(tieBlock, offset + 0x34);                 //null
            off_38 = ReadUint(tieBlock, offset + 0x38);                 //null
            off_3C = ReadUint(tieBlock, offset + 0x3C);                 //null

            size = 1.0f;

            textureConfig = GetTextureConfigs(fs, texturePointer, textureCount, TIETEXELEMSIZE);
            int faceCount = GetFaceCount();

            //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ] and UV array float[U, V] * vertexCount
            vertexBuffer = GetVertices(fs, vertexPointer, UVPointer, vertexCount, TIEVERTELEMSIZE, TIEUVELEMSIZE);

            //Get index buffer ushort[i] * faceCount
            indexBuffer = GetIndices(fs, indexPointer, faceCount);
        }

        public byte[] SerializeHead(int offStart)
        {
            byte[] outBytes = new byte[0x40];

            WriteFloat(outBytes, 0x00, cullingX);
            WriteFloat(outBytes, 0x04, cullingY);
            WriteFloat(outBytes, 0x08, cullingZ);
            WriteFloat(outBytes, 0x0C, cullingRadius);

            int texturePointer = GetLength(offStart);
            int hack = DistToFile80(texturePointer + textureConfig.Count * TIETEXELEMSIZE);
            int vertexPointer = GetLength(texturePointer + textureConfig.Count * TIETEXELEMSIZE + hack); //+ 0x70
            int UVPointer = GetLength(vertexPointer + (vertexBuffer.Length / 8) * TIEVERTELEMSIZE);
            int indexPointer = GetLength(UVPointer + (vertexBuffer.Length / 8) * TIEUVELEMSIZE);

            WriteInt(outBytes, 0x10, vertexPointer);
            WriteInt(outBytes, 0x14, UVPointer);
            WriteInt(outBytes, 0x18, indexPointer);
            WriteInt(outBytes, 0x1C, texturePointer);

            WriteUint(outBytes, 0x20, off_20);
            WriteInt(outBytes, 0x24, vertexBuffer.Length / 8);
            WriteShort(outBytes, 0x28, (short)textureConfig.Count);
            WriteShort(outBytes, 0x2A, wiggleMode);
            WriteFloat(outBytes, 0x2C, off_2C);

            WriteShort(outBytes, 0x30, id);
            WriteUint(outBytes, 0x34, off_34);
            WriteUint(outBytes, 0x38, off_38);
            WriteUint(outBytes, 0x3C, off_3C);

            return outBytes;
        }

        public byte[] SerializeBody(int offStart)
        {
            int texturePointer = 0;
            int hack = DistToFile80(offStart + texturePointer + textureConfig.Count * TIETEXELEMSIZE);
            int vertexPointer = GetLength(texturePointer + textureConfig.Count * TIETEXELEMSIZE + hack); //+ 0x70
            int UVPointer = GetLength(vertexPointer + (vertexBuffer.Length / 8) * TIEVERTELEMSIZE);
            int indexPointer = GetLength(UVPointer + (vertexBuffer.Length / 8) * TIEUVELEMSIZE);
            int length = GetLength(indexPointer + indexBuffer.Length * 2);

            byte[] outBytes = new byte[length];
            SerializeTieVertices().CopyTo(outBytes, vertexPointer);
            GetFaceBytes().CopyTo(outBytes, indexPointer);
            SerializeUVs().CopyTo(outBytes, UVPointer);

            for (int i = 0; i < textureConfig.Count; i++)
            {
                WriteInt(outBytes, texturePointer + i * 0x18 + 0x00, textureConfig[i].ID);
                WriteInt(outBytes, texturePointer + i * 0x18 + 0x04, -1);
                WriteInt(outBytes, texturePointer + i * 0x18 + 0x08, textureConfig[i].start);
                WriteInt(outBytes, texturePointer + i * 0x18 + 0x0C, textureConfig[i].size);
                WriteInt(outBytes, texturePointer + i * 0x18 + 0x10, 0);
                WriteInt(outBytes, texturePointer + i * 0x18 + 0x14, textureConfig[i].mode);
            }

            return outBytes;
        }
    }
}
