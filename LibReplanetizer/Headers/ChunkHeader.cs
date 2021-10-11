// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;
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
