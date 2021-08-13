using System;
using OpenTK.Graphics.OpenGL4;

namespace Replanetizer.Utils
{
    public static class FramebufferRenderer
    {
        public static void ToTexture(int width, int height, ref int targetTexture, Action renderFunction)
        {
            int bufferTexture, framebufferId;
            
            GL.DeleteTexture(targetTexture);
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

            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, targetTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, bufferTexture, 0);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);

            renderFunction();

            GL.DeleteFramebuffer(framebufferId);
            GL.DeleteTexture(bufferTexture);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}