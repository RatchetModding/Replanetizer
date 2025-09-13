// Copyright (C) 2018-2025, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.ComponentModel;
using LibReplanetizer.LevelObjects;
using System.Collections.Generic;
using System.IO;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models
{
    /*
        Model with metal configs
    */

    public abstract class MetalModel : Model
    {
        public float[] metalVertexBuffer = { };
        public ushort[] metalIndexBuffer = { };

        [Category("Attributes"), DisplayName("Metal Bone Weights")]
        public uint[] metalVertexBoneWeights { get; set; } = new uint[0];
        [Category("Attributes"), DisplayName("Metal Vertex to Bone Map")]
        public uint[] metalVertexBoneIds { get; set; } = new uint[0];

        [Category("Attributes"), DisplayName("Metal Vertex Count")]
        public int metalVertexCount
        {
            get
            {
                return metalVertexBuffer.Length / vertexStride;
            }
        }

        [Category("Attributes"), DisplayName("Texture Configurations")]
        public List<TextureConfig> metalTextureConfig { get; set; } = new List<TextureConfig>();

        protected int GetMetalFaceCount()
        {
            int faceCount = 0;
            if (metalTextureConfig != null)
            {
                foreach (TextureConfig tex in metalTextureConfig)
                {
                    faceCount += tex.size;
                }
            }
            return faceCount;
        }

        public byte[] SerializeMetalVertices()
        {
            int elemSize = 0x28;
            byte[] outBytes = new byte[(metalVertexBuffer.Length / 8) * elemSize];

            for (int i = 0; i < metalVertexBuffer.Length / 8; i++)
            {
                WriteFloat(outBytes, (i * elemSize) + 0x00, metalVertexBuffer[(i * 8) + 0]);
                WriteFloat(outBytes, (i * elemSize) + 0x04, metalVertexBuffer[(i * 8) + 1]);
                WriteFloat(outBytes, (i * elemSize) + 0x08, metalVertexBuffer[(i * 8) + 2]);
                WriteFloat(outBytes, (i * elemSize) + 0x0C, metalVertexBuffer[(i * 8) + 3]);
                WriteFloat(outBytes, (i * elemSize) + 0x10, metalVertexBuffer[(i * 8) + 4]);
                WriteFloat(outBytes, (i * elemSize) + 0x14, metalVertexBuffer[(i * 8) + 5]);
                WriteFloat(outBytes, (i * elemSize) + 0x18, metalVertexBuffer[(i * 8) + 6]);
                WriteFloat(outBytes, (i * elemSize) + 0x1C, metalVertexBuffer[(i * 8) + 7]);
                WriteUint(outBytes, (i * elemSize) + 0x20, metalVertexBoneWeights[i]);
                WriteUint(outBytes, (i * elemSize) + 0x24, metalVertexBoneIds[i]);
            }

            return outBytes;
        }

        public byte[] SerializeMetalIndices(ushort offset = 0)
        {
            byte[] indexBytes = new byte[metalIndexBuffer.Length * sizeof(ushort)];
            for (int i = 0; i < metalIndexBuffer.Length; i++)
            {
                WriteUshort(indexBytes, i * sizeof(ushort), (ushort) (metalIndexBuffer[i] + offset));
            }
            return indexBytes;
        }
    }
}
