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

                foreach(TextureConfig t in models[i].textureConfig)
                {
                    maxID = (maxID < t.ID + 1) ? t.ID + 1 : maxID;
                    t.ID += offset;
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
