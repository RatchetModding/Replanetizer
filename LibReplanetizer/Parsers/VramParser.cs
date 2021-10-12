// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    class VramParser : RatchetFileParser, IDisposable
    {
        public bool valid = false;

        public VramParser(string filePath) : base(filePath)
        {
            valid = File.Exists(filePath);
        }

        public void GetTextures(List<Texture> textures)
        {
            if (!valid) return;

            for (int i = 0; i < textures.Count; i++)
            {
                int length;
                if (i < textures.Count - 1)
                {
                    length = (int) (textures[i + 1].vramPointer - textures[i].vramPointer);
                }
                else
                {
                    length = (int) (fileStream.Length - textures[i].vramPointer);
                }
                textures[i].data = ReadBlock(fileStream, textures[i].vramPointer, length);
            }
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
