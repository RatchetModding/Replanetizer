using OpenTK;
using OpenTK.Graphics.OpenGL;
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

        int IBO;
        int VBO;
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
        static readonly ushort[] cubeElements = new ushort[] { 0, 1, 2, 2, 3, 0, 1, 5, 6, 6, 2, 1, 7, 6, 5, 5, 4, 7, 4, 0, 3, 3, 7, 4, 4, 5, 1, 1, 0, 4, 3, 2, 6, 6, 7, 3 };

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
            mat1.M44 = 1;
            originalM44 = ReadFloat(block, offset + 0x4C);
            mat2 = ReadMatrix4(block, offset + 0x50);

            modelMatrix = mat1;
            _rotation = modelMatrix.ExtractRotation().Xyz * 2.2f;
            _position = modelMatrix.ExtractTranslation();
            _scale = modelMatrix.ExtractScale();

            GetVBO();
            GetIBO();
        }

        public void GetVBO() {
            //Get the vertex buffer object, or create one if one doesn't exist
            if (VBO == 0) {
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, cube.Length * sizeof(float), cube, BufferUsageHint.StaticDraw);

            }
            else {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            }
        }

        public void GetIBO() {
            if (IBO == 0) {
                GL.GenBuffers(1, out IBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, cubeElements.Length * sizeof(ushort), cubeElements, BufferUsageHint.StaticDraw);
            }
            else {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            }
        }

        public override byte[] ToByteArray()
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

        public override LevelObject Clone() {
            throw new NotImplementedException();
        }

        public override void Render(CustomGLControl glControl, bool selected = false) {
            GL.UseProgram(glControl.colorShaderID);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            Matrix4 mvp = modelMatrix * glControl.worldView;
            GL.UniformMatrix4(glControl.matrixID, false, ref mvp);
            GL.Uniform4(glControl.colorID, selected ? selectedColor : normalColor);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.DrawElements(PrimitiveType.Triangles, cubeElements.Length, DrawElementsType.UnsignedShort, 0);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

    }
}
