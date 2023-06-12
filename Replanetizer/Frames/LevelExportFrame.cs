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
        private Level? level => levelFrame.level;

        private ExporterLevelSettings settings;
        private ExporterModelSettings modelSettings;

        public LevelExportFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
            settings = new ExporterLevelSettings();
            modelSettings = new ExporterModelSettings();

            if (level != null)
            {
                if (level.terrainChunks.Count == 0)
                {
                    settings.chunksSelected[0] = true;
                }
                else
                {
                    for (int i = 0; i < level.terrainChunks.Count; i++)
                    {
                        settings.chunksSelected[i] = true;
                    }
                }

            }
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
            if (level != null)
            {
                ImGui.SetNextItemOpen(true);
                if (ImGui.TreeNode("Mesh mode"))
                {
                    int meshMode = (int) settings.mode;
                    if (ImGui.Combo("Mesh Mode", ref meshMode, ExporterLevelSettings.MODE_STRINGS, ExporterLevelSettings.MODE_STRINGS.Length))
                    {
                        settings.mode = (ExporterLevelSettings.Mode) meshMode;
                    }
                    int orientation = (int) modelSettings.orientation;
                    if (ImGui.Combo("Orientation", ref orientation, ExporterModelSettings.ORIENTATION_STRINGS, ExporterModelSettings.ORIENTATION_STRINGS.Length))
                    {
                        modelSettings.orientation = (ExporterModelSettings.Orientation) orientation;
                    }
                    ImGui.TreePop();
                }

                ImGui.SetNextItemOpen(true);
                if (ImGui.TreeNode("Objects to include"))
                {
                    ImGui.Checkbox("Include Ties", ref settings.writeTies);
                    ImGui.Checkbox("Include Shrubs", ref settings.writeShrubs);
                    ImGui.Checkbox("Include Mobies", ref settings.writeMobies);
                    if (level.terrainChunks.Count == 0)
                        ImGui.Checkbox("Include Terrain", ref settings.chunksSelected[0]);
                    ImGui.Checkbox("Include Colors", ref settings.writeColors);
                    ImGui.Checkbox("Include MTL File", ref settings.exportMtlFile);
                    ImGui.TreePop();
                }

                if (level.terrainChunks.Count != 0)
                {
                    ImGui.SetNextItemOpen(true);
                    if (ImGui.TreeNode("Chunk config"))
                    {
                        ImGui.TextDisabled("Use CTRL key to select multiple");
                        for (int i = 0; i < level.terrainChunks.Count; i++)
                        {
                            if (ImGui.Selectable("Chunk " + i, settings.chunksSelected[i]))
                            {
                                settings.chunksSelected[i] = !settings.chunksSelected[i];
                            }
                        }
                        ImGui.TreePop();
                    }
                }

                if (ImGui.Button("Perform export"))
                {
                    var res = CrossFileDialog.SaveFile("Level.obj", ".obj");
                    if (res.Length > 0)
                    {
                        WavefrontExporter exporter = new WavefrontExporter(modelSettings, settings);
                        exporter.ExportLevel(res, level);
                        isOpen = false;
                    }
                }
            }
            else
            {
                ImGui.Text("Export unavailable, no level found!");
            }
        }
    }
}
