using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Level
    {
        public string path;
        public EngineHeader engineHeader;

        public GameType game;

        //Models
        public List<Model> mobyModels;
        public List<Model> tieModels;
        public List<Model> shrubModels;
        public Model terrainModel;
        public Model collisionModel;
        public List<Model> chunks;
        public List<Texture> textures;
        public SkyboxModel skybox;

        //Level objects
        public List<Moby> mobs;
        public List<Tie> ties;
        public List<Tie> shrubs;
        public List<Spline> splines;
        public List<TerrainModel> terrains;

        public LevelVariables levelVariables;
        public OcclusionData occlusionData;

        public byte[] english;
        public byte[] lang2;
        public byte[] french;
        public byte[] german;
        public byte[] spanish;
        public byte[] italian;
        public byte[] lang7;
        public byte[] lang8;

        public byte[] unk6;
        public byte[] unk7;
        public byte[] unk9;
        public byte[] unk17;

        public byte[] tieData;
        public byte[] shrubData;

        public List<Type04> type04s;
        public List<Type0C> type0Cs;
        public List<Type64> type64s;
        public List<Type68> type68s;
        public List<Type80> type80s;
        public List<Type88> type88s;

        public List<byte[]> pVars;
        public List<SpawnPoint> spawnPoints;
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

            EngineParser engineParser = new EngineParser(enginePath);

            game = engineParser.DetectGame();

            VramParser vramParser = new VramParser(path + @"/vram.ps3");
            GameplayParser gameplayParser = new GameplayParser(game, path + @"/gameplay_ntsc");

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

            Console.WriteLine("Parsing textures...");
            textures = engineParser.GetTextures();
            vramParser.GetTextures(textures);
            Console.WriteLine("Added " + textures.Count + " textures");

            Console.WriteLine("Parsing ties...");
            ties = engineParser.GetTies(tieModels);
            Console.WriteLine("Added " + ties.Count + " ties");

            Console.WriteLine("Parsing Shrubs...");
            shrubs = engineParser.GetShrubs(shrubModels);
            Console.WriteLine("Added " + shrubs.Count + " Shrubs");

            Console.WriteLine("Parsing terrain elements...");
            terrains = engineParser.GetTerrainModels();
            Console.WriteLine("Added " + terrains?.Count + " terrain elements");


            Console.WriteLine("Parsing Level variables...");
            levelVariables = gameplayParser.GetLevelVariables();

            Console.WriteLine("Parsing mobs...");
            mobs = gameplayParser.GetMobies(game, mobyModels);
            Console.WriteLine("Added " + mobs?.Count + " mobs");

            Console.WriteLine("Parsing splines...");
            splines = gameplayParser.GetSplines();
            Console.WriteLine("Added " + splines.Count + " splines");

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
            unk6 = gameplayParser.getUnk6();
            unk7 = gameplayParser.getUnk7();
            unk9 = gameplayParser.getUnk9();
            unk17 = gameplayParser.getUnk17();

            tieData = gameplayParser.getTieData(ties.Count);
            shrubData = gameplayParser.getShrubData(shrubs.Count);

            type04s = gameplayParser.GetType04s();
            type0Cs = gameplayParser.GetType0Cs();
            type64s = gameplayParser.GetType64s();
            type68s = gameplayParser.GetType68s();
            type80s = gameplayParser.getType80();
            type88s = gameplayParser.GetType88s();
            pVars = gameplayParser.getPvars();
            spawnPoints = gameplayParser.GetSpawnPoints();
            gameCameras = gameplayParser.GetGameCameras();

            mobyIds = gameplayParser.getMobyIds();
            tieIds = gameplayParser.getTieIds();
            shrubIds = gameplayParser.getShrubIds();
            occlusionData = gameplayParser.getOcclusionData();

            engineParser.Close();
            vramParser.Close();
            gameplayParser.Close();

            Console.WriteLine("Level parsing done");
        }

    }
}
