// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using NLog.Time;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Serializers.SerializerFunctions;

namespace LibReplanetizer.Serializers
{
    public class EngineSerializer
    {
        private string? enginePath;

        public void Save(Level level, string directory)
        {
            enginePath = Path.Join(directory, "engine.ps3");
            FileStream fs = ReplanetizerFileStream.Open(enginePath, FileMode.Create, FileAccess.Write);

            switch (level.game.num)
            {
                case 1:
                    SaveRC1(level, fs);
                    break;
                case 2:
                case 3:
                    SaveRC23(level, fs);
                    break;
                case 4:
                    SaveRC4(level, fs);
                    break;
            }

            fs.Close();
        }

        private void SaveRC1(Level level, FileStream fs)
        {
            fs.Seek(0x90, SeekOrigin.Begin);

            EngineHeader engineHeader = new EngineHeader()
            {
                game = level.game,
                uiElementPointer = SeekWrite(fs, WriteUiElements(level.uiElements, (int) fs.Position)),
                skyboxPointer = level.skybox.WriteBytes(fs),
                terrainPointer = WriteTfrags(fs, level.terrainEngine, level.game),
                mobyOcclusionPointer = WriteMobyOcclusion(fs, level.mobyOcclusion),
                unk1Pointer = SeekWrite(fs, level.unk1),
                unk2Pointer = SeekWrite(fs, level.unk2),
                precipitationMapPointer = WritePrecipicationMap(fs, level.precipitationMap),
                collisionPointer = SeekWrite(fs, level.collisionEngine.Serialize()),
                mobyModelPointer = WriteMobies(fs, level.mobyModels),
                playerAnimationPointer = WritePlayerAnimations(fs, level.playerAnimations),
                gadgetPointer = WriteWeapons(fs, level.gadgetModels),
                tieModelPointer = WriteTieModels(fs, level.tieModels),
                tiePointer = WriteTies(fs, level.ties, 0x80),
                shrubModelPointer = SeekWrite(fs, WriteShrubModels(level.shrubModels, (int) fs.Position)),
                shrubPointer = SeekWrite(fs, WriteShrubs(level.shrubs)),
                textureConfigMenuPointer = SeekWrite(fs, WriteTextureConfigMenus(level.textureConfigMenus)),
                texture2dPointer = SeekWrite(fs, level.billboardBytes),
                soundConfigPointer = SeekWrite(fs, level.soundConfigBytes),
                lightPointer = SeekWrite(fs, WriteLights(level.lights)),
                lightConfigPointer = SeekWrite(fs, WriteLightConfig(level.lightConfig)),
                texturePointer = SeekWrite(fs, WriteTextures(level.textures)),
                // Counts
                tieModelCount = level.tieModels.Count,
                tieCount = level.ties.Count,
                shrubModelCount = level.shrubModels.Count,
                shrubCount = level.shrubs.Count,
                gadgetCount = level.gadgetModels.Count,
                textureCount = level.textures.Count,
                lightCount = level.lights.Count,
                textureConfigMenuCount = level.textureConfigMenus.Count,
            };

            // Seek to the beginning and write the header now that we have all the pointers
            byte[] head = engineHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        private void SaveRC23(Level level, FileStream fs)
        {
            fs.Seek(0x90, SeekOrigin.Begin);

            EngineHeader engineHeader = new EngineHeader
            {
                game = level.game,
                uiElementPointer = SeekWrite(fs, WriteUiElements(level.uiElements, (int) fs.Position)),
                terrainPointer = WriteTfrags(fs, level.terrainEngine, level.game),
                mobyOcclusionPointer = WriteMobyOcclusion(fs, level.mobyOcclusion),
                unk1Pointer = SeekWrite(fs, level.unk1),
                unk2Pointer = SeekWrite(fs, level.unk2),
                precipitationMapPointer = WritePrecipicationMap(fs, level.precipitationMap),
                collisionPointer = SeekWrite(fs, level.collisionEngine.Serialize()),
                tieModelPointer = WriteTieModels(fs, level.tieModels),
                tiePointer = WriteTies(fs, level.ties, 0x10),
                shrubModelPointer = SeekWriteForced(fs, WriteShrubModels(level.shrubModels, (int) fs.Position)),
                shrubPointer = SeekWriteForced(fs, WriteShrubs(level.shrubs)),
                textureConfigMenuPointer = SeekWrite(fs, WriteTextureConfigMenus(level.textureConfigMenus)),
                texture2dPointer = SeekWrite(fs, level.billboardBytes),
                mobyModelPointer = WriteMobies(fs, level.mobyModels),
                soundConfigPointer = SeekWrite(fs, level.soundConfigBytes),
                playerAnimationPointer = WritePlayerAnimations(fs, level.playerAnimations),
                skyboxPointer = level.skybox.WriteBytes(fs),
                lightPointer = SeekWrite(fs, WriteLights(level.lights)),
                lightConfigPointer = SeekWrite(fs, WriteLightConfig(level.lightConfig)),
                texturePointer = SeekWrite(fs, WriteTextures(level.textures)),
                // Counts
                tieModelCount = level.tieModels.Count,
                tieCount = level.ties.Count,
                shrubModelCount = level.shrubModels.Count,
                shrubCount = level.shrubs.Count,
                textureCount = level.textures.Count,
                lightCount = level.lights.Count,
                textureConfigMenuCount = level.textureConfigMenus.Count,
            };

            SeekWrite(fs, new byte[2304], 0x01);

            // Seek to the beginning and write the header now that we have all the pointers
            byte[] head = engineHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        private void SaveRC4(Level level, FileStream fs)
        {
            fs.Seek(0xA0, SeekOrigin.Begin);

            EngineHeader engineHeader = new EngineHeader
            {
                game = level.game,
                uiElementPointer = SeekWrite(fs, WriteUiElements(level.uiElements, (int) fs.Position)),
                unk8Pointer = SeekWrite(fs, level.unk8),
                mobyModelPointer = WriteMobies(fs, level.mobyModels),
                soundConfigPointer = SeekWrite(fs, level.soundConfigBytes),
                unk9Pointer = SeekWrite(fs, level.unk9),
                terrainPointer = WriteTfrags(fs, level.terrainEngine, level.game),
                mobyOcclusionPointer = WriteMobyOcclusion(fs, level.mobyOcclusion),
                collisionPointer = SeekWrite(fs, level.collisionEngine.Serialize()),
                shrubModelPointer = SeekWrite(fs, WriteShrubModels(level.shrubModels, (int) fs.Position)),
                shrubPointer = SeekWrite(fs, WriteShrubs(level.shrubs)),
                unk5Pointer = SeekWrite(fs, level.unk5),
                tieModelPointer = WriteTieModels(fs, level.tieModels),
                tiePointer = WriteTies(fs, level.ties, 0x10),
                unk4Pointer = SeekWrite(fs, level.unk4),
                textureConfigMenuPointer = SeekWrite(fs, WriteTextureConfigMenus(level.textureConfigMenus)),
                texture2dPointer = SeekWrite(fs, level.billboardBytes),
                skyboxPointer = level.skybox.WriteBytes(fs),
                lightPointer = SeekWrite(fs, WriteLights(level.lights)),
                lightConfigPointer = SeekWrite(fs, WriteLightConfig(level.lightConfig)),
                precipitationMapPointer = WritePrecipicationMap(fs, level.precipitationMap),
                texturePointer = SeekWrite(fs, WriteTextures(level.textures)),
                // Counts
                tieModelCount = level.tieModels.Count,
                tieCount = level.ties.Count,
                shrubModelCount = level.shrubModels.Count,
                shrubCount = level.shrubs.Count,
                textureCount = level.textures.Count,
                lightCount = level.lights.Count,
                textureConfigMenuCount = level.textureConfigMenus.Count,
            };

            // Seek to the beginning and write the header now that we have all the pointers
            byte[] head = engineHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        private byte[] WriteLightConfig(LightConfig config)
        {
            return config.Serialize();
        }

        private byte[] WriteUiElements(List<UiElement> uiElements, int fileOffset)
        {
            short offset = 0;
            var spriteIds = new List<int>();
            var elemBytes = new byte[uiElements.Count * 8];

            for (int i = 0; i < uiElements.Count; i++)
            {
                WriteShort(elemBytes, i * 8 + 0x00, uiElements[i].id);
                if (uiElements[i].id == -1) continue;
                WriteShort(elemBytes, i * 8 + 0x02, (short) uiElements[i].sprites.Count);
                WriteShort(elemBytes, i * 8 + 0x04, offset);

                spriteIds.AddRange(uiElements[i].sprites);

                offset += (short) uiElements[i].sprites.Count;
            }

            var spriteBytes = new byte[spriteIds.Count * 4];
            for (int i = 0; i < spriteIds.Count; i++)
            {
                WriteInt(spriteBytes, i * 4, spriteIds[i]);
            }

            int elemStart = fileOffset + 0x10;
            int spriteStart = GetLength(elemStart + elemBytes.Length);

            var headBytes = new byte[0x10];
            WriteShort(headBytes, 0x00, (short) uiElements.Count);
            WriteShort(headBytes, 0x02, (short) spriteIds.Count);
            WriteInt(headBytes, 0x04, elemStart);
            WriteInt(headBytes, 0x08, spriteStart);

            var outBytes = new byte[headBytes.Length + GetLength(elemBytes.Length) + GetLength(spriteBytes.Length)];
            headBytes.CopyTo(outBytes, 0);
            elemBytes.CopyTo(outBytes, 0x10);
            spriteBytes.CopyTo(outBytes, 0x10 + GetLength(elemBytes.Length));

            return outBytes;
        }

        private int WriteMobies(FileStream fs, List<Model> mobyModels)
        {
            int headerSize = mobyModels.Count * 0x08 + 0x04;
            int headerOffset = SeekReserve(fs, headerSize);

            byte[] headBytes = new byte[headerSize];

            WriteInt(headBytes, 0x00, mobyModels.Count);

            for (int i = 0; i < mobyModels.Count; i++)
            {
                int mobyIDOffset = 0x04 + i * 0x08;

                WriteShort(headBytes, mobyIDOffset + 0x02, mobyModels[i].id);

                MobyModel g = (MobyModel) mobyModels[i];
                if (!g.isModel)
                    continue;

                int modelDataOffset = g.WriteBytes(fs);

                WriteInt(headBytes, mobyIDOffset + 0x04, modelDataOffset);
            }

            WriteBytesAtOffset(fs, headBytes, headerOffset);

            return headerOffset;
        }


        private int WriteWeapons(FileStream fs, List<Model> weaponModels)
        {
            int headerSize = weaponModels.Count * 0x10;
            int headerOffset = SeekReserve(fs, headerSize);

            byte[] headBytes = new byte[headerSize];

            for (int i = 0; i < weaponModels.Count; i++)
            {
                int modelHeaderOffset = i * 0x10;

                WriteInt(headBytes, modelHeaderOffset + 0x00, weaponModels[i].id);

                MobyModel g = (MobyModel) weaponModels[i];
                if (!g.isModel)
                    continue;

                int modelOffset = g.WriteBytes(fs);

                int modelBytesEnd = (int) fs.Position;

                WriteInt(headBytes, modelHeaderOffset + 0x04, modelOffset);
                WriteInt(headBytes, modelHeaderOffset + 0x08, modelBytesEnd - modelOffset);
            }

            WriteBytesAtOffset(fs, headBytes, headerOffset);

            return headerOffset;
        }

        private int WriteTies(FileStream fs, List<Tie> ties, int alignment)
        {
            int headerSize = ties.Count * 0x70;
            int headerOffset = SeekReserve(fs, headerSize);

            byte[] headBytes = new byte[headerSize];
            for (int i = 0; i < ties.Count; i++)
            {
                int colorBytesOffset = SeekWrite(fs, ties[i].colorBytes, alignment);

                ties[i].ToByteArray(colorBytesOffset).CopyTo(headBytes, i * 0x70);
            }

            WriteBytesAtOffset(fs, headBytes, headerOffset);

            return headerOffset;
        }

        private byte[] WriteShrubs(List<Shrub> shrubs)
        {
            var outBytes = new byte[shrubs.Count * 0x70];
            for (int i = 0; i < shrubs.Count; i++)
            {
                shrubs[i].ToByteArray().CopyTo(outBytes, i * 0x70);
            }

            return outBytes;
        }

        private byte[] WriteLights(List<Light> lights)
        {
            var outBytes = new byte[lights.Count * 0x40];
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].Serialize().CopyTo(outBytes, i * 0x40);
            }

            return outBytes;
        }

        private byte[] WriteTextureConfigMenus(List<int> textureConfigMenus)
        {
            var outBytes = new byte[textureConfigMenus.Count * 0x4];
            for (int i = 0; i < textureConfigMenus.Count; i++)
            {
                WriteInt(outBytes, i * 4, textureConfigMenus[i]);
            }

            return outBytes;
        }

        private int WriteTieModels(FileStream fs, List<Model> tiemodels)
        {
            int headerSize = tiemodels.Count * 0x40;
            int headerOffset = SeekReserve(fs, headerSize);

            for (int i = 0; i < tiemodels.Count; i++)
            {
                TieModel g = (TieModel) tiemodels[i];

                g.WriteBytes(fs, headerOffset + i * 0x40);
            }

            return headerOffset;
        }

        private byte[] WriteShrubModels(List<Model> shrubmodels, int offset)
        {
            offset += shrubmodels.Count * 0x40;

            var headBytes = new byte[shrubmodels.Count * 0x40];
            var bodyBytes = new List<byte>();

            for (int i = 0; i < shrubmodels.Count; i++)
            {
                ShrubModel g = (ShrubModel) shrubmodels[i];
                byte[] tieByte = g.SerializeHead(offset);
                byte[] bodBytes = g.SerializeBody(offset);
                bodyBytes.AddRange(bodBytes);
                offset += bodBytes.Length;
                tieByte.CopyTo(headBytes, i * 0x40);
            }

            var outBytes = new byte[headBytes.Length + bodyBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            bodyBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteTextures(List<Texture> textures)
        {
            if (enginePath == null)
                throw new System.Exception("Cannot write textures without a path!");

            var vramBytes = new List<byte>();
            var outBytes = new byte[textures.Count * 0x24];

            for (int i = 0; i < textures.Count; i++)
            {
                Pad(vramBytes);

                int vramOffset = vramBytes.Count;

                textures[i].Serialize(vramOffset).CopyTo(outBytes, i * 0x24);
                vramBytes.AddRange(textures[i].data);
            }

            FileStream fs = ReplanetizerFileStream.Open(Path.Join(Path.GetDirectoryName(enginePath), "vram.ps3"), FileMode.Create, FileAccess.Write);
            fs.Write(vramBytes.ToArray(), 0, vramBytes.Count);
            fs.Close();

            return outBytes;
        }

        private int WritePlayerAnimations(FileStream fs, List<Animation> animations)
        {
            int headerSize = animations.Count * 0x04;
            int headerOffset = SeekReserve(fs, headerSize);

            byte[] headerBytes = new byte[headerSize];

            for (int i = 0; i < animations.Count; i++)
            {
                int animOffset = animations[i].WriteBytes(fs, 0);
                WriteInt(headerBytes, i * 0x04, animOffset);
            }

            WriteBytesAtOffset(fs, headerBytes, headerOffset);

            return headerOffset;
        }

        private int WriteMobyOcclusion(FileStream fs, MobyOcclusion? mobyOcclusion)
        {
            if (mobyOcclusion == null)
                return 0;

            return mobyOcclusion.WriteBytes(fs);
        }

        private int WritePrecipicationMap(FileStream fs, PrecipitationMap? map)
        {
            if (map == null)
                return 0;

            return map.WriteBytes(fs);
        }
    }
}
