using System;
using System.IO;
using System.Collections.Generic;
using RatchetEdit.Models;
using RatchetEdit.Headers;
using RatchetEdit.LevelObjects;
using RatchetEdit.Models.Animations;
using static RatchetEdit.DataFunctions;


namespace RatchetEdit.Serializers
{
    class EngineSerializer
    {
        public string pathName;

        public void Save(Level level, string pathName)
        {
            this.pathName = pathName;
            FileStream fs = File.Open(pathName + "/engine.ps3", FileMode.Create);

            // Seek past the header, as we don't have the data ready for it yet
            fs.Seek(0x90, SeekOrigin.Begin);

            EngineHeader engineHeader = new EngineHeader
            {
                uiElementPointer =          SeekWrite(fs, WriteUiElements(level.uiElements, (int)fs.Position)),
                skyboxPointer =             SeekWrite(fs, level.skybox.Serialize((int)fs.Position)),
                terrainPointer =            SeekWrite(fs, WriteTfrags(level.terrains, (int)fs.Position)),              // 0x3c - terrain
                renderDefPointer =          SeekWrite(fs, level.renderDefBytes),            // 0x04 - renderdef
                collisionPointer =          SeekWrite(fs, level.collBytes),                 // 0x14 - collision
                mobyModelPointer =          SeekWrite(fs, WriteMobies(level.mobyModels, (int)fs.Position)),
                playerAnimationPointer =    SeekWrite(fs, WritePlayerAnimations(level.playerAnimations, (int)fs.Position)),
                weaponPointer =             SeekWrite(fs, WriteWeapons(level.weaponModels, (int)fs.Position)),
                tieModelPointer =           SeekWrite(fs, WriteTieModels(level.tieModels, (int)fs.Position)),
                tiePointer =                SeekWrite(fs, WriteTies(level.ties, (int)fs.Position)),
                shrubModelPointer =         SeekWrite(fs, WriteShrubModels(level.shrubModels, (int)fs.Position)),
                shrubPointer =              SeekWrite(fs, WriteShrubs(level.shrubs)),
                textureConfigMenuPointer =  SeekWrite(fs, WriteTextureConfigMenus(level.textureConfigMenus)),
                texture2dPointer =          SeekWrite(fs, level.billboardBytes),            // 0x70 2dtexturestuff
                soundConfigPointer =        SeekWrite(fs, level.soundConfigBytes),          // 0x48 soundconfigs
                lightPointer =              SeekWrite(fs, WriteLights(level.lights)),
                lightConfigPointer =        SeekWrite(fs, WriteLightConfig(level.lightConfig)),
                texturePointer =            SeekWrite(fs, WriteTextures(level.textures)),
                // Counts
                tieModelCount =             level.tieModels.Count,
                tieCount =                  level.ties.Count,
                shrubModelCount =           level.shrubModels.Count,
                shrubCount =                level.shrubs.Count,
                weaponCount =               level.weaponModels.Count,
                textureCount =              level.textures.Count,
                lightCount =                level.lights.Count,
                textureConfigMenuCount =    level.textureConfigMenus.Count
            };

            // Seek to the beginning and write the header now that we have all the pointers
            byte[] head = engineHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
        }

        private int SeekWrite(FileStream fs, byte[] bytes)
        {
            int pos = (int)fs.Position;
            fs.Write(bytes, 0, bytes.Length);
            SeekPast(fs);
            return pos;
        }

        private void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        private byte[] WriteLightConfig(LightConfig config)
        {

            return config.Serialize();
        }

