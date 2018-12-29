using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit
{
    class ShrubModel : Model
    {
        const int SHRUBTEXELEMSIZE = 0x10;
        const int SHRUBVERTELEMSIZE = 0x18;
        const int SHRUBUVELEMSIZE = 0x08;

        [CategoryAttribute("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Header")]
        public TieModelHeader head { get; set; }

        public ShrubModel(FileStream fs, TieModelHeader head)
        {
            this.head = head;
            size = 1.0f;
            ID = head.modelID;

            textureConfig = GetTextureConfigs(fs, head.texturePointer, head.textureCount, SHRUBTEXELEMSIZE);
            int faceCount = GetFaceCount();

            //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ] and UV array float[U, V] * vertexCount
            vertexBuffer = GetVertices(fs, head.vertexPointer, head.UVPointer, head.vertexCount, SHRUBVERTELEMSIZE, SHRUBUVELEMSIZE);

            //Get index buffer ushort[i] * faceCount
            indexBuffer = GetIndices(fs, head.indexPointer, faceCount);
        }
    }
}
