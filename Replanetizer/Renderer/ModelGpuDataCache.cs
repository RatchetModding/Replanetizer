using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;

namespace Replanetizer.Renderer
{
    internal enum ModelGPULayout
    {
        Static,
        Terrain,
        Tie,
        Animated
    }

    internal sealed class ModelGPUData
    {
        internal readonly Model model;
        internal readonly int vao;
        internal readonly int vbo;
        internal readonly int ibo;
        internal readonly List<ModelGPUData?> subModels;
        internal readonly int metalVao;
        internal readonly int metalVbo;
        internal readonly int metalIbo;
        internal readonly int metalIndexCount;
        internal int References { get; set; }

        private ModelGPUData(Model model, int vao, int vbo, int ibo, List<ModelGPUData?> subModels,
            int metalVao, int metalVbo, int metalIbo, int metalIndexCount)
        {
            this.model = model;
            this.vao = vao;
            this.vbo = vbo;
            this.ibo = ibo;
            this.subModels = subModels;
            this.metalVao = metalVao;
            this.metalVbo = metalVbo;
            this.metalIbo = metalIbo;
            this.metalIndexCount = metalIndexCount;
        }

        internal static ModelGPUData Create(Model model, ModelGPULayout layout, byte[]? ambientRgbas = null, List<int>? terrainLights = null)
        {
            int vao;
            int vbo = 0;
            int ibo = 0;

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            ushort[] indices = model.GetIndices();
            if (indices.Length > 0)
            {
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);
            }

            float[] vertices = BuildVertexData(model, layout, ambientRgbas, terrainLights);
            if (vertices.Length > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            }

            SetupVertexAttribPointers(layout);

            List<ModelGPUData?> subModels = new List<ModelGPUData?>();
            for (int i = 0; i < model.GetSubModelCount(); i++)
            {
                Model? subModel = model.GetSubModel(i);
                subModels.Add(subModel == null ? null : Create(subModel, layout));
            }

            int metalVao = 0;
            int metalVbo = 0;
            int metalIbo = 0;
            int metalIndexCount = 0;
            if (model is MetalModel metalModel && metalModel.metalIndexBuffer.Length > 0
                && metalModel.metalVertexBuffer.Length > 0
                && (layout != ModelGPULayout.Animated
                    || (metalModel.metalVertexBoneIds.Length >= metalModel.metalVertexCount
                        && metalModel.metalVertexBoneWeights.Length >= metalModel.metalVertexCount)))
            {
                CreateMetalBuffers(metalModel, layout, out metalVao, out metalVbo, out metalIbo, out metalIndexCount);
            }

            return new ModelGPUData(model, vao, vbo, ibo, subModels,
                metalVao, metalVbo, metalIbo, metalIndexCount);
        }

        private static void CreateMetalBuffers(Model model, ModelGPULayout layout, out int vao, out int vbo, out int ibo, out int indexCount)
        {
            MetalModel metalModel = (MetalModel) model;
            vao = 0;
            vbo = 0;
            ibo = 0;
            indexCount = metalModel.metalIndexBuffer.Length;

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            int regularVertexCount = model.GetVertices().Length / 8;
            ushort[] metalIndices = new ushort[metalModel.metalIndexBuffer.Length];
            for (int i = 0; i < metalIndices.Length; i++)
            {
                metalIndices[i] = (ushort) (metalModel.metalIndexBuffer[i] - regularVertexCount);
            }
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                metalIndices.Length * sizeof(ushort), metalIndices, BufferUsageHint.StaticDraw);

