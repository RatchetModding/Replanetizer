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

        public uint mobyModelPointer;
        public uint skyboxPointer;
        public uint collisionPointer;
        public uint tieModelPointer;
        public int tieModelCount;
        public uint tiePointer;
        public int tieCount;
        public uint shrubModelPointer;
        public int shrubModelCount;
        public uint shrubPointer;
        public int shrubCount;
        public uint terrainPointer;
        public uint texturePointer;
        public int textureCount;
        public uint lightingLevel;
        public uint textureConfigMenuCount;
        

        public EngineHeader(FileStream engineFile)
        {
            byte[] engineHeadBlock = new byte[RAC1ENGINESIZE];
            engineFile.Read(engineHeadBlock, 0, RAC1ENGINESIZE);

            mobyModelPointer =      ReadUint(engineHeadBlock, 0x00);
            //(0x04)Map render definitions
            //(0x08)null
            //(0x0C)null
            skyboxPointer = ReadUint(engineHeadBlock, 0x10);
            collisionPointer =      ReadUint(engineHeadBlock, 0x14);
            //(0x18)Player animations
            tieModelPointer =     ReadUint(engineHeadBlock, 0x1C);
            tieModelCount =       ReadInt(engineHeadBlock, 0x20);
            tiePointer =    ReadUint(engineHeadBlock, 0x24);
            tieCount =      ReadInt(engineHeadBlock, 0x28);
            shrubModelPointer =   ReadUint(engineHeadBlock, 0x2C);
            shrubModelCount =     ReadInt(engineHeadBlock, 0x30);
            shrubPointer =  ReadUint(engineHeadBlock, 0x34);
            shrubCount =    ReadInt(engineHeadBlock, 0x38);
            terrainPointer =        ReadUint(engineHeadBlock, 0x3C);
            //(0x40)null
            //(0x44)null
            //(0x48)Sound config
            //(0x4C)Weapons pointer (rac1)
            //(0x50)Weapons count (rac1)
            texturePointer =        ReadUint(engineHeadBlock, 0x54);
            textureCount =          ReadInt(engineHeadBlock, 0x58);
            //(0x5C)Lighting pointer
            lightingLevel =         ReadUint(engineHeadBlock, 0x60);
            //(0x64)Lighting config
            //(0x68)Texture config menu
            textureConfigMenuCount = ReadUint(engineHeadBlock, 0x6C);
            //(0x70)textureConfig2DGFX
            //(0x74)Sprite def
        }
    }
}
