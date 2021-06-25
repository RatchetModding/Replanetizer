using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class GadgetHeader
    {
        public List<int> modelPointers;
        public int texturePointer;
        public int textureCount;

        // Are they always 47? If not I would expect the count somewhere
        private const int gadgetCount = 47;

        public GadgetHeader() { }


        public GadgetHeader(FileStream gadgetFile)
        {
            byte[] gadgetHeaderBytes = ReadBlock(gadgetFile, 0x00, gadgetCount * 0x04);

            modelPointers = new List<int>();

            for (int i = 0; i < gadgetCount; i++)
            {
                modelPointers.Add(ReadInt(gadgetHeaderBytes, i * 0x04));
            }

            texturePointer = ReadInt(ReadBlock(gadgetFile, 0x3C4, 0x04), 0x00);

            textureCount = 0;

            for (int i = texturePointer; i < gadgetFile.Length; i+= 0x24)
            {
                textureCount++;
            }
        }

        /*
         * Assumes default folder structure, i.e.
         * global
         * 
         * levelK
         *      engine.ps3
         */
        public static string FindGadgetFile(GameType game, string enginePath)
        {
            string superFolder = Directory.GetParent(enginePath).Parent.FullName;

            switch (game.num)
            {
                case 1:
                    return "";
                case 2:
                    if (Directory.Exists(superFolder + @"\global") && Directory.Exists(superFolder + @"\global\gadgets") && File.Exists(superFolder + @"\global\gadgets\gadgets.ps3"))
                    {
                        return superFolder + @"\global\gadgets\gadgets.ps3";
                    } else
                    {
                        return "";
                    }
                case 3:
                    if (Directory.Exists(superFolder + @"\global") && File.Exists(superFolder + @"\global\gadgets.ps3"))
                    {
                        return superFolder + @"\global\gadgets.ps3";
                    }
                    else
                    {
                        return "";
                    }
                case 4:
                    return "";
                default:
                    return "";
            }
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            /*WriteInt(bytes, 0x00, modelPointer);
            WriteInt(bytes, 0x04, texturePointer);
            WriteInt(bytes, 0x08, textureCount);*/

            return bytes;
        }
    }
}
