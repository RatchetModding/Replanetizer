// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using LibReplanetizer;

namespace Replanetizer.Utils
{
    public static class TextureIO
    {
        public static void ExportTexture(Texture texture, string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            ImageFormat imageFormat = ImageFormat.Png;

            switch (extension)
            {
                case ".bmp":
                    imageFormat = ImageFormat.Bmp;
                    break;
                case ".jpg":
                case ".jpeg":
                    imageFormat = ImageFormat.Jpeg;
                    break;
            }
            var bitmap = texture.GetTextureImage();
            bitmap.Save(path, imageFormat);
        }

        public static void ExportAllTextures(Level level, string path)
        {
            for (int i = 0; i < level.textures.Count; i++)
            {
                Bitmap image = level.textures[i].GetTextureImage();
                image.Save(Path.Join(path, $"{i}.png"), ImageFormat.Png);
            }

            for (int i = 0; i < level.armorTextures.Count; i++)
            {
                List<Texture> textures = level.armorTextures[i];
                for (int j = 0; j < textures.Count; j++)
                {
                    Bitmap image = textures[j].GetTextureImage();
                    image.Save(Path.Join(path, $"armor_{i}_{j}.png"), ImageFormat.Png);
                }
            }

            for (int i = 0; i < level.gadgetTextures.Count; i++)
            {
                Bitmap image = level.gadgetTextures[i].GetTextureImage();
                image.Save(Path.Join(path, $"gadget_{i}.png"), ImageFormat.Png);
            }

            for (int i = 0; i < level.missions.Count; i++)
            {
                List<Texture> textures = level.missions[i].textures;
                for (int j = 0; j < textures.Count; j++)
                {
                    Bitmap image = textures[j].GetTextureImage();
                    image.Save(Path.Join(path, $"mission_{i}_{j}.png"), ImageFormat.Png);
                }
            }
        }
    }
}