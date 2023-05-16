// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace LibReplanetizer
{
    public class GLTFExporter : Exporter
    {
        private class GLTFDataObject
        {
            public class GLTFAssetProperty
            {
                public String version = "2.0";
                public String generator = "Replanetizer glTF Exporter";
            }

            public class GLTFNodesEntry
            {
                public string name;
                public int mesh;

                public GLTFNodesEntry(string name, int mesh)
                {
                    this.name = name;
                    this.mesh = mesh;
                }
            }

            public class GLTFScenesEntry
            {
                public String name;
                public int[] nodes;

                public GLTFScenesEntry(String name, int[] nodes)
                {
                    this.name = name;
                    this.nodes = nodes;
                }
            }

            public class GLTFMaterialEntry
            {
                // alphaMode
                public const String OPAQUE = "OPAQUE";
                public const String MASK = "MASK";
                public const String BLEND = "BLEND";

                public class GLTFMaterialPBRValues
                {
                    public class GLTFMaterialPBRValuesBaseColorTexture
                    {
                        public int index;

                        public GLTFMaterialPBRValuesBaseColorTexture(int index)
                        {
                            this.index = index;
                        }
                    }

                    public GLTFMaterialPBRValuesBaseColorTexture baseColorTexture;

                    // Metallic and Roughness are hardcoded, change this if support for exporting
                    // special moby rendering models is implemented.
                    public float metallicFactor = 0.0f;
                    public float roughnessFactor = 1.0f;

                    public GLTFMaterialPBRValues(GLTFMaterialPBRValuesBaseColorTexture baseColorTexture)
                    {
                        this.baseColorTexture = baseColorTexture;
                    }

                    public GLTFMaterialPBRValues(int baseColorTextureIndex)
                    {
                        this.baseColorTexture = new GLTFMaterialPBRValuesBaseColorTexture(baseColorTextureIndex);
                    }
                }

                public bool doubleSided = true;
                public String name;
                public String alphaMode;
                public float alphaCutoff = 0.5f; // Only used if alphaMode == MASK.
                public GLTFMaterialPBRValues pbrMetallicRoughness;

                public GLTFMaterialEntry(String name, String alphaMode, GLTFMaterialPBRValues pbrMetallicRoughness)
                {
                    this.name = name;
                    this.alphaMode = alphaMode;
                    this.pbrMetallicRoughness = pbrMetallicRoughness;
                }

                public GLTFMaterialEntry(TextureConfig conf, int texOffset)
                {
                    this.name = "Material_" + texOffset + "_" + conf.id;
                    this.alphaMode = (conf.IgnoresTransparency()) ? GLTFMaterialEntry.OPAQUE : GLTFMaterialEntry.BLEND;
                    this.pbrMetallicRoughness = new GLTFMaterialEntry.GLTFMaterialPBRValues(texOffset);
                }
            }

            public class GLTFMeshEntry
            {
                public class GLTFMeshPrimitivesEntry
                {
                    public class GLTFMeshPrimitivesEntryAttributes
                    {
                        public int POSITION;
                        public int TEXCOORD_0;
                        public int NORMAL;

                        public GLTFMeshPrimitivesEntryAttributes(int POSITION, int TEXCOORD_0, int NORMAL)
                        {
                            this.POSITION = POSITION;
                            this.TEXCOORD_0 = TEXCOORD_0;
                            this.NORMAL = NORMAL;
                        }
                    }

                    public GLTFMeshPrimitivesEntryAttributes attributes;
                    public int indices;
                    public int material;

                    public GLTFMeshPrimitivesEntry(GLTFMeshPrimitivesEntryAttributes attributes, int indices, int material)
                    {
                        this.attributes = attributes;
                        this.indices = indices;
                        this.material = material;
                    }
                }
                public String name;
                public GLTFMeshPrimitivesEntry[] primitives;

                public GLTFMeshEntry(String name, GLTFMeshPrimitivesEntry[] primitives)
                {
                    this.name = name;
                    this.primitives = primitives;
                }
            }

            public class GLTFTextureEntry
            {
                public int sampler;
                public int source;

                public GLTFTextureEntry(int sampler, int source)
                {
                    this.sampler = sampler;
                    this.source = source;
                }
            }

            public class GLTFImageEntry
            {
                // mimeType
                private const String PNG = "image/png";
                private const String JPEG = "image/jpeg";

                public String mimeType;
                public String name;

                // Image is stored in a buffer
                public int? bufferView = null;

                // Image is stored in a file.
                public String? uri = null;

                public GLTFImageEntry(int bufferView, String mimeType, String name)
                {
                    this.bufferView = bufferView;
                    this.mimeType = mimeType;
                    this.name = name;
                }

                public GLTFImageEntry(String uri, String mimeType, String name)
                {
                    this.uri = uri;
                    this.mimeType = mimeType;
                    this.name = name;
                }

                public GLTFImageEntry(TextureConfig conf)
                {
                    this.uri = conf.id + ".png";
                    this.mimeType = PNG;
                    this.name = "Image" + conf.id;
                }
            }

            public class GLTFAccessorEntry
            {
                // componentType (SIGNED_INT is not supported [see glTF docs 3.6.2.2. "Signed 32-bit integer components are not supported."])
                public const int SIGNED_BYTE = 5120;
                public const int UNSIGNED_BYTE = 5121;
                public const int SIGNED_SHORT = 5122;
                public const int UNSIGNED_SHORT = 5123;
                public const int UNSIGNED_INT = 5125;
                public const int FLOAT = 5126;

                // type
                public const String SCALAR = "SCALAR";
                public const String VEC2 = "VEC2";
                public const String VEC3 = "VEC3";
                public const String VEC4 = "VEC4";
                public const String MAT2 = "MAT2";
                public const String MAT3 = "MAT3";
                public const String MAT4 = "MAT4";

                public String name;
                public int bufferView;
                public int componentType;
                public int count;
                public int byteOffset;
                public float[]? max;
                public float[]? min;
                public String type;

                public GLTFAccessorEntry(String name, int bufferView, int componentType, int count, int byteOffset, float[] max, float[] min, String type)
                {
                    this.name = name;
                    this.bufferView = bufferView;
                    this.componentType = componentType;
                    this.count = count;
                    this.byteOffset = byteOffset;
                    this.max = max;
                    this.min = min;
                    this.type = type;
                }

                public GLTFAccessorEntry(String name, int bufferView, int componentType, int count, int byteOffset, String type)
                {
                    this.name = name;
                    this.bufferView = bufferView;
                    this.componentType = componentType;
                    this.count = count;
                    this.byteOffset = byteOffset;
                    this.type = type;
                }
            }

            public class GLTFBufferViewEntry
            {
                // target
                public const int ARRAY_BUFFER = 34962;
                public const int ELEMENT_ARRAY_BUFFER = 34963;

                public String name;
                public int buffer;
                public int byteLength;
                public int byteOffset;
                public int byteStride;
                public int target;

                public GLTFBufferViewEntry(String name, int buffer, int byteLength, int byteOffset, int byteStride, int target)
                {
                    this.name = name;
                    this.buffer = buffer;
                    this.byteLength = byteLength;
                    this.byteOffset = byteOffset;
                    this.byteStride = byteStride;
                    this.target = target;
                }
            }

            public class GLTFSamplerEntry
            {
                // magFilter, minFilter
                public const int NEAREST = 9728;
                public const int LINEAR = 9729;
                // minFilter
                public const int NEAREST_MIPMAP_NEAREST = 9984;
                public const int LINEAR_MIPMAP_NEAREST = 9985;
                public const int NEAREST_MIPMAP_LINEAR = 9986;
                public const int LINEAR_MIPMAP_LINEAR = 9987;
                // wrapS, wrapT
                public const int CLAMP_TO_EDGE = 33071;
                public const int MIRRORED_REPEAT = 33648;
                public const int REPEAT = 10497;

                public String name;
                public int magFilter;
                public int minFilter;
                public int wrapS;
                public int wrapT;

                public GLTFSamplerEntry(String name, int magFilter, int minFilter, int wrapS, int wrapT)
                {
                    this.name = name;
                    this.magFilter = magFilter;
                    this.minFilter = minFilter;
                    this.wrapS = wrapS;
                    this.wrapT = wrapT;
                }

                public GLTFSamplerEntry(TextureConfig conf)
                {
                    this.name = "Texture " + conf.id;
                    this.magFilter = LINEAR;
                    this.minFilter = LINEAR;

                    TextureConfig.WrapMode wrapS = conf.wrapModeS;
                    TextureConfig.WrapMode wrapT = conf.wrapModeT;

                    this.wrapS = (wrapS == TextureConfig.WrapMode.ClampEdge) ? CLAMP_TO_EDGE : REPEAT;
                    this.wrapT = (wrapT == TextureConfig.WrapMode.ClampEdge) ? CLAMP_TO_EDGE : REPEAT;
                }
            }

            public class GLTFBufferEntry
            {
                public String name;
                public int byteLength;
                public String uri;

                public GLTFBufferEntry(String name, int byteLength, String uri)
                {
                    this.name = name;
                    this.byteLength = byteLength;
                    this.uri = uri;
                }

                public GLTFBufferEntry(String name, Array arr)
                {
                    this.name = name;

                    byte[] data = new byte[Buffer.ByteLength(arr)];
                    Buffer.BlockCopy(arr, 0, data, 0, Buffer.ByteLength(arr));

                    this.byteLength = data.Length;
                    this.uri = "data:application/octet-stream;base64," + Convert.ToBase64String(data, Base64FormattingOptions.None);
                }
            }

            public GLTFAssetProperty asset = new GLTFAssetProperty();
            public String[] extensionUsed = { };
            public int scene = 0;
            public GLTFNodesEntry[] nodes = new GLTFNodesEntry[] { new GLTFNodesEntry("modelObject", 0) };
            public GLTFScenesEntry[] scenes = new GLTFScenesEntry[] { new GLTFScenesEntry("modelObject", new int[] { 0 }) };
            public GLTFMaterialEntry[] materials = new GLTFMaterialEntry[] { };
            public GLTFMeshEntry[] meshes = new GLTFMeshEntry[] { };
            public GLTFTextureEntry[] textures = new GLTFTextureEntry[] { };
            public GLTFImageEntry[] images = new GLTFImageEntry[] { };
            public GLTFAccessorEntry[] accessors = new GLTFAccessorEntry[] { };
            public GLTFBufferViewEntry[] bufferViews = new GLTFBufferViewEntry[] { };
            public GLTFSamplerEntry[] samplers = new GLTFSamplerEntry[] { };
            public GLTFBufferEntry[] buffers = new GLTFBufferEntry[] { };

            public GLTFDataObject(Level level, Model model, bool includeSkeleton)
            {
                // skybox model has no normals and thus the vertex buffer has a different layout
                // if we see other cases like this, it may be advisable to generalize this
                bool skyboxModel = (model is SkyboxModel);

                bool hasNormals = !(skyboxModel);
                bool hasVertexColors = (skyboxModel) || (model is TerrainModel);

                int vOffset = 0x00;
                int vnOffset = 0x03;
                int vtOffset = (skyboxModel) ? 0x03 : 0x06;
                int vcOffset = 0x05;

                // Separate vertex buffer into components
                Vector3[] vertices = new Vector3[model.vertexCount];
                Vector3[] normals = new Vector3[model.vertexCount];
                Vector2[] UVs = new Vector2[model.vertexCount];
                Vector4[] vertexColors = new Vector4[model.vertexCount];

                for (int i = 0; i < model.vertexCount; i++)
                {
                    float px = model.vertexBuffer[(i * model.vertexStride) + vOffset + 0x0] * model.size;
                    float py = model.vertexBuffer[(i * model.vertexStride) + vOffset + 0x1] * model.size;
                    float pz = model.vertexBuffer[(i * model.vertexStride) + vOffset + 0x2] * model.size;
                    vertices[i] = new Vector3(px, py, pz);
                }

                for (int i = 0; i < model.vertexCount; i++)
                {
                    float u = model.vertexBuffer[(i * model.vertexStride) + vtOffset + 0x0];
                    float v = model.vertexBuffer[(i * model.vertexStride) + vtOffset + 0x1];
                    UVs[i] = new Vector2(u, v);
                }

                if (hasNormals)
                {
                    for (int i = 0; i < model.vertexCount; i++)
                    {
                        float nx = model.vertexBuffer[(i * model.vertexStride) + vnOffset + 0x0];
                        float ny = model.vertexBuffer[(i * model.vertexStride) + vnOffset + 0x1];
                        float nz = model.vertexBuffer[(i * model.vertexStride) + vnOffset + 0x2];
                        normals[i] = new Vector3(nx, ny, nz);
                    }
                }

                if (hasVertexColors)
                {
                    for (int i = 0; i < model.vertexCount; i++)
                    {
                        byte[] colors = BitConverter.GetBytes(model.vertexBuffer[(i * model.vertexStride) + vcOffset + 0x00]);
                        float a = ((float) colors[0]) / 255.0f;
                        float b = ((float) colors[1]) / 255.0f;
                        float g = ((float) colors[2]) / 255.0f;
                        float r = ((float) colors[3]) / 255.0f;
                        vertexColors[i] = new Vector4(r, g, b, a);
                    }
                }

                ushort[] gltfIndexBuffer = new ushort[model.faceCount * 3];
                model.indexBuffer.CopyTo(gltfIndexBuffer, 0);

                // Correct face orientation
                if (hasNormals)
                {
                    for (int i = 0; i < model.faceCount; i++)
                    {
                        ushort f1 = gltfIndexBuffer[i * 3 + 0];
                        ushort f2 = gltfIndexBuffer[i * 3 + 1];
                        ushort f3 = gltfIndexBuffer[i * 3 + 2];

                        if (ShouldReverseWinding(vertices, normals, f1, f2, f3))
                        {
                            gltfIndexBuffer[i * 3 + 1] = f3;
                            gltfIndexBuffer[i * 3 + 2] = f2;
                        }
                    }
                }

                // Construct Vertex Buffer for glTF export
                int gltfVertexBufferStride = 3 + 2;
                int gltfVertexPosOffset = 0;
                int gltfVertexUVOffset = 3;
                int gltfVertexNormalOffset = 0;
                int gltfVertexColorOffset = 0;
                if (hasNormals)
                {
                    gltfVertexNormalOffset = gltfVertexBufferStride;
                    gltfVertexBufferStride += 3;
                }
                if (hasVertexColors)
                {
                    gltfVertexColorOffset = gltfVertexBufferStride;
                    gltfVertexBufferStride += 4;
                }

                int gltfVertexBufferByteStride = gltfVertexBufferStride * 4;
                int gltfVertexPosByteOffset = gltfVertexPosOffset * 4;
                int gltfVertexUVByteOffset = gltfVertexUVOffset * 4;
                int gltfVertexNormalByteOffset = gltfVertexNormalOffset * 4;
                int gltfVertexColorByteOffset = gltfVertexColorOffset * 4;

                float[] gltfVertexBuffer = new float[gltfVertexBufferStride * model.vertexCount];

                for (int i = 0; i < model.vertexCount; i++)
                {
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexPosOffset + 0x0] = vertices[i].X;
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexPosOffset + 0x1] = vertices[i].Y;
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexPosOffset + 0x2] = vertices[i].Z;
                }

                for (int i = 0; i < model.vertexCount; i++)
                {
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexUVOffset + 0x0] = UVs[i].X;
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexUVOffset + 0x1] = UVs[i].Y;
                }

                if (hasNormals)
                {
                    for (int i = 0; i < model.vertexCount; i++)
                    {
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexNormalOffset + 0x0] = normals[i].X;
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexNormalOffset + 0x1] = normals[i].Y;
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexNormalOffset + 0x2] = normals[i].Z;
                    }
                }

                if (hasVertexColors)
                {
                    for (int i = 0; i < model.vertexCount; i++)
                    {
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexColorOffset + 0x0] = vertexColors[i].X;
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexColorOffset + 0x1] = vertexColors[i].Y;
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexColorOffset + 0x2] = vertexColors[i].Z;
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexColorOffset + 0x3] = vertexColors[i].W;
                    }
                }

                Dictionary<int, int> texIDToImageOffset = new Dictionary<int, int>();

                List<GLTFImageEntry> listImages = new List<GLTFImageEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    if (!texIDToImageOffset.ContainsKey(conf.id))
                    {
                        texIDToImageOffset.Add(conf.id, listImages.Count);
                        listImages.Add(new GLTFImageEntry(conf));
                    }
                }
                this.images = listImages.ToArray();

                List<GLTFSamplerEntry> listSamplers = new List<GLTFSamplerEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listSamplers.Add(new GLTFSamplerEntry(model.textureConfig[i]));
                }
                this.samplers = listSamplers.ToArray();

                List<GLTFTextureEntry> listTextures = new List<GLTFTextureEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    listTextures.Add(new GLTFTextureEntry(i, texIDToImageOffset[conf.id]));
                }
                this.textures = listTextures.ToArray();

                List<GLTFBufferEntry> listBuffers = new List<GLTFBufferEntry>();
                listBuffers.Add(new GLTFBufferEntry("VertexBuffer", gltfVertexBuffer));
                listBuffers.Add(new GLTFBufferEntry("IndexBuffer", gltfIndexBuffer));
                this.buffers = listBuffers.ToArray();

                List<GLTFBufferViewEntry> listBufferViews = new List<GLTFBufferViewEntry>();
                listBufferViews.Add(new GLTFBufferViewEntry("VertexBufferView", 0, gltfVertexBufferByteStride * model.vertexCount, 0, gltfVertexBufferByteStride, GLTFBufferViewEntry.ARRAY_BUFFER));
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    int stride = 2;
                    int byteOffset = stride * conf.start;
                    int byteLength = stride * conf.size;

                    listBufferViews.Add(new GLTFBufferViewEntry("IndexBufferView" + i, 1, byteLength, byteOffset, stride, GLTFBufferViewEntry.ELEMENT_ARRAY_BUFFER));
                }
                this.bufferViews = listBufferViews.ToArray();

                List<GLTFAccessorEntry> listAccessors = new List<GLTFAccessorEntry>();
                listAccessors.Add(new GLTFAccessorEntry("VertexPosAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, gltfVertexPosByteOffset, GLTFAccessorEntry.VEC3));
                listAccessors.Add(new GLTFAccessorEntry("VertexUVAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, gltfVertexUVByteOffset, GLTFAccessorEntry.VEC2));
                listAccessors.Add(new GLTFAccessorEntry("VertexNormalAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, gltfVertexNormalByteOffset, GLTFAccessorEntry.VEC3));
                listAccessors.Add(new GLTFAccessorEntry("VertexColorAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, gltfVertexColorByteOffset, GLTFAccessorEntry.VEC4));
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listAccessors.Add(new GLTFAccessorEntry("IndexAccessor" + i, i + 1, GLTFAccessorEntry.UNSIGNED_SHORT, model.textureConfig[i].size, 0, GLTFAccessorEntry.SCALAR));
                }

                this.accessors = listAccessors.ToArray();

                List<GLTFMaterialEntry> listMaterials = new List<GLTFMaterialEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listMaterials.Add(new GLTFMaterialEntry(model.textureConfig[i], i));
                }
                this.materials = listMaterials.ToArray();

                GLTFMeshEntry.GLTFMeshPrimitivesEntry.GLTFMeshPrimitivesEntryAttributes vertexAttribs = new GLTFMeshEntry.GLTFMeshPrimitivesEntry.GLTFMeshPrimitivesEntryAttributes(0, 1, 2);

                List<GLTFMeshEntry.GLTFMeshPrimitivesEntry> listMeshPrimitives = new List<GLTFMeshEntry.GLTFMeshPrimitivesEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listMeshPrimitives.Add(new GLTFMeshEntry.GLTFMeshPrimitivesEntry(vertexAttribs, 4 + i, i));
                }

                List<GLTFMeshEntry> listMeshes = new List<GLTFMeshEntry>();
                listMeshes.Add(new GLTFMeshEntry("Mesh", listMeshPrimitives.ToArray()));
                this.meshes = listMeshes.ToArray();
            }
        }

        private ExporterModelSettings settings = new ExporterModelSettings();

        public GLTFExporter()
        {
        }

        public GLTFExporter(ExporterModelSettings settings)
        {
            this.settings = settings;
        }

        public override string GetFileEnding()
        {
            return ".gltf";
        }

        public override void ExportModel(string fileName, Level level, Model model)
        {
            LOGGER.Trace(fileName);

            bool includeSkeleton = (model is MobyModel mobyModel && mobyModel.boneCount != 0 && level.game != GameType.DL);

            StreamWriter stream = new StreamWriter(fileName);

            try
            {
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                jsonOptions.WriteIndented = true;
                jsonOptions.IncludeFields = true;
                jsonOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                GLTFDataObject gltfDataObject = new GLTFDataObject(level, model, includeSkeleton);
                String gltfJson = JsonSerializer.Serialize(gltfDataObject, jsonOptions);
                stream.Write(gltfJson);
            }
            finally
            {
                stream.Close();
            }
        }
    }
}
