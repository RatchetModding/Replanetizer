// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using Hexa.NET.ImGui;


namespace Replanetizer.Frames
{
    public abstract class Frame : IDisposable
    {
        protected Window wnd;
        protected abstract string frameName { get; set; }
        public bool isOpen = true;
        private string frameID;
        private bool frameIDReleased = false;
        private static readonly HashSet<string> CLAIMED_IDS = new HashSet<string>();

        public Frame(Window wnd)
        {
            this.wnd = wnd;
            frameID = ClaimFrameID(GetType());
            SetWindowTitle(frameName);
        }

        private static string ClaimFrameID(Type type)
        {
            for (uint i = 0; ; i++)
            {
                string candidate = type.Name + "#" + i;
                if (CLAIMED_IDS.Add(candidate))
                    return candidate;
            }
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

        public virtual void Dispose()
        {
            if (frameIDReleased)
                return;

            frameIDReleased = true;
            CLAIMED_IDS.Remove(frameID);
        }
    }
}