            float[] vertices = BuildMetalVertexData(metalModel, layout);
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            SetupMetalVertexAttribPointers(layout);
        }

        private static float[] BuildMetalVertexData(MetalModel model, ModelGPULayout layout)
        {
            int vertexCount = model.metalVertexBuffer.Length / 8;
            bool animated = layout == ModelGPULayout.Animated;
            int stride = animated ? 10 : 8;
            float[] result = new float[vertexCount * stride];
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    result[i * stride + j] = model.metalVertexBuffer[i * 8 + j];
                }

                if (animated)
                {
                    result[i * stride + 8] = BitConverter.UInt32BitsToSingle(model.metalVertexBoneIds[i]);
                    result[i * stride + 9] = BitConverter.UInt32BitsToSingle(model.metalVertexBoneWeights[i]);
                }
            }
            return result;
        }

        private static void SetupMetalVertexAttribPointers(ModelGPULayout layout)
        {
            if (layout == ModelGPULayout.Animated)
            {
                GLUtil.ActivateNumberOfVertexAttribArrays(5);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 40, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 40, 12);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 40, 24);
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, false, 40, 32);
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.UnsignedByte, true, 40, 36);
                return;
            }

            GLUtil.ActivateNumberOfVertexAttribArrays(3);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 32, 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 32, 12);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 32, 24);
        }

        private static float[] BuildVertexData(Model model, ModelGPULayout layout, byte[]? ambientRgbas, List<int>? terrainLights)
        {
            float[] source = model.GetVertices();
            if (layout == ModelGPULayout.Static || layout == ModelGPULayout.Animated)
            {
                if (layout == ModelGPULayout.Static) return source;

                uint[] boneIds = model.vertexBoneIds;
                uint[] boneWeights = model.vertexBoneWeights;
                float[] result = new float[source.Length + boneIds.Length + boneWeights.Length];
                for (int i = 0; i < source.Length / 8; i++)
                {
                    CopyVertex(source, result, i, 10);
                    result[10 * i + 8] = BitConverter.UInt32BitsToSingle(boneIds[i]);
                    result[10 * i + 9] = BitConverter.UInt32BitsToSingle(boneWeights[i]);
                }
                return result;
            }

            int stride = layout == ModelGPULayout.Terrain ? 10 : 9;
            float[] fullData = new float[source.Length / 8 * stride];
            for (int i = 0; i < source.Length / 8; i++)
            {
                CopyVertex(source, fullData, i, stride);
                if (layout == ModelGPULayout.Terrain)
                {
                    fullData[stride * i + 8] = ambientRgbas != null && i * 4 < ambientRgbas.Length
                        ? BitConverter.ToSingle(ambientRgbas, i * 4) : 0.0f;
                    fullData[stride * i + 9] = terrainLights != null && i < terrainLights.Count
                        ? terrainLights[i] : 0.0f;
                }
                else
                {
                    fullData[stride * i + 8] = ambientRgbas != null && i * 4 < ambientRgbas.Length
                        ? BitConverter.ToSingle(ambientRgbas, i * 4) : 0.0f;
                }
            }
            return fullData;
        }

        private static void CopyVertex(float[] source, float[] destination, int index, int stride)
        {
            for (int j = 0; j < 8; j++) destination[stride * index + j] = source[8 * index + j];
        }

        private static void SetupVertexAttribPointers(ModelGPULayout layout)
        {
            switch (layout)
            {
                case ModelGPULayout.Terrain:
                    GLUtil.ActivateNumberOfVertexAttribArrays(5);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 40, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 40, 12);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 40, 24);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, 40, 32);
                    GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, 40, 36);
                    break;
                case ModelGPULayout.Tie:
                    GLUtil.ActivateNumberOfVertexAttribArrays(4);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 36, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 36, 12);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 36, 24);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, 36, 32);
                    break;
                case ModelGPULayout.Animated:
                    GLUtil.ActivateNumberOfVertexAttribArrays(5);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 40, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 40, 12);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 40, 24);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, false, 40, 32);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.UnsignedByte, true, 40, 36);
                    break;
                default:
                    GLUtil.ActivateNumberOfVertexAttribArrays(3);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 32, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 32, 12);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 32, 24);
                    break;
            }
        }

        internal void Dispose()
        {
            foreach (ModelGPUData? subModel in subModels) subModel?.Dispose();
            if (ibo != 0) GL.DeleteBuffer(ibo);
            if (vbo != 0) GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            if (metalIbo != 0) GL.DeleteBuffer(metalIbo);
            if (metalVbo != 0) GL.DeleteBuffer(metalVbo);
            if (metalVao != 0) GL.DeleteVertexArray(metalVao);
        }
    }

    public sealed class ModelGPUDataCache : IDisposable
    {
        private readonly Dictionary<CacheKey, ModelGPUData> data = new Dictionary<CacheKey, ModelGPUData>();

        internal ModelGPUData Acquire(Model model, ModelGPULayout layout, string instanceKey = "", byte[]? ambientRgbas = null, List<int>? terrainLights = null)
        {
            CacheKey key = new CacheKey(model, model.meshDataVersion, layout, instanceKey);
            if (!data.TryGetValue(key, out ModelGPUData? gpuData))
            {
                gpuData = ModelGPUData.Create(model, layout, ambientRgbas, terrainLights);
                data.Add(key, gpuData);
            }
            gpuData.References++;
            return gpuData;
        }

        internal void Release(ModelGPUData? gpuData)
        {
            if (gpuData == null) return;
            CacheKey? key = null;
            foreach (KeyValuePair<CacheKey, ModelGPUData> pair in data)
            {
                if (ReferenceEquals(pair.Value, gpuData)) { key = pair.Key; break; }
            }
            if (key == null) return;
            gpuData.References--;
            if (gpuData.References <= 0)
            {
                gpuData.Dispose();
                data.Remove(key.Value);
            }
        }

        public void Dispose()
        {
            foreach (ModelGPUData gpuData in data.Values) gpuData.Dispose();
            data.Clear();
        }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly Model model;
            private readonly uint meshDataVersion;
            private readonly ModelGPULayout layout;
            private readonly string instanceKey;

            internal CacheKey(Model model, uint meshDataVersion, ModelGPULayout layout, string instanceKey)
            {
                this.model = model;
                this.meshDataVersion = meshDataVersion;
                this.layout = layout;
                this.instanceKey = instanceKey;
            }

            public bool Equals(CacheKey other) => ReferenceEquals(model, other.model)
                && meshDataVersion == other.meshDataVersion
                && layout == other.layout
                && instanceKey == other.instanceKey;
            public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(RuntimeHelpers.GetHashCode(model), meshDataVersion, layout, instanceKey);
        }
    }
}
