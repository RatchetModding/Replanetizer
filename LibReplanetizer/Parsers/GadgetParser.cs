// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;

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

            foreach (Tuple<int, int> model in gadgetHead.modelData)
            {
                // ID of zero implies that something wrong and this model is to be ignored.
                if (model.Item2 != 0)
                {
                    models.Add(new MobyModel(fileStream, game, (short) model.Item2, model.Item1));
                }
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
