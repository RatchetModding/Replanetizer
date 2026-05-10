// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using Xunit;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.Models
{
    // Minimal concrete subclass of the abstract Model base class.
    internal class TestModel : Model
    {
        public TestModel(float[] vertices, ushort[] indices, uint[] boneWeights, uint[] boneIds)
        {
            vertexBuffer = vertices;
            indexBuffer = indices;
            vertexBoneWeights = boneWeights;
            vertexBoneIds = boneIds;
        }
    }

    public class ModelTests
    {
        // Build a float[vertexCount * 8] where each vertex is
        // [x, y, z, nx, ny, nz, u, v] with predictable values.
        private static float[] BuildVertexBuffer(int vertexCount)
        {
            float[] buf = new float[vertexCount * 8];
            for (int i = 0; i < vertexCount; i++)
            {
                buf[i * 8 + 0] = i * 1.0f;       // x
                buf[i * 8 + 1] = i * 2.0f;       // y
                buf[i * 8 + 2] = i * 3.0f;       // z
                buf[i * 8 + 3] = i * 0.1f;       // nx
                buf[i * 8 + 4] = i * 0.2f;       // ny
                buf[i * 8 + 5] = i * 0.3f;       // nz
                buf[i * 8 + 6] = i * 0.01f;      // u
                buf[i * 8 + 7] = i * 0.02f;      // v
            }
            return buf;
        }

        [Fact]
        public void SerializeVertices_RoundTrip_VertexData()
        {
            int vertexCount = 4;
            float[] vertices = BuildVertexBuffer(vertexCount);
            uint[] weights = new uint[] { 0x01020304u, 0u, 0xFFFFFFFFu, 0x80808080u };
            uint[] ids    = new uint[] { 0u, 1u, 2u, 3u };

            var model = new TestModel(vertices, new ushort[0], weights, ids);
            byte[] serialized = model.SerializeVertices();

            Assert.Equal(vertexCount * 0x28, serialized.Length);

            // Verify a sample vertex
            Assert.Equal(vertices[0], ReadFloat(serialized, 0x00));
            Assert.Equal(vertices[1], ReadFloat(serialized, 0x04));
            Assert.Equal(vertices[6], ReadFloat(serialized, 0x18));
            Assert.Equal(weights[0], ReadUint(serialized, 0x20));
            Assert.Equal(ids[0],     ReadUint(serialized, 0x24));
        }

        [Fact]
        public void SerializeUVs_RoundTrip_UVsMatch()
        {
            int vertexCount = 3;
            float[] vertices = BuildVertexBuffer(vertexCount);

            var model = new TestModel(vertices, new ushort[0], new uint[vertexCount], new uint[vertexCount]);
            byte[] uvBytes = model.SerializeUVs();

            Assert.Equal(vertexCount * 0x08, uvBytes.Length);

            for (int i = 0; i < vertexCount; i++)
            {
                float expectedU = vertices[i * 8 + 6];
                float expectedV = vertices[i * 8 + 7];
                Assert.Equal(expectedU, ReadFloat(uvBytes, i * 0x08 + 0x00));
                Assert.Equal(expectedV, ReadFloat(uvBytes, i * 0x08 + 0x04));
            }
        }

        [Fact]
        public void SerializeTieVertices_RoundTrip_PositionAndNormals()
        {
            int vertexCount = 2;
            float[] vertices = BuildVertexBuffer(vertexCount);

            var model = new TestModel(vertices, new ushort[0], new uint[vertexCount], new uint[vertexCount]);
            byte[] tieBytes = model.SerializeTieVertices();

            Assert.Equal(vertexCount * 0x18, tieBytes.Length);

            // Check XYZ and normals for first vertex
            Assert.Equal(vertices[0], ReadFloat(tieBytes, 0x00)); // x
            Assert.Equal(vertices[1], ReadFloat(tieBytes, 0x04)); // y
            Assert.Equal(vertices[2], ReadFloat(tieBytes, 0x08)); // z
            Assert.Equal(vertices[3], ReadFloat(tieBytes, 0x0C)); // nx
            Assert.Equal(vertices[4], ReadFloat(tieBytes, 0x10)); // ny
            Assert.Equal(vertices[5], ReadFloat(tieBytes, 0x14)); // nz
        }
    }
}
