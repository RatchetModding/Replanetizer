// Copyright (C) 2018-2021, The Replanetizer Contributors.
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
    public class ArmorParser : RatchetFileParser, IDisposable
    {
        ArmorHeader armorHead;
        GameType game;

        public ArmorParser(GameType game, string armorFile) : base(armorFile)
        {
            this.game = game;
            armorHead = new ArmorHeader(fileStream);
        }

        public List<Texture> GetTextures()
        {
            return GetTextures(armorHead.texturePointer, armorHead.textureCount);
        }

        public MobyModel? GetArmor()
        {
            if (armorHead.modelPointer == 0) return null;

            if (game == GameType.DL)
            {
                return new MobyModel(fileStream, game, 0, armorHead.modelPointer);
            }
            else
            {
                return MobyModel.GetArmorMobyModel(fileStream, armorHead.modelPointer);
            }
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
