// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
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

        public static void RenderTextureList(List<Texture> textures, float itemSizeX, Dictionary<Texture, GLTexture> textureIds, LevelFrame levelFrame, string prefix = "", int additionalOffset = 0, bool useLocalIndex = false)
        {
            var width = ImGui.GetContentRegionAvail().X - additionalOffset;
            var itemsPerRow = (int) Math.Floor(width / itemSizeX);

            if (itemsPerRow == 0) return;

            int i = 0;
            while (i < textures.Count)
            {
                Texture t = textures[i];

                if (ImGui.BeginChild("imageChild_" + prefix + i, ITEM_SIZE, ImGuiChildFlags.None))
                {
                    unsafe
                    {
                        ImGui.Image(new ImTextureRef(default, (ulong) textureIds[t].textureID), IMAGE_SIZE);
                    }
                    string idText = useLocalIndex ? i.ToString() : t.id.ToString();
                    float idWidth = ImGui.CalcTextSize(idText).X;
                    ImGui.SetCursorPosX(ITEM_SIZE.X - idWidth);
                    ImGui.Text(idText);
                }
                ImGui.EndChild();

                if (ImGui.BeginPopupContextItem($"context-menu for {prefix}{i}"))
                {
                    if (ImGui.Button("Export"))
                    {
                        var targetFile = CrossFileDialog.SaveFile(filter: ".bmp;.jpg;.jpeg;.png");
                        if (targetFile.Length > 0)
                        {
                            TextureIO.ExportTexture(t, targetFile, true);
                        }
                    }
                    if (ImGui.Button("Replace"))
                    {
                        var sourceFile = CrossFileDialog.OpenFile(filter: ".dds");
                        if (sourceFile.Length > 0)
                        {
                            t.data = TextureIO.ImportDDSTexture(sourceFile, t.data.Length - 16);

                            if (levelFrame.textureIds.Remove(t, out var oldGl))
                                oldGl.Dispose();
                            levelFrame.textureIds[t] = new GLTexture(t);
                        }
                    }
                    ImGui.EndPopup();
                }
                else if (ImGui.IsItemHovered())
                {
                    if (ImGui.BeginTooltip())
                    {
                        unsafe
                        {
                            ImGui.Image(new ImTextureRef(default, (ulong) textureIds[t].textureID), new System.Numerics.Vector2(t.width, t.height));
                        }
                        string resolutionText = $"{t.width}x{t.height}";
                        float resolutionWidth = ImGui.CalcTextSize(resolutionText).X;
                        ImGui.SetCursorPosX(t.width - resolutionWidth);
                        ImGui.Text(resolutionText);
                    }
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
            levelFrame.textureFrameVisible = ImGui.Begin(frameName, ImGuiWindowFlags.AlwaysVerticalScrollbar);

            if (levelFrame.textureFrameVisible)
            {
                Render(deltaTime);
            }
            ImGui.End();
        }

        public override void Render(float deltaTime)
        {
            if (ImGui.CollapsingHeader("Level textures"))
            {
                RenderTextureList(level.textures, itemSizeX, levelFrame.textureIds, levelFrame, "levelTextures");

                // We only allow additions to the level textures tab
                if (ImGui.BeginPopupContextWindow("levelTexturesContextMenu", ImGuiPopupFlags.MouseButtonRight))
                {
                    if (ImGui.Button("Add Texture"))
                    {
                        var res = CrossFileDialog.OpenFile(filter: ".dds");
                        if (res.Length > 0)
                        {
                            var (height, width, len) = TextureIO.ReadDDSHeader(res);
                            byte[] data = TextureIO.ImportDDSTexture(res, len);

                            Texture newTexture = new Texture(level.textures[^1].id + 1, width, height, data);
                            level.textures.Add(newTexture);
                            levelFrame.textureIds[newTexture] = new GLTexture(newTexture);
                        }
                    }
                    ImGui.EndPopup();
                }
            }

            if (ImGui.CollapsingHeader("Menu textures"))
            {
                var menuTextures = level.textures.FindAll(tex => level.textureConfigMenus.Contains(tex.id));
                RenderTextureList(menuTextures, itemSizeX, levelFrame.textureIds, levelFrame, "menuTextures", 0, true);
            }
            if (ImGui.CollapsingHeader("Spaceship textures"))
            {
                RenderTextureList(level.spaceshipTextures, itemSizeX, levelFrame.textureIds, levelFrame, "spaceshipTextures");
            }
            if (ImGui.CollapsingHeader("Gadget textures"))
            {
                RenderTextureList(level.gadgetTextures, itemSizeX, levelFrame.textureIds, levelFrame, "gadgetTextures");
            }
            if (ImGui.CollapsingHeader("Armor textures"))
            {
                for (int i = 0; i < level.armorTextures.Count; i++)
                {
                    List<Texture> textureList = level.armorTextures[i];
                    if (ImGui.TreeNode("Armor " + i))
                    {
                        RenderTextureList(textureList, itemSizeX, levelFrame.textureIds, levelFrame);
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
                        RenderTextureList(mission.textures, itemSizeX, levelFrame.textureIds, levelFrame);
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
                            RenderTextureList(textureList, itemSizeX, levelFrame.textureIds, levelFrame);
                            ImGui.TreePop();
                        }
                    }
                }
            }
        }
    }
}
