// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LibReplanetizer;
using Replanetizer.Renderer;
using Replanetizer.Utils;


namespace Replanetizer.Frames
{
    public class TextureFrame : LevelSubFrame
    {
        protected sealed override string frameName { get; set; } = "Textures";
        private Level level => levelFrame.level;
        private static Vector2 IMAGE_SIZE = new(64, 64);
        private static Vector2 ITEM_SIZE = new(64, 84);
        private float itemSizeX;

        public TextureFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
            itemSizeX = IMAGE_SIZE.X + ImGui.GetStyle().ItemSpacing.X;
        }

        public static void RenderTextureList(List<Texture> textures, float itemSizeX, Dictionary<Texture, GLTexture> textureIds, string prefix = "", int additionalOffset = 0)
        {
            var width = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - additionalOffset;
            var itemsPerRow = (int) Math.Floor(width / itemSizeX);

            if (itemsPerRow == 0) return;

            int i = 0;
            while (i < textures.Count)
            {
                Texture t = textures[i];

                ImGui.BeginChild("imageChild_" + prefix + i, ITEM_SIZE, ImGuiChildFlags.None);
                ImGui.Image((IntPtr) textureIds[t].textureID, IMAGE_SIZE);
                string idText = prefix + t.id;
                float idWidth = ImGui.CalcTextSize(idText).X;
                ImGui.SetCursorPosX(ITEM_SIZE.X - idWidth);
                ImGui.Text(idText);
                ImGui.EndChild();

                if (ImGui.BeginPopupContextItem($"context-menu for {i}"))
                {
                    if (ImGui.Button("Export"))
                    {
                        var targetFile = CrossFileDialog.SaveFile(filter: ".bmp;.jpg;.jpeg;.png");
                        if (targetFile.Length > 0)
                        {
                            TextureIO.ExportTexture(t, targetFile, true);
                        }
                    }
                    ImGui.EndPopup();
                }
                else if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Image((IntPtr) textureIds[t].textureID, new System.Numerics.Vector2(t.width, t.height));
                    string resolutionText = $"{t.width}x{t.height}";
                    float resolutionWidth = ImGui.CalcTextSize(resolutionText).X;
                    ImGui.SetCursorPosX(t.width - resolutionWidth);
                    ImGui.Text(resolutionText);
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
                    List<Texture> textureList = level.armorTextures[i];
                    if (ImGui.TreeNode("Armor " + i))
                    {
                        RenderTextureList(textureList, itemSizeX, levelFrame.textureIds);
                        ImGui.TreePop();
                    }
                }
            }
            if (ImGui.CollapsingHeader("Mission textures"))
            {
                foreach (Mission mission in level.missions)
                {
                    if (ImGui.TreeNode("Mission " + mission.missionID))
                    {
                        RenderTextureList(mission.textures, itemSizeX, levelFrame.textureIds);
                        ImGui.TreePop();
                    }
                }
            }
            if (ImGui.CollapsingHeader("Mobyload textures"))
            {
                for (int i = 0; i < level.mobyloadTextures.Count; i++)
                {
                    List<Texture> textureList = level.mobyloadTextures[i];

                    if (textureList.Count > 0)
                    {
                        if (ImGui.TreeNode("Mobyload " + i))
                        {
                            RenderTextureList(textureList, itemSizeX, levelFrame.textureIds);
                            ImGui.TreePop();
                        }
                    }
                }
            }
        }
    }
}
