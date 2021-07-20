using System;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    class VramParser : RatchetFileParser, IDisposable
    {
        public bool valid = false;

        public VramParser(string filePath) : base(filePath)
        {
            valid = File.Exists(filePath);
        }

        public void GetTextures(List<Texture> textures)
        {
            if (!valid) return;

            for (int i = 0; i < textures.Count; i++)
            {
                int length;
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

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
