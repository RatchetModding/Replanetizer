using OpenTK;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type0C : MatrixObject, IRenderable
    {
        //Looks like 0C can be some sort of trigger that is tripped off when you go near them. They're generally placed in rivers with current and in front of unlockable doors.
        public const int ELEMENTSIZE = 0x90;

        public int off_00;
        public int off_04;
        public int off_08;
        public int off_0C;

        private readonly float originalM44;
        static readonly float[] cube = new float[]
{
            -1.0f, -1.0f,  1.0f,
            1.0f, -1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            // back
            -1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f
        };
        public static readonly ushort[] cubeElements = new ushort[] { 0, 1, 2, 2, 3, 0, 1, 5, 6, 6, 2, 1, 7, 6, 5, 5, 4, 7, 4, 0, 3, 3, 7, 4, 4, 5, 1, 1, 0, 4, 3, 2, 6, 6, 7, 3 };

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

            originalM44 = mat1.M44;

            rotation = mat1.ExtractRotation();
            position = mat1.ExtractTranslation();
            scale = mat1.ExtractScale();
            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteInt(bytes, 0x00, off_00);
            WriteInt(bytes, 0x04, off_04);
            WriteInt(bytes, 0x08, off_08);
            WriteInt(bytes, 0x0C, off_0C);

            mat1.M44 = originalM44;
            WriteMatrix4(bytes, 0x10, mat1);
            WriteMatrix4(bytes, 0x50, mat2);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public ushort[] GetIndices()
        {
            return cubeElements;
        }

        public float[] GetVertices()
        {
            return cube;
        }

        public bool IsDynamic()
        {
            return false;
        }
    }
}
