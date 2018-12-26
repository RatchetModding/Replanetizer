using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit {
    public class Level {
        public string path;
        public EngineHeader engineHeader;
        public List<Model> mobyModels;
        public List<Model> tieModels;
        public List<Model> shrubModels;
        public Model terrainModel;
        public Model collisionModel;
        public List<Model> chunks;
        public List<Texture> textures;
        public List<Moby> mobs;
        public List<Tie> ties;
        public List<Tie> shrubs;
        public List<Spline> splines;
        public List<TerrainModel> terrains;


        //New file constructor
        public Level() { }

        //Engine file constructor
        public Level(string enginePath) {
            path = Path.GetDirectoryName(enginePath);

            EngineParser levelParser = new EngineParser(enginePath);

            Console.WriteLine("Parsing moby models...");
            mobyModels = levelParser.GetMobyModels();
            Console.WriteLine("Added " + mobyModels.Count + " moby models");

            Console.WriteLine("Parsing tie models...");
            tieModels = levelParser.GetTieModels();
            Console.WriteLine("Added " + tieModels.Count + " tie models");

            Console.WriteLine("Parsing shrub models...");
            shrubModels = levelParser.GetShrubModels();
            Console.WriteLine("Added " + shrubModels.Count + " shrub models");

            Console.WriteLine("Parsing textures...");
            textures = levelParser.GetTextures();
            Console.WriteLine("Added " + textures.Count + " textures");

            Console.WriteLine("Parsing ties...");
            ties = levelParser.GetTies(tieModels);
            Console.WriteLine("Added " + ties.Count + " ties");

            Console.WriteLine("Parsing Shrubs...");
            shrubs = levelParser.GetShrubs(shrubModels);
            Console.WriteLine("Added " + shrubs.Count + " Shrubs");

            Console.WriteLine("Parsing terrain elements...");
            terrains = levelParser.GetTerrainModels();
            Console.WriteLine("Added " + terrains.Count + " terrain elements");

            levelParser.Close();


            VramParser vramParser = new VramParser(path + @"/vram.ps3");
            vramParser.GetTextures(textures);
            vramParser.Close();

            GameplayParser gameplayParser = new GameplayParser(path + @"/gameplay_ntsc");
            mobs = gameplayParser.GetMobies(mobyModels);
            splines = gameplayParser.GetSplines();
            gameplayParser.Close();
        }
    }
}
