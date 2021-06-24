using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static LibReplanetizer.DataFunctions;

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

        public List<Model> GetArmor()
        {
            return GetMobyModels(game, armorHead.modelPointer);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