        private byte[] WriteTfrags(List<TerrainFragment> tFrags, int fileOffset)
        {
            List<List<byte>> vertBytes = new List<List<byte>>(){ new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> rgbaBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> uvBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> indexBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };

            List<byte> textureBytes = new List<byte>();

            byte[] tfragHeads = new byte[0x30 * tFrags.Count];

            ushort chunk = 0;

            for (int i = 0; i < tFrags.Count; i++)
            {
                TerrainModel mod = (TerrainModel)(tFrags[i].model);

                int offset = i * 0x30;
                WriteFloat(tfragHeads, offset + 0x00, tFrags[i].off_00);
                WriteFloat(tfragHeads, offset + 0x04, tFrags[i].off_04);
                WriteFloat(tfragHeads, offset + 0x08, tFrags[i].off_08);
                WriteFloat(tfragHeads, offset + 0x0C, tFrags[i].off_0C);

                WriteInt(tfragHeads, offset + 0x10, fileOffset + 0x60 + tfragHeads.Length + textureBytes.Count);
                WriteInt(tfragHeads, offset + 0x14, tFrags[i].model.textureConfig.Count);

                byte[] modelVertBytes = mod.SerializeVerts();
                if (((vertBytes[chunk].Count + modelVertBytes.Length) / 0x1c) > 0xffff)
                {
                    chunk++;
                }

                WriteUshort(tfragHeads, offset + 0x18, (ushort)(vertBytes[chunk].Count / 0x1c));
                WriteUshort(tfragHeads, offset + 0x1a, (ushort)(tFrags[i].model.vertexBuffer.Length / 8));

                WriteUshort(tfragHeads, offset + 0x1C, tFrags[i].off_1C);
                WriteUshort(tfragHeads, offset + 0x1E, tFrags[i].off_1E);
                WriteUshort(tfragHeads, offset + 0x20, tFrags[i].off_20);
                WriteUshort(tfragHeads, offset + 0x22, chunk);
                WriteUint(tfragHeads, offset + 0x24, tFrags[i].off_24);
                WriteUint(tfragHeads, offset + 0x28, tFrags[i].off_28);
                WriteUint(tfragHeads, offset + 0x2C, tFrags[i].off_2C);

                foreach (var texConf in tFrags[i].model.textureConfig)
                {
                    byte[] texBytes = new byte[0x10];
                    WriteInt(texBytes, 0x00, texConf.ID);
                    WriteInt(texBytes, 0x04, texConf.start + indexBytes[chunk].Count / 2);
                    WriteInt(texBytes, 0x08, texConf.size);
                    WriteInt(texBytes, 0x0C, texConf.mode);
                    textureBytes.AddRange(texBytes);
                }

                indexBytes[chunk].AddRange(tFrags[i].model.GetFaceBytes((ushort)(vertBytes[chunk].Count / 0x1C)));
                vertBytes[chunk].AddRange(modelVertBytes);
                rgbaBytes[chunk].AddRange(tFrags[i].model.rgbas);
                uvBytes[chunk].AddRange(tFrags[i].model.SerializeUVs());

            }

            List<byte> outBytes = new List<byte>();

            byte[] head = new byte[0x60];
            WriteInt(head, 0, fileOffset + 0x60);
            WriteUshort(head, 0x4, (ushort)tFrags.Count);
            WriteUshort(head, 0x6, (ushort)tFrags.Count);

            outBytes.AddRange(head);
            outBytes.AddRange(tfragHeads);
            outBytes.AddRange(textureBytes);

            int[] vertOffsets = { 0, 0, 0, 0 };
            int[] rgbaOffsets = { 0, 0, 0, 0 };
            int[] uvOffsets = { 0, 0, 0, 0 };
            int[] indexOffsets = { 0, 0, 0, 0 };

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
            }


            byte[] outByteArr = outBytes.ToArray();

            for (int i = 0; i < 4; i++)
            {
                WriteInt(outByteArr, 0x08 + i * 4, vertOffsets[i]);
                WriteInt(outByteArr, 0x18 + i * 4, rgbaOffsets[i]);
                WriteInt(outByteArr, 0x28 + i * 4, uvOffsets[i]);
                WriteInt(outByteArr, 0x38 + i * 4, indexOffsets[i]);
            }

            return outByteArr;
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
                WriteShort(elemBytes, i * 8 + 0x02, (short)uiElements[i].sprites.Count);
                WriteShort(elemBytes, i * 8 + 0x04, offset);

                spriteIds.AddRange(uiElements[i].sprites);

                offset += (short)uiElements[i].sprites.Count;
            }

            var spriteBytes = new byte[spriteIds.Count * 4];
            for (int i = 0; i < spriteIds.Count; i++)
            {
                WriteInt(spriteBytes, i * 4, spriteIds[i]);
            }

            int elemStart = fileOffset + 0x10;
            int spriteStart = GetLength(elemStart + elemBytes.Length);

            var headBytes = new byte[0x10];
            WriteShort(headBytes, 0x00, (short)uiElements.Count);
            WriteShort(headBytes, 0x02, (short)spriteIds.Count);
            WriteInt(headBytes, 0x04, elemStart);
            WriteInt(headBytes, 0x08, spriteStart);

            var outBytes = new byte[headBytes.Length + GetLength(elemBytes.Length) + GetLength(spriteBytes.Length)];
            headBytes.CopyTo(outBytes, 0);
            elemBytes.CopyTo(outBytes, 0x10);
            spriteBytes.CopyTo(outBytes, 0x10 + GetLength(elemBytes.Length));

