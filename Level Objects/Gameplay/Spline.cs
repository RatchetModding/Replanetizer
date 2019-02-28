using static RatchetEdit.DataFunctions;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using static RatchetEdit.Utilities;
namespace RatchetEdit.LevelObjects
{
    public class Spline : LevelObject, ITransformable
    {
        public int name;
        public float[] vertexBuffer;
        public float[] wVals;

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

        public override Vector3 scale
        {
            get { return _scale; }
            set
            {
                Vector3 requiredScaling = Vector3.Divide(value, _scale);
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
            LoadFromByteArray(splineBlock, offset);
        }

        public static LevelObject CreateFromByteArray(byte[] splineBlock, int offset) {
            return new Spline(splineBlock, offset);
        }

        public void LoadFromByteArray(byte[] splineBlock, int offset) {
            name = cnt;
            int count = ReadInt(splineBlock, offset);
            vertexBuffer = new float[count * 3];
            wVals = new float[count];
            for (int i = 0; i < count; i++) {
                vertexBuffer[(i * 3) + 0] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x00);
                vertexBuffer[(i * 3) + 1] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x04);
                vertexBuffer[(i * 3) + 2] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x08);
                wVals[i] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x0C);
            }

            if (count > 0) {
                _position = new Vector3(vertexBuffer[0], vertexBuffer[1], vertexBuffer[2]);
            }

            cnt++;
        }

        public override byte[] ToByteArray()
        {
            int count = vertexBuffer.Length / 3;

            byte[] bytes = new byte[count * 0x10 + 0x10];

            WriteUint(ref bytes, 0, (uint)count);

            for (int i = 0; i < count; i++)
            {
                WriteFloat(ref bytes, (i * 0x10) + 0x10, vertexBuffer[(i * 3) + 0]);
                WriteFloat(ref bytes, (i * 0x10) + 0x14, vertexBuffer[(i * 3) + 1]);
                WriteFloat(ref bytes, (i * 0x10) + 0x18, vertexBuffer[(i * 3) + 2]);
                WriteFloat(ref bytes, (i * 0x10) + 0x1C, wVals[i]);
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

        public Vector3 GetVertex(int index) {
            float x = vertexBuffer[index * 3];
            float y = vertexBuffer[index * 3 + 1];
            float z = vertexBuffer[index * 3 + 2];

            return new Vector3(x, y, z);
        }

        public void SetVertex(int index, Vector3 value) {
            vertexBuffer[(index * 3) + 0] = value.X;
            vertexBuffer[(index * 3) + 1] = value.Y;
            vertexBuffer[(index * 3) + 2] = value.Z;
        }

        public void TranslateVertex(int vertexIndex, Vector3 translationVector) {
            vertexBuffer[vertexIndex * 3] += translationVector.X;
            vertexBuffer[vertexIndex * 3 + 1] += translationVector.Y;
            vertexBuffer[vertexIndex * 3 + 2] += translationVector.Z;
        }

        public int GetVertexCount() {
            return vertexBuffer.Length / 3;
        }

        public override LevelObject Clone()
        {
            return new Spline(name, vertexBuffer);
        }

        public override void Translate(Vector3 vector)
        {
            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                SetVertex(i, GetVertex(i) + vector);
            }

            _position += vector;
        }

        public override void Rotate(Vector3 vector)
        {
            float x = vector.X;
            float y = vector.Y;
            float z = vector.Z;
            //Record base position
            Vector3 basePosition = GetVertex(0);


            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                //Record vertex position
                Vector3 vertex = GetVertex(i);

                //Get local position relative to base
                Vector3 distance = vertex - basePosition;
                
                //Rotate local position around Z axis
                Vector3 rotated1 = new Vector3(
                    distance.X * fCos(z) - distance.Y * fSin(z),
                    distance.X * fSin(z) + distance.Y * fCos(z),
                    distance.Z
                );

                //Rotate local position around Y axis
                Vector3 rotated2 = new Vector3(
                    rotated1.X * fCos(y) + rotated1.Z * fSin(y),
                    rotated1.Y,
                    -rotated1.X * fSin(y) + rotated1.Z * fCos(y)
                );

                //Rotate local position around X axis
                Vector3 rotated3 = new Vector3(
                    rotated2.X,
                    rotated2.Y * fCos(x) - rotated2.Z * fSin(x),
                    rotated2.Y * fSin(x) + rotated2.Z * fCos(x)
                );

                //Add new local position to base position
                Vector3 newPosition = basePosition + rotated3;

                //Write new position
                SetVertex(i, newPosition);
            }
            _rotation += vector;
        }

        public override void Scale(Vector3 scaleVector)
        {
            //Record base position
            Vector3 basePosition = GetVertex(0);

            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                Vector3 vertex = GetVertex(i);
                Vector3 distance = vertex - basePosition;
                Vector3 scaledDistance = distance * scaleVector;
                Vector3 newVertexPosition = basePosition + scaledDistance;

                //Write new position
                SetVertex(i, newVertexPosition);
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
