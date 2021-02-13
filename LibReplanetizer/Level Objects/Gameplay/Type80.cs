using OpenTK;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type80 : MatrixObject
    {
        public const int HEADSIZE = 0x10;
        public const int DATASIZE = 0x80;

        public float off_00;
        public float off_04;
        public float off_08;
        public float off_0C;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type80(byte[] block, byte[] dataBlock, int num)
        {
            int headOffset = num * HEADSIZE;
            int dataOffset = num * DATASIZE;

            off_00 = ReadFloat(block, headOffset + 0x00);
            off_04 = ReadFloat(block, headOffset + 0x04);
            off_08 = ReadFloat(block, headOffset + 0x08);
            off_0C = ReadFloat(block, headOffset + 0x0C);

            mat1 = ReadMatrix4(dataBlock, dataOffset + 0x00); //May be matrix data, but i'm not sure.
            mat2 = ReadMatrix4(dataBlock, dataOffset + 0x40); //Don't think this is a matrix.

            rotation = mat1.ExtractRotation();
            position = mat1.ExtractTranslation();
            UpdateTransformMatrix();
        }

        public byte[] SerializeHead()
        {
            byte[] bytes = new byte[HEADSIZE];

            WriteFloat(bytes, 0x00, off_00);
            WriteFloat(bytes, 0x04, off_04);
            WriteFloat(bytes, 0x08, off_08);
            WriteFloat(bytes, 0x0C, off_0C);

            return bytes;
        }

        public byte[] SerializeData()
        {
            byte[] bytes = new byte[DATASIZE];

            WriteMatrix4(bytes, 0x00, mat1);
            WriteMatrix4(bytes, 0x40, mat2);

            return bytes;
        }

        public override byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }
    }
}
