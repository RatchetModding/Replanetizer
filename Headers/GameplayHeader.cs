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

        public int levelVarPointer;
        public int unkPointer1;
        public int cameraPointer;
        public int unkPointer2;

        public int englishPointer;
        public int lang2Pointer;
        public int frenchPointer;
        public int germanPointer;

        public int spanishPointer;
        public int italianPointer;
        public int unkPointer4;
        public int unkPointer5;

        public int tieIdPointer;
        public int tiePointer;
        public int shrubIdPointer;
        public int shrubPointer;

        public int mobyIdPointer;
        public int mobyPointer;
        public int unkPointer6;
        public int unkPointer7;

        public int unkPointer8;
        public int pvarSizePointer;
        public int pvarPointer;
        public int unkPointer9;

        public int spawnPointPointer;
        public int unkPointer10;
        public int unkPointer11;
        public int unkPointer12;

        public int splinePointer;
        public int unkPointer13;
        public int unkPointer14;
        public int unkPointer15;

        public int unkPointer16;
        public int unkPointer17;
        public int unkPointer18;
        public int occlusionPointer;

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

            levelVarPointer = ReadInt(gameplayHeadBlock, 0x00);
            unkPointer1 = ReadInt(gameplayHeadBlock, 0x04);
            cameraPointer = ReadInt(gameplayHeadBlock, 0x08);
            unkPointer2 = ReadInt(gameplayHeadBlock, 0x0C);

            englishPointer = ReadInt(gameplayHeadBlock, 0x10);//Nullable
            lang2Pointer = ReadInt(gameplayHeadBlock, 0x14);//Nullable
            frenchPointer = ReadInt(gameplayHeadBlock, 0x18);//Nullable
            germanPointer = ReadInt(gameplayHeadBlock, 0x1C);//Nullable

            spanishPointer = ReadInt(gameplayHeadBlock, 0x20);//Nullable
            italianPointer = ReadInt(gameplayHeadBlock, 0x24);//Nullable
            unkPointer4 = ReadInt(gameplayHeadBlock, 0x28);//Nullable
            unkPointer5 = ReadInt(gameplayHeadBlock, 0x2C);//Nullable

            tieIdPointer = ReadInt(gameplayHeadBlock, 0x30);//Nullable
            tiePointer = ReadInt(gameplayHeadBlock, 0x34);//Nullable
            shrubIdPointer = ReadInt(gameplayHeadBlock, 0x38);//Nullable
            shrubPointer = ReadInt(gameplayHeadBlock, 0x3C);//Nullable

            mobyIdPointer = ReadInt(gameplayHeadBlock, 0x40);//Nullable
            mobyPointer = ReadInt(gameplayHeadBlock, 0x44);
            unkPointer6 = ReadInt(gameplayHeadBlock, 0x48);
            unkPointer7 = ReadInt(gameplayHeadBlock, 0x4C);

            unkPointer8 = ReadInt(gameplayHeadBlock, 0x50);//Nullable
            pvarSizePointer = ReadInt(gameplayHeadBlock, 0x54);
            pvarPointer = ReadInt(gameplayHeadBlock, 0x58);
            unkPointer9 = ReadInt(gameplayHeadBlock, 0x5C);

            spawnPointPointer = ReadInt(gameplayHeadBlock, 0x60);
            unkPointer10 = ReadInt(gameplayHeadBlock, 0x64);//Nullable
            unkPointer11 = ReadInt(gameplayHeadBlock, 0x68);//Nullable
            unkPointer12 = ReadInt(gameplayHeadBlock, 0x6C);//Nullable

            splinePointer = ReadInt(gameplayHeadBlock, 0x70);
            unkPointer13= ReadInt(gameplayHeadBlock, 0x74);//Nullable
            unkPointer14 = ReadInt(gameplayHeadBlock, 0x78);//Nullable
            unkPointer15 = ReadInt(gameplayHeadBlock, 0x7C);//Nullable

            unkPointer16 = ReadInt(gameplayHeadBlock, 0x80);//Nullable
            unkPointer17 = ReadInt(gameplayHeadBlock, 0x84);
            unkPointer18 = ReadInt(gameplayHeadBlock, 0x88);//Nullable
            occlusionPointer = ReadInt(gameplayHeadBlock, 0x8C);//Nullable
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[0xB0];

            WriteInt(ref bytes, 0x00, levelVarPointer);
            WriteInt(ref bytes, 0x04, unkPointer1);
            WriteInt(ref bytes, 0x08, cameraPointer);
            WriteInt(ref bytes, 0x0C, unkPointer2);

            WriteInt(ref bytes, 0x10, englishPointer);
            WriteInt(ref bytes, 0x14, lang2Pointer);
            WriteInt(ref bytes, 0x18, frenchPointer);
            WriteInt(ref bytes, 0x1C, germanPointer);

            WriteInt(ref bytes, 0x20, spanishPointer);
            WriteInt(ref bytes, 0x24, italianPointer);
            WriteInt(ref bytes, 0x28, unkPointer4);
            WriteInt(ref bytes, 0x2C, unkPointer5);

            WriteInt(ref bytes, 0x30, tieIdPointer);
            WriteInt(ref bytes, 0x34, tiePointer);
            WriteInt(ref bytes, 0x38, shrubIdPointer);
            WriteInt(ref bytes, 0x3C, shrubPointer);

            WriteInt(ref bytes, 0x40, mobyIdPointer);
            WriteInt(ref bytes, 0x44, mobyPointer);
            WriteInt(ref bytes, 0x48, unkPointer6);
            WriteInt(ref bytes, 0x4C, unkPointer7);

            WriteInt(ref bytes, 0x50, unkPointer8);
            WriteInt(ref bytes, 0x54, pvarSizePointer);
            WriteInt(ref bytes, 0x58, pvarPointer);
            WriteInt(ref bytes, 0x5C, unkPointer9);

            WriteInt(ref bytes, 0x60, spawnPointPointer);
            WriteInt(ref bytes, 0x64, unkPointer10);
            WriteInt(ref bytes, 0x68, unkPointer11);
            WriteInt(ref bytes, 0x6C, unkPointer12);

            WriteInt(ref bytes, 0x70, splinePointer);
            WriteInt(ref bytes, 0x74, unkPointer13);
            WriteInt(ref bytes, 0x78, unkPointer13);
            WriteInt(ref bytes, 0x7C, unkPointer14);

            WriteInt(ref bytes, 0x80, unkPointer16);
            WriteInt(ref bytes, 0x84, unkPointer17);
            WriteInt(ref bytes, 0x88, unkPointer18);
            WriteInt(ref bytes, 0x8C, occlusionPointer);

            return bytes;
        }
    }
}
