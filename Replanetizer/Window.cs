// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Replanetizer.Frames;
using Replanetizer.Utils;

namespace Replanetizer
{
    public class Window : GameWindow
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public string openGLString = "Unknown OpenGL Version";

        private ImGuiController? controller;
        private List<Frame> openFrames;

        public string[] args;

        public Window(string[] args) : base(GameWindowSettings.Default,
            new NativeWindowSettings() { Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core })
        {
            this.args = args;
            openFrames = new List<Frame>();

            try
            {
                System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(System.AppContext.BaseDirectory + "Replanetizer.exe");
                if (icon != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        icon.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        byte[] iconBytes = ms.ToArray();
                        int rowSize = icon.Width * 4;
                        int iconSize = rowSize * icon.Height;
                        byte[] pixelBytes = new byte[iconSize];
                        for (int y = 0; y < icon.Height; y++)
                        {
                            for (int i = 0; i < 4 * icon.Width; i++)
                            {
                                pixelBytes[i + y * rowSize] = iconBytes[54 + i + (icon.Height - y - 1) * rowSize];
                            }
                        }
                        OpenTK.Windowing.Common.Input.Image img = new OpenTK.Windowing.Common.Input.Image(icon.Width, icon.Height, pixelBytes);
                        OpenTK.Windowing.Common.Input.Image[] imgs = new OpenTK.Windowing.Common.Input.Image[] { img };
                        this.Icon = new OpenTK.Windowing.Common.Input.WindowIcon(imgs);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            openGLString = "OpenGL " + GL.GetString(StringName.Version);
            Title = String.Format("Replanetizer ({0})", openGLString);

            controller = new ImGuiController(ClientSize.X, ClientSize.Y);

            UpdateInfoFrame.CheckForNewVersion(this);

            if (args.Length > 0)
            {
                LevelFrame lf = new LevelFrame(this, args[0]);
                AddFrame(lf);
            }
        }

        public void AddFrame(Frame frame)
        {
            openFrames.Add(frame);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            if (controller != null)
            {
                // Tell ImGui of the new size
                controller.WindowResized(ClientSize.X, ClientSize.Y);
            }
        }

        public static bool FrameMustClose(Frame frame)
        {
            return !frame.isOpen;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            openFrames.RemoveAll(FrameMustClose);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            if (controller != null)
                controller.Update(this, (float) e.Time);

            RenderUI((float) e.Time);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            if (controller != null)
                controller.Render();

            Util.CheckGlError("End of frame");
            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            if (controller != null)
                controller.PressChar((char) e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (controller != null)
                controller.MouseScroll(e.Offset);
        }

        private static bool FrameIsLevel(Frame frame)
        {
            return frame.GetType() == typeof(LevelFrame);
        }

        private void RenderMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open engine.ps3"))
                    {
                        var res = CrossFileDialog.OpenFile(filter: ".ps3");
                        if (res.Length > 0)
                        {
                            openFrames.RemoveAll(FrameIsLevel);
                            LevelFrame lf = new LevelFrame(this, res);
                            AddFrame(lf);
                        }
                    }

                    if (ImGui.MenuItem("Quit"))
                    {
                        Environment.Exit(0);
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("About"))
                {
                    if (ImGui.MenuItem("About Replanetizer"))
                    {
                        AddFrame(new AboutFrame(this));
                    }
                    if (ImGui.MenuItem("Open ImGui demo window"))
                    {
                        AddFrame(new DemoWindowFrame(this));
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        private void RenderUI(float deltaTime)
        {
            RenderMenuBar();

            foreach (Frame frame in openFrames)
            {
                frame.RenderAsWindow(deltaTime);
            }
        }
    }
}
