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
            off_00 = ReadFloat(headBlock, (HEADSIZE * num) + 0x00);
            off_04 = ReadFloat(headBlock, (HEADSIZE * num) + 0x04);
            off_08 = ReadFloat(headBlock, (HEADSIZE * num) + 0x08);
            off_0C = ReadFloat(headBlock, (HEADSIZE * num) + 0x0C);

            mat1 = new Matrix4();

            mat1.M11 = ReadFloat(dataBlock, (DATASIZE * num) + 0x00);
            mat1.M12 = ReadFloat(dataBlock, (DATASIZE * num) + 0x04);
            mat1.M13 = ReadFloat(dataBlock, (DATASIZE * num) + 0x08);
            mat1.M14 = ReadFloat(dataBlock, (DATASIZE * num) + 0x0C);

            mat1.M21 = ReadFloat(dataBlock, (DATASIZE * num) + 0x10);
            mat1.M22 = ReadFloat(dataBlock, (DATASIZE * num) + 0x14);
            mat1.M23 = ReadFloat(dataBlock, (DATASIZE * num) + 0x18);
            mat1.M24 = ReadFloat(dataBlock, (DATASIZE * num) + 0x1C);

            mat1.M31 = ReadFloat(dataBlock, (DATASIZE * num) + 0x20);
            mat1.M32 = ReadFloat(dataBlock, (DATASIZE * num) + 0x24);
            mat1.M33 = ReadFloat(dataBlock, (DATASIZE * num) + 0x28);
            mat1.M34 = ReadFloat(dataBlock, (DATASIZE * num) + 0x2C);

            mat1.M41 = ReadFloat(dataBlock, (DATASIZE * num) + 0x30);
            mat1.M42 = ReadFloat(dataBlock, (DATASIZE * num) + 0x34);
            mat1.M43 = ReadFloat(dataBlock, (DATASIZE * num) + 0x38);
            mat1.M44 = ReadFloat(dataBlock, (DATASIZE * num) + 0x3C);

            mat2 = new Matrix4();

            mat2.M11 = ReadFloat(dataBlock, (DATASIZE * num) + 0x40);
            mat2.M12 = ReadFloat(dataBlock, (DATASIZE * num) + 0x44);
            mat2.M13 = ReadFloat(dataBlock, (DATASIZE * num) + 0x48);
            mat2.M14 = ReadFloat(dataBlock, (DATASIZE * num) + 0x4C);

            mat2.M21 = ReadFloat(dataBlock, (DATASIZE * num) + 0x50);
            mat2.M22 = ReadFloat(dataBlock, (DATASIZE * num) + 0x54);
            mat2.M23 = ReadFloat(dataBlock, (DATASIZE * num) + 0x58);
            mat2.M24 = ReadFloat(dataBlock, (DATASIZE * num) + 0x5C);

            mat2.M31 = ReadFloat(dataBlock, (DATASIZE * num) + 0x60);
            mat2.M32 = ReadFloat(dataBlock, (DATASIZE * num) + 0x64);
            mat2.M33 = ReadFloat(dataBlock, (DATASIZE * num) + 0x68);
            mat2.M34 = ReadFloat(dataBlock, (DATASIZE * num) + 0x6C);

            mat2.M41 = ReadFloat(dataBlock, (DATASIZE * num) + 0x70);
            mat2.M42 = ReadFloat(dataBlock, (DATASIZE * num) + 0x74);
            mat2.M43 = ReadFloat(dataBlock, (DATASIZE * num) + 0x78);
            mat2.M44 = ReadFloat(dataBlock, (DATASIZE * num) + 0x7C);
        }

        public byte[] serializeHead()
        {
            byte[] bytes = new byte[HEADSIZE];

            WriteFloat(ref bytes, 0x00, off_00);
            WriteFloat(ref bytes, 0x04, off_04);
            WriteFloat(ref bytes, 0x08, off_08);
            WriteFloat(ref bytes, 0x0C, off_0C);

            return bytes;
        }

        public byte[] serializeData()
        {
            byte[] bytes = new byte[DATASIZE];

            WriteFloat(ref bytes, 0x00, mat1.M11);
            WriteFloat(ref bytes, 0x04, mat1.M12);
            WriteFloat(ref bytes, 0x08, mat1.M13);
            WriteFloat(ref bytes, 0x0C, mat1.M14);

            WriteFloat(ref bytes, 0x10, mat1.M21);
            WriteFloat(ref bytes, 0x14, mat1.M22);
            WriteFloat(ref bytes, 0x18, mat1.M23);
            WriteFloat(ref bytes, 0x1C, mat1.M24);

            WriteFloat(ref bytes, 0x20, mat1.M31);
            WriteFloat(ref bytes, 0x24, mat1.M32);
            WriteFloat(ref bytes, 0x28, mat1.M33);
            WriteFloat(ref bytes, 0x2C, mat1.M34);

            WriteFloat(ref bytes, 0x30, mat1.M41);
            WriteFloat(ref bytes, 0x34, mat1.M42);
            WriteFloat(ref bytes, 0x38, mat1.M43);
            WriteFloat(ref bytes, 0x3C, mat1.M44);

            WriteFloat(ref bytes, 0x40, mat2.M11);
            WriteFloat(ref bytes, 0x44, mat2.M12);
            WriteFloat(ref bytes, 0x48, mat2.M13);
            WriteFloat(ref bytes, 0x4C, mat2.M14);

            WriteFloat(ref bytes, 0x50, mat2.M21);
            WriteFloat(ref bytes, 0x54, mat2.M22);
            WriteFloat(ref bytes, 0x58, mat2.M23);
            WriteFloat(ref bytes, 0x5C, mat2.M24);

            WriteFloat(ref bytes, 0x60, mat2.M31);
            WriteFloat(ref bytes, 0x64, mat2.M32);
            WriteFloat(ref bytes, 0x68, mat2.M33);
            WriteFloat(ref bytes, 0x6C, mat2.M34);

            WriteFloat(ref bytes, 0x70, mat2.M41);
            WriteFloat(ref bytes, 0x74, mat2.M42);
            WriteFloat(ref bytes, 0x78, mat2.M43);
            WriteFloat(ref bytes, 0x7C, mat2.M44);

            return bytes;
        }
    }
}
