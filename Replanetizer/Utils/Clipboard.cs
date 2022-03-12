// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer;
using System.Collections.Generic;
using Replanetizer.Frames;

namespace Replanetizer.Utils
{
    /// <summary>
    /// A class which handles the clipboard feature.
    /// Currently there are only shrubs supported.
    /// TODO:
    ///     - Mobies (Implemented but copies crash the game)
    ///     - Ties (Implemented but issues with occlusion system)
    /// </summary>
    public class Clipboard
    {
        private List<LevelObject>? content = null;

        /// <summary>
        /// Takes a selection and copies all supported level object into the clipboard.
        /// </summary>
        public void Copy(Selection? selection)
        {
            if (selection == null) return;

            content = new List<LevelObject>();

            List<LevelObject> originalObjects = selection.ToList();

            foreach (LevelObject o in originalObjects)
            {
                // Add different types here once they are supported
                if (o is Moby || o is Shrub || o is Tie)
                    content.Add(o.Clone());
            }
        }

        /// <summary>
        /// Adds all elements from the clipboard to the level/levelFrame.
        /// Automatically selects all added elements if the clipboard was not empty.
        /// </summary>
        public void Apply(Level level, LevelFrame levelFrame)
        {
            if (content == null || content.Count == 0) return;

            levelFrame.selectedObjects.Clear();

            foreach (LevelObject o in content)
            {
                LevelObject o2 = o.Clone();

                if (o2 is Moby mob)
                {
                    level.mobs.Add(mob);
                    levelFrame.mobiesBuffers.Add(new RenderableBuffer(mob, RenderedObjectType.Moby, level.mobs.Count - 1, level, levelFrame.textureIds));
                    levelFrame.selectedObjects.Add(mob);
                }
                else if (o2 is Shrub shrub)
                {
                    level.shrubs.Add(shrub);
                    levelFrame.shrubsBuffers.Add(new RenderableBuffer(shrub, RenderedObjectType.Shrub, level.shrubs.Count - 1, level, levelFrame.textureIds));
                    levelFrame.selectedObjects.Add(shrub);
                }
                else if (o2 is Tie tie)
                {
                    level.ties.Add(tie);
                    levelFrame.tiesBuffers.Add(new RenderableBuffer(tie, RenderedObjectType.Tie, level.ties.Count - 1, level, levelFrame.textureIds));
                    levelFrame.selectedObjects.Add(tie);
                }
            }
        }
    }
}
