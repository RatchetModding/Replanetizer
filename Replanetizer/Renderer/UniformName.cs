// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;

namespace Replanetizer.Renderer
{
    /*
     * For optimal performance we use this enum to assign each uniform name an ID. This way we can avoid
     * string comparisons to search for uniforms by names and can instead just use direct indexing.
     *
     * The names in this enum must match the name of the uniform in the shaders exactly.
     *
     * This also makes renaming uniforms easier. If you rename a uniform in a shader you can use common
     * IDE features like "Rename Symbol" in the enum entry to change all occurrences aswell.
     */
    public enum UniformName
    {
        worldToView,
        modelToWorld,
        levelObjectType,
        levelObjectNumber,
        lightIndex,
        useFog,
        fogParams,
        staticColor,
        texAvailable,
        selected,
        fogColor,
        dissolvePattern,
        objectBlendDistance,
        useTransparency,
        incolor,
        up,
        right,
        position,
        bones,
        fontTexture,
        resolution
    }

}
