// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using OpenTK.Graphics.OpenGL;

namespace Replanetizer.Utils
{
    public class FramebufferRenderer : IDisposable
    {
        public static int MSAA_LEVEL = 2;
        private int internalAllocatedMsaaLevel;

        private bool disposed = false;

        private int targetTexture;
        private int typeTexture;
        public int outputTexture { get; set; }
        public int outputTypeTexture { get; set; }
        private int framebufferID;
        private int renderbufferID;
        private int outputFramebufferID;

        private int width, height;

        private void AllocateAllResources()
        {
            internalAllocatedMsaaLevel = MSAA_LEVEL;

            targetTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, targetTexture);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, MSAA_LEVEL, PixelInternalFormat.Rgb, width, height, true);

            renderbufferID = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferID);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, MSAA_LEVEL, RenderbufferStorage.DepthComponent, width, height);

            typeTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, typeTexture);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, MSAA_LEVEL, PixelInternalFormat.R32i, width, height, true);

            framebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, targetTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2DMultisample, typeTexture, 0);
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
        }

        public FramebufferRenderer(int width, int height)
        {
            this.width = width;
            this.height = height;

            AllocateAllResources();
        }

        public void RenderToTexture(Action renderFunction)
        {
            if (internalAllocatedMsaaLevel != MSAA_LEVEL)
            {
                DeleteAllResources();
                AllocateAllResources();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.Viewport(0, 0, width, height);

            DrawBuffersEnum[] buffers = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
            GL.DrawBuffers(2, buffers);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.GenVertexArrays(1, out int vao);
            GL.BindVertexArray(vao);

            renderFunction();

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebufferID);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, outputFramebufferID);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.DeleteVertexArray(vao);
        }

        public void ExposeFramebuffer(Action func)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputFramebufferID);

            func();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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
