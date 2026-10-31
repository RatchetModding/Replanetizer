// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Part
    {
        public int id;
        public List<int> sprites;

        public Part(byte[] partBlock, int id, byte[] spriteBlock, int baseTextureIndex)
        {
            this.id = id;
            sprites = new List<int>();

            int start = ReadInt(partBlock, id * 4);
            if (id != 0 && start == 0)
                return;

            int end = spriteBlock.Length;
            for (int i = id + 1; i < partBlock.Length / 4 && end == spriteBlock.Length; i++)
                if (ReadInt(partBlock, i * 4) != 0)
                    end = ReadInt(partBlock, i * 4);

            for (int i = start; i < end; i++)
                sprites.Add(baseTextureIndex + spriteBlock[i]);
        }
    }
}