            return outBytes;
        }

        private byte[] WriteMobies(List<Model> mobyModels, int offset)
        {
            var headBytes = new byte[mobyModels.Count * 8 + 4];
            var bodBytes = new List<byte>();

            WriteInt(headBytes, 0, mobyModels.Count);
            offset += headBytes.Length;

            for (int i = 0; i < mobyModels.Count; i++)
            {
                WriteInt(headBytes, 4 + i * 8, mobyModels[i].id);

                MobyModel g = (MobyModel)mobyModels[i];
                if (!g.isModel)
                    continue;

                WriteInt(headBytes, 4 + i * 8 + 4, offset);
                byte[] bodyByte = g.Serialize(offset);
                bodBytes.AddRange(bodyByte);
                offset += bodyByte.Length;
            }

            var outBuff = new byte[headBytes.Length + bodBytes.Count];
            headBytes.CopyTo(outBuff, 0);
            bodBytes.CopyTo(outBuff,headBytes.Length);

            return outBuff;
        }


        private byte[] WriteWeapons(List<Model> weaponModels, int offset)
        {
            var headBytes = new byte[weaponModels.Count * 0x10];
            var bodyBytes = new List<byte>();

            int headLength = GetLength(headBytes.Length);
            offset += headLength;

            for (int i = 0; i < weaponModels.Count; i++)
            {
                WriteInt(headBytes, i * 0x10, weaponModels[i].id);

                MobyModel g = (MobyModel)weaponModels[i];
                if (!g.isModel)
                    continue;

                byte[] bodyByte = g.Serialize(offset);
                WriteInt(headBytes, i * 0x10 + 4, offset);
                WriteInt(headBytes, i * 0x10 + 8, bodyByte.Length);
                bodyBytes.AddRange(bodyByte);
                offset += bodyByte.Length;

            }

            var outBuff = new byte[headLength + bodyBytes.Count];
            headBytes.CopyTo(outBuff, 0);
            bodyBytes.CopyTo(outBuff, headLength);

            return outBuff;
        }

        private byte[] WriteTies(List<Tie> ties, int offset)
        {
            offset += ties.Count * 0x70;

            var headBytes = new byte[ties.Count * 0x70];
            var colorBytes = new List<byte>();

            for (int i = 0; i < ties.Count; i++)
            {
                ties[i].ToByteArray(offset + colorBytes.Count).CopyTo(headBytes, i * 0x70);
                byte[] colByte = ties[i].colorBytes;
                colorBytes.AddRange(colByte);

                if (i != ties.Count - 1)
                {
                    while ((colorBytes.Count % 0x80) != 0)
                    {
                        colorBytes.Add(0);
                    }
                }

            }

            int hack = DistToFile80(offset);
            var outBytes = new byte[headBytes.Length + hack + colorBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            colorBytes.CopyTo(outBytes, headBytes.Length + hack);

            return outBytes;
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

        private byte[] WriteTieModels(List<Model> tiemodels, int offset)
        {
            offset += tiemodels.Count * 0x40;

            var headBytes = new byte[tiemodels.Count * 0x40];
            var bodyBytes = new List<byte>();

            for (int i = 0; i < tiemodels.Count; i++)
            {
                TieModel g = (TieModel)tiemodels[i];
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

        private byte[] WriteShrubModels(List<Model> shrubmodels, int offset)
        {
            offset += shrubmodels.Count * 0x40;

            var headBytes = new byte[shrubmodels.Count * 0x40];
            var bodyBytes = new List<byte>();

            for (int i = 0; i < shrubmodels.Count; i++)
            {
                ShrubModel g = (ShrubModel)shrubmodels[i];
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
            var vramBytes = new List<byte>();
            var outBytes = new byte[textures.Count * 0x24];

            for (int i = 0; i < textures.Count; i++)
            {
                while (vramBytes.Count % 0x10 != 0) vramBytes.Add(0);
                int vramOffset = vramBytes.Count;

                textures[i].Serialize(vramOffset).CopyTo(outBytes, i * 0x24);
                vramBytes.AddRange(textures[i].data);
            }

            FileStream fs = File.Open(pathName + "/vram.ps3", FileMode.Create);
            fs.Write(vramBytes.ToArray(), 0, vramBytes.Count);
            fs.Close();

            return outBytes;
        }

        private byte[] WritePlayerAnimations(List<Animation> animations, int animationOffset)
        {
            int offsetListLength = GetLength(animations.Count * 4);
            animationOffset += offsetListLength;

            var animByteList = new List<byte>();
            var animOffsets = new List<int>();

            foreach (Animation anim in animations)
            {
                if (anim.frames.Count != 0)
                {
                    animOffsets.Add(animationOffset);
                    byte[] anima = anim.Serialize(0, animationOffset);
                    animByteList.AddRange(anima);
                    animationOffset += anima.Length;
                }
                else
                {
                    animOffsets.Add(0);
                }
            }

            var outBytes = new byte[offsetListLength + animByteList.Count];
            for (int i = 0; i < animations.Count; i++)
            {
                WriteInt(outBytes, i * 0x04, animOffsets[i]);
            }
            animByteList.CopyTo(outBytes, offsetListLength);

            return outBytes;
        }
    }
}
