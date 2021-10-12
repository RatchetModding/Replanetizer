// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;

namespace LibReplanetizer.LevelObjects
{
    public interface ITransformable
    {
        void Translate(float x, float y, float z);
        void Translate(Vector3 vector);

        void Rotate(float x, float y, float z);
        void Rotate(Vector3 vector);

        void Scale(Vector3 scale);
        void Scale(float x, float y, float z);
    }
}
