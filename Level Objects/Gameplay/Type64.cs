using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type64
    {
        public const int ELEMENTSIZE = 0x80;
        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type64(byte[] type64Block, int num)
        {
            int offset = num * ELEMENTSIZE;

            mat1 = ReadMatrix4(type64Block, offset + 0x00);
            mat2 = ReadMatrix4(type64Block, offset + 0x40);
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
