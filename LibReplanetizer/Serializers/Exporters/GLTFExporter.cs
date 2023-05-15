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

                    MemoryStream ms = new MemoryStream();
                    StreamWriter sw = new StreamWriter(ms);

                    foreach (object obj in arr)
                    {
                        sw.Write(obj);
                    }
                    sw.Flush();

                    byte[] data = ms.GetBuffer();

                    sw.Close();
                    ms.Close();

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
                List<GLTFImageEntry> listImages = new List<GLTFImageEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    listImages.Add(new GLTFImageEntry(conf));
                }
                this.images = listImages.ToArray();

                List<GLTFSamplerEntry> listSamplers = new List<GLTFSamplerEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    listSamplers.Add(new GLTFSamplerEntry(conf));
                }
                this.samplers = listSamplers.ToArray();

                List<GLTFTextureEntry> listTextures = new List<GLTFTextureEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listTextures.Add(new GLTFTextureEntry(i, i));
                }
                this.textures = listTextures.ToArray();

                List<GLTFBufferEntry> listBuffers = new List<GLTFBufferEntry>();
                listBuffers.Add(new GLTFBufferEntry("VertexBuffer", model.vertexBuffer));
                listBuffers.Add(new GLTFBufferEntry("IndexBuffer", model.indexBuffer));
                this.buffers = listBuffers.ToArray();

                List<GLTFBufferViewEntry> listBufferViews = new List<GLTFBufferViewEntry>();
                listBufferViews.Add(new GLTFBufferViewEntry("VertexBufferView", 0, 32 * model.vertexCount, 0, 32, GLTFBufferViewEntry.ARRAY_BUFFER));
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    int strideIndex = 6;
                    int byteOffsetIndex = strideIndex * conf.start;
                    int byteLengthIndex = strideIndex * conf.size;

                    listBufferViews.Add(new GLTFBufferViewEntry("IndexBufferView" + i, 1, byteLengthIndex, byteOffsetIndex, strideIndex, GLTFBufferViewEntry.ELEMENT_ARRAY_BUFFER));
                }
                this.bufferViews = listBufferViews.ToArray();

                List<GLTFAccessorEntry> listAccessors = new List<GLTFAccessorEntry>();
                listAccessors.Add(new GLTFAccessorEntry("VertexPosAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, 0, GLTFAccessorEntry.VEC3));
                listAccessors.Add(new GLTFAccessorEntry("VertexNormalAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, 12, GLTFAccessorEntry.VEC3));
                listAccessors.Add(new GLTFAccessorEntry("VertexUVAccessor", 0, GLTFAccessorEntry.FLOAT, model.vertexCount, 24, GLTFAccessorEntry.VEC2));
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];

                    listAccessors.Add(new GLTFAccessorEntry("IndexAccessor" + i, i + 1, GLTFAccessorEntry.UNSIGNED_SHORT, conf.size, 0, GLTFAccessorEntry.VEC3));
                }

                this.accessors = listAccessors.ToArray();

                List<GLTFMaterialEntry> listMaterials = new List<GLTFMaterialEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];

                    String alphaMode = (conf.IgnoresTransparency()) ? GLTFMaterialEntry.OPAQUE : GLTFMaterialEntry.BLEND;
                    GLTFMaterialEntry.GLTFMaterialPBRValues pbrValues = new GLTFMaterialEntry.GLTFMaterialPBRValues(i);

                    listMaterials.Add(new GLTFMaterialEntry("Material" + i, alphaMode, pbrValues));
                }
                this.materials = listMaterials.ToArray();

                GLTFMeshEntry.GLTFMeshPrimitivesEntry.GLTFMeshPrimitivesEntryAttributes vertexAttribs = new GLTFMeshEntry.GLTFMeshPrimitivesEntry.GLTFMeshPrimitivesEntryAttributes(0, 2, 1);

                List<GLTFMeshEntry.GLTFMeshPrimitivesEntry> listMeshPrimitives = new List<GLTFMeshEntry.GLTFMeshPrimitivesEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listMeshPrimitives.Add(new GLTFMeshEntry.GLTFMeshPrimitivesEntry(vertexAttribs, 3 + i, i));
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
