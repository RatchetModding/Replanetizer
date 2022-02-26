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
using LibReplanetizer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using LibReplanetizer.Serializers;

namespace LibReplanetizer
{
    public class Level
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public bool valid;

        public string? path;

        public GameType game;

        //Models
        public List<Model> mobyModels;
        public List<Model> tieModels;
        public List<Model> shrubModels;
        public List<Model> gadgetModels;
        public List<Model> armorModels;
        public Model collisionEngine;
        public List<Model> collisionChunks;
        public List<Texture> textures;
        public List<List<Texture>> armorTextures;
        public List<Texture> gadgetTextures;
        public SkyboxModel skybox;

        public byte[] renderDefBytes;
        public byte[] collBytesEngine;
        public List<byte[]> collBytesChunks;
        public byte[] billboardBytes;
        public byte[] soundConfigBytes;

        public List<Animation> playerAnimations;
        public List<UiElement> uiElements;


        //Level objects
        public List<Moby> mobs;
        public List<Tie> ties;
        public List<Shrub> shrubs;
        public List<Light> lights;
        public List<Spline> splines;
        public Terrain terrainEngine;
        public List<Terrain> terrainChunks;
        public List<int> textureConfigMenus;
        public List<Mission> missions;

        public LevelVariables levelVariables;
        public OcclusionData? occlusionData;

        public List<LanguageData> english;
        public List<LanguageData> ukenglish;
        public List<LanguageData> french;
        public List<LanguageData> german;
        public List<LanguageData> spanish;
        public List<LanguageData> italian;
        public List<LanguageData> japanese;
        public List<LanguageData> korean;

        public byte[] unk3;
        public byte[] unk4;
        public byte[] unk5;
        public byte[] unk6;
        public byte[] unk7;
        public byte[] unk8;
        public byte[] unk9;
        public byte[] unk12;
        public byte[] unk13;
        public byte[] unk14;
        public byte[] unk16;
        public byte[] unk17;
        public byte[] unk18;

        public LightConfig lightConfig;

        public List<KeyValuePair<int, int>> type50s;
        public List<KeyValuePair<int, int>> type5Cs;

        public byte[] tieData;
        public byte[] shrubData;

        public byte[] tieGroupData;
        public byte[] shrubGroupData;

        public byte[] areasData;

        public List<DirectionalLight> directionalLights;
        public List<Type0C> type0Cs;
        public List<Type4C> type4Cs;
        public List<Type7C> type7Cs;
        public List<Type80> type80s;
        public List<Type88> type88s;

        public List<byte[]> pVars;
        public List<Cuboid> cuboids;
        public List<Sphere> spheres;
        public List<Cylinder> cylinders;
        public List<GameCamera> gameCameras;

        public List<int> mobyIds;
        public List<int> tieIds;
        public List<int> shrubIds;

        ~Level()
        {
            LOGGER.Trace("Level destroyed");
        }

