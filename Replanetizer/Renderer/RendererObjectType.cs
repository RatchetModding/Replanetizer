// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;

namespace Replanetizer.Renderer
{
    public enum RenderedObjectType
    {
        Null = 0,
        Terrain = 1,
        Shrub = 2,
        Tie = 3,
        Moby = 4,
        Spline = 5,
        Cuboid = 6,
        Sphere = 7,
        Cylinder = 8,
        Pill = 9,
        SoundInstance = 10,
        GameCamera = 11,
        PointLight = 12,
        EnvSample = 13,
        EnvTransition = 14,
        GrindPath = 15,
        Tool = 16,
        Skybox = 999
    }

    public static class RenderedObjectTypeUtils
    {
        public static RenderedObjectType GetRenderTypeFromLevelObject(LevelObject obj)
        {
            switch (obj)
            {
                case TerrainFragment:
                    return RenderedObjectType.Terrain;
                case Shrub:
                    return RenderedObjectType.Shrub;
                case Tie:
                    return RenderedObjectType.Tie;
                case Moby:
                    return RenderedObjectType.Moby;
                case Spline:
                    return RenderedObjectType.Spline;
                case Cuboid:
                    return RenderedObjectType.Cuboid;
                case Sphere:
                    return RenderedObjectType.Sphere;
                case Cylinder:
                    return RenderedObjectType.Cylinder;
                case Pill:
                    return RenderedObjectType.Pill;
                case SoundInstance:
                    return RenderedObjectType.SoundInstance;
                case GameCamera:
                    return RenderedObjectType.GameCamera;
                case PointLight:
                    return RenderedObjectType.PointLight;
                case EnvSample:
                    return RenderedObjectType.EnvSample;
                case EnvTransition:
                    return RenderedObjectType.EnvTransition;
                case GrindPath:
                    return RenderedObjectType.GrindPath;
            }

            return RenderedObjectType.Null;
        }
    }

}
