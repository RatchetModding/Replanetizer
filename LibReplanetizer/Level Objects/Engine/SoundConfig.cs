// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using LibReplanetizer.Models;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class SoundConfig
    {
        public List<int> levelSounds = new List<int>();
        public Dictionary<short, List<int>> mobySounds = new Dictionary<short, List<int>>();
        public SoundConfig()
        {

        }

        public SoundConfig(byte[] block, List<Model> mobyModels)
        {
            int levelSoundsOffset = ReadShort(block, 0x04);
            int levelSoundsCount = ReadShort(block, 0x06);

            for (int i = 0; i < levelSoundsCount; i++)
                levelSounds.Add(ReadInt(block, levelSoundsOffset + i * 4));

            for (int i = 0; i < mobyModels.Count; i++)
            {
                int entryOffset = 0x08 + i * 4;

                int mobySoundsOffset = ReadShort(block, entryOffset + 0x00);
                int mobySoundsCount = ReadShort(block, entryOffset + 0x02);

                var sounds = new List<int>();
                for (int slot = 0; slot < mobySoundsCount; slot++)
                    sounds.Add(ReadInt(block, mobySoundsOffset + slot * 4));

                var oClass = mobyModels[i].id;
                mobySounds[oClass] = sounds;
            }
        }
    }
}