        //Engine file constructor
        public Level(string enginePath)
        {

            path = Path.GetDirectoryName(enginePath);

            // Engine elements
            using (EngineParser engineParser = new EngineParser(enginePath))
            {
                game = engineParser.GetGameType();

                //REMOVE THESE ASAP!!!!!111
                renderDefBytes = engineParser.GetRenderDefBytes();
                collBytesEngine = engineParser.GetCollisionBytes();
                billboardBytes = engineParser.GetBillboardBytes();
                soundConfigBytes = engineParser.GetSoundConfigBytes();

                LOGGER.Debug("Parsing skybox...");
                skybox = engineParser.GetSkyboxModel();
                LOGGER.Debug("Success");

                LOGGER.Debug("Parsing moby models...");
                mobyModels = engineParser.GetMobyModels();
                LOGGER.Debug("Added {0} moby models", mobyModels.Count);

                LOGGER.Debug("Parsing tie models...");
                tieModels = engineParser.GetTieModels();
                LOGGER.Debug("Added {0} tie models", tieModels.Count);

                LOGGER.Debug("Parsing shrub models...");
                shrubModels = engineParser.GetShrubModels();
                LOGGER.Debug("Added {0} shrub models", shrubModels.Count);

                LOGGER.Debug("Parsing weapons...");
                gadgetModels = engineParser.GetGadgets();
                LOGGER.Debug("Added {0} weapons", gadgetModels.Count);

                LOGGER.Debug("Parsing textures...");
                textures = engineParser.GetTextures();
                LOGGER.Debug("Added {0} textures", textures.Count);

                LOGGER.Debug("Parsing ties...");
                ties = engineParser.GetTies(tieModels);
                LOGGER.Debug("Added {0} ties", ties.Count);

                LOGGER.Debug("Parsing Shrubs...");
                shrubs = engineParser.GetShrubs(shrubModels);
                LOGGER.Debug("Added {0} shrubs", shrubs.Count);

                LOGGER.Debug("Parsing Lights...");
                lights = engineParser.GetLights();
                LOGGER.Debug("Added {0} lights", lights.Count);

                LOGGER.Debug("Parsing terrain elements...");
                terrainEngine = engineParser.GetTerrainModel();
                LOGGER.Debug("Added {0} terrain elements", terrainEngine.fragments.Count);

                LOGGER.Debug("Parsing player animations...");
                playerAnimations = engineParser.GetPlayerAnimations((MobyModel) mobyModels[0]);
                LOGGER.Debug("Added {0} player animations", playerAnimations.Count);

                uiElements = engineParser.GetUiElements();
                LOGGER.Debug("Added {0} ui elements", uiElements.Count);

                lightConfig = engineParser.GetLightConfig();
                textureConfigMenus = engineParser.GetTextureConfigMenu();

                collisionEngine = engineParser.GetCollisionModel();

                unk3 = engineParser.GetUnk3Bytes();
                unk4 = engineParser.GetUnk4Bytes();
                unk5 = engineParser.GetUnk5Bytes();
                unk8 = engineParser.GetUnk8Bytes();
                unk9 = engineParser.GetUnk9Bytes();
            }


            // Gameplay elements
            using (GameplayParser gameplayParser = new GameplayParser(game, path + @"/gameplay_ntsc"))
            {
                LOGGER.Debug("Parsing Level variables...");
                levelVariables = gameplayParser.GetLevelVariables();

                LOGGER.Debug("Parsing mobs...");
                mobs = gameplayParser.GetMobies(mobyModels);
                LOGGER.Debug("Added {0} mobs", mobs.Count);

                LOGGER.Debug("Parsing splines...");
                splines = gameplayParser.GetSplines();
                LOGGER.Debug("Added {0} splines", splines.Count);

                LOGGER.Debug("Parsing languages...");
                english = gameplayParser.GetEnglish();
                ukenglish = gameplayParser.GetUkEnglish();
                french = gameplayParser.GetFrench();
                german = gameplayParser.GetGerman();
                spanish = gameplayParser.GetSpanish();
                italian = gameplayParser.GetItalian();
                japanese = gameplayParser.GetJapanese();
                korean = gameplayParser.GetKorean();

                LOGGER.Debug("Parsing other gameplay assets...");
                unk6 = gameplayParser.GetUnk6();
                unk7 = gameplayParser.GetUnk7();
                unk12 = gameplayParser.GetUnk12();
                unk13 = gameplayParser.GetUnk13();
                unk14 = gameplayParser.GetUnk14();
                unk16 = gameplayParser.GetUnk16();
                unk17 = gameplayParser.GetUnk17();
                unk18 = gameplayParser.GetUnk18();

                tieData = gameplayParser.GetTieData(ties.Count);
                shrubData = gameplayParser.GetShrubData(shrubs.Count);

                tieGroupData = gameplayParser.GetTieGroups();
                shrubGroupData = gameplayParser.GetShrubGroups();

                areasData = gameplayParser.GetAreasData();

                directionalLights = gameplayParser.GetDirectionalLights();
                type0Cs = gameplayParser.GetType0Cs();
                spheres = gameplayParser.GetSpheres();
                cylinders = gameplayParser.GetCylinders();
                type4Cs = gameplayParser.GetType4Cs();
                type7Cs = gameplayParser.GetType7Cs();
                type80s = gameplayParser.GetType80();
                type88s = gameplayParser.GetType88s();
                type50s = gameplayParser.GetType50s();
                type5Cs = gameplayParser.GetType5Cs();

                pVars = gameplayParser.GetPvars(mobs);
                cuboids = gameplayParser.GetCuboids();
                gameCameras = gameplayParser.GetGameCameras();

                mobyIds = gameplayParser.GetMobyIds();
                tieIds = gameplayParser.GetTieIds();
                shrubIds = gameplayParser.GetShrubIds();
                occlusionData = gameplayParser.GetOcclusionData();
            }

            terrainChunks = new List<Terrain>();
            collisionChunks = new List<Model>();
            collBytesChunks = new List<byte[]>();

            for (int i = 0; i < 5; i++)
            {
                var chunkPath = Path.Join(path, @"chunk" + i + ".ps3");
                if (!File.Exists(chunkPath)) continue;

                using (ChunkParser chunkParser = new ChunkParser(chunkPath, game))
                {
                    terrainChunks.Add(chunkParser.GetTerrainModels());
                    collisionChunks.Add(chunkParser.GetCollisionModel());
                    collBytesChunks.Add(chunkParser.GetCollBytes());
                }
            }

            List<string> armorPaths = ArmorHeader.FindArmorFiles(game, enginePath);
            armorModels = new List<Model>();
            armorTextures = new List<List<Texture>>();

            foreach (string armor in armorPaths)
            {
                LOGGER.Debug("Looking for armor data in {0}", armor);
                List<Texture> tex;
                MobyModel? model;
                using (ArmorParser parser = new ArmorParser(game, armor))
                {
                    tex = parser.GetTextures();
                    model = parser.GetArmor();
                }

                string vram = armor.Replace(".ps3", ".vram");

                using (VramParser parser = new VramParser(vram))
                {
                    parser.GetTextures(tex);
                }

                if (model != null)
                    armorModels.Add(model);

                armorTextures.Add(tex);
            }

            string gadgetPath = GadgetHeader.FindGadgetFile(game, enginePath);
            gadgetTextures = new List<Texture>();

            if (gadgetPath != "")
            {
                LOGGER.Debug("Looking for gadget data in {0}", gadgetPath);
                using (GadgetParser parser = new GadgetParser(game, gadgetPath))
                {
                    gadgetModels.AddRange(parser.GetModels());
                    gadgetTextures.AddRange(parser.GetTextures());
                }

                using (VramParser parser = new VramParser(gadgetPath.Replace(".ps3", ".vram")))
                {
                    parser.GetTextures(gadgetTextures);
                }
            }

            List<string> missionPaths = MissionHeader.FindMissionFiles(game, enginePath);
            missions = new List<Mission>();

            for (int i = 0; i < missionPaths.Count; i++)
            {
                string missionPath = missionPaths[i];
                string vramPath = missionPath.Replace(".ps3", ".vram");

                if (!File.Exists(vramPath))
                {
                    LOGGER.Warn("Could not find .vram file for {0}", missionPath);
                    continue;
                }

                LOGGER.Debug("Looking for mission data in {0}", missionPath);

                Mission mission = new Mission(i);

                using (MissionParser parser = new MissionParser(game, missionPath))
                {
                    mission.models = parser.GetModels();
                    mission.textures = parser.GetTextures();
                }

                using (VramParser parser = new VramParser(vramPath))
                {
                    parser.GetTextures(mission.textures);
                }

                missions.Add(mission);
            }

            using (VramParser vramParser = new VramParser(path + @"/vram.ps3"))
            {
                vramParser.GetTextures(textures);
            }

            LOGGER.Info("Level parsing done");
            valid = true;
        }

        public void Save(string outputFile)
        {
            string? directory;
            if (File.Exists(outputFile) && File.GetAttributes(outputFile).HasFlag(FileAttributes.Directory))
            {
                directory = outputFile;
            }
            else
            {
                directory = Path.GetDirectoryName(outputFile);
            }

            if (directory == null) return;

            EngineSerializer engineSerializer = new EngineSerializer();
            engineSerializer.Save(this, directory);
            GameplaySerializer gameplaySerializer = new GameplaySerializer();
            gameplaySerializer.Save(this, directory);

            for (int i = 0; i < terrainChunks.Count; i++)
            {
                ChunkSerializer chunkSerializer = new ChunkSerializer();
                chunkSerializer.Save(this, directory, i);
            }
        }
    }
}
