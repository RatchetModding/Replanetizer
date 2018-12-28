using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit {
    public class Level {
        public string path;
        public EngineHeader engineHeader;

        //Models
        public List<Model> mobyModels;
        public List<Model> tieModels;
        public List<Model> shrubModels;
        public Model terrainModel;
        public Model collisionModel;
        public List<Model> chunks;
        public List<Texture> textures;

        //Level objects
        public List<Moby> mobs;
        public List<Tie> ties;
        public List<Tie> shrubs;
        public List<Spline> splines;
        public List<TerrainModel> terrains;

        public LevelVariables levelVariables;
        public byte[] english;
        public byte[] lang2;
        public byte[] french;
        public byte[] german;
        public byte[] spanish;
        public byte[] italian;

        public byte[] unk6;
        public byte[] unk7;
        public byte[] unk9;
        public byte[] unk17;

        public List<byte[]> pVars;
        public List<SpawnPoint> spawnPoints;

        //New file constructor
        public Level() { }

        //Engine file constructor
        public Level(string enginePath) {
            path = Path.GetDirectoryName(enginePath);

            EngineParser engineParser = new EngineParser(enginePath);
            VramParser vramParser = new VramParser(path + @"/vram.ps3");
            GameplayParser gameplayParser = new GameplayParser(path + @"/gameplay_ntsc");

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
            Console.WriteLine("Added " + terrains.Count + " terrain elements");


            Console.WriteLine("Parsing Level variables...");
            levelVariables = gameplayParser.GetLevelVariables();

            Console.WriteLine("Parsing mobs...");
            mobs = gameplayParser.GetMobies(mobyModels);
            Console.WriteLine("Added " + mobs.Count + " mobs");

            Console.WriteLine("Parsing splines...");
            splines = gameplayParser.GetSplines();
            Console.WriteLine("Added " + splines.Count + " splines");

            Console.WriteLine("Parsing languages...");
            english = gameplayParser.getEnglish();
            lang2 = gameplayParser.getLang2();
            french = gameplayParser.getFrench();
            german = gameplayParser.getGerman();
            spanish = gameplayParser.getSpanish();
            italian = gameplayParser.getItalian();

            Console.WriteLine("Parsing other gameplay assets...");
            unk6 = gameplayParser.getUnk6();
            unk7 = gameplayParser.getUnk7();
            unk9 = gameplayParser.getUnk9();
            unk17 = gameplayParser.getUnk17();

            pVars = gameplayParser.getPvars();
            spawnPoints = gameplayParser.GetSpawnPoints();

        engineParser.Close();
            vramParser.Close();
            gameplayParser.Close();
        }
    }
}
