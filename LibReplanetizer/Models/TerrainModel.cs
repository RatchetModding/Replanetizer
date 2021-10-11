﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;


namespace LibReplanetizer.Models
{
    public class TerrainModel : Model
    {
        // Don't use this directly, call getIDAssigned() instead to retrieve an ID
        private static short STATIC_ID = 0;

        public List<int> lights = new List<int>();
        int faceCount;
        public TerrainModel(FileStream fs, TerrainHead head, byte[] tfragBlock, int num)
        {
            id = GetIDAssigned();
            size = 1.0f;

            int offset = num * 0x30;
            int texturePointer = ReadInt(tfragBlock, offset + 0x10);
            int textureCount = ReadInt(tfragBlock, offset + 0x14);
            ushort vertexIndex = ReadUshort(tfragBlock, offset + 0x18);
            ushort vertexCount = ReadUshort(tfragBlock, offset + 0x1A);
            ushort slotNum = ReadUshort(tfragBlock, offset + 0x22);

            // Oh yes, we are hacking
            int faceStart = ReadInt(ReadBlock(fs, texturePointer + 4, 4), 0);

            textureConfig = GetTextureConfigs(fs, texturePointer, textureCount, 0x10, true);
            faceCount = GetFaceCount();

            vertexBuffer = GetVertices(fs, head.vertexPointers[slotNum] + vertexIndex * 0x1C, head.uvPointers[slotNum] + vertexIndex * 0x08, vertexCount, 0x1C, 0x08);
            indexBuffer = GetIndices(fs, head.indexPointers[slotNum] + faceStart * 2, faceCount, vertexIndex);

            rgbas = ReadBlock(fs, head.rgbaPointers[slotNum] + vertexIndex * 4, vertexCount * 4);

            // OOOf hack
            byte[] vertBlock = ReadBlock(fs, head.vertexPointers[slotNum] + vertexIndex * 0x1C, vertexCount * 0x1C);
            for (int i = 0; i < vertBlock.Length / 0x1c; i++)
            {
                lights.Add(ReadInt(vertBlock, i * 0x1c + 0x18));
            }

        }

        public byte[] SerializeVerts()
        {
            int elemSize = 0x1C;
            byte[] outBytes = new byte[(vertexBuffer.Length / 8) * elemSize];

            for (int i = 0; i < vertexBuffer.Length / 8; i++)
            {
                int offset = i * elemSize;
                WriteFloat(outBytes, offset + 0x00, vertexBuffer[(i * 8) + 0]);
                WriteFloat(outBytes, offset + 0x04, vertexBuffer[(i * 8) + 1]);
                WriteFloat(outBytes, offset + 0x08, vertexBuffer[(i * 8) + 2]);
                WriteFloat(outBytes, offset + 0x0C, vertexBuffer[(i * 8) + 3]);
                WriteFloat(outBytes, offset + 0x10, vertexBuffer[(i * 8) + 4]);
                WriteFloat(outBytes, offset + 0x14, vertexBuffer[(i * 8) + 5]);
                WriteInt(outBytes, offset + 0x18, lights[i]);
            }

            return outBytes;
        }

        private static short GetIDAssigned()
        {
            return STATIC_ID++;
        }

    }
}
