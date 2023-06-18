// Copyright (C) 2018-2023, The Replanetizer Contributors.
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
using Replanetizer.Utils;
using SixLabors.ImageSharp.PixelFormats;
using LibReplanetizer.Models.Animations;

namespace Replanetizer.Renderer
{

    /*
     * A container to store IBO and VBO references for a Model
     */
    public class MeshRenderer : Renderer
    {

        private static readonly int ALLOCATED_LIGHTS = 20;

        private ModelObject? modelObject;
        private Model? modelStandalone;

        private int loadedModelID = -1;

        private int ibo = 0;
        private int vbo = 0;
        private int vao = 0;

        private bool iboAllocated = false;
        private bool vboAllocated = false;
        private bool vaoAllocated = false;

        private RenderedObjectType type { get; set; }
        private int objectID = 0;
        private int light { get; set; }
        private Rgba32 ambient;
        private float renderDistance { get; set; }
        private Matrix4 modelToWorld = Matrix4.Identity;
        private Matrix4 worldToView = Matrix4.Identity;
        private Model? modelRender;

        private bool selected;
        private float blendDistance = 0.0f;

        private bool renderPrepared = false;
        private bool renderPerform = true;
        private bool renderPerformBillboardOnly = false;

        private List<Animation>? ratchetAnimations = null;

        private List<Texture> textures;
        private Dictionary<Texture, GLTexture> textureIds;
        private ShaderTable shaderTable;
        private BillboardRenderer fallback;
        private AnimationRenderer? animationRenderer = null;

        public MeshRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, GLTexture> textureIds, List<Animation>? ratchetAnimations = null)
        {
            this.shaderTable = shaderTable;
            this.textureIds = textureIds;
            this.textures = textures;
            this.ratchetAnimations = ratchetAnimations;
            this.fallback = new BillboardRenderer(shaderTable);
        }

        public void ChangeTextures(List<Texture> textures, Dictionary<Texture, GLTexture>? textureIds = null)
        {
            this.textures = textures;

            if (textureIds != null)
            {
                this.textureIds = textureIds;
            }

            if (animationRenderer != null)
            {
                animationRenderer.ChangeTextures(textures, textureIds);
            }
        }

        public override void Include<T>(T obj)
        {
            modelRender = null;
            modelObject = null;
            modelStandalone = null;
            animationRenderer = null;

            if (obj is ModelObject mObj)
            {
                fallback.Include(mObj);

                this.modelObject = mObj;
                this.type = RenderedObjectTypeUtils.GetRenderTypeFromLevelObject(mObj);

                GenerateBuffers();
                UpdateVars();

                if (mObj is Moby mob)
                {
                    animationRenderer = new AnimationRenderer(shaderTable, textures, textureIds, ratchetAnimations);
                    animationRenderer.Include(mob);
                }

                return;
            }

            if (obj is Model model)
            {
                this.modelStandalone = model;
                this.type = RenderedObjectType.Null;

                GenerateBuffers();
                UpdateVars();

                if (model is MobyModel mobyModel)
                {
                    animationRenderer = new AnimationRenderer(shaderTable, textures, textureIds, ratchetAnimations);
                    animationRenderer.Include(mobyModel);
                }

                return;
            }

            throw new NotImplementedException();
        }

        public override void Include<T>(List<T> list) => throw new NotImplementedException();

        /// <summary>
        /// Deletes IBO and VBO if they are allocated.
        /// </summary>
        private void DeleteBuffers()
        {
            if (iboAllocated)
            {
                GL.DeleteBuffer(ibo);
                ibo = 0;
                iboAllocated = false;
            }

            if (vboAllocated)
            {
                GL.DeleteBuffer(vbo);
                vbo = 0;
                vboAllocated = false;
            }

            if (vaoAllocated)
            {
                GL.DeleteVertexArray(vao);
                vao = 0;
                vaoAllocated = false;
            }

            loadedModelID = -1;
        }

