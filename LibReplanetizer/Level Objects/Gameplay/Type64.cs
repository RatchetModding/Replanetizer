using OpenTK;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type64 : MatrixObject
    {
        public const int ELEMENTSIZE = 0x80;

        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type64(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            mat1 = ReadMatrix4(block, offset + 0x00);
            mat2 = ReadMatrix4(block, offset + 0x40);

            modelMatrix = mat1 + mat2;
            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, mat1);
            WriteMatrix4(bytes, 0x40, mat2);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }
    }
}
