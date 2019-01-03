using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type68
    {
        public const int ELEMENTSIZE = 0x80;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type68(byte[] type68Block, int num)
        {
            int offset = num * ELEMENTSIZE;

            mat1 = ReadMatrix4(type68Block, offset + 0x00);
            mat2 = ReadMatrix4(type68Block, offset + 0x40);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(ref bytes, 0x00, mat1);
            WriteMatrix4(ref bytes, 0x40, mat2);

            return bytes;
        }
    }
}
