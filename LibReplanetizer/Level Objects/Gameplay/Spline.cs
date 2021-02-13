using OpenTK;
using System.ComponentModel;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Spline : LevelObject, ITransformable, IRenderable
    {
        public int name;
        public float[] vertexBuffer;
        public float[] wVals;

        [Category("Attributes"), DisplayName("offset")]
        public long offset { get; set; }

        static int cnt = 0;

        /*public override Vector3 position
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
            set { Rotate(value - _rotation); }
        }

        public override Vector3 scale
        {
            get { return _scale; }
            set
            {
                Vector3 requiredScaling = Vector3.Divide(value, _scale);
                Scale(requiredScaling);
            }
        }*/

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
            this.offset = offset;
            LoadFromByteArray(splineBlock, offset);
        }

        public static LevelObject CreateFromByteArray(byte[] splineBlock, int offset)
        {
            return new Spline(splineBlock, offset);
        }

        public void LoadFromByteArray(byte[] splineBlock, int offset)
        {
            name = cnt;
            int count = ReadInt(splineBlock, offset);
            vertexBuffer = new float[count * 3];
            wVals = new float[count];
            for (int i = 0; i < count; i++)
            {
                vertexBuffer[(i * 3) + 0] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x00);
                vertexBuffer[(i * 3) + 1] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x04);
                vertexBuffer[(i * 3) + 2] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x08);
                wVals[i] = ReadFloat(splineBlock, offset + 0x10 + (i * 0x10) + 0x0C);
            }

            if (count > 0)
            {
                //_position = new Vector3(vertexBuffer[0], vertexBuffer[1], vertexBuffer[2]);
            }

            cnt++;
        }

        public override byte[] ToByteArray()
        {
            int count = vertexBuffer.Length / 3;

            byte[] bytes = new byte[count * 0x10 + 0x10];

            WriteUint(bytes, 0, (uint)count);

            for (int i = 0; i < count; i++)
            {
                WriteFloat(bytes, (i * 0x10) + 0x10, vertexBuffer[(i * 3) + 0]);
                WriteFloat(bytes, (i * 0x10) + 0x14, vertexBuffer[(i * 3) + 1]);
                WriteFloat(bytes, (i * 0x10) + 0x18, vertexBuffer[(i * 3) + 2]);
                WriteFloat(bytes, (i * 0x10) + 0x1C, wVals[i]);
            }
            return bytes;
        }

        public Vector3 GetVertex(int index)
        {
            float x = vertexBuffer[index * 3 + 0];
            float y = vertexBuffer[index * 3 + 1];
            float z = vertexBuffer[index * 3 + 2];

            return new Vector3(x, y, z);
        }

        public void SetVertex(int index, Vector3 value)
        {
            vertexBuffer[(index * 3) + 0] = value.X;
            vertexBuffer[(index * 3) + 1] = value.Y;
            vertexBuffer[(index * 3) + 2] = value.Z;
        }

        public void TranslateVertex(int vertexIndex, Vector3 translationVector)
        {
            vertexBuffer[vertexIndex * 3 + 0] += translationVector.X;
            vertexBuffer[vertexIndex * 3 + 1] += translationVector.Y;
            vertexBuffer[vertexIndex * 3 + 2] += translationVector.Z;
        }

        public int GetVertexCount()
        {
            return vertexBuffer.Length / 3;
        }

        public override LevelObject Clone()
        {
            return new Spline(name, vertexBuffer);
        }

        /*public override void Translate(Vector3 vector)
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
        }*/

        public ushort[] GetIndices()
        {
            return new ushort[] { };
        }

        public float[] GetVertices()
        {
            return vertexBuffer;
        }
        public bool IsDynamic()
        {
            return true;
        }
    }
}
