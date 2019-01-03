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
        public const int TYPE68ELEMSIZE = 0x80;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type68(byte[] type68Block, int num)
        {
            float m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44;

            m11 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x00);
            m12 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x04);
            m13 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x08);
            m14 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x0C);

            m21 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x10);
            m22 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x14);
            m23 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x18);
            m24 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x1C);

            m31 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x20);
            m32 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x24);
            m33 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x28);
            m34 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x2C);

            m41 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x30);
            m42 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x34);
            m43 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x38);
            m44 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x3C);

            mat1 = new Matrix4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);

            m11 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x40);
            m12 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x44);
            m13 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x48);
            m14 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x4C);

            m21 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x50);
            m22 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x54);
            m23 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x58);
            m24 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x5C);

            m31 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x60);
            m32 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x64);
            m33 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x68);
            m34 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x6C);

            m41 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x70);
            m42 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x74);
            m43 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x78);
            m44 = ReadFloat(type68Block, (TYPE68ELEMSIZE * num) + 0x7C);

            mat2 = new Matrix4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[TYPE68ELEMSIZE];

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
