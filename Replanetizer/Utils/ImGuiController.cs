// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
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

        private readonly Dictionary<int, GLTexture> textures = new Dictionary<int, GLTexture>();
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

            ImGuiContextPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            unsafe
            {
                io.Fonts.AddFontDefault();
            }

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasTextures;

            CreateDeviceResources();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            // Prevent imgui.ini from being written. Just the mere existence of the file causes issues with docking persistence.
            unsafe
            {
                io.IniFilename = null;
            }

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
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

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

            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<ImDrawVert>(), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<ImDrawVert>(), 8);

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, Unsafe.SizeOf<ImDrawVert>(), 16);

            GLUtil.CheckGlError("End of ImGui setup");
        }

        private void UpdateTextures(ImDrawDataPtr drawData)
        {
            var textureVector = drawData.Textures;

            for (int i = 0; i < textureVector.Size; i++)
            {
                ImTextureDataPtr tex = textureVector[i];

                if (tex.Status == ImTextureStatus.WantCreate)
                {
                    CreateTexture(tex);
                }
                else if (tex.Status == ImTextureStatus.WantUpdates)
                {
                    UpdateTexture(tex);
                }
                else if (tex.Status == ImTextureStatus.WantDestroy)
                {
                    DestroyTexture(tex);
                }
            }
        }

        private unsafe void CreateTexture(ImTextureDataPtr tex)
        {
            if (textures.TryGetValue(tex.UniqueID, out GLTexture? existing))
            {
                existing.Dispose();
                textures.Remove(tex.UniqueID);
            }

            var glTexture = new GLTexture("ImGui Texture", tex.Width, tex.Height, (IntPtr) tex.GetPixels());
            glTexture.SetMagFilter(TextureMagFilter.Linear);
            glTexture.SetMinFilter(TextureMinFilter.Linear);

            textures[tex.UniqueID] = glTexture;

            tex.SetTexID((ulong) (uint) glTexture.textureID);
            tex.SetStatus(ImTextureStatus.Ok);
        }

        private unsafe void UpdateTexture(ImTextureDataPtr tex)
        {
            if (textures.TryGetValue(tex.UniqueID, out GLTexture? existing))
            {
                existing.Dispose();
                textures.Remove(tex.UniqueID);
            }

            CreateTexture(tex);
        }

        private void DestroyTexture(ImTextureDataPtr tex)
        {
            if (textures.TryGetValue(tex.UniqueID, out GLTexture? glTexture))
            {
                glTexture.Dispose();
                textures.Remove(tex.UniqueID);
            }

            tex.SetTexID((ulong) 0);
            tex.SetStatus(ImTextureStatus.Destroyed);
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

                GL.BindVertexArray(vertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<ImDrawVert>(), 0);

                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<ImDrawVert>(), 8);

                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, Unsafe.SizeOf<ImDrawVert>(), 16);

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
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.AddKeyEvent(ConvertKeyToImGuiKey(key), keyboardState.IsKeyDown(key));
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

            UpdateTextures(drawData);

            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[i];

                int vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > vertexBufferSize)
                {
                    int newSize = (int) Math.Max(vertexBufferSize * 1.5f, vertexSize);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    vertexBufferSize = newSize;

                    LOGGER.Info("Resized dear imgui vertex buffer to new size {0}", vertexBufferSize);
                }

                int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > indexBufferSize)
                {
                    int newSize = (int) Math.Max(indexBufferSize * 1.5f, indexSize);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
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

                unsafe
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), (IntPtr) cmdList.VtxBuffer.Data);
                    GLUtil.CheckGlError($"Data Vert {n}");

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
                    GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmdList.IdxBuffer.Size * sizeof(ushort), (IntPtr) cmdList.IdxBuffer.Data);
                    GLUtil.CheckGlError($"Data Idx {n}");
                }

                int vtxOffset = 0;
                int idxOffset = 0;

                for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
                {
                    ImDrawCmd cmd = cmdList.CmdBuffer[cmdI];
                    bool hasUserCallback;
                    unsafe
                    {
                        hasUserCallback = cmd.UserCallback != null;
                    }

                    if (hasUserCallback)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        ImTextureID texId = cmd.GetTexID();
                        int glTextureHandle = (int) (ulong) texId;

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, glTextureHandle);
                        GLUtil.CheckGlError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = cmd.ClipRect;
                        GL.Scissor((int) clip.X, windowHeight - (int) clip.W, (int) (clip.Z - clip.X), (int) (clip.W - clip.Y));
                        GLUtil.CheckGlError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int) cmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr) (idxOffset * sizeof(ushort)), vtxOffset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int) cmd.ElemCount, DrawElementsType.UnsignedShort, (int) cmd.IdxOffset * sizeof(ushort));
                        }
                        GLUtil.CheckGlError("Draw");
                    }

                    idxOffset += (int) cmd.ElemCount;
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
            foreach (var texture in textures.Values)
                texture.Dispose();
            textures.Clear();

            if (shader != null)
                shader.Dispose();
        }
    }
}
