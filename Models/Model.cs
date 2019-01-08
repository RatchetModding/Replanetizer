using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using static RatchetEdit.DataFunctions;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit
{
    /*
        General purpose 3D model used for rendering
    */

    public abstract class Model
    {
        public short id { get; set; }
        public float size;
        public float[] vertexBuffer;
        public ushort[] indexBuffer;
        public List<TextureConfig> textureConfig;
        int VBO = 0;
        int IBO = 0;

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
            Draw(glControl.textures);
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
            int IDoffset = 0, startOffset = 0, sizeOffset = 0;

            switch (elemSize)
            {
                case 0x10:
                    IDoffset = 0x00;
                    startOffset = 0x04;
                    sizeOffset = 0x08;
                    break;
                case 0x18:
                    IDoffset = 0x00;
                    startOffset = 0x08;
                    sizeOffset = 0x0C;
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
                textureConfigs.Add(textureConfig);
            }
            return textureConfigs;
        }

        //Get vertices with UV's baked in
        public static float[] GetVertices(FileStream fs, int vertexPointer, int vertexCount, int elemSize)
        {
            float[] vertexBuffer = new float[vertexCount * 8];

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
            }
            return vertexBuffer;
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
                vertexBuffer[(i * 8) + 3] = 0;    //NormX
                vertexBuffer[(i * 8) + 4] = 0;    //NormY
                vertexBuffer[(i * 8) + 5] = 0;    //NormZ
                vertexBuffer[(i * 8) + 6] = (ReadFloat(vertBlock, (i * elemSize) + 0x0C));    //UVu
                vertexBuffer[(i * 8) + 7] = (ReadFloat(vertBlock, (i * elemSize) + 0x10));    //UVv
            }
            return vertexBuffer;
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
            for (int i = 0; i < faceCount; i++)
            {
                indexBuffer[i] = (ReadUshort(indexBlock, i * sizeof(ushort)));
            }

            return indexBuffer;
        }
    }
}
