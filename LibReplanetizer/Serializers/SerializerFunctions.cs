// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Serializers
{
    public static class SerializerFunctions
    {
        public static int SeekWriteForced(FileStream fs, byte[] bytes, int alignment = 0x10)
        {
            SeekPast(fs, alignment);
            int pos = (int) fs.Position;
            fs.Write(bytes, 0, bytes.Length);
            return pos;
        }


        public static int SeekWrite(FileStream fs, byte[]? bytes, int alignment = 0x10)
        {
            if (bytes == null || bytes.Length == 0) return 0;

            return SeekWriteForced(fs, bytes, alignment);
        }

        public static int SeekReserve(FileStream fs, int length, int alignment = 0x10)
        {
            if (length == 0)
                return 0;

            SeekPast(fs, alignment);
            int pos = (int) fs.Position;
            fs.Seek(length, SeekOrigin.Current);
            return pos;
        }

        public static int SeekPast(FileStream fs, int alignment)
        {
            long alignmentError = fs.Position % alignment;
            if (alignmentError != 0)
            {
                fs.Seek(alignment - alignmentError, SeekOrigin.Current);
            }
            return (int) fs.Position;
        }

        public static void WriteBytesAtOffset(FileStream fs, byte[]? bytes, int offset)
        {
            if (bytes == null || bytes.Length == 0) return;

            long previousPosition = fs.Position;

            fs.Seek(offset, SeekOrigin.Begin);
            fs.Write(bytes, 0, bytes.Length);
            fs.Seek(previousPosition, SeekOrigin.Begin);
        }

        public static int WriteTfrags(FileStream fs, Terrain terrain, GameType game)
        {
            List<TerrainFragment> tFrags = terrain.fragments;

            int headerSize = (game == GameType.DL) ? 0x70 : 0x60;

            int textureBytesLength = 0;
            for (int i = 0; i < tFrags.Count; i++)
            {
                TerrainModel? mod = (TerrainModel?) tFrags[i].model;

                if (mod == null) continue;

                foreach (TextureConfig texConf in mod.textureConfig)
                    textureBytesLength += 0x10;
            }

            int headerOffset = SeekReserve(fs, headerSize);
            int tfragHeadsOffset = SeekReserve(fs, 0x30 * tFrags.Count);
            int textureBytesOffset = SeekReserve(fs, textureBytesLength);

            byte[] headerBytes = new byte[headerSize];
            byte[] tfragHeads = new byte[0x30 * tFrags.Count];
            byte[] textureBytes = new byte[textureBytesLength];

            List<List<byte>> vertBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> rgbaBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> uvBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> indexBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };

            ushort chunk = 0;
            int textureBytesPointer = 0;

            for (int i = 0; i < tFrags.Count; i++)
            {
                TerrainModel? mod = (TerrainModel?) tFrags[i].model;

                if (mod == null) continue;

                int offset = i * 0x30;
                tFrags[i].ToByteArray().CopyTo(tfragHeads, offset);

                WriteInt(tfragHeads, offset + 0x10, textureBytesOffset + textureBytesPointer);
                WriteInt(tfragHeads, offset + 0x14, mod.textureConfig.Count);

                byte[] modelVertBytes = mod.SerializeVerts(tFrags[i].modelMatrix);
                if (((vertBytes[chunk].Count + modelVertBytes.Length) / 0x1c) > 0xffff)
                    chunk++;

                WriteUshort(tfragHeads, offset + 0x18, (ushort) (vertBytes[chunk].Count / 0x1c));
                WriteUshort(tfragHeads, offset + 0x1a, (ushort) (mod.vertexBuffer.Length / 8));

                WriteUshort(tfragHeads, offset + 0x22, chunk);

                foreach (TextureConfig texConf in mod.textureConfig)
                {
                    byte[] texBytes = new byte[0x10];
                    WriteInt(texBytes, 0x00, texConf.id);
                    WriteInt(texBytes, 0x04, texConf.start + indexBytes[chunk].Count / 2);
                    WriteInt(texBytes, 0x08, texConf.size);
                    WriteInt(texBytes, 0x0C, texConf.mode);
                    texBytes.CopyTo(textureBytes, textureBytesPointer);
                    textureBytesPointer += 0x10;
                }

                indexBytes[chunk].AddRange(mod.GetFaceBytes((ushort) (vertBytes[chunk].Count / 0x1C)));
                vertBytes[chunk].AddRange(modelVertBytes);
                rgbaBytes[chunk].AddRange(mod.rgbas);
                uvBytes[chunk].AddRange(mod.SerializeUVs());
            }

            WriteInt(headerBytes, 0x00, tfragHeadsOffset);
            WriteUshort(headerBytes, 0x04, terrain.levelNumber);
            WriteUshort(headerBytes, 0x06, (ushort) tFrags.Count);

            int[] vertOffsets = { 0, 0, 0, 0 };
            int[] rgbaOffsets = { 0, 0, 0, 0 };
            int[] uvOffsets = { 0, 0, 0, 0 };
            int[] indexOffsets = { 0, 0, 0, 0 };
            int[] unkOffsets = { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (i > 0 && vertBytes[i].Count == 0 && rgbaBytes[i].Count == 0 && uvBytes[i].Count == 0 && indexBytes[i].Count == 0)
                    continue;

                vertOffsets[i] = SeekWrite(fs, vertBytes[i].ToArray());
                rgbaOffsets[i] = SeekWrite(fs, rgbaBytes[i].ToArray());
                uvOffsets[i] = SeekWrite(fs, uvBytes[i].ToArray());
                indexOffsets[i] = SeekWrite(fs, indexBytes[i].ToArray());
                unkOffsets[i] = 0; // TODO: SeekWrite(fs, unkBytes[i].ToArray());
            }

            for (int i = 0; i < 4; i++)
            {
                WriteInt(headerBytes, 0x08 + i * 4, vertOffsets[i]);
                WriteInt(headerBytes, 0x18 + i * 4, rgbaOffsets[i]);
                WriteInt(headerBytes, 0x28 + i * 4, uvOffsets[i]);
                WriteInt(headerBytes, 0x38 + i * 4, indexOffsets[i]);
                if (game == GameType.DL)
                    WriteInt(headerBytes, 0x48 + i * 4, unkOffsets[i]);
            }

            WriteBytesAtOffset(fs, headerBytes, headerOffset);
            WriteBytesAtOffset(fs, tfragHeads, tfragHeadsOffset);
            WriteBytesAtOffset(fs, textureBytes, textureBytesOffset);

            return headerOffset;
        }
    }
}
