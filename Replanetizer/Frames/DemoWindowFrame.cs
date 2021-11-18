// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using ImGuiNET;

namespace Replanetizer.Frames
{
    public class DemoWindowFrame : Frame
    {
        protected override string frameName { get; set; }

        public DemoWindowFrame(Window wnd) : base(wnd)
        {
            frameName = "DefaultFrameName";
        }

        public override void Render(float deltaTime)
        {
            ImGui.ShowDemoWindow(ref isOpen);
        }

        public override void RenderAsWindow(float deltaTime)
        {
            ImGui.ShowDemoWindow(ref isOpen);
        }
    }
}
