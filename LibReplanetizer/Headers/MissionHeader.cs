// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class MissionHeader
    {
        public int mobiesCount;
        public int textureCount;
        public int texturePointer;

        public MissionHeader() { }


        public MissionHeader(FileStream missionFile)
        {
            byte[] missionHeaderBytes = ReadBlock(missionFile, 0x00, 0x10);

            mobiesCount = ReadInt(missionHeaderBytes, 0x00);
            textureCount = ReadInt(missionHeaderBytes, 0x04);
            texturePointer = ReadInt(missionHeaderBytes, 0x08);
            // 0x0C always 0
        }

        /*
         * Assumes default folder structure, that is, mission files lie in the same folder as the engine file
         */
        public static List<string> FindMissionFiles(GameType game, string enginePath)
        {
            string? dir = Path.GetDirectoryName(enginePath);

            List<string> files = new List<string>();

            if (dir == null) return files;

            if (game == GameType.DL)
            {
                files.AddRange(Directory.GetFiles(dir, "gameplay_mission_classes[*].ps3"));
            }

            return files;
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            WriteInt(bytes, 0x00, mobiesCount);
            WriteInt(bytes, 0x04, textureCount);
            WriteInt(bytes, 0x08, texturePointer);

            return bytes;
        }
    }
}
