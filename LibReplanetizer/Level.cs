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
        public bool valid;

        public string path;
        public EngineHeader engineHeader;

        public GameType game;

        //Models
        public List<Model> mobyModels;
        public List<Model> tieModels;
        public List<Model> shrubModels;
        public List<Model> weaponModels;
        public Model collisionModel;
        public List<Model> chunks;
        public List<Texture> textures;
        public SkyboxModel skybox;

        public byte[] renderDefBytes;
        public byte[] collBytes;
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
        public List<TerrainFragment> terrains;
        public List<int> textureConfigMenus;

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
            Console.WriteLine("Level destroyed");
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
                game = engineParser.DetectGame();

                //REMOVE THESE ASAP!!!!!111
                renderDefBytes = engineParser.GetRenderDefBytes();
                collBytes = engineParser.GetCollisionBytes();
                billboardBytes = engineParser.GetBillboardBytes();
                soundConfigBytes = engineParser.GetSoundConfigBytes();

                Console.WriteLine("Parsing skybox...");
                skybox = engineParser.GetSkyboxModel();
                Console.WriteLine("Success");

                Console.WriteLine("Parsing moby models...");
                mobyModels = engineParser.GetMobyModels();
                Console.WriteLine("Added " + mobyModels.Count + " moby models");

                Console.WriteLine("Parsing tie models...");
                tieModels = engineParser.GetTieModels();
                Console.WriteLine("Added " + tieModels.Count + " tie models");

                Console.WriteLine("Parsing shrub models...");
                shrubModels = engineParser.GetShrubModels();
                Console.WriteLine("Added " + shrubModels.Count + " shrub models");

                Console.WriteLine("Parsing weapons...");
                weaponModels = engineParser.GetWeapons();
                Console.WriteLine("Added " + weaponModels.Count + " weapons");

                Console.WriteLine("Parsing textures...");
                textures = engineParser.GetTextures();
                Console.WriteLine("Added " + textures.Count + " textures");

                Console.WriteLine("Parsing ties...");
                ties = engineParser.GetTies(tieModels);
                Console.WriteLine("Added " + ties.Count + " ties");

                Console.WriteLine("Parsing Shrubs...");
                shrubs = engineParser.GetShrubs(shrubModels);
                Console.WriteLine("Added " + shrubs.Count + " Shrubs");

                Console.WriteLine("Parsing Lights...");
                lights = engineParser.GetLights();
                Console.WriteLine("Added " + lights.Count + " lights");

                Console.WriteLine("Parsing terrain elements...");
                terrains = engineParser.GetTerrainModels();
                Console.WriteLine("Added " + terrains?.Count + " terrain elements");

                Console.WriteLine("Parsing player animations...");
                playerAnimations = engineParser.GetPlayerAnimations((MobyModel)mobyModels[0]);
                Console.WriteLine("Added " + playerAnimations?.Count + " player animations");

                uiElements = engineParser.GetUiElements();
                Console.WriteLine("Added " + uiElements?.Count + " ui elements");

                lightConfig = engineParser.GetLightConfig();
                textureConfigMenus = engineParser.GetTextureConfigMenu();
                collisionModel = engineParser.GetCollisionModel();
            }


            // Gameplay elements
            using (GameplayParser gameplayParser = new GameplayParser(game, path + @"/gameplay_ntsc"))
            {
                Console.WriteLine("Parsing Level variables...");
                levelVariables = gameplayParser.GetLevelVariables();

                Console.WriteLine("Parsing mobs...");
                mobs = gameplayParser.GetMobies(game, mobyModels);
                Console.WriteLine("Added " + mobs?.Count + " mobs");

                Console.WriteLine("Parsing splines...");
                splines = gameplayParser.GetSplines();
                //Console.WriteLine("Added " + splines.Count + " splines");

                Console.WriteLine("Parsing languages...");
                english = gameplayParser.GetEnglish();
                lang2 = gameplayParser.GetLang2();
                french = gameplayParser.GetFrench();
                german = gameplayParser.GetGerman();
                spanish = gameplayParser.GetSpanish();
                italian = gameplayParser.GetItalian();
                lang7 = gameplayParser.GetLang7();
                lang8 = gameplayParser.GetLang8();

                Console.WriteLine("Parsing other gameplay assets...");
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


            VramParser vramParser = new VramParser(path + @"/vram.ps3");
            if (!vramParser.valid)
            {
                valid = false;
                return;
            }

            vramParser.GetTextures(textures);
            vramParser.Close();


            Console.WriteLine("Level parsing done");
            valid = true;
        }

    }
}
