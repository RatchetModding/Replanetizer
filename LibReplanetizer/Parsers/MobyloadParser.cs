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
    public class MobyloadParser : RatchetFileParser, IDisposable
    {
        MobyloadHeader mobyloadHead;
        GameType game;

        public MobyloadParser(GameType game, string mobyloadFile) : base(mobyloadFile)
        {
            this.game = game;
            mobyloadHead = new MobyloadHeader(fileStream);
        }

        public List<Texture> GetTextures()
        {
            return GetTexturesMobyload(mobyloadHead.texturePointer, mobyloadHead.textureCount);
        }

        public List<MobyModel> GetMobyModels()
        {
            List<MobyModel> models = new List<MobyModel>();

            foreach (Tuple<int, int> model in mobyloadHead.modelData)
            {
                // ID of zero implies that something wrong and this model is to be ignored.
                if (model.Item2 != 0)
                {
                    models.Add(new MobyModel(fileStream, game, (short) model.Item2, model.Item1));
                }
            }

            foreach (MobyModel model in models)
            {
                for (int i = 0; i < model.vertexCount; i++)
                {
                    // This fixes the UVs on Tyhrranosis Mobyload2.
                    // Completely unclear how this is handled correctly.
                    model.vertexBuffer[i * 8 + 6] += 0.285f;
                }

            }

            return models;

        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
