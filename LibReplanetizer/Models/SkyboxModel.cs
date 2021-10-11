﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models
{
    public class SkyboxModel : Model
    {
        public const int VERTELEMSIZE = 0x18;

        public GameType game;

        //Unhandled offsets for serialization
        int off_00;
        short off_04;
        int off_08;
        int off_0C;

        List<List<TextureConfig>> textureConfigs;

        public SkyboxModel(FileStream fs, GameType game, int offset)
        {
            if (offset == 0) return;

            int headSize = (game.num == 4) ? 0x20 : 0x1C;

            size = 1.0f;
            byte[] skyBlockHead = ReadBlock(fs, offset, headSize);

            off_00 = ReadInt(skyBlockHead, 0x00);
            off_04 = ReadShort(skyBlockHead, 0x04);
            off_08 = ReadInt(skyBlockHead, 0x08);
            off_0C = ReadInt(skyBlockHead, 0x0C);

            short faceGroupCount = ReadShort(skyBlockHead, 0x06);
            int vertOffset = ReadInt(skyBlockHead, headSize - 0x8);
            int faceOffset = ReadInt(skyBlockHead, headSize - 0x4);

            int vertexCount = (int) ((faceOffset - vertOffset) / VERTELEMSIZE);


            textureConfigs = new List<List<TextureConfig>>();
            textureConfig = new List<TextureConfig>();
            byte[] faceGroupBlock = ReadBlock(fs, offset + headSize, faceGroupCount * 4);
            for (int i = 0; i < faceGroupCount; i++)
            {
                int faceGroupOffset = ReadInt(faceGroupBlock, (i * 4));
                short texCount = ReadShort(ReadBlock(fs, faceGroupOffset + 0x02, 0x02), 0);

                var texconfigs = new List<TextureConfig>(GetTextureConfigs(fs, faceGroupOffset + 0x10, texCount, 0x10));
                textureConfig.AddRange(texconfigs);
                textureConfigs.Add(texconfigs);
            }

            int faceCount = GetFaceCount();
            vertexBuffer = GetVerticesUV(fs, vertOffset, vertexCount, VERTELEMSIZE);

            indexBuffer = GetIndices(fs, faceOffset, faceCount);

            this.game = game;
        }

        public byte[] Serialize(int startOffset)
        {
            int headSize = (game.num == 4) ? 0x20 : 0x1C;

            int faceStart = GetLength(headSize + textureConfigs.Count * 4);
            int faceLength = textureConfigs.Count * 0x10;
            foreach (List<TextureConfig> conf in textureConfigs)
            {
                faceLength += conf.Count * 0x10;
            }

            int headLength = faceStart + faceLength;

            var headBytes = new byte[headLength];
            WriteInt(headBytes, 0x00, off_00);
            WriteShort(headBytes, 0x04, off_04);
            WriteShort(headBytes, 0x06, (short) textureConfigs.Count);
            WriteInt(headBytes, 0x08, off_08);
            WriteInt(headBytes, 0x0C, off_0C);

            int offs = faceStart;
            int[] headList = new int[textureConfigs.Count];
            for (int i = 0; i < textureConfigs.Count; i++)
            {
                headList[i] = startOffset + offs;
                if (textureConfigs[i][0].ID == 0)
                {
                    WriteShort(headBytes, offs + 0x00, 1);
                }

                WriteShort(headBytes, offs + 0x02, (short) textureConfigs[i].Count);
                offs += 0x10;
                foreach (var conf in textureConfigs[i])
                {
                    WriteInt(headBytes, offs, conf.ID);
                    offs += 4;
                    WriteInt(headBytes, offs, conf.start);
                    offs += 4;
                    WriteInt(headBytes, offs, conf.size);
                    offs += 8;
                }
            }
            for (int i = 0; i < headList.Length; i++)
            {
                WriteInt(headBytes, headSize + i * 4, headList[i]);
            }


            int vertOffset = GetLength(offs);
            byte[] vertexBytes = GetVertexBytesUV(vertexBuffer);

            int faceOffset = GetLength(vertOffset + vertexBytes.Length);
            byte[] faceBytes = GetFaceBytes();

            int endOffset = GetLength(faceOffset + faceBytes.Length);

            byte[] returnBytes = new byte[endOffset];
            headBytes.CopyTo(returnBytes, 0);
            vertexBytes.CopyTo(returnBytes, vertOffset);
            faceBytes.CopyTo(returnBytes, faceOffset);

            WriteInt(returnBytes, headSize - 0x08, startOffset + vertOffset);
            WriteInt(returnBytes, headSize - 0x04, startOffset + faceOffset);

            return returnBytes;
        }
    }
}
