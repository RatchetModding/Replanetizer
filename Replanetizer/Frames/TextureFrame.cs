using System;
using System.Collections.Generic;
using ImGuiNET;
using LibReplanetizer;


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

        private void RenderTextureList(List<Texture> textures, int additionalOffset = 0)
        {
            var width = ImGui.GetWindowContentRegionWidth() - additionalOffset;
            var itemsPerRow = (int) Math.Floor(width / itemSizeX);

            int i = 0;
            while (i < textures.Count)
            {
                Texture t = textures[i];
                ImGui.Image((IntPtr)levelFrame.textureIds[t], listSize);
                
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Image((IntPtr)levelFrame.textureIds[t], new System.Numerics.Vector2(t.width, t.height));
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

        public override void Render(float deltaTime)
        {
            if (ImGui.Begin(frameName, ref isOpen, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                if (ImGui.CollapsingHeader("Level textures"))
                {
                    RenderTextureList(level.textures);
                }
                if (ImGui.CollapsingHeader("Gadget textures"))
                {
                    RenderTextureList(level.gadgetTextures);
                }
                if (ImGui.CollapsingHeader("Armor textures"))
                {
                    for (int i = 0; i < level.armorTextures.Count; i++)
                    {
                        var textureList = level.armorTextures[i];
                        if (ImGui.TreeNode("Armor " + i))
                        {
                            var offset = (int) ImGui.GetTreeNodeToLabelSpacing();
                            RenderTextureList(textureList);
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
                            RenderTextureList(mission.textures);
                            ImGui.TreePop();
                        }
                    }
                }
            }
        }
    }
}
