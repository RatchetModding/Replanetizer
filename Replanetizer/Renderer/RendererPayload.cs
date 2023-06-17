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

            public bool enableMoby = true, enableTie = true, enableShrub = true, enableSpline = false,
            enableCuboid = false, enableSpheres = false, enableCylinders = false, enablePills = false,
            enableSkybox = true, enableTerrain = true, enableCollision = false, enableTransparency = true,
            enableDistanceCulling = true, enableFrustumCulling = true, enableFog = true, enableGameCameras = false,
            enablePointLights = false, enableEnvSamples = false, enableEnvTransitions = false, enableSoundInstances = false,
            enableGrindPaths = false, enableMeshlessModels = true, enableAnimations = false;

            public VisibilitySettings()
            {

            }
        }

        public Camera camera;
        public Selection selection;
        public Toolbox? toolbox;
        public VisibilitySettings visibility = new VisibilitySettings();

        // This is used for animations.
        public float deltaTime = 1.0f;

        // This is used for the model viewer.
        public int forcedAnimationID = 0;

        public RendererPayload(Camera camera, Selection? selection = null, Toolbox? toolbox = null)
        {
            this.camera = camera;
            this.selection = (selection != null) ? selection : new Selection();
            this.toolbox = toolbox;
        }
    }

}
