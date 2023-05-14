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
                public class GLTFMaterialExtensionEntry
                {
                    // Define Classes for Extensions here
                }

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
                }

                public bool doubleSided = true;
                public GLTFMaterialExtensionEntry[] extensions;
                public String name;
                public GLTFMaterialPBRValues pbrMetallicRoughness;

                public GLTFMaterialEntry(GLTFMaterialExtensionEntry[] extensions, String name, GLTFMaterialPBRValues pbrMetallicRoughness)
                {
                    this.extensions = extensions;
                    this.name = name;
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
                public int bufferView;
                public String mimeType;
                public String name;

                public GLTFImageEntry(int bufferView, String mimeType, String name)
                {
                    this.bufferView = bufferView;
                    this.mimeType = mimeType;
                    this.name = name;
                }
            }

            public class GLTFAccessorEntry
            {
                public int bufferView;
                public int componentType;
                public int count;
                public float[] max;
                public float[] min;
                public String type;

                public GLTFAccessorEntry(int bufferView, int componentType, int count, float[] max, float[] min, String type)
                {
                    this.bufferView = bufferView;
                    this.componentType = componentType;
                    this.count = count;
                    this.max = max;
                    this.min = min;
                    this.type = type;
                }
            }

            public class GLTFBufferViewEntry
            {
                public int buffer;
                public int byteLength;
                public int byteOffset;
                public int target;

                public GLTFBufferViewEntry(int buffer, int byteLength, int byteOffset, int target)
                {
                    this.buffer = buffer;
                    this.byteLength = byteLength;
                    this.byteOffset = byteOffset;
                    this.target = target;
                }
            }

            public class GLTFSamplerEntry
            {
                public int magFilter;
                public int minFilter;

                public GLTFSamplerEntry(int magFilter, int minFilter)
                {
                    this.magFilter = magFilter;
                    this.minFilter = minFilter;
                }
            }

            public class GLTFBufferEntry
            {
                public int byteLength;
                public String uri;

                public GLTFBufferEntry(int byteLength, String uri)
                {
                    this.byteLength = byteLength;
                    this.uri = uri;
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
