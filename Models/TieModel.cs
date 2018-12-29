using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit
{
    public class TieModel : Model
    {
        const int TIETEXELEMSIZE = 0x18;
        const int TIEVERTELEMSIZE = 0x18;
        const int TIEUVELEMSIZE = 0x08;

        [CategoryAttribute("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Header")]
        public TieModelHeader head { get; set; }

        public TieModel(FileStream fs, TieModelHeader head)
        {
            this.head = head;
            size = 1.0f;
            ID = head.modelID;

            textureConfig = GetTextureConfigs(fs, head.texturePointer, head.textureCount, TIETEXELEMSIZE);
            int faceCount = GetFaceCount();

            //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ] and UV array float[U, V] * vertexCount
            vertexBuffer = GetVertices(fs, head.vertexPointer, head.UVPointer, head.vertexCount, TIEVERTELEMSIZE, TIEUVELEMSIZE);

            //Get index buffer ushort[i] * faceCount
            indexBuffer = GetIndices(fs, head.indexPointer, faceCount);
        }
    }
}
