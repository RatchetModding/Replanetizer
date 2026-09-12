// Copyright (C) 2018-2026, The Replanetizer Contributors.
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
namespace LibReplanetizer.Parsers
{
    public class SpaceshipParser : RatchetFileParser, IDisposable
    {
        private readonly SpaceshipHeader header;
        private readonly GameType game;
        public SpaceshipParser(GameType game, string spaceshipFile, int spaceshipNum) : base(spaceshipFile)
        {
            this.game = game;
            header = new SpaceshipHeader(game, fileStream, spaceshipNum);
        }

        public MobyModel GetShipModel() => game == GameType.RaC2 || game == GameType.RaC3
            ? MobyModel.GetStandaloneMobyModel(fileStream, game, header.shipModelID)
            : new MobyModel(fileStream, game, header.shipModelID, header.shipModelPointer);
        public MobyModel GetCockpitModel() => new MobyModel(fileStream, game, header.cockpitModelID, header.cockpitModelPointer);
        public List<Texture> GetTextures() => GetTextures(header.texturePointer, header.textureCount);

        private static List<Texture> GetRaC23Textures(GameType game, string enginePath)
        {
            string? folder = Path.GetDirectoryName(Path.GetDirectoryName(enginePath));

            List<Texture> textures = new List<Texture>();
            int numTextures = (game == GameType.RaC2) ? 18 : 32;

            for (int textureID = 0; textureID < numTextures; textureID++)
            {
                string texturePath = Path.Join(folder, "global/spaceships", $"ship_tex{textureID}.ps3");
                if (!File.Exists(texturePath))
                    continue;

                byte[] textureBlock = File.ReadAllBytes(texturePath);
                if (textureBlock.Length != Texture.TEXTUREELEMSIZE)
                    continue;

                Texture texture = new Texture(textureBlock, 0);
                string vramPath = Path.ChangeExtension(texturePath, ".vram");
                if (File.Exists(vramPath))
                {
                    using VramParser vramParser = new VramParser(vramPath);
                    vramParser.GetTextures(new List<Texture> { texture });
                }

                textures.Add(texture);
            }

            return textures;
        }

        public static (List<MobyModel> models, List<Texture> textures) GetAllSpaceshipData(GameType game, string enginePath)
        {
            List<MobyModel> models = new List<MobyModel>();
            List<Texture> textures = new List<Texture>();

            foreach (var (spaceshipNum, path) in SpaceshipHeader.FindSpaceshipFiles(game, enginePath))
            {
                using (SpaceshipParser parser = new SpaceshipParser(game, path, spaceshipNum))
                {
                    MobyModel shipModel = parser.GetShipModel();
                    if (game == GameType.RaC1)
                    {
                        MobyModel cockpitModel = parser.GetCockpitModel();

                        foreach (TextureConfig conf in shipModel.textureConfig)
                            conf.id += textures.Count;

                        foreach (TextureConfig conf in cockpitModel.textureConfig)
                            conf.id += textures.Count + 1;

                        List<Texture> fileTextures = parser.GetTextures();
                        string vramPath = Path.ChangeExtension(path, ".vram");

                        using (VramParser vramParser = new VramParser(vramPath))
                            vramParser.GetTextures(fileTextures);

                        models.Add(cockpitModel);
                        textures.AddRange(fileTextures);
                    }
                    else shipModel.id = (short) (SpaceshipHeader.RAC23_SPACESHIP_OCLASS + models.Count);
                    models.Add(shipModel);
                }
            }

            if (game == GameType.RaC2 || game == GameType.RaC3)
            {
                textures = GetRaC23Textures(game, enginePath);
            }

            return (models, textures);
        }

        private static List<short> RAC23_SPACESHIP_ATTACHMENT_MODEL_IDS = new List<short> { 4308, 4309, 4310, 4311, 4312, 4313, 4314, 4315, 4316, 4317, 4318, 4319, 4320, 4321, 4322, 4323, 4324, 4325 };

        public static List<short> GetAllSpaceshipAttachmentModelIDs(GameType game)
        {
            if (game == GameType.RaC2 || game == GameType.RaC3)
                return RAC23_SPACESHIP_ATTACHMENT_MODEL_IDS;

            return new List<short>();
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
