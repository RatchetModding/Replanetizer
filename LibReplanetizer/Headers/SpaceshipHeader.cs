// Copyright (C) 2018-2026, The Replanetizer Contributors.
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
    public class SpaceshipHeader
    {
        private static readonly short[][] SPACESHIP_MODEL_PAIRS = { [530, 534], [531, 535], [532, 536], [533, 537] };
        public const short RAC23_SPACESHIP_OCLASS = 3412;

        public int shipModelPointer;
        public short shipModelID;
        public int cockpitModelPointer;
        public short cockpitModelID;
        public int texturePointer;
        public int textureCount;

        public SpaceshipHeader(GameType game, FileStream fs, int spaceshipNum)
        {
            if (game == GameType.RaC2 || game == GameType.RaC3)
            {
                shipModelID = RAC23_SPACESHIP_OCLASS;
                shipModelPointer = 0;
                return;
            }

            short[] modelIDs = SPACESHIP_MODEL_PAIRS[spaceshipNum];
            shipModelID = modelIDs[0];
            cockpitModelID = modelIDs[1];

            byte[] pointerBlock = ReadBlock(fs, 0x00, 0x0C);
            shipModelPointer = ReadInt(pointerBlock, 0x00);
            cockpitModelPointer = ReadInt(pointerBlock, 0x04);
            texturePointer = ReadInt(pointerBlock, 0x08);

            textureCount = (int) (fs.Length - texturePointer) / Texture.TEXTUREELEMSIZE;
        }

        public static List<(int spaceshipNum, string path)> FindSpaceshipFiles(GameType game, string enginePath)
        {
            List<(int spaceshipNum, string path)> found = new List<(int, string)>();

            string? folder = Path.GetDirectoryName(Path.GetDirectoryName(enginePath));

            if (game == GameType.RaC1)
            {
                for (int i = 0; i < SPACESHIP_MODEL_PAIRS.Length; i++)
                {
                    string path = Path.Join(folder, "global", $"spaceship{i}.ps3");
                    if (File.Exists(path))
                    {
                        found.Add((i, path));
                    }
                }
            }
            else if (game == GameType.RaC2 || game == GameType.RaC3)
            {
                for (int i = 0; i < 3; i++)
                {
                    string path = Path.Join(folder, "global/spaceships", $"ship_bodies{i}.ps3");
                    if (File.Exists(path))
                    {
                        found.Add((i, path));
                    }
                }
            }

            return found;
        }
    }
}
