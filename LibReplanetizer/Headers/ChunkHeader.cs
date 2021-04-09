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
    }
}
