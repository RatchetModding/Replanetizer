// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Serializers.SerializerFunctions;

namespace LibReplanetizer.Models
{
    public class SkyShell
    {
        [DisplayName("Clusters")]
        public short cluster_cnt { get; set; }
        [DisplayName("Textures")]
        public short tex_cnt { get; set; }

        // only in uya + dl. UYA has its fields zeroed.
        [DisplayName("Rotation X")]
        public short rot_x { get; set; }
        [DisplayName("Rotation Y")]
        public short rot_y { get; set; }
        [DisplayName("Rotation Z")]
        public short rot_z { get; set; }
        [DisplayName("Velocity X")]
        public short vel_x { get; set; }
        [DisplayName("Velocity y")]
        public short vel_y { get; set; }
        [DisplayName("Velocity Z")]
        public short vel_z { get; set; }
    }
    public class SkyboxModel : Model
    {
        [Category(""), DisplayName("Sky Shells")]
        public List<SkyShell> skyShells { get; set; } = new List<SkyShell>();
        public const int VERTELEMSIZE = 0x18;

        public GameType game;

        [Category("Sky Header"), DisplayName("Sky Shell Count")]
        public short shellCount { get; set; }

        // Not sure if this is used, has values in rac1 and 2.
        [Category("Sky Header"), DisplayName("Sprite Count")]
        public short spriteCnt { get; set; }

        [Category("Sky Header"), DisplayName("FX Count")]
        public short fxCnt { get; set; }

        // Seemingly always sero
        public short unusedclearScreen;
        public short unusedmaxSpriteCnt;
        public short unusedtexCnt;
        public short unusedtexDefs;
        public Rgba32 unusedsomeColor;

        public List<List<TextureConfig>> textureConfigs = new List<List<TextureConfig>>();

        public SkyboxModel(FileStream fs, GameType game, int offset)
        {
            this.game = game;

            if (offset == 0) return;

            int headSize = (game == GameType.DL) ? 0x20 : 0x1C;

            // skybox model has no normals and thus the vertex buffer has a different layout
            this.vertexStride = 6;

            size = 1.0f;
            byte[] skyBlockHead = ReadBlock(fs, offset, headSize);

            byte red = skyBlockHead[0x00];
            byte green = skyBlockHead[0x01];
            byte blue = skyBlockHead[0x02];
            byte alpha = skyBlockHead[0x03];

            unusedclearScreen = ReadShort(skyBlockHead, 0x04);
            shellCount = ReadShort(skyBlockHead, 0x06);
            spriteCnt = ReadShort(skyBlockHead, 0x08);
            unusedmaxSpriteCnt = ReadShort(skyBlockHead, 0x0A);
            unusedtexCnt = ReadShort(skyBlockHead, 0x0C);
            fxCnt = ReadShort(skyBlockHead, 0x0e);
            unusedtexDefs = ReadShort(skyBlockHead, 0x10);

            int vertOffset = ReadInt(skyBlockHead, headSize - 0x8);  // DL: 0x18, RC123: 0x14 - fx_list
            int faceOffset = ReadInt(skyBlockHead, headSize - 0x4);  // DL: 0x1c, RC123: 0x18 - sprites

            int vertexCount = (int) ((faceOffset - vertOffset) / VERTELEMSIZE);

            textureConfigs = new List<List<TextureConfig>>();
            textureConfig = new List<TextureConfig>();
            byte[] skyShellsBlock = ReadBlock(fs, offset + headSize, shellCount * 4);
            for (int i = 0; i < shellCount; i++)
            {
                int shelloffset = ReadInt(skyShellsBlock, (i * 4));

                int shellSize;
                if (game == GameType.RaC1 || game == GameType.RaC2) shellSize = 0x04;
                else shellSize = 0x10;

                byte[] shellBlock = ReadBlock(fs, shelloffset, shellSize);

                SkyShell shell = new SkyShell();
                shell.cluster_cnt = ReadShort(shellBlock, 0x00);
                shell.tex_cnt = ReadShort(shellBlock, 0x02);

                if (shellSize == 0x10) // uya and dl
                {
                    shell.rot_x = ReadShort(shellBlock, 0x04);
                    shell.rot_y = ReadShort(shellBlock, 0x06);
                    shell.rot_z = ReadShort(shellBlock, 0x08);
                    shell.vel_x = ReadShort(shellBlock, 0x0A);
                    shell.vel_y = ReadShort(shellBlock, 0x0C);
                    shell.vel_z = ReadShort(shellBlock, 0x0E);
                }

                skyShells.Add(shell);

                var texconfigs = new List<TextureConfig>(GetTextureConfigs(fs, shelloffset + 0x10, shell.tex_cnt, 0x10));
                textureConfig.AddRange(texconfigs);
                textureConfigs.Add(texconfigs);
            }

            int faceCount = GetFaceCount();
            vertexBuffer = GetVerticesSkybox(fs, vertOffset, vertexCount);

            indexBuffer = GetIndices(fs, faceOffset, faceCount);

            unusedsomeColor = Color.FromRgba(red, green, blue, alpha).ToPixel<Rgba32>();
        }

