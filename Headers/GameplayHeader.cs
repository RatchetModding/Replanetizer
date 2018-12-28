using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class GameplayHeader
    {
        public const int RAC1GAMEPLAYSIZE = 0x90;

        public uint levelVarPointer;
        public uint unkPointer1;
        public uint cameraPointer;
        public uint unkPointer2;

        public uint englishPointer;
        public uint lang2Pointer;
        public uint frenchPointer;
        public uint germanPointer;

        public uint spanishPointer;
        public uint italianPointer;
        public uint unkPointer4;
        public uint unkPointer5;

        public uint tieIdPointer;
        public uint tiePointer;
        public uint shrubIdPointer;
        public uint shrubPointer;

        public uint mobyIdPointer;
        public uint mobyPointer;
        public uint unkPointer6;
        public uint unkPointer7;

        public uint unkPointer8;
        public uint pvarSizePointer;
        public uint pvarPointer;
        public uint unkPointer9;

        public uint spawnPointPointer;
        public uint unkPointer10;
        public uint unkPointer11;
        public uint unkPointer12;

        public uint splinePointer;
        public uint unkPointer13;
        public uint unkPointer14;
        public uint unkPointer15;

        public uint unkPointer16;
        public uint unkPointer17;
        public uint unkPointer18;
        public uint occlusionPointer;

        public GameplayHeader()
        {
            levelVarPointer = 0;
            unkPointer1 = 0;
            cameraPointer = 0;
            unkPointer2 = 0;
            englishPointer = 0;
            lang2Pointer = 0;
            frenchPointer = 0;
            germanPointer = 0;
            spanishPointer = 0;
            italianPointer = 0;
            unkPointer4 = 0;
            unkPointer5 = 0;
            tieIdPointer = 0;
            tiePointer = 0;
            shrubIdPointer = 0;
            shrubPointer = 0;
            mobyIdPointer = 0;
            mobyPointer = 0;
            unkPointer6 = 0;
            unkPointer7 = 0;
            unkPointer8 = 0;
            pvarSizePointer = 0;
            pvarPointer = 0;
            unkPointer9 = 0;
            spawnPointPointer = 0;
            unkPointer10 = 0;
            unkPointer11 = 0;
            unkPointer12 = 0;
            splinePointer = 0;
            unkPointer13 = 0;
            unkPointer14 = 0;
            unkPointer15 = 0;
            unkPointer16 = 0;
            unkPointer17 = 0;
            unkPointer18 = 0;
            occlusionPointer = 0;
        }

        public GameplayHeader(FileStream gameplayFile)
        {
            byte[] gameplayHeadBlock = new byte[RAC1GAMEPLAYSIZE];
            gameplayFile.Read(gameplayHeadBlock, 0, RAC1GAMEPLAYSIZE);

            levelVarPointer = ReadUint(gameplayHeadBlock, 0x00);
            unkPointer1 = ReadUint(gameplayHeadBlock, 0x04);
            cameraPointer = ReadUint(gameplayHeadBlock, 0x08);
            unkPointer2 = ReadUint(gameplayHeadBlock, 0x0C);

            englishPointer = ReadUint(gameplayHeadBlock, 0x10);//Nullable
            lang2Pointer = ReadUint(gameplayHeadBlock, 0x14);//Nullable
            frenchPointer = ReadUint(gameplayHeadBlock, 0x18);//Nullable
            germanPointer = ReadUint(gameplayHeadBlock, 0x1C);//Nullable

            spanishPointer = ReadUint(gameplayHeadBlock, 0x20);//Nullable
            italianPointer = ReadUint(gameplayHeadBlock, 0x24);//Nullable
            unkPointer4 = ReadUint(gameplayHeadBlock, 0x28);//Nullable
            unkPointer5 = ReadUint(gameplayHeadBlock, 0x2C);//Nullable

            tieIdPointer = ReadUint(gameplayHeadBlock, 0x30);//Nullable
            tiePointer = ReadUint(gameplayHeadBlock, 0x34);//Nullable
            shrubIdPointer = ReadUint(gameplayHeadBlock, 0x38);//Nullable
            shrubPointer = ReadUint(gameplayHeadBlock, 0x3C);//Nullable

            mobyIdPointer = ReadUint(gameplayHeadBlock, 0x40);//Nullable
            mobyPointer = ReadUint(gameplayHeadBlock, 0x44);
            unkPointer6 = ReadUint(gameplayHeadBlock, 0x48);
            unkPointer7 = ReadUint(gameplayHeadBlock, 0x4C);

            unkPointer8 = ReadUint(gameplayHeadBlock, 0x50);//Nullable
            pvarSizePointer = ReadUint(gameplayHeadBlock, 0x54);
            pvarPointer = ReadUint(gameplayHeadBlock, 0x58);
            unkPointer9 = ReadUint(gameplayHeadBlock, 0x5C);

            spawnPointPointer = ReadUint(gameplayHeadBlock, 0x60);
            unkPointer10 = ReadUint(gameplayHeadBlock, 0x64);//Nullable
            unkPointer11 = ReadUint(gameplayHeadBlock, 0x68);//Nullable
            unkPointer12 = ReadUint(gameplayHeadBlock, 0x6C);//Nullable

            splinePointer = ReadUint(gameplayHeadBlock, 0x70);
            unkPointer13= ReadUint(gameplayHeadBlock, 0x74);//Nullable
            unkPointer14 = ReadUint(gameplayHeadBlock, 0x78);//Nullable
            unkPointer15 = ReadUint(gameplayHeadBlock, 0x7C);//Nullable

            unkPointer16 = ReadUint(gameplayHeadBlock, 0x80);//Nullable
            unkPointer17 = ReadUint(gameplayHeadBlock, 0x84);
            unkPointer18 = ReadUint(gameplayHeadBlock, 0x88);//Nullable
            occlusionPointer = ReadUint(gameplayHeadBlock, 0x8C);//Nullable
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[0xB0];

            WriteUint(ref bytes, 0x00, levelVarPointer);
            WriteUint(ref bytes, 0x04, unkPointer1);
            WriteUint(ref bytes, 0x08, cameraPointer);
            WriteUint(ref bytes, 0x0C, unkPointer2);

            WriteUint(ref bytes, 0x10, englishPointer);
            WriteUint(ref bytes, 0x14, lang2Pointer);
            WriteUint(ref bytes, 0x18, frenchPointer);
            WriteUint(ref bytes, 0x1C, germanPointer);

            WriteUint(ref bytes, 0x20, spanishPointer);
            WriteUint(ref bytes, 0x24, italianPointer);
            WriteUint(ref bytes, 0x28, unkPointer4);
            WriteUint(ref bytes, 0x2C, unkPointer5);

            WriteUint(ref bytes, 0x30, tieIdPointer);
            WriteUint(ref bytes, 0x34, tiePointer);
            WriteUint(ref bytes, 0x38, shrubIdPointer);
            WriteUint(ref bytes, 0x3C, shrubPointer);

            WriteUint(ref bytes, 0x40, mobyIdPointer);
            WriteUint(ref bytes, 0x44, mobyPointer);
            WriteUint(ref bytes, 0x48, unkPointer6);
            WriteUint(ref bytes, 0x4C, unkPointer7);

            WriteUint(ref bytes, 0x50, unkPointer8);
            WriteUint(ref bytes, 0x54, pvarSizePointer);
            WriteUint(ref bytes, 0x58, pvarPointer);
            WriteUint(ref bytes, 0x5C, unkPointer9);

            WriteUint(ref bytes, 0x60, spawnPointPointer);
            WriteUint(ref bytes, 0x64, unkPointer10);
            WriteUint(ref bytes, 0x68, unkPointer11);
            WriteUint(ref bytes, 0x6C, unkPointer12);

            WriteUint(ref bytes, 0x70, splinePointer);
            WriteUint(ref bytes, 0x74, unkPointer13);
            WriteUint(ref bytes, 0x78, unkPointer13);
            WriteUint(ref bytes, 0x7C, unkPointer14);

            WriteUint(ref bytes, 0x80, unkPointer16);
            WriteUint(ref bytes, 0x84, unkPointer17);
            WriteUint(ref bytes, 0x88, unkPointer18);
            WriteUint(ref bytes, 0x8C, occlusionPointer);

            return bytes;
        }
    }
}
