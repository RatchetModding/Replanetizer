using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit
{
    public class MobyModel : Model
    {
        const int VERTELEMENTSIZE = 0x28;
        const int TEXTUREELEMENTSIZE = 0x10;

        [Category("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Model Header")]
        public MobyModelHeader head { get; set; }

        public MobyModel(FileStream fs, short modelID, int offset)
        {
            id = modelID;
            if (offset != 0x00)
            {
                head = new MobyModelHeader(fs, offset);
                id = modelID;
                size = head.scale;

                int faceCount = 0;

                //Texture configuration
                if (head.texBlockPointer > 0)
                {
                    textureConfig = GetTextureConfigs(fs, head.texBlockPointer, head.texCount, TEXTUREELEMENTSIZE);
                    faceCount = GetFaceCount();
                }

                if (head.vertPointer > 0 && head.vertexCount > 0)
                {
                    //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ, U, V, reserved, reserved]
                    vertexBuffer = GetVertices(fs, head.vertPointer, head.vertexCount, VERTELEMENTSIZE);
                }

                if (head.indexPointer > 0 && faceCount > 0)
                {
                    //Index buffer
                    indexBuffer = GetIndices(fs, head.indexPointer, faceCount);
                }

            }
        }
    }
}
