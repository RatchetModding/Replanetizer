using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Texture
    {
        public const int TEXTUREELEMSIZE = 0x24;

        public int ID;
        public int width;
        public int height;
        public int imageSize;
        public int mipMapCount;
        public uint vramPointer;
        public byte[] data;
        public byte[] texHeader;
        public bool reverseRGB;
        int textureID = 0;

        public Texture(byte[] textureBlock, int offset)
        {
            ID = offset;
            vramPointer =   ReadUint(textureBlock, (offset * TEXTUREELEMSIZE));
            width =         ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x18);
            height =        ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x1A);
            mipMapCount =   ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x1C);
        }

        public int getTexture()
        {
            if(textureID == 0)
            {
                GL.GenTextures(1, out textureID);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                int offset = 0;
                for (int level = 0; level < mipMapCount; level++)
                {
                    if(width > 0 && height > 0)
                    {
                        int size = ((width + 3) / 4) * ((height + 3) / 4) * 16;
                        byte[] texPart = new byte[size];
                        Array.Copy(data, offset, texPart, 0, size);
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, level, InternalFormat.CompressedRgbaS3tcDxt5Ext, width, height, 0, size, texPart);
                        offset += size;
                        width /= 2;
                        height /= 2;
                    }
                }
                //Console.WriteLine("Created new texture with ID: " + textureID.ToString());
            }
            return textureID;
        }
    }
}
