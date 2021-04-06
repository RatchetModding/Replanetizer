using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibReplanetizer.Parsers
{
    public class ChunkParser : RatchetFileParser, IDisposable
    {

        ChunkHeader chunkHeader;

        public ChunkParser(string chunkFile) : base(chunkFile)
        {
            chunkHeader = new ChunkHeader(fileStream);
        }

        public List<TerrainFragment> GetTerrainModels()
        {
            return GetTerrainModels(chunkHeader.terrainPointer);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
