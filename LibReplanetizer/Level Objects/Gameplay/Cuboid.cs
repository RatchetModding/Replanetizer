using OpenTK;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Cuboid : MatrixObject, IRenderable
    {
        public const int ELEMENTSIZE = 0x80;

        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;

        // Try to refactor this away at some point
        private readonly float originalM44;

        static readonly float[] cube = {
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

        public static readonly ushort[] cubeElements = {
            0, 1, 2,
            2, 3, 0,
            1, 5, 6,
            6, 2, 1,
            7, 6, 5,
            5, 4, 7,
            4, 0, 3,
            3, 7, 4,
            4, 5, 1,
            1, 0, 4,
            3, 2, 6,
            6, 7, 3
        };

        public Cuboid(byte[] block, int index)
        {
            id = index;
            int offset = index * ELEMENTSIZE;

            mat1 = ReadMatrix4(block, offset + 0x00);
            mat2 = ReadMatrix4(block, offset + 0x40);

            originalM44 = mat1.M44;

            rotation = mat1.ExtractRotation();
            position = mat1.ExtractTranslation();
            scale = mat1.ExtractScale();

            UpdateTransformMatrix();
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[0x80];

            modelMatrix.M44 = originalM44;
            WriteMatrix4(bytes, 0x00, modelMatrix);
            WriteMatrix4(bytes, 0x40, mat2);

            return bytes;
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
