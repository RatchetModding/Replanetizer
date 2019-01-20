using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type68 : MatrixObject
    {
        public const int ELEMENTSIZE = 0x80;

        public Matrix4 mat1;
        public Matrix4 mat2;

        public Type68(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            mat1 = ReadMatrix4(block, offset + 0x00);
            mat2 = ReadMatrix4(block, offset + 0x40);
            modelMatrix = mat1 + mat2;
            _rotation = modelMatrix.ExtractRotation().Xyz * 2.2f;
            _position = modelMatrix.ExtractTranslation();
        }

        public override LevelObject Clone() {
            throw new NotImplementedException();
        }

        public override void Render(CustomGLControl glControl, bool selected) {
            throw new NotImplementedException();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(ref bytes, 0x00, mat1);
            WriteMatrix4(ref bytes, 0x40, mat2);

            return bytes;
        }
    }
}
