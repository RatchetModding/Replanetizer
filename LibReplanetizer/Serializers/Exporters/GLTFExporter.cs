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
using SixLabors.ImageSharp;

// Due to the direct implementation of the GLTF standard through C# classes, we cannot
// comply with all of our styling rules.
#pragma warning disable IDE0003
#pragma warning disable IDE1006

namespace LibReplanetizer
{
    public class GLTFExporter : Exporter
    {
        private class GLTFDataObject
        {
            public class GLTFAssetProperty
            {
                public string version = "2.0";
                public string generator = "Replanetizer glTF Exporter";
            }

            public class GLTFNodesEntry
            {
                public string name;
                public int? mesh = null;
                public int? skin = null;
                public int[]? children = null;
                public float[]? translation = null;
                public float[]? rotation = null;
                public float[]? scale = null;

                public GLTFNodesEntry(string name, int mesh, int? skin = null)
                {
                    this.name = name;
                    this.mesh = mesh;
                    this.skin = skin;
                }

                public GLTFNodesEntry(string name, int[] children)
                {
                    this.name = name;
                    this.children = children;
                }

                public GLTFNodesEntry(Skeleton skeleton, float modelSize = 1.0f)
                {
                    this.name = "Skel" + skeleton.bone.id;

                    Matrix4 transformation = skeleton.GetRelativeTransformation();

                    ChangeOrientation(ref transformation, ExporterModelSettings.Orientation.Y_UP);

                    // OpenTK interpretes everything in transposed so we need to transpose first to get what we want.
                    transformation.Transpose();

                    Vector3 t = transformation.ExtractTranslation() * modelSize;
                    this.translation = new float[3] { t.X, t.Y, t.Z };

                    Quaternion q = transformation.ExtractRotation();
                    this.rotation = new float[4] { q.X, q.Y, q.Z, q.W };

                    Vector3 s = transformation.ExtractScale();
                    this.scale = new float[3] { s.X, s.Y, s.Z };

                    if (skeleton.children.Count > 0)
                    {
                        this.children = new int[skeleton.children.Count];
                        for (int i = 0; i < skeleton.children.Count; i++)
                        {
                            this.children[i] = skeleton.children[i].bone.id;
                        }
                    }
                }
            }

            public class GLTFScenesEntry
            {
                public string name;
                public int[] nodes;

                public GLTFScenesEntry(string name, int[] nodes)
                {
                    this.name = name;
                    this.nodes = nodes;
                }
            }

            public class GLTFMaterialEntry
            {
                // alphaMode
                public const string OPAQUE = "OPAQUE";
                public const string MASK = "MASK";
                public const string BLEND = "BLEND";

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

                    public GLTFMaterialPBRValuesBaseColorTexture? baseColorTexture;
                    public float[]? baseColorFactor;

                    // Metallic and Roughness are hardcoded, change this if support for exporting
                    // special moby rendering models is implemented.
                    public float metallicFactor = 0.0f;
                    public float roughnessFactor = 1.0f;

                    public GLTFMaterialPBRValues(GLTFMaterialPBRValuesBaseColorTexture baseColorTexture)
                    {
                        this.baseColorTexture = baseColorTexture;
                    }

                    public GLTFMaterialPBRValues(int baseColorTextureIndex, int texID)
                    {
                        if (texID == 0)
                        {
                            this.baseColorFactor = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
                        }
                        else
                        {
                            this.baseColorTexture = new GLTFMaterialPBRValuesBaseColorTexture(baseColorTextureIndex);
                        }
                    }
                }

                public bool doubleSided = true;
                public string name;
                public string alphaMode;
                public float alphaCutoff = 0.5f; // Only used if alphaMode == MASK.
                public GLTFMaterialPBRValues pbrMetallicRoughness;

                public GLTFMaterialEntry(string name, string alphaMode, GLTFMaterialPBRValues pbrMetallicRoughness)
                {
                    this.name = name;
                    this.alphaMode = alphaMode;
                    this.pbrMetallicRoughness = pbrMetallicRoughness;
                }

