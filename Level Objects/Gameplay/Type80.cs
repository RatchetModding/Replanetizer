using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type80
    {
        public const int HEADSIZE = 0x10;
        public const int DATASIZE = 0x80;

        public float off_00;
        public float off_04;
        public float off_08;
        public float off_0C;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type80(byte[] headBlock, byte[] dataBlock, int num)
        {
            int headOffset = num * HEADSIZE;
            int dataOffset = num * DATASIZE;

            off_00 = ReadFloat(headBlock, headOffset + 0x00);
            off_04 = ReadFloat(headBlock, headOffset + 0x04);
            off_08 = ReadFloat(headBlock, headOffset + 0x08);
            off_0C = ReadFloat(headBlock, headOffset + 0x0C);

            mat1 = ReadMatrix4(dataBlock, dataOffset + 0x00);
            mat1 = ReadMatrix4(dataBlock, dataOffset + 0x40);
        }

        public byte[] SerializeHead()
        {
            byte[] bytes = new byte[HEADSIZE];

            WriteFloat(ref bytes, 0x00, off_00);
            WriteFloat(ref bytes, 0x04, off_04);
            WriteFloat(ref bytes, 0x08, off_08);
            WriteFloat(ref bytes, 0x0C, off_0C);

            return bytes;
        }

        public byte[] SerializeData()
        {
            byte[] bytes = new byte[DATASIZE];

            WriteMatrix4(ref bytes, 0x00, mat1);
            WriteMatrix4(ref bytes, 0x40, mat2);

            return bytes;
        }
    }
}
