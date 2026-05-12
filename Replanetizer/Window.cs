// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using Replanetizer.Frames;
using Replanetizer.Utils;
using Replanetizer.Renderer;
using SixLabors.ImageSharp.PixelFormats;

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
            new NativeWindowSettings() { ClientSize = new Vector2i(1600, 900), APIVersion = new Version(3, 3), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core, Vsync = VSyncMode.On })
        {
            this.args = args;
            openFrames = new List<Frame>();

            string? applicationFolder = System.AppContext.BaseDirectory;
            string iconsFolder = Path.Join(applicationFolder, "Icons");

            using Image<Rgba32> image = Image.Load<Rgba32>(Path.Join(iconsFolder, "Replanetizer.png"));
            byte[] imageBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(imageBytes);

            OpenTK.Windowing.Common.Input.Image img = new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes);
            OpenTK.Windowing.Common.Input.Image[] imgs = new OpenTK.Windowing.Common.Input.Image[] { img };
            this.Icon = new OpenTK.Windowing.Common.Input.WindowIcon(imgs);
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

            foreach (var frame in openFrames.Where(FrameMustClose))
            {
                frame.Dispose();
            }
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

            GLUtil.CheckGlError("End of frame");
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
                            foreach (var frame in openFrames.Where(FrameIsLevel))
                            {
                                frame.Dispose();
                            }
                            openFrames.RemoveAll(FrameIsLevel);
                            LevelFrame lf = new LevelFrame(this, res);
                            AddFrame(lf);
                        }
                    }
                    if (ImGui.MenuItem("Open ps3data folder"))
                    {
                        var res = CrossFileDialog.OpenFolder();
                        if (res.Length > 0) TryOpenFolder(res);
                    }

                    if (ImGui.MenuItem("Quit"))
                    {
                        Environment.Exit(0);
                    }
                    ImGui.EndMenu();
                }

                // Put levels dropdown if it can find the game id in the path.
                // This is probably stupid.
                // Could probably be better to open the first engine.ps3 files it finds, and then from there maybe work out
                // what game it is, since there is a way to detect gametype in the engine parser?
                // Ahhhh.... I don't know.
                if (activeGameId != null && rootFolder != null)
                {
                    var names = LevelLists.GetLevelNames(activeGameId, levelListsFolder);
                    if (names != null && ImGui.BeginMenu("Levels"))
                    {
                        foreach (var (id, name) in names.OrderBy(x => x.Key))
                        {
                            string levelPath = Path.Join(rootFolder, $"level{id}", "engine.ps3");
                            bool exists = File.Exists(levelPath);

                            if (!exists)
                                ImGui.BeginDisabled();

                            if (ImGui.MenuItem(name))
                            {
                                foreach (var frame in openFrames.Where(FrameIsLevel))
                                    frame.Dispose();

                                openFrames.RemoveAll(FrameIsLevel);
                                openFrames.Add(new LevelFrame(this, levelPath));
                            }

                            if (!exists)
                                ImGui.EndDisabled();
                        }
                        ImGui.EndMenu();
                    }
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

        private string? rootFolder = null;
        private string? activeGameId = null;
        private string levelListsFolder = Path.Join(AppContext.BaseDirectory, "LevelLists");
        private void TryOpenFolder(string folder)
        {
            string? gameId = LevelLists.DetectGameFile(folder);
            if (gameId == null)
                return;

            rootFolder = folder;
            activeGameId = gameId;
        }

        private void RenderUI(float deltaTime)
        {
            RenderMenuBar();

            foreach (Frame frame in openFrames)
            {
                frame.RenderAsWindow(deltaTime);
            }
        }

        protected override void OnUnload()
        {
            foreach (var frame in openFrames)
            {
                frame.Dispose();
            }
            openFrames.Clear();

            BillboardRenderer.CleanupStaticResources();
            GLTexture.CleanupStaticResources();
            base.OnUnload();
        }
    }
}