        /// <summary>
        /// Sets up the vertex attribute pointers according to the type.
        /// </summary>
        private void SetupVertexAttribPointers()
        {
            if (modelObject != null)
            {
                switch (type)
                {
                    case RenderedObjectType.Terrain:
                        GLUtil.ActivateNumberOfVertexAttribArrays(5);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 10, 0);
                        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 10, sizeof(float) * 3);
                        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 10, sizeof(float) * 6);
                        GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 10, sizeof(float) * 8);
                        GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, sizeof(float) * 10, sizeof(float) * 9);
                        break;
                    case RenderedObjectType.Tie:
                        GLUtil.ActivateNumberOfVertexAttribArrays(4);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 9, 0);
                        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 9, sizeof(float) * 3);
                        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 9, sizeof(float) * 6);
                        GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 9, sizeof(float) * 8);
                        break;
                    case RenderedObjectType.Shrub:
                    case RenderedObjectType.Moby:
                        GLUtil.ActivateNumberOfVertexAttribArrays(3);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
                        break;
                }
            }
            else
            {
                GLUtil.ActivateNumberOfVertexAttribArrays(3);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
            }

        }

        /// <summary>
        /// Generates IBO and VBO.
        /// </summary>
        private void GenerateBuffers()
        {
            DeleteBuffers();

            modelRender = modelObject?.model ?? modelStandalone;

            if (modelObject != null)
            {
                loadedModelID = modelObject.modelID;
            }
            else if (modelStandalone != null)
            {
                loadedModelID = modelStandalone.id;
            }

            if (modelRender == null)
                return;

            if (modelRender.GetIndices().Length == 0)
                return;

            // This is a camera object that only exist at runtime and blocks vision in interactive mode.
            // We simply don't draw it.
            if (loadedModelID == 0x3EF && modelObject != null)
            {
                loadedModelID = -1;
                modelObject = null;
                modelRender = null;
                return;
            }

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);
            vaoAllocated = true;

            // IBO
            int iboLength = modelRender.GetIndices().Length * sizeof(ushort);
            if (iboLength > 0)
            {
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboLength, IntPtr.Zero, BufferUsageHint.StaticDraw);
                iboAllocated = true;
            }

            // VBO
            int vboLength = modelRender.GetVertices().Length * sizeof(float);
            if (modelObject != null)
            {
                switch (type)
                {
                    case RenderedObjectType.Terrain:
                        TerrainModel? terrainModel = (TerrainModel?) modelRender;
                        if (terrainModel == null) break;
                        vboLength += modelObject.GetAmbientRgbas().Length * sizeof(Byte) + terrainModel.lights.Count * sizeof(int);
                        break;
                    case RenderedObjectType.Tie:
                        vboLength += modelObject.GetAmbientRgbas().Length * sizeof(Byte);
                        break;
                }
            }

            if (vboLength > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboLength, IntPtr.Zero, BufferUsageHint.StaticDraw);
                vboAllocated = true;
            }

            SetupVertexAttribPointers();

            UpdateBuffers();
        }

        /// <summary>
        /// Updates the buffers. This is not actually needed as long as mesh manipulations are not possible.
        /// </summary>
        private void UpdateBuffers()
        {
            if (modelRender == null) return;

            GL.BindVertexArray(vao);

            ushort[] iboData = modelRender.GetIndices();
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, iboData.Length * sizeof(ushort), iboData);

            float[] vboData = modelRender.GetVertices();
            if (modelObject != null)
            {
                switch (type)
                {
                    case RenderedObjectType.Terrain:
                        {
                            byte[] rgbas = modelObject.GetAmbientRgbas();
                            TerrainModel? terrainModel = (TerrainModel?) modelRender;
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
            }
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vboData.Length * sizeof(float), vboData);
        }

        /// <summary>
        /// Updates the light and ambient variables which can then be used to update the shader. Check if the modelID
        /// has changed and update the buffers if necessary.
        /// </summary>
        private void UpdateVars()
        {
            if (modelObject != null)
            {
                switch (type)
                {
                    case RenderedObjectType.Terrain:
                        light = ALLOCATED_LIGHTS;
                        renderDistance = float.MaxValue;
                        break;
                    case RenderedObjectType.Moby:
                        Moby mob = (Moby) modelObject;
                        light = Math.Max(0, Math.Min(ALLOCATED_LIGHTS, mob.light));
                        mob.color.ToRgba32(ref ambient);
                        renderDistance = mob.drawDistance;
                        break;
                    case RenderedObjectType.Tie:
                        Tie tie = (Tie) modelObject;
                        light = Math.Max(0, Math.Min(ALLOCATED_LIGHTS, tie.light));
                        renderDistance = float.MaxValue;
                        break;
                    case RenderedObjectType.Shrub:
                        Shrub shrub = (Shrub) modelObject;
                        light = Math.Max(0, Math.Min(ALLOCATED_LIGHTS, shrub.light));
                        ambient = shrub.color;
                        renderDistance = shrub.drawDistance;
                        break;
                }

                modelToWorld = modelObject.modelMatrix;
                objectID = modelObject.globalID;

                if (loadedModelID != modelObject.modelID)
                {
                    GenerateBuffers();
                }
            }
            else if (modelStandalone != null)
            {
                modelToWorld = Matrix4.Identity;
                objectID = 0;

                if (loadedModelID != modelStandalone.id)
                {
                    GenerateBuffers();
                }
            }
        }

        /// <summary>
        /// Takes a textureConfig mode as input and sets the transparency mode based on that.
        /// </summary>
        private void SetTransparencyMode(TextureConfig config)
        {
            shaderTable.meshShader.SetUniform1(UniformName.useTransparency, (config.IgnoresTransparency()) ? 0 : 1);
        }

        /// <summary>
        /// Takes a textureConfig as input and sets the texture wrap modes based on that.
        /// </summary>
        private void SetTextureWrapMode(TextureConfig conf, GLTexture tex)
        {
            /*
             * There is an issue with opaque edges in some transparent objects
             * This can easily be observed on RaC 1 Kerwan where you have these ugly edges on some trees and the bottom
             * of the fading out buildings.
             */

            TextureWrapMode wrapS, wrapT;

            switch (conf.wrapModeS)
            {
                case TextureConfig.WrapMode.ClampEdge:
                    wrapS = TextureWrapMode.ClampToEdge;
                    break;
                case TextureConfig.WrapMode.Repeat:
                default:
                    wrapS = TextureWrapMode.Repeat;
                    break;
            }

            switch (conf.wrapModeT)
            {
                case TextureConfig.WrapMode.ClampEdge:
                    wrapT = TextureWrapMode.ClampToEdge;
                    break;
                case TextureConfig.WrapMode.Repeat:
                default:
                    wrapT = TextureWrapMode.Repeat;
                    break;
            }

            tex.SetWrapModes(wrapS, wrapT);
        }

        /// <summary>
        /// Sets an internal variable to true if the corresponding modelObject is equal to
        /// the selectedObject in which case an outline will be rendered.
        /// </summary>
        private void Select(LevelObject selectedObject)
        {
            selected = modelObject == selectedObject;
        }

        /// <summary>
        /// Sets an internal variable to true if the corresponding modelObject is a member
        /// of selectedObjects in which case an outline will be rendered.
        /// </summary>
        private void Select(ICollection<LevelObject> selectedObjects)
        {
            if (modelObject == null) return;

            selected = selectedObjects.Contains(modelObject);
        }

        /// <summary>
        /// Returns true if the object is to be culled.
        /// Mobies and shrubs are culled by their drawDistance.
        /// Ties, terrain and shrubs are culled by frustum culling.
        /// </summary>
        private bool ComputeCulling(Camera camera, bool distanceCulling, bool frustumCulling)
        {
            if (modelRender == null) return false;
            if (modelStandalone != null) return false;
            if (modelObject == null) return true;

            if (distanceCulling)
            {
                float dist = (modelObject.position - camera.position).Length;

                blendDistance = MathF.Max(0.125f * (dist - renderDistance), 0.0f);

                if (dist > renderDistance + 8.0f)
                {
                    return true;
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

                    Camera.Frustum frustum = camera.GetFrustum();

                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 planeNormal = frustum.planeNormals[i];
                        Vector3 planePoint = frustum.planePoints[i];

                        if (Vector3.Dot(center - planePoint, planeNormal) < -size)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /*
         * This function computes all values required for Render.
         * Note that any changes made between PrepareRender and Render will be ignored.
         */
        public void PrepareRender(RendererPayload payload)
        {
            if (modelObject == null && modelStandalone == null)
            {
                renderPrepared = true;
                renderPerform = false;
                return;
            }

            if (modelObject is Moby mob && mob.memory != null && mob.memory.IsDead())
            {
                renderPrepared = true;
                renderPerform = false;
                return;
            }

            UpdateVars();

            modelRender = modelObject?.model ?? modelStandalone;

            if (ComputeCulling(payload.camera, payload.visibility.enableDistanceCulling, payload.visibility.enableFrustumCulling))
            {
                renderPrepared = true;
                renderPerform = false;
                return;
            }

            renderPrepared = true;
            renderPerform = true;

            if (modelRender == null || modelRender.GetIndices().Length == 0)
            {
                renderPerformBillboardOnly = true;
                return;
            }

            worldToView = payload.camera.GetWorldViewMatrix();
            Select(payload.selection);

            renderPerformBillboardOnly = false;
        }

        public override void Render(RendererPayload payload)
        {
            if (renderPrepared && !renderPerform)
            {
                renderPrepared = false;
                return;
            }
            else if (!renderPrepared)
            {
                PrepareRender(payload);
                renderPrepared = false;
                if (!renderPerform)
                {
                    return;
                }
            }

            renderPrepared = false;

            if (renderPerformBillboardOnly)
            {
                if (payload.visibility.enableMeshlessModels)
                {
                    fallback.Render(payload);
                }
                return;
            }

            if (payload.visibility.enableAnimations && animationRenderer != null)
            {
                if (animationRenderer.IsValid())
                {
                    animationRenderer.Render(payload);
                    return;
                }
            }

            if (modelRender == null) return;

            GL.BindVertexArray(vao);

            shaderTable.meshShader.UseShader();

            shaderTable.meshShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
            shaderTable.meshShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.meshShader.SetUniform1(UniformName.levelObjectNumber, objectID);
            shaderTable.meshShader.SetUniform1(UniformName.levelObjectType, (int) type);
            shaderTable.meshShader.SetUniform4(UniformName.staticColor, ambient);
            shaderTable.meshShader.SetUniform1(UniformName.lightIndex, light);
            shaderTable.meshShader.SetUniform1(UniformName.objectBlendDistance, blendDistance);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in modelRender.textureConfig)
            {
                if (conf.id >= 0)
                {
                    GLTexture tex = textureIds[textures[conf.id]];
                    tex.Bind();
                    SetTextureWrapMode(conf, tex);
                }
                else
                {
                    GLTexture.BindNull();
                }
                SetTransparencyMode(conf);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }

            if (selected)
            {
                shaderTable.colorShader.UseShader();

                shaderTable.colorShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
                shaderTable.colorShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
                shaderTable.colorShader.SetUniform1(UniformName.levelObjectNumber, objectID);
                shaderTable.colorShader.SetUniform1(UniformName.levelObjectType, (int) type);
                shaderTable.colorShader.SetUniform4(UniformName.incolor, 1.0f, 1.0f, 1.0f, 1.0f);

                GL.Enable(EnableCap.LineSmooth);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, modelRender.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Disable(EnableCap.LineSmooth);
            }

            GLUtil.CheckGlError("MeshRenderer");
        }

        public override void Dispose()
        {
            DeleteBuffers();
        }
    }
}
