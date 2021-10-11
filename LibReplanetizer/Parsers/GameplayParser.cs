// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    class GameplayParser : IDisposable
    {
        GameType game;
        FileStream fileStream;
        GameplayHeader gameplayHeader;

        public GameplayParser(GameType game, string gameplayFilepath)
        {
            this.game = game;
            fileStream = File.OpenRead(gameplayFilepath);
            gameplayHeader = new GameplayHeader(game, fileStream);
        }

        public List<Spline> GetSplines()
        {
            if (gameplayHeader.splinePointer == 0) { return null; }

            var splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(fileStream, gameplayHeader.splinePointer, 4), 0);
            int splineOffset = ReadInt(ReadBlock(fileStream, gameplayHeader.splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(fileStream, gameplayHeader.splinePointer + 8, 4), 0);

            byte[] splineHeadBlock = ReadBlock(fileStream, gameplayHeader.splinePointer + 0x10, splineCount * 4);
            byte[] splineBlock = ReadBlock(fileStream, gameplayHeader.splinePointer + splineOffset, splineSectionSize);

            for (int i = 0; i < splineCount; i++)
            {
                int offset = ReadInt(splineHeadBlock, (i * 4));
                splines.Add(new Spline(splineBlock, offset));
            }
            return splines;
        }

        public LevelVariables GetLevelVariables()
        {
            return new LevelVariables(game, fileStream, gameplayHeader.levelVarPointer, gameplayHeader.englishPointer - gameplayHeader.levelVarPointer);
        }

        public Dictionary<int, String> GetLang(int offset)
        {
            if (offset == 0) { return new Dictionary<int, string>(); }

            byte[] langHeader = ReadBlock(fileStream, offset, 0x08);
            int numItems = ReadInt(langHeader, 0x00);
            int langLength = ReadInt(langHeader, 0x04);

            Dictionary<int, String> languageData = new Dictionary<int, String>();

            for (int i = 0; i < numItems; i++)
            {
                int pointerOffset = offset + 8 + (i * 16);
                byte[] block = ReadBlock(fileStream, pointerOffset, 0x08);

                int textPointer = ReadInt(block, 0x00);
                int textId = ReadInt(block, 0x04);

                String textData = ReadString(fileStream, offset + textPointer);
                languageData.Add(textId, textData);
            }

            return languageData;
        }

        public Dictionary<int, String> GetEnglish()
        {
            return GetLang(gameplayHeader.englishPointer);
        }

        public Dictionary<int, String> GetUKEnglish()
        {
            return GetLang(gameplayHeader.ukenglishPointer);
        }

        public Dictionary<int, String> GetFrench()
        {
            return GetLang(gameplayHeader.frenchPointer);
        }

        public Dictionary<int, String> GetGerman()
        {
            return GetLang(gameplayHeader.germanPointer);
        }

        public Dictionary<int, String> GetSpanish()
        {
            return GetLang(gameplayHeader.spanishPointer);
        }

        public Dictionary<int, String> GetItalian()
        {
            return GetLang(gameplayHeader.italianPointer);
        }

        public Dictionary<int, String> GetJapanese()
        {
            return GetLang(gameplayHeader.japanesePointer);
        }

        public Dictionary<int, String> GetKorean()
        {
            return GetLang(gameplayHeader.koreanPointer);
        }


        // TODO consolidate all these into a single function, as they work pretty much the same
        public List<Moby> GetMobies(List<Model> mobyModels)
        {
            var mobs = new List<Moby>();

            if (gameplayHeader.mobyPointer == 0) { return mobs; }

            int mobyCount = ReadInt(ReadBlock(fileStream, gameplayHeader.mobyPointer, 4), 0);

            byte[] mobyBlock = ReadBlock(fileStream, gameplayHeader.mobyPointer + 0x10, game.mobyElemSize * mobyCount);
            for (int i = 0; i < mobyCount; i++)
            {
                mobs.Add(new Moby(game, mobyBlock, i, mobyModels));
            }
            return mobs;
        }

        public List<Cuboid> GetCuboids()
        {
            var cuboids = new List<Cuboid>();

            if (gameplayHeader.cuboidPointer == 0) { return cuboids; }

            int cuboidCount = ReadInt(ReadBlock(fileStream, gameplayHeader.cuboidPointer, 4), 0);
            byte[] spawnPointBlock = ReadBlock(fileStream, gameplayHeader.cuboidPointer + 0x10, Cuboid.ELEMENTSIZE * cuboidCount);
            for (int i = 0; i < cuboidCount; i++)
            {
                cuboids.Add(new Cuboid(spawnPointBlock, i));
            }
            return cuboids;
        }

        public List<GameCamera> GetGameCameras()
        {
            var cameraList = new List<GameCamera>();
            if (gameplayHeader.cameraPointer == 0) { return cameraList; }

            int cameraCount = ReadInt(ReadBlock(fileStream, gameplayHeader.cameraPointer, 4), 0);
            byte[] cameraBlock = ReadBlock(fileStream, gameplayHeader.cameraPointer + 0x10, GameCamera.ELEMENTSIZE * cameraCount);
            for (int i = 0; i < cameraCount; i++)
            {
                cameraList.Add(new GameCamera(cameraBlock, i));
            }

            return cameraList;
        }

        public List<DirectionalLight> GetDirectionalLights()
        {
            var dirLights = new List<DirectionalLight>();
            if (gameplayHeader.lightsPointer == 0) { return dirLights; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.lightsPointer, 4), 0);
            byte[] type04Block = ReadBlock(fileStream, gameplayHeader.lightsPointer + 0x10, DirectionalLight.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                dirLights.Add(new DirectionalLight(type04Block, i));
            }

            return dirLights;
        }

        public List<Type0C> GetType0Cs()
        {
            var type0Cs = new List<Type0C>();
            if (gameplayHeader.soundPointer == 0) { return type0Cs; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.soundPointer, 4), 0);
            byte[] type0CBlock = ReadBlock(fileStream, gameplayHeader.soundPointer + 0x10, Type0C.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type0Cs.Add(new Type0C(type0CBlock, i));
            }

            return type0Cs;
        }

        public List<Sphere> GetSpheres()
        {
            List<Sphere> spheres = new List<Sphere>();
            if (gameplayHeader.spherePointer == 0) { return spheres; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.spherePointer, 4), 0);
            byte[] sphereBlock = ReadBlock(fileStream, gameplayHeader.spherePointer + 0x10, Sphere.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                spheres.Add(new Sphere(sphereBlock, i));
            }

            return spheres;
        }

        public List<Cylinder> GetCylinders()
        {
            List<Cylinder> cylinders = new List<Cylinder>();
            if (gameplayHeader.cylinderPointer == 0) { return cylinders; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.cylinderPointer, 4), 0);
            byte[] cylinderBlock = ReadBlock(fileStream, gameplayHeader.cylinderPointer + 0x10, Cylinder.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                cylinders.Add(new Cylinder(cylinderBlock, i));
            }

            return cylinders;
        }

        public List<Type88> GetType88s()
        {
            var type88s = new List<Type88>();
            if (gameplayHeader.type88Pointer == 0) { return type88s; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.type88Pointer, 4), 0);
            byte[] type88Block = ReadBlock(fileStream, gameplayHeader.type88Pointer + 0x10, Type88.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type88s.Add(new Type88(type88Block, i));
            }
            return type88s;
        }

        public List<Type4C> GetType4Cs()
        {
            List<Type4C> type4Cs = new List<Type4C>();
            if (gameplayHeader.type4CPointer == 0) { return type4Cs; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.type4CPointer, 4), 0);
            byte[] type4CBlock = ReadBlock(fileStream, gameplayHeader.type4CPointer + 0x10, Type4C.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type4Cs.Add(new Type4C(type4CBlock, i));
            }

            return type4Cs;
        }

        public List<Type7C> GetType7Cs()
        {
            var type7Cs = new List<Type7C>();
            if (gameplayHeader.type7CPointer == 0) { return type7Cs; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.type7CPointer, 4), 0);
            byte[] type7CBlock = ReadBlock(fileStream, gameplayHeader.type7CPointer + 0x10, Type7C.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type7Cs.Add(new Type7C(type7CBlock, i));
            }

            return type7Cs;
        }

        public List<Type80> GetType80()
        {
            var type80s = new List<Type80>();
            if (gameplayHeader.type80Pointer == 0) { return type80s; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.type80Pointer, 4), 0);
            byte[] headBlock = ReadBlock(fileStream, gameplayHeader.type80Pointer + 0x10, Type80.HEADSIZE * count);
            byte[] dataBlock = ReadBlock(fileStream, gameplayHeader.type80Pointer + 0x10 + Type80.HEADSIZE * count, Type80.DATASIZE * count);

            for (int i = 0; i < count; i++)
            {
                type80s.Add(new Type80(headBlock, dataBlock, i));
            }

            return type80s;
        }

        public byte[] GetUnk6()
        {
            if (gameplayHeader.mobyGroupsPointer == 0) { return null; }
            int count1 = ReadInt(ReadBlock(fileStream, gameplayHeader.mobyGroupsPointer + 0x00, 4), 0);
            int count2 = ReadInt(ReadBlock(fileStream, gameplayHeader.mobyGroupsPointer + 0x04, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.mobyGroupsPointer, count1 * 4 + count2 + 0x10);
        }

        public byte[] GetUnk7()
        {
            if (gameplayHeader.type4CPointer == 0) { return null; }
            int count1 = ReadInt(ReadBlock(fileStream, gameplayHeader.type4CPointer, 4), 0);
            int count2 = ReadInt(ReadBlock(fileStream, gameplayHeader.type4CPointer + 4, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.type4CPointer, count1 + count2 * 8 + 0x10);
        }

        public byte[] GetUnk12()
        {
            if (gameplayHeader.unkPointer12 == 0) { return null; }
            int count1 = ReadInt(ReadBlock(fileStream, gameplayHeader.unkPointer12, 4), 0);
            int count2 = ReadInt(ReadBlock(fileStream, gameplayHeader.unkPointer12 + 4, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.unkPointer12, count1 + count2 * 8 + 0x10);
        }

        public List<KeyValuePair<int, int>> GetType5Cs()
        {
            var keyValuePairs = new List<KeyValuePair<int, int>>();
            byte[] bytes;
            for (int i = 0; (bytes = ReadBlock(fileStream, gameplayHeader.type5CPointer + i * 8, 8))[0] != 0xFF; i++)
            {
                int id = ReadInt(bytes, 0);
                int value = ReadInt(bytes, 4);
                keyValuePairs.Add(new KeyValuePair<int, int>(id, value));
            }
            return keyValuePairs;
        }


        public List<KeyValuePair<int, int>> GetType50s()
        {
            var keyValuePairs = new List<KeyValuePair<int, int>>();
            byte[] bytes;
            for (int i = 0; (bytes = ReadBlock(fileStream, gameplayHeader.type50Pointer + i * 8, 8))[0] != 0xFF; i++)
            {
                int id = ReadInt(bytes, 0);
                int value = ReadInt(bytes, 4);
                keyValuePairs.Add(new KeyValuePair<int, int>(id, value));
            }
            return keyValuePairs;
        }


        public byte[] GetUnk13()
        {
            int sectionLength;

            switch (game.num)
            {
                case 1:
                    sectionLength = gameplayHeader.occlusionPointer - gameplayHeader.grindPathsPointer;
                    break;
                case 2:
                case 3:
                case 4:
                default:
                    sectionLength = gameplayHeader.areasPointer - gameplayHeader.grindPathsPointer;
                    break;
            }

            if (sectionLength > 0)
            {
                return ReadBlock(fileStream, gameplayHeader.grindPathsPointer, sectionLength);
            }
            else
            {
                return null;
            }

        }

        public byte[] GetUnk14()
        {
            if (gameplayHeader.unkPointer14 == 0) { return null; }
            int sectionLength = ReadInt(ReadBlock(fileStream, gameplayHeader.unkPointer14, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.unkPointer14, sectionLength);
        }

        public byte[] GetUnk16()
        {
            if (gameplayHeader.unkPointer16 == 0) { return null; }
            return ReadBlock(fileStream, gameplayHeader.unkPointer16, gameplayHeader.grindPathsPointer - gameplayHeader.unkPointer16);
        }

        public byte[] GetUnk17()
        {
            if (gameplayHeader.unkPointer17 == 0) { return null; }
            int sectionLength = ReadInt(ReadBlock(fileStream, gameplayHeader.unkPointer17, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.unkPointer17, sectionLength);
        }

        public byte[] GetUnk18()
        {
            if (gameplayHeader.unkPointer18 == 0) { return null; }
            int amount = ReadInt(ReadBlock(fileStream, gameplayHeader.unkPointer18, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.unkPointer18, 0x10 + amount * 0x20);
        }

        public List<int> GetMobyIds()
        {
            if (gameplayHeader.mobyIdPointer == 0) { return null; }

            int count = BitConverter.ToInt32(ReadBlock(fileStream, gameplayHeader.mobyIdPointer, 4), 0);

            byte[] mobyIdBlock = ReadBlock(fileStream, gameplayHeader.mobyIdPointer + 0x04, count * 0x04);

            var mobyIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                mobyIds.Add(BitConverter.ToInt32(mobyIdBlock, (i * 0x04)));
            }

            return mobyIds;
        }

        public List<int> GetTieIds()
        {
            if (gameplayHeader.tieIdPointer == 0) { return null; }

            int count = BitConverter.ToInt32(ReadBlock(fileStream, gameplayHeader.tieIdPointer, 4), 0);

            byte[] tieIdBlock = ReadBlock(fileStream, gameplayHeader.tieIdPointer + 0x04, count * 0x04);

            var tieIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                tieIds.Add(BitConverter.ToInt32(tieIdBlock, (i * 0x04)));
            }

            return tieIds;
        }

        public List<int> GetShrubIds()
        {
            if (gameplayHeader.shrubIdPointer == 0) { return null; }

            int count = BitConverter.ToInt32(ReadBlock(fileStream, gameplayHeader.shrubIdPointer, 4), 0);

            byte[] shrubIdBlock = ReadBlock(fileStream, gameplayHeader.shrubIdPointer + 0x04, count * 0x04);

            var shrubIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                shrubIds.Add(BitConverter.ToInt32(shrubIdBlock, (i * 0x04)));
            }

            return shrubIds;
        }

        public OcclusionData GetOcclusionData()
        {
            if (gameplayHeader.occlusionPointer == 0) { return null; }

            byte[] headBlock = ReadBlock(fileStream, gameplayHeader.occlusionPointer, 0x10);
            OcclusionDataHeader head = new OcclusionDataHeader(headBlock);

            byte[] occlusionBlock = ReadBlock(fileStream, gameplayHeader.occlusionPointer + 0x10, head.totalCount * 0x08);
            OcclusionData data = new OcclusionData(occlusionBlock, head);


            return data;
        }

        //In RaC 2/3 each element should be of size 0x60 but the block is much larger and not a multiple of that
        //Hence we just load the block based on pointers
        public byte[] GetTieData(int tieCount)
        {
            if (gameplayHeader.tiePointer == 0) { return null; }

            switch (game.num)
            {
                case 1:
                    return ReadBlock(fileStream, gameplayHeader.tiePointer, 0x10 + 0xE0 * tieCount);
                case 2:
                case 3:
                case 4:
                default:
                    return ReadBlock(fileStream, gameplayHeader.tiePointer, gameplayHeader.tieGroupsPointer - gameplayHeader.tiePointer);
            }
        }


        public byte[] GetShrubData(int shrubCount)
        {
            if (gameplayHeader.shrubPointer == 0) { return null; }
            return ReadBlock(fileStream, gameplayHeader.shrubPointer, 0x10 + 0x70 * shrubCount);
        }

        public byte[] GetTieGroups()
        {
            if (gameplayHeader.tieGroupsPointer == 0) { return null; }
            byte[] header = ReadBlock(fileStream, gameplayHeader.tieGroupsPointer, 16);
            int count = ReadInt(header, 0);
            int length = ReadInt(header, 4);
            return ReadBlock(fileStream, gameplayHeader.tieGroupsPointer, 0x10 + length + count * 0x4);
        }

        public byte[] GetShrubGroups()
        {
            if (gameplayHeader.shrubGroupsPointer == 0) { return null; }
            byte[] header = ReadBlock(fileStream, gameplayHeader.shrubGroupsPointer, 16);
            int count = ReadInt(header, 0);
            int length = ReadInt(header, 4);
            return ReadBlock(fileStream, gameplayHeader.shrubGroupsPointer, 0x10 + length + count * 0x4);
        }

        public byte[] GetAreasData()
        {
            if (gameplayHeader.areasPointer == 0) { return null; }
            return ReadBlock(fileStream, gameplayHeader.areasPointer, gameplayHeader.occlusionPointer - gameplayHeader.areasPointer);
        }

        public List<byte[]> GetPvars(List<Moby> mobs)
        {
            int pvarCount = 0;
            foreach (Moby mob in mobs)
            {
                if (mob.pvarIndex > pvarCount)
                {
                    pvarCount = mob.pvarIndex;
                }
            }

            pvarCount++;

            var pVars = new List<byte[]>();

            byte[] pVarHeadBlock = ReadBlock(fileStream, gameplayHeader.pvarSizePointer, pvarCount * 8);
            uint pVarSectionLength = 0;
            for (int i = 0; i < pvarCount; i++)
            {
                pVarSectionLength += ReadUint(pVarHeadBlock, (i * 8) + 0x04);
            }

            byte[] pVarBlock = ReadBlock(fileStream, gameplayHeader.pvarPointer, (int) pVarSectionLength);
            for (int i = 0; i < pvarCount; i++)
            {
                uint mobpVarsStart = ReadUint(pVarHeadBlock, (i * 8));
                uint mobpVarsCount = ReadUint(pVarHeadBlock, (i * 8) + 0x04);
                byte[] mobpVars = GetBytes(pVarBlock, (int) mobpVarsStart, (int) mobpVarsCount);
                pVars.Add(mobpVars);
            }

            return pVars;
        }


        public void Close()
        {
            fileStream.Close();
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
