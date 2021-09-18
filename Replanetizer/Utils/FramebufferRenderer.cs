using System;
using OpenTK.Graphics.OpenGL4;

namespace Replanetizer.Utils
{
    public class FramebufferRenderer : IDisposable
    {
        private bool disposed = false;

        public int targetTexture { get; }
        private int bufferTexture;
        private int framebufferID;

        public FramebufferRenderer(int width, int height)
        {
            targetTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, targetTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr) 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMagFilter.Nearest);

            bufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, bufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr) 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMagFilter.Nearest);

            framebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, targetTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, bufferTexture, 0);
        }

        public void RenderToTexture(Action renderFunction)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);

            renderFunction();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteVertexArray(VAO);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                GL.DeleteFramebuffer(framebufferID);
                GL.DeleteTexture(bufferTexture);
                GL.DeleteTexture(targetTexture);
            }

            disposed = true;
        }
    }
}