        public int WriteBytes(FileStream fs)
        {
            int headSize = (game == GameType.DL) ? 0x20 : 0x1C;

            int headerOffset = SeekReserve(fs, headSize);
            int textureConfigOffsetsOffset = SeekReserve(fs, textureConfigs.Count * 0x04, 0x01);

            int textureConfigBytesLength = textureConfigs.Count * 0x10;
            foreach (List<TextureConfig> conf in textureConfigs)
            {
                textureConfigBytesLength += conf.Count * 0x10;
            }

            int textureConfigOffset = SeekReserve(fs, textureConfigBytesLength);

            int offs = 0;
            byte[] textureConfigOffsetsBytes = new byte[textureConfigs.Count * 0x04];
            byte[] textureConfigBytes = new byte[textureConfigBytesLength];
            for (int i = 0; i < textureConfigs.Count; i++)
            {
                WriteInt(textureConfigOffsetsBytes, i * 0x04, textureConfigOffset + offs);

                if (i < skyShells.Count)
                {
                    SkyShell shell = skyShells[i];
                    WriteShort(textureConfigBytes, offs + 0x00, shell.cluster_cnt);

                    if (game == GameType.RaC3 || game == GameType.DL)
                    {
                        WriteShort(textureConfigBytes, offs + 0x04, shell.rot_x);
                        WriteShort(textureConfigBytes, offs + 0x06, shell.rot_y);
                        WriteShort(textureConfigBytes, offs + 0x08, shell.rot_z);
                        WriteShort(textureConfigBytes, offs + 0x0A, shell.vel_x);
                        WriteShort(textureConfigBytes, offs + 0x0C, shell.vel_y);
                        WriteShort(textureConfigBytes, offs + 0x0E, shell.vel_z);
                    }
                }

                if (textureConfigs[i].Count > 0 && textureConfigs[i][0].id == 0)
                {
                    WriteShort(textureConfigBytes, offs + 0x00, 1);
                }

                WriteShort(textureConfigBytes, offs + 0x02, skyShells[i].tex_cnt);
                offs += 0x10;
                foreach (TextureConfig conf in textureConfigs[i])
                {
                    WriteInt(textureConfigBytes, offs + 0x00, conf.id);
                    WriteInt(textureConfigBytes, offs + 0x04, conf.start);
                    WriteInt(textureConfigBytes, offs + 0x08, conf.size);
                    offs += 0x10;
                }
            }

            WriteBytesAtOffset(fs, textureConfigOffsetsBytes, textureConfigOffsetsOffset);
            WriteBytesAtOffset(fs, textureConfigBytes, textureConfigOffset);

            int vertOffset = SeekWrite(fs, GetVertexBytesSkybox(vertexBuffer));
            int faceOffset = SeekWrite(fs, GetFaceBytes());

            byte[] headBytes = new byte[headSize];
            headBytes[0x00] = unusedsomeColor.R;
            headBytes[0x01] = unusedsomeColor.G;
            headBytes[0x02] = unusedsomeColor.B;
            headBytes[0x03] = unusedsomeColor.A;
            WriteShort(headBytes, 0x04, unusedclearScreen);
            WriteShort(headBytes, 0x06, shellCount);
            WriteShort(headBytes, 0x08, spriteCnt);
            WriteShort(headBytes, 0x0A, unusedmaxSpriteCnt);
            WriteShort(headBytes, 0x0C, unusedtexCnt);
            WriteShort(headBytes, 0x0E, fxCnt);
            WriteShort(headBytes, 0x10, unusedtexDefs);
            WriteInt(headBytes, headSize - 0x08, vertOffset);
            WriteInt(headBytes, headSize - 0x04, faceOffset);

            WriteBytesAtOffset(fs, headBytes, headerOffset);

            return headerOffset;
        }
    }
}
