// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using LibReplanetizer.Models;
using System.Drawing;

namespace Replanetizer.Utils
{
    /*
     * A container to store IBO and VBO references for a Model
     */
    public class RenderableBuffer : IDisposable
    {
        private ModelObject modelObject;

        private int loadedModelID = -1;

        private int ibo = 0;
        private int vbo = 0;

        private bool iboAllocated = false;
        private bool vboAllocated = false;

        public int id { get; }
        public RenderedObjectType type { get; }
        public int light { get; set; }
        public Color ambient { get; set; }
        public float renderDistance { get; set; }

        private bool selected;
        private bool culled;
        private float blendDistance = 0.0f;

        private Level level;
        private Dictionary<Texture, int> textureIds;

        public static ShaderIDTable? SHADER_ID_TABLE { get; set; }

        public RenderableBuffer(ModelObject modelObject, RenderedObjectType type, int id, Level level, Dictionary<Texture, int> textureIds)
        {
            this.modelObject = modelObject;
            this.id = id;
            this.type = type;
            this.textureIds = textureIds;
            this.level = level;

            GenerateBuffers();
            UpdateVars();
        }

        /// <summary>
        /// Deletes IBO and VBO if they are allocated.
        /// </summary>
        private void DeleteBuffers()
        {
            if (iboAllocated)
            {
                GL.DeleteBuffer(ibo);
            }

            if (vboAllocated)
            {
                GL.DeleteBuffer(vbo);
            }
        }

        /// <summary>
        /// Generates IBO and VBO.
        /// </summary>
        private void GenerateBuffers()
        {
            DeleteBuffers();
            loadedModelID = modelObject.modelID;

            BufferUsageHint hint = BufferUsageHint.StaticDraw;
            if (modelObject.IsDynamic())
            {
                hint = BufferUsageHint.DynamicDraw;
            }

            // IBO
            int iboLength = modelObject.GetIndices().Length * sizeof(ushort);
            if (iboLength > 0)
            {
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboLength * sizeof(ushort), IntPtr.Zero, hint);
                iboAllocated = true;
            }

            // VBO
            int vboLength = modelObject.GetVertices().Length * sizeof(float);
            switch (type)
            {
                case RenderedObjectType.Terrain:
                    TerrainModel? terrainModel = (TerrainModel?) modelObject.model;
                    if (terrainModel == null) break;
                    vboLength += modelObject.GetAmbientRgbas().Length * sizeof(Byte) + terrainModel.lights.Count * sizeof(int);
                    break;
                case RenderedObjectType.Tie:
                    vboLength += modelObject.GetAmbientRgbas().Length * sizeof(Byte);
                    break;
            }

            if (vboLength > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboLength, IntPtr.Zero, hint);
                vboAllocated = true;
            }

