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
        public static List<string> FindArmorFiles(string enginePath)
        {
            string superFolder = Directory.GetParent(enginePath).Parent.FullName;
            List<string> files = new List<string>();

            if (Directory.Exists(superFolder + @"\global") && Directory.Exists(superFolder + @"\global\armor"))
            {
                files.AddRange(Directory.GetFiles(superFolder + @"\global\armor", "armor*.ps3"));
            }

            return files;
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            return bytes;
        }
    }
}
