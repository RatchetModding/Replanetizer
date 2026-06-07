// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using Xunit;
using LibReplanetizer.LevelObjects;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.LevelObjects
{
    public class SplineTests
    {
        private static byte[] BuildSplineBlock(float[][] points, float[] wVals)
        {
            int count = points.Length;
            // Layout: 0x10 header (count at 0x00, rest zeroed) + count * 0x10 point entries
            byte[] block = new byte[0x10 + count * 0x10];
            WriteUint(block, 0x00, (uint) count);
            for (int i = 0; i < count; i++)
            {
                WriteFloat(block, 0x10 + i * 0x10 + 0x00, points[i][0]);
                WriteFloat(block, 0x10 + i * 0x10 + 0x04, points[i][1]);
                WriteFloat(block, 0x10 + i * 0x10 + 0x08, points[i][2]);
                WriteFloat(block, 0x10 + i * 0x10 + 0x0C, wVals[i]);
            }
            return block;
        }

        [Fact]
        public void Constructor_ParsesVertexBuffer()
        {
            float[][] pts = { new[] { 1f, 2f, 3f }, new[] { 4f, 5f, 6f } };
            float[] w = { 0.25f, 0.75f };
            byte[] block = BuildSplineBlock(pts, w);

            var spline = new Spline(block, 0, 0);

            Assert.Equal(6, spline.vertexBuffer.Length);
            Assert.Equal(1f, spline.vertexBuffer[0]);
            Assert.Equal(2f, spline.vertexBuffer[1]);
            Assert.Equal(3f, spline.vertexBuffer[2]);
            Assert.Equal(4f, spline.vertexBuffer[3]);
        }

        [Fact]
        public void Constructor_ParsesWValues()
        {
            float[][] pts = { new[] { 0f, 0f, 0f }, new[] { 1f, 0f, 0f } };
            float[] w = { 0.1f, 0.9f };
            byte[] block = BuildSplineBlock(pts, w);

            var spline = new Spline(block, 0, 0);

            Assert.Equal(w[0], spline.wVals[0]);
            Assert.Equal(w[1], spline.wVals[1]);
        }

        [Fact]
        public void ToByteArray_RoundTrip_MatchesOriginal()
        {
            float[][] pts = { new[] { 1f, 2f, 3f }, new[] { 4f, 5f, 6f }, new[] { 7f, 8f, 9f } };
            float[] w = { 0f, 0.5f, 1f };
            byte[] original = BuildSplineBlock(pts, w);

            var spline = new Spline(original, 0, 0);
            byte[] serialized = spline.ToByteArray();

            Assert.Equal(original, serialized);
        }

        [Fact]
        public void GetVertex_ReturnsCorrectVector()
        {
            float[][] pts = { new[] { 1f, 2f, 3f }, new[] { 7f, 8f, 9f } };
            float[] w = { 0f, 1f };
            var spline = new Spline(BuildSplineBlock(pts, w), 0, 0);

            var v = spline.GetVertex(1);

            Assert.Equal(new Vector3(7f, 8f, 9f), v);
        }

        [Fact]
        public void SetVertex_UpdatesVertexBuffer()
        {
            float[][] pts = { new[] { 0f, 0f, 0f } };
            float[] w = { 0f };
            var spline = new Spline(BuildSplineBlock(pts, w), 0, 0);

            spline.SetVertex(0, new Vector3(5f, 6f, 7f));

            Assert.Equal(new Vector3(5f, 6f, 7f), spline.GetVertex(0));
        }
    }

    public class CuboidTests
    {
        private static byte[] BuildCuboidBlock(int index, Matrix4 transform, Matrix4 inverse)
        {
            byte[] block = new byte[(index + 1) * Cuboid.ELEMENTSIZE];
            int offset = index * Cuboid.ELEMENTSIZE;
            WriteMatrix4(block, offset + 0x00, transform);
            WriteMatrix4(block, offset + 0x40, inverse);
            return block;
        }

        [Fact]
        public void Constructor_SetsId()
        {
            byte[] block = BuildCuboidBlock(2, Matrix4.Identity, Matrix4.Identity);
            var cuboid = new Cuboid(block, 2);
            Assert.Equal(2, cuboid.id);
        }

        [Fact]
        public void ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildCuboidBlock(0, Matrix4.Identity, Matrix4.Identity);
            var cuboid = new Cuboid(block, 0);
            byte[] serialized = cuboid.ToByteArray();
            Assert.Equal(Cuboid.ELEMENTSIZE, serialized.Length);
        }

        [Fact]
        public void ToByteArray_RoundTrip_TransformMatrixPreserved()
        {
            var transform = Matrix4.CreateTranslation(1f, 2f, 3f);
            byte[] block = BuildCuboidBlock(0, transform, Matrix4.Identity);
            var cuboid = new Cuboid(block, 0);
            byte[] serialized = cuboid.ToByteArray();

            // Re-read the transform matrix from the serialized bytes
            Matrix4 recovered = ReadMatrix4(serialized, 0x00);
            // Positions encoded in the translation row
            Assert.Equal(transform.Row3.X, recovered.Row3.X, 4);
            Assert.Equal(transform.Row3.Y, recovered.Row3.Y, 4);
            Assert.Equal(transform.Row3.Z, recovered.Row3.Z, 4);
        }
    }

    public class SoundInstanceTests
    {
        private static byte[] BuildBlock(int index, ushort id, ushort id2, int funcPtr, int pvarIdx, float updateDist)
        {
            byte[] block = new byte[(index + 1) * SoundInstance.ELEMENTSIZE];
            int offset = index * SoundInstance.ELEMENTSIZE;
            WriteUshort(block, offset + 0x00, id);
            WriteUshort(block, offset + 0x02, id2);
            WriteInt(block, offset + 0x04, funcPtr);
            WriteInt(block, offset + 0x08, pvarIdx);
            WriteFloat(block, offset + 0x0C, updateDist);
            WriteMatrix4(block, offset + 0x10, Matrix4.Identity);
            WriteMatrix4(block, offset + 0x50, Matrix4.Identity);
            return block;
        }

        [Fact]
        public void Constructor_ParsesAllFields()
        {
            byte[] block = BuildBlock(0, 42, 7, 0xDEAD, 3, 15.5f);
            var si = new SoundInstance(block, 0);

            Assert.Equal((ushort) 42, si.id);
            Assert.Equal((ushort) 7, si.id2);
            Assert.Equal(0xDEAD, si.functionPointer);
            Assert.Equal(3, si.pvarIndex);
            Assert.Equal(15.5f, si.updateDistance);
        }

        [Fact]
        public void ToByteArray_RoundTrip_MatchesOriginal()
        {
            byte[] original = BuildBlock(0, 42, 7, 0xDEAD, 3, 15.5f);
            var si = new SoundInstance(original, 0);
            byte[] serialized = si.ToByteArray();

            Assert.Equal(SoundInstance.ELEMENTSIZE, serialized.Length);
            // Verify key scalar fields
            Assert.Equal(original[0], serialized[0]);
            Assert.Equal(original[1], serialized[1]);
            Assert.Equal(ReadFloat(original, 0x0C), ReadFloat(serialized, 0x0C));
        }
    }

    public class DirectionalLightTests
    {
        private static byte[] BuildBlock(int index, Vector4 colorA, Vector4 dirA, Vector4 colorB, Vector4 dirB)
        {
            byte[] block = new byte[(index + 1) * DirectionalLight.ELEMENTSIZE];
            int offset = index * DirectionalLight.ELEMENTSIZE;
            WriteFloat(block, offset + 0x00, colorA.X);
            WriteFloat(block, offset + 0x04, colorA.Y);
            WriteFloat(block, offset + 0x08, colorA.Z);
            WriteFloat(block, offset + 0x0C, colorA.W);
            WriteFloat(block, offset + 0x10, dirA.X);
            WriteFloat(block, offset + 0x14, dirA.Y);
            WriteFloat(block, offset + 0x18, dirA.Z);
            WriteFloat(block, offset + 0x1C, dirA.W);
            WriteFloat(block, offset + 0x20, colorB.X);
            WriteFloat(block, offset + 0x24, colorB.Y);
            WriteFloat(block, offset + 0x28, colorB.Z);
            WriteFloat(block, offset + 0x2C, colorB.W);
            WriteFloat(block, offset + 0x30, dirB.X);
            WriteFloat(block, offset + 0x34, dirB.Y);
            WriteFloat(block, offset + 0x38, dirB.Z);
            WriteFloat(block, offset + 0x3C, dirB.W);
            return block;
        }

        [Fact]
        public void Constructor_ParsesAllVectors()
        {
            var cA = new Vector4(1f, 0f, 0f, 1f);
            var dA = new Vector4(0f, 1f, 0f, 0f);
            var cB = new Vector4(0f, 0f, 1f, 1f);
            var dB = new Vector4(0f, 0f, 1f, 0f);
            byte[] block = BuildBlock(0, cA, dA, cB, dB);
            var dl = new DirectionalLight(block, 0);

            Assert.Equal(cA, dl.colorA);
            Assert.Equal(dA, dl.directionA);
            Assert.Equal(cB, dl.colorB);
            Assert.Equal(dB, dl.directionB);
        }

        [Fact]
        public void ToByteArray_RoundTrip_MatchesOriginal()
        {
            var cA = new Vector4(0.5f, 0.6f, 0.7f, 1f);
            var dA = new Vector4(1f, 0f, 0f, 0f);
            var cB = new Vector4(0.1f, 0.2f, 0.3f, 1f);
            var dB = new Vector4(0f, 1f, 0f, 0f);
            byte[] original = BuildBlock(0, cA, dA, cB, dB);
            var dl = new DirectionalLight(original, 0);
            byte[] serialized = dl.ToByteArray();

            Assert.Equal(original, serialized);
        }
    }

    public class PointLightTests
    {
        private static byte[] BuildRC1Block(int index, float x, float y, float z, float radius, byte r, byte g, byte b)
        {
            int elemSize = 0x20;
            byte[] block = new byte[(index + 1) * elemSize];
            int offset = index * elemSize;
            WriteFloat(block, offset + 0x00, x);
            WriteFloat(block, offset + 0x04, y);
            WriteFloat(block, offset + 0x08, z);
            WriteFloat(block, offset + 0x0C, radius);
            block[offset + 0x10] = r;
            block[offset + 0x11] = g;
            block[offset + 0x12] = b;
            return block;
        }

        [Fact]
        public void Constructor_RC1_ParsesPosition()
        {
            byte[] block = BuildRC1Block(0, 1f, 2f, 3f, 5f, 128, 64, 32);
            var light = new PointLight(GameType.RaC1, block, 0);

            Assert.Equal(1f, light.position.X);
            Assert.Equal(2f, light.position.Y);
            Assert.Equal(3f, light.position.Z);
        }

        [Fact]
        public void Constructor_RC1_ParsesRadius()
        {
            byte[] block = BuildRC1Block(0, 0f, 0f, 0f, 7.5f, 0, 0, 0);
            var light = new PointLight(GameType.RaC1, block, 0);
            Assert.Equal(7.5f, light.radius);
        }

        [Fact]
        public void Constructor_RC1_ParsesColor()
        {
            byte[] block = BuildRC1Block(0, 0f, 0f, 0f, 0f, 255, 128, 0);
            var light = new PointLight(GameType.RaC1, block, 0);

            Assert.Equal(1.0f, light.color.X, 3);
            Assert.Equal(128 / 255.0f, light.color.Y, 3);
            Assert.Equal(0.0f, light.color.Z, 3);
        }

        [Fact]
        public void ToByteArray_RC1_RoundTrip_MatchesOriginal()
        {
            byte[] original = BuildRC1Block(0, 1f, 2f, 3f, 5f, 200, 100, 50);
            var light = new PointLight(GameType.RaC1, original, 0);
            byte[] serialized = light.ToByteArray();

            // Position and radius must round-trip exactly
            Assert.Equal(ReadFloat(original, 0x00), ReadFloat(serialized, 0x00));
            Assert.Equal(ReadFloat(original, 0x04), ReadFloat(serialized, 0x04));
            Assert.Equal(ReadFloat(original, 0x08), ReadFloat(serialized, 0x08));
            Assert.Equal(ReadFloat(original, 0x0C), ReadFloat(serialized, 0x0C));
            Assert.Equal(original[0x10], serialized[0x10]);
            Assert.Equal(original[0x11], serialized[0x11]);
            Assert.Equal(original[0x12], serialized[0x12]);
        }
    }

    public class GameCameraTests
    {
        private static byte[] BuildBlock(int index, int id, float x, float y, float z, float rx, float ry, float rz, int pvarIdx)
        {
            byte[] block = new byte[(index + 1) * GameCamera.ELEMENTSIZE];
            int offset = index * GameCamera.ELEMENTSIZE;
            WriteInt(block, offset + 0x00, id);
            WriteFloat(block, offset + 0x04, x);
            WriteFloat(block, offset + 0x08, y);
            WriteFloat(block, offset + 0x0C, z);
            WriteFloat(block, offset + 0x10, rx);
            WriteFloat(block, offset + 0x14, ry);
            WriteFloat(block, offset + 0x18, rz);
            WriteInt(block, offset + 0x1C, pvarIdx);
            return block;
        }

        [Fact]
        public void Constructor_ParsesIdAndPvarIndex()
        {
            byte[] block = BuildBlock(0, 99, 0f, 0f, 0f, 0f, 0f, 0f, 7);
            var cam = new GameCamera(block, 0);

            Assert.Equal(99, cam.id);
            Assert.Equal(7, cam.pvarIndex);
        }

        [Fact]
        public void Constructor_ParsesPosition()
        {
            byte[] block = BuildBlock(0, 0, 1f, 2f, 3f, 0f, 0f, 0f, 0);
            var cam = new GameCamera(block, 0);

            Assert.Equal(1f, cam.position.X, 5);
            Assert.Equal(2f, cam.position.Y, 5);
            Assert.Equal(3f, cam.position.Z, 5);
        }

        [Fact]
        public void ToByteArray_PreservesIdPvarAndPosition()
        {
            byte[] original = BuildBlock(0, 5, 1f, 2f, 3f, 0f, 0f, 0f, 11);
            var cam = new GameCamera(original, 0);
            byte[] serialized = cam.ToByteArray();

            Assert.Equal(5, ReadInt(serialized, 0x00));
            Assert.Equal(11, ReadInt(serialized, 0x1C));
            Assert.Equal(ReadFloat(original, 0x04), ReadFloat(serialized, 0x04));
            Assert.Equal(ReadFloat(original, 0x08), ReadFloat(serialized, 0x08));
            Assert.Equal(ReadFloat(original, 0x0C), ReadFloat(serialized, 0x0C));
        }
    }
}
