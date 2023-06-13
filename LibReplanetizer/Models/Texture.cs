// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer
{
    public class Texture
    {
        public const int TEXTUREELEMSIZE = 0x24;

        public Image? renderedImage;
        public Bitmap? img;

        public short width;
        public short height;
        public short mipMapCount;
        public int vramPointer;
        public byte[] data = new byte[0];

        public short off06;
        public int off08;
        public int off0C;

        public int off10;
        public int off14;
        public int off1C;

        public int off20;

        public int id;


        public Texture(int id, short width, short height, byte[] data)
        {
            this.id = id;
            this.width = width;
            this.height = height;
            this.data = data;

            mipMapCount = 1;
            off06 = unchecked((short) 0x8829);
            off08 = 0x00010101;
            off0C = unchecked((int) 0x80030000);

            off10 = 0x0000AAE4;
            off14 = 0x02063E80;
            off1C = 0x00100000;

            off20 = 0x00FF0000;
        }

        public Texture(byte[] textureBlock, int offset)
        {
            vramPointer = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x00);
            mipMapCount = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x04);
            off06 = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x06);
            off08 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x08);
            off0C = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x0C);

            off10 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x10);
            off14 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x14);
            width = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x18);
            height = ReadShort(textureBlock, (offset * TEXTUREELEMSIZE) + 0x1A);
            off1C = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x1C);

            off20 = ReadInt(textureBlock, (offset * TEXTUREELEMSIZE) + 0x20);

            id = offset;
        }

        public byte[] Serialize(int vramOffset)
        {
            byte[] outBytes = new byte[0x24];

            WriteInt(outBytes, 0x00, vramOffset);
            WriteShort(outBytes, 0x04, mipMapCount);
            WriteShort(outBytes, 0x06, off06);
            WriteInt(outBytes, 0x08, off08);
            WriteInt(outBytes, 0x0C, off0C);

            WriteInt(outBytes, 0x10, off10);
            WriteInt(outBytes, 0x14, off14);
            WriteShort(outBytes, 0x18, width);
            WriteShort(outBytes, 0x1A, height);
            WriteInt(outBytes, 0x1C, off1C);

            WriteInt(outBytes, 0x20, off20);

            return outBytes;
        }

        public Bitmap? GetTextureImage(bool includeTransparency)
        {
            if (img != null) return img;

            byte[]? imgData = DecompressDxt5(data, width, height);

            if (imgData != null)
            {
                if (!includeTransparency)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        imgData[i * 4 + 3] = 255;
                    }
                }

                img = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);
                IntPtr pNative = bmData.Scan0;
                Marshal.Copy(imgData, 0, pNative, width * height * 4);
                img.UnlockBits(bmData);
            }
            return img;
        }


        public static byte[]? DecompressDxt5(byte[] imageData, int width, int height)
        {
            if (imageData != null)
            {
                using (MemoryStream imageStream = new MemoryStream(imageData))
                    return DecompressDxt5(imageStream, width, height);
            }
            return null;
        }

        internal static byte[] DecompressDxt5(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt5Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte alpha0 = imageReader.ReadByte();
            byte alpha1 = imageReader.ReadByte();

            ulong alphaMask = (ulong) imageReader.ReadByte();
            alphaMask += (ulong) imageReader.ReadByte() << 8;
            alphaMask += (ulong) imageReader.ReadByte() << 16;
            alphaMask += (ulong) imageReader.ReadByte() << 24;
            alphaMask += (ulong) imageReader.ReadByte() << 32;
            alphaMask += (ulong) imageReader.ReadByte() << 40;

            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out b0, out g0, out r0);
            ConvertRgb565ToRgb888(c1, out b1, out g1, out r1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    uint alphaIndex = (uint) ((alphaMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (alphaIndex == 0)
                    {
                        a = alpha0;
                    }
                    else if (alphaIndex == 1)
                    {
                        a = alpha1;
                    }
                    else if (alpha0 > alpha1)
                    {
                        a = (byte) (((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7);
                    }
                    else if (alphaIndex == 6)
                    {
                        a = 0;
                    }
                    else if (alphaIndex == 7)
                    {
                        a = 0xff;
                    }
                    else
                    {
                        a = (byte) (((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5);
                    }

                    switch (index)
                    {
                        case 0:
                            r = r0;
                            g = g0;
                            b = b0;
                            break;
                        case 1:
                            r = r1;
                            g = g1;
                            b = b1;
                            break;
                        case 2:
                            r = (byte) ((2 * r0 + r1) / 3);
                            g = (byte) ((2 * g0 + g1) / 3);
                            b = (byte) ((2 * b0 + b1) / 3);
                            break;
                        case 3:
                            r = (byte) ((r0 + 2 * r1) / 3);
                            g = (byte) ((g0 + 2 * g1) / 3);
                            b = (byte) ((b0 + 2 * b1) / 3);
                            break;
                    }

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }
        private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
        {
            int temp;

            temp = (color >> 11) * 255 + 16;
            r = (byte) ((temp / 32 + temp) / 32);
            temp = ((color & 0x07E0) >> 5) * 255 + 32;
            g = (byte) ((temp / 64 + temp) / 64);
            temp = (color & 0x001F) * 255 + 16;
            b = (byte) ((temp / 32 + temp) / 32);
        }
    }
}

