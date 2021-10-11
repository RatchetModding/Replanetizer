// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using ImGuiNET;
using LibReplanetizer;
using Replanetizer.Utils;


namespace Replanetizer.Frames
{
    public class TextureFrame : LevelSubFrame
    {
        protected sealed override string frameName { get; set; } = "Textures";
        private Level level => levelFrame.level;
        private static System.Numerics.Vector2 listSize = new(64, 64);
        private float itemSizeX;

        public TextureFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
            itemSizeX = listSize.X + ImGui.GetStyle().ItemSpacing.X;
        }

        public static void RenderTextureList(List<Texture> textures, float itemSizeX, Dictionary<Texture, int> textureIds, int additionalOffset = 0)
        {
            var width = ImGui.GetWindowContentRegionWidth() - additionalOffset;
            var itemsPerRow = (int) Math.Floor(width / itemSizeX);

            int i = 0;
            while (i < textures.Count)
            {
                Texture t = textures[i];
                ImGui.Image((IntPtr) textureIds[t], listSize);
                if (ImGui.BeginPopupContextItem($"context-menu for {i}"))
                {
                    if (ImGui.Button("Export"))
                    {
                        var targetFile = CrossFileDialog.SaveFile(filter: ".bmp;.jpg;.jpeg;.png");
                        if (targetFile.Length > 0)
                        {
                            TextureIO.ExportTexture(t, targetFile);
                        }
                    }
                    ImGui.EndPopup();
                }
                else if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"{t.width}x{t.height}");
                    ImGui.Image((IntPtr) textureIds[t], new System.Numerics.Vector2(t.width, t.height));
                    ImGui.EndTooltip();
                }

                i++;

                if ((i % itemsPerRow) != 0)
                {
                    ImGui.SameLine();
                }
            }

            ImGui.NewLine();
        }

        public override void RenderAsWindow(float deltaTime)
        {
            if (ImGui.Begin(frameName, ref isOpen, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                Render(deltaTime);
                ImGui.End();
            }
        }

        public override void Render(float deltaTime)
        {
            if (ImGui.CollapsingHeader("Level textures"))
            {
                RenderTextureList(level.textures, itemSizeX, levelFrame.textureIds);
            }
            if (ImGui.CollapsingHeader("Gadget textures"))
            {
                RenderTextureList(level.gadgetTextures, itemSizeX, levelFrame.textureIds);
            }
            if (ImGui.CollapsingHeader("Armor textures"))
            {
                for (int i = 0; i < level.armorTextures.Count; i++)
                {
                    var textureList = level.armorTextures[i];
                    if (ImGui.TreeNode("Armor " + i))
                    {
                        var offset = (int) ImGui.GetTreeNodeToLabelSpacing();
                        RenderTextureList(textureList, itemSizeX, levelFrame.textureIds);
                        ImGui.TreePop();
                    }
                }
            }
            if (ImGui.CollapsingHeader("Mission textures"))
            {
                foreach (var mission in level.missions)
                {
                    if (ImGui.TreeNode("Mission " + mission.missionID))
                    {
                        var offset = (int) ImGui.GetTreeNodeToLabelSpacing();
                        RenderTextureList(mission.textures, itemSizeX, levelFrame.textureIds);
                        ImGui.TreePop();
                    }
                }
            }
        }
    }
}
