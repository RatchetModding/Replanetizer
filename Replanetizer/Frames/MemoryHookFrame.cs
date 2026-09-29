// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Numerics;
using Hexa.NET.ImGui;
using Replanetizer.MemoryHook;

namespace Replanetizer.Frames
{
    public class MemoryHookFrame : LevelSubFrame
    {
        private string informationText;
        private string warningText;
        private bool useBreakpoints = false;
        private MemoryHookHandle? hookHandle = null;
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
@"Currently, Deadlocked is not supported. Only the EU version of the trilogy is supported.
Once the memory hook is engaged you will no longer be able to save the level in Replanetizer.
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

            bool disableControls = hookHandle != null;

            if (disableControls)
            {
                ImGui.BeginDisabled();
            }

#if _WINDOWS
            ImGui.Checkbox("Use breakpoints (Experimental)", ref useBreakpoints);
            ImGui.SameLine();

            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                if (ImGui.BeginTooltip())
                {
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 40.0f);
                    ImGui.TextUnformatted("Improves hook timing which results in a more stable and accurate synchronization between Replanetizer and the game. Might result in unforeseen issues as Replanetizer takes control over RPCS3.");
                    ImGui.PopTextWrapPos();
                }
                ImGui.EndTooltip();
            }
#endif
            if (ImGui.Button("Activate Hook"))
            {
                hookHandle = levelFrame.StartMemoryHook(useBreakpoints);
            }

            if (disableControls)
            {
                ImGui.EndDisabled();
            }

            if (hookHandle != null)
            {
                ImGui.Text("Hook Status:");
                ImGui.SameLine();
                ImGui.TextColored(hookHandle.hookWorking ? SUCCESS_COLOR : FAILURE_COLOR, hookHandle.GetLastErrorMessage());
            }
        }
    }
}
