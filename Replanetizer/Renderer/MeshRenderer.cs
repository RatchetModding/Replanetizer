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
using System.IO;
using LibReplanetizer.Models;
using Replanetizer.Utils;
using SixLabors.ImageSharp;
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

        public ModelObject? modelObject;
        private Model? modelStandalone;

        private int loadedModelID = -1;
        private bool modelHasMeshData = false;

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
        private float mobyAlpha = 1.0f;

        private bool renderPrepared = false;
        private bool renderPerform = true;
        private bool renderPerformBillboardOnly = false;
        private bool renderCameraMesh = true;

        private List<Animation>? ratchetAnimations = null;

        private List<Texture> textures;
        private Dictionary<Texture, GLTexture> textureIds;
        private readonly GLTexture metalTexture;
        private ShaderTable shaderTable;
        private BillboardRenderer fallback;
        private AnimationRenderer? animationRenderer = null;
        private readonly ModelGPUDataCache gpuDataCache;
        private ModelGPUData? gpuData;

        public MeshRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, GLTexture> textureIds, GLTexture metalTexture, List<Animation>? ratchetAnimations = null, ModelGPUDataCache? gpuDataCache = null)
        {
            this.shaderTable = shaderTable;
            this.textureIds = textureIds;
            this.textures = textures;
            this.metalTexture = metalTexture;
            this.ratchetAnimations = ratchetAnimations;
            this.gpuDataCache = gpuDataCache ?? new ModelGPUDataCache();
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

                UpdateVars();
                return;
            }

            if (obj is Model model)
            {
                this.modelStandalone = model;
                this.type = RenderedObjectType.Null;

                UpdateVars();
                return;
            }

            throw new NotImplementedException();
        }

        public override void Include<T>(List<T> list) => throw new NotImplementedException();

        private void DeleteBuffers()
        {
            gpuDataCache.Release(gpuData);
            gpuData = null;
            loadedModelID = -1;
            modelHasMeshData = false;
            modelRender = null;
            renderPrepared = false;
        }

        private static bool HasMeshData(Model model)
        {
            if (model.GetIndices().Length > 0)
            {
                return true;
            }

            return model is MetalModel metalModel && metalModel.metalIndexBuffer.Length > 0;
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

                if (modelObject is Moby mob)
                {
                    animationRenderer = new AnimationRenderer(shaderTable, textures, textureIds, metalTexture, ratchetAnimations, gpuDataCache);
                    animationRenderer.Include(mob);
                }
            }
            else if (modelStandalone != null)
            {
                loadedModelID = modelStandalone.id;

                if (modelStandalone is MobyModel mobyModel)
                {
                    animationRenderer = new AnimationRenderer(shaderTable, textures, textureIds, metalTexture, ratchetAnimations, gpuDataCache);
                    animationRenderer.Include(mobyModel);
                }
            }

            if (modelRender == null)
                return;

            if (!HasMeshData(modelRender))
                return;

            // This is a camera object that only exist at runtime and blocks vision in interactive mode.
            // We simply don't draw it.
            if (renderCameraMesh == false && loadedModelID == 0x3EF && modelObject != null)
            {
                loadedModelID = -1;
                modelHasMeshData = false;
                modelObject = null;
                modelRender = null;
                return;
            }

            modelHasMeshData = true;

            ModelGPULayout layout = type switch
            {
                RenderedObjectType.Terrain => ModelGPULayout.Terrain,
                RenderedObjectType.Tie => ModelGPULayout.Tie,
                _ => ModelGPULayout.Static
            };
            byte[]? ambientRgbas = modelObject is Tie || modelObject is TerrainFragment
                ? modelObject.GetAmbientRgbas() : null;
            List<int>? terrainLights = (modelRender as TerrainModel)?.lights;
            string instanceKey = (ambientRgbas == null ? "" : Convert.ToBase64String(ambientRgbas)) + ":"
                + (terrainLights == null ? "" : string.Join(",", terrainLights));

            gpuData = gpuDataCache.Acquire(modelRender, layout, instanceKey, ambientRgbas, terrainLights);
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
                        renderDistance = (mob.drawDistance > 0.0f) ? mob.drawDistance : float.MaxValue;
                        if (mob.memory != null)
                        {
                            renderDistance = mob.memory.drawDistance;
                        }
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

                bool currentModelHasMeshData = modelObject.model != null && HasMeshData(modelObject.model);
                if (modelRender != modelObject.model
                    || loadedModelID != modelObject.modelID
                    || modelHasMeshData != currentModelHasMeshData)
                {
                    GenerateBuffers();
                }
            }
            else if (modelStandalone != null)
            {
                modelToWorld = Matrix4.Identity;
                objectID = 0;

                bool currentModelHasMeshData = HasMeshData(modelStandalone);
                if (modelRender != modelStandalone
                    || loadedModelID != modelStandalone.id
                    || modelHasMeshData != currentModelHasMeshData)
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

        private void SetTextureMode(TextureConfig config)
        {
            shaderTable.meshShader.SetUniform1(UniformName.useTexture, (config.id >= 0) ? 1 : 0);
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
        private bool ComputeCulling(Camera camera, bool distanceCulling, bool frustumCulling, bool visibleCulling)
        {
            if (modelRender == null) return false;
            if (modelStandalone != null) return false;
            if (modelObject == null) return true;

            if (visibleCulling && modelObject is Moby mob && mob.memory != null)
            {
                if (mob.memory.visible == 0)
                    return true;
            }

            if (distanceCulling)
            {
                Vector3 cullingCenter = modelObject.position;
                float cullingRadius = 0.0f;
                if (type == RenderedObjectType.Moby && modelObject is Moby mobyMemory && mobyMemory.memory != null)
                {
                    cullingCenter = new Vector3(
                        mobyMemory.memory.collPos.X / 1024.0f,
                        mobyMemory.memory.collPos.Y / 1024.0f,
                        mobyMemory.memory.collPos.Z / 1024.0f);
                    cullingRadius = mobyMemory.memory.collPos.W / 1024.0f;
                }

                float dist = (cullingCenter - camera.position).Length;

                float blendScale = 8.0f;

                blendDistance = MathF.Max((dist - cullingRadius - renderDistance + blendScale) / blendScale, 0.0f);

                if (dist > renderDistance + cullingRadius)
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
                            center = (modelObject.reflection * new Vector4(center, 1.0f)).Xyz;
                            center = modelObject.rotation * center;
                            center += modelObject.position;
                            float shrubScale = MathF.MaxMagnitude(modelObject.scale.X, MathF.MaxMagnitude(modelObject.scale.Y, modelObject.scale.Z));
                            size = shrub.cullingRadius * shrubScale;
                            break;
                        case RenderedObjectType.Tie:
                            TieModel? tie = (TieModel?) modelObject.model;
                            if (tie == null) break;
                            center = new Vector3(tie.cullingX, tie.cullingY, tie.cullingZ);
                            center = (modelObject.reflection * new Vector4(center, 1.0f)).Xyz;
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

                        if (Vector3.Dot(planePoint - center, planeNormal) > size)
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

            renderCameraMesh = payload.visibility.hideCameraMoby == false;

            UpdateVars();

            mobyAlpha = 1.0f;
            if (modelObject is Moby moby && moby.memory != null)
            {
                mobyAlpha = moby.memory.alpha / 128.0f;
            }

            modelRender = modelObject?.model ?? modelStandalone;

            if (ComputeCulling(payload.camera, payload.visibility.enableDistanceCulling, payload.visibility.enableFrustumCulling, payload.visibility.enableVisibleCulling))
            {
                renderPrepared = true;
                renderPerform = false;
                return;
            }

            renderPrepared = true;
            renderPerform = true;
            renderPerformBillboardOnly = false;

            if (modelRender == null || !HasMeshData(modelRender))
            {
                renderPerformBillboardOnly = true;
                return;
            }

            worldToView = payload.camera.GetWorldViewMatrix();
            Select(payload.selection);

            renderPerformBillboardOnly = false;
        }

        private void RenderModel(Model model, ModelGPUData modelGPUData)
        {
            GL.BindVertexArray(modelGPUData.vao);

            shaderTable.meshShader.UseShader();
            shaderTable.meshShader.SetUniform1(UniformName.useMetalShading, 0);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in model.textureConfig)
            {
                if (conf.id >= 0 && conf.id < textures.Count)
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
                SetTextureMode(conf);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }

            if (model is MetalModel metalModel && modelGPUData.metalVao != 0 && modelGPUData.metalIndexCount > 0)
            {
                GL.BindVertexArray(modelGPUData.metalVao);
                shaderTable.meshShader.SetUniform1(UniformName.useMetalShading, 1);

                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(-1.0f, -1.0f);
                GL.DepthMask(false);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                GL.BlendEquation(BlendEquationMode.FuncAdd);

                foreach (TextureConfig conf in metalModel.metalTextureConfig)
                {
                    metalTexture.Bind();
                    SetTextureWrapMode(conf, metalTexture);

                    SetTransparencyMode(conf);
                    SetTextureMode(conf);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, (conf.start - model.faceCount * 3) * sizeof(ushort));
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.PolygonOffsetFill);
                GL.DepthMask(true);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            if (selected)
            {
                GL.BindVertexArray(modelGPUData.vao);
                shaderTable.colorShader.UseShader();

                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public override void Render(RendererPayload payload)
        {
            // Check if payload changes require preparation.
            if (renderCameraMesh == payload.visibility.hideCameraMoby)
            {
                DeleteBuffers();
            }

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

            if (modelRender == null || gpuData == null) return;

            // Setup shaders
            shaderTable.meshShader.UseShader();

            shaderTable.meshShader.SetUniform1(UniformName.mainTexture, 0);
            shaderTable.meshShader.SetUniform1(UniformName.blueNoiseTexture, 1);
            shaderTable.meshShader.SetUniform1(UniformName.ssaaLevelLog, FramebufferRenderer.SSAA_LEVEL_LOG);
            shaderTable.meshShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
            shaderTable.meshShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            Matrix4 viewMatrix = payload.camera.GetViewMatrix();
            shaderTable.meshShader.SetUniformMatrix4(UniformName.viewMatrix, ref viewMatrix);
            shaderTable.meshShader.SetUniform1(UniformName.levelObjectNumber, objectID);
            shaderTable.meshShader.SetUniform1(UniformName.levelObjectType, (int) type);
            shaderTable.meshShader.SetUniform4(UniformName.staticColor, ambient);
            shaderTable.meshShader.SetUniform1(UniformName.lightIndex, light);
            shaderTable.meshShader.SetUniform1(UniformName.objectBlendDistance, blendDistance);
            shaderTable.meshShader.SetUniform1(UniformName.mobyAlpha, mobyAlpha);


            GLTexture.blueNoiseTexture.Bind(1);

            if (selected)
            {
                shaderTable.colorShader.UseShader();

                shaderTable.colorShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
                shaderTable.colorShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
                shaderTable.colorShader.SetUniform1(UniformName.levelObjectNumber, objectID);
                shaderTable.colorShader.SetUniform1(UniformName.levelObjectType, (int) type);
                shaderTable.colorShader.SetUniform4(UniformName.incolor, 1.0f, 1.0f, 1.0f, 1.0f);
            }

            RenderModel(modelRender, gpuData);

            int subModelCount = modelRender.GetSubModelCount();
            for (int i = 0; i < subModelCount; i++)
            {
                if ((payload.visibility.subModelsMask & (1u << i)) == 0) continue;

                Model? subModel = modelRender.GetSubModel(i);
                ModelGPUData? modelGPUData = gpuData.subModels[i];

                if (subModel == null || modelGPUData == null) continue;

                RenderModel(subModel, modelGPUData);
            }

            GLUtil.CheckGlError("MeshRenderer");
        }

        public override void Dispose()
        {
            DeleteBuffers();
            fallback?.Dispose();
            animationRenderer?.Dispose();
        }
    }
}
