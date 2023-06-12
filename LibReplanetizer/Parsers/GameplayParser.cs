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
            if (gameplayHeader.splinePointer == 0) { return new List<Spline>(); }

            var splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(fileStream, gameplayHeader.splinePointer, 4), 0);
            int splineOffset = ReadInt(ReadBlock(fileStream, gameplayHeader.splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(fileStream, gameplayHeader.splinePointer + 8, 4), 0);

            byte[] splineHeadBlock = ReadBlock(fileStream, gameplayHeader.splinePointer + 0x10, splineCount * 0x04);
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

        public List<LanguageData> GetLang(int offset)
        {
            if (offset == 0) { return new List<LanguageData>(); }

            byte[] langHeader = ReadBlock(fileStream, offset, 0x08);
            int numItems = ReadInt(langHeader, 0x00);
            int langLength = ReadInt(langHeader, 0x04);


            byte[] langBlock = ReadBlock(fileStream, offset, langLength);

            var languageData = new List<LanguageData>();

            for (int i = 0; i < numItems; i++)
            {
                languageData.Add(new LanguageData(langBlock, i));
            }

            return languageData;
        }

        public List<LanguageData> GetEnglish()
        {
            return GetLang(gameplayHeader.englishPointer);
        }

        public List<LanguageData> GetUkEnglish()
        {
            return GetLang(gameplayHeader.ukenglishPointer);
        }

        public List<LanguageData> GetFrench()
        {
            return GetLang(gameplayHeader.frenchPointer);
        }

        public List<LanguageData> GetGerman()
        {
            return GetLang(gameplayHeader.germanPointer);
        }

        public List<LanguageData> GetSpanish()
        {
            return GetLang(gameplayHeader.spanishPointer);
        }

        public List<LanguageData> GetItalian()
        {
            return GetLang(gameplayHeader.italianPointer);
        }

        public List<LanguageData> GetJapanese()
        {
            return GetLang(gameplayHeader.japanesePointer);
        }

        public List<LanguageData> GetKorean()
        {
            return GetLang(gameplayHeader.koreanPointer);
        }


        // TODO consolidate all these into a single function, as they work pretty much the same
        public List<Moby> GetMobies(List<Model> mobyModels, List<byte[]> pVars)
        {
            var mobs = new List<Moby>();

            if (gameplayHeader.mobyPointer == 0) { return mobs; }

            int mobyCount = ReadInt(ReadBlock(fileStream, gameplayHeader.mobyPointer, 4), 0);

            byte[] mobyBlock = ReadBlock(fileStream, gameplayHeader.mobyPointer + 0x10, game.mobyElemSize * mobyCount);
            for (int i = 0; i < mobyCount; i++)
            {
                mobs.Add(new Moby(game, mobyBlock, i, mobyModels, pVars));
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

        public List<SoundInstance> GetSoundInstances()
        {
            List<SoundInstance> soundInstances = new List<SoundInstance>();
            if (gameplayHeader.soundPointer == 0) { return soundInstances; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.soundPointer, 4), 0);
            byte[] soundInstanceBlock = ReadBlock(fileStream, gameplayHeader.soundPointer + 0x10, SoundInstance.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                soundInstances.Add(new SoundInstance(soundInstanceBlock, i));
            }

            return soundInstances;
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

        public List<GlobalPvarBlock> GetType4Cs()
        {
            List<GlobalPvarBlock> type4Cs = new List<GlobalPvarBlock>();
            if (gameplayHeader.globalPvarPointer == 0) { return type4Cs; }

            int offset = ReadInt(ReadBlock(fileStream, gameplayHeader.globalPvarPointer, 4), 0);
            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.globalPvarPointer + 4, 4), 0);
            byte[] type4CBlock = ReadBlock(fileStream, gameplayHeader.globalPvarPointer + 0x10 + offset, GlobalPvarBlock.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type4Cs.Add(new GlobalPvarBlock(type4CBlock, i));
            }

            return type4Cs;
        }

        public List<EnvTransition> GetEnvTransitions()
        {
            List<EnvTransition> envTransitions = new List<EnvTransition>();
            if (gameplayHeader.envTransitionsPointer == 0) { return envTransitions; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.envTransitionsPointer, 4), 0);
            byte[] headBlock = ReadBlock(fileStream, gameplayHeader.envTransitionsPointer + 0x10, EnvTransition.HEADSIZE * count);
            byte[] mainBlock = ReadBlock(fileStream, gameplayHeader.envTransitionsPointer + 0x10 + EnvTransition.HEADSIZE * count, EnvTransition.ELEMENTSIZE * count);

            for (int i = 0; i < count; i++)
            {
                envTransitions.Add(new EnvTransition(headBlock, mainBlock, i));
            }

            return envTransitions;
        }

        public byte[] GetUnk6()
        {
            if (gameplayHeader.mobyGroupsPointer == 0) { return new byte[0]; }
            int count1 = ReadInt(ReadBlock(fileStream, gameplayHeader.mobyGroupsPointer + 0x00, 4), 0);
            int count2 = ReadInt(ReadBlock(fileStream, gameplayHeader.mobyGroupsPointer + 0x04, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.mobyGroupsPointer, count1 * 4 + count2 + 0x10);
        }

        public byte[] GetUnk7()
        {
            if (gameplayHeader.globalPvarPointer == 0) { return new byte[0]; }
            int count1 = ReadInt(ReadBlock(fileStream, gameplayHeader.globalPvarPointer, 4), 0);
            int count2 = ReadInt(ReadBlock(fileStream, gameplayHeader.globalPvarPointer + 4, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.globalPvarPointer, count1 + count2 * 8 + 0x10);
        }

        public List<Pill> GetPills()
        {
            List<Pill> pills = new List<Pill>();
            if (gameplayHeader.pillPointer == 0) { return pills; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.pillPointer, 4), 0);
            byte[] pillBlock = ReadBlock(fileStream, gameplayHeader.pillPointer + 0x10, Pill.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                pills.Add(new Pill(pillBlock, i));
            }

            return pills;
        }

        public List<KeyValuePair<int, int>> GetType5Cs()
        {
            var keyValuePairs = new List<KeyValuePair<int, int>>();
            byte[] bytes;
            for (int i = 0; (bytes = ReadBlock(fileStream, gameplayHeader.pvarRewirePointer + i * 8, 8))[0] != 0xFF; i++)
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
            for (int i = 0; (bytes = ReadBlock(fileStream, gameplayHeader.pvarScratchPadPointer + i * 8, 8))[0] != 0xFF; i++)
            {
                int id = ReadInt(bytes, 0);
                int value = ReadInt(bytes, 4);
                keyValuePairs.Add(new KeyValuePair<int, int>(id, value));
            }
            return keyValuePairs;
        }


        public List<GrindPath> GetGrindPaths()
        {
            List<GrindPath> grindPaths = new List<GrindPath>();
            if (gameplayHeader.grindPathsPointer == 0) { return grindPaths; }

            byte[] head = ReadBlock(fileStream, gameplayHeader.grindPathsPointer, 0x10);
            int count = ReadInt(head, 0x00);
            int splineOffset = ReadInt(head, 0x04);
            int splineSize = ReadInt(head, 0x08);

            byte[] grindPathBlock = ReadBlock(fileStream, gameplayHeader.grindPathsPointer + 0x10, count * GrindPath.ELEMENTSIZE);
            byte[] splineHeadBlock = ReadBlock(fileStream, gameplayHeader.grindPathsPointer + 0x10 + count * GrindPath.ELEMENTSIZE, count * 0x04);
            byte[] splineBlock = ReadBlock(fileStream, gameplayHeader.grindPathsPointer + splineOffset, splineSize);

            List<Spline> splines = new List<Spline>();

            for (int i = 0; i < count; i++)
            {
                int offset = ReadInt(splineHeadBlock, i * 0x04);
                splines.Add(new Spline(splineBlock, offset));
            }

            for (int i = 0; i < count; i++)
            {
                grindPaths.Add(new GrindPath(grindPathBlock, i, splines[i]));
            }

            return grindPaths;
        }

        public List<PointLight> GetPointLights()
        {
            List<PointLight> pointLights = new List<PointLight>();
            if (gameplayHeader.pointLightPointer == 0) { return pointLights; }

            // Pointlights are not used in the PS3 remasters, only RaC 1 contains valid information.
            if (game != GameType.RaC1) { return pointLights; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.pointLightPointer, 4), 0);
            byte[] pointLightBlock = ReadBlock(fileStream, gameplayHeader.pointLightPointer + 0x10, PointLight.GetElementSize(game) * count);
            for (int i = 0; i < count; i++)
            {
                pointLights.Add(new PointLight(game, pointLightBlock, i));
            }

            return pointLights;
        }

        public byte[] GetUnk14()
        {
            if (gameplayHeader.pointLightGridPointer == 0) { return new byte[0]; }
            int sectionLength = ReadInt(ReadBlock(fileStream, gameplayHeader.pointLightGridPointer, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.pointLightGridPointer, sectionLength);
        }

        public byte[] GetUnk17()
        {
            if (gameplayHeader.camCollisionPointer == 0) { return new byte[0]; }
            int sectionLength = ReadInt(ReadBlock(fileStream, gameplayHeader.camCollisionPointer, 4), 0);
            return ReadBlock(fileStream, gameplayHeader.camCollisionPointer, sectionLength);
        }

        public List<EnvSample> GetEnvSamples()
        {
            List<EnvSample> envSamples = new List<EnvSample>();
            if (gameplayHeader.envSamplesPointer == 0) { return envSamples; }

            int count = ReadInt(ReadBlock(fileStream, gameplayHeader.envSamplesPointer, 4), 0);
            Byte[] envSamplesBlock = ReadBlock(fileStream, gameplayHeader.envSamplesPointer + 0x10, count * EnvSample.GetElementSize(game));
            for (int i = 0; i < count; i++)
            {
                envSamples.Add(new EnvSample(game, envSamplesBlock, i));
            }

            return envSamples;
        }

        public List<int> GetMobyIds()
        {
            if (gameplayHeader.mobyIdPointer == 0) { return new List<int>(); }

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
            if (gameplayHeader.tieIdPointer == 0) { return new List<int>(); }

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
            if (gameplayHeader.shrubIdPointer == 0) { return new List<int>(); }

            int count = BitConverter.ToInt32(ReadBlock(fileStream, gameplayHeader.shrubIdPointer, 4), 0);

            byte[] shrubIdBlock = ReadBlock(fileStream, gameplayHeader.shrubIdPointer + 0x04, count * 0x04);

            var shrubIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                shrubIds.Add(BitConverter.ToInt32(shrubIdBlock, (i * 0x04)));
            }

            return shrubIds;
        }

        public OcclusionData? GetOcclusionData()
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
            if (gameplayHeader.tiePointer == 0) { return new byte[0]; }

            if (game == GameType.RaC1)
            {
                return ReadBlock(fileStream, gameplayHeader.tiePointer, 0x10 + 0xE0 * tieCount);
            }
            else
            {
                return ReadBlock(fileStream, gameplayHeader.tiePointer, gameplayHeader.tieGroupsPointer - gameplayHeader.tiePointer);
            }
        }


        public byte[] GetShrubData(int shrubCount)
        {
            if (gameplayHeader.shrubPointer == 0) { return new byte[0]; }
            return ReadBlock(fileStream, gameplayHeader.shrubPointer, 0x10 + 0x70 * shrubCount);
        }

        public byte[] GetTieGroups()
        {
            if (gameplayHeader.tieGroupsPointer == 0) { return new byte[0]; }
            byte[] header = ReadBlock(fileStream, gameplayHeader.tieGroupsPointer, 16);
            int count = ReadInt(header, 0);
            int length = ReadInt(header, 4);
            return ReadBlock(fileStream, gameplayHeader.tieGroupsPointer, 0x10 + length + count * 0x4);
        }

        public byte[] GetShrubGroups()
        {
            if (gameplayHeader.shrubGroupsPointer == 0) { return new byte[0]; }
            byte[] header = ReadBlock(fileStream, gameplayHeader.shrubGroupsPointer, 16);
            int count = ReadInt(header, 0);
            int length = ReadInt(header, 4);
            return ReadBlock(fileStream, gameplayHeader.shrubGroupsPointer, 0x10 + length + count * 0x4);
        }

        public byte[] GetAreasData()
        {
            if (gameplayHeader.areasPointer == 0) { return new byte[0]; }
            return ReadBlock(fileStream, gameplayHeader.areasPointer, gameplayHeader.occlusionPointer - gameplayHeader.areasPointer);
        }

        public List<byte[]> GetPvars()
        {
            var pVars = new List<byte[]>();

            byte[] pVarSizes = ReadBlock(fileStream, gameplayHeader.pvarSizePointer, gameplayHeader.pvarPointer - gameplayHeader.pvarSizePointer);

            // Like this because padding is a thing
            int pVarSizeBlockSize = ReadInt(pVarSizes, pVarSizes.Length - 0x08) + ReadInt(pVarSizes, pVarSizes.Length - 0x04);
            if (pVarSizeBlockSize == 0)
            {
                pVarSizeBlockSize = ReadInt(pVarSizes, pVarSizes.Length - 0x10) + ReadInt(pVarSizes, pVarSizes.Length - 0xC);
            }

            int pvarCount = pVarSizes.Length / 0x08;

            byte[] pVarBlock = ReadBlock(fileStream, gameplayHeader.pvarPointer, pVarSizeBlockSize);
            for (int i = 0; i < pvarCount; i++)
            {
                uint mobpVarsStart = ReadUint(pVarSizes, (i * 8));
                uint mobpVarsCount = ReadUint(pVarSizes, (i * 8) + 0x04);
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
