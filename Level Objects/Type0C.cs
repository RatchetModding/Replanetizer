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
        public const int TYPE0CELEMSIZE = 0x90;
        public int off_00;
        public int off_04;
        public int off_08;
        public int off_0C;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type0C(byte[] type0CBlock, int num)
        {
            off_00 = ReadInt(type0CBlock, (TYPE0CELEMSIZE * num) + 0x00);
            off_04 = ReadInt(type0CBlock, (TYPE0CELEMSIZE * num) + 0x04);
            off_08 = ReadInt(type0CBlock, (TYPE0CELEMSIZE * num) + 0x08);
            off_0C = ReadInt(type0CBlock, (TYPE0CELEMSIZE * num) + 0x0C);

            float m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44;

            m11 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x10);
            m12 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x14);
            m13 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x18);
            m14 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x1C);

            m21 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x20);
            m22 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x24);
            m23 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x28);
            m24 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x2C);

            m31 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x30);
            m32 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x34);
            m33 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x38);
            m34 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x3C);

            m41 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x40);
            m42 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x44);
            m43 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x48);
            m44 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x4C);

            mat1 = new Matrix4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);

            m11 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x50);
            m12 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x54);
            m13 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x58);
            m14 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x5C);

            m21 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x60);
            m22 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x64);
            m23 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x68);
            m24 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x6C);

            m31 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x70);
            m32 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x74);
            m33 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x78);
            m34 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x7C);

            m41 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x80);
            m42 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x84);
            m43 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x88);
            m44 = ReadFloat(type0CBlock, (TYPE0CELEMSIZE * num) + 0x8C);

            mat2 = new Matrix4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[TYPE0CELEMSIZE];

            WriteInt(ref bytes, 0x00, off_00);
            WriteInt(ref bytes, 0x04, off_04);
            WriteInt(ref bytes, 0x08, off_08);
            WriteInt(ref bytes, 0x0C, off_0C);

            WriteFloat(ref bytes, 0x10, mat1.M11);
            WriteFloat(ref bytes, 0x14, mat1.M12);
            WriteFloat(ref bytes, 0x18, mat1.M13);
            WriteFloat(ref bytes, 0x1C, mat1.M14);

            WriteFloat(ref bytes, 0x20, mat1.M21);
            WriteFloat(ref bytes, 0x24, mat1.M22);
            WriteFloat(ref bytes, 0x28, mat1.M23);
            WriteFloat(ref bytes, 0x2C, mat1.M24);

            WriteFloat(ref bytes, 0x30, mat1.M31);
            WriteFloat(ref bytes, 0x34, mat1.M32);
            WriteFloat(ref bytes, 0x38, mat1.M33);
            WriteFloat(ref bytes, 0x3C, mat1.M34);

            WriteFloat(ref bytes, 0x40, mat1.M41);
            WriteFloat(ref bytes, 0x44, mat1.M42);
            WriteFloat(ref bytes, 0x48, mat1.M43);
            WriteFloat(ref bytes, 0x4C, mat1.M44);

            WriteFloat(ref bytes, 0x50, mat2.M11);
            WriteFloat(ref bytes, 0x54, mat2.M12);
            WriteFloat(ref bytes, 0x58, mat2.M13);
            WriteFloat(ref bytes, 0x5C, mat2.M14);

            WriteFloat(ref bytes, 0x60, mat2.M21);
            WriteFloat(ref bytes, 0x64, mat2.M22);
            WriteFloat(ref bytes, 0x68, mat2.M23);
            WriteFloat(ref bytes, 0x6C, mat2.M24);

            WriteFloat(ref bytes, 0x70, mat2.M31);
            WriteFloat(ref bytes, 0x74, mat2.M32);
            WriteFloat(ref bytes, 0x78, mat2.M33);
            WriteFloat(ref bytes, 0x7C, mat2.M34);

            WriteFloat(ref bytes, 0x80, mat2.M41);
            WriteFloat(ref bytes, 0x84, mat2.M42);
            WriteFloat(ref bytes, 0x88, mat2.M43);
            WriteFloat(ref bytes, 0x8C, mat2.M44);

            return bytes;
        }
    }
}
