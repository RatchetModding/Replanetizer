// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using ImGuiNET;

namespace Replanetizer.Frames
{
    public abstract class Frame
    {
        protected Window wnd;
        protected abstract string frameName { get; set; }
        public bool isOpen = true;
        private uint frameID;
        private static uint frameIDSource { get { return FRAME_ID_SOURCE++; } }
        private static uint FRAME_ID_SOURCE = 0;

        public Frame(Window wnd)
        {
            this.wnd = wnd;
            frameID = frameIDSource;
            SetWindowTitle(frameName);
        }

        protected void SetWindowTitle(string title)
        {
            /*
             * The ###frameID tells ImGui that the Windows ID is "frameID"
             * This is necessary as otherwise every title change would create a new window
             */
            frameName = title + " ###" + frameID;
        }

        public abstract void Render(float deltaTime);

        public virtual void RenderAsWindow(float deltaTime)
        {
            if (ImGui.Begin(frameName, ref isOpen))
            {
                Render(deltaTime);
                ImGui.End();
            }
        }
    }
}
