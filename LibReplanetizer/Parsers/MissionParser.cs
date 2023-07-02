// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    public class MissionParser : RatchetFileParser, IDisposable
    {
        MissionHeader missionHead;
        GameType game;

        public MissionParser(GameType game, string armorFile) : base(armorFile)
        {
            this.game = game;
            missionHead = new MissionHeader(fileStream);
        }

        public List<Model> GetModels()
        {
            List<Model> models = new List<Model>();

            byte[] mobyBlock = ReadBlock(fileStream, 0x10, missionHead.mobiesCount * 0x08);

            List<Tuple<int, int>> modelData = new List<Tuple<int, int>>();
            for (int i = 0; i < missionHead.mobiesCount; i++)
            {
                short modelID = ReadShort(mobyBlock, (i * 0x08) + 0x02);
                int offset = ReadInt(mobyBlock, (i * 0x08) + 0x04);

                modelData.Add(new Tuple<int, int>(offset, modelID));
            }

            foreach (Tuple<int, int> model in modelData)
            {
                // ID of zero implies that something wrong and this model is to be ignored.
                if (model.Item2 != 0)
                {
                    models.Add(new MobyModel(fileStream, game, (short) model.Item2, model.Item1));
                }
            }

            return models;
        }

        public List<Texture> GetTextures()
        {
            return GetTextures(missionHead.texturePointer, missionHead.textureCount);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
