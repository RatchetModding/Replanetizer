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

        public MobyModel GetArmor()
        {
            if (armorHead.modelPointer == 0) return null;

            if (game.num == 4)
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
