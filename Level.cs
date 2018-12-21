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
        public Level()
        {

        }

        //Engine file constructor
        public Level(string enginePath)
        {
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


            FileStream vfs;
            try {
                vfs = File.OpenRead(path + @"/vram.ps3");
            }
            catch (Exception e) {
                Console.WriteLine(e);
                MessageBox.Show("vram.ps3 missing!", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                System.Windows.Forms.Application.Exit();
                return;
            }
            
            for(int i = 0; i < textures.Count; i++)
            {
                int length = 0;
                if(i < textures.Count - 1)
                {
                    length = (int)(textures[i + 1].vramPointer - textures[i].vramPointer);
                }
                else
                {
                    length = (int)(vfs.Length - textures[i].vramPointer);
                }
                textures[i].data = ReadBlock(vfs, textures[i].vramPointer, length);
            }
            vfs.Close();



            //These will be in their own file at some point
            FileStream gpf = File.OpenRead(path + @"/gameplay_ntsc");
            uint mobyPointer = ReadUint(ReadBlock(gpf, 0x44, 4), 0);
            uint splinePointer = ReadUint(ReadBlock(gpf, 0x70, 4), 0);


            mobs = new List<Moby>();
            int mobyCount = ReadInt(ReadBlock(gpf, mobyPointer, 4), 0);
            byte[] mobyBlock = ReadBlock(gpf, mobyPointer + 0x10, mobyCount * 0x78);
            for(int i = 0; i < mobyCount * 0x78; i+= 0x78)
            {
                Moby moby = new Moby(mobyBlock, i, mobyModels);
                mobs.Add(moby);
            }

            splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(gpf, splinePointer, 4), 0);
            uint splineOffset = ReadUint(ReadBlock(gpf, splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(gpf, splinePointer + 8, 4), 0);
            byte[] splineHeadBlock = ReadBlock(gpf, splinePointer + 0x10, splineCount * 4);
            byte[] splineBlock = ReadBlock(gpf, splinePointer + splineOffset, splineSectionSize);
            for (int i = 0; i < splineCount; i++)
            {
                int offset = ReadInt(splineHeadBlock, (i * 4));
                splines.Add(new Spline(splineBlock, offset));
            }

            gpf.Close();
        }
    }
}
