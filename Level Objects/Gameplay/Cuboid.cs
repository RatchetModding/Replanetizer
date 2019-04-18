using System;
using static RatchetEdit.DataFunctions;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit.LevelObjects
{
    public class Cuboid : MatrixObject
    {
        public const int ELEMENTSIZE = 0x80;

        public int id;
        public Matrix4 mat1;
        public Matrix4 mat2;

		int IBO;
		int VBO;
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

        public override LevelObject Clone() {
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

		public override void Render(CustomGLControl glControl, bool selected = false)
		{
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
