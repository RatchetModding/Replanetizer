// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Numerics;
using ImGuiNET;

namespace Replanetizer.Frames
{
    public class MemoryHookFrame : LevelSubFrame
    {
        private string informationText;
        private string warningText;
        private string lastReturnMessage = "";
        private bool attempted = false;
        private bool success = false;
        private static readonly Vector4 SUCCESS_COLOR = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 FAILURE_COLOR = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 WARNING_COLOR = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        protected override string frameName { get; set; } = "RPCS3 Memory Hook";

        public MemoryHookFrame(Window wnd, LevelFrame frame) : base(wnd, frame)
        {
            informationText = String.Format(
@"The memory hook reads from a running instance of Ratchet and Clank on RPCS3
and applies the data to the level in Replanetizer. Note that no data is ever sent
to the game.
"
            );

            warningText = String.Format(
@"Currently, only Ratchet and Clank 1 is supported. Once the memory hook is
engaged you will no longer be able to save the level in Replanetizer.
"
            );
        }

        public override void RenderAsWindow(float deltaTime)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(0, 0));
            if (ImGui.Begin(frameName, ref isOpen))
            {
                Render(deltaTime);
                ImGui.End();
            }
        }

        public override void Render(float deltaTime)
        {
            ImGui.Text(informationText);
            ImGui.TextColored(WARNING_COLOR, warningText);

            bool attemptSucceeded = false;

            if (success)
            {
                ImGui.BeginDisabled();
            }
            if (ImGui.Button("Activate Hook"))
            {
                attemptSucceeded = levelFrame.StartMemoryHook(ref lastReturnMessage);
                attempted = true;
            }
            if (success)
            {
                ImGui.EndDisabled();
            }

            if (attemptSucceeded)
            {
                success = attemptSucceeded;
            }

            if (attempted)
            {
                ImGui.SameLine();
                ImGui.TextColored((success) ? SUCCESS_COLOR : FAILURE_COLOR, lastReturnMessage);
            }
        }
    }
}
