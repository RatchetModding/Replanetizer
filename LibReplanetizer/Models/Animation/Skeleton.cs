// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;

namespace LibReplanetizer.Models.Animations
{
    public class Skeleton
    {
        public BoneMatrix bone;
        public List<Skeleton> children;
        public Skeleton? parent;

        /// <summary>
        /// Creates a new skeleton with a root node. Use this function once and use InsertBone to construct the skeleton.
        /// </summary>
        public Skeleton(BoneMatrix root, Skeleton? parent)
        {
            bone = root;
            this.parent = parent;
            children = new List<Skeleton>();
        }

        public bool InsertBone(BoneMatrix bone)
        {
            if (this.bone.id == bone.parent)
            {
                children.Add(new Skeleton(bone, this));
                return true;
            }

            bool found = false;

            foreach (Skeleton skel in children)
            {
                found = skel.InsertBone(bone);
                if (found) break;
            }

            return found;
        }
    }
}
