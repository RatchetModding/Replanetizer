using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class GameplayHeader
    {
        public const int GAMEPLAYSIZE = 0xA0;

        public int levelVarPointer;
        public int lightsPointer;
        public int cameraPointer;
        public int soundPointer;

        public int englishPointer;
        public int ukenglishPointer;
        public int frenchPointer;
        public int germanPointer;

        public int spanishPointer;
        public int italianPointer;
        public int japanesePointer;
        public int koreanPointer;

        public int tieIdPointer;
        public int tiePointer;
        public int shrubIdPointer;
        public int shrubPointer;

        public int mobyIdPointer;
        public int mobyPointer;
        public int mobyGroupsPointer;
        public int unkPointer7;

        public int type50Pointer;
        public int pvarSizePointer;
        public int pvarPointer;
        public int type5CPointer;

        public int cuboidPointer;
        public int spherePointer;
        public int cylinderPointer;
        public int unkPointer12;

        public int splinePointer;
        public int grindPathsPointer;
        public int unkPointer14;
        public int type7CPointer;

        public int type80Pointer;
        public int unkPointer17;
        public int type88Pointer;
        public int occlusionPointer;

        public GameplayHeader() { }

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
                    GetRC23Vals(gameplayHeadBlock);
                    break;
                case 4:
                    GetDLVals(gameplayHeadBlock);
                    break;
                default:
                    GetRC23Vals(gameplayHeadBlock);
                    break;
            }
        }

        private void GetRC1Vals(byte[] gameplayHeadBlock)
        {
            levelVarPointer = ReadInt(gameplayHeadBlock, 0x00);
            lightsPointer = ReadInt(gameplayHeadBlock, 0x04);
            cameraPointer = ReadInt(gameplayHeadBlock, 0x08);
            soundPointer = ReadInt(gameplayHeadBlock, 0x0C);

            englishPointer = ReadInt(gameplayHeadBlock, 0x10);
            ukenglishPointer = ReadInt(gameplayHeadBlock, 0x14);
            frenchPointer = ReadInt(gameplayHeadBlock, 0x18);
            germanPointer = ReadInt(gameplayHeadBlock, 0x1C);

            spanishPointer = ReadInt(gameplayHeadBlock, 0x20);
            italianPointer = ReadInt(gameplayHeadBlock, 0x24);
            japanesePointer = ReadInt(gameplayHeadBlock, 0x28);
            koreanPointer = ReadInt(gameplayHeadBlock, 0x2C);

            tieIdPointer = ReadInt(gameplayHeadBlock, 0x30);
            tiePointer = ReadInt(gameplayHeadBlock, 0x34);
            shrubIdPointer = ReadInt(gameplayHeadBlock, 0x38);
            shrubPointer = ReadInt(gameplayHeadBlock, 0x3C);

            mobyIdPointer = ReadInt(gameplayHeadBlock, 0x40);
            mobyPointer = ReadInt(gameplayHeadBlock, 0x44);
            mobyGroupsPointer = ReadInt(gameplayHeadBlock, 0x48);
            unkPointer7 = ReadInt(gameplayHeadBlock, 0x4C);

            type50Pointer = ReadInt(gameplayHeadBlock, 0x50);
            pvarSizePointer = ReadInt(gameplayHeadBlock, 0x54);
            pvarPointer = ReadInt(gameplayHeadBlock, 0x58);
            type5CPointer = ReadInt(gameplayHeadBlock, 0x5C);

            cuboidPointer = ReadInt(gameplayHeadBlock, 0x60);
            spherePointer = ReadInt(gameplayHeadBlock, 0x64);
            cylinderPointer = ReadInt(gameplayHeadBlock, 0x68);
            unkPointer12 = ReadInt(gameplayHeadBlock, 0x6C);

            splinePointer = ReadInt(gameplayHeadBlock, 0x70);
            grindPathsPointer = ReadInt(gameplayHeadBlock, 0x74);
            unkPointer14 = ReadInt(gameplayHeadBlock, 0x78);
            type7CPointer = ReadInt(gameplayHeadBlock, 0x7C);

            type80Pointer = ReadInt(gameplayHeadBlock, 0x80);
            unkPointer17 = ReadInt(gameplayHeadBlock, 0x84);
            type88Pointer = ReadInt(gameplayHeadBlock, 0x88);
            occlusionPointer = ReadInt(gameplayHeadBlock, 0x8C);
        }

        private void GetRC23Vals(byte[] gameplayHeadBlock)
        {
            levelVarPointer = ReadInt(gameplayHeadBlock, 0x00);
            lightsPointer = ReadInt(gameplayHeadBlock, 0x04);
            cameraPointer = ReadInt(gameplayHeadBlock, 0x08);
            soundPointer = ReadInt(gameplayHeadBlock, 0x0C);

            englishPointer = ReadInt(gameplayHeadBlock, 0x10);
            ukenglishPointer = ReadInt(gameplayHeadBlock, 0x14);
            frenchPointer = ReadInt(gameplayHeadBlock, 0x18);
            germanPointer = ReadInt(gameplayHeadBlock, 0x1C);

            spanishPointer = ReadInt(gameplayHeadBlock, 0x20);
            italianPointer = ReadInt(gameplayHeadBlock, 0x24);
            japanesePointer = ReadInt(gameplayHeadBlock, 0x28);
            koreanPointer = ReadInt(gameplayHeadBlock, 0x2C);

            tieIdPointer = ReadInt(gameplayHeadBlock, 0x30);
            //tiePointer = ReadInt(gameplayHeadBlock, 0x34);
            //tieGroupsPointer = ReadInt(gameplayHeadBlock, 0x38);
            shrubIdPointer = ReadInt(gameplayHeadBlock, 0x3C);

            shrubPointer = ReadInt(gameplayHeadBlock, 0x40);
            //shrubGroupsPointer = ReadInt(gameplayHeadBlock, 0x44);
            mobyIdPointer = ReadInt(gameplayHeadBlock, 0x48);
            mobyPointer = ReadInt(gameplayHeadBlock, 0x4C);

            mobyGroupsPointer = ReadInt(gameplayHeadBlock, 0x50);
            unkPointer7 = ReadInt(gameplayHeadBlock, 0x54);
            type50Pointer = ReadInt(gameplayHeadBlock, 0x58);
            pvarSizePointer = ReadInt(gameplayHeadBlock, 0x5C);

            pvarPointer = ReadInt(gameplayHeadBlock, 0x60);
            type5CPointer = ReadInt(gameplayHeadBlock, 0x64);
            cuboidPointer = ReadInt(gameplayHeadBlock, 0x68);
            spherePointer = ReadInt(gameplayHeadBlock, 0x6C);

            cylinderPointer = ReadInt(gameplayHeadBlock, 0x70);
            unkPointer12 = ReadInt(gameplayHeadBlock, 0x74);
            splinePointer = ReadInt(gameplayHeadBlock, 0x78);
            grindPathsPointer = ReadInt(gameplayHeadBlock, 0x7C);

            // = ReadInt(gameplayHeadBlock, 0x80);
            type80Pointer = ReadInt(gameplayHeadBlock, 0x84);
            unkPointer17 = ReadInt(gameplayHeadBlock, 0x88);
            // = ReadInt(gameplayHeadBlock, 0x8C);

            occlusionPointer = ReadInt(gameplayHeadBlock, 0x90);
            // tieAmbientPointer = ReadInt(gameplayHeadBlock, 0x94);
            // areasPointer = ReadInt(gameplayHeadBlock, 0x98);
        }

        private void GetDLVals(byte[] gameplayHeadBlock)
        {
            levelVarPointer = ReadInt(gameplayHeadBlock, 0x00);
            cameraPointer = ReadInt(gameplayHeadBlock, 0x04);
            soundPointer = ReadInt(gameplayHeadBlock, 0x08);
            englishPointer = ReadInt(gameplayHeadBlock, 0x0C);

            ukenglishPointer = ReadInt(gameplayHeadBlock, 0x10); 
            frenchPointer = ReadInt(gameplayHeadBlock, 0x14);
            germanPointer = ReadInt(gameplayHeadBlock, 0x18);
            spanishPointer = ReadInt(gameplayHeadBlock, 0x1C);

            italianPointer = ReadInt(gameplayHeadBlock, 0x20);
            japanesePointer = ReadInt(gameplayHeadBlock, 0x24);
            koreanPointer = ReadInt(gameplayHeadBlock, 0x28);
            mobyIdPointer = ReadInt(gameplayHeadBlock, 0x2C);

            mobyPointer = ReadInt(gameplayHeadBlock, 0x30);
            
            pvarSizePointer = ReadInt(gameplayHeadBlock, 0x40);
            pvarPointer = ReadInt(gameplayHeadBlock, 0x44);
            cuboidPointer = ReadInt(gameplayHeadBlock, 0x4C);

            spherePointer = ReadInt(gameplayHeadBlock, 0x50);
            cylinderPointer = ReadInt(gameplayHeadBlock, 0x54);
            splinePointer = ReadInt(gameplayHeadBlock, 0x5C);
            /*
            type04Pointer = ReadInt(gameplayHeadBlock, 0x04);
            
            

            lang2Pointer = ReadInt(gameplayHeadBlock, 0x14);

            tieIdPointer = ReadInt(gameplayHeadBlock, 0x30);
            // = ReadInt(gameplayHeadBlock, 0x34);
            // = ReadInt(gameplayHeadBlock, 0x38);
            shrubIdPointer = ReadInt(gameplayHeadBlock, 0x3C);

            shrubPointer = ReadInt(gameplayHeadBlock, 0x40);
            // = ReadInt(gameplayHeadBlock, 0x44);

            unkPointer6 = ReadInt(gameplayHeadBlock, 0x50);
            unkPointer7 = ReadInt(gameplayHeadBlock, 0x54);
            type50Pointer = ReadInt(gameplayHeadBlock, 0x58);

            type5CPointer = ReadInt(gameplayHeadBlock, 0x64);
            type64Pointer = ReadInt(gameplayHeadBlock, 0x6C);

            type68Pointer = ReadInt(gameplayHeadBlock, 0x70);
            unkPointer12 = ReadInt(gameplayHeadBlock, 0x74);
            unkPointer13 = ReadInt(gameplayHeadBlock, 0x7C);

            // = ReadInt(gameplayHeadBlock, 0x80);
            type80Pointer = ReadInt(gameplayHeadBlock, 0x84);
            unkPointer17 = ReadInt(gameplayHeadBlock, 0x88);
            // = ReadInt(gameplayHeadBlock, 0x8C);

            occlusionPointer = ReadInt(gameplayHeadBlock, 0x90);*/
        }

        public byte[] Serialize(GameType game)
        {
            switch (game.num)
            {
                case 1:
                    return SerializeRC1();
                case 2:
                case 3:
                    return SerializeRC23();
                case 4:
                    return SerializeDL();
                default:
                    return SerializeRC23();
            }
        }

        private byte[] SerializeRC1()
        {
            byte[] bytes = new byte[GAMEPLAYSIZE];

            WriteInt(bytes, 0x00, levelVarPointer);
            WriteInt(bytes, 0x04, lightsPointer);
            WriteInt(bytes, 0x08, cameraPointer);
            WriteInt(bytes, 0x0C, soundPointer);

            WriteInt(bytes, 0x10, englishPointer);
            WriteInt(bytes, 0x14, ukenglishPointer);
            WriteInt(bytes, 0x18, frenchPointer);
            WriteInt(bytes, 0x1C, germanPointer);

            WriteInt(bytes, 0x20, spanishPointer);
            WriteInt(bytes, 0x24, italianPointer);
            WriteInt(bytes, 0x28, japanesePointer);
            WriteInt(bytes, 0x2C, koreanPointer);

            WriteInt(bytes, 0x30, tieIdPointer);
            WriteInt(bytes, 0x34, tiePointer);
            WriteInt(bytes, 0x38, shrubIdPointer);
            WriteInt(bytes, 0x3C, shrubPointer);

            WriteInt(bytes, 0x40, mobyIdPointer);
            WriteInt(bytes, 0x44, mobyPointer);
            WriteInt(bytes, 0x48, mobyGroupsPointer);
            WriteInt(bytes, 0x4C, unkPointer7);

            WriteInt(bytes, 0x50, type50Pointer);
            WriteInt(bytes, 0x54, pvarSizePointer);
            WriteInt(bytes, 0x58, pvarPointer);
            WriteInt(bytes, 0x5C, type5CPointer);

            WriteInt(bytes, 0x60, cuboidPointer);
            WriteInt(bytes, 0x64, spherePointer);
            WriteInt(bytes, 0x68, cylinderPointer);
            WriteInt(bytes, 0x6C, unkPointer12);

            WriteInt(bytes, 0x70, splinePointer);
            WriteInt(bytes, 0x74, grindPathsPointer);
            WriteInt(bytes, 0x78, unkPointer14);
            WriteInt(bytes, 0x7C, type7CPointer);

            WriteInt(bytes, 0x80, type80Pointer);
            WriteInt(bytes, 0x84, unkPointer17);
            WriteInt(bytes, 0x88, type88Pointer);
            WriteInt(bytes, 0x8C, occlusionPointer);

            return bytes;
        }

        private byte[] SerializeRC23()
        {
            byte[] bytes = new byte[GAMEPLAYSIZE];

            WriteInt(bytes, 0x00, levelVarPointer);
            WriteInt(bytes, 0x04, lightsPointer);
            WriteInt(bytes, 0x08, cameraPointer);
            WriteInt(bytes, 0x0C, soundPointer);

            WriteInt(bytes, 0x10, englishPointer);
            WriteInt(bytes, 0x14, ukenglishPointer);
            WriteInt(bytes, 0x18, frenchPointer);
            WriteInt(bytes, 0x1C, germanPointer);

            WriteInt(bytes, 0x20, spanishPointer);
            WriteInt(bytes, 0x24, italianPointer);
            WriteInt(bytes, 0x28, japanesePointer);
            WriteInt(bytes, 0x2C, koreanPointer);

            WriteInt(bytes, 0x30, tieIdPointer);
            //0x34
            //0x38
            WriteInt(bytes, 0x3C, shrubIdPointer);

            WriteInt(bytes, 0x40, shrubPointer);
            //0x44
            WriteInt(bytes, 0x48, mobyIdPointer);
            WriteInt(bytes, 0x4C, mobyPointer);

            WriteInt(bytes, 0x50, mobyGroupsPointer);
            WriteInt(bytes, 0x54, unkPointer7);
            WriteInt(bytes, 0x58, type50Pointer);
            WriteInt(bytes, 0x5C, pvarSizePointer);

            WriteInt(bytes, 0x60, pvarPointer);
            WriteInt(bytes, 0x64, type5CPointer);
            WriteInt(bytes, 0x68, cuboidPointer);
            WriteInt(bytes, 0x6C, spherePointer);

            WriteInt(bytes, 0x70, cylinderPointer);
            WriteInt(bytes, 0x74, unkPointer12);
            WriteInt(bytes, 0x78, splinePointer);
            WriteInt(bytes, 0x7C, grindPathsPointer);

            //0x80
            WriteInt(bytes, 0x84, type80Pointer);
            WriteInt(bytes, 0x88, unkPointer17);
            //0x8C

            WriteInt(bytes, 0x90, occlusionPointer);

            return bytes;
        }

        private byte[] SerializeDL()
        {
            byte[] bytes = new byte[GAMEPLAYSIZE];

            WriteInt(bytes, 0x00, levelVarPointer);
            WriteInt(bytes, 0x04, cameraPointer);
            WriteInt(bytes, 0x08, soundPointer);
            WriteInt(bytes, 0x0C, englishPointer);

            WriteInt(bytes, 0x10, ukenglishPointer);
            WriteInt(bytes, 0x14, frenchPointer);
            WriteInt(bytes, 0x18, germanPointer);
            WriteInt(bytes, 0x1C, spanishPointer);

            WriteInt(bytes, 0x20, italianPointer);
            WriteInt(bytes, 0x24, japanesePointer);
            WriteInt(bytes, 0x28, koreanPointer);
            WriteInt(bytes, 0x2C, mobyIdPointer);

            WriteInt(bytes, 0x30, mobyPointer);

            WriteInt(bytes, 0x40, pvarSizePointer);
            WriteInt(bytes, 0x44, pvarPointer);
            //
            WriteInt(bytes, 0x4C, cuboidPointer);

            WriteInt(bytes, 0x50, spherePointer);
            WriteInt(bytes, 0x54, cylinderPointer);
            WriteInt(bytes, 0x5C, splinePointer);

            return bytes;
        }
    }
}
