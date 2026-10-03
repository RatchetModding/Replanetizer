// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.IO;
using LibReplanetizer;
using LibReplanetizer.Models;
using SixLabors.ImageSharp.Formats.Png;
using System.Linq;
using System.Text;

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
                case ".dds":
                    byte[] dds = ConstructDDS(texture);
                    using (var fs = new FileStream(path, FileMode.Create))
                        fs.Write(dds, 0, dds.Length);
                    break;

                default:
                    PngEncoder pngEncoder = new PngEncoder() {
                        Gamma = 1.0f / 2.2f,                                        // Textures are sRGB encoded
                        ColorType = PngColorType.RgbWithAlpha,                        
                        TransparentColorMode = PngTransparentColorMode.Preserve,    // Some textures in RaC 3 are fully transparent but are rendered without transparency.
                        BitDepth = PngBitDepth.Bit8,
                        InterlaceMethod = PngInterlaceMode.None,                    // Interlacing may not be supported by all PNG importers.
                        FilterMethod = PngFilterMethod.Paeth,                       // Combined filter method may not be supported by all PNG importers.
                        CompressionLevel = PngCompressionLevel.BestCompression      // Textures are small so compression speed is not a huge issue.
                    };
                    image.SaveAsPng(path, pngEncoder);
                    break;
            }
        }

        public static void ExportAllTextures(Level level, string path, string extension)
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
                ExportTexture(level.textures[i], Path.Join(path, $"{i}{extension}"), !forcedOpaque[i]);
            }

            for (int i = 0; i < level.armorTextures.Count; i++)
            {
                List<Texture> textures = level.armorTextures[i];
                for (int j = 0; j < textures.Count; j++)
                {
                    ExportTexture(textures[j], Path.Join(path, $"armor_{i}_{j}{extension}"), true);
                }
            }

            for (int i = 0; i < level.gadgetTextures.Count; i++)
            {
                ExportTexture(level.gadgetTextures[i], Path.Join(path, $"gadget_{i}{extension}"), true);
            }

            for (int i = 0; i < level.missions.Count; i++)
            {
                List<Texture> textures = level.missions[i].textures;
                for (int j = 0; j < textures.Count; j++)
                {
                    ExportTexture(textures[j], Path.Join(path, $"mission_{i}_{j}{extension}"), true);
                }
            }

            for (int i = 0; i < level.mobyloadTextures.Count; i++)
            {
                List<Texture> textures = level.mobyloadTextures[i];
                for (int j = 0; j < textures.Count; j++)
                {
                    ExportTexture(textures[j], Path.Join(path, $"mobyload_{i}_{j}{extension}"), true);
                }
            }
        }
        public static byte[] ConstructDDS(Texture texture)
        {
            byte[] head = new byte[128]
            {
                // Copied header from GIMP output
                // This does not win me the nobel price
                0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x0A, 0x00, 0x00, 0x01, 0x00, 0x00,
                0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00,
                0x52, 0x45, 0x50, 0x4c, 0x41, 0x4E, 0x00, 0x00, 0x00, 0x09, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00, 0x44, 0x58, 0x54, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x10, 0x40, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            BitConverter.GetBytes((int)texture.height).CopyTo(head, 0x0C);
            BitConverter.GetBytes((int)texture.width).CopyTo(head, 0x10);
            BitConverter.GetBytes((int)texture.mipMapCount).CopyTo(head, 0x1C);

            return head.Concat(texture.data).ToArray();
        }
        public static byte[] ImportDDSTexture(string path, int len)
        {
            byte[] file = File.ReadAllBytes(path);
            byte[] data = new byte[len];
            Buffer.BlockCopy(file, 128, data, 0, len);

            // Adds trailing zeros if it came from outside of replanetizer
            if (Encoding.ASCII.GetString(file, 0x20, 6) != "REPLAN") 
                Array.Resize(ref data, data.Length + 16);

            return data;
        }
        public static (short height, short width, int len) ReadDDSHeader(string path)
        {
            byte[] ddsHead = new byte[128];
            long length;
            using (var fs = File.OpenRead(path))
            {
                fs.Read(ddsHead, 0, 128);
                length = fs.Length - 128;
            }

            short height = BitConverter.ToInt16(ddsHead, 12);
            short width = BitConverter.ToInt16(ddsHead, 16);

            return (height, width, (int) length);
        }
    }
}
