using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace RatchetEdit
{
    public class Spline : LevelObject, ITransformable {
        public int name;
        public float[] vertexBuffer;

        int VBO;

        public Spline(int name, float[] vertexBuffer) {
            this.name = name;
            this.vertexBuffer = new float[vertexBuffer.Length];
            for (int i = 0; i < vertexBuffer.Length; i++) {
                this.vertexBuffer[i] = vertexBuffer[i];
            }
        }

        public Spline(byte[] splineBlock, int offset)
        {
            name = offset;
            int count = ReadInt(splineBlock, offset);
            vertexBuffer = new float[count * 3];
            for(int i = 0; i < count; i++)
            {
                vertexBuffer[(i * 3) + 0] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x00);
                vertexBuffer[(i * 3) + 1] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x04);
                vertexBuffer[(i * 3) + 2] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x08);
            }

            if(count > 0) {
                position = new Vector3(vertexBuffer[0], vertexBuffer[1], vertexBuffer[2]);
            }
        }

        public void getVBO()
        {
            //Get the vertex buffer object, or create one if one doesn't exist
            if (VBO == 0)
            {
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.DynamicDraw);
                //Console.WriteLine("Generated VBO with ID: " + VBO.ToString());
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.DynamicDraw);
            }
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }

        public override LevelObject Clone() {
            return new Spline(name, vertexBuffer);
        }

        public override void UpdateTransform() {}

        //Transformable methods
        public override void Translate(float x, float y, float z) {
            Translate(new Vector3(x,y,z));
        }
        public override void Translate(Vector3 vector) {
            for (int i = 0; i < vertexBuffer.Length / 3; i++) {
                vertexBuffer[(i * 3) + 0] += vector.X;
                vertexBuffer[(i * 3) + 1] += vector.Y;
                vertexBuffer[(i * 3) + 2] += vector.Z;
            }

            position += vector;
        }

        public override void Rotate(float x, float y, float z) {
            float base_x = vertexBuffer[0];
            float base_y = vertexBuffer[1];
            float base_z = vertexBuffer[2];

            for (int i = 0; i < vertexBuffer.Length / 3; i++) {
                float vertex_x = vertexBuffer[(i * 3) + 0];
                float vertex_y = vertexBuffer[(i * 3) + 1];
                float vertex_z = vertexBuffer[(i * 3) + 2];

                float distance_x = vertex_x - base_x;
                float distance_y = vertex_y - base_y;
                float distance_z = vertex_z - base_z;


                vertexBuffer[(i * 3) + 0] = base_x + distance_x;
                vertexBuffer[(i * 3) + 1] = base_x + distance_y;
                vertexBuffer[(i * 3) + 2] = base_z + distance_z;
            }
        }

        public override void Rotate(Vector3 vector) {
            Rotate(vector.X, vector.Y, vector.Z);
        }

        public override void Scale(float scale) {
            throw new NotImplementedException();
        }

    }
}
