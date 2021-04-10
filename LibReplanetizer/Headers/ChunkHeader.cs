using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class ChunkHeader
    {
        public int terrainPointer;
        public int collisionPointer;

        public ChunkHeader() { }

        public ChunkHeader(FileStream chunkFile)
        {
            byte[] chunkHeaderBytes = ReadBlock(chunkFile, 0x00, 0x08);

            terrainPointer = ReadInt(chunkHeaderBytes, 0x00);
            collisionPointer = ReadInt(chunkHeaderBytes, 0x04);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            WriteInt(bytes, 0x00, terrainPointer);
            WriteInt(bytes, 0x04, collisionPointer);

            return bytes;
        }
    }
}
