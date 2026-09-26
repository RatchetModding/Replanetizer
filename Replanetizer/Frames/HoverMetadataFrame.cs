// Copyright (C) 2018-2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;
using Replanetizer.Renderer;

namespace Replanetizer.Frames
{
    public class HoverMetadataFrame : LevelSubFrame
    {
        protected sealed override string frameName { get; set; } = "Hovered object";

        private const float CURSOR_OFFSET = 12.0f;

        private static readonly ImGuiWindowFlags WINDOW_FLAGS =
            ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoInputs;

        public HoverMetadataFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
        }

        public override void RenderAsWindow(float deltaTime)
        {
            int metadata;
            if (!levelFrame.TryGetHoverData(out metadata, out Vector2 mousePos))
                return;

            System.Numerics.Vector2 screenPosition = new System.Numerics.Vector2(mousePos.X, mousePos.Y);

            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            System.Numerics.Vector2 viewportCenter = viewport.WorkPos + viewport.WorkSize * 0.5f;
            bool placeLeft = screenPosition.X > viewportCenter.X;
            bool placeAbove = screenPosition.Y > viewportCenter.Y;
            System.Numerics.Vector2 windowPosition = screenPosition + new System.Numerics.Vector2(
                placeLeft ? -CURSOR_OFFSET : CURSOR_OFFSET,
                placeAbove ? -CURSOR_OFFSET : CURSOR_OFFSET);
            System.Numerics.Vector2 pivot = new System.Numerics.Vector2(placeLeft ? 1.0f : 0.0f, placeAbove ? 1.0f : 0.0f);

            ImGui.SetNextWindowPos(windowPosition, ImGuiCond.Always, pivot);
            ImGui.SetNextWindowBgAlpha(0.85f);

            if (ImGui.Begin(frameName, WINDOW_FLAGS))
                RenderMetadata((uint) metadata);
            ImGui.End();
        }

        public override void Render(float deltaTime)
        {
        }

        private static void RenderMetadata(uint metadata)
        {
            RenderedObjectType hitType = (RenderedObjectType) (metadata >> 24);
            uint hitId = metadata & 0xFFFFFF;

            ImGui.TextUnformatted(hitType.ToString());

            switch (hitType)
            {
                case RenderedObjectType.Collision:
                    {
                        CollisionGeometryCategory category = CollisionVertexMetadata.GetCategory(hitId);
                        CollisionType type = CollisionVertexMetadata.GetCollisionType(hitId);

                        ImGui.TextUnformatted("Category: " + category.ToString());

                        if (category == CollisionGeometryCategory.Standard || category == CollisionGeometryCategory.MobyTriangle)
                        {
                            ImGui.TextUnformatted("Type: " + type.GetMaterialName() + " [" + type.materialID + "]");
                            ImGui.TextUnformatted("Sound: " + type.groupID);
                        }

                        ImGui.TextUnformatted("Ignore Camera: " + type.ignoreCameraCollision.ToString());
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
