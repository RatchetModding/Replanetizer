// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.LevelObjects
{
    /// <summary>
    /// Tests for Moby parsing and serialization (RC1 and RC2/3 layouts).
    /// </summary>
    public class MobyParserTests
    {
        // RC1 moby element size = 0x78
        private const int RC1_SIZE = 0x78;
        // RC2/3 moby element size = 0x88
        private const int RC23_SIZE = 0x88;

        private static byte[] BuildRC1Block(int num,
            int missionID, int spawnType, int mobyID,
            int bolts, int dataval, int modelID, float scale,
            int drawDist, int updateDist,
            float x, float y, float z, float rotx, float roty, float rotz,
            int groupIndex, int isRooted, float rootedDist,
            int pvarIndex, int occlusion, int mode,
            int r, int g, int b, int light, int cutscene)
        {
            byte[] block = new byte[(num + 1) * RC1_SIZE];
            int o = num * RC1_SIZE;
            WriteInt(block, o + 0x00, RC1_SIZE);
            WriteInt(block, o + 0x04, missionID);
            WriteInt(block, o + 0x08, spawnType);
            WriteInt(block, o + 0x0C, mobyID);
            WriteInt(block, o + 0x10, bolts);
            WriteInt(block, o + 0x14, dataval);
            WriteInt(block, o + 0x18, modelID);
            WriteFloat(block, o + 0x1C, scale);
            WriteInt(block, o + 0x20, drawDist);
            WriteInt(block, o + 0x24, updateDist);
            WriteFloat(block, o + 0x30, x);
            WriteFloat(block, o + 0x34, y);
            WriteFloat(block, o + 0x38, z);
            WriteFloat(block, o + 0x3C, rotx);
            WriteFloat(block, o + 0x40, roty);
            WriteFloat(block, o + 0x44, rotz);
            WriteInt(block, o + 0x48, groupIndex);
            WriteInt(block, o + 0x4C, isRooted);
            WriteFloat(block, o + 0x50, rootedDist);
            WriteInt(block, o + 0x58, pvarIndex);
            WriteInt(block, o + 0x5C, occlusion);
            WriteInt(block, o + 0x60, mode);
            WriteInt(block, o + 0x64, r);
            WriteInt(block, o + 0x68, g);
            WriteInt(block, o + 0x6C, b);
            WriteInt(block, o + 0x70, light);
            WriteInt(block, o + 0x74, cutscene);
            return block;
        }

        private static byte[] BuildRC23Block(int num,
            int missionID, int dataval, int spawnType, int mobyID,
            int bolts, int modelID, float scale,
            int drawDist, int updateDist,
            float x, float y, float z, float rotx, float roty, float rotz,
            int groupIndex, int isRooted, float rootedDist,
            int pvarIndex, int occlusion, int mode,
            int r, int g, int b, int light, int cutscene)
        {
            byte[] block = new byte[(num + 1) * RC23_SIZE];
            int o = num * RC23_SIZE;
            WriteInt(block, o + 0x00, RC23_SIZE);
            WriteInt(block, o + 0x04, missionID);
            WriteInt(block, o + 0x08, dataval);
            WriteInt(block, o + 0x0C, spawnType);
            WriteInt(block, o + 0x10, mobyID);
            WriteInt(block, o + 0x14, bolts);
            WriteInt(block, o + 0x28, modelID);
            WriteFloat(block, o + 0x2C, scale);
            WriteInt(block, o + 0x30, drawDist);
            WriteInt(block, o + 0x34, updateDist);
            WriteFloat(block, o + 0x40, x);
            WriteFloat(block, o + 0x44, y);
            WriteFloat(block, o + 0x48, z);
            WriteFloat(block, o + 0x4C, rotx);
            WriteFloat(block, o + 0x50, roty);
            WriteFloat(block, o + 0x54, rotz);
            WriteInt(block, o + 0x58, groupIndex);
            WriteInt(block, o + 0x5C, isRooted);
            WriteFloat(block, o + 0x60, rootedDist);
            WriteInt(block, o + 0x68, pvarIndex);
            WriteInt(block, o + 0x6C, occlusion);
            WriteInt(block, o + 0x70, mode);
            WriteInt(block, o + 0x74, r);
            WriteInt(block, o + 0x78, g);
            WriteInt(block, o + 0x7C, b);
            WriteInt(block, o + 0x80, light);
            WriteInt(block, o + 0x84, cutscene);
            return block;
        }

        // ── RC1 parsing ─────────────────────────────────────────────────────────

        [Fact]
        public void RC1_Constructor_ParsesMissionIdAndSpawnType()
        {
            byte[] block = BuildRC1Block(0, 7, 0b00011, 42, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(7, moby.missionID);
            Assert.True(moby.spawnBeforeMissionCompletion);
            Assert.True(moby.spawnAfterMissionCompletion);
            Assert.False(moby.isCrate);
        }

        [Fact]
        public void RC1_Constructor_ParsesMobyIdAndModelId()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 99, 0, 0, 55, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(99, moby.mobyID);
            Assert.Equal(55, moby.modelID);
        }

        [Fact]
        public void RC1_Constructor_ParsesPosition()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 10f, 20f, 30f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(10f, moby.position.X, 4);
            Assert.Equal(20f, moby.position.Y, 4);
            Assert.Equal(30f, moby.position.Z, 4);
        }

        [Fact]
        public void RC1_Constructor_ParsesScale()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 2.5f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(2.5f, moby.scale.X, 4);
        }

        [Fact]
        public void RC1_Constructor_ParsesBoltsAndDataval()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 150, 7, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(150, moby.bolts);
            Assert.Equal(7, moby.dataval);
        }

        [Fact]
        public void RC1_Constructor_ParsesColor()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 200, 100, 50, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal((byte) 200, moby.color.R);
            Assert.Equal((byte) 100, moby.color.G);
            Assert.Equal((byte) 50, moby.color.B);
        }

        [Fact]
        public void RC1_Constructor_ParsesOcclusion()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 1, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.True(moby.occlusion);
        }

        [Fact]
        public void RC1_Constructor_ParsesDrawAndUpdateDistance()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 128, 64, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(128, moby.drawDistance);
            Assert.Equal(64, moby.updateDistance);
        }

        [Fact]
        public void RC1_Constructor_WithNegativePvarIndex_AssignsEmptyPvars()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(-1, moby.pvarIndex);
            Assert.Empty(moby.pVars);
        }

        [Fact]
        public void RC1_Constructor_WithPvarIndex_AssignsPvars()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, 0, 0, 0, 0, 0, 0, 0, 0);
            var pVars = new List<byte[]> { new byte[] { 0xAA, 0xBB } };
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), pVars);

            Assert.Equal(0, moby.pvarIndex);
            Assert.Equal(new byte[] { 0xAA, 0xBB }, moby.pVars);
        }

        [Fact]
        public void RC1_Constructor_ParsesAtNonZeroIndex()
        {
            // Build a 2-moby block; parse index 1
            byte[] block = new byte[2 * RC1_SIZE];
            // Index 1 position fields
            int o = 1 * RC1_SIZE;
            WriteInt(block, o + 0x00, RC1_SIZE);
            WriteFloat(block, o + 0x1C, 3f); // scale
            WriteFloat(block, o + 0x30, 5f); // x
            WriteFloat(block, o + 0x34, 6f); // y
            WriteFloat(block, o + 0x38, 7f); // z
            WriteInt(block, o + 0x58, -1);   // pvarIndex

            var moby = new Moby(GameType.RaC1, block, 1, new List<Model>(), new List<byte[]>());

            Assert.Equal(3f, moby.scale.X, 4);
            Assert.Equal(5f, moby.position.X, 4);
        }

        // ── RC1 serialization ────────────────────────────────────────────────────

        [Fact]
        public void RC1_ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(GameType.RaC1.mobyElemSize, serialized.Length);
        }

        [Fact]
        public void RC1_ToByteArray_FirstDwordIsElementSize()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(RC1_SIZE, ReadInt(serialized, 0x00));
        }

        [Fact]
        public void RC1_ToByteArray_PreservesMissionId()
        {
            byte[] block = BuildRC1Block(0, 12, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(12, ReadInt(serialized, 0x04));
        }

        [Fact]
        public void RC1_ToByteArray_PreservesModelId()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 77, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(77, ReadInt(serialized, 0x18));
        }

        [Fact]
        public void RC1_ToByteArray_PreservesScale()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1.5f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(1.5f, ReadFloat(serialized, 0x1C), 5);
        }

        [Fact]
        public void RC1_ToByteArray_PreservesColor()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 200, 100, 50, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(200, ReadInt(serialized, 0x64));
            Assert.Equal(100, ReadInt(serialized, 0x68));
            Assert.Equal(50,  ReadInt(serialized, 0x6C));
        }

        [Fact]
        public void RC1_ToByteArray_PreservesOcclusion()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 1, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(1, ReadInt(serialized, 0x5C)); // occlusion
        }

        [Fact]
        public void RC1_ToByteArray_OcclusionFalse_WritesZero()
        {
            byte[] block = BuildRC1Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(0, ReadInt(serialized, 0x5C));
        }

        [Fact]
        public void RC1_ToByteArray_PreservesSpawnType()
        {
            byte[] block = BuildRC1Block(0, 0, 0b00110, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(0b00110, ReadInt(serialized, 0x08));
        }

        // ── RC2/3 parsing ────────────────────────────────────────────────────────

        [Fact]
        public void RC2_Constructor_ParsesMissionIdAndDataval()
        {
            byte[] block = BuildRC23Block(0, 5, 9, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC2, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(5, moby.missionID);
            Assert.Equal(9, moby.dataval);
        }

        [Fact]
        public void RC2_Constructor_ParsesPosition()
        {
            byte[] block = BuildRC23Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 3f, 4f, 5f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC2, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(3f, moby.position.X, 4);
            Assert.Equal(4f, moby.position.Y, 4);
            Assert.Equal(5f, moby.position.Z, 4);
        }

        [Fact]
        public void RC2_Constructor_ParsesModelIdAndScale()
        {
            byte[] block = BuildRC23Block(0, 0, 0, 0, 0, 0, 88, 3f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC2, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(88, moby.modelID);
            Assert.Equal(3f, moby.scale.X, 4);
        }

        [Fact]
        public void RC2_ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildRC23Block(0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC2, block, 0, new List<Model>(), new List<byte[]>());

            Assert.Equal(GameType.RaC2.mobyElemSize, moby.ToByteArray().Length);
        }

        [Fact]
        public void RC2_ToByteArray_PreservesModelId()
        {
            byte[] block = BuildRC23Block(0, 0, 0, 0, 0, 0, 33, 1f, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, -1, 0, 0, 0, 0, 0, 0, 0);
            var moby = new Moby(GameType.RaC2, block, 0, new List<Model>(), new List<byte[]>());
            byte[] serialized = moby.ToByteArray();

            Assert.Equal(33, ReadInt(serialized, 0x28));
        }

        // ── Parameterless constructor ─────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_SetsGame_RaC1()
        {
            var moby = new Moby(GameType.RaC1);
            // We can verify pVars is initialised (not null)
            Assert.NotNull(moby.pVars);
            Assert.Empty(moby.pVars);
        }

        [Fact]
        public void DefaultConstructor_SpawnTypeIsZero()
        {
            var moby = new Moby(GameType.RaC1);
            Assert.False(moby.spawnBeforeMissionCompletion);
            Assert.False(moby.spawnAfterMissionCompletion);
            Assert.False(moby.isCrate);
            Assert.False(moby.spawnBeforeDeath);
            Assert.False(moby.isSpawner);
        }

        // ── Copy constructor ──────────────────────────────────────────────────────

        [Fact]
        public void CopyConstructor_CopiesAllKeyFields()
        {
            byte[] block = BuildRC1Block(0, 5, 0b00011, 99, 150, 7, 55, 2f, 128, 64, 10f, 20f, 30f, 0f, 0f, 0f, 1, 0, 0f, -1, 1, 3, 200, 100, 50, 2, 4);
            var original = new Moby(GameType.RaC1, block, 0, new List<Model>(), new List<byte[]>());
            var copy = new Moby(original);

            Assert.Equal(original.missionID, copy.missionID);
            Assert.Equal(original.bolts, copy.bolts);
            Assert.Equal(original.modelID, copy.modelID);
            Assert.Equal(original.drawDistance, copy.drawDistance);
            Assert.Equal(original.spawnType, copy.spawnType);
            Assert.Equal(original.color, copy.color);
            Assert.Equal(original.position, copy.position);
            Assert.Equal(original.scale, copy.scale);
            Assert.Equal(original.light, copy.light);
        }
    }

    /// <summary>
    /// Tests for Shrub parsing and serialization.
    /// </summary>
    public class ShrubTests
    {
        private static byte[] BuildBlock(int index, Matrix4 transform,
            int modelID, float drawDist, uint off58, uint off5C,
            byte r, byte g, byte b, byte a, uint off64, ushort light, uint off6C)
        {
            byte[] block = new byte[(index + 1) * Shrub.ELEMENTSIZE];
            int o = index * Shrub.ELEMENTSIZE;
            WriteMatrix4(block, o + 0x00, transform);
            WriteInt(block, o + 0x50, modelID);
            WriteFloat(block, o + 0x54, drawDist);
            WriteUint(block, o + 0x58, off58);
            WriteUint(block, o + 0x5C, off5C);
            block[o + 0x60] = r;
            block[o + 0x61] = g;
            block[o + 0x62] = b;
            block[o + 0x63] = a;
            WriteUint(block, o + 0x64, off64);
            WriteUshort(block, o + 0x68, light);
            WriteUshort(block, o + 0x6A, 0xffff);
            WriteUint(block, o + 0x6C, off6C);
            return block;
        }

        [Fact]
        public void Constructor_ParsesModelId()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 42, 100f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            Assert.Equal(42, shrub.modelID);
        }

        [Fact]
        public void Constructor_ParsesDrawDistance()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 75.5f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            Assert.Equal(75.5f, shrub.drawDistance, 4);
        }

        [Fact]
        public void Constructor_ParsesColor()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0f, 0, 0, 200, 100, 50, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            Assert.Equal((byte) 200, shrub.color.R);
            Assert.Equal((byte) 100, shrub.color.G);
            Assert.Equal((byte) 50, shrub.color.B);
        }

        [Fact]
        public void Constructor_ParsesLight()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0f, 0, 0, 0, 0, 0, 255, 0, 7, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            Assert.Equal((ushort) 7, shrub.light);
        }

        [Fact]
        public void Constructor_ExtractsTranslationFromMatrix()
        {
            var transform = Matrix4.CreateTranslation(1f, 2f, 3f);
            byte[] block = BuildBlock(0, transform, 0, 0f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            Assert.Equal(1f, shrub.position.X, 4);
            Assert.Equal(2f, shrub.position.Y, 4);
            Assert.Equal(3f, shrub.position.Z, 4);
        }

        [Fact]
        public void Constructor_ParsesAtNonZeroIndex()
        {
            byte[] block = BuildBlock(2, Matrix4.Identity, 33, 50f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 2, new List<Model>());
            Assert.Equal(33, shrub.modelID);
            Assert.Equal(50f, shrub.drawDistance, 4);
        }

        [Fact]
        public void ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            Assert.Equal(Shrub.ELEMENTSIZE, shrub.ToByteArray().Length);
        }

        [Fact]
        public void ToByteArray_PreservesModelId()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 77, 0f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            byte[] serialized = shrub.ToByteArray();
            Assert.Equal(77, ReadInt(serialized, 0x50));
        }

        [Fact]
        public void ToByteArray_PreservesDrawDistance()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 88.0f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            byte[] serialized = shrub.ToByteArray();
            Assert.Equal(88.0f, ReadFloat(serialized, 0x54), 4);
        }

        [Fact]
        public void ToByteArray_PreservesColor()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0f, 0, 0, 128, 64, 32, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            byte[] serialized = shrub.ToByteArray();
            Assert.Equal(128, serialized[0x60]);
            Assert.Equal(64, serialized[0x61]);
            Assert.Equal(32, serialized[0x62]);
        }

        [Fact]
        public void ToByteArray_PreservesLight()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0f, 0, 0, 0, 0, 0, 255, 0, 5, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            byte[] serialized = shrub.ToByteArray();
            Assert.Equal((ushort) 5, ReadUshort(serialized, 0x68));
        }

        [Fact]
        public void ToByteArray_AlwaysWritesFfffAt6A()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            byte[] serialized = shrub.ToByteArray();
            Assert.Equal(0xffff, ReadUshort(serialized, 0x6A));
        }

        [Fact]
        public void ToByteArray_TranslationPreservedInMatrix()
        {
            var transform = Matrix4.CreateTranslation(4f, 5f, 6f);
            byte[] block = BuildBlock(0, transform, 0, 0f, 0, 0, 0, 0, 0, 255, 0, 0, 0);
            var shrub = new Shrub(block, 0, new List<Model>());
            byte[] serialized = shrub.ToByteArray();
            Matrix4 recovered = ReadMatrix4(serialized, 0x00);
            Assert.Equal(transform.Row3.X, recovered.Row3.X, 4);
            Assert.Equal(transform.Row3.Y, recovered.Row3.Y, 4);
            Assert.Equal(transform.Row3.Z, recovered.Row3.Z, 4);
        }

        [Fact]
        public void CopyConstructor_CopiesAllKeyFields()
        {
            byte[] block = BuildBlock(0, Matrix4.CreateTranslation(1f, 2f, 3f), 42, 75f, 1, 2, 200, 100, 50, 255, 3, 7, 4);
            var original = new Shrub(block, 0, new List<Model>());
            var copy = new Shrub(original);

            Assert.Equal(original.modelID, copy.modelID);
            Assert.Equal(original.drawDistance, copy.drawDistance);
            Assert.Equal(original.color, copy.color);
            Assert.Equal(original.light, copy.light);
            Assert.Equal(original.position, copy.position);
        }
    }

    /// <summary>
    /// Tests for Tie parsing and serialization.
    /// Tie constructor needs a FileStream but only uses it when a model is found;
    /// passing null is safe when the model list is empty.
    /// </summary>
    public class TieTests
    {
        // ELEMENTSIZE = 0x70 (const, not public static — use literal)
        private const int TIE_ELEMENTSIZE = 0x70;

        private static byte[] BuildBlock(int index, Matrix4 transform,
            int modelID, uint off54, uint off58, uint off5C,
            int colorOffset, uint off64, ushort light, uint off6C)
        {
            byte[] block = new byte[(index + 1) * TIE_ELEMENTSIZE];
            int o = index * TIE_ELEMENTSIZE;
            WriteMatrix4(block, o + 0x00, transform);
            WriteInt(block, o + 0x50, modelID);
            WriteUint(block, o + 0x54, off54);
            WriteUint(block, o + 0x58, off58);
            WriteUint(block, o + 0x5C, off5C);
            WriteInt(block, o + 0x60, colorOffset);
            WriteUint(block, o + 0x64, off64);
            WriteUshort(block, o + 0x68, light);
            WriteUshort(block, o + 0x6A, 0xffff);
            WriteUint(block, o + 0x6C, off6C);
            return block;
        }

        [Fact]
        public void Constructor_ParsesModelId()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 99, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            Assert.Equal(99, tie.modelID);
        }

        [Fact]
        public void Constructor_ParsesLight()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0, 0, 0, 0, 0, 3, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            Assert.Equal((ushort) 3, tie.light);
        }

        [Fact]
        public void Constructor_ParsesOff54()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0x4000u, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            Assert.Equal(0x4000u, tie.off54);
        }

        [Fact]
        public void Constructor_ExtractsTranslationFromMatrix()
        {
            var transform = Matrix4.CreateTranslation(7f, 8f, 9f);
            byte[] block = BuildBlock(0, transform, 0, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            Assert.Equal(7f, tie.position.X, 4);
            Assert.Equal(8f, tie.position.Y, 4);
            Assert.Equal(9f, tie.position.Z, 4);
        }

        [Fact]
        public void Constructor_WithNoMatchingModel_ColorBytesEmpty()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 999, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            Assert.Empty(tie.colorBytes);
        }

        [Fact]
        public void Constructor_ParsesAtNonZeroIndex()
        {
            byte[] block = BuildBlock(2, Matrix4.CreateTranslation(1f, 2f, 3f), 55, 0, 0, 0, 0, 0, 4, 0);
            var tie = new Tie(block, 2, new List<Model>(), null!);
            Assert.Equal(55, tie.modelID);
            Assert.Equal((ushort) 4, tie.light);
        }

        [Fact]
        public void ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            Assert.Equal(TIE_ELEMENTSIZE, tie.ToByteArray().Length);
        }

        [Fact]
        public void ToByteArray_PreservesModelId()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 44, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            byte[] serialized = tie.ToByteArray();
            Assert.Equal(44, ReadInt(serialized, 0x50));
        }

        [Fact]
        public void ToByteArray_PreservesLight()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0, 0, 0, 0, 0, 6, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            byte[] serialized = tie.ToByteArray();
            Assert.Equal((ushort) 6, ReadUshort(serialized, 0x68));
        }

        [Fact]
        public void ToByteArray_AlwaysWritesFfffAt6A()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            byte[] serialized = tie.ToByteArray();
            Assert.Equal(0xffff, ReadUshort(serialized, 0x6A));
        }

        [Fact]
        public void ToByteArray_PreservesOff54()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, 0, 0x1234u, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            byte[] serialized = tie.ToByteArray();
            Assert.Equal(0x1234u, ReadUint(serialized, 0x54));
        }

        [Fact]
        public void ToByteArray_TranslationPreservedInMatrix()
        {
            var transform = Matrix4.CreateTranslation(2f, 4f, 6f);
            byte[] block = BuildBlock(0, transform, 0, 0, 0, 0, 0, 0, 0, 0);
            var tie = new Tie(block, 0, new List<Model>(), null!);
            byte[] serialized = tie.ToByteArray();
            Matrix4 recovered = ReadMatrix4(serialized, 0x00);
            Assert.Equal(transform.Row3.X, recovered.Row3.X, 4);
            Assert.Equal(transform.Row3.Y, recovered.Row3.Y, 4);
            Assert.Equal(transform.Row3.Z, recovered.Row3.Z, 4);
        }

        [Fact]
        public void CopyConstructor_CopiesAllKeyFields()
        {
            byte[] block = BuildBlock(0, Matrix4.CreateTranslation(1f, 2f, 3f), 42, 0x4000u, 0, 0, 0, 0, 5, 0);
            var original = new Tie(block, 0, new List<Model>(), null!);
            var copy = new Tie(original);

            Assert.Equal(original.modelID, copy.modelID);
            Assert.Equal(original.off54, copy.off54);
            Assert.Equal(original.light, copy.light);
            Assert.Equal(original.position, copy.position);
        }
    }

    /// <summary>
    /// Tests for the Terrain container class (no FileStream required).
    /// </summary>
    public class TerrainTests
    {
        [Fact]
        public void Constructor_SetsLevelNumber()
        {
            var terrain = new Terrain(new List<TerrainFragment>(), 3);
            Assert.Equal((ushort) 3, terrain.levelNumber);
        }

        [Fact]
        public void Constructor_SetsFragmentList()
        {
            var fragments = new List<TerrainFragment>();
            var terrain = new Terrain(fragments, 0);
            Assert.Same(fragments, terrain.fragments);
        }

        [Fact]
        public void Constructor_EmptyFragments_CountIsZero()
        {
            var terrain = new Terrain(new List<TerrainFragment>(), 1);
            Assert.Empty(terrain.fragments);
        }
    }
}
