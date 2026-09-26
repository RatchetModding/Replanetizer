// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Serializers.SerializerFunctions;

namespace LibReplanetizer.Serializers
{
    public class GameplaySerializer
    {
        public const int MOBYLENGTH = 0x78;

        public void Save(Level level, string directory)
        {
            directory = Path.Join(directory, "gameplay_ntsc");
            FileStream fs = ReplanetizerFileStream.Open(directory, FileMode.Create, FileAccess.Write);

            switch (level.game.num)
            {
                case 1:
                    SaveRC1(level, fs);
                    break;
                case 2:
                    SaveRC2(level, fs);
                    break;
                case 3:
                    SaveRC3(level, fs);
                    break;
                case 4:
                    SaveRC4(level, fs);
                    break;
            }

            fs.Close();
        }

        private void SaveRC1(Level level, FileStream fs)
        {
            //Seek past the header
            fs.Seek(0xA0, SeekOrigin.Begin);

            GameplayHeader gameplayHeader = new GameplayHeader
            {
                envSamplesPointer = SeekWrite(fs, SerializeLevelObjects(level.envSamples, EnvSample.GetElementSize(GameType.RaC1))),
                levelVarPointer = SeekWrite(fs, level.levelVariables.Serialize(level.game)),
                englishPointer = SeekWrite(fs, GetLangBytes(level.english)),
                ukenglishPointer = SeekWrite(fs, GetLangBytes(level.ukenglish)),
                frenchPointer = SeekWrite(fs, GetLangBytes(level.french)),
                germanPointer = SeekWrite(fs, GetLangBytes(level.german)),
                spanishPointer = SeekWrite(fs, GetLangBytes(level.spanish)),
                italianPointer = SeekWrite(fs, GetLangBytes(level.italian)),
                japanesePointer = SeekWrite(fs, GetLangBytes(level.japanese)),
                koreanPointer = SeekWrite(fs, GetLangBytes(level.korean)),
                lightsPointer = SeekWrite(fs, SerializeLevelObjects(level.directionalLights, DirectionalLight.ELEMENTSIZE)),
                envTransitionsPointer = SeekWrite(fs, GetEnvTransitionBytes(level.envTransitions)),
                cameraPointer = SeekWrite(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE)),
                soundPointer = SeekWrite(fs, SerializeLevelObjects(level.soundInstances, SoundInstance.ELEMENTSIZE)),
                mobyIdPointer = SeekWrite(fs, GetIdBytes(level.mobyIds)),
                mobyPointer = SeekWrite(fs, GetMobyBytes(level.mobs, level.game)),
                pvarSizePointer = SeekWrite(fs, GetPvarSizeBytes(level.pVars)),
                pvarPointer = SeekWrite(fs, GetPvarBytes(level.pVars)),
                pvarScratchPadPointer = SeekWrite(fs, GetPvarScratchPadBytes(level.pvarScratchPads)),
                pvarRewirePointer = SeekWrite(fs, GetPvarRewireBytes(level.pvarRewires)),
                mobyGroupsPointer = SeekWrite(fs, level.unk6),
                globalPvarPointer = SeekWrite(fs, GetPvarBlocksBytes(level.pvarBlocks, level.pvarBlocksHeaderPadding)),
                tieIdPointer = SeekWrite(fs, GetIdBytes(level.tieIds)),
                tiePointer = SeekWrite(fs, level.tieData),
                shrubIdPointer = SeekWrite(fs, GetIdBytes(level.shrubIds)),
                shrubPointer = SeekWrite(fs, level.shrubData),
                splinePointer = SeekWrite(fs, GetSplineBytes(level.splines)),
                cuboidPointer = SeekWrite(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE)),
                spherePointer = SeekWrite(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE)),
                cylinderPointer = SeekWrite(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE)),
                pillPointer = SeekWrite(fs, SerializeLevelObjects(level.pills, Pill.ELEMENTSIZE)),
                camCollisionPointer = SeekWrite(fs, level.unk17),
                pointLightPointer = SeekWrite(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.RaC1))),
                pointLightGridPointer = SeekWrite(fs, level.unk14),
                grindPathsPointer = SeekWrite(fs, GetGrindPathsBytes(level.grindPaths)),
                occlusionPointer = SeekWrite(fs, GetOcclusionBytes(level.occlusionData), 0x40)
            };

            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.Serialize(level.game);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        private void SaveRC2(Level level, FileStream fs)
        {
            //Seek past the header
            fs.Seek(0xA0, SeekOrigin.Begin);

            GameplayHeader gameplayHeader = new GameplayHeader
            {
                envSamplesPointer = SeekWrite(fs, SerializeLevelObjects(level.envSamples, EnvSample.GetElementSize(GameType.RaC2))),
                levelVarPointer = SeekWrite(fs, level.levelVariables.Serialize(level.game)),
                englishPointer = SeekWrite(fs, GetLangBytes(level.english)),
                ukenglishPointer = SeekWrite(fs, GetLangBytes(level.ukenglish)),
                frenchPointer = SeekWrite(fs, GetLangBytes(level.french)),
                germanPointer = SeekWrite(fs, GetLangBytes(level.german)),
                spanishPointer = SeekWrite(fs, GetLangBytes(level.spanish)),
                italianPointer = SeekWrite(fs, GetLangBytes(level.italian)),
                japanesePointer = SeekWrite(fs, GetLangBytes(level.japanese)),
                koreanPointer = SeekWrite(fs, GetLangBytes(level.korean)),
                lightsPointer = SeekWrite(fs, SerializeLevelObjects(level.directionalLights, DirectionalLight.ELEMENTSIZE)),
                envTransitionsPointer = SeekWrite(fs, GetEnvTransitionBytes(level.envTransitions)),
                cameraPointer = SeekWrite(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE)),
                soundPointer = SeekWrite(fs, SerializeLevelObjects(level.soundInstances, SoundInstance.ELEMENTSIZE)),
                mobyIdPointer = SeekWrite(fs, GetIdBytes(level.mobyIds)),
                mobyPointer = SeekWrite(fs, GetMobyBytes(level.mobs, level.game)),
                pvarSizePointer = SeekWrite(fs, GetPvarSizeBytes(level.pVars)),
                pvarPointer = SeekWrite(fs, GetPvarBytes(level.pVars)),
                pvarScratchPadPointer = SeekWrite(fs, GetPvarScratchPadBytes(level.pvarScratchPads)),
                pvarRewirePointer = SeekWrite(fs, GetPvarRewireBytes(level.pvarRewires)),
                mobyGroupsPointer = SeekWrite(fs, level.unk6),
                globalPvarPointer = SeekWrite(fs, level.unk7),
                tieIdPointer = SeekWrite(fs, GetIdBytes(level.tieIds)),
                tiePointer = SeekWrite(fs, level.tieData),
                tieGroupsPointer = SeekWrite(fs, level.tieGroupData),
                shrubIdPointer = SeekWrite(fs, GetIdBytes(level.shrubIds)),
                shrubPointer = SeekWrite(fs, level.shrubData),
                shrubGroupsPointer = SeekWrite(fs, level.shrubGroupData),
                splinePointer = SeekWrite(fs, GetSplineBytes(level.splines)),
                cuboidPointer = SeekWrite(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE)),
                spherePointer = SeekWrite(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE)),
                cylinderPointer = SeekWrite(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE)),
                pillPointer = SeekWrite(fs, SerializeLevelObjects(level.pills, Pill.ELEMENTSIZE)),
                camCollisionPointer = SeekWrite(fs, level.unk17),
                pointLightPointer = SeekWrite(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.RaC2)).Concat(new byte[2048]).ToArray()), // The game appends 2048 padding bytes!
                grindPathsPointer = SeekWrite(fs, GetGrindPathsBytes(level.grindPaths)),
                areasPointer = SeekWrite(fs, level.areasData),
                occlusionPointer = SeekWrite(fs, GetOcclusionBytes(level.occlusionData))
            };

            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.Serialize(level.game);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        private void SaveRC3(Level level, FileStream fs)
        {
            //Seek past the header
            fs.Seek(0xA0, SeekOrigin.Begin);

            GameplayHeader gameplayHeader = new GameplayHeader
            {
                envSamplesPointer = SeekWrite(fs, SerializeLevelObjects(level.envSamples, EnvSample.GetElementSize(GameType.RaC3)), 0x04),
                levelVarPointer = SeekWrite(fs, level.levelVariables.Serialize(level.game), 0x04),
                englishPointer = SeekWrite(fs, GetLangBytes(level.english), 0x04),
                ukenglishPointer = SeekWrite(fs, GetLangBytes(level.ukenglish), 0x04),
                frenchPointer = SeekWrite(fs, GetLangBytes(level.french), 0x04),
                germanPointer = SeekWrite(fs, GetLangBytes(level.german), 0x04),
                spanishPointer = SeekWrite(fs, GetLangBytes(level.spanish), 0x04),
                italianPointer = SeekWrite(fs, GetLangBytes(level.italian), 0x04),
                japanesePointer = SeekWrite(fs, GetLangBytes(level.japanese), 0x04),
                koreanPointer = SeekWrite(fs, GetLangBytes(level.korean), 0x04),
                lightsPointer = SeekWrite(fs, SerializeLevelObjects(level.directionalLights, DirectionalLight.ELEMENTSIZE), 0x04),
                envTransitionsPointer = SeekWrite(fs, GetEnvTransitionBytes(level.envTransitions), 0x04),
                cameraPointer = SeekWrite(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE), 0x04),
                soundPointer = SeekWrite(fs, SerializeLevelObjects(level.soundInstances, SoundInstance.ELEMENTSIZE), 0x04),
                mobyIdPointer = SeekWrite(fs, GetIdBytes(level.mobyIds), 0x04),
                mobyPointer = SeekWrite(fs, GetMobyBytes(level.mobs, level.game), 0x04),
                pvarSizePointer = SeekWrite(fs, GetPvarSizeBytes(level.pVars), 0x04),
                pvarPointer = SeekWrite(fs, GetPvarBytes(level.pVars)),
                pvarScratchPadPointer = SeekWrite(fs, GetPvarScratchPadBytes(level.pvarScratchPads), 0x04),
                pvarRewirePointer = SeekWrite(fs, GetPvarRewireBytes(level.pvarRewires), 0x04),
                mobyGroupsPointer = SeekWrite(fs, level.unk6, 0x04),
                globalPvarPointer = SeekWrite(fs, level.unk7, 0x04),
                tieIdPointer = SeekWrite(fs, GetIdBytes(level.tieIds), 0x04),
                tiePointer = SeekWrite(fs, level.tieData, 0x04),
                tieGroupsPointer = SeekWrite(fs, level.tieGroupData, 0x04),
                shrubIdPointer = SeekWrite(fs, GetIdBytes(level.shrubIds), 0x04),
                shrubPointer = SeekWrite(fs, level.shrubData, 0x04),
                shrubGroupsPointer = SeekWrite(fs, level.shrubGroupData, 0x04),
                splinePointer = SeekWrite(fs, GetSplineBytes(level.splines), 0x04),
                cuboidPointer = SeekWrite(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE), 0x04),
                spherePointer = SeekWrite(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE), 0x04),
                cylinderPointer = SeekWrite(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE), 0x04),
                pillPointer = SeekWrite(fs, SerializeLevelObjects(level.pills, Pill.ELEMENTSIZE), 0x04),
                camCollisionPointer = SeekWrite(fs, level.unk17, 0x04),
                pointLightPointer = SeekWrite(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.RaC3)), 0x04),
                grindPathsPointer = SeekWrite(fs, GetGrindPathsBytes(level.grindPaths), 0x04),
                areasPointer = SeekWrite(fs, level.areasData, 0x04),
                occlusionPointer = SeekWrite(fs, GetOcclusionBytes(level.occlusionData), 0x04)
            };

            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.Serialize(level.game);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        private void SaveRC4(Level level, FileStream fs)
        {
            //Seek past the header
            fs.Seek(0x90, SeekOrigin.Begin);

            GameplayHeader gameplayHeader = new GameplayHeader
            {
                levelVarPointer = SeekWrite(fs, level.levelVariables.Serialize(level.game), 0x04),
                englishPointer = SeekWrite(fs, GetLangBytes(level.english), 0x04),
                ukenglishPointer = SeekWrite(fs, GetLangBytes(level.ukenglish), 0x04),
                frenchPointer = SeekWrite(fs, GetLangBytes(level.french), 0x04),
                germanPointer = SeekWrite(fs, GetLangBytes(level.german), 0x04),
                spanishPointer = SeekWrite(fs, GetLangBytes(level.spanish), 0x04),
                italianPointer = SeekWrite(fs, GetLangBytes(level.italian), 0x04),
                japanesePointer = SeekWrite(fs, GetLangBytes(level.japanese), 0x04),
                koreanPointer = SeekWrite(fs, GetLangBytes(level.korean), 0x04),
                lightsPointer = SeekWrite(fs, SerializeLevelObjects(level.directionalLights, DirectionalLight.ELEMENTSIZE), 0x04),
                envTransitionsPointer = SeekWrite(fs, GetEnvTransitionBytes(level.envTransitions), 0x04),
                cameraPointer = SeekWrite(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE), 0x04),
                soundPointer = SeekWrite(fs, SerializeLevelObjects(level.soundInstances, SoundInstance.ELEMENTSIZE), 0x04),
                mobyIdPointer = SeekWrite(fs, GetIdBytes(level.mobyIds), 0x04),
                mobyPointer = SeekWrite(fs, GetMobyBytes(level.mobs, level.game), 0x04),
                pvarSizePointer = SeekWrite(fs, GetPvarSizeBytes(level.pVars), 0x04),
                pvarPointer = SeekWrite(fs, GetPvarBytes(level.pVars), 0x04),
                pvarScratchPadPointer = SeekWrite(fs, GetPvarScratchPadBytes(level.pvarScratchPads), 0x04),
                pvarRewirePointer = SeekWrite(fs, GetPvarRewireBytes(level.pvarRewires), 0x04),
                mobyGroupsPointer = SeekWrite(fs, level.unk6, 0x04),
                globalPvarPointer = SeekWrite(fs, level.unk7, 0x04),
                tieIdPointer = SeekWrite(fs, GetIdBytes(level.tieIds), 0x04),
                tiePointer = SeekWrite(fs, level.tieData, 0x04),
                tieGroupsPointer = SeekWrite(fs, level.tieGroupData, 0x04),
                shrubIdPointer = SeekWrite(fs, GetIdBytes(level.shrubIds), 0x04),
                shrubPointer = SeekWrite(fs, level.shrubData, 0x04),
                shrubGroupsPointer = SeekWrite(fs, level.shrubGroupData, 0x04),
                splinePointer = SeekWrite(fs, GetSplineBytes(level.splines), 0x04),
                cuboidPointer = SeekWrite(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE), 0x04),
                spherePointer = SeekWrite(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE), 0x04),
                cylinderPointer = SeekWrite(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE), 0x04),
                pillPointer = SeekWrite(fs, SerializeLevelObjects(level.pills, Pill.ELEMENTSIZE), 0x04),
                camCollisionPointer = SeekWrite(fs, level.unk17, 0x04),
                pointLightPointer = SeekWrite(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.DL)), 0x04),
                grindPathsPointer = SeekWrite(fs, GetGrindPathsBytes(level.grindPaths), 0x04),
                areasPointer = SeekWrite(fs, level.areasData, 0x04),
                occlusionPointer = SeekWrite(fs, GetOcclusionBytes(level.occlusionData), 0x04)
            };

            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.Serialize(level.game);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);
        }

        public static byte[] GetLangBytes(List<LanguageData> languageData)
        {
            int headerSize = (languageData.Count * 16) + 8;
            int dataSize = 0;
            foreach (LanguageData entry in languageData)
            {
                int entrySize = entry.text.Length + 1;
                if (entrySize % 4 != 0)
                {
                    entrySize += (4 - entrySize % 4);
                }
                dataSize += entrySize;
            }

            int totalSize = headerSize + dataSize;
            byte[] bytes = new byte[totalSize];

            WriteUint(bytes, 0, (uint) languageData.Count);
            WriteUint(bytes, 4, (uint) totalSize);

            int textPos = headerSize;
            int headerPos = 8;

            foreach (LanguageData entry in languageData)
            {
                int entrySize = entry.text.Length + 1;
                if (entrySize % 4 != 0)
                {
                    entrySize += 4 - (entrySize % 4);
                }

                entry.text.CopyTo(bytes, textPos);

                WriteUint(bytes, headerPos, (uint) textPos);
                WriteUint(bytes, headerPos + 4, (uint) entry.id);
                WriteInt(bytes, headerPos + 8, entry.secondId);
                WriteUint(bytes, headerPos + 12, 0);
                headerPos += 16;
                textPos += entrySize;
            }

            return bytes;
        }

        public byte[] GetMobyBytes(List<Moby> mobs, GameType game)
        {
            if (mobs == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + mobs.Count * game.mobyElemSize];

            //Header
            WriteUint(bytes, 0, (uint) mobs.Count);
            WriteUint(bytes, 4, 0x100);

            for (int i = 0; i < mobs.Count; i++)
            {
                mobs[i].ToByteArray().CopyTo(bytes, 0x10 + i * game.mobyElemSize);
            }

            return bytes;
        }

        public byte[] SerializeLevelObjects<T>(List<T> levelobjects, int elementSize) where T : LevelObject
        {
            if (levelobjects == null) return [];

            byte[] bytes = new byte[0x10 + levelobjects.Count * elementSize];

            //Header
            WriteInt(bytes, 0x00, levelobjects.Count);

            for (int i = 0; i < levelobjects.Count; i++)
            {
                levelobjects[i].ToByteArray().CopyTo(bytes, 0x10 + i * elementSize);
            }

            return bytes;
        }

        public byte[] GetGrindPathsBytes(List<GrindPath> grindPaths)
        {
            if (grindPaths == null) return [];

            List<byte> splineData = new List<byte>();
            List<int> offsets = new List<int>();

            int offset = 0;
            for (int i = 0; i < grindPaths.Count; i++)
            {
                byte[] splineBytes = grindPaths[i].spline.ToByteArray();
                splineData.AddRange(splineBytes);
                offsets.Add(offset);
                offset += splineBytes.Length;
            }

            byte[] grindPathBytes = new byte[grindPaths.Count * GrindPath.ELEMENTSIZE];

            for (int i = 0; i < grindPaths.Count; i++)
            {
                grindPaths[i].ToByteArray().CopyTo(grindPathBytes, i * GrindPath.ELEMENTSIZE);
            }

            byte[] offsetBytes = new byte[0x04 * (grindPaths.Count + 1)];
            for (int i = 0; i < grindPaths.Count; i++)
            {
                WriteInt(offsetBytes, i * 0x04, offsets[i]);
            }

            //Header
            byte[] headerBytes = new byte[0x10];
            WriteInt(headerBytes, 0x00, grindPaths.Count);
            WriteInt(headerBytes, 0x04, 0x10 + grindPathBytes.Length + offsetBytes.Length);
            WriteInt(headerBytes, 0x08, splineData.Count);

            List<byte> block = new List<byte>();
            block.AddRange(headerBytes);
            block.AddRange(grindPathBytes);
            block.AddRange(offsetBytes);
            block.AddRange(splineData);

            return block.ToArray();
        }

        public byte[] GetEnvTransitionBytes(List<EnvTransition> envTransitions)
        {
            if (envTransitions == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + envTransitions.Count * (EnvTransition.HEADSIZE + EnvTransition.ELEMENTSIZE)];

            //Header
            WriteInt(bytes, 0, envTransitions.Count);

            for (int i = 0; i < envTransitions.Count; i++)
            {
                envTransitions[i].ToByteArrayHead().CopyTo(bytes, 0x10 + i * EnvTransition.HEADSIZE);
                envTransitions[i].ToByteArrayMain().CopyTo(bytes, 0x10 + envTransitions.Count * EnvTransition.HEADSIZE + i * EnvTransition.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetPvarBlocksBytes(List<GlobalPvarBlock> pvarBlocks, int paddingSize)
        {
            if (pvarBlocks == null) return [];

            byte[] bytes = new byte[0x10 + paddingSize + pvarBlocks.Count * GlobalPvarBlock.ELEMENTSIZE];

            //Header
            WriteInt(bytes, 0x00, paddingSize);
            WriteInt(bytes, 0x04, pvarBlocks.Count);

            for (int i = 0; i < pvarBlocks.Count; i++)
            {
                pvarBlocks[i].ToByteArray().CopyTo(bytes, 0x10 + paddingSize + i * GlobalPvarBlock.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetPvarScratchPadBytes(List<PvarScratchPad> scratchPads)
        {
            if (scratchPads == null) return new byte[0x10];

            byte[] bytes = new byte[scratchPads.Count * 8 + 0x08];

            int idx = 0;
            foreach (PvarScratchPad pad in scratchPads)
            {
                WriteInt(bytes, idx * 8 + 0, pad.id);
                WriteInt(bytes, idx * 8 + 4, pad.value);
                idx++;
            }

            WriteInt(bytes, bytes.Length - 8, -1);
            WriteInt(bytes, bytes.Length - 4, -1);
            return bytes;
        }

        public byte[] GetPvarRewireBytes(List<PvarRewire> rewires)
        {
            if (rewires == null) return new byte[0x10];

            byte[] bytes = new byte[rewires.Count * 8 + 0x08];

            int idx = 0;
            foreach (PvarRewire rewire in rewires)
            {
                WriteInt(bytes, idx * 8 + 0, rewire.id);
                WriteInt(bytes, idx * 8 + 4, rewire.value);
                idx++;
            }

            WriteInt(bytes, bytes.Length - 8, -1);
            WriteInt(bytes, bytes.Length - 4, -1);
            return bytes;
        }

        public byte[] GetIdBytes(List<int> ids)
        {
            if (ids == null) return new byte[0x10];

            byte[] bytes = new byte[0x04 + ids.Count * 4];
            BitConverter.GetBytes(ids.Count).CopyTo(bytes, 0);
            for (int i = 0; i < ids.Count; i++)
            {
                BitConverter.GetBytes(ids[i]).CopyTo(bytes, 0x04 + i * 0x04);
            }
            return bytes;
        }


        public byte[] GetSplineBytes(List<Spline> splines)
        {
            if (splines == null) return new byte[0x10];

            List<byte> splineData = new List<byte>();
            List<int> offsets = new List<int>();

            int offset = 0;
            foreach (Spline spline in splines)
            {
                byte[] splineBytes = spline.ToByteArray();
                splineData.AddRange(splineBytes);
                offsets.Add(offset);
                offset += splineBytes.Length;
            }

            byte[] offsetBlock = new byte[GetLength(offsets.Count * 4)];
            for (int i = 0; i < offsets.Count; i++)
            {
                WriteUint(offsetBlock, i * 4, (uint) offsets[i]);
            }

            var bytes = new byte[0x10 + offsetBlock.Length + splineData.Count];
            WriteUint(bytes, 0, (uint) splines.Count);
            WriteUint(bytes, 0x04, (uint) (0x10 + offsetBlock.Length));
            WriteUint(bytes, 0x08, (uint) (splineData.Count));
            offsetBlock.CopyTo(bytes, 0x10);
            splineData.CopyTo(bytes, 0x10 + offsetBlock.Length);

            return bytes;
        }

        public byte[] GetPvarSizeBytes(List<byte[]> pVars)
        {
            if (pVars == null) return [];

            byte[] bytes = new byte[pVars.Count * 0x08];
            uint offset = 0;
            for (int i = 0; i < pVars.Count; i++)
            {
                WriteUint(bytes, (i * 0x08) + 0x00, offset);
                WriteUint(bytes, (i * 0x08) + 0x04, (uint) pVars[i].Length);
                offset += (uint) pVars[i].Length;
            }
            return bytes;
        }

        public byte[] GetPvarBytes(List<byte[]> pVars)
        {
            if (pVars == null) return new byte[0x10];

            var bytes = new byte[pVars.Sum(arr => arr.Length)];
            int index = 0;
            foreach (var pVar in pVars)
            {
                pVar.CopyTo(bytes, index);
                index += pVar.Length;
            }

            return bytes;
        }

        public byte[] GetOcclusionBytes(OcclusionData? occlusionData)
        {
            if (occlusionData == null) return [];

            return occlusionData.ToByteArray();
        }

    }
}
