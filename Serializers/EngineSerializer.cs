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

        public void Save(Level level, String pathName, int textureCount)
        {
            this.pathName = pathName;
            FileStream fs = File.Open(pathName + "/engine.ps3", FileMode.Create);

            // Seek past the header, as we don't have the data ready for it yet
            fs.Seek(0x90, SeekOrigin.Begin);

            EngineHeader engineHeader = new EngineHeader
            {
                uiElementPointer =          SeekWrite(fs, WriteUiElements(level.uiElements, (int)fs.Position)),
                skyboxPointer =             SeekWrite(fs, level.skybox.Serialize((int)fs.Position)),
                terrainPointer =            SeekWrite(fs, level.terrainBytes),              // 0x3c - terrain
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
                lightConfigPointer =        SeekWrite(fs, level.lightConfig),
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

        private byte[] WriteTerrainBytes(byte[] terrainBlock, int fileOffset, int textureCount)
        {
            int texOffset0 = ReadInt(terrainBlock, 0x70);
            int off_00 = ReadInt(terrainBlock, 0x00);
            int off_08 = ReadInt(terrainBlock, 0x08);
            int off_18 = ReadInt(terrainBlock, 0x18);
            int off_28 = ReadInt(terrainBlock, 0x28);
            int off_38 = ReadInt(terrainBlock, 0x38);

            WriteInt(ref terrainBlock, 0x00, off_00 + fileOffset);
            WriteInt(ref terrainBlock, 0x08, off_08 + fileOffset);
            WriteInt(ref terrainBlock, 0x18, off_18 + fileOffset);
            WriteInt(ref terrainBlock, 0x28, off_28 + fileOffset);
            WriteInt(ref terrainBlock, 0x38, off_38 + fileOffset);

            short headCount = ReadShort(terrainBlock, 0x06);

            int texCount = 0;
            for (int i = 0; i < headCount; i++)
            {
                int texOffset = ReadInt(terrainBlock, 0x70 + i * 0x30);
                WriteInt(ref terrainBlock, 0x70 + i * 0x30, texOffset + fileOffset);
                texCount += ReadShort(terrainBlock, 0x76 + i * 0x30);
            }


            

            for (int i = 0; i < texCount; i++)
            {
                int texId = ReadInt(terrainBlock, texOffset0 + i * 0x10);
                WriteInt(ref terrainBlock, texOffset0 + i * 0x10, texId + textureCount);
            }

            Console.WriteLine("Texutre0offset: " + texOffset0);
            Console.WriteLine("textureCount: " + textureCount);
            Console.WriteLine("texCount: " + texCount);



            return terrainBlock;
        }

        private byte[] WriteUiElements(List<UiElement> uiElements, int fileOffset)
        {
            short offset = 0;
            var spriteIds = new List<int>();
            byte[] elemBytes = new byte[uiElements.Count * 8];
            for (int i = 0; i < uiElements.Count; i++)
            {
                WriteShort(ref elemBytes, i * 8 + 0x00, uiElements[i].id);
                if (uiElements[i].id == -1) continue;
                WriteShort(ref elemBytes, i * 8 + 0x02, (short)uiElements[i].sprites.Count);
                WriteShort(ref elemBytes, i * 8 + 0x04, offset);

                spriteIds.AddRange(uiElements[i].sprites);

                offset += (short)uiElements[i].sprites.Count;
            }

            byte[] spriteBytes = new byte[spriteIds.Count * 4];
            for (int i = 0; i < spriteIds.Count; i++)
            {
                WriteInt(ref spriteBytes, i * 4, spriteIds[i]);
            }

            int elemStart = fileOffset + 0x10;
            int spriteStart = GetLength(elemStart + elemBytes.Length);

            byte[] headBytes = new byte[0x10];
            WriteShort(ref headBytes, 0x00, (short)uiElements.Count);
            WriteShort(ref headBytes, 0x02, (short)spriteIds.Count);
            WriteInt(ref headBytes, 0x04, elemStart);
            WriteInt(ref headBytes, 0x08, spriteStart);

            byte[] outBytes = new byte[headBytes.Length + GetLength(elemBytes.Length) + GetLength(spriteBytes.Length)];
            headBytes.CopyTo(outBytes, 0);
            elemBytes.CopyTo(outBytes, 0x10);
            spriteBytes.CopyTo(outBytes, 0x10 + GetLength(elemBytes.Length));

            return outBytes;
        }

        private byte[] WriteMobies(List<Model> mobyModels, int initOffset)
        {
            int offs = initOffset;
            byte[] headBytes = new byte[mobyModels.Count * 8 + 4];
            WriteInt(ref headBytes, 0, mobyModels.Count);
            offs += GetLength(headBytes.Length);

            List<byte> bodBytes = new List<byte>();

            for (int i = 0; i < mobyModels.Count; i++)
            {
                WriteInt(ref headBytes, 4 + i * 8, mobyModels[i].id);

                MobyModel g = (MobyModel)mobyModels[i];
                if (!g.isModel)
                    continue;

                WriteInt(ref headBytes, 4 + i * 8 + 4, offs);
                byte[] bodyByte = g.Serialize();
                bodBytes.AddRange(bodyByte);
                offs += GetLength(bodyByte.Length);

            }

            byte[] outBuff = new byte[GetLength(headBytes.Length) + bodBytes.Count];
            headBytes.CopyTo(outBuff, 0);
            bodBytes.CopyTo(outBuff, GetLength(headBytes.Length));

            return outBuff;
        }


        private byte[] WriteWeapons(List<Model> weaponModels, int initOffset)
        {
            int offs = initOffset;
            byte[] headBytes = new byte[weaponModels.Count * 0x10];
            int headLength = GetLength(headBytes.Length);
            offs += headLength;

            List<byte> bodBytes = new List<byte>();

            for (int i = 0; i < weaponModels.Count; i++)
            {
                WriteInt(ref headBytes, i * 0x10, weaponModels[i].id);

                MobyModel g = (MobyModel)weaponModels[i];
                if (!g.isModel)
                    continue;

                byte[] bodyByte = g.Serialize();
                WriteInt(ref headBytes, i * 0x10 + 4, offs);
                WriteInt(ref headBytes, i * 0x10 + 8, bodyByte.Length);
                bodBytes.AddRange(bodyByte);
                offs += GetLength(bodyByte.Length);

            }

            byte[] outBuff = new byte[headLength + bodBytes.Count];
            headBytes.CopyTo(outBuff, 0);
            bodBytes.CopyTo(outBuff, headLength);

            return outBuff;
        }

        private byte[] WriteTies(List<Tie> ties, int offset)
        {
            byte[] headBytes = new byte[ties.Count * 0x70];
            List<byte> colorBytes = new List<byte>();
            int initOffset = offset + ties.Count * 0x70;

            for (int i = 0; i < ties.Count; i++)
            {
                ties[i].ToByteArray(initOffset + colorBytes.Count).CopyTo(headBytes, i * 0x70);
                byte[] colByte = ties[i].colorBytes;
                colorBytes.AddRange(colByte);
                while ((colorBytes.Count % 0x80) != 0)
                {
                    colorBytes.Add(0);
                }
            }

            byte[] outBytes = new byte[headBytes.Length + colorBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            colorBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteShrubs(List<Shrub> shrubs)
        {
            byte[] outBytes = new byte[shrubs.Count * 0x70];
            for (int i = 0; i < shrubs.Count; i++)
            {
                shrubs[i].ToByteArray().CopyTo(outBytes, i * 0x70);
            }

            return outBytes;
        }

        private byte[] WriteLights(List<Light> lights)
        {
            byte[] outBytes = new byte[lights.Count * 0x40];
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].Serialize().CopyTo(outBytes, i * 0x40);
            }

            return outBytes;
        }

        private byte[] WriteTextureConfigMenus(List<int> textureConfigMenus)
        {
            byte[] outBytes = new byte[textureConfigMenus.Count * 0x4];
            for (int i = 0; i < textureConfigMenus.Count; i++)
            {
                WriteInt(ref outBytes, i * 4, textureConfigMenus[i]);
            }

            return outBytes;
        }

        private byte[] WriteTieModels(List<Model> tiemodels, int startOff)
        {
            byte[] headBytes = new byte[tiemodels.Count * 0x40];
            List<byte> bodyBytes = new List<byte>();

            startOff += tiemodels.Count * 0x40;

            for (int i = 0; i < tiemodels.Count; i++)
            {
                TieModel g = (TieModel)tiemodels[i];
                byte[] tieByte = g.SerializeHead(startOff);
                byte[] bodBytes = g.SerializeBody(startOff);
                bodyBytes.AddRange(bodBytes);
                startOff += bodBytes.Length;
                tieByte.CopyTo(headBytes, i * 0x40);
            }

            byte[] outBytes = new byte[headBytes.Length + bodyBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            bodyBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteShrubModels(List<Model> shrubmodels, int startOff)
        {
            byte[] headBytes = new byte[shrubmodels.Count * 0x40];
            List<byte> bodyBytes = new List<byte>();

            startOff += shrubmodels.Count * 0x40;

            for (int i = 0; i < shrubmodels.Count; i++)
            {
                ShrubModel g = (ShrubModel)shrubmodels[i];
                byte[] tieByte = g.SerializeHead(startOff);
                byte[] bodBytes = g.SerializeBody(startOff);
                bodyBytes.AddRange(bodBytes);
                startOff += bodBytes.Length;
                tieByte.CopyTo(headBytes, i * 0x40);
            }

            byte[] outBytes = new byte[headBytes.Length + bodyBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            bodyBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteTextures(List<Texture> textures)
        {
            List<byte> vramBytes = new List<byte>();

            int vramOffset = 0;

            byte[] outBytes = new byte[textures.Count * 0x24];
            for (int i = 0; i < textures.Count; i++)
            {
                while (vramBytes.Count % 0x10 != 0) vramBytes.Add(0);
                vramOffset = vramBytes.Count;

                textures[i].Serialize(vramOffset).CopyTo(outBytes, i * 0x24);
                vramBytes.AddRange(textures[i].data);
            }


            byte[] vramBys = vramBytes.ToArray();

            FileStream fs = File.Open(pathName + "/vram.ps3", FileMode.Create);
            fs.Write(vramBys, 0, vramBys.Length);
            fs.Close();

            return outBytes;
        }

        private byte[] WritePlayerAnimations(List<Animation> animations, int animationOffset)
        {
            int offsetListLength = GetLength(animations.Count * 4);
            animationOffset += offsetListLength;

            List<byte> animByteList = new List<byte>();
            List<int> animOffsets = new List<int>();

            foreach (Animation anim in animations)
            {
                if (anim.frames.Count != 0)
                {
                    animOffsets.Add(animationOffset);
                    byte[] anima = anim.Serialize(0);
                    animByteList.AddRange(anima);
                    animationOffset += anima.Length;
                }
                else
                {
                    animOffsets.Add(0);
                }
            }

            byte[] outBytes = new byte[offsetListLength + animByteList.Count];
            for (int i = 0; i < animations.Count; i++)
            {
                WriteInt(ref outBytes, i * 0x04, animOffsets[i]);
            }
            animByteList.CopyTo(outBytes, offsetListLength);

            return outBytes;
        }

        private void WriteSkybox(FileStream fs, List<UiElement> uiElements)
        {

        }
    }
}
