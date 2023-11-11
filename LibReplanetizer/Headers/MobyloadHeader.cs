// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class MobyloadHeader
    {
        public int mobyCount;
        public int textureCount;
        public int texturePointer;
        public int textureDataPointer;
        public List<Tuple<int, int>> modelData = new List<Tuple<int, int>>();

        public MobyloadHeader(FileStream mobyloadFile)
        {
            byte[] headerBytes = ReadBlock(mobyloadFile, 0x00, 0x10);

            mobyCount = ReadInt(headerBytes, 0x00);
            textureCount = ReadInt(headerBytes, 0x04);
            texturePointer = ReadInt(headerBytes, 0x08);
            textureDataPointer = ReadInt(headerBytes, 0x0C);

            byte[] pointerBlock = ReadBlock(mobyloadFile, 0x10, mobyCount * 0x0C);

            for (int i = 0; i < mobyCount; i++)
            {
                int modelPointer = ReadInt(pointerBlock, 0x00 + i * 0x0C);
                int modelID = ReadInt(pointerBlock, 0x04 + i * 0x0C);

                modelData.Add(new Tuple<int, int>(modelPointer, modelID));
            }
        }

        public static string? FindMobyloadFile(GameType game, string enginePath, int id)
        {
            string? folder = Path.GetDirectoryName(enginePath);

            switch (game.num)
            {
                case 2:
                case 3:
                    var path = Path.Join(folder, "mobyload" + id + ".ps3");
                    if (File.Exists(path)) return path;
                    break;
            }

            return null;
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            return bytes;
        }
    }
}
