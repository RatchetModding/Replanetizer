// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using ImGuiNET;
using LibReplanetizer;
using Replanetizer.Utils;

namespace Replanetizer.Frames
{
    public class LevelExportFrame : LevelSubFrame
    {
        protected override string frameName { get; set; } = "Level Export";
        private Level level => levelFrame.level;

        private ModelWriter.WriterLevelSettings settings;
        private string[] enumNameMap;

        public LevelExportFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
            settings = new ModelWriter.WriterLevelSettings();
            enumNameMap = Enum.GetNames(typeof(ModelWriter.WriterLevelMode));

            for (int i = 0; i < level.terrainChunks.Count; i++)
            {
                settings.chunksSelected[i] = true;
            }
        }

        public override void Render(float deltaTime)
        {
            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode("Mesh mode"))
            {
                int meshMode = (int) settings.mode;
                if (ImGui.Combo("Mesh Mode", ref meshMode, enumNameMap, enumNameMap.Length))
                {
                    settings.mode = (ModelWriter.WriterLevelMode) meshMode;
                }
                ImGui.TreePop();
            }

            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode("Objects to include"))
            {
                ImGui.Checkbox("Include Ties", ref settings.writeTies);
                ImGui.Checkbox("Include Shrubs", ref settings.writeShrubs);
                ImGui.Checkbox("Include Mobies", ref settings.writeMobies);
                ImGui.Checkbox("Include MLT File", ref settings.exportMtlFile);
                ImGui.TreePop();
            }

            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode("Chunk config"))
            {
                ImGui.TextDisabled("Use CTRL key to select multiple");
                for (int i = 0; i < settings.chunksSelected.Length; i++)
                {
                    if (ImGui.Selectable("Chunk " + i, settings.chunksSelected[i]))
                    {
                        settings.chunksSelected[i] = !settings.chunksSelected[i];
                    }
                }
                ImGui.TreePop();
            }

            if (ImGui.Button("Perform export"))
            {
                var res = CrossFileDialog.SaveFile("Level.obj", "*.obj");
                if (res.Length > 0)
                {
                    ModelWriter.WriteObj(res, level, settings);
                    isOpen = false;
                }
            }

        }
    }
}
