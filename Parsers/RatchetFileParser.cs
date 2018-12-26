using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class RatchetFileParser {
        protected FileStream fileStream;

        protected RatchetFileParser(string filePath) {
            try {
                fileStream = File.OpenRead(filePath);
            }
            catch(Exception e) {
                Console.WriteLine(e);
                Console.WriteLine("Couldn't load engine file.");
                Application.Exit();
                return;
            }
        }

        protected List<Model> GetMobyModels(uint mobyModelPointer) {
            List<Model> mobyModels = new List<Model>();

            //Get the moby count from the start of the section
            int mobyModelCount = ReadInt(ReadBlock(fileStream, mobyModelPointer, 4), 0);

            //Each moby is stored as a [MobyID, offset] pair
            byte[] mobyIDBlock = ReadBlock(fileStream, mobyModelPointer + 4, mobyModelCount * 8);
            for (int i = 0; i < mobyModelCount; i++)
            {
                short modelID = ReadShort(mobyIDBlock,  (i * 8) + 2);
                uint offset =   ReadUint(mobyIDBlock,   (i * 8) + 4);
                mobyModels.Add(new MobyModel(fileStream, modelID, offset));
            }
            return mobyModels;
        }

        protected List<Model> GetTieModels(uint tieModelPointer, int tieModelCount)
        {
            List<Model> tieModelList = new List<Model>();

            //Read the whole header block, and add models based on the count
            byte[] levelBlock = ReadBlock(fileStream, tieModelPointer, tieModelCount * TieModelHeader.TIEELEMSIZE);
            for (int i = 0; i < tieModelCount; i++)
            {
                TieModelHeader head = new TieModelHeader(levelBlock, i);
                tieModelList.Add(new TieModel(fileStream, head));
            }

            return tieModelList;
        }

        protected List<Model> GetShrubModels(uint shrubModelPointer, int shrubModelCount)
        {
            List<Model> shrubModelList = new List<Model>();

            //Read the whole header block, and add models based on the count
            byte[] shrubBlock = ReadBlock(fileStream, shrubModelPointer, shrubModelCount * TieModelHeader.TIEELEMSIZE);
            for (int i = 0; i < shrubModelCount; i++)
            {
                TieModelHeader head = new TieModelHeader(shrubBlock, i);
                shrubModelList.Add(new ShrubModel(fileStream, head));
            }
            return shrubModelList;
        }

        protected List<Texture> GetTextures(uint texturePointer, int textureCount)
        {
            List<Texture> textureList = new List<Texture>();

            //Read the whole texture header block, and add textures based on the count
            byte[] textureBlock = ReadBlock(fileStream, texturePointer, textureCount * Texture.TEXTUREELEMSIZE);
            for (int i = 0; i < textureCount; i++)
            {
                textureList.Add(new Texture(textureBlock, i));
            }
            return textureList;
        }

        protected List<Tie> GetTies(List<Model> tieModels, uint tiePointer, int tieCount)
        {
            List<Tie> ties = new List<Tie>();

            //Read the whole texture header block, and add textures based on the count
            byte[] tieBlock = ReadBlock(fileStream, tiePointer, tieCount * 0x70);
            for (int i = 0; i < tieCount; i++)
            {
                Tie tie = new Tie(tieBlock, i, tieModels);
                ties.Add(tie);
            }
            return ties;
        }


        protected List<TerrainModel> GetTerrainModels(uint terrainModelPointer)
        {
            List<TerrainHeader> pointerList = new List<TerrainHeader>();
            //Read the whole terrain header
            byte[] terrainBlock = ReadBlock(fileStream, terrainModelPointer, 0x60);
            uint terrainHeadPointer = ReadUint(terrainBlock, 0);
            ushort terrainHeadCount = ReadUshort(terrainBlock, 0x06);


            byte[] terrainHeadBlock = ReadBlock(fileStream, terrainHeadPointer, terrainHeadCount * 0x30);
            for (int i = 0; i < terrainHeadCount; i++)
            {
                TerrainFragHeader head = new TerrainFragHeader(fileStream, terrainHeadBlock, i);
                if(pointerList.Count < head.slotNum + 1)
                {
                    pointerList.Add(new TerrainHeader(terrainBlock, (head.slotNum * 4)));
                }
                pointerList[head.slotNum].vertexCount += head.vertexCount;
                pointerList[head.slotNum].heads.Add(head);
            }

            List<TerrainModel> terrainModels = new List<TerrainModel>();
            foreach(TerrainHeader hd in pointerList)
            {
                terrainModels.Add(new TerrainModel(fileStream, hd));
            }

            return terrainModels;
        }



        public void Close()
        {
            fileStream.Close();
        }
    }
}
