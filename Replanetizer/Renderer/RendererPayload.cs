// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Replanetizer.Tools;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class RendererPayload
    {
        public class VisibilitySettings
        {
            public bool[] chunks = new bool[5] { true, true, true, true, true };

            public uint subModelsMask = 0xFFFFFFFF;

            public bool enableMoby = true, enableTie = true, enableShrub = true, enableSpline = false,
            enableCuboid = false, enableSpheres = false, enableCylinders = false, enablePills = false,
            enableSkybox = true, enableTerrain = true, enableCollision = false, enableTransparency = true,
            enableDistanceCulling = true, enableFrustumCulling = true, enableFog = true, enableGameCameras = false,
            enablePointLights = false, enableEnvSamples = false, enableEnvTransitions = false, enableSoundInstances = false,
            enableGrindPaths = false, enableMeshlessModels = false, enableAnimations = false, enableLighting = true;

            public VisibilitySettings()
            {

            }
        }

        public Camera camera;
        public int width;
        public int height;
        public Selection selection;
        public Toolbox? toolbox;
        public VisibilitySettings visibility = new VisibilitySettings();

        // This is used for animations. In seconds.
        public float deltaTime = 1.0f;

        // This is used for the model viewer.
        public int forcedAnimationID = 0;

        public void SetShowSubmodels(bool showSubModels)
        {
            this.visibility.subModelsMask = showSubModels ? 0xFFFFFFFF : 0;
        }

        public RendererPayload(Camera camera, Selection? selection = null, Toolbox? toolbox = null, bool showSubModels = true)
        {
            this.camera = camera;
            this.selection = (selection != null) ? selection : new Selection();
            this.toolbox = toolbox;

            SetShowSubmodels(showSubModels);
        }

        public void SetWindowSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

}
