using System.Collections.Generic;
using System.IO;
using RatchetEdit.Headers;

namespace RatchetEdit.Models
{
    public class TerrainModel : Model
    {
        const int VERTELEMSIZE = 0x1C;
        const int TEXELEMSIZE = 0x10;
        const int UVELEMSIZE = 0x08;


        public TerrainModel(FileStream fs, TerrainHeader head)
        {
            size = 1.0f;

            textureConfig = new List<TextureConfig>();
            foreach (TerrainFragHeader fh in head.heads)
            {
                textureConfig.AddRange(GetTextureConfigs(fs, fh.texturePointer, fh.textureCount, 0x10));
            }
            int faceCount = GetFaceCount();

            vertexBuffer = GetVertices(fs, head.vertexPointer, head.UVPointer, (int)head.vertexCount, 0x1C, 0x08);
            indexBuffer = GetIndices(fs, head.indexPointer, faceCount);
        }
    }
}