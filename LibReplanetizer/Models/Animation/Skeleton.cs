// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using OpenTK.Mathematics;

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

        public bool InsertBone(BoneMatrix bone, int parentBoneID)
        {
            if (this.bone.id == parentBoneID)
            {
                children.Add(new Skeleton(bone, this));
                return true;
            }

            bool found = false;

            foreach (Skeleton skel in children)
            {
                found = skel.InsertBone(bone, parentBoneID);
                if (found) break;
            }

            return found;
        }

        public Matrix4 GetRelativeTransformation()
        {
            // This does not work for Deadlocked yet. Rotation is already inverted in Deadlocked so we will need to invert again here to obtain
            // the bind matrix as is needed here.
            Matrix3x4 trans = bone.transformation;
            Matrix3 rotation = new Matrix3(trans.Row0.Xyz, trans.Row1.Xyz, trans.Row2.Xyz);

            // We need to represent our transformation relative to the parent node
            if (parent != null)
            {
                Matrix3x4 matP = parent.bone.transformation;
                Matrix3 matPTrans = new Matrix3(matP.Row0.Xyz, matP.Row1.Xyz, matP.Row2.Xyz);
                matPTrans.Transpose();
                rotation = matPTrans * rotation;
            }

            Matrix4 result = new Matrix4(rotation);
            result.M14 = trans.M14 / 1024.0f;
            result.M24 = trans.M24 / 1024.0f;
            result.M34 = trans.M34 / 1024.0f;
            result.M44 = 1.0f;

            return result;
        }
    }
}
