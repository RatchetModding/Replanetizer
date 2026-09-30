// Copyright (C) 2018-2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.



using Replanetizer.Renderer;
using Hexa.NET.ImGui;

namespace Replanetizer.Frames
{
    public class RenderFrame : LevelSubFrame
    {
        protected sealed override string frameName { get; set; } = "Render";

        private readonly RendererPayload.VisibilitySettings visibility;

        public RenderFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
            visibility = levelFrame.RenderVisibility;
        }

        public override void RenderAsWindow(float deltaTime)
        {
            if (levelFrame.modelFrameVisible || levelFrame.textureFrameVisible)
                return;

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(260, 520), ImGuiCond.FirstUseEver);
            if (ImGui.Begin(frameName, ref isOpen))
            {
                Render(deltaTime);
            }
            ImGui.End();
        }

        public override void Render(float deltaTime)
        {
            if (ImGui.Checkbox("Moby", ref visibility.enableMoby)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Tie", ref visibility.enableTie)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Shrub", ref visibility.enableShrub)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Skybox", ref visibility.enableSkybox)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Terrain", ref visibility.enableTerrain)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Meshless Models", ref visibility.enableMeshlessModels)) levelFrame.InvalidateView();
            ImGui.Separator();
            if (ImGui.Checkbox("Collision", ref visibility.enableCollision)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Moby Collision", ref visibility.enableMobyCollision)) levelFrame.InvalidateView();
            ImGui.Separator();
            if (ImGui.Checkbox("Spline", ref visibility.enableSpline)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Cuboid", ref visibility.enableCuboid)) levelFrame.InvalidateView();
            ImGui.Separator();
            if (ImGui.Checkbox("Fog", ref visibility.enableFog)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Lighting", ref visibility.enableLighting)) levelFrame.InvalidateView();
            ImGui.Separator();
            if (ImGui.Checkbox("Spheres", ref visibility.enableSpheres)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Cylinders", ref visibility.enableCylinders)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Pills", ref visibility.enablePills)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("SoundInstances", ref visibility.enableSoundInstances)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Cameras", ref visibility.enableGameCameras)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Pointlights", ref visibility.enablePointLights)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("EnvSamples", ref visibility.enableEnvSamples)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("EnvTransitions", ref visibility.enableEnvTransitions)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("GrindPaths", ref visibility.enableGrindPaths)) levelFrame.InvalidateView();
            ImGui.Separator();
            if (ImGui.Checkbox("Transparency", ref visibility.enableTransparency)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Distance Culling", ref visibility.enableDistanceCulling)) levelFrame.InvalidateView();
            if (ImGui.Checkbox("Frustum Culling", ref visibility.enableFrustumCulling)) levelFrame.InvalidateView();
            if (levelFrame.HasValidHook() && ImGui.Checkbox("Visible Culling", ref visibility.enableVisibleCulling)) levelFrame.InvalidateView();

            bool showBangles = levelFrame.ShowBangles;
            if (ImGui.Checkbox("Bangles", ref showBangles))
            {
                levelFrame.ShowBangles = showBangles;
            }

            int antialiasing = levelFrame.Antialiasing;
            ImGui.PushItemWidth(90.0f);
            if (ImGui.Combo("Antialiasing", ref antialiasing, LevelFrame.antialiasingOptions, LevelFrame.antialiasingOptions.Length))
            {
                levelFrame.Antialiasing = antialiasing;
            }
            ImGui.PopItemWidth();

            ImGui.Separator();

            bool enableCameraInfo = levelFrame.EnableCameraInfo;
            if (ImGui.Checkbox("Camera Info", ref enableCameraInfo))
            {
                levelFrame.EnableCameraInfo = enableCameraInfo;
            }
        }
    }
}
