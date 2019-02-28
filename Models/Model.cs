using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Models
{
    /*
        General purpose 3D model used for rendering
    */

    public abstract class Model
    {
        public short id { get; set; }
        public float size;
        public float[] vertexBuffer = {  };
        public ushort[] indexBuffer = {  };
        public uint[] weights;
        public uint[] ids;


        public List<TextureConfig> textureConfig { get; set; } = new List<TextureConfig>();
        public int VBO = 0;
        public int IBO = 0;

        protected int GetFaceCount()
        {
            int faceCount = 0;
            if (textureConfig != null)
            {
                foreach (TextureConfig tex in textureConfig)
                {
                    faceCount += tex.size;
                }
            }
            return faceCount;
        }

        public void Draw(List<Texture> textures)
        {
            GetVBO();
            GetIBO();

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in textureConfig)
            {
                GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? textures[conf.ID].getTexture() : 0);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }
        }

        public void Draw(CustomGLControl glControl)
        {
            GL.UseProgram(glControl.shaderID);

            Matrix4 worldView = glControl.worldView;
            GL.UniformMatrix4(glControl.matrixID, false, ref worldView);
            Draw(glControl.level.textures);
        }

        public void GetVBO()
        {
            //Get the vertex buffer object, or create one if one doesn't exist
            if (VBO == 0)
            {
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticDraw);
                //Console.WriteLine("Generated VBO with ID: " + VBO.ToString());
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            }
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
        }

        public void GetIBO()
        {
            //Get the index buffer object, or create one if one doesn't exist
            if (IBO == 0)
            {
                GL.GenBuffers(1, out IBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indexBuffer.Length * sizeof(ushort), indexBuffer, BufferUsageHint.StaticDraw);
                //Console.WriteLine("Generated IBO with ID: " + IBO.ToString());
            }
            else
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            }
        }

        //Get texture configs of different types using elemsize
        public static List<TextureConfig> GetTextureConfigs(FileStream fs, int texturePointer, int textureCount, int elemSize)
        {
            int IDoffset = 0, startOffset = 0, sizeOffset = 0, modeOffset = 0;

            switch (elemSize)
            {
                case 0x10:
                    IDoffset = 0x00;
                    startOffset = 0x04;
                    sizeOffset = 0x08;
                    modeOffset = 0x0C;
                    break;
                case 0x18:
                    IDoffset = 0x00;
                    startOffset = 0x08;
                    sizeOffset = 0x0C;
                    modeOffset = 0x14;
                    break;
            }

            var textureConfigs = new List<TextureConfig>();
            byte[] texBlock = ReadBlock(fs, texturePointer, textureCount * elemSize);
            for (int i = 0; i < textureCount; i++)
            {
                TextureConfig textureConfig = new TextureConfig();
                textureConfig.ID = ReadInt(texBlock, (i * elemSize) + IDoffset);
                textureConfig.start = ReadInt(texBlock, (i * elemSize) + startOffset);
                textureConfig.size = ReadInt(texBlock, (i * elemSize) + sizeOffset);
                textureConfig.mode = ReadInt(texBlock, (i * elemSize) + modeOffset);
                textureConfigs.Add(textureConfig);
            }
            return textureConfigs;
        }

        //Get vertices with UV's baked in
        public float[] GetVertices(FileStream fs, int vertexPointer, int vertexCount, int elemSize)
        {
            float[] vertexBuffer = new float[vertexCount * 8];
            weights = new uint[vertexCount];
            ids = new uint[vertexCount];
            //List<float> vertexBuffer = new List<float>();
            byte[] vertBlock = ReadBlock(fs, vertexPointer, vertexCount * elemSize);
            for (int i = 0; i < vertexCount; i++)
            {
                vertexBuffer[(i * 8) + 0] = (ReadFloat(vertBlock, (i * elemSize) + 0x00));    //VertexX
                vertexBuffer[(i * 8) + 1] = (ReadFloat(vertBlock, (i * elemSize) + 0x04));    //VertexY
                vertexBuffer[(i * 8) + 2] = (ReadFloat(vertBlock, (i * elemSize) + 0x08));    //VertexZ
                vertexBuffer[(i * 8) + 3] = (ReadFloat(vertBlock, (i * elemSize) + 0x0C));    //NormX
                vertexBuffer[(i * 8) + 4] = (ReadFloat(vertBlock, (i * elemSize) + 0x10));    //NormY
                vertexBuffer[(i * 8) + 5] = (ReadFloat(vertBlock, (i * elemSize) + 0x14));    //NormZ
                vertexBuffer[(i * 8) + 6] = (ReadFloat(vertBlock, (i * elemSize) + 0x18));    //UVu
                vertexBuffer[(i * 8) + 7] = (ReadFloat(vertBlock, (i * elemSize) + 0x1C));    //UVv
                if(elemSize == 0x28)
                {
                    weights[i] = (ReadUint(vertBlock, (i * elemSize) + 0x20));
                    ids[i] = (ReadUint(vertBlock, (i * elemSize) + 0x24));
                }


            }
            return vertexBuffer;
        }

        public byte[] SerializeVertices()
        {
            int elemSize = 0x28;
            byte[] outBytes = new byte[(vertexBuffer.Length / 8) * elemSize];

            for (int i = 0; i < vertexBuffer.Length / 8; i++)
            {
                WriteFloat(ref outBytes, (i * elemSize) + 0x00, vertexBuffer[(i * 8) + 0]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x04, vertexBuffer[(i * 8) + 1]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x08, vertexBuffer[(i * 8) + 2]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x0C, vertexBuffer[(i * 8) + 3]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x10, vertexBuffer[(i * 8) + 4]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x14, vertexBuffer[(i * 8) + 5]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x18, vertexBuffer[(i * 8) + 6]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x1C, vertexBuffer[(i * 8) + 7]);
                WriteUint(ref outBytes, (i * elemSize) + 0x20, weights[i]);
                WriteUint(ref outBytes, (i * elemSize) + 0x24, ids[i]);
            }

            return outBytes;
        }

        public byte[] SerializeTieVertices()
        {
            int elemSize = 0x18;
            byte[] outBytes = new byte[(vertexBuffer.Length / 8) * elemSize];

            for (int i = 0; i < vertexBuffer.Length / 8; i++)
            {
                WriteFloat(ref outBytes, (i * elemSize) + 0x00, vertexBuffer[(i * 8) + 0]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x04, vertexBuffer[(i * 8) + 1]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x08, vertexBuffer[(i * 8) + 2]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x0C, vertexBuffer[(i * 8) + 3]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x10, vertexBuffer[(i * 8) + 4]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x14, vertexBuffer[(i * 8) + 5]);
            }

            return outBytes;
        }

        public byte[] SerializeUVs()
        {
            int elemSize = 0x08;
            byte[] outBytes = new byte[(vertexBuffer.Length / 8) * elemSize];

            for (int i = 0; i < vertexBuffer.Length / 8; i++)
            {
                WriteFloat(ref outBytes, (i * elemSize) + 0x00, vertexBuffer[(i * 8) + 6]);
                WriteFloat(ref outBytes, (i * elemSize) + 0x04, vertexBuffer[(i * 8) + 7]);
            }

            return outBytes;
        }

        //Get vertices with UV's baked in, but no normals
        public static float[] GetVerticesUV(FileStream fs, int vertexPointer, int vertexCount, int elemSize)
        {
            float[] vertexBuffer = new float[vertexCount * 8];

            //List<float> vertexBuffer = new List<float>();
            byte[] vertBlock = ReadBlock(fs, vertexPointer, vertexCount * elemSize);
            for (int i = 0; i < vertexCount; i++)
            {
                vertexBuffer[(i * 8) + 0] = (ReadFloat(vertBlock, (i * elemSize) + 0x00));    //VertexX
                vertexBuffer[(i * 8) + 1] = (ReadFloat(vertBlock, (i * elemSize) + 0x04));    //VertexY
                vertexBuffer[(i * 8) + 2] = (ReadFloat(vertBlock, (i * elemSize) + 0x08));    //VertexZ
                vertexBuffer[(i * 8) + 3] = (ReadFloat(vertBlock, (i * elemSize) + 0x14));    //Actually vertexcolors
                vertexBuffer[(i * 8) + 4] = 0;    //NormY
                vertexBuffer[(i * 8) + 5] = 0;    //NormZ
                vertexBuffer[(i * 8) + 6] = (ReadFloat(vertBlock, (i * elemSize) + 0x0C));    //UVu
                vertexBuffer[(i * 8) + 7] = (ReadFloat(vertBlock, (i * elemSize) + 0x10));    //UVv
            }
            return vertexBuffer;
        }

        public static byte[] GetVertexBytesUV(float[] vertexBuffer)
        {
            int vertexCount = vertexBuffer.Length / 8;
            byte[] vertexBytes = new byte[vertexCount * 0x18];

            for (int i = 0; i < vertexCount; i++)
            {
                WriteFloat(ref vertexBytes, (i * 0x18) + 0x00, vertexBuffer[(i * 8) + 0]);
                WriteFloat(ref vertexBytes, (i * 0x18) + 0x04, vertexBuffer[(i * 8) + 1]);
                WriteFloat(ref vertexBytes, (i * 0x18) + 0x08, vertexBuffer[(i * 8) + 2]);
                WriteFloat(ref vertexBytes, (i * 0x18) + 0x0C, vertexBuffer[(i * 8) + 6]);
                WriteFloat(ref vertexBytes, (i * 0x18) + 0x10, vertexBuffer[(i * 8) + 7]);
                WriteFloat(ref vertexBytes, (i * 0x18) + 0x14, vertexBuffer[(i * 8) + 3]);
            }
            return vertexBytes;
        }

        public byte[] GetFaceBytes()
        {
            byte[] indexBytes = new byte[indexBuffer.Length * sizeof(ushort)];
            for (int i = 0; i < indexBuffer.Length; i++)
            {
                WriteShort(ref indexBytes, i * sizeof(ushort), (short)indexBuffer[i]);
            }
            return indexBytes;
        }

        //Get vertices with UV's located somewhere else
        public static float[] GetVertices(FileStream fs, int vertexPointer, int UVPointer, int vertexCount, int vertexElemSize, int UVElemSize)
        {
            float[] vertexBuffer = new float[vertexCount * 8];

            byte[] vertBlock = ReadBlock(fs, vertexPointer, vertexCount * vertexElemSize);
            byte[] UVBlock = ReadBlock(fs, UVPointer, vertexCount * UVElemSize);
            for (int i = 0; i < vertexCount; i++)
            {
                vertexBuffer[(i * 8) + 0] = (ReadFloat(vertBlock, (i * vertexElemSize) + 0x00));    //VertexX
                vertexBuffer[(i * 8) + 1] = (ReadFloat(vertBlock, (i * vertexElemSize) + 0x04));    //VertexY
                vertexBuffer[(i * 8) + 2] = (ReadFloat(vertBlock, (i * vertexElemSize) + 0x08));    //VertexZ
                vertexBuffer[(i * 8) + 3] = (ReadFloat(vertBlock, (i * vertexElemSize) + 0x0C));    //NormX
                vertexBuffer[(i * 8) + 4] = (ReadFloat(vertBlock, (i * vertexElemSize) + 0x10));    //NormY
                vertexBuffer[(i * 8) + 5] = (ReadFloat(vertBlock, (i * vertexElemSize) + 0x14));    //NormZ

                vertexBuffer[(i * 8) + 6] = (ReadFloat(UVBlock, (i * UVElemSize) + 0x00));    //UVu
                vertexBuffer[(i * 8) + 7] = (ReadFloat(UVBlock, (i * UVElemSize) + 0x04));    //UVv
            }
            return vertexBuffer;
        }

        //Get indices
        public static ushort[] GetIndices(FileStream fs, int indexPointer, int faceCount)
        {
            ushort[] indexBuffer = new ushort[faceCount];
            byte[] indexBlock = ReadBlock(fs, indexPointer, faceCount * sizeof(ushort));

            ushort negate = 0;

            for (int i = 0; i < faceCount; i++)
            {
                ushort face = ReadUshort(indexBlock, i * sizeof(ushort));
                if (i == 0 && face > 0) negate = face;
                indexBuffer[i] = (ushort)(face - negate);
            }

            return indexBuffer;
        }
    }
}
