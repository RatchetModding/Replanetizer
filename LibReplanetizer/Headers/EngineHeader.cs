// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class EngineHeader
    {
        const int RAC123_ENGINE_SIZE = 0x84;
        const int DL_ENGINE_SIZE = 0x98;

        public GameType game = GameType.RaC1;

        public int mobyModelPointer;
        public int renderDefPointer;            // TODO
        public int unk1Pointer;               // TODO     xx
        public int unk2Pointer;               // TODO     xx

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

        public int unk3Pointer;               // TODO     xx
        public int unk4Pointer;               // TODO     xx
        public int soundConfigPointer;          // TODO
        public int gadgetPointer;

        public int gadgetCount;
        public int texturePointer;
        public int textureCount;
        public int lightPointer;

        public int lightCount;
        public int lightConfigPointer;
        public int textureConfigMenuPointer;
        public int textureConfigMenuCount;

        public int texture2dPointer;            // TODO
        public int uiElementPointer;

        public int unk5Pointer;
        public int unk6Pointer;
        public int unk7Pointer;
        public int unk8Pointer;
        public int unk9Pointer;

        public EngineHeader() { }

        public EngineHeader(FileStream engineFile)
        {
            game = DetectGame(engineFile, 0xA0);

            switch (game.num)
            {
                case 1:
                case 2:
                case 3:
                    GetRC123Vals(engineFile);
                    break;
                case 4:
                    GetDLVals(engineFile);
                    break;
                default:
                    GetRC123Vals(engineFile);
                    break;
            }
        }

        private GameType DetectGame(FileStream fileStream, int offset)
        {
            uint magic = ReadUint(ReadBlock(fileStream, offset, 4), 0);
            switch (magic)
            {
                case 0x00000001:
                    return GameType.RaC1;
                case 0xEAA90001:
                    return GameType.RaC2;
                case 0xEAA60001:
                    return GameType.RaC3;
                case 0x008E008D:
                    return GameType.DL;
                default:
                    return GameType.RaC3;
            }
        }

        private void GetRC123Vals(FileStream engineFile)
        {
            byte[] engineHeadBlock = ReadBlock(engineFile, 0, RAC123_ENGINE_SIZE);

            mobyModelPointer = ReadInt(engineHeadBlock, 0x00);
            renderDefPointer = ReadInt(engineHeadBlock, 0x04);
            unk1Pointer = ReadInt(engineHeadBlock, 0x08);
            unk2Pointer = ReadInt(engineHeadBlock, 0x0C);

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

            unk3Pointer = ReadInt(engineHeadBlock, 0x40);
            unk4Pointer = ReadInt(engineHeadBlock, 0x44);
            soundConfigPointer = ReadInt(engineHeadBlock, 0x48);
            gadgetPointer = ReadInt(engineHeadBlock, 0x4C);

            gadgetCount = ReadInt(engineHeadBlock, 0x50);
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

        private void GetDLVals(FileStream engineFile)
        {
            byte[] engineHeadBlock = ReadBlock(engineFile, 0, DL_ENGINE_SIZE);

            mobyModelPointer = ReadInt(engineHeadBlock, 0x00);
            renderDefPointer = ReadInt(engineHeadBlock, 0x04);
            unk1Pointer = ReadInt(engineHeadBlock, 0x08);
            unk2Pointer = ReadInt(engineHeadBlock, 0x0C);

            unk3Pointer = ReadInt(engineHeadBlock, 0x10);
            skyboxPointer = ReadInt(engineHeadBlock, 0x14);
            collisionPointer = ReadInt(engineHeadBlock, 0x18);
            playerAnimationPointer = ReadInt(engineHeadBlock, 0x1C);

            tieModelPointer = ReadInt(engineHeadBlock, 0x20);
            tieModelCount = ReadInt(engineHeadBlock, 0x24);
            tiePointer = ReadInt(engineHeadBlock, 0x28);
            tieCount = ReadInt(engineHeadBlock, 0x2C);

            unk4Pointer = ReadInt(engineHeadBlock, 0x30);
            shrubModelPointer = ReadInt(engineHeadBlock, 0x34);
            shrubModelCount = ReadInt(engineHeadBlock, 0x38);
            shrubPointer = ReadInt(engineHeadBlock, 0x3C);

            shrubCount = ReadInt(engineHeadBlock, 0x40);
            unk5Pointer = ReadInt(engineHeadBlock, 0x44);
            terrainPointer = ReadInt(engineHeadBlock, 0x48);
            unk6Pointer = ReadInt(engineHeadBlock, 0x4C);

            unk7Pointer = ReadInt(engineHeadBlock, 0x50);
            soundConfigPointer = ReadInt(engineHeadBlock, 0x54);
            gadgetPointer = ReadInt(engineHeadBlock, 0x58);
            gadgetCount = ReadInt(engineHeadBlock, 0x5C);

            texturePointer = ReadInt(engineHeadBlock, 0x60);
            textureCount = ReadInt(engineHeadBlock, 0x64);
            lightPointer = ReadInt(engineHeadBlock, 0x68);
            lightCount = ReadInt(engineHeadBlock, 0x6C);

            lightConfigPointer = ReadInt(engineHeadBlock, 0x70);
            textureConfigMenuPointer = ReadInt(engineHeadBlock, 0x74);
            textureConfigMenuCount = ReadInt(engineHeadBlock, 0x78);
            texture2dPointer = ReadInt(engineHeadBlock, 0x7C);

            uiElementPointer = ReadInt(engineHeadBlock, 0x80);
            unk8Pointer = ReadInt(engineHeadBlock, 0x84);
            unk9Pointer = ReadInt(engineHeadBlock, 0x88);
        }

        public byte[] Serialize()
        {
            switch (game.num)
            {
                case 1:
                case 2:
                case 3:
                    return SerializeRC123();
                case 4:
                    return SerializeDL();
                default:
                    return SerializeRC123();
            }
        }

        private byte[] SerializeRC123()
        {
            byte[] bytes = new byte[RAC123_ENGINE_SIZE];

            WriteInt(bytes, 0x00, mobyModelPointer);
            WriteInt(bytes, 0x04, renderDefPointer);
            WriteInt(bytes, 0x08, unk1Pointer);
            WriteInt(bytes, 0x0C, unk2Pointer);

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

            WriteInt(bytes, 0x40, unk3Pointer);
            WriteInt(bytes, 0x44, unk4Pointer);
            WriteInt(bytes, 0x48, soundConfigPointer);
            WriteInt(bytes, 0x4C, gadgetPointer);

            WriteInt(bytes, 0x50, gadgetCount);
            WriteInt(bytes, 0x54, texturePointer);
            WriteInt(bytes, 0x58, textureCount);
            WriteInt(bytes, 0x5C, lightPointer);

            WriteInt(bytes, 0x60, lightCount);
            WriteInt(bytes, 0x64, lightConfigPointer);
            WriteInt(bytes, 0x68, textureConfigMenuPointer);
            WriteInt(bytes, 0x6C, textureConfigMenuCount);

            WriteInt(bytes, 0x70, texture2dPointer);
            WriteInt(bytes, 0x74, uiElementPointer);
            WriteInt(bytes, 0x78, 0);
            WriteInt(bytes, 0x7C, 1);

            WriteInt(bytes, 0x80, 2);

            return bytes;
        }

        private byte[] SerializeDL()
        {
            byte[] bytes = new byte[DL_ENGINE_SIZE];

            WriteInt(bytes, 0x00, mobyModelPointer);
            WriteInt(bytes, 0x04, renderDefPointer);
            WriteInt(bytes, 0x08, unk1Pointer);
            WriteInt(bytes, 0x0C, unk2Pointer);

            WriteInt(bytes, 0x10, unk3Pointer);
            WriteInt(bytes, 0x14, skyboxPointer);
            WriteInt(bytes, 0x18, collisionPointer);
            WriteInt(bytes, 0x1C, playerAnimationPointer);

            WriteInt(bytes, 0x20, tieModelPointer);
            WriteInt(bytes, 0x24, tieModelCount);
            WriteInt(bytes, 0x28, tiePointer);
            WriteInt(bytes, 0x2C, tieCount);

            WriteInt(bytes, 0x30, unk4Pointer);
            WriteInt(bytes, 0x34, shrubModelPointer);
            WriteInt(bytes, 0x38, shrubModelCount);
            WriteInt(bytes, 0x3C, shrubPointer);

            WriteInt(bytes, 0x40, shrubCount);
            WriteInt(bytes, 0x44, unk5Pointer);
            WriteInt(bytes, 0x48, terrainPointer);
            WriteInt(bytes, 0x4C, unk6Pointer);

            WriteInt(bytes, 0x50, unk7Pointer);
            WriteInt(bytes, 0x54, soundConfigPointer);
            WriteInt(bytes, 0x58, gadgetPointer);
            WriteInt(bytes, 0x5C, gadgetCount);

            WriteInt(bytes, 0x60, texturePointer);
            WriteInt(bytes, 0x64, textureCount);
            WriteInt(bytes, 0x68, lightPointer);
            WriteInt(bytes, 0x6C, lightCount);

            WriteInt(bytes, 0x70, lightConfigPointer);
            WriteInt(bytes, 0x74, textureConfigMenuPointer);
            WriteInt(bytes, 0x78, textureConfigMenuCount);
            WriteInt(bytes, 0x7C, texture2dPointer);

            WriteInt(bytes, 0x80, uiElementPointer);
            WriteInt(bytes, 0x84, unk8Pointer);
            WriteInt(bytes, 0x88, unk9Pointer);
            WriteInt(bytes, 0x8C, 0);

            WriteInt(bytes, 0x90, 1);
            WriteInt(bytes, 0x94, 2);

            return bytes;
        }
    }
}
