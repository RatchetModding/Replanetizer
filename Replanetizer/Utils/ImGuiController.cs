// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Replanetizer.Renderer;

namespace Replanetizer.Utils
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private bool frameBegun;

        private int vertexArray;
        private int vertexBuffer;
        private int vertexBufferSize;
        private int indexBuffer;
        private int indexBufferSize;

        private GLTexture? fontGlTexture;
        private Shader? shader;

        private int windowWidth;
        private int windowHeight;

        private System.Numerics.Vector2 scaleFactor = System.Numerics.Vector2.One;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            windowWidth = width;
            windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            ImGui.NewFrame();
            frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            windowWidth = width;
            windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            GLUtil.CreateVertexArray("ImGui", out vertexArray);

            vertexBufferSize = 10000;
            indexBufferSize = 2000;

            GLUtil.CreateVertexBuffer("ImGui", out vertexBuffer);
            GLUtil.CreateElementBuffer("ImGui", out indexBuffer);
            GL.NamedBufferData(vertexBuffer, vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(indexBuffer, indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string vertexSource = @"#version 330 core

uniform mat4 worldToView;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = worldToView * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string fragmentSource = @"#version 330 core

uniform sampler2D fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(fontTexture, texCoord);
}";
            shader = new Shader("ImGui", vertexSource, fragmentSource);

            GL.VertexArrayVertexBuffer(vertexArray, 0, vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
            GL.VertexArrayElementBuffer(vertexArray, indexBuffer);

            GL.EnableVertexArrayAttrib(vertexArray, 0);
            GL.VertexArrayAttribBinding(vertexArray, 0, 0);
            GL.VertexArrayAttribFormat(vertexArray, 0, 2, VertexAttribType.Float, false, 0);

            GL.EnableVertexArrayAttrib(vertexArray, 1);
            GL.VertexArrayAttribBinding(vertexArray, 1, 0);
            GL.VertexArrayAttribFormat(vertexArray, 1, 2, VertexAttribType.Float, false, 8);

            GL.EnableVertexArrayAttrib(vertexArray, 2);
            GL.VertexArrayAttribBinding(vertexArray, 2, 0);
            GL.VertexArrayAttribFormat(vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);

            GLUtil.CheckGlError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            fontGlTexture = new GLTexture("ImGui Text Atlas", width, height, pixels);
            fontGlTexture.SetMagFilter(TextureMagFilter.Linear);
            fontGlTexture.SetMinFilter(TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr) fontGlTexture.textureID);
            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (frameBegun)
            {
                frameBegun = false;

                GL.VertexArrayVertexBuffer(vertexArray, 0, vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
                GL.VertexArrayElementBuffer(vertexArray, indexBuffer);

                GL.EnableVertexArrayAttrib(vertexArray, 0);
                GL.VertexArrayAttribBinding(vertexArray, 0, 0);
                GL.VertexArrayAttribFormat(vertexArray, 0, 2, VertexAttribType.Float, false, 0);

                GL.EnableVertexArrayAttrib(vertexArray, 1);
                GL.VertexArrayAttribBinding(vertexArray, 1, 0);
                GL.VertexArrayAttribFormat(vertexArray, 1, 2, VertexAttribType.Float, false, 8);

                GL.EnableVertexArrayAttrib(vertexArray, 2);
                GL.VertexArrayAttribBinding(vertexArray, 2, 0);
                GL.VertexArrayAttribFormat(vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);

                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                windowWidth / scaleFactor.X,
                windowHeight / scaleFactor.Y);
            io.DisplayFramebufferScale = scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PRESSED_CHARS = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState mouseState = wnd.MouseState;
            KeyboardState keyboardState = wnd.KeyboardState;

            io.MouseDown[0] = mouseState[MouseButton.Left];
            io.MouseDown[1] = mouseState[MouseButton.Right];
            io.MouseDown[2] = mouseState[MouseButton.Middle];

            var screenPoint = new Vector2i((int) mouseState.X, (int) mouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown || !keyboardState.IsKeyDown(key))
                {
                    continue;
                }
                io.AddKeyEvent(ConvertKeyToImGuiKey(key), true);
            }

            foreach (var c in PRESSED_CHARS)
            {
                io.AddInputCharacter(c);
            }
            PRESSED_CHARS.Clear();

            io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PRESSED_CHARS.Add(keyChar);
        }

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static ImGuiKey ConvertKeyToImGuiKey(Keys key)
        {
            switch (key)
            {
                case Keys.Tab:
                    return ImGuiKey.Tab;
                case Keys.Left:
                    return ImGuiKey.LeftArrow;
                case Keys.Right:
                    return ImGuiKey.RightArrow;
                case Keys.Up:
                    return ImGuiKey.UpArrow;
                case Keys.Down:
                    return ImGuiKey.DownArrow;
                case Keys.PageUp:
                    return ImGuiKey.PageUp;
                case Keys.PageDown:
                    return ImGuiKey.PageDown;
                case Keys.Home:
                    return ImGuiKey.Home;
                case Keys.End:
                    return ImGuiKey.End;
                case Keys.Delete:
                    return ImGuiKey.Delete;
                case Keys.Backspace:
                    return ImGuiKey.Backspace;
                case Keys.Enter:
                    return ImGuiKey.Enter;
                case Keys.Escape:
                    return ImGuiKey.Escape;
                case Keys.Space:
                    return ImGuiKey.Space;
                case Keys.LeftControl:
                    return ImGuiKey.LeftCtrl;
                case Keys.C:
                    return ImGuiKey.C;
                case Keys.V:
                    return ImGuiKey.V;
                case Keys.X:
                    return ImGuiKey.X;
                case Keys.Y:
                    return ImGuiKey.Y;
                case Keys.Z:
                    return ImGuiKey.Z;
                case Keys.W:
                    return ImGuiKey.W;
                case Keys.A:
                    return ImGuiKey.A;
                case Keys.S:
                    return ImGuiKey.S;
                case Keys.D:
                    return ImGuiKey.D;
            }

            return ImGuiKey.None;
        }

        private void RenderImDrawData(ImDrawDataPtr drawData)
        {
            if (shader == null) return;

            if (drawData.CmdListsCount == 0)
            {
                return;
            }

            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[i];

                int vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > vertexBufferSize)
                {
                    int newSize = (int) Math.Max(vertexBufferSize * 1.5f, vertexSize);
                    GL.NamedBufferData(vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    vertexBufferSize = newSize;

                    LOGGER.Info("Resized dear imgui vertex buffer to new size {0}", vertexBufferSize);
                }

                int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > indexBufferSize)
                {
                    int newSize = (int) Math.Max(indexBufferSize * 1.5f, indexSize);
                    GL.NamedBufferData(indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    indexBufferSize = newSize;

                    LOGGER.Info("Resized dear imgui index buffer to new size {0}", indexBufferSize);
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            shader.UseShader();
            shader.SetUniformMatrix4(UniformName.worldToView, ref mvp);
            shader.SetUniform1(UniformName.fontTexture, 0);
            GLUtil.CheckGlError("Projection");

            GL.BindVertexArray(vertexArray);
            GLUtil.CheckGlError("VAO");

            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                GL.NamedBufferSubData(vertexBuffer, IntPtr.Zero, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data);
                GLUtil.CheckGlError($"Data Vert {n}");

                GL.NamedBufferSubData(indexBuffer, IntPtr.Zero, cmdList.IdxBuffer.Size * sizeof(ushort), cmdList.IdxBuffer.Data);
                GLUtil.CheckGlError($"Data Idx {n}");

                int vtxOffset = 0;
                int idxOffset = 0;

                for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdI];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int) pcmd.TextureId);
                        GLUtil.CheckGlError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int) clip.X, windowHeight - (int) clip.W, (int) (clip.Z - clip.X), (int) (clip.W - clip.Y));
                        GLUtil.CheckGlError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int) pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr) (idxOffset * sizeof(ushort)), vtxOffset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int) pcmd.ElemCount, DrawElementsType.UnsignedShort, (int) pcmd.IdxOffset * sizeof(ushort));
                        }
                        GLUtil.CheckGlError("Draw");
                    }

                    idxOffset += (int) pcmd.ElemCount;
                }
                vtxOffset += cmdList.VtxBuffer.Size;
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            if (fontGlTexture != null)
                fontGlTexture.Dispose();

            if (shader != null)
                shader.Dispose();
        }
    }
}
