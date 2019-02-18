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
        Matrix4 mat1;
        public BoneMatrix(byte[] boneBlock, int num)
        {
            int offset = num * 0x40;
            mat1 = ReadMatrix4(boneBlock, offset);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x40];

            WriteMatrix4(ref outBytes, 0, mat1);

            return outBytes;
        }
    }
}
