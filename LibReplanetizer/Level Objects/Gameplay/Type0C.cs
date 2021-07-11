using OpenTK;
using System;
using System.ComponentModel;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type0C : MatrixObject, IRenderable
    {
        //Looks like 0C can be some sort of trigger that is tripped off when you go near them. They're generally placed in rivers with current and in front of unlockable doors.
        public const int ELEMENTSIZE = 0x90;

        [Category("Attributes"), DisplayName("ID")]
        public ushort ID { get; set; }
        [Category("Attributes"), DisplayName("ID2")]
        public ushort ID2 { get; set; }
        [Category("Attributes"), DisplayName("Function Pointer")]
        public int functionPointer { get; set; }
        [Category("Attributes"), DisplayName("Pvar Index")]
        public int pvarIndex { get; set; }
        [Category("Attributes"), DisplayName("Update Distance")]
        public float updateDistance { get; set; }

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
            ID = ReadUshort(block, offset + 0x00);
            ID2 = ReadUshort(block, offset + 0x02);
            functionPointer = ReadInt(block, offset + 0x04);
            pvarIndex = ReadInt(block, offset + 0x08);
            updateDistance = ReadFloat(block, offset + 0x0C);

            mat1 = ReadMatrix4(block, offset + 0x10);
            mat2 = ReadMatrix4(block, offset + 0x50);

            modelMatrix = mat1 + mat2;
            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();
            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteUshort(bytes, 0x00, ID);
            WriteUshort(bytes, 0x02, ID2);
            WriteInt(bytes, 0x04, functionPointer);
            WriteInt(bytes, 0x08, pvarIndex);
            WriteFloat(bytes, 0x0C, updateDistance);

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
