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
        const int RAC1ENGINESIZE = 0x90;

        public int mobyModelPointer;
        public int renderDefPointer;
        public int type08Pointer;
        public int type0CPointer;

        public int skyboxPointer;
        public int collisionPointer;
        public int playerAnimationPointer;
        public int tieModelPointer;

        public int tieModelCount;
        public int tiePointer;
        public int tieCount;
        public int shrubModelPointer;

        public int shrubModelCount;
        public int shrubPointer;
        public int shrubCount;
        public int terrainPointer;

        public int type40Pointer;
        public int type44Pointer;
        public int soundConfigPointer;
        public int weaponPointer;

        public int weaponCount;
        public int texturePointer;
        public int textureCount;
        public int lightPointer;

        public int lightCount;
        public int lightConfigPointer;
        public int textureConfigMenuPointer;
        public int textureConfigMenuCount;

        public int texture2dPointer;
        public int uiElementPointer;
        public int null1Pointer;
        public int onePointer;

        public int twoPointer;

        public EngineHeader()
        {
            mobyModelPointer = 0;
            renderDefPointer = 0;
            type08Pointer = 0;
            type0CPointer = 0;

            skyboxPointer = 0;
            collisionPointer = 0;
            playerAnimationPointer = 0;
            tieModelPointer = 0;

            tieModelCount = 0;
            tiePointer = 0;
            tieCount = 0;
            shrubModelPointer = 0;

            shrubModelCount = 0;
            shrubPointer = 0;
            shrubCount = 0;
            terrainPointer = 0;

            type40Pointer = 0;
            type44Pointer = 0;
            soundConfigPointer = 0;
            weaponPointer = 0;

            weaponCount = 0;
            texturePointer = 0;
            textureCount = 0;
            lightPointer = 0;

            lightCount = 0;
            lightConfigPointer = 0;
            textureConfigMenuPointer = 0;
            textureConfigMenuCount = 0;

            texture2dPointer = 0;
            uiElementPointer = 0;
        }


        public EngineHeader(FileStream engineFile)
        {
            byte[] engineHeadBlock = new byte[RAC1ENGINESIZE];
            engineFile.Read(engineHeadBlock, 0, RAC1ENGINESIZE);

            mobyModelPointer = ReadInt(engineHeadBlock, 0x00);
            renderDefPointer = ReadInt(engineHeadBlock, 0x04);
            type08Pointer = ReadInt(engineHeadBlock, 0x08);
            type0CPointer = ReadInt(engineHeadBlock, 0x0C);

            skyboxPointer = ReadInt(engineHeadBlock, 0x10);
            collisionPointer = ReadInt(engineHeadBlock, 0x14);
            playerAnimationPointer = ReadInt(engineHeadBlock, 0x18);
            tieModelPointer = ReadInt(engineHeadBlock, 0x1C);

            tieModelCount = ReadInt(engineHeadBlock, 0x20);
            tiePointer = ReadInt(engineHeadBlock, 0x24);
            tieCount = ReadInt(engineHeadBlock, 0x28);
            shrubModelPointer = ReadInt(engineHeadBlock, 0x2C);

            shrubModelCount = ReadInt(engineHeadBlock, 0x30);
            shrubPointer = ReadInt(engineHeadBlock, 0x34);
            shrubCount = ReadInt(engineHeadBlock, 0x38);
            terrainPointer = ReadInt(engineHeadBlock, 0x3C);

            type40Pointer = ReadInt(engineHeadBlock, 0x40);
            type44Pointer = ReadInt(engineHeadBlock, 0x44);
            soundConfigPointer = ReadInt(engineHeadBlock, 0x48);
            weaponPointer = ReadInt(engineHeadBlock, 0x4C);

            weaponCount = ReadInt(engineHeadBlock, 0x50);
            texturePointer = ReadInt(engineHeadBlock, 0x54);
            textureCount = ReadInt(engineHeadBlock, 0x58);
            lightPointer = ReadInt(engineHeadBlock, 0x5C);

            lightCount = ReadInt(engineHeadBlock, 0x60);
            lightConfigPointer = ReadInt(engineHeadBlock, 0x64);
            textureConfigMenuPointer = ReadInt(engineHeadBlock, 0x68);
            textureConfigMenuCount = ReadInt(engineHeadBlock, 0x6C);

            texture2dPointer = ReadInt(engineHeadBlock, 0x70);
            uiElementPointer = ReadInt(engineHeadBlock, 0x74);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x90];

                WriteInt(ref bytes, 0x00, mobyModelPointer);
                WriteInt(ref bytes, 0x04, renderDefPointer);
                WriteInt(ref bytes, 0x08, type08Pointer);
                WriteInt(ref bytes, 0x0C, type0CPointer);

                WriteInt(ref bytes, 0x10, skyboxPointer);
                WriteInt(ref bytes, 0x14, collisionPointer);
                WriteInt(ref bytes, 0x18, playerAnimationPointer);
                WriteInt(ref bytes, 0x1C, tieModelPointer);

                WriteInt(ref bytes, 0x20, tieModelCount);
                WriteInt(ref bytes, 0x24, tiePointer);
                WriteInt(ref bytes, 0x28, tieCount);
                WriteInt(ref bytes, 0x2C, shrubModelPointer);

                WriteInt(ref bytes, 0x30, shrubModelCount);
                WriteInt(ref bytes, 0x34, shrubPointer);
                WriteInt(ref bytes, 0x38, shrubCount);
                WriteInt(ref bytes, 0x3C, terrainPointer);

                WriteInt(ref bytes, 0x40, type40Pointer);
                WriteInt(ref bytes, 0x44, type44Pointer);
                WriteInt(ref bytes, 0x48, soundConfigPointer);
                WriteInt(ref bytes, 0x4C, weaponPointer);

                WriteInt(ref bytes, 0x50, weaponCount);
                WriteInt(ref bytes, 0x54, texturePointer);
                WriteInt(ref bytes, 0x58, textureCount);
                WriteInt(ref bytes, 0x5C, lightPointer);

                WriteInt(ref bytes, 0x60, lightCount);
                WriteInt(ref bytes, 0x64, lightConfigPointer);
                WriteInt(ref bytes, 0x68, textureConfigMenuPointer);
                WriteInt(ref bytes, 0x6C, textureConfigMenuCount);

                WriteInt(ref bytes, 0x70, texture2dPointer);
                WriteInt(ref bytes, 0x74, uiElementPointer);
                //0x78 always 0
                WriteInt(ref bytes, 0x7C, 1);

                WriteInt(ref bytes, 0x80, 2);
                //0x84 always 0
                //0x88 always 0
                //0x8C always 0

            return bytes;
        }
    }
}
