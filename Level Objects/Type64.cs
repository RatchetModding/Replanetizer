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

        public Type64(byte[] type64Block, int index)
        {
            float v11, v12, v13, v14, v21, v22, v23, v24, v31, v32, v33, v34, v41, v42, v43, v44;

            v11 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x00);
            v12 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x04);
            v13 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x08);
            v14 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x0C);

            v21 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x10);
            v22 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x14);
            v23 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x18);
            v24 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x1C);

            v31 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x20);
            v32 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x24);
            v33 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x28);
            v34 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x2C);

            v41 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x30);
            v42 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x34);
            v43 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x38);
            v44 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x3C);

            mat1 = new Matrix4(v11, v12, v13, v14, v21, v22, v23, v24, v31, v32, v33, v34, v41, v42, v43, v44);

            v11 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x40);
            v12 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x44);
            v13 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x48);
            v14 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x4C);

            v21 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x50);
            v22 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x54);
            v23 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x58);
            v24 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x5C);

            v31 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x60);
            v32 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x64);
            v33 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x68);
            v34 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x6C);

            v41 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x70);
            v42 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x74);
            v43 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x78);
            v44 = ReadFloat(type64Block, (index * ELEMENTSIZE) + 0x7C);

            mat2 = new Matrix4(v11, v12, v13, v14, v21, v22, v23, v24, v31, v32, v33, v34, v41, v42, v43, v44);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

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
