// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class GadgetHeader
    {
        public List<int> modelPointers = new List<int>();
        public int texturePointer;
        public int textureCount;

        // Are they always 47? If not I would expect the count somewhere
        private const int GADGET_COUNT = 47;

        public GadgetHeader() { }


        public GadgetHeader(FileStream gadgetFile)
        {
            byte[] gadgetHeaderBytes = ReadBlock(gadgetFile, 0x00, GADGET_COUNT * 0x04);

            modelPointers = new List<int>();

            for (int i = 0; i < GADGET_COUNT; i++)
            {
                modelPointers.Add(ReadInt(gadgetHeaderBytes, i * 0x04));
            }

            texturePointer = ReadInt(ReadBlock(gadgetFile, 0x3C4, 0x04), 0x00);

            textureCount = 0;

            for (int i = texturePointer; i < gadgetFile.Length; i += 0x24)
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
                    var rac2Path = Path.Join(superFolder, "global", "gadgets", "gadgets.ps3");
                    if (File.Exists(rac2Path)) return rac2Path;
                    break;
                case 3:
                    var rac3Path = Path.Join(superFolder, "global", "gadgets.ps3");
                    if (File.Exists(rac3Path)) return rac3Path;
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
