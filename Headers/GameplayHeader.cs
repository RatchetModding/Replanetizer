using System.IO;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Headers
{
    public class GameplayHeader
    {
        public const int GAMEPLAYSIZE = 0xA0;

        public int levelVarPointer;
        public int type04Pointer;
        public int cameraPointer;
        public int type0CPointer;

        public int englishPointer;
        public int lang2Pointer;
        public int frenchPointer;
        public int germanPointer;

        public int spanishPointer;
        public int italianPointer;
        public int lang7Pointer;
        public int lang8Pointer;

        public int tieIdPointer;
        public int tiePointer;
        public int shrubIdPointer;
        public int shrubPointer;

        public int mobyIdPointer;
        public int mobyPointer;
        public int unkPointer6;
        public int unkPointer7;

        public int type50Pointer;
        public int pvarSizePointer;
        public int pvarPointer;
        public int type5CPointer;

        public int cuboidPointer;
        public int type64Pointer;
        public int type68Pointer;
        public int unkPointer12;

        public int splinePointer;
        public int unkPointer13;
        public int unkPointer14;
        public int type7CPointer;

        public int type80Pointer;
        public int unkPointer17;
        public int type88Pointer;
        public int occlusionPointer;

        public GameplayHeader(){}

        public GameplayHeader(GameType game, FileStream gameplayFile)
        {
            byte[] gameplayHeadBlock = new byte[GAMEPLAYSIZE];
            gameplayFile.Read(gameplayHeadBlock, 0, GAMEPLAYSIZE);

            switch (game.num)
            {
                case 1:
                    GetRC1Vals(gameplayHeadBlock);
                    break;
                case 2:
                case 3:
                    GetRC2Vals(gameplayHeadBlock);
                    break;
            }
        }

        private void GetRC1Vals(byte[] gameplayHeadBlock)
        {
            levelVarPointer = ReadInt(gameplayHeadBlock, 0x00);
            type04Pointer = ReadInt(gameplayHeadBlock, 0x04);
            cameraPointer = ReadInt(gameplayHeadBlock, 0x08);
            type0CPointer = ReadInt(gameplayHeadBlock, 0x0C);

            englishPointer = ReadInt(gameplayHeadBlock, 0x10);
            lang2Pointer = ReadInt(gameplayHeadBlock, 0x14);
            frenchPointer = ReadInt(gameplayHeadBlock, 0x18);
            germanPointer = ReadInt(gameplayHeadBlock, 0x1C);

            spanishPointer = ReadInt(gameplayHeadBlock, 0x20);
            italianPointer = ReadInt(gameplayHeadBlock, 0x24);
            lang7Pointer = ReadInt(gameplayHeadBlock, 0x28);
            lang8Pointer = ReadInt(gameplayHeadBlock, 0x2C);

            tieIdPointer = ReadInt(gameplayHeadBlock, 0x30);
            tiePointer = ReadInt(gameplayHeadBlock, 0x34);
            shrubIdPointer = ReadInt(gameplayHeadBlock, 0x38);
            shrubPointer = ReadInt(gameplayHeadBlock, 0x3C);

            mobyIdPointer = ReadInt(gameplayHeadBlock, 0x40);
            mobyPointer = ReadInt(gameplayHeadBlock, 0x44);
            unkPointer6 = ReadInt(gameplayHeadBlock, 0x48);
            unkPointer7 = ReadInt(gameplayHeadBlock, 0x4C);

            type50Pointer = ReadInt(gameplayHeadBlock, 0x50);
            pvarSizePointer = ReadInt(gameplayHeadBlock, 0x54);
            pvarPointer = ReadInt(gameplayHeadBlock, 0x58);
            type5CPointer = ReadInt(gameplayHeadBlock, 0x5C);

            cuboidPointer = ReadInt(gameplayHeadBlock, 0x60);
            type64Pointer = ReadInt(gameplayHeadBlock, 0x64);
            type68Pointer = ReadInt(gameplayHeadBlock, 0x68);
            unkPointer12 = ReadInt(gameplayHeadBlock, 0x6C);

            splinePointer = ReadInt(gameplayHeadBlock, 0x70);
            unkPointer13 = ReadInt(gameplayHeadBlock, 0x74);
            unkPointer14 = ReadInt(gameplayHeadBlock, 0x78);
            type7CPointer = ReadInt(gameplayHeadBlock, 0x7C);

            type80Pointer = ReadInt(gameplayHeadBlock, 0x80);
            unkPointer17 = ReadInt(gameplayHeadBlock, 0x84);
            type88Pointer = ReadInt(gameplayHeadBlock, 0x88);
            occlusionPointer = ReadInt(gameplayHeadBlock, 0x8C);
        }

        private void GetRC2Vals(byte[] gameplayHeadBlock)
        {
            levelVarPointer = ReadInt(gameplayHeadBlock, 0x00);
            type04Pointer = ReadInt(gameplayHeadBlock, 0x04);
            cameraPointer = ReadInt(gameplayHeadBlock, 0x08);
            type0CPointer = ReadInt(gameplayHeadBlock, 0x0C);

            englishPointer = ReadInt(gameplayHeadBlock, 0x10);
            lang2Pointer = ReadInt(gameplayHeadBlock, 0x14);
            frenchPointer = ReadInt(gameplayHeadBlock, 0x18);
            germanPointer = ReadInt(gameplayHeadBlock, 0x1C);

            spanishPointer = ReadInt(gameplayHeadBlock, 0x20);
            italianPointer = ReadInt(gameplayHeadBlock, 0x24);
            lang7Pointer = ReadInt(gameplayHeadBlock, 0x28);
            lang8Pointer = ReadInt(gameplayHeadBlock, 0x2C);

            tieIdPointer = ReadInt(gameplayHeadBlock, 0x30);
            // = ReadInt(gameplayHeadBlock, 0x34);
            // = ReadInt(gameplayHeadBlock, 0x38);
            shrubIdPointer = ReadInt(gameplayHeadBlock, 0x3C);

            shrubPointer = ReadInt(gameplayHeadBlock, 0x40);
            // = ReadInt(gameplayHeadBlock, 0x44);
            mobyIdPointer = ReadInt(gameplayHeadBlock, 0x48);
            mobyPointer = ReadInt(gameplayHeadBlock, 0x4C);

            unkPointer6 = ReadInt(gameplayHeadBlock, 0x50);
            unkPointer7 = ReadInt(gameplayHeadBlock, 0x54);
            type50Pointer = ReadInt(gameplayHeadBlock, 0x58);
            pvarSizePointer = ReadInt(gameplayHeadBlock, 0x5C);

            pvarPointer = ReadInt(gameplayHeadBlock, 0x60);
            type5CPointer = ReadInt(gameplayHeadBlock, 0x64);
            cuboidPointer = ReadInt(gameplayHeadBlock, 0x68);
            type64Pointer = ReadInt(gameplayHeadBlock, 0x6C);

            type68Pointer = ReadInt(gameplayHeadBlock, 0x70);
            unkPointer12 = ReadInt(gameplayHeadBlock, 0x74);
            splinePointer = ReadInt(gameplayHeadBlock, 0x78);
            unkPointer13 = ReadInt(gameplayHeadBlock, 0x7C);

            // = ReadInt(gameplayHeadBlock, 0x80);
            type80Pointer = ReadInt(gameplayHeadBlock, 0x84);
            unkPointer17 = ReadInt(gameplayHeadBlock, 0x88);
            // = ReadInt(gameplayHeadBlock, 0x8C);

            occlusionPointer = ReadInt(gameplayHeadBlock, 0x90);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[GAMEPLAYSIZE];

            WriteInt(bytes, 0x00, levelVarPointer);
            WriteInt(bytes, 0x04, type04Pointer);
            WriteInt(bytes, 0x08, cameraPointer);
            WriteInt(bytes, 0x0C, type0CPointer);

            WriteInt(bytes, 0x10, englishPointer);
            WriteInt(bytes, 0x14, lang2Pointer);
            WriteInt(bytes, 0x18, frenchPointer);
            WriteInt(bytes, 0x1C, germanPointer);

            WriteInt(bytes, 0x20, spanishPointer);
            WriteInt(bytes, 0x24, italianPointer);
            WriteInt(bytes, 0x28, lang7Pointer);
            WriteInt(bytes, 0x2C, lang8Pointer);

            WriteInt(bytes, 0x30, tieIdPointer);
            WriteInt(bytes, 0x34, tiePointer);
            WriteInt(bytes, 0x38, shrubIdPointer);
            WriteInt(bytes, 0x3C, shrubPointer);

            WriteInt(bytes, 0x40, mobyIdPointer);
            WriteInt(bytes, 0x44, mobyPointer);
            WriteInt(bytes, 0x48, unkPointer6);
            WriteInt(bytes, 0x4C, unkPointer7);

            WriteInt(bytes, 0x50, type50Pointer);
            WriteInt(bytes, 0x54, pvarSizePointer);
            WriteInt(bytes, 0x58, pvarPointer);
            WriteInt(bytes, 0x5C, type5CPointer);

            WriteInt(bytes, 0x60, cuboidPointer);
            WriteInt(bytes, 0x64, type64Pointer);
            WriteInt(bytes, 0x68, type68Pointer);
            WriteInt(bytes, 0x6C, unkPointer12);

            WriteInt(bytes, 0x70, splinePointer);
            WriteInt(bytes, 0x74, unkPointer13);
            WriteInt(bytes, 0x78, unkPointer14);
            WriteInt(bytes, 0x7C, type7CPointer);

            WriteInt(bytes, 0x80, type80Pointer);
            WriteInt(bytes, 0x84, unkPointer17);
            WriteInt(bytes, 0x88, type88Pointer);
            WriteInt(bytes, 0x8C, occlusionPointer);

            return bytes;
        }
    }
}
