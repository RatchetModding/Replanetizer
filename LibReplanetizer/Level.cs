using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using LibReplanetizer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibReplanetizer
{
    public class Level
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public bool valid;

        public string path;
        public EngineHeader engineHeader;

        public GameType game;

        //Models
        public List<Model> mobyModels;
        public List<Model> tieModels;
        public List<Model> shrubModels;
        public List<Model> gadgetModels;
        public List<Model> armorModels;
        public Model collisionEngine;
        public List<Model> collisionChunks;
        public List<Model> chunks;
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
        public List<TerrainFragment> terrainEngine;
        public List<List<TerrainFragment>> terrainChunks;
        public List<int> textureConfigMenus;
        public List<Mission> missions;

        public LevelVariables levelVariables;
        public OcclusionData occlusionData;

        public Dictionary<int, String> english;
        public Dictionary<int, String> lang2;
        public Dictionary<int, String> french;
        public Dictionary<int, String> german;
        public Dictionary<int, String> spanish;
        public Dictionary<int, String> italian;
        public Dictionary<int, String> lang7;
        public Dictionary<int, String> lang8;

        public byte[] unk6;
        public byte[] unk7;
        public byte[] unk9;
        public byte[] unk13;
        public byte[] unk17;
        public byte[] unk14;

        public LightConfig lightConfig;

        public List<KeyValuePair<int, int>> type50s;
        public List<KeyValuePair<int, int>> type5Cs;

        public byte[] tieData;
        public byte[] shrubData;

        public List<Type04> type04s;
        public List<Type0C> type0Cs;
        public List<Type64> type64s;
        public List<Type68> type68s;
        public List<Type7C> type7Cs;
        public List<Type80> type80s;
        public List<Type88> type88s;

        public List<byte[]> pVars;
        public List<Cuboid> cuboids;
        public List<GameCamera> gameCameras;

        public List<int> mobyIds;
        public List<int> tieIds;
        public List<int> shrubIds;

        ~Level()
        {
            Logger.Trace("Level destroyed");
        }

        //New file constructor
        public Level() { }

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

                Logger.Debug("Parsing skybox...");
                skybox = engineParser.GetSkyboxModel();
                Logger.Debug("Success");

                Logger.Debug("Parsing moby models...");
                mobyModels = engineParser.GetMobyModels();  
                Logger.Debug("Added {0} moby models", mobyModels.Count);

                Logger.Debug("Parsing tie models...");
                tieModels = engineParser.GetTieModels();
                Logger.Debug("Added {0} tie models", tieModels.Count);

                Logger.Debug("Parsing shrub models...");
                shrubModels = engineParser.GetShrubModels();
                Logger.Debug("Added {0} shrub models", shrubModels.Count);

                Logger.Debug("Parsing weapons...");
                gadgetModels = engineParser.GetGadgets();
                Logger.Debug("Added {0} weapons", gadgetModels.Count);

                Logger.Debug("Parsing textures...");
                textures = engineParser.GetTextures();
                Logger.Debug("Added {0} textures", textures.Count);

                Logger.Debug("Parsing ties...");
                ties = engineParser.GetTies(tieModels);
                Logger.Debug("Added {0} ties", ties.Count);

                Logger.Debug("Parsing Shrubs...");
                shrubs = engineParser.GetShrubs(shrubModels);
                Logger.Debug("Added {0} shrubs", shrubs.Count);

                Logger.Debug("Parsing Lights...");
                lights = engineParser.GetLights();
                Logger.Debug("Added {0} lights", lights.Count);

                Logger.Debug("Parsing terrain elements...");
                terrainEngine = engineParser.GetTerrainModels();
                Logger.Debug("Added {0} terrain elements" + terrainEngine?.Count);

                Logger.Debug("Parsing player animations...");
                playerAnimations = engineParser.GetPlayerAnimations((MobyModel)mobyModels[0]);
                Logger.Debug("Added {0} player animations", playerAnimations?.Count);

                uiElements = engineParser.GetUiElements();
                Logger.Debug("Added {0} ui elements", uiElements?.Count);

                lightConfig = engineParser.GetLightConfig();
                textureConfigMenus = engineParser.GetTextureConfigMenu();

                collisionEngine = engineParser.GetCollisionModel();
            }


            // Gameplay elements
            using (GameplayParser gameplayParser = new GameplayParser(game, path + @"/gameplay_ntsc"))
            {
                Logger.Debug("Parsing Level variables...");
                levelVariables = gameplayParser.GetLevelVariables();

                Logger.Debug("Parsing mobs...");
                mobs = gameplayParser.GetMobies(game, mobyModels);
                Logger.Debug("Added {0} mobs", mobs?.Count);

                Logger.Debug("Parsing splines...");
                splines = gameplayParser.GetSplines();
                Logger.Debug("Added {0} splines", splines?.Count);

                Logger.Debug("Parsing languages...");
                english = gameplayParser.GetEnglish();
                lang2 = gameplayParser.GetLang2();
                french = gameplayParser.GetFrench();
                german = gameplayParser.GetGerman();
                spanish = gameplayParser.GetSpanish();
                italian = gameplayParser.GetItalian();
                lang7 = gameplayParser.GetLang7();
                lang8 = gameplayParser.GetLang8();

                Logger.Debug("Parsing other gameplay assets...");
                unk6 = gameplayParser.GetUnk6();
                unk7 = gameplayParser.GetUnk7();
                unk13 = gameplayParser.GetUnk13();
                unk17 = gameplayParser.GetUnk17();
                unk14 = gameplayParser.GetUnk14();

                tieData = gameplayParser.GetTieData(ties.Count);
                shrubData = gameplayParser.getShrubData(shrubs.Count);

                type04s = gameplayParser.GetType04s();
                type0Cs = gameplayParser.GetType0Cs();
                type64s = gameplayParser.GetType64s();
                type68s = gameplayParser.GetType68s();
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

            terrainChunks = new List<List<TerrainFragment>>();
            collisionChunks = new List<Model>();
            collBytesChunks = new List<byte[]>();

            for (int i = 0; i < 5; i++)
            {
                if (!File.Exists(path + @"/chunk" + i + ".ps3")) continue;

                using (ChunkParser chunkParser = new ChunkParser(path + @"/chunk"+i+".ps3"))
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
                List<Texture> tex;
                MobyModel model;
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

                Mission mission = new Mission(i);

                using (MissionParser parser = new MissionParser(game, missionPath))
                {
                    mission.models = parser.GetModels();
                    mission.textures = parser.GetTextures();
                }

                string vram = missionPath.Replace(".ps3", ".vram");

                using (VramParser parser = new VramParser(vram))
                {
                    parser.GetTextures(mission.textures);
                }

                missions.Add(mission);
            }

            using (VramParser vramParser = new VramParser(path + @"/vram.ps3"))
            {
                vramParser.GetTextures(textures);
            }

            Logger.Info("Level parsing done");
            valid = true;
        }
    }
}
