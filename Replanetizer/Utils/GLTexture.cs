// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Replanetizer.Utils
{
    public enum TextureCoordinate
    {
        S = TextureParameterName.TextureWrapS,
        T = TextureParameterName.TextureWrapT,
        R = TextureParameterName.TextureWrapR
    }

    public class GLTexture : IDisposable
    {
        public const SizedInternalFormat SRGB8_ALPHA8 = (SizedInternalFormat) All.Srgb8Alpha8;
        public const SizedInternalFormat RGB32F = (SizedInternalFormat) All.Rgb32f;

        public const GetPName MAX_TEXTURE_MAX_ANISOTROPY = (GetPName) 0x84FF;

        public static readonly float MAX_ANISO;

        static GLTexture()
        {
            MAX_ANISO = GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPY);
        }

        public readonly string NAME;
        public readonly int TEXTURE;
        public readonly int WIDTH, HEIGHT;
        public readonly int MIPMAP_LEVELS;
        public readonly SizedInternalFormat INTERNAL_FORMAT;

        public GLTexture(string name, Bitmap image, bool generateMipmaps, bool srgb)
        {
            NAME = name;
            WIDTH = image.Width;
            HEIGHT = image.Height;
            INTERNAL_FORMAT = srgb ? SRGB8_ALPHA8 : SizedInternalFormat.Rgba8;

            if (generateMipmaps)
            {
                // Calculate how many levels to generate for this texture
                MIPMAP_LEVELS = (int) Math.Floor(Math.Log(Math.Max(WIDTH, HEIGHT), 2));
            }
            else
            {
                // There is only one level
                MIPMAP_LEVELS = 1;
            }

            Util.CheckGlError("Clear");

            Util.CreateTexture(TextureTarget.Texture2D, NAME, out TEXTURE);
            GL.TextureStorage2D(TEXTURE, MIPMAP_LEVELS, INTERNAL_FORMAT, WIDTH, HEIGHT);
            Util.CheckGlError("Storage2d");

            BitmapData data = image.LockBits(new Rectangle(0, 0, WIDTH, HEIGHT),
                ImageLockMode.ReadOnly, global::System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TextureSubImage2D(TEXTURE, 0, 0, 0, WIDTH, HEIGHT, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            Util.CheckGlError("SubImage");

            image.UnlockBits(data);

            if (generateMipmaps) GL.GenerateTextureMipmap(TEXTURE);

            GL.TextureParameter(TEXTURE, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            Util.CheckGlError("WrapS");
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
            Util.CheckGlError("WrapT");

            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMinFilter, (int) (generateMipmaps ? TextureMinFilter.Linear : TextureMinFilter.LinearMipmapLinear));
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            Util.CheckGlError("Filtering");

            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMaxLevel, MIPMAP_LEVELS - 1);

            // This is a bit weird to do here
            image.Dispose();
        }

        public GLTexture(string name, int glTex, int width, int height, int mipmaplevels, SizedInternalFormat internalFormat)
        {
            NAME = name;
            TEXTURE = glTex;
            WIDTH = width;
            HEIGHT = height;
            MIPMAP_LEVELS = mipmaplevels;
            INTERNAL_FORMAT = internalFormat;
        }

        public GLTexture(string name, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
        {
            NAME = name;
            WIDTH = width;
            HEIGHT = height;
            INTERNAL_FORMAT = srgb ? SRGB8_ALPHA8 : SizedInternalFormat.Rgba8;
            MIPMAP_LEVELS = generateMipmaps == false ? 1 : (int) Math.Floor(Math.Log(Math.Max(WIDTH, HEIGHT), 2));

            Util.CreateTexture(TextureTarget.Texture2D, NAME, out TEXTURE);
            GL.TextureStorage2D(TEXTURE, MIPMAP_LEVELS, INTERNAL_FORMAT, WIDTH, HEIGHT);

            GL.TextureSubImage2D(TEXTURE, 0, 0, 0, WIDTH, HEIGHT, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            if (generateMipmaps) GL.GenerateTextureMipmap(TEXTURE);

            SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
            SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);

            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMaxLevel, MIPMAP_LEVELS - 1);
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMinFilter, (int) filter);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMagFilter, (int) filter);
        }

        public void SetAnisotropy(float level)
        {
            const TextureParameterName TEXTURE_MAX_ANISOTROPY = (TextureParameterName) 0x84FE;
            GL.TextureParameter(TEXTURE, TEXTURE_MAX_ANISOTROPY, Util.Clamp(level, 1, MAX_ANISO));
        }

        public void SetLod(int @base, int min, int max)
        {
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureLodBias, @base);
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMinLod, min);
            GL.TextureParameter(TEXTURE, TextureParameterName.TextureMaxLod, max);
        }

        public void SetWrap(TextureCoordinate coord, TextureWrapMode mode)
        {
            GL.TextureParameter(TEXTURE, (TextureParameterName) coord, (int) mode);
        }

        public void Dispose()
        {
            GL.DeleteTexture(TEXTURE);
        }
    }
}
