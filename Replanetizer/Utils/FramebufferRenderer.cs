// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using OpenTK.Graphics.OpenGL;
using Replanetizer.Renderer;

namespace Replanetizer.Utils
{
    public class FramebufferRenderer : IDisposable
    {
        public static int SSAA_LEVEL_LOG = 1;
        public static int SSAA_LEVEL { get { return 1 << SSAA_LEVEL_LOG; } }
        private int internalAllocatedSsaaLevel;

        private bool disposed = false;

        private int targetTexture;
        private int typeTexture;
        public int outputTexture { get; set; }
        public int outputTypeTexture { get; set; }
        private int framebufferID;
        private int renderbufferID;
        private int outputFramebufferID;

        private int width, height;
        private readonly Shader resolveShader;
        public int RenderWidth { get; private set; }
        public int RenderHeight { get; private set; }

        private void AllocateAllResources()
        {
            internalAllocatedSsaaLevel = SSAA_LEVEL;
            RenderWidth = checked(width * SSAA_LEVEL);
            RenderHeight = checked(height * SSAA_LEVEL);

            int maxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
            if (RenderWidth > maxTextureSize || RenderHeight > maxTextureSize)
            {
                if (SSAA_LEVEL_LOG > 0)
                {
                    SSAA_LEVEL_LOG--;

                    AllocateAllResources();
                    return;
                }

                // If we couldn't reduce the SSAA level, just try it anyway and see what happens, there is nothing we can do at this point.
            }

            targetTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, targetTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, RenderWidth, RenderHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr) 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);

            renderbufferID = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferID);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, RenderWidth, RenderHeight);

            typeTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, typeTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32i, RenderWidth, RenderHeight, 0, PixelFormat.RedInteger, PixelType.Int, (IntPtr) 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);

            framebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, targetTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, typeTexture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbufferID);

            outputTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, outputTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr) 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);

            outputTypeTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, outputTypeTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32i, width, height, 0, PixelFormat.RedInteger, PixelType.Int, (IntPtr) 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);

            outputFramebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputFramebufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, outputTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, outputTypeTexture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public FramebufferRenderer(int width, int height, Shader resolveShader)
        {
            this.width = width;
            this.height = height;
            this.resolveShader = resolveShader;

            AllocateAllResources();
        }

        public void RenderToTexture(Action renderFunction)
        {
            if (internalAllocatedSsaaLevel != SSAA_LEVEL)
            {
                DeleteAllResources();
                AllocateAllResources();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.Viewport(0, 0, RenderWidth, RenderHeight);

            DrawBuffersEnum[] buffers = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
            GL.DrawBuffers(2, buffers);

            GL.ClearBuffer(ClearBuffer.Color, 1, new[] { 0 });

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.GenVertexArrays(1, out int vao);
            GL.BindVertexArray(vao);

            renderFunction();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputFramebufferID);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.Viewport(0, 0, width, height);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.Blend);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, targetTexture);
            resolveShader.UseShader();
            resolveShader.SetUniform1(UniformName.resolveTexture, 0);
            resolveShader.SetUniform1(UniformName.resolveSsaaLevel, SSAA_LEVEL);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebufferID);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, outputFramebufferID);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
            GL.BlitFramebuffer(0, 0, RenderWidth, RenderHeight, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.DeleteVertexArray(vao);
        }

        public void ExposeFramebuffer(Action func)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputFramebufferID);

            func();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public bool TryReadDepth(int x, int y, out float depth)
        {
            depth = 1.0f;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            int renderX = Math.Min(RenderWidth - 1, x * RenderWidth / width);
            int renderY = Math.Min(RenderHeight - 1, (height - 1 - y) * RenderHeight / height);

            int previousFramebuffer = GL.GetInteger(GetPName.FramebufferBinding);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.ReadPixels(renderX, renderY, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref depth);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebuffer);

            return depth >= 0.0f && depth < 1.0f;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void DeleteAllResources()
        {
            GL.DeleteFramebuffer(framebufferID);
            GL.DeleteFramebuffer(outputFramebufferID);
            GL.DeleteRenderbuffer(renderbufferID);
            GL.DeleteTexture(targetTexture);
            GL.DeleteTexture(typeTexture);
            GL.DeleteTexture(outputTexture);
            GL.DeleteTexture(outputTypeTexture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                DeleteAllResources();
            }

            disposed = true;
        }

        ~FramebufferRenderer()
        {
            Dispose(false);
        }
    }
}
