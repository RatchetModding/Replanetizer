using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using static RatchetEdit.Utilities;
namespace RatchetEdit
{
    public class Spline : LevelObject, ITransformable
    {
        public int name;
        public float[] vertexBuffer;

        static int cnt = 0;

        int VBO;

        public override Vector3 position
        {
            get { return _position; }
            set
            {
                Translate(value - _position);
            }
        }
        public override Vector3 rotation
        {
            get { return _rotation; }
            set
            {
                Rotate(value - _rotation);
            }
        }

        public override float scale
        {
            get { return _scale; }
            set
            {
                float requiredScaling = value / _scale;
                Scale(requiredScaling);
            }
        }

        public Spline(int name, float[] vertexBuffer)
        {
            this.name = name;
            this.vertexBuffer = new float[vertexBuffer.Length];
            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                this.vertexBuffer[i] = vertexBuffer[i];
            }
        }

        public Spline(byte[] splineBlock, int offset)
        {
            name = cnt;
            int count = ReadInt(splineBlock, offset);
            vertexBuffer = new float[count * 3];
            for (int i = 0; i < count; i++)
            {
                vertexBuffer[(i * 3) + 0] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x00);
                vertexBuffer[(i * 3) + 1] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x04);
                vertexBuffer[(i * 3) + 2] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x08);
            }

            if (count > 0)
            {
                _position = new Vector3(vertexBuffer[0], vertexBuffer[1], vertexBuffer[2]);
            }

            cnt++;
        }

        public Vector3 GetVertex(int index)
        {
            float x = vertexBuffer[index * 3];
            float y = vertexBuffer[index * 3 + 1];
            float z = vertexBuffer[index * 3 + 2];

            return new Vector3(x, y, z);
        }

        public void TranslateVertex(int vertexIndex, Vector3 translationVector)
        {
            vertexBuffer[vertexIndex * 3] += translationVector.X;
            vertexBuffer[vertexIndex * 3 + 1] += translationVector.Y;
            vertexBuffer[vertexIndex * 3 + 2] += translationVector.Z;
        }

        public int GetVertexCount()
        {
            return vertexBuffer.Length / 3;
        }

        public byte[] Serialize()
        {
            int count = vertexBuffer.Length / 3;

            byte[] bytes = new byte[count * 0x10 + 0x10];

            WriteUint(ref bytes, 0, (uint)count);

            for (int i = 0; i < count; i++)
            {
                WriteFloat(ref bytes, (i * 0x10) + 0x10, vertexBuffer[(i * 3) + 0]);
                WriteFloat(ref bytes, (i * 0x10) + 0x14, vertexBuffer[(i * 3) + 1]);
                WriteFloat(ref bytes, (i * 0x10) + 0x18, vertexBuffer[(i * 3) + 2]);
                WriteFloat(ref bytes, (i * 0x10) + 0x1C, -1);
            }
            return bytes;
        }

        public void GetVBO()
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

        public override LevelObject Clone()
        {
            return new Spline(name, vertexBuffer);
        }

        public override void UpdateTransformMatrix() { }

        //Transformable methods
        public override void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z));
        }
        public override void Translate(Vector3 vector)
        {
            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                vertexBuffer[(i * 3) + 0] += vector.X;
                vertexBuffer[(i * 3) + 1] += vector.Y;
                vertexBuffer[(i * 3) + 2] += vector.Z;
            }

            _position += vector;
        }

        public override void Rotate(float x, float y, float z)
        {
            //Record base position
            float base_x = vertexBuffer[0];
            float base_y = vertexBuffer[1];
            float base_z = vertexBuffer[2];

            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                //Record vertex position
                float vertex_x = vertexBuffer[(i * 3) + 0];
                float vertex_y = vertexBuffer[(i * 3) + 1];
                float vertex_z = vertexBuffer[(i * 3) + 2];

                //Get local position relative to base
                float distance_x = vertex_x - base_x;
                float distance_y = vertex_y - base_y;
                float distance_z = vertex_z - base_z;

                //Rotate local position around Z axis
                float rotated_x1 = distance_x * fCos(z) - distance_y * fSin(z);
                float rotated_y1 = distance_x * fSin(z) + distance_y * fCos(z);
                float rotated_z1 = distance_z;

                //Rotate local position around Y axis
                float rotated_x2 = rotated_x1 * fCos(y) + rotated_z1 * fSin(y);
                float rotated_y2 = rotated_y1;
                float rotated_z2 = -rotated_x1 * fSin(y) + rotated_z1 * fCos(y);

                //Rotate local position around X axis
                float rotated_x3 = rotated_x2;
                float rotated_y3 = rotated_y2 * fCos(x) - rotated_z2 * fSin(x);
                float rotated_z3 = rotated_y2 * fSin(x) + rotated_z2 * fCos(x);

                //Add new local position to base position
                float new_x = base_x + rotated_x3;
                float new_y = base_y + rotated_y3;
                float new_z = base_z + rotated_z3;

                //Write new position
                vertexBuffer[(i * 3) + 0] = new_x;
                vertexBuffer[(i * 3) + 1] = new_y;
                vertexBuffer[(i * 3) + 2] = new_z;
            }
            _rotation += new Vector3(x, y, z);
        }

        public override void Rotate(Vector3 vector)
        {
            Rotate(vector.X, vector.Y, vector.Z);
        }

        public override void Scale(float scale)
        {
            //Record base position
            float base_x = vertexBuffer[0];
            float base_y = vertexBuffer[1];
            float base_z = vertexBuffer[2];

            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                //Record vertex position
                float vertex_x = vertexBuffer[(i * 3) + 0];
                float vertex_y = vertexBuffer[(i * 3) + 1];
                float vertex_z = vertexBuffer[(i * 3) + 2];

                //Get local position relative to base
                float distance_x = vertex_x - base_x;
                float distance_y = vertex_y - base_y;
                float distance_z = vertex_z - base_z;

                //Scale local position
                float scaled_x = distance_x * scale;
                float scaled_y = distance_y * scale;
                float scaled_z = distance_z * scale;

                //Add new local position and base position
                float new_x = base_x + scaled_x;
                float new_y = base_y + scaled_y;
                float new_z = base_z + scaled_z;

                //Write new position
                vertexBuffer[(i * 3) + 0] = new_x;
                vertexBuffer[(i * 3) + 1] = new_y;
                vertexBuffer[(i * 3) + 2] = new_z;
            }
            _scale *= scale;
        }

        public override void Render(CustomGLControl glControl, bool selected = false)
        {
            var worldView = glControl.worldView;

            GL.UseProgram(glControl.colorShaderID);
            GL.UniformMatrix4(glControl.matrixID, false, ref worldView);
            GL.Uniform4(glControl.colorID, selected ? selectedColor : normalColor);
            GetVBO();
            GL.DrawArrays(PrimitiveType.LineStrip, 0, vertexBuffer.Length / 3);
        }
    }
}
