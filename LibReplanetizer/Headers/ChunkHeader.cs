using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibReplanetizer.Headers
{
    public class ChunkHeader
    {
        public int terrainPointer;

        public ChunkHeader() { }

        public ChunkHeader(FileStream engineFile)
        {
            terrainPointer = 0x10;
        }
    }
}
