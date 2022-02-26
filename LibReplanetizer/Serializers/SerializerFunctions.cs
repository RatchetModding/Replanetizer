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
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Serializers
{
    public static class SerializerFunctions
    {
        public static int SeekWrite(FileStream fs, byte[] bytes)
        {
            if (bytes != null)
            {
                SeekPast(fs);
                int pos = (int) fs.Position;
                fs.Write(bytes, 0, bytes.Length);
                return pos;
            }
            else return 0;
        }

        public static int SeekWrite4(FileStream fs, byte[] bytes)
        {
            if (bytes != null)
            {
                SeekPast4(fs);
                int pos = (int) fs.Position;
                fs.Write(bytes, 0, bytes.Length);
                return pos;
            }
            else return 0;
        }

        public static void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(2, SeekOrigin.Current);
            }
        }

        public static void SeekPast4(FileStream fs)
        {
            while (fs.Position % 0x4 != 0)
            {
                fs.Seek(2, SeekOrigin.Current);
            }
        }

        public static byte[] WriteTfrags(Terrain terrain, int fileOffset, GameType game)
        {
            List<TerrainFragment> tFrags = terrain.fragments;

            List<List<byte>> vertBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> rgbaBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> uvBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> indexBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };

            List<byte> textureBytes = new List<byte>();

            byte[] tfragHeads = new byte[0x30 * tFrags.Count];

            int headerSize = (game.num == 4) ? 0x70 : 0x60;

            ushort chunk = 0;

            for (int i = 0; i < tFrags.Count; i++)
            {
                TerrainModel? mod = (TerrainModel?) (tFrags[i].model);

                if (mod == null) continue;

                int offset = i * 0x30;
                tFrags[i].ToByteArray().CopyTo(tfragHeads, offset);

                WriteInt(tfragHeads, offset + 0x10, fileOffset + headerSize + tfragHeads.Length + textureBytes.Count);
                WriteInt(tfragHeads, offset + 0x14, mod.textureConfig.Count);

                byte[] modelVertBytes = mod.SerializeVerts();
                if (((vertBytes[chunk].Count + modelVertBytes.Length) / 0x1c) > 0xffff)
                {
                    chunk++;
                }

                WriteUshort(tfragHeads, offset + 0x18, (ushort) (vertBytes[chunk].Count / 0x1c));
                WriteUshort(tfragHeads, offset + 0x1a, (ushort) (mod.vertexBuffer.Length / 8));

                WriteUshort(tfragHeads, offset + 0x22, chunk);

                foreach (var texConf in mod.textureConfig)
                {
                    byte[] texBytes = new byte[0x10];
                    WriteInt(texBytes, 0x00, texConf.id);
                    WriteInt(texBytes, 0x04, texConf.start + indexBytes[chunk].Count / 2);
                    WriteInt(texBytes, 0x08, texConf.size);
                    WriteInt(texBytes, 0x0C, texConf.mode);
                    textureBytes.AddRange(texBytes);
                }

                indexBytes[chunk].AddRange(mod.GetFaceBytes((ushort) (vertBytes[chunk].Count / 0x1C)));
                vertBytes[chunk].AddRange(modelVertBytes);
                rgbaBytes[chunk].AddRange(mod.rgbas);
                uvBytes[chunk].AddRange(mod.SerializeUVs());

            }

            List<byte> outBytes = new List<byte>();

            byte[] head = new byte[headerSize];
            WriteInt(head, 0, fileOffset + headerSize);
            WriteUshort(head, 0x4, terrain.levelNumber);
            WriteUshort(head, 0x6, (ushort) tFrags.Count);

            outBytes.AddRange(head);
            outBytes.AddRange(tfragHeads);
            outBytes.AddRange(textureBytes);

            int[] vertOffsets = { 0, 0, 0, 0 };
            int[] rgbaOffsets = { 0, 0, 0, 0 };
            int[] uvOffsets = { 0, 0, 0, 0 };
            int[] indexOffsets = { 0, 0, 0, 0 };
            int[] unkOffsets = { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (vertBytes[i].Count == 0)
                {
                    continue;
                }
                Pad(outBytes);
                vertOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(vertBytes[i]);
                Pad(outBytes);
                rgbaOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(rgbaBytes[i]);
                Pad(outBytes);
                uvOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(uvBytes[i]);
                Pad(outBytes);
                indexOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(indexBytes[i]);
                Pad(outBytes);
                unkOffsets[i] = fileOffset + outBytes.Count;
            }


            byte[] outByteArr = outBytes.ToArray();

            for (int i = 0; i < 4; i++)
            {
                WriteInt(outByteArr, 0x08 + i * 4, vertOffsets[i]);
                WriteInt(outByteArr, 0x18 + i * 4, rgbaOffsets[i]);
                WriteInt(outByteArr, 0x28 + i * 4, uvOffsets[i]);
                WriteInt(outByteArr, 0x38 + i * 4, indexOffsets[i]);
                if (game.num == 4)
                    WriteInt(outByteArr, 0x48 + i * 4, unkOffsets[i]);
            }

            return outByteArr;
        }
    }
}
