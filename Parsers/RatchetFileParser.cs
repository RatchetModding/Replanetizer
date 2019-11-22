using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using RatchetEdit.Models;
using RatchetEdit.Headers;
using RatchetEdit.LevelObjects;
using RatchetEdit.Models.Animations;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Parsers
{
    public class RatchetFileParser
    {
        protected FileStream fileStream;

        protected RatchetFileParser(string filePath)
        {
            try
            {
                fileStream = File.OpenRead(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Couldn't load engine file.");
                Application.Exit();
                return;
            }
        }

        protected List<Model> GetMobyModels(int mobyModelPointer)
        {
            //Get the moby count from the start of the section
            int mobyModelCount = ReadInt(ReadBlock(fileStream, mobyModelPointer, 4), 0);

            List<Model> mobyModels = new List<Model>(mobyModelCount);

            //Each moby is stored as a [MobyID, offset] pair
            byte[] mobyIDBlock = ReadBlock(fileStream, mobyModelPointer + 4, mobyModelCount * 8);
            for (int i = 0; i < mobyModelCount; i++)
            {
                short modelID = ReadShort(mobyIDBlock, (i * 8) + 2);
                int offset = ReadInt(mobyIDBlock, (i * 8) + 4);
                mobyModels.Add(new MobyModel(fileStream, modelID, offset));
            }
            return mobyModels;
        }

        protected List<Model> GetTieModels(int tieModelPointer, int tieModelCount)
        {
            List<Model> tieModelList = new List<Model>(tieModelCount);

            //Read the whole header block, and add models based on the count
            byte[] levelBlock = ReadBlock(fileStream, tieModelPointer, tieModelCount * 0x40);
            for (int i = 0; i < tieModelCount; i++)
            {
                tieModelList.Add(new TieModel(fileStream, levelBlock, i));
            }

            return tieModelList;
        }

        protected List<Model> GetShrubModels(int shrubModelPointer, int shrubModelCount)
        {
            List<Model> shrubModelList = new List<Model>(shrubModelCount);

            //Read the whole header block, and add models based on the count
            byte[] shrubBlock = ReadBlock(fileStream, shrubModelPointer, shrubModelCount * 0x40);
            for (int i = 0; i < shrubModelCount; i++)
            {
                shrubModelList.Add(new ShrubModel(fileStream, shrubBlock, i));
            }
            return shrubModelList;
        }

        protected List<Texture> GetTextures(int texturePointer, int textureCount)
        {
            List<Texture> textureList = new List<Texture>(textureCount);

            //Read the whole texture header block, and add textures based on the count
            byte[] textureBlock = ReadBlock(fileStream, texturePointer, textureCount * Texture.TEXTUREELEMSIZE);
            for (int i = 0; i < textureCount; i++)
            {
                textureList.Add(new Texture(textureBlock, i));
            }
            return textureList;
        }

        protected List<Tie> GetTies(List<Model> tieModels, int tiePointer, int tieCount)
        {
            List<Tie> ties = new List<Tie>(tieCount);

            //Read the whole texture header block, and add textures based on the count
            byte[] tieBlock = ReadBlock(fileStream, tiePointer, tieCount * 0x70);
            for (int i = 0; i < tieCount; i++)
            {
                Tie tie = new Tie(tieBlock, i, tieModels, fileStream);
                ties.Add(tie);
            }
            return ties;
        }

        protected List<Shrub> GetShrubs(List<Model> shrubModels, int shrubPointer, int shrubCount)
        {
            List<Shrub> shrubs = new List<Shrub>(shrubCount);

            //Read the whole texture header block, and add models based on the count
            byte[] shrubBlock = ReadBlock(fileStream, shrubPointer, shrubCount * 0x70);
            for (int i = 0; i < shrubCount; i++)
            {
                Shrub shrub = new Shrub(shrubBlock, i, shrubModels);
                shrubs.Add(shrub);
            }
            return shrubs;
        }

        protected List<Light> GetLights(int lightPointer, int lightCount)
        {
            List<Light> lightList = new List<Light>(lightCount);

            //Read the whole header block, and add lights based on the count
            byte[] lightBlock = ReadBlock(fileStream, lightPointer, lightCount * 0x40);
            for (int i = 0; i < lightCount; i++)
            {
                lightList.Add(new Light(lightBlock, i));
            }
            return lightList;
        }


        protected List<TerrainFragment> GetTerrainModels(int terrainModelPointer)
        {
            List<TerrainFragment> tFrags = new List<TerrainFragment>();

            //Read the whole terrain header
            byte[] terrainBlock = ReadBlock(fileStream, terrainModelPointer, 0x60);
            TerrainHead head = new TerrainHead(terrainBlock);

            byte[] tfragBlock = ReadBlock(fileStream, terrainModelPointer + 0x60, head.headCount * 0x30);

            for (int i = 0; i < head.headCount; i++)
            {
                tFrags.Add(new TerrainFragment(fileStream, head, tfragBlock, i));
            }

            /*
            List<TerrainHeader> pointerList = new List<TerrainHeader>(terrainHeadCount);

            byte[] terrainHeadBlock = ReadBlock(fileStream, terrainHeadPointer, terrainHeadCount * 0x30);
            for (int i = 0; i < terrainHeadCount; i++)
            {
                TerrainFragHeader head = new TerrainFragHeader(fileStream, terrainHeadBlock, i);
                if (pointerList.Count < head.slotNum + 1)
                {
                    pointerList.Add(new TerrainHeader(terrainBlock, (head.slotNum * 4)));
                }
                pointerList[head.slotNum].vertexCount += head.vertexCount;
                pointerList[head.slotNum].heads.Add(head);
            }

            List<TerrainModel> terrainModels = new List<TerrainModel>(pointerList.Count);
            foreach (TerrainHeader hd in pointerList)
            {
                terrainModels.Add(new TerrainModel(fileStream, hd));
            }*/

            return tFrags;
        }

        protected SkyboxModel GetSkyboxModel(int skyboxPointer)
        {
            return new SkyboxModel(fileStream, skyboxPointer);
        }

        protected GameType DetectGame(int offset)
        {
            uint magic = ReadUint(ReadBlock(fileStream, offset, 4), 0);
            switch (magic)
            {
                case 0x00000001:
                    return new GameType(1);
                case 0xEAA90001:
                    return new GameType(2);
                case 0xEAA60001:
                    return new GameType(3);
                default:
                    return new GameType(3);
            }
        }

        protected List<UiElement> GetUiElements(int offset)
        {
            byte[] headBlock = ReadBlock(fileStream, offset, 0x10);
            short elemCount = ReadShort(headBlock, 0x00);
            short spriteCount = ReadShort(headBlock, 0x02);
            int elemOffset = ReadInt(headBlock, 0x04);
            int spriteOffset = ReadInt(headBlock, 0x08);

            byte[] elemBlock = ReadBlock(fileStream, elemOffset, elemCount * 8);
            byte[] spriteBlock = ReadBlock(fileStream, spriteOffset, spriteCount * 4);

            var list = new List<UiElement>(elemCount);
            for(int i = 0; i < elemCount; i++)
            {
                list.Add(new UiElement(elemBlock, i, spriteBlock));
            }
            return list;
        }

        protected List<Animation> GetPlayerAnimations(int offset, MobyModel ratchet)
        {
            int count = ratchet.animations.Count;
            byte boneCount = (byte)ratchet.boneMatrices.Count;

            byte[] headBlock = ReadBlock(fileStream, offset, count * 0x04);
            List<Animation> animations = new List<Animation>(ratchet.animations.Count);
            for(int i = 0; i < count; i++)
            {
                animations.Add(new Animation(fileStream, ReadInt(headBlock, i * 4), 0, boneCount, true));
            }
            return animations;
        }

        protected List<Model> GetWeapons(int weaponPointer, int count)
        {
            List<Model> weaponModels = new List<Model>(count);

            //Each moby is stored as a [MobyID, offset] pair
            byte[] mobyIDBlock = ReadBlock(fileStream, weaponPointer, count * 0x10);
            for (int i = 0; i < count; i++)
            {
                short modelID = ReadShort(mobyIDBlock, (i * 0x10) + 2);
                int offset = ReadInt(mobyIDBlock, (i * 0x10) + 4);
                weaponModels.Add(new MobyModel(fileStream, modelID, offset));
            }
            return weaponModels;
        }

        protected byte[] GetLightConfig(int lightConfigOffset)
        {
            return ReadBlock(fileStream, lightConfigOffset, 0x30);
        }

        protected List<int> GetTextureConfigMenu(int textureConfigMenuOffset, int textureConfigMenuCount)
        {
            List<int> textureConfigMenuList = new List<int>(textureConfigMenuCount);
            byte[] textureConfigMenuBlock = ReadBlock(fileStream, textureConfigMenuOffset, textureConfigMenuCount * 4);

            for (int i = 0; i < textureConfigMenuCount; i++)
            {
                textureConfigMenuList.Add(ReadInt(textureConfigMenuBlock, i * 4));
            }
            return textureConfigMenuList;
        }

        protected Model GetCollisionModel(int collisionOffset)
        {
            return new Collision(fileStream, collisionOffset);
        }

        protected byte[] ReadArbBytes(int offset, int length)
        {
            return ReadBlock(fileStream, offset, length);
        }

        public void Close()
        {
            fileStream.Close();
        }
    }
}
