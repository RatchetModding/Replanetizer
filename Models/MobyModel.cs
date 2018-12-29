using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit
{
    public class MobyModel : Model
    {
        const int MOBYVERTELEMSIZE = 0x28;
        const int MOBYTEXELEMSIZE = 0x10;


        public MobyModel(FileStream fs, short modelID, int offset)
        {
            ID = modelID;

            if (offset != 0x00)
            {
                MobyModelHeader head = new MobyModelHeader(fs, offset);
                if (head.headSize > 0)
                {
                    ID = modelID;
                    size = head.modelSize;

                    //Texture configuration
                    textureConfig = GetTextureConfigs(fs, head.texBlockPointer, head.texCount, MOBYTEXELEMSIZE);
                    int faceCount = GetFaceCount();

                    //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ, U, V, reserved, reserved]
                    vertexBuffer = GetVertices(fs, head.vertPointer, head.vertexCount, MOBYVERTELEMSIZE);

                    //Index buffer
                    indexBuffer = GetIndices(fs, head.indexPointer, faceCount);
                }
            }
        }
    }
}
