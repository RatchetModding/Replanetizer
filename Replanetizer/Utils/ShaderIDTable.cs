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
        public int ShaderMain { get; set; }
        public int ShaderColor { get; set; }
        public int ShaderCollision { get; set; }
        public int UniformColor { get; set; }
        public int UniformWorldToViewMatrix { get; set; }
        public int UniformModelToWorldMatrix { get; set; }
        public int UniformColorWorldToViewMatrix { get; set; }
        public int UniformColorModelToWorldMatrix { get; set; }
        public int UniformCollisionWorldToViewMatrix { get; set; }
        public int UniformUseFog { get; set; }
        public int UniformFogColor { get; set; }
        public int UniformFogFarDist { get; set; }
        public int UniformFogNearDist { get; set; }
        public int UniformFogFarIntensity { get; set; }
        public int UniformFogNearIntensity { get; set; }
        public int UniformLevelObjectType { get; set; }
        public int UniformLevelObjectNumber { get; set; }
        public int UniformColorLevelObjectType { get; set; }
        public int UniformColorLevelObjectNumber { get; set; }
        public int UniformAmbientColor { get; set; }

        // Number of lights we allocate, if you change this, you need to change it in the shader aswell.
        public static readonly int ALLOCATED_LIGHTS = 20;
        public int UniformLightIndex { get; set; }
    }
}