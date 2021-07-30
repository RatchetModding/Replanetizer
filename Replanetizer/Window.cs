using System;
using System.Collections.Generic;
using ImGuiNET;
using LibReplanetizer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Replanetizer.Frames;
using Replanetizer.Utils;

namespace Replanetizer
{
    public class Window : GameWindow
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public string OpenGLString;
        
        private ImGuiController _controller;
        private List<Frame> openFrames;

        public string[] args;
        
        public Window(string[] args) : base(GameWindowSettings.Default,
            new NativeWindowSettings() { Size = new Vector2i(1600, 900), APIVersion = new Version(4, 5) })
        {
            this.args = args;
            openFrames = new List<Frame>();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            OpenGLString = "OpenGL " + GL.GetString(StringName.Version);
            Title = String.Format("Replanetizer ({0})", OpenGLString);

            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            
            if (args.Length > 0)
            {
                LevelFrame lf = new LevelFrame(this);
                Level l = new Level(args[0]);
                lf.LoadLevel(l);
                
                openFrames.Add(lf);
            }
        }
        
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
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
            _controller.Update(this, (float)e.Time);
            
            RenderUI((float) e.Time);
            
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            _controller.Render();
            Util.CheckGLError("End of frame");
            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            
            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            
            _controller.MouseScroll(e.Offset);
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
                            LevelFrame lf = new LevelFrame(this);
                            Level l = new Level(res);
                            lf.LoadLevel(l);
                            openFrames.Add(lf);
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
                        openFrames.Add(new AboutFrame(this));
                    }
                    if (ImGui.MenuItem("Open ImGui demo window"))
                    {
                        openFrames.Add(new DemoWindowFrame(this));
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
                frame.Render(deltaTime);
            }
        }
    }
}
