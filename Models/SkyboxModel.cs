﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class SkyboxModel : Model
    {
        public const int VERTELEMSIZE = 0x18;

        public SkyboxModel(FileStream fs, int offset)
        {
            size = 1.0f;
            byte[] skyBlockHead = ReadBlock(fs, offset, 0x1C);

            short faceGroupCount = ReadShort(skyBlockHead, 0x06);
            int vertOffset = ReadInt(skyBlockHead, 0x14);
            int faceOffset = ReadInt(skyBlockHead, VERTELEMSIZE);

            int vertexCount = (int)((faceOffset - vertOffset) / VERTELEMSIZE);



            textureConfig = new List<TextureConfig>();
            byte[] faceGroupBlock = ReadBlock(fs, offset + 0x1C, faceGroupCount * 4);
            for (int i = 0; i < faceGroupCount; i++)
            {
                int faceGroupOffset = ReadInt(faceGroupBlock, (i * 4));
                short texCount = ReadShort(ReadBlock(fs, faceGroupOffset + 0x02, 0x02), 0);
                textureConfig.AddRange(GetTextureConfigs(fs, faceGroupOffset + 0x10, texCount, 0x10));
            }

            int faceCount = GetFaceCount();
            Console.WriteLine("vertexCount: " + vertexCount.ToString());
            vertexBuffer = GetVerticesUV(fs, vertOffset, vertexCount, VERTELEMSIZE);

            indexBuffer = GetIndices(fs, faceOffset, faceCount);

        }
    }
}
