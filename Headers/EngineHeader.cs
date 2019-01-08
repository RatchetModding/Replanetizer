using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class EngineHeader
    {
        const int RAC1ENGINESIZE = 0x78;

        public int mobyModelPointer;
        public int skyboxPointer;
        public int collisionPointer;
        public int tieModelPointer;
        public int tieModelCount;
        public int tiePointer;
        public int tieCount;
        public int shrubModelPointer;
        public int shrubModelCount;
        public int shrubPointer;
        public int shrubCount;
        public int terrainPointer;
        public int texturePointer;
        public int textureCount;
        public int lightingLevel;
        public int textureConfigMenuCount;


        public EngineHeader(FileStream engineFile)
        {
            byte[] engineHeadBlock = new byte[RAC1ENGINESIZE];
            engineFile.Read(engineHeadBlock, 0, RAC1ENGINESIZE);

            mobyModelPointer = ReadInt(engineHeadBlock, 0x00);
            //(0x04)Map render definitions
            //(0x08)null
            //(0x0C)null
            skyboxPointer = ReadInt(engineHeadBlock, 0x10);
            collisionPointer = ReadInt(engineHeadBlock, 0x14);
            //(0x18)Player animations
            tieModelPointer = ReadInt(engineHeadBlock, 0x1C);
            tieModelCount = ReadInt(engineHeadBlock, 0x20);
            tiePointer = ReadInt(engineHeadBlock, 0x24);
            tieCount = ReadInt(engineHeadBlock, 0x28);
            shrubModelPointer = ReadInt(engineHeadBlock, 0x2C);
            shrubModelCount = ReadInt(engineHeadBlock, 0x30);
            shrubPointer = ReadInt(engineHeadBlock, 0x34);
            shrubCount = ReadInt(engineHeadBlock, 0x38);
            terrainPointer = ReadInt(engineHeadBlock, 0x3C);
            //(0x40)null
            //(0x44)null
            //(0x48)Sound config
            //(0x4C)Weapons pointer (rac1)
            //(0x50)Weapons count (rac1)
            texturePointer = ReadInt(engineHeadBlock, 0x54);
            textureCount = ReadInt(engineHeadBlock, 0x58);
            //(0x5C)Lighting pointer
            lightingLevel = ReadInt(engineHeadBlock, 0x60);
            //(0x64)Lighting config
            //(0x68)Texture config menu
            textureConfigMenuCount = ReadInt(engineHeadBlock, 0x6C);
            //(0x70)textureConfig2DGFX
            //(0x74)Sprite def
        }
    }
}
