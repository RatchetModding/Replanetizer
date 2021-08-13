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
            var bitmap = texture.getTextureImage();
            bitmap.Save(path, imageFormat);
        }

        public static void ExportAllTextures(Level level, string path)
        {
            for (int i = 0; i < level.textures.Count; i++)
            {
                Bitmap image = level.textures[i].getTextureImage();
                image.Save(Path.Join(path, $"texture_{i}.png"), ImageFormat.Png);
            }

            for (int i = 0; i < level.armorTextures.Count; i++)
            {
                List<Texture> textures = level.armorTextures[i];
                for (int j = 0; j < textures.Count; j++)
                {
                    Bitmap image = textures[j].getTextureImage();
                    image.Save(Path.Join(path, $"armor_{i}_{j}.png"), ImageFormat.Png);
                }
            }

            for (int i = 0; i < level.gadgetTextures.Count; i++)
            {
                Bitmap image = level.gadgetTextures[i].getTextureImage();
                image.Save(Path.Join(path, $"gadget_{i}.png"), ImageFormat.Png);
            }

            for (int i = 0; i < level.missions.Count; i++)
            {
                List<Texture> textures = level.missions[i].textures;
                for (int j = 0; j < textures.Count; j++)
                {
                    Bitmap image = textures[j].getTextureImage();
                    image.Save(Path.Join(path, $"mission_{i}_{j}.png"), ImageFormat.Png);
                }
            }
        }
    }
}