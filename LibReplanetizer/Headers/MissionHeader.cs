using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Headers
{
    public class MissionHeader
    {
        public int mobiesCount;
        public int textureCount;
        public int texturePointer;

        public MissionHeader() { }


        public MissionHeader(FileStream missionFile)
        {
            byte[] missionHeaderBytes = ReadBlock(missionFile, 0x00, 0x10);

            mobiesCount = ReadInt(missionHeaderBytes, 0x00);
            textureCount = ReadInt(missionHeaderBytes, 0x04);
            texturePointer = ReadInt(missionHeaderBytes, 0x08);
            // 0x0C always 0
        }

        /*
         * Assumes default folder structure, that is, mission files lie in the same folder as the engine file
         */
        public static List<string> FindMissionFiles(GameType game, string enginePath)
        {
            string dir = Directory.GetParent(enginePath).FullName;
            List<string> files = new List<string>();

            switch (game.num)
            {
                case 1:
                case 2:
                case 3:
                    break;
                case 4:
                    for (int i = 0; i < 50; i++)
                    {
                        if (File.Exists(dir + @"\gameplay_mission_classes[" + i + "].ps3"))
                        {
                            files.Add(dir + @"\gameplay_mission_classes[" + i + "].ps3");
                        }
                    }
                    break;
                default:
                    break;
            }

            return files;
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x10];

            WriteInt(bytes, 0x00, mobiesCount);
            WriteInt(bytes, 0x04, textureCount);
            WriteInt(bytes, 0x08, texturePointer);

            return bytes;
        }
    }
}
