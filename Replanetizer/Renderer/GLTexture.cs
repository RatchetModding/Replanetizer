// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using SixLabors.ImageSharp;
using LibReplanetizer;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using SixLabors.ImageSharp.PixelFormats;

namespace Replanetizer.Renderer
{
    public enum TextureCoordinate
    {
        S = TextureParameterName.TextureWrapS,
        T = TextureParameterName.TextureWrapT,
        R = TextureParameterName.TextureWrapR
    }

    public class GLTexture : IDisposable
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private const SizedInternalFormat SRGB8_ALPHA8 = (SizedInternalFormat) All.Srgb8Alpha8;
        private const SizedInternalFormat RGB32F = (SizedInternalFormat) All.Rgb32f;

        private const GetPName MAX_TEXTURE_MAX_ANISOTROPY = (GetPName) 0x84FF;

        private static readonly float MAX_ANISO;

        static GLTexture()
        {
            MAX_ANISO = GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPY);
        }

        public readonly string name;
        public readonly int textureID;
        public readonly int width, height;
        public readonly int mipmapLevels;
        public readonly SizedInternalFormat internalFormat;

        // Default values are invalid and hence will also trigger a replacement
        private TextureWrapMode wrapS = TextureWrapMode.ClampToEdgeSgis;
        private TextureWrapMode wrapT = TextureWrapMode.ClampToEdgeSgis;

        public static void BindNull()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public GLTexture(string name, Image<Rgba32> image, bool generateMipmaps, bool srgb)
        {
            this.name = name;
            width = image.Width;
            height = image.Height;
            internalFormat = srgb ? SRGB8_ALPHA8 : SizedInternalFormat.Rgba8;

            if (generateMipmaps)
            {
                // Calculate how many levels to generate for this texture
                mipmapLevels = (int) Math.Floor(Math.Log(Math.Max(width, height), 2));
            }
            else
            {
                // There is only one level
                mipmapLevels = 1;
            }

            GLUtil.CheckGlError("Clear");

            GLUtil.CreateTexture(TextureTarget.Texture2D, this.name, out textureID);
            GL.TextureStorage2D(textureID, mipmapLevels, internalFormat, width, height);
            GLUtil.CheckGlError("Storage2d");

            byte[] imageBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(imageBytes);

            GL.TextureSubImage2D(textureID, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, imageBytes);
            GLUtil.CheckGlError("SubImage");

            if (generateMipmaps) GL.GenerateTextureMipmap(textureID);

            SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            GL.TextureParameter(textureID, TextureParameterName.TextureMinFilter, (int) (generateMipmaps ? TextureMinFilter.Linear : TextureMinFilter.LinearMipmapLinear));
            GL.TextureParameter(textureID, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GLUtil.CheckGlError("Filtering");

            GL.TextureParameter(textureID, TextureParameterName.TextureMaxLevel, mipmapLevels - 1);
        }

        public GLTexture(string name, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            internalFormat = srgb ? SRGB8_ALPHA8 : SizedInternalFormat.Rgba8;
            mipmapLevels = generateMipmaps == false ? 1 : (int) Math.Floor(Math.Log(Math.Max(this.width, this.height), 2));

            GLUtil.CreateTexture(TextureTarget.Texture2D, this.name, out textureID);
            GL.TextureStorage2D(textureID, mipmapLevels, internalFormat, this.width, this.height);

            GL.TextureSubImage2D(textureID, 0, 0, 0, this.width, this.height, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            if (generateMipmaps) GL.GenerateTextureMipmap(textureID);

            SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            GL.TextureParameter(textureID, TextureParameterName.TextureMaxLevel, mipmapLevels - 1);
        }

        public GLTexture(Texture t)
        {
            name = "Texture " + t.id;

            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);

            // The game uses a negative LOD Bias, the value is taken from RenderDoc on RPCS3.
            // The game may do this because of the low-res textures.
            // NOTE: This data was gathered using RPCS3's strict rendering mode so it seems unlikely that it was introduced through RPCS3.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -1.5f);

            internalFormat = SRGB8_ALPHA8;

            // Custom MP levels may have an incorrect number of mipmaps specified so we need to dynamically figure that out
            int mipLevel = 0;

            if (t.mipMapCount > 1)
            {
                int mipWidth = t.width;
                int mipHeight = t.height;
                int offset = 0;

                for (; mipLevel < t.mipMapCount; mipLevel++)
                {
                    if (mipWidth > 0 && mipHeight > 0)
                    {
                        int size = ((mipWidth + 3) / 4) * ((mipHeight + 3) / 4) * 16;
                        if (offset + size > t.data.Length)
                        {
                            LOGGER.Debug($"Texture {t.id} claims to have {t.mipMapCount} mipmaps but only has {mipLevel}!");
                            break;
                        }
                        byte[] texPart = new byte[size];
                        Array.Copy(t.data, offset, texPart, 0, size);
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, mipLevel, InternalFormat.CompressedRgbaS3tcDxt5Ext, mipWidth, mipHeight, 0, size, texPart);
                        offset += size;
                        mipWidth /= 2;
                        mipHeight /= 2;
                    }
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mipLevel - 1);
            }
            else
            {
                int size = ((t.width + 3) / 4) * ((t.height + 3) / 4) * 16;
                GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedRgbaS3tcDxt5Ext, t.width, t.height, 0, size, t.data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            GLUtil.CheckGlError(name);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.TextureParameter(textureID, TextureParameterName.TextureMinFilter, (int) filter);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.TextureParameter(textureID, TextureParameterName.TextureMagFilter, (int) filter);
        }

        public void SetAnisotropy(float level)
        {
            const TextureParameterName TEXTURE_MAX_ANISOTROPY = (TextureParameterName) 0x84FE;
            GL.TextureParameter(textureID, TEXTURE_MAX_ANISOTROPY, GLUtil.Clamp(level, 1, MAX_ANISO));
        }

        public void SetLod(int @base, int min, int max)
        {
            GL.TextureParameter(textureID, TextureParameterName.TextureLodBias, @base);
            GL.TextureParameter(textureID, TextureParameterName.TextureMinLod, min);
            GL.TextureParameter(textureID, TextureParameterName.TextureMaxLod, max);
        }

        public void SetWrapModes(TextureWrapMode modeS, TextureWrapMode modeT)
        {
            if (wrapS != modeS)
            {
                GL.TextureParameter(textureID, TextureParameterName.TextureWrapS, (int) modeS);
                wrapS = modeS;
            }

            if (wrapT != modeT)
            {
                GL.TextureParameter(textureID, TextureParameterName.TextureWrapT, (int) modeT);
                wrapT = modeT;
            }
        }

        public void Dispose()
        {
            GL.DeleteTexture(textureID);
        }
    }
}
