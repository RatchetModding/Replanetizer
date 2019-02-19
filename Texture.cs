using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public short rawWidth;
        public short rawHeight;

        public short width;
        public short height;
        public int imageSize;
        public short mipMapCount;
        public int vramPointer;
        public byte[] data;
        public byte[] texHeader;
        public bool reverseRGB;
        int textureID = 0;


        public short off_06;
        public int off_08;
        public int off_0C;

        public int off_10;
        public int off_14;
        public int off_1C;

        public int off_20;

        public Texture(byte[] textureBlock, int offset)
        {
            ID = offset;
            vramPointer = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x00);
            mipMapCount = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x04);
            off_06 = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x06);
            off_08 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x08);
            off_0C = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x0C);

            off_10 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x10);
            off_14 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x14);
            rawWidth = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x18);
            rawHeight = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x1A);
            off_1C = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x1C);

            off_20 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x20);

            height = rawHeight;
            width = rawWidth;
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x24];

            WriteInt(ref outBytes, 0x00, vramPointer);
            WriteShort(ref outBytes, 0x04, mipMapCount);
            WriteShort(ref outBytes, 0x06, off_06);
            WriteInt(ref outBytes, 0x08, off_08);
            WriteInt(ref outBytes, 0x0C, off_0C);

            WriteInt(ref outBytes, 0x10, off_10);
            WriteInt(ref outBytes, 0x14, off_14);
            WriteShort(ref outBytes, 0x18, rawWidth);
            WriteShort(ref outBytes, 0x1A, rawHeight);
            WriteInt(ref outBytes, 0x1C, off_1C);

            WriteInt(ref outBytes, 0x20, off_20);

            return outBytes;
        }

        public int getTexture()
        {
            if (textureID == 0)
            {
                GL.GenTextures(1, out textureID);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                int offset = 0;

                if (mipMapCount > 1)
                {
                    for (int level = 0; level < mipMapCount; level++)
                    {
                        if (width > 0 && height > 0)
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
                }
                else
                {
                    int size = ((width + 3) / 4) * ((height + 3) / 4) * 16;
                    GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedRgbaS3tcDxt5Ext, width, height, 0, size, data);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                //Console.WriteLine("Created new texture with ID: " + textureID.ToString());
            }
            return textureID;
        }


        public byte[] GetTexture2D()
        {
            byte[] g = new byte[1000];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Bitmap, g);
            return g;
        }
    }
}
