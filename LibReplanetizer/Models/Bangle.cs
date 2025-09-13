// Copyright (C) 2018-2025, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using NLog;
using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models
{
    public class Bangle : Model
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        const int VERTELEMENTSIZE = 0x28;
        const int TEXTUREELEMENTSIZE = 0x10;
        const int MESHHEADERSIZE = 0x20;

        [Category("Unknowns"), DisplayName("Other Buffer")]
        public List<byte> otherBuffer { get; set; } = new List<byte>();

        [Category("Unknowns"), DisplayName("Other Texture Configurations")]
        public List<TextureConfig> otherTextureConfigs { get; set; } = new List<TextureConfig>();

        [Category("Unknowns"), DisplayName("Other Index Buffer")]
        public List<ushort> otherIndexBuffer { get; set; } = new List<ushort>();

        public Bangle(FileStream fs, int baseOffset, int headerOffset)
        {
            byte[] meshHeader = ReadBlock(fs, baseOffset + headerOffset, MESHHEADERSIZE);

            int texCount = ReadInt(meshHeader, 0x00);
            int otherCount = ReadInt(meshHeader, 0x04);
            int texBlockPointer = baseOffset + ReadInt(meshHeader, 0x08);
            int otherBlockPointer = baseOffset + ReadInt(meshHeader, 0x0C);
            int vertPointer = baseOffset + ReadInt(meshHeader, 0x10);
            int indexPointer = baseOffset + ReadInt(meshHeader, 0x14);
            ushort vertexCount = ReadUshort(meshHeader, 0x18);
            ushort otherVertCount = ReadUshort(meshHeader, 0x1a);

            int otherPointer = vertPointer + vertexCount * 0x28;

            int faceCount = 0;

            //Texture configuration
            if (texBlockPointer > 0)
            {
                textureConfig = GetTextureConfigs(fs, texBlockPointer, texCount, TEXTUREELEMENTSIZE);
                faceCount = GetFaceCount();
            }

            if (vertPointer > 0 && vertexCount > 0)
            {
                //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ, U, V, reserved, reserved]
                vertexBuffer = GetVertices(fs, vertPointer, vertexCount, VERTELEMENTSIZE);
            }

            if (indexPointer > 0 && faceCount > 0)
            {
                //Index buffer
                indexBuffer = GetIndices(fs, indexPointer, faceCount);
            }
            if (otherPointer > 0)
            {
                otherBuffer.AddRange(ReadBlockNopad(fs, otherPointer, otherVertCount * 0x20));
                otherTextureConfigs = GetTextureConfigs(fs, otherBlockPointer, otherCount, 0x10);
                int otherfaceCount = 0;
                foreach (TextureConfig tex in otherTextureConfigs)
                {
                    otherfaceCount += tex.size;
                }
                otherIndexBuffer.AddRange(GetIndices(fs, indexPointer + faceCount * sizeof(ushort), otherfaceCount));
            }
        }

        public byte[] Serialize()
        {
            return new byte[0];
        }
    }
}
