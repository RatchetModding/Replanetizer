// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

namespace Replanetizer.Utils
{
    public class ShaderIDTable
    {
        public int shaderMain { get; set; }
        public int shaderColor { get; set; }
        public int shaderCollision { get; set; }
        public int shaderSky { get; set; }
        public int uniformColor { get; set; }
        public int uniformWorldToViewMatrix { get; set; }
        public int uniformModelToWorldMatrix { get; set; }
        public int uniformColorWorldToViewMatrix { get; set; }
        public int uniformColorModelToWorldMatrix { get; set; }
        public int uniformCollisionWorldToViewMatrix { get; set; }
        public int uniformSkyWorldToViewMatrix { get; set; }
        public int uniformUseFog { get; set; }
        public int uniformFogColor { get; set; }
        public int uniformFogFarDist { get; set; }
        public int uniformFogNearDist { get; set; }
        public int uniformFogFarIntensity { get; set; }
        public int uniformFogNearIntensity { get; set; }
        public int uniformLevelObjectType { get; set; }
        public int uniformLevelObjectNumber { get; set; }
        public int uniformColorLevelObjectType { get; set; }
        public int uniformColorLevelObjectNumber { get; set; }
        public int uniformAmbientColor { get; set; }

        // Number of lights we allocate, if you change this, you need to change it in the shader aswell.
        public static readonly int ALLOCATED_LIGHTS = 20;
        public int uniformLightIndex { get; set; }
        public int uniformSkyTexAvailable { get; set; }
    }
}
