// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using System;
using System.IO;
using static LibReplanetizer.Serializers.SerializerFunctions;

namespace LibReplanetizer.Serializers
{
    public class ChunkSerializer
    {
        public void Save(Level level, string directory, int chunk)
        {
            if (chunk >= level.terrainChunks.Count)
                throw new IndexOutOfRangeException("Chunk does not exist!");

            directory = Path.Join(directory, "chunk" + chunk + ".ps3");

            FileStream fs = File.Open(directory, FileMode.Create);

            // Seek past the header
            fs.Seek(0x10, SeekOrigin.Begin);

            ChunkHeader chunkHeader = new ChunkHeader()
            {
                terrainPointer = SeekWrite(fs, WriteTfrags(level.terrainChunks[chunk], (int) fs.Position, level.game)),
                collisionPointer = SeekWrite(fs, level.collBytesChunks[chunk])
            };

            byte[] head = chunkHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
        }

        private int SeekWrite(FileStream fs, byte[] bytes)
        {
            int pos = (int) fs.Position;
            fs.Write(bytes, 0, bytes.Length);
            SeekPast(fs);
            return pos;
        }

        private void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }


    }
}