                public GLTFMaterialEntry(TextureConfig conf, int texOffset)
                {
                    this.name = "Material_" + texOffset + "_" + conf.id;
                    this.alphaMode = (conf.IgnoresTransparency() || conf.id == 0) ? GLTFMaterialEntry.OPAQUE : GLTFMaterialEntry.MASK;
                    this.pbrMetallicRoughness = new GLTFMaterialEntry.GLTFMaterialPBRValues(texOffset, conf.id);
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
                        public int? NORMAL = null;
                        public int? COLOR_n = null;
                        public int? WEIGHTS_0 = null;
                        public int? JOINTS_0 = null;

                        public GLTFMeshPrimitivesEntryAttributes(int POSITION, int TEXCOORD_0)
                        {
                            this.POSITION = POSITION;
                            this.TEXCOORD_0 = TEXCOORD_0;
                        }

                        public GLTFMeshPrimitivesEntryAttributes(int POSITION, int TEXCOORD_0, int NORMAL)
                        {
                            this.POSITION = POSITION;
                            this.TEXCOORD_0 = TEXCOORD_0;
                            this.NORMAL = NORMAL;
                        }

                        public GLTFMeshPrimitivesEntryAttributes(int POSITION, int TEXCOORD_0, int NORMAL, int COLOR_n)
                        {
                            this.POSITION = POSITION;
                            this.TEXCOORD_0 = TEXCOORD_0;
                            this.NORMAL = NORMAL;
                            this.COLOR_n = COLOR_n;
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
                public string name;
                public GLTFMeshPrimitivesEntry[] primitives;

                public GLTFMeshEntry(string name, GLTFMeshPrimitivesEntry[] primitives)
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
                public const string PNG = "image/png";
                public const string JPEG = "image/jpeg";

                public string mimeType;
                public string name;

                // Image is stored in a buffer
                public int? bufferView = null;

                // Image is stored in a file.
                public string? uri = null;

                public GLTFImageEntry(int bufferView, string mimeType, string name)
                {
                    this.bufferView = bufferView;
                    this.mimeType = mimeType;
                    this.name = name;
                }

                public GLTFImageEntry(string uri, string mimeType, string name)
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
                public const string SCALAR = "SCALAR";
                public const string VEC2 = "VEC2";
                public const string VEC3 = "VEC3";
                public const string VEC4 = "VEC4";
                public const string MAT2 = "MAT2";
                public const string MAT3 = "MAT3";
                public const string MAT4 = "MAT4";

                public string name;
                public int bufferView;
                public int componentType;
                public bool normalized;
                public int count;
                public int byteOffset;
                public float[]? max = null;
                public float[]? min = null;
                public string type;

                public GLTFAccessorEntry(string name, int bufferView, int componentType, bool normalized, int count, int byteOffset, Vector3 max, Vector3 min, string type)
                {
                    this.name = name;
                    this.bufferView = bufferView;
                    this.componentType = componentType;
                    this.normalized = normalized;
                    this.count = count;
                    this.byteOffset = byteOffset;
                    this.type = type;

                    this.max = new float[3] { max.X, max.Y, max.Z };
                    this.min = new float[3] { min.X, min.Y, min.Z };
                }

                public GLTFAccessorEntry(string name, int bufferView, int componentType, bool normalized, int count, int byteOffset, string type)
                {
                    this.name = name;
                    this.bufferView = bufferView;
                    this.componentType = componentType;
                    this.normalized = normalized;
                    this.count = count;
                    this.byteOffset = byteOffset;
                    this.type = type;
                }

                public GLTFAccessorEntry(string name, int bufferView, int componentType, bool normalized, int count, int byteOffset, float min, float max, string type)
                {
                    this.name = name;
                    this.bufferView = bufferView;
                    this.componentType = componentType;
                    this.normalized = normalized;
                    this.count = count;
                    this.byteOffset = byteOffset;
                    this.type = type;

                    this.max = new float[1] { max };
                    this.min = new float[1] { min };
                }
            }

            public class GLTFBufferViewEntry
            {
                // target
                public const int ARRAY_BUFFER = 34962;
                public const int ELEMENT_ARRAY_BUFFER = 34963;

                public string name;
                public int buffer;
                public int byteLength;
                public int byteOffset;
                public int? byteStride = null;
                public int? target = null;

                public GLTFBufferViewEntry(string name, int buffer, int byteLength, int byteOffset, int byteStride, int target)
                {
                    this.name = name;
                    this.buffer = buffer;
                    this.byteLength = byteLength;
                    this.byteOffset = byteOffset;
                    this.byteStride = byteStride;
                    this.target = target;
                }

                public GLTFBufferViewEntry(string name, int buffer, int byteLength, int byteOffset, int target)
                {
                    this.name = name;
                    this.buffer = buffer;
                    this.byteLength = byteLength;
                    this.byteOffset = byteOffset;
                    this.target = target;
                }

                public GLTFBufferViewEntry(string name, int buffer, int byteLength, int byteOffset)
                {
                    this.name = name;
                    this.buffer = buffer;
                    this.byteLength = byteLength;
                    this.byteOffset = byteOffset;
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

                public string name;
                public int magFilter;
                public int minFilter;
                public int wrapS;
                public int wrapT;

                public GLTFSamplerEntry(string name, int magFilter, int minFilter, int wrapS, int wrapT)
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
                public string name;
                public int byteLength;
                public string uri;

                public GLTFBufferEntry(string name, int byteLength, string uri)
                {
                    this.name = name;
                    this.byteLength = byteLength;
                    this.uri = uri;
                }

                public GLTFBufferEntry(string name, Array arr)
                {
                    this.name = name;

                    byte[] data = new byte[Buffer.ByteLength(arr)];
                    Buffer.BlockCopy(arr, 0, data, 0, Buffer.ByteLength(arr));

                    this.byteLength = data.Length;
                    this.uri = "data:application/octet-stream;base64," + Convert.ToBase64String(data, Base64FormattingOptions.None);
                }
            }

            public class GLTFSkinEntry
            {
                public string name;
                public int[] joints;
                public int? inverseBindMatrices = null;

                public GLTFSkinEntry(string name, int numBones, int inverseBindMatrices)
                {
                    this.name = name;
                    this.joints = new int[numBones];

                    for (int i = 0; i < numBones; i++)
                    {
                        this.joints[i] = i;
                    }

                    this.inverseBindMatrices = inverseBindMatrices;
                }
            }

            public class GLTFAnimationEntry
            {
                public class GLTFAnimationChannel
                {
                    public class GLTFAnimationChannelTarget
                    {
                        public const string TRANSLATION = "translation";
                        public const string ROTATION = "rotation";
                        public const string SCALE = "scale";
                        public const string WEIGHTS = "weights";

                        public int node;
                        public string path;

                        public GLTFAnimationChannelTarget(int node, string path)
                        {
                            this.node = node;
                            this.path = path;
                        }
                    }

                    public int sampler;
                    public GLTFAnimationChannelTarget target;

                    public GLTFAnimationChannel(int sampler, GLTFAnimationChannelTarget target)
                    {
                        this.sampler = sampler;
                        this.target = target;
                    }
                }

                public class GLTFAnimationSampler
                {
                    public const string LINEAR = "LINEAR";
                    public const string STEP = "STEP";
                    public const string CUBICSPLINE = "CUBICSPLINE";

                    public int input;
                    public int output;
                    public string? interpolation = null;

                    public GLTFAnimationSampler(int input, int output, string interpolation = LINEAR)
                    {
                        this.input = input;
                        this.output = output;
                        this.interpolation = interpolation;
                    }
                }

                public string name;
                public GLTFAnimationChannel[] channels;
                public GLTFAnimationSampler[] samplers;

                public GLTFAnimationEntry(string name, int boneCount, int keyframeAccessor, int outputAccessorOffset)
                {
                    this.name = name;

                    this.samplers = new GLTFAnimationSampler[boneCount * 3];
                    for (int i = 0; i < boneCount; i++)
                    {
                        this.samplers[3 * i + 0] = new GLTFAnimationSampler(keyframeAccessor, outputAccessorOffset + 3 * i + 0);
                        this.samplers[3 * i + 1] = new GLTFAnimationSampler(keyframeAccessor, outputAccessorOffset + 3 * i + 1);
                        this.samplers[3 * i + 2] = new GLTFAnimationSampler(keyframeAccessor, outputAccessorOffset + 3 * i + 2);
                    }

                    this.channels = new GLTFAnimationChannel[boneCount * 3];
                    for (int i = 0; i < boneCount; i++)
                    {
                        this.channels[i * 3 + 0] = new GLTFAnimationChannel(i * 3 + 0, new GLTFAnimationChannel.GLTFAnimationChannelTarget(i, GLTFAnimationChannel.GLTFAnimationChannelTarget.TRANSLATION));
                        this.channels[i * 3 + 1] = new GLTFAnimationChannel(i * 3 + 1, new GLTFAnimationChannel.GLTFAnimationChannelTarget(i, GLTFAnimationChannel.GLTFAnimationChannelTarget.ROTATION));
                        this.channels[i * 3 + 2] = new GLTFAnimationChannel(i * 3 + 2, new GLTFAnimationChannel.GLTFAnimationChannelTarget(i, GLTFAnimationChannel.GLTFAnimationChannelTarget.SCALE));
                    }
                }
            }

            public GLTFAssetProperty asset = new GLTFAssetProperty();
            public string[]? extensionsUsed = null;
            public int scene = 0;
            public GLTFNodesEntry[] nodes = new GLTFNodesEntry[] { };
            public GLTFScenesEntry[] scenes = new GLTFScenesEntry[] { };
            public GLTFMaterialEntry[] materials = new GLTFMaterialEntry[] { };
            public GLTFMeshEntry[] meshes = new GLTFMeshEntry[] { };
            public GLTFTextureEntry[] textures = new GLTFTextureEntry[] { };
            public GLTFImageEntry[] images = new GLTFImageEntry[] { };
            public GLTFAccessorEntry[] accessors = new GLTFAccessorEntry[] { };
            public GLTFBufferViewEntry[] bufferViews = new GLTFBufferViewEntry[] { };
            public GLTFSamplerEntry[] samplers = new GLTFSamplerEntry[] { };
            public GLTFSkinEntry[]? skins = null;
            public GLTFAnimationEntry[]? animations = null;
            public GLTFBufferEntry[] buffers = new GLTFBufferEntry[] { };

            public GLTFDataObject(Level level, Model model, bool includeSkeleton, ExporterModelSettings settings, List<Texture>? textures)
            {
                // skybox model has no normals and thus the vertex buffer has a different layout
                // if we see other cases like this, it may be advisable to generalize this
                bool skyboxModel = (model is SkyboxModel);

                bool hasNormals = !(skyboxModel);
                bool hasVertexColors = (skyboxModel) || (model is TerrainModel);

                bool exportAnimations = (settings.animationChoice != ExporterModelSettings.AnimationChoice.None && includeSkeleton);

                // Animations may have no frames but GLTF does not allow that.
                List<Animation> animations = new List<Animation>();

                if (exportAnimations)
                {
                    MobyModel mobModel = (MobyModel) model;

                    if (model.id == 0)
                    {
                        foreach (Animation anim in level.playerAnimations)
                        {
                            if (anim.frames.Count > 0)
                            {
                                animations.Add(anim);
                            }
                        }
                    }
                    else
                    {
                        foreach (Animation anim in mobModel.animations)
                        {
                            if (anim.frames.Count > 0)
                            {
                                animations.Add(anim);
                            }
                        }
                    }

                    if (animations.Count == 0) exportAnimations = false;
                }

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
                        float r = ((float) colors[0]) / 255.0f;
                        float g = ((float) colors[1]) / 255.0f;
                        float b = ((float) colors[2]) / 255.0f;
                        float a = ((float) colors[3]) / 255.0f;
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

                // Change orientation, glTF enforces an orientation of Y Up
                ChangeOrientation(ref vertices, ExporterModelSettings.Orientation.Y_UP);
                if (hasNormals)
                {
                    ChangeOrientation(ref normals, ExporterModelSettings.Orientation.Y_UP);
                }

                // Construct Vertex Buffer for glTF export
                // Offsets in number of floats
                int gltfVertexBufferStride = 3 + 2;
                int gltfVertexPosOffset = 0;
                int gltfVertexUVOffset = 3;
                int gltfVertexNormalOffset = 0;
                int gltfVertexColorOffset = 0;
                int gltfVertexWeightsOffset = 0;
                int gltfVertexIDsOffset = 0;
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
                if (includeSkeleton)
                {
                    gltfVertexWeightsOffset = gltfVertexBufferStride;
                    gltfVertexBufferStride += 1;

                    gltfVertexIDsOffset = gltfVertexBufferStride;
                    gltfVertexBufferStride += 1;
                }

                int gltfVertexBufferByteStride = gltfVertexBufferStride * 4;
                int gltfVertexPosByteOffset = gltfVertexPosOffset * 4;
                int gltfVertexUVByteOffset = gltfVertexUVOffset * 4;
                int gltfVertexNormalByteOffset = gltfVertexNormalOffset * 4;
                int gltfVertexColorByteOffset = gltfVertexColorOffset * 4;
                int gltfVertexWeightsByteOffset = gltfVertexWeightsOffset * 4;
                int gltfVertexIDsByteOffset = gltfVertexIDsOffset * 4;

                float[] gltfVertexBuffer = new float[gltfVertexBufferStride * model.vertexCount];
                Vector3 vertexMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Vector3 vertexMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

                for (int i = 0; i < model.vertexCount; i++)
                {
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexPosOffset + 0x0] = vertices[i].X;
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexPosOffset + 0x1] = vertices[i].Y;
                    gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexPosOffset + 0x2] = vertices[i].Z;

                    vertexMax = Vector3.ComponentMax(vertexMax, vertices[i]);
                    vertexMin = Vector3.ComponentMin(vertexMin, vertices[i]);
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

                float[] gltfInvBindMatrixBuffer = new float[0];
                int boneCount = 0;

                if (includeSkeleton)
                {
                    MobyModel mobModel = (MobyModel) model;

                    boneCount = mobModel.boneCount;

                    for (int i = 0; i < model.vertexCount; i++)
                    {
                        // GLTF strictly requires the weights to sum to 255. However, the values in the game will sum to
                        // 255 or 256, hence we need to fix this.
                        // The issue is that it is not straight forward to normalize this using bytes.
                        // Maybe just using floats is the best solution here.
                        float weightFloat = BitConverter.UInt32BitsToSingle(mobModel.weights[i]);
                        byte[] weights = BitConverter.GetBytes(weightFloat);

                        int sum = 0;
                        for (int j = 0; j < 4; j++)
                        {
                            sum += weights[j];
                        }

                        while (sum > 255)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                if (weights[j] > 0)
                                {
                                    weights[j]--;
                                    sum--;
                                    break;
                                }
                            }
                        }

                        while (sum < 255)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                if (weights[j] > 0 && weights[j] < 255)
                                {
                                    weights[j]++;
                                    sum++;
                                    break;
                                }
                            }
                        }

                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexWeightsOffset] = BitConverter.ToSingle(weights, 0);
                        gltfVertexBuffer[(i * gltfVertexBufferStride) + gltfVertexIDsOffset] = BitConverter.UInt32BitsToSingle(mobModel.ids[i]);
                    }

                    if (mobModel.skeleton != null)
                    {
                        Skeleton[] skeletonSorted = new Skeleton[boneCount];
                        Stack<Skeleton> skeletonStack = new Stack<Skeleton>();
                        skeletonStack.Push(mobModel.skeleton);

                        while (skeletonStack.TryPop(out Skeleton? skel))
                        {
                            skeletonSorted[skel.bone.id] = skel;

                            foreach (Skeleton child in skel.children)
                            {
                                skeletonStack.Push(child);
                            }
                        }

                        List<float> listInvBindMatrix = new List<float>();
                        foreach (Skeleton s in skeletonSorted)
                        {
                            Matrix4 invBindMatrix = s.bone.GetInvBindMatrix();

                            invBindMatrix.M14 *= model.size;
                            invBindMatrix.M24 *= model.size;
                            invBindMatrix.M34 *= model.size;

                            ChangeOrientation(ref invBindMatrix, ExporterModelSettings.Orientation.Y_UP);

                            listInvBindMatrix.Add(invBindMatrix.M11);
                            listInvBindMatrix.Add(invBindMatrix.M21);
                            listInvBindMatrix.Add(invBindMatrix.M31);
                            listInvBindMatrix.Add(invBindMatrix.M41);
                            listInvBindMatrix.Add(invBindMatrix.M12);
                            listInvBindMatrix.Add(invBindMatrix.M22);
                            listInvBindMatrix.Add(invBindMatrix.M32);
                            listInvBindMatrix.Add(invBindMatrix.M42);
                            listInvBindMatrix.Add(invBindMatrix.M13);
                            listInvBindMatrix.Add(invBindMatrix.M23);
                            listInvBindMatrix.Add(invBindMatrix.M33);
                            listInvBindMatrix.Add(invBindMatrix.M43);
                            listInvBindMatrix.Add(invBindMatrix.M14);
                            listInvBindMatrix.Add(invBindMatrix.M24);
                            listInvBindMatrix.Add(invBindMatrix.M34);
                            listInvBindMatrix.Add(invBindMatrix.M44);
                        }
                        gltfInvBindMatrixBuffer = listInvBindMatrix.ToArray();
                    }
                }



                float[] gltfKeyframeBuffer = new float[0];
                float[] gltfAnimOutputBuffer = new float[0];
                int[] keyframeOffset = new int[0];
                int[] animTranslationOffset = new int[0];
                int[] animRotationOffset = new int[0];
                int[] animScaleOffset = new int[0];
                int animTranslationSize = 0;
                int animRotationSize = 0;
                int animScaleSize = 0;
                int animTranslationBaseOffset = 0;
                int animRotationBaseOffset = 0;
                int animScaleBaseOffset = 0;
                float[] animLength = new float[0];

                if (exportAnimations)
                {
                    MobyModel mobModel = (MobyModel) model;

                    int frameCount = 0;

                    foreach (Animation anim in animations)
                    {
                        frameCount += anim.frames.Count;
                    }

                    animTranslationSize = frameCount * 3 * boneCount;
                    animRotationSize = frameCount * 4 * boneCount;
                    animScaleSize = frameCount * 3 * boneCount;

                    animTranslationBaseOffset = 0;
                    animRotationBaseOffset = animTranslationBaseOffset + animTranslationSize;
                    animScaleBaseOffset = animRotationBaseOffset + animRotationSize;

                    gltfKeyframeBuffer = new float[frameCount];
                    gltfAnimOutputBuffer = new float[animTranslationSize + animRotationSize + animScaleSize];

                    keyframeOffset = new int[animations.Count];
                    animTranslationOffset = new int[animations.Count];
                    animRotationOffset = new int[animations.Count];
                    animScaleOffset = new int[animations.Count];

                    animLength = new float[animations.Count];

                    int currKeyframeOffset = 0;
                    int currTranslationOffset = 0;
                    int currRotationOffset = 0;
                    int currScaleOffset = 0;

                    for (int k = 0; k < animations.Count; k++)
                    {
                        Animation anim = animations[k];

                        keyframeOffset[k] = currKeyframeOffset;
                        animTranslationOffset[k] = currTranslationOffset;
                        animRotationOffset[k] = currRotationOffset;
                        animScaleOffset[k] = currScaleOffset;

                        float keyframeValue = 0.0f;

                        for (int i = 0; i < anim.frames.Count; i++)
                        {
                            gltfKeyframeBuffer[currKeyframeOffset++] = keyframeValue;
                            animLength[k] = keyframeValue;
                            float speed = (anim.speed != 0.0f) ? anim.speed : anim.frames[i].speed;
                            keyframeValue += 1.0f / (speed * 60.0f);
                        }

                        for (int j = 0; j < boneCount; j++)
                        {
                            for (int i = 0; i < anim.frames.Count; i++)
                            {
                                Frame frame = anim.frames[i];

                                Vector3? translation = frame.GetTranslation(j);
                                Quaternion? rotation = frame.GetRotationQuaternion(j);
                                Vector3? scaling = frame.GetScaling(j);

                                Vector3 t = (translation != null) ? (Vector3) translation : mobModel.boneDatas[j].translation;
                                t *= model.size;
                                ChangeOrientation(ref t, ExporterModelSettings.Orientation.Y_UP);

                                gltfAnimOutputBuffer[animTranslationBaseOffset + currTranslationOffset + 0] = t.X;
                                gltfAnimOutputBuffer[animTranslationBaseOffset + currTranslationOffset + 1] = t.Y;
                                gltfAnimOutputBuffer[animTranslationBaseOffset + currTranslationOffset + 2] = t.Z;

                                Quaternion q = (rotation != null) ? (Quaternion) rotation : Quaternion.Identity;
                                ChangeOrientation(ref q, ExporterModelSettings.Orientation.Y_UP);

                                gltfAnimOutputBuffer[animRotationBaseOffset + currRotationOffset + 0] = q.X;
                                gltfAnimOutputBuffer[animRotationBaseOffset + currRotationOffset + 1] = q.Y;
                                gltfAnimOutputBuffer[animRotationBaseOffset + currRotationOffset + 2] = q.Z;
                                gltfAnimOutputBuffer[animRotationBaseOffset + currRotationOffset + 3] = q.W;

                                Vector3 s = Vector3.One;
                                if (scaling != null)
                                {
                                    s = (Vector3) scaling;
                                    float temp = s.Y;
                                    s.Y = s.Z;
                                    s.Z = temp;
                                }

                                gltfAnimOutputBuffer[animScaleBaseOffset + currScaleOffset + 0] = s.X;
                                gltfAnimOutputBuffer[animScaleBaseOffset + currScaleOffset + 1] = s.Y;
                                gltfAnimOutputBuffer[animScaleBaseOffset + currScaleOffset + 2] = s.Z;

                                currTranslationOffset += 3;
                                currRotationOffset += 4;
                                currScaleOffset += 3;
                            }
                        }
                    }
                }

                Dictionary<int, byte[]> textureDataBuffers = new Dictionary<int, byte[]>();
                Dictionary<int, int> texIDToImageBuffer = new Dictionary<int, int>();

                if (settings.embedTextures && textures != null)
                {
                    for (int i = 0; i < model.textureConfig.Count; i++)
                    {
                        TextureConfig conf = model.textureConfig[i];
                        if (!texIDToImageBuffer.ContainsKey(conf.id))
                        {
                            Texture? tex = textures.Find(t => t.id == conf.id);

                            if (tex != null)
                            {
                                Image? image = tex.GetTextureImage(!conf.IgnoresTransparency());

                                if (image != null)
                                {
                                    using (MemoryStream stream = new MemoryStream())
                                    {
                                        image.SaveAsPng(stream);
                                        texIDToImageBuffer.Add(conf.id, textureDataBuffers.Count);
                                        textureDataBuffers.Add(conf.id, stream.ToArray());
                                    }
                                }
                            }

                        }
                    }
                }

                ////
                //  Nodes
                ////

                List<GLTFNodesEntry> listNodes = new List<GLTFNodesEntry>();

                int? skin = null;

                if (includeSkeleton)
                {
                    MobyModel mobModel = (MobyModel) model;

                    if (mobModel.skeleton != null)
                    {
                        Skeleton[] skeletonSorted = new Skeleton[mobModel.boneCount];
                        Stack<Skeleton> skeletonStack = new Stack<Skeleton>();
                        skeletonStack.Push(mobModel.skeleton);

                        while (skeletonStack.TryPop(out Skeleton? skel))
                        {
                            skeletonSorted[skel.bone.id] = skel;

                            foreach (Skeleton child in skel.children)
                            {
                                skeletonStack.Push(child);
                            }
                        }

                        foreach (Skeleton s in skeletonSorted)
                        {
                            listNodes.Add(new GLTFNodesEntry(s, mobModel.size));
                        }


                        skin = 0;
                    }
                }
                listNodes.Add(new GLTFNodesEntry("modelObject", 0, skin));

                if (includeSkeleton)
                {
                    listNodes.Add(new GLTFNodesEntry("Armature", new int[] { 0, listNodes.Count - 1 }));
                }

                this.nodes = listNodes.ToArray();

                ////
                //  Buffers
                ////

                List<GLTFBufferEntry> listBuffers = new List<GLTFBufferEntry>();
                listBuffers.Add(new GLTFBufferEntry("VertexBuffer", gltfVertexBuffer));
                listBuffers.Add(new GLTFBufferEntry("IndexBuffer", gltfIndexBuffer));
                if (includeSkeleton)
                {
                    listBuffers.Add(new GLTFBufferEntry("InvBindMatrixBuffer", gltfInvBindMatrixBuffer));
                }
                if (exportAnimations)
                {
                    listBuffers.Add(new GLTFBufferEntry("KeyframeBuffer", gltfKeyframeBuffer));
                    listBuffers.Add(new GLTFBufferEntry("AnimOutputBuffer", gltfAnimOutputBuffer));
                }
                Dictionary<int, int> textureDataBufferIDs = new Dictionary<int, int>();
                foreach (var texBuffer in textureDataBuffers)
                {
                    textureDataBufferIDs.Add(texBuffer.Key, listBuffers.Count);
                    listBuffers.Add(new GLTFBufferEntry("TextureBuffer" + texBuffer.Key, texBuffer.Value));
                }
                this.buffers = listBuffers.ToArray();

                ////
                //  BufferViews
                ////

                List<GLTFBufferViewEntry> listBufferViews = new List<GLTFBufferViewEntry>();
                listBufferViews.Add(new GLTFBufferViewEntry("VertexBufferView", 0, gltfVertexBufferByteStride * model.vertexCount, 0, gltfVertexBufferByteStride, GLTFBufferViewEntry.ARRAY_BUFFER));
                if (includeSkeleton)
                {
                    listBufferViews.Add(new GLTFBufferViewEntry("InvBindMatrixBufferView", 2, gltfInvBindMatrixBuffer.Length * sizeof(float), 0));
                }
                if (exportAnimations)
                {
                    listBufferViews.Add(new GLTFBufferViewEntry("KeyframeBufferView", 3, gltfKeyframeBuffer.Length * sizeof(float), 0));
                    listBufferViews.Add(new GLTFBufferViewEntry("TranslationBufferView", 4, animTranslationSize * sizeof(float), animTranslationBaseOffset * sizeof(float)));
                    listBufferViews.Add(new GLTFBufferViewEntry("RotationBufferView", 4, animRotationSize * sizeof(float), animRotationBaseOffset * sizeof(float)));
                    listBufferViews.Add(new GLTFBufferViewEntry("ScaleBufferView", 4, animScaleSize * sizeof(float), animScaleBaseOffset * sizeof(float)));
                }
                int indexBufferViewBaseID = listBufferViews.Count;
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    int byteOffset = sizeof(ushort) * conf.start;
                    int byteLength = sizeof(ushort) * conf.size;

                    listBufferViews.Add(new GLTFBufferViewEntry("IndexBufferView" + i, 1, byteLength, byteOffset, GLTFBufferViewEntry.ELEMENT_ARRAY_BUFFER));
                }
                Dictionary<int, int> textureDataBufferViewIDs = new Dictionary<int, int>();
                foreach (var texBuffer in textureDataBuffers)
                {
                    textureDataBufferViewIDs.Add(texBuffer.Key, listBufferViews.Count);
                    listBufferViews.Add(new GLTFBufferViewEntry("TextureBufferView" + texBuffer.Key, textureDataBufferIDs[texBuffer.Key], texBuffer.Value.Length, 0));
                }
                this.bufferViews = listBufferViews.ToArray();

                ////
                //  Accessors
                ////

                int normalAccessorID = 0;
                int vertexColorAccessorID = 0;
                int vertexWeightsAccessorID = 0;
                int vertexIDsAccessorID = 0;
                int invBindMatrixAccessorID = 0;
                int animationAccessorBaseID = 0;

                List<GLTFAccessorEntry> listAccessors = new List<GLTFAccessorEntry>();
                listAccessors.Add(new GLTFAccessorEntry("VertexPosAccessor", 0, GLTFAccessorEntry.FLOAT, false, model.vertexCount, gltfVertexPosByteOffset, vertexMax, vertexMin, GLTFAccessorEntry.VEC3));
                listAccessors.Add(new GLTFAccessorEntry("VertexUVAccessor", 0, GLTFAccessorEntry.FLOAT, false, model.vertexCount, gltfVertexUVByteOffset, GLTFAccessorEntry.VEC2));
                if (hasNormals)
                {
                    normalAccessorID = listAccessors.Count;
                    listAccessors.Add(new GLTFAccessorEntry("VertexNormalAccessor", 0, GLTFAccessorEntry.FLOAT, false, model.vertexCount, gltfVertexNormalByteOffset, GLTFAccessorEntry.VEC3));
                }
                if (hasVertexColors)
                {
                    vertexColorAccessorID = listAccessors.Count;
                    listAccessors.Add(new GLTFAccessorEntry("VertexColorAccessor", 0, GLTFAccessorEntry.UNSIGNED_BYTE, true, model.vertexCount, gltfVertexColorByteOffset, GLTFAccessorEntry.VEC4));
                }
                if (includeSkeleton)
                {
                    vertexWeightsAccessorID = listAccessors.Count;
                    listAccessors.Add(new GLTFAccessorEntry("VertexWeightsAccessor", 0, GLTFAccessorEntry.UNSIGNED_BYTE, true, model.vertexCount, gltfVertexWeightsByteOffset, GLTFAccessorEntry.VEC4));
                    vertexIDsAccessorID = listAccessors.Count;
                    listAccessors.Add(new GLTFAccessorEntry("VertexIDsAccessor", 0, GLTFAccessorEntry.UNSIGNED_BYTE, false, model.vertexCount, gltfVertexIDsByteOffset, GLTFAccessorEntry.VEC4));
                    invBindMatrixAccessorID = listAccessors.Count;
                    listAccessors.Add(new GLTFAccessorEntry("InvBindMatrixAccessor", 1, GLTFAccessorEntry.FLOAT, false, gltfInvBindMatrixBuffer.Length / 16, 0, GLTFAccessorEntry.MAT4));
                }

                int indexAccessorBaseID = listAccessors.Count;

                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listAccessors.Add(new GLTFAccessorEntry("IndexAccessor" + i, indexBufferViewBaseID + i, GLTFAccessorEntry.UNSIGNED_SHORT, false, model.textureConfig[i].size, 0, GLTFAccessorEntry.SCALAR));
                }

                if (exportAnimations)
                {
                    animationAccessorBaseID = listAccessors.Count;

                    for (int i = 0; i < animations.Count; i++)
                    {
                        Animation anim = animations[i];

                        listAccessors.Add(new GLTFAccessorEntry("KeyframeAccessor" + i, 2, GLTFAccessorEntry.FLOAT, false, anim.frames.Count, keyframeOffset[i] * sizeof(float), 0.0f, animLength[i], GLTFAccessorEntry.SCALAR));
                    }

                    int animOutputStride = boneCount * 10;

                    for (int i = 0; i < animations.Count; i++)
                    {
                        Animation anim = animations[i];

                        int currTranslationOffsetBytes = animTranslationOffset[i] * sizeof(float);
                        int currRotationOffsetBytes = animRotationOffset[i] * sizeof(float);
                        int currScaleOffsetBytes = animScaleOffset[i] * sizeof(float);

                        for (int j = 0; j < boneCount; j++)
                        {
                            listAccessors.Add(new GLTFAccessorEntry("TranslationAccessor" + i + "Bone" + j, 3, GLTFAccessorEntry.FLOAT, false, anim.frames.Count, currTranslationOffsetBytes, GLTFAccessorEntry.VEC3));
                            listAccessors.Add(new GLTFAccessorEntry("RotationAccessor" + i + "Bone" + j, 4, GLTFAccessorEntry.FLOAT, false, anim.frames.Count, currRotationOffsetBytes, GLTFAccessorEntry.VEC4));
                            listAccessors.Add(new GLTFAccessorEntry("ScaleAccessor" + i + "Bone" + j, 5, GLTFAccessorEntry.FLOAT, false, anim.frames.Count, currScaleOffsetBytes, GLTFAccessorEntry.VEC3));

                            currTranslationOffsetBytes += anim.frames.Count * 3 * sizeof(float);
                            currRotationOffsetBytes += anim.frames.Count * 4 * sizeof(float);
                            currScaleOffsetBytes += anim.frames.Count * 3 * sizeof(float);
                        }
                    }
                }

                this.accessors = listAccessors.ToArray();

                ////
                //  Animations
                ////

                if (exportAnimations)
                {
                    List<GLTFAnimationEntry> listAnimationEntries = new List<GLTFAnimationEntry>();

                    int outputAccessorOffset = animationAccessorBaseID + animations.Count;

                    for (int i = 0; i < animations.Count; i++)
                    {
                        listAnimationEntries.Add(new GLTFAnimationEntry("Animation" + i, boneCount, animationAccessorBaseID + i, outputAccessorOffset));

                        outputAccessorOffset += boneCount * 3;
                    }

                    this.animations = listAnimationEntries.ToArray();
                }

                ////
                //  Skins
                ////

                if (includeSkeleton)
                {
                    MobyModel mobModel = (MobyModel) model;

                    List<GLTFSkinEntry> listSkins = new List<GLTFSkinEntry>();
                    listSkins.Add(new GLTFSkinEntry("Armature", mobModel.boneCount, invBindMatrixAccessorID));
                    this.skins = listSkins.ToArray();
                }

                ////
                //  Images
                ////

                Dictionary<int, int> texIDToImageOffset = new Dictionary<int, int>();

                List<GLTFImageEntry> listImages = new List<GLTFImageEntry>();
                if (settings.embedTextures && textures != null)
                {
                    for (int i = 0; i < model.textureConfig.Count; i++)
                    {
                        TextureConfig conf = model.textureConfig[i];
                        if (!texIDToImageOffset.ContainsKey(conf.id) && textureDataBufferViewIDs.ContainsKey(conf.id))
                        {
                            texIDToImageOffset.Add(conf.id, listImages.Count);
                            listImages.Add(new GLTFImageEntry(textureDataBufferViewIDs[conf.id], GLTFImageEntry.PNG, "Image" + conf.id));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < model.textureConfig.Count; i++)
                    {
                        TextureConfig conf = model.textureConfig[i];
                        if (!texIDToImageOffset.ContainsKey(conf.id))
                        {
                            texIDToImageOffset.Add(conf.id, listImages.Count);
                            listImages.Add(new GLTFImageEntry(conf));
                        }
                    }
                }

                this.images = listImages.ToArray();

                ////
                //  TextureSamplers
                ////

                List<GLTFSamplerEntry> listSamplers = new List<GLTFSamplerEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listSamplers.Add(new GLTFSamplerEntry(model.textureConfig[i]));
                }
                this.samplers = listSamplers.ToArray();

                ////
                //  Textures
                ////

                List<GLTFTextureEntry> listTextures = new List<GLTFTextureEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    TextureConfig conf = model.textureConfig[i];
                    listTextures.Add(new GLTFTextureEntry(i, texIDToImageOffset[conf.id]));
                }
                this.textures = listTextures.ToArray();

                ////
                //  Materials
                ////

                List<GLTFMaterialEntry> listMaterials = new List<GLTFMaterialEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listMaterials.Add(new GLTFMaterialEntry(model.textureConfig[i], i));
                }
                this.materials = listMaterials.ToArray();

                GLTFMeshEntry.GLTFMeshPrimitivesEntry.GLTFMeshPrimitivesEntryAttributes vertexAttribs = new GLTFMeshEntry.GLTFMeshPrimitivesEntry.GLTFMeshPrimitivesEntryAttributes(0, 1);
                if (hasNormals)
                {
                    vertexAttribs.NORMAL = normalAccessorID;
                }
                if (hasVertexColors)
                {
                    vertexAttribs.COLOR_n = vertexColorAccessorID;
                }
                if (includeSkeleton)
                {
                    vertexAttribs.WEIGHTS_0 = vertexWeightsAccessorID;
                    vertexAttribs.JOINTS_0 = vertexIDsAccessorID;
                }

                ////
                //  MeshPrimitives
                ////

                List<GLTFMeshEntry.GLTFMeshPrimitivesEntry> listMeshPrimitives = new List<GLTFMeshEntry.GLTFMeshPrimitivesEntry>();
                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    listMeshPrimitives.Add(new GLTFMeshEntry.GLTFMeshPrimitivesEntry(vertexAttribs, indexAccessorBaseID + i, i));
                }

                ////
                //  Meshes
                ////

                List<GLTFMeshEntry> listMeshes = new List<GLTFMeshEntry>();
                listMeshes.Add(new GLTFMeshEntry("Mesh", listMeshPrimitives.ToArray()));
                this.meshes = listMeshes.ToArray();

                ////
                //  Scenes
                ////

                List<GLTFScenesEntry> listScenes = new List<GLTFScenesEntry>();
                listScenes.Add(new GLTFScenesEntry("ModelObject", new int[] { listNodes.Count - 1 }));
                this.scenes = listScenes.ToArray();
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

        public override void ExportModel(string fileName, Level level, Model model, List<Texture>? textures = null)
        {
            LOGGER.Trace(fileName);

            bool includeSkeleton = (model is MobyModel mobyModel && mobyModel.boneCount != 0);

            StreamWriter stream = new StreamWriter(fileName);

            try
            {
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                jsonOptions.WriteIndented = true;
                jsonOptions.IncludeFields = true;
                jsonOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                GLTFDataObject gltfDataObject = new GLTFDataObject(level, model, includeSkeleton, this.settings, textures);
                string gltfJson = JsonSerializer.Serialize(gltfDataObject, jsonOptions);
                stream.Write(gltfJson);
            }
            finally
            {
                stream.Close();
            }
        }
    }
}
