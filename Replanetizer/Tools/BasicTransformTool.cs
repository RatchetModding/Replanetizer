// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Tools
{
    public abstract class BasicTransformTool : Tool
    {
        public abstract void Transform(LevelObject obj, Vector3 vec, Vector3 pivot);

        public void Transform(
            LevelObject obj, Vector3 direction, Vector3 magnitude, Vector3 pivot)
        {
            Transform(obj, ProcessVec(direction, magnitude), pivot);
        }

        public void Transform(Selection selection, Vector3 direction, Vector3 magnitude)
        {
            Vector3 vec = ProcessVec(direction, magnitude);
            Vector3 pivot = selection.pivot;
            foreach (var obj in selection)
            {
                if (pivotPositioning == PivotPositioning.IndividualOrigins)
                    pivot = obj.position;
                Transform(obj, vec, pivot);
            }
        }
    }
}
