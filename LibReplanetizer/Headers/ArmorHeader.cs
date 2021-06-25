using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class ArmorHeader
    {
        public int modelPointer;
        public int texturePointer;
        public int textureCount;

        public ArmorHeader() { }


        public ArmorHeader(FileStream armorFile)
        {
            byte[] armorHeaderBytes = ReadBlock(armorFile, 0x00, 0x10);

            modelPointer = ReadInt(armorHeaderBytes, 0x00);
            texturePointer = ReadInt(armorHeaderBytes, 0x04);
            textureCount = ReadInt(armorHeaderBytes, 0x08);
            // 0x0C always 0

            // if modelPointer is zero, this is a DL texture file
            if (modelPointer == 0)
            {
                texturePointer = 0;
                textureCount = (int)(armorFile.Length / 0x24);
            }
        }

        /*
         * Assumes default folder structure, i.e.
         * global
         *      armor
         *          armor0.ps3
         *          ...
         *          armorN.ps3
         * levelK
         *      engine.ps3
         */
        public static List<string> FindArmorFiles(GameType game, string enginePath)
        {
            string superFolder = Directory.GetParent(enginePath).Parent.FullName;
            List<string> files = new List<string>();

            if (Directory.Exists(superFolder + @"\global") && Directory.Exists(superFolder + @"\global\armor"))
            {
                files.AddRange(Directory.GetFiles(superFolder + @"\global\armor", "armor*.ps3"));

                if (game.num == 4)
                {
                    files.AddRange(Directory.GetFiles(superFolder + @"\global\armor", "bot_tex*.ps3"));
                    files.AddRange(Directory.GetFiles(superFolder + @"\global\armor", "dropship*.ps3"));
                    files.AddRange(Directory.GetFiles(superFolder + @"\global\armor", "landstalker*.ps3"));
                }
            }

            return files;
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            WriteInt(bytes, 0x00, modelPointer);
            WriteInt(bytes, 0x04, texturePointer);
            WriteInt(bytes, 0x08, textureCount);

            return bytes;
        }
    }
}
