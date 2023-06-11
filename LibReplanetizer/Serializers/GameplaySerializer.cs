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
            FileStream fs = File.Open(directory, FileMode.Create);

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
                pvarScratchPadPointer = SeekWrite(fs, GetKeyValueBytes(level.type50s)),
                pvarRewirePointer = SeekWrite(fs, GetKeyValueBytes(level.type5Cs)),
                mobyGroupsPointer = SeekWrite(fs, level.unk6),
                globalPvarPointer = SeekWrite(fs, GetType4CBytes(level.type4Cs)),
                tieIdPointer = SeekWrite(fs, GetIdBytes(level.tieIds)),
                tiePointer = SeekWrite(fs, level.tieData),
                shrubIdPointer = SeekWrite(fs, GetIdBytes(level.shrubIds)),
                shrubPointer = SeekWrite(fs, level.shrubData),
                splinePointer = SeekWrite(fs, GetSplineBytes(level.splines)),
                cuboidPointer = SeekWrite(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE)),
                spherePointer = SeekWrite(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE)),
                cylinderPointer = SeekWrite(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE)),
                pillPointer = SeekWrite(fs, new byte[0x10]),
                camCollisionPointer = SeekWrite(fs, level.unk17),
                pointLightPointer = SeekWrite(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.RaC1))),
                pointLightGridPointer = SeekWrite(fs, level.unk14),
                grindPathsPointer = SeekWrite(fs, GetGrindPathsBytes(level.grindPaths)),
                occlusionPointer = SeekWrite(fs, GetOcclusionBytes(level.occlusionData))
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
                pvarScratchPadPointer = SeekWrite(fs, GetKeyValueBytes(level.type50s)),
                pvarRewirePointer = SeekWrite(fs, GetKeyValueBytes(level.type5Cs)),
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
                pillPointer = SeekWrite(fs, new byte[0x10]),
                camCollisionPointer = SeekWrite(fs, level.unk17),
                pointLightPointer = SeekWrite4(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.RaC2))),
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
                envSamplesPointer = SeekWrite(fs, SerializeLevelObjects(level.envSamples, EnvSample.GetElementSize(GameType.RaC3))),
                levelVarPointer = SeekWrite4(fs, level.levelVariables.Serialize(level.game)),
                englishPointer = SeekWrite4(fs, GetLangBytes(level.english)),
                ukenglishPointer = SeekWrite4(fs, GetLangBytes(level.ukenglish)),
                frenchPointer = SeekWrite4(fs, GetLangBytes(level.french)),
                germanPointer = SeekWrite4(fs, GetLangBytes(level.german)),
                spanishPointer = SeekWrite4(fs, GetLangBytes(level.spanish)),
                italianPointer = SeekWrite4(fs, GetLangBytes(level.italian)),
                japanesePointer = SeekWrite4(fs, GetLangBytes(level.japanese)),
                koreanPointer = SeekWrite4(fs, GetLangBytes(level.korean)),
                lightsPointer = SeekWrite4(fs, SerializeLevelObjects(level.directionalLights, DirectionalLight.ELEMENTSIZE)),
                envTransitionsPointer = SeekWrite4(fs, GetEnvTransitionBytes(level.envTransitions)),
                cameraPointer = SeekWrite4(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE)),
                soundPointer = SeekWrite4(fs, SerializeLevelObjects(level.soundInstances, SoundInstance.ELEMENTSIZE)),
                mobyIdPointer = SeekWrite4(fs, GetIdBytes(level.mobyIds)),
                mobyPointer = SeekWrite4(fs, GetMobyBytes(level.mobs, level.game)),
                pvarSizePointer = SeekWrite4(fs, GetPvarSizeBytes(level.pVars)),
                pvarPointer = SeekWrite4(fs, GetPvarBytes(level.pVars)),
                pvarScratchPadPointer = SeekWrite4(fs, GetKeyValueBytes(level.type50s)),
                pvarRewirePointer = SeekWrite4(fs, GetKeyValueBytes(level.type5Cs)),
                mobyGroupsPointer = SeekWrite4(fs, level.unk6),
                globalPvarPointer = SeekWrite4(fs, level.unk7),
                tieIdPointer = SeekWrite4(fs, GetIdBytes(level.tieIds)),
                tiePointer = SeekWrite4(fs, level.tieData),
                tieGroupsPointer = SeekWrite4(fs, level.tieGroupData),
                shrubIdPointer = SeekWrite4(fs, GetIdBytes(level.shrubIds)),
                shrubPointer = SeekWrite4(fs, level.shrubData),
                shrubGroupsPointer = SeekWrite4(fs, level.shrubGroupData),
                splinePointer = SeekWrite4(fs, GetSplineBytes(level.splines)),
                cuboidPointer = SeekWrite4(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE)),
                spherePointer = SeekWrite4(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE)),
                cylinderPointer = SeekWrite4(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE)),
                pillPointer = SeekWrite4(fs, new byte[0x10]),
                camCollisionPointer = SeekWrite4(fs, level.unk17),
                pointLightPointer = SeekWrite4(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.RaC3))),
                grindPathsPointer = SeekWrite4(fs, GetGrindPathsBytes(level.grindPaths)),
                areasPointer = SeekWrite4(fs, level.areasData),
                occlusionPointer = SeekWrite4(fs, GetOcclusionBytes(level.occlusionData))
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
                levelVarPointer = SeekWrite4(fs, level.levelVariables.Serialize(level.game)),
                englishPointer = SeekWrite4(fs, GetLangBytes(level.english)),
                ukenglishPointer = SeekWrite4(fs, GetLangBytes(level.ukenglish)),
                frenchPointer = SeekWrite4(fs, GetLangBytes(level.french)),
                germanPointer = SeekWrite4(fs, GetLangBytes(level.german)),
                spanishPointer = SeekWrite4(fs, GetLangBytes(level.spanish)),
                italianPointer = SeekWrite4(fs, GetLangBytes(level.italian)),
                japanesePointer = SeekWrite4(fs, GetLangBytes(level.japanese)),
                koreanPointer = SeekWrite4(fs, GetLangBytes(level.korean)),
                lightsPointer = SeekWrite4(fs, SerializeLevelObjects(level.directionalLights, DirectionalLight.ELEMENTSIZE)),
                envTransitionsPointer = SeekWrite4(fs, GetEnvTransitionBytes(level.envTransitions)),
                cameraPointer = SeekWrite4(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE)),
                soundPointer = SeekWrite4(fs, SerializeLevelObjects(level.soundInstances, SoundInstance.ELEMENTSIZE)),
                mobyIdPointer = SeekWrite4(fs, GetIdBytes(level.mobyIds)),
                mobyPointer = SeekWrite4(fs, GetMobyBytes(level.mobs, level.game)),
                pvarSizePointer = SeekWrite4(fs, GetPvarSizeBytes(level.pVars)),
                pvarPointer = SeekWrite4(fs, GetPvarBytes(level.pVars)),
                pvarScratchPadPointer = SeekWrite4(fs, GetKeyValueBytes(level.type50s)),
                pvarRewirePointer = SeekWrite4(fs, GetKeyValueBytes(level.type5Cs)),
                mobyGroupsPointer = SeekWrite4(fs, level.unk6),
                globalPvarPointer = SeekWrite4(fs, level.unk7),
                tieIdPointer = SeekWrite4(fs, GetIdBytes(level.tieIds)),
                tiePointer = SeekWrite4(fs, level.tieData),
                tieGroupsPointer = SeekWrite4(fs, level.tieGroupData),
                shrubIdPointer = SeekWrite4(fs, GetIdBytes(level.shrubIds)),
                shrubPointer = SeekWrite4(fs, level.shrubData),
                shrubGroupsPointer = SeekWrite4(fs, level.shrubGroupData),
                splinePointer = SeekWrite4(fs, GetSplineBytes(level.splines)),
                cuboidPointer = SeekWrite4(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE)),
                spherePointer = SeekWrite4(fs, SerializeLevelObjects(level.spheres, Sphere.ELEMENTSIZE)),
                cylinderPointer = SeekWrite4(fs, SerializeLevelObjects(level.cylinders, Cylinder.ELEMENTSIZE)),
                pillPointer = SeekWrite4(fs, new byte[0x10]),
                camCollisionPointer = SeekWrite4(fs, level.unk17),
                pointLightPointer = SeekWrite4(fs, SerializeLevelObjects(level.pointLights, PointLight.GetElementSize(GameType.DL))),
                grindPathsPointer = SeekWrite4(fs, GetGrindPathsBytes(level.grindPaths)),
                areasPointer = SeekWrite4(fs, level.areasData),
                occlusionPointer = SeekWrite4(fs, GetOcclusionBytes(level.occlusionData))
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
            if (levelobjects == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + levelobjects.Count * elementSize];

            //Header
            WriteInt(bytes, 0, levelobjects.Count);

            for (int i = 0; i < levelobjects.Count; i++)
            {
                levelobjects[i].ToByteArray().CopyTo(bytes, 0x10 + i * elementSize);
            }

            return bytes;
        }

        public byte[] GetGrindPathsBytes(List<GrindPath> grindPaths)
        {
            if (grindPaths == null) return new byte[0x10];

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

            byte[] bytes = new byte[0x10 + grindPaths.Count * GrindPath.ELEMENTSIZE];

            for (int i = 0; i < grindPaths.Count; i++)
            {
                grindPaths[i].ToByteArray().CopyTo(bytes, 0x10 + i * GrindPath.ELEMENTSIZE);
            }

            byte[] offsetBytes = new byte[0x04 * grindPaths.Count];
            for (int i = 0; i < grindPaths.Count; i++)
            {
                WriteInt(bytes, i * 0x04, offsets[i]);
            }

            //Header
            WriteInt(bytes, 0x00, grindPaths.Count);
            WriteInt(bytes, 0x04, bytes.Length + offsetBytes.Length);
            WriteInt(bytes, 0x08, splineData.Count);

            List<byte> block = new List<byte>();
            block.AddRange(bytes);
            block.AddRange(offsetBytes);
            block.AddRange(splineData);

            return bytes;
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

        public byte[] GetType4CBytes(List<GlobalPvarBlock> type4Cs)
        {
            if (type4Cs == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + 0x10 + type4Cs.Count * GlobalPvarBlock.ELEMENTSIZE];

            //Header
            WriteInt(bytes, 0x00, 0x10);
            WriteInt(bytes, 0x04, type4Cs.Count);

            for (int i = 0; i < type4Cs.Count; i++)
            {
                type4Cs[i].ToByteArray().CopyTo(bytes, 0x10 + 0x10 + i * GlobalPvarBlock.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetKeyValueBytes(List<KeyValuePair<int, int>> type50s)
        {
            if (type50s == null) return new byte[0x10];

            byte[] bytes = new byte[type50s.Count * 8 + 0x08];

            int idx = 0;
            foreach (KeyValuePair<int, int> pair in type50s)
            {
                WriteInt(bytes, idx * 8 + 0, pair.Key);
                WriteInt(bytes, idx * 8 + 4, pair.Value);
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
            if (pVars == null) return new byte[0x10];

            byte[] bytes = new byte[pVars.Count * 8];
            uint offset = 0;
            for (int i = 0; i < pVars.Count; i++)
            {
                WriteUint(bytes, (i * 8) + 0x00, offset);
                WriteUint(bytes, (i * 8) + 0x04, (uint) pVars[i].Length);
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
            if (occlusionData == null) return new byte[0x10];

            return occlusionData.ToByteArray();
        }

    }
}
