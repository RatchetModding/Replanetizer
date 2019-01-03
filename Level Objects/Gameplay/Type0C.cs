using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type0C
    {
        public const int ELEMENTSIZE = 0x90;
        public int off_00;
        public int off_04;
        public int off_08;
        public int off_0C;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type0C(byte[] type0CBlock, int num)
        {
            int offset = num * ELEMENTSIZE;

            off_00 = ReadInt(type0CBlock, offset + 0x00);
            off_04 = ReadInt(type0CBlock, offset + 0x04);
            off_08 = ReadInt(type0CBlock, offset + 0x08);
            off_0C = ReadInt(type0CBlock, offset + 0x0C);

            mat1 = ReadMatrix4(type0CBlock, offset + 0x10);
            mat2 = ReadMatrix4(type0CBlock, offset + 0x50);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteInt(ref bytes, 0x00, off_00);
            WriteInt(ref bytes, 0x04, off_04);
            WriteInt(ref bytes, 0x08, off_08);
            WriteInt(ref bytes, 0x0C, off_0C);

            WriteMatrix4(ref bytes, 0x10, mat1);
            WriteMatrix4(ref bytes, 0x50, mat2);

            return bytes;
        }
    }
}
