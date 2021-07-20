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
            for (int i = 0; i < missionHead.mobiesCount; i++)
            {
                short modelID = ReadShort(mobyBlock, (i * 0x08) + 0x02);
                int offset = ReadInt(mobyBlock, (i * 0x08) + 0x04);

                if (offset != 0)
                {
                    MobyModel model = MobyModel.GetGadgetMobyModel(fileStream, offset);
                    model.id = modelID;
                    models.Add(model);
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
