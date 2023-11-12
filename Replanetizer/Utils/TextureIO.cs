// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.IO;
using LibReplanetizer;
using LibReplanetizer.Models;
using SixLabors.ImageSharp.Formats.Png;

namespace Replanetizer.Utils
{
    public static class TextureIO
    {
        public static void ExportTexture(Texture texture, string path, bool includeTransparency)
        {
            string extension = Path.GetExtension(path).ToLower();

            Image? image = texture.GetTextureImage(includeTransparency);

            if (image == null) return;

            switch (extension)
            {
                case ".bmp":
                    image.SaveAsBmp(path);
                    break;
                case ".jpg":
                case ".jpeg":
                    image.SaveAsJpeg(path);
                    break;
                default:
                    image.SaveAsPng(path);
                    break;
            }
        }

        public static void ExportAllTextures(Level level, string path)
        {
            bool[] forcedOpaque = new bool[level.textures.Count];
            if (level.game == GameType.RaC3)
            {
                foreach (TieModel model in level.tieModels)
                {
                    foreach (TextureConfig conf in model.textureConfig)
                    {
                        if (conf.IgnoresTransparency())
                        {
                            forcedOpaque[conf.id] = true;
                        }
                    }
                }

                foreach (MobyModel model in level.mobyModels)
                {
                    foreach (TextureConfig conf in model.textureConfig)
                    {
                        if (conf.IgnoresTransparency())
                        {
                            forcedOpaque[conf.id] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < level.textures.Count; i++)
            {
                ExportTexture(level.textures[i], Path.Join(path, $"{i}.png"), !forcedOpaque[i]);
            }

            for (int i = 0; i < level.armorTextures.Count; i++)
            {
                List<Texture> textures = level.armorTextures[i];
                for (int j = 0; j < textures.Count; j++)
                {
                    ExportTexture(textures[j], Path.Join(path, $"armor_{i}_{j}.png"), true);
                }
            }

            for (int i = 0; i < level.gadgetTextures.Count; i++)
            {
                ExportTexture(level.gadgetTextures[i], Path.Join(path, $"gadget_{i}.png"), true);
            }

            for (int i = 0; i < level.missions.Count; i++)
            {
                List<Texture> textures = level.missions[i].textures;
                for (int j = 0; j < textures.Count; j++)
                {
                    ExportTexture(textures[j], Path.Join(path, $"mission_{i}_{j}.png"), true);
                }
            }

            for (int i = 0; i < level.mobyloadTextures.Count; i++)
            {
                List<Texture> textures = level.missions[i].textures;
                for (int j = 0; j < textures.Count; j++)
                {
                    ExportTexture(textures[j], Path.Join(path, $"mobyload_{i}_{j}.png"), true);
                }
            }
        }
    }
}
