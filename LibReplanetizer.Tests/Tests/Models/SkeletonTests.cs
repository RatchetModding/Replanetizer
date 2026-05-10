// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Xunit;
using LibReplanetizer.Models.Animations;
using LibReplanetizer.LevelObjects;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.Models
{
    public class SkeletonTests
    {
        /// <summary>
        /// Builds a minimal 0x40-byte RC1 BoneMatrix block for the bone at
        /// index <paramref name="num"/>.  The rotation is identity and the
        /// cumulative offsets are all zero.
        /// </summary>
        private static BoneMatrix MakeBoneMatrix(int num, short parentBoneId = -1)
        {
            int totalSize = (num + 1) * 0x40;
            byte[] block = new byte[totalSize];
            int offset = num * 0x40;

            // Write a 3x4 identity-ish rotation matrix (row-major, big-endian).
            WriteFloat(block, offset + 0x00, 1.0f); // M11
            WriteFloat(block, offset + 0x14, 1.0f); // M22
            WriteFloat(block, offset + 0x28, 1.0f); // M33

            // cumulativeOffset = (0, 0, 0)
            WriteFloat(block, offset + 0x30, 0.0f);
            WriteFloat(block, offset + 0x34, 0.0f);
            WriteFloat(block, offset + 0x38, 0.0f);

            WriteShort(block, offset + 0x3C, 0);                          // unk0x3C
            WriteShort(block, offset + 0x3E, (short) (parentBoneId * 0x40)); // parent raw

            return new BoneMatrix(GameType.RaC1, block, num);
        }

        [Fact]
        public void Skeleton_Construction_HasCorrectRootBone()
        {
            BoneMatrix root = MakeBoneMatrix(0);
            var skel = new Skeleton(root, null);

            Assert.Equal(root, skel.bone);
            Assert.Null(skel.parent);
            Assert.Empty(skel.children);
        }

        [Fact]
        public void Skeleton_InsertBone_AddsChild()
        {
            BoneMatrix root = MakeBoneMatrix(0);
            BoneMatrix child = MakeBoneMatrix(1, parentBoneId: 0);
            var skel = new Skeleton(root, null);

            bool inserted = skel.InsertBone(child, parentBoneID: 0);

            Assert.True(inserted);
            Assert.Single(skel.children);
            Assert.Equal(child, skel.children[0].bone);
        }

        [Fact]
        public void Skeleton_InsertBone_SetsChildParent()
        {
            BoneMatrix root = MakeBoneMatrix(0);
            BoneMatrix child = MakeBoneMatrix(1, parentBoneId: 0);
            var skel = new Skeleton(root, null);

            skel.InsertBone(child, parentBoneID: 0);

            Assert.Equal(skel, skel.children[0].parent);
        }

        [Fact]
        public void Skeleton_InsertBone_ReturnsFalse_WhenParentNotFound()
        {
            BoneMatrix root = MakeBoneMatrix(0);
            BoneMatrix bone = MakeBoneMatrix(1, parentBoneId: 99);
            var skel = new Skeleton(root, null);

            bool inserted = skel.InsertBone(bone, parentBoneID: 99);

            Assert.False(inserted);
        }

        [Fact]
        public void Skeleton_InsertBone_DeepInsertion()
        {
            BoneMatrix root  = MakeBoneMatrix(0);
            BoneMatrix child = MakeBoneMatrix(1, parentBoneId: 0);
            BoneMatrix grand = MakeBoneMatrix(2, parentBoneId: 1);

            var skel = new Skeleton(root, null);
            skel.InsertBone(child, parentBoneID: 0);
            bool inserted = skel.InsertBone(grand, parentBoneID: 1);

            Assert.True(inserted);
            Assert.Single(skel.children[0].children);
            Assert.Equal(grand, skel.children[0].children[0].bone);
        }

        [Fact]
        public void Skeleton_GetRelativeTransformation_RootReturnsMatrix()
        {
            BoneMatrix root = MakeBoneMatrix(0);
            var skel = new Skeleton(root, null);

            var mat = skel.GetRelativeTransformation();

            // M44 should always be 1.0f
            Assert.Equal(1.0f, mat.M44);
        }
    }
}
