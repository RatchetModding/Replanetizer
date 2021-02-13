using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class EngineHeader
    {
        const int RAC1ENGINESIZE = 0x90;

        public int mobyModelPointer;
        public int renderDefPointer;            // TODO
        public int type08Pointer;               // TODO     xx
        public int type0CPointer;               // TODO     xx

        public int skyboxPointer;
        public int collisionPointer;            // TODO
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

        public int type40Pointer;               // TODO     xx
        public int type44Pointer;               // TODO     xx
        public int soundConfigPointer;          // TODO
        public int weaponPointer;

        public int weaponCount;
        public int texturePointer;
        public int textureCount;
        public int lightPointer;

        public int lightCount;
        public int lightConfigPointer;
        public int textureConfigMenuPointer;
        public int textureConfigMenuCount;

        public int texture2dPointer;            // TODO
        public int uiElementPointer;


        public EngineHeader() { }

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

            WriteInt(bytes, 0x00, mobyModelPointer);
            WriteInt(bytes, 0x04, renderDefPointer);
            WriteInt(bytes, 0x08, type08Pointer);
            WriteInt(bytes, 0x0C, type0CPointer);

            WriteInt(bytes, 0x10, skyboxPointer);
            WriteInt(bytes, 0x14, collisionPointer);
            WriteInt(bytes, 0x18, playerAnimationPointer);
            WriteInt(bytes, 0x1C, tieModelPointer);

            WriteInt(bytes, 0x20, tieModelCount);
            WriteInt(bytes, 0x24, tiePointer);
            WriteInt(bytes, 0x28, tieCount);
            WriteInt(bytes, 0x2C, shrubModelPointer);

            WriteInt(bytes, 0x30, shrubModelCount);
            WriteInt(bytes, 0x34, shrubPointer);
            WriteInt(bytes, 0x38, shrubCount);
            WriteInt(bytes, 0x3C, terrainPointer);

            WriteInt(bytes, 0x40, type40Pointer);
            WriteInt(bytes, 0x44, type44Pointer);
            WriteInt(bytes, 0x48, soundConfigPointer);
            WriteInt(bytes, 0x4C, weaponPointer);

            WriteInt(bytes, 0x50, weaponCount);
            WriteInt(bytes, 0x54, texturePointer);
            WriteInt(bytes, 0x58, textureCount);
            WriteInt(bytes, 0x5C, lightPointer);

            WriteInt(bytes, 0x60, lightCount);
            WriteInt(bytes, 0x64, lightConfigPointer);
            WriteInt(bytes, 0x68, textureConfigMenuPointer);
            WriteInt(bytes, 0x6C, textureConfigMenuCount);

            WriteInt(bytes, 0x70, texture2dPointer);
            WriteInt(bytes, 0x74, uiElementPointer);
            //0x78 always 0
            WriteInt(bytes, 0x7C, 1);

            WriteInt(bytes, 0x80, 2);
            //0x84 always 0
            //0x88 always 0
            //0x8C always 0

            return bytes;
        }
    }
}