            UpdateBuffers();
        }

        /// <summary>
        /// Updates the buffers. This is not actually needed as long as mesh manipulations are not possible.
        /// </summary>
        public void UpdateBuffers()
        {
            if (BindIbo())
            {
                ushort[] iboData = modelObject.GetIndices();
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, iboData.Length * sizeof(ushort), iboData);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }

            if (BindVbo())
            {
                float[] vboData = modelObject.GetVertices();
                switch (type)
                {
                    case RenderedObjectType.Terrain:
                        {
                            byte[] rgbas = modelObject.GetAmbientRgbas();
                            TerrainModel? terrainModel = (TerrainModel?) modelObject.model;
                            if (terrainModel == null) break;
                            int[] lights = terrainModel.lights.ToArray();
                            float[] fullData = new float[vboData.Length + rgbas.Length / 4 + lights.Length];
                            for (int i = 0; i < vboData.Length / 8; i++)
                            {
                                fullData[10 * i + 0] = vboData[8 * i + 0];
                                fullData[10 * i + 1] = vboData[8 * i + 1];
                                fullData[10 * i + 2] = vboData[8 * i + 2];
                                fullData[10 * i + 3] = vboData[8 * i + 3];
                                fullData[10 * i + 4] = vboData[8 * i + 4];
                                fullData[10 * i + 5] = vboData[8 * i + 5];
                                fullData[10 * i + 6] = vboData[8 * i + 6];
                                fullData[10 * i + 7] = vboData[8 * i + 7];
                                fullData[10 * i + 8] = BitConverter.ToSingle(rgbas, i * 4);
                                fullData[10 * i + 9] = (float) lights[i];
                            }
                            vboData = fullData;
                            break;
                        }
                    case RenderedObjectType.Tie:
                        {
                            byte[] rgbas = modelObject.GetAmbientRgbas();
                            float[] fullData = new float[vboData.Length + rgbas.Length / 4];
                            for (int i = 0; i < vboData.Length / 8; i++)
                            {
                                fullData[9 * i + 0] = vboData[8 * i + 0];
                                fullData[9 * i + 1] = vboData[8 * i + 1];
                                fullData[9 * i + 2] = vboData[8 * i + 2];
                                fullData[9 * i + 3] = vboData[8 * i + 3];
                                fullData[9 * i + 4] = vboData[8 * i + 4];
                                fullData[9 * i + 5] = vboData[8 * i + 5];
                                fullData[9 * i + 6] = vboData[8 * i + 6];
                                fullData[9 * i + 7] = vboData[8 * i + 7];
                                fullData[9 * i + 8] = BitConverter.ToSingle(rgbas, i * 4);
                            }
                            vboData = fullData;
                            break;
                        }
                }
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vboData.Length * sizeof(float), vboData);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        /// <summary>
        /// Updates the light and ambient variables which can then be used to update the shader. Check if the modelID
        /// has changed and update the buffers if necessary.
        /// </summary>
        public void UpdateVars()
        {
            switch (type)
            {
                case RenderedObjectType.Terrain:
                    light = ShaderIDTable.ALLOCATED_LIGHTS;
                    renderDistance = float.MaxValue;
                    break;
                case RenderedObjectType.Moby:
                    Moby mob = (Moby) modelObject;
                    light = Math.Max(0, Math.Min(ShaderIDTable.ALLOCATED_LIGHTS, mob.light));
                    ambient = mob.color;
                    renderDistance = mob.drawDistance;
                    break;
                case RenderedObjectType.Tie:
                    Tie tie = (Tie) modelObject;
                    light = Math.Max(0, Math.Min(ShaderIDTable.ALLOCATED_LIGHTS, tie.light));
                    renderDistance = float.MaxValue;
                    break;
                case RenderedObjectType.Shrub:
                    Shrub shrub = (Shrub) modelObject;
                    light = Math.Max(0, Math.Min(ShaderIDTable.ALLOCATED_LIGHTS, shrub.light));
                    ambient = shrub.color;
                    renderDistance = shrub.drawDistance;
                    break;
            }

            if (loadedModelID != modelObject.modelID)
            {
                GenerateBuffers();
            }
        }

        /// <summary>
        /// Sets an internal variable to true if the corresponding modelObject is equal to
        /// the selectedObject in which case an outline will be rendered.
        /// </summary>
        public void Select(LevelObject selectedObject)
        {
            selected = modelObject == selectedObject;
        }

        /// <summary>
        /// Sets an internal variable to true if the corresponding modelObject is a member
        /// of selectedObjects in which case an outline will be rendered.
        /// </summary>
        public void Select(ICollection<LevelObject> selectedObjects)
        {
            selected = selectedObjects.Contains(modelObject);
        }

        /// <summary>
        /// Sets an internal variable to true if the object is to be culled.
        /// Mobies and shrubs are culled by their drawDistance.
        /// Ties, terrain and shrubs are culled by frustum culling.
        /// </summary>
        public void ComputeCulling(Camera camera, bool distanceCulling, bool frustumCulling)
        {
            if (distanceCulling)
            {
                float dist = (modelObject.position - camera.position).Length;

                blendDistance = MathF.Max(0.125f * (dist - renderDistance), 0.0f);

                if (dist > renderDistance + 8.0f)
                {
                    culled = true;
                    return;
                }
            }
            else
            {
                blendDistance = 0.0f;
            }

            if (frustumCulling)
            {
                if (type == RenderedObjectType.Terrain || type == RenderedObjectType.Tie || type == RenderedObjectType.Shrub)
                {
                    Vector3 center = Vector3.Zero;
                    float size = 0.0f;

                    switch (type)
                    {
                        case RenderedObjectType.Terrain:
                            TerrainFragment frag = (TerrainFragment) modelObject;
                            center = frag.cullingCenter;
                            size = frag.cullingSize;
                            break;
                        case RenderedObjectType.Shrub:
                            ShrubModel? shrub = (ShrubModel?) modelObject.model;
                            if (shrub == null) break;
                            center = new Vector3(shrub.cullingX, shrub.cullingY, shrub.cullingZ);
                            center = modelObject.rotation * center;
                            center += modelObject.position;
                            float shrubScale = MathF.MaxMagnitude(modelObject.scale.X, MathF.MaxMagnitude(modelObject.scale.Y, modelObject.scale.Z));
                            size = shrub.cullingRadius * shrubScale;
                            break;
                        case RenderedObjectType.Tie:
                            TieModel? tie = (TieModel?) modelObject.model;
                            if (tie == null) break;
                            center = new Vector3(tie.cullingX, tie.cullingY, tie.cullingZ);
                            center = modelObject.rotation * center;
                            center += modelObject.position;
                            float tieScale = MathF.MaxMagnitude(modelObject.scale.X, MathF.MaxMagnitude(modelObject.scale.Y, modelObject.scale.Z));
                            size = tie.cullingRadius * tieScale;
                            break;
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 planeNormal = camera.frustumPlaneNormals[i];
                        Vector3 planePoint = camera.frustumPlanePoints[i];

                        if (Vector3.Dot(center - planePoint, planeNormal) < -size)
                        {
                            culled = true;
                            return;
                        }
                    }
                }
            }

            culled = false;
        }

        /// <summary>
        /// Renders an object based on the buffers.
        /// </summary>
        public void Render()
        {
            if (SHADER_ID_TABLE == null) return;
            if (culled) return;
            if (!BindIbo() || !BindVbo()) return;
            if (modelObject.model == null) return;

            SetupVertexAttribPointers();

            switch (type)
            {
                case RenderedObjectType.Terrain:
                case RenderedObjectType.Shrub:
                case RenderedObjectType.Tie:
                case RenderedObjectType.Moby:
                    GL.UniformMatrix4(SHADER_ID_TABLE.uniformModelToWorldMatrix, false, ref modelObject.modelMatrix);
                    GL.Uniform1(SHADER_ID_TABLE.uniformLevelObjectNumber, id);
                    GL.Uniform4(SHADER_ID_TABLE.uniformAmbientColor, ambient);
                    GL.Uniform1(SHADER_ID_TABLE.uniformLightIndex, light);
                    GL.Uniform1(SHADER_ID_TABLE.uniformObjectBlendDistance, blendDistance);

                    //Bind textures one by one, applying it to the relevant vertices based on the index array
                    foreach (TextureConfig conf in modelObject.model.textureConfig)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, (conf.id > 0) ? textureIds[level.textures[conf.id]] : 0);
                        GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                    }

                    if (selected)
                    {
                        GL.UseProgram(SHADER_ID_TABLE.shaderColor);
                        GL.Uniform1(SHADER_ID_TABLE.uniformColorLevelObjectType, (int) type);
                        GL.Uniform1(SHADER_ID_TABLE.uniformColorLevelObjectNumber, id);
                        GL.Enable(EnableCap.LineSmooth);
                        GL.UniformMatrix4(SHADER_ID_TABLE.uniformColorModelToWorldMatrix, false, ref modelObject.modelMatrix);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        GL.DrawElements(PrimitiveType.Triangles, modelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        GL.UseProgram(SHADER_ID_TABLE.shaderMain);
                        GL.Disable(EnableCap.LineSmooth);
                    }
                    break;
            }

        }

        /// <summary>
        /// Attempts to bind the index buffer object. Returns true on success, false else.
        /// </summary>
        public bool BindIbo()
        {
            bool success = ibo != 0;
            if (success)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

            return success;
        }

        /// <summary>
        /// Attempts to bind the vertex buffer object. Returns true on success, false else.
        /// </summary>
        public bool BindVbo()
        {
            bool success = vbo != 0;
            if (success)
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            return success;
        }

        /// <summary>
        /// Sets up the vertex attribute pointers according to the type.
        /// </summary>
        private void SetupVertexAttribPointers()
        {
            switch (type)
            {
                case RenderedObjectType.Terrain:
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 10, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 10, sizeof(float) * 3);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 10, sizeof(float) * 6);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 10, sizeof(float) * 8);
                    GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, sizeof(float) * 10, sizeof(float) * 9);
                    break;
                case RenderedObjectType.Tie:
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 9, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 9, sizeof(float) * 3);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 9, sizeof(float) * 6);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 9, sizeof(float) * 8);
                    break;
                case RenderedObjectType.Shrub:
                case RenderedObjectType.Moby:
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
                    break;
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ibo);
            GL.DeleteBuffer(vbo);
        }
    }
}
