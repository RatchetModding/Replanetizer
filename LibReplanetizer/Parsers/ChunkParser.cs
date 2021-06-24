using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static LibReplanetizer.DataFunctions;

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

        public Model GetCollisionModel()
        {
            return GetCollisionModel(chunkHeader.collisionPointer);
        }

        /*
         * The point of this is so that we can save the collision data
         * reconstructing this block of data from a collision model is tedious
         * and at the moment since you cannot modify the collision anyway it is unnecessary
         * This here costs a few megabytes of RAM at best so it is fine
         */
        public byte[] GetCollBytes()
        {
            byte[] headBlock = ReadBlock(fileStream, chunkHeader.collisionPointer, 8);
            int collisionStart = chunkHeader.collisionPointer + ReadInt(headBlock, 0);
            int collisionLength = ReadInt(headBlock, 4);
            int totalLength = collisionStart + collisionLength - chunkHeader.collisionPointer;

            return ReadArbBytes(chunkHeader.collisionPointer, totalLength);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
