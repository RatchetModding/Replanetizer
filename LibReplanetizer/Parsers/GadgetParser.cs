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
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    public class GadgetParser : RatchetFileParser, IDisposable
    {
        GadgetHeader gadgetHead;
        GameType game;

        public GadgetParser(GameType game, string gadgetFile) : base(gadgetFile)
        {
            this.game = game;
            gadgetHead = new GadgetHeader(fileStream);
        }

        public List<Texture> GetTextures()
        {
            return GetTextures(gadgetHead.texturePointer, gadgetHead.textureCount);
        }

        public List<MobyModel> GetModels()
        {
            List<MobyModel> models = new List<MobyModel>();

            foreach (int modelPointer in gadgetHead.modelPointers)
            {
                models.Add(MobyModel.GetGadgetMobyModel(fileStream, modelPointer));
            }

            // textureID is always 0 based hence we shift them so that they match with the textures
            int offset = 0;

            for (int i = 0; i < models.Count; i++)
            {
                int maxID = 0;

                foreach (TextureConfig t in models[i].textureConfig)
                {
                    maxID = (maxID < t.id + 1) ? t.id + 1 : maxID;
                    t.id += offset;
                }

                offset += maxID;
            }

            return models;
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
