using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type0C : MatrixObject
    {
        //Looks like 0C can be some sort of trigger that is tripped off when you go near them. They're generally placed in rivers with current and in front of unlockable doors.
        public const int ELEMENTSIZE = 0x90;
        public int off_00;
        public int off_04;
        public int off_08;
        public int off_0C;

        public Matrix4 mat1; //Definitely a matrix. Contains logically positioned and rotated points.
        public Matrix4 mat2; //Not entirely sure if is a matrix. If it is, it has to be relative to mat1.

        public Type0C(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;
            off_00 = ReadInt(block, offset + 0x00);
            off_04 = ReadInt(block, offset + 0x04);
            off_08 = ReadInt(block, offset + 0x08);
            off_0C = ReadInt(block, offset + 0x0C);

            mat1 = ReadMatrix4(block, offset + 0x10);
            mat2 = ReadMatrix4(block, offset + 0x50);

            modelMatrix = mat1 + mat2;
            _rotation = modelMatrix.ExtractRotation().Xyz * 2.2f;
            _position = modelMatrix.ExtractTranslation();
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
