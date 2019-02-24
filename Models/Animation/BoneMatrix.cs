using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{ 
    public class BoneMatrix
    {
        public Matrix4 mat1;
        public short bb;

        public BoneMatrix(byte[] boneBlock, int num)
        {
            int offset = num * 0x40;
            mat1 = ReadMatrix4(boneBlock, offset);
            
            //mat1.M14 = 0;
            //mat1.M24 = 0;
            //mat1.M34 = 0;
            mat1.M44 = 1;



            bb = ReadShort(boneBlock, offset + 0x3E);
            mat1.Transpose();
            //mat1.Invert();
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x40];

            WriteMatrix4(ref outBytes, 0, mat1);

            return outBytes;
        }
    }
}
