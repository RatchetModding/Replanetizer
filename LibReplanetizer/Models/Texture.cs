// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using SixLabors.ImageSharp;
using System.IO;
using System.Runtime.InteropServices;
using static LibReplanetizer.DataFunctions;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;

namespace LibReplanetizer
{
    public class Texture
    {
        public enum CompressionFormat : byte
        {
            BC1 = 0x86,
            BC2 = 0x87,
            BC3 = 0x88
        }

        public const int TEXTUREELEMSIZE = 0x24;

        public Image? renderedImage;
        public Image? img;

        public short width;
        public short height;
        public int vramPointer;
        public byte[] data = new byte[0];

        public byte unk0x04;
        public byte mipMapCount;
        public CompressionFormat compressionFormat;
        public byte unk0x07;

        public int off08;
        public int off0C;

        public int gtfFlags;
        public int off14;
        public int off1C;

        public int off20;

        public int id;


        public Texture(int id, short width, short height, byte[] data, CompressionFormat format = CompressionFormat.BC3)
        {
            this.id = id;
            this.data = data;

            unk0x04 = 0;
            mipMapCount = 1;
            compressionFormat = format;
            unk0x07 = (byte) 0x29;
            off08 = 0x00010101;
            off0C = unchecked((int) 0x80030000);

            gtfFlags = 0x0000AAE4;
            off14 = 0x02063E80;
            this.width = width;
            this.height = height;
            off1C = 0x00100000;

            off20 = 0x00FF0000;
        }

        public Texture(byte[] textureBlock, int index)
        {
            int offset = index * TEXTUREELEMSIZE;

            vramPointer = ReadInt(textureBlock, offset + 0x00);
            unk0x04 = textureBlock[offset + 0x04];
            mipMapCount = textureBlock[offset + 0x05];
            compressionFormat = (CompressionFormat) textureBlock[offset + 0x06];
            unk0x07 = textureBlock[offset + 0x07];
            off08 = ReadInt(textureBlock, offset + 0x08);
            off0C = ReadInt(textureBlock, offset + 0x0C);

            gtfFlags = ReadInt(textureBlock, offset + 0x10);
            off14 = ReadInt(textureBlock, offset + 0x14);
            width = ReadShort(textureBlock, offset + 0x18);
            height = ReadShort(textureBlock, offset + 0x1A);
            off1C = ReadInt(textureBlock, offset + 0x1C);

            off20 = ReadInt(textureBlock, offset + 0x20);

            id = index;
        }

        public byte[] Serialize(int vramOffset)
        {
            byte[] outBytes = new byte[0x24];

            WriteInt(outBytes, 0x00, vramOffset);
            outBytes[0x04] = unk0x04;
            outBytes[0x05] = mipMapCount;
            outBytes[0x06] = (byte)compressionFormat;
            outBytes[0x07] = unk0x07;
            WriteInt(outBytes, 0x08, off08);
            WriteInt(outBytes, 0x0C, off0C);

            WriteInt(outBytes, 0x10, gtfFlags);
            WriteInt(outBytes, 0x14, off14);
            WriteShort(outBytes, 0x18, width);
            WriteShort(outBytes, 0x1A, height);
            WriteInt(outBytes, 0x1C, off1C);

            WriteInt(outBytes, 0x20, off20);

            return outBytes;
        }

        public Image? GetTextureImage(bool includeTransparency)
        {
            if (img != null) return img;

            byte[]? imgData = null;

            switch (compressionFormat)
            {
                case CompressionFormat.BC1:
                    imgData = DecompressDxt1(data, width, height);
                    break;
                case CompressionFormat.BC3:
                    imgData = DecompressDxt5(data, width, height);
                    break;
                default:
                    throw new NotImplementedException($"Decompressing texture with compression format {compressionFormat} is not implemented.");
            }

            if (imgData != null)
            {
                if (!includeTransparency)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        imgData[i * 4 + 3] = 255;
                    }
                }

                img = Image.LoadPixelData<Bgra32>(imgData, width, height);
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

        public static byte[]? DecompressDxt1(byte[] imageData, int width, int height)
        {
            if (imageData != null)
            {
                using (MemoryStream imageStream = new MemoryStream(imageData))
                    return DecompressDxt1(imageStream, width, height);
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

        internal static byte[] DecompressDxt1(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt1Block(imageReader, x, y, blockCountX, width, height, imageData);
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

        private static void DecompressDxt1Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
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

