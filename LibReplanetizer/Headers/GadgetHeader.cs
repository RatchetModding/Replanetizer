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
    public class GadgetHeader
    {
        public List<Tuple<int, int>> modelData = new List<Tuple<int, int>>();
        public int texturePointer;
        public int textureCount;

        public GadgetHeader(FileStream gadgetFile)
        {
            int gadgetCount = ReadInt(ReadBlock(gadgetFile, 0x3C0, 0x04), 0x00);

            byte[] gadgetHeaderBytes = ReadBlock(gadgetFile, 0x00, 0x3D0);

            for (int i = 0; i < gadgetCount; i++)
            {
                int modelPointer = ReadInt(gadgetHeaderBytes, 0x00 + i * 0x04);
                int modelID = ReadInt(gadgetHeaderBytes, 0x240 + i * 0x04);

                modelData.Add(new Tuple<int, int>(modelPointer, modelID));
            }

            texturePointer = ReadInt(ReadBlock(gadgetFile, 0x3C4, 0x04), 0x00);

            textureCount = 0;

            for (int i = texturePointer; i < gadgetFile.Length; i += Texture.TEXTUREELEMSIZE)
            {
                textureCount++;
            }
        }

        /*
         * Assumes default folder structure, i.e.
         * global
         *
         * levelK
         *      engine.ps3
         */
        public static string FindGadgetFile(GameType game, string enginePath)
        {
            string? superFolder = Path.GetDirectoryName(Path.GetDirectoryName(enginePath));

            switch (game.num)
            {
                case 2:
                case 3:
                    var path = Path.Join(superFolder, "global", "gadgets.ps3");
                    if (File.Exists(path)) return path;
                    break;
            }

            return "";
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            /*WriteInt(bytes, 0x00, modelPointer);
            WriteInt(bytes, 0x04, texturePointer);
            WriteInt(bytes, 0x08, textureCount);*/

            return bytes;
        }
    }
}
