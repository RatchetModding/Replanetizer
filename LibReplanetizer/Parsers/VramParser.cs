using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    class VramParser
    {
        FileStream fileStream;
        public bool valid = false;

        public VramParser(string filepath)
        {
            fileStream = File.OpenRead(filepath);
            valid = true;
        }

        public void GetTextures(List<Texture> textures)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                int length = 0;
                if (i < textures.Count - 1)
                {
                    length = (int)(textures[i + 1].vramPointer - textures[i].vramPointer);
                }
                else
                {
                    length = (int)(fileStream.Length - textures[i].vramPointer);
                }
                textures[i].data = ReadBlock(fileStream, textures[i].vramPointer, length);
            }
        }

        public void Close()
        {
            fileStream.Close();
        }
    }
}
