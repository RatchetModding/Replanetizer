using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;
using OpenTK;

namespace RatchetEdit
{
    public class SpawnPoint : MatrixObject
    {
        public const int ELEMENTSIZE = 0x80;
        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;

        public SpawnPoint(byte[] block, int index)
        {
            id = index;
            int offset = index * ELEMENTSIZE;

            mat1 = new Matrix4(
                ReadFloat(block, offset + 0x00),
                ReadFloat(block, offset + 0x04),
                ReadFloat(block, offset + 0x08),
                ReadFloat(block, offset + 0x0C),
                                                         
                ReadFloat(block, offset + 0x10),
                ReadFloat(block, offset + 0x14),
                ReadFloat(block, offset + 0x18),
                ReadFloat(block, offset + 0x1C),
                                                         
                ReadFloat(block, offset + 0x20),
                ReadFloat(block, offset + 0x24),
                ReadFloat(block, offset + 0x28),
                ReadFloat(block, offset + 0x2C),
                                                         
                ReadFloat(block, offset + 0x30),
                ReadFloat(block, offset + 0x34),
                ReadFloat(block, offset + 0x38),
                ReadFloat(block, offset + 0x3C)
                );

            mat2 = new Matrix4(
                ReadFloat(block, offset + 0x40),
                ReadFloat(block, offset + 0x44),
                ReadFloat(block, offset + 0x48),
                ReadFloat(block, offset + 0x4C),
                                                         
                ReadFloat(block, offset + 0x50),
                ReadFloat(block, offset + 0x54),
                ReadFloat(block, offset + 0x58),
                ReadFloat(block, offset + 0x5C),
                                                         
                ReadFloat(block, offset + 0x60),
                ReadFloat(block, offset + 0x64),
                ReadFloat(block, offset + 0x68),
                ReadFloat(block, offset + 0x6C),
                                                         
                ReadFloat(block, offset + 0x70),
                ReadFloat(block, offset + 0x74),
                ReadFloat(block, offset + 0x78),
                ReadFloat(block, offset + 0x7C)
            );

            modelMatrix = mat1 + mat2;
            _rotation = modelMatrix.ExtractRotation().Xyz * 2.2f;
            _position = modelMatrix.ExtractTranslation();
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x80];

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
