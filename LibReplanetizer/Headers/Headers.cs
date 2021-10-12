// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    class Headers
    {
    }
    public class TerrainHead
    {
        public uint headPointer;
        // this number is equal over all terrains in a level but it differs between levels
        public ushort levelNumber;
        public ushort headCount;
        public List<int> vertexPointers = new List<int>();
        public List<int> rgbaPointers = new List<int>();
        public List<int> uvPointers = new List<int>();
        public List<int> indexPointers = new List<int>();
        public List<int> unkPointers = new List<int>();

        public TerrainHead(byte[] terrainBlock, GameType game)
        {
            headPointer = ReadUint(terrainBlock, 0x00);
            levelNumber = ReadUshort(terrainBlock, 0x04);
            headCount = ReadUshort(terrainBlock, 0x06);

            switch (game.num)
            {
                case 1:
                case 2:
                case 3:
                    for (int i = 0; i < 4; i++)
                    {
                        vertexPointers.Add(ReadInt(terrainBlock, 0x08 + i * 4));
                        rgbaPointers.Add(ReadInt(terrainBlock, 0x18 + i * 4));
                        uvPointers.Add(ReadInt(terrainBlock, 0x28 + i * 4));
                        indexPointers.Add(ReadInt(terrainBlock, 0x38 + i * 4));
                    }
                    break;
                case 4:
                    for (int i = 0; i < 4; i++)
                    {
                        vertexPointers.Add(ReadInt(terrainBlock, 0x08 + i * 4));
                        rgbaPointers.Add(ReadInt(terrainBlock, 0x18 + i * 4));
                        uvPointers.Add(ReadInt(terrainBlock, 0x28 + i * 4));
                        indexPointers.Add(ReadInt(terrainBlock, 0x38 + i * 4));
                        unkPointers.Add(ReadInt(terrainBlock, 0x48 + i * 4));
                    }
                    break;
            }
        }
    }

    public class OcclusionDataHeader
    {
        public int mobyCount;
        public int tieCount;
        public int shrubCount;
        public int totalCount;

        public OcclusionDataHeader(byte[] headBlock)
        {
            mobyCount = ReadInt(headBlock, 0x00);
            tieCount = ReadInt(headBlock, 0x04);
            shrubCount = ReadInt(headBlock, 0x08);
            totalCount = mobyCount + tieCount + shrubCount;
        }
    }
}
