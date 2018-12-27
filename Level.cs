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

            Console.WriteLine("Parsing mobs...");
            mobs = gameplayParser.GetMobies(mobyModels);
            Console.WriteLine("Added " + mobs.Count + " mobs");

            Console.WriteLine("Parsing splines...");
            splines = gameplayParser.GetSplines();
            Console.WriteLine("Added " + splines.Count + " splines");

            engineParser.Close();
            vramParser.Close();
            gameplayParser.Close();
        }
    }
}
