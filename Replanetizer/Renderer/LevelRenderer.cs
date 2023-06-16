// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Drawing;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class LevelRenderer : Renderer
    {
        private readonly ShaderTable shaderTable;
        private Dictionary<Texture, GLTexture> textureIDs;

        private LevelVariables? levelVariables;
        private List<Light>? lights;
        private List<Texture>? textures;

        private SkyRenderer? skyRenderer;
        private CollisionRenderer? collisionRenderer;
        private List<MeshRenderer> mobyRenderers = new List<MeshRenderer>();
        private List<MeshRenderer> tieRenderers = new List<MeshRenderer>();
        private List<MeshRenderer> shrubRenderers = new List<MeshRenderer>();
        private List<List<MeshRenderer>> terrainRenderers = new List<List<MeshRenderer>>();
        private WireframeRenderer? cuboidRenderer;
        private WireframeRenderer? sphereRenderer;
        private WireframeRenderer? cylinderRenderer;
        private WireframeRenderer? pillRenderer;
        private WireframeRenderer? gameCameraRenderer;
        private WireframeRenderer? splineRenderer;
        private BillboardRenderer? soundInstanceRenderer;
        private BillboardRenderer? pointLightRenderer;
        private BillboardRenderer? envSamplesRenderer;
        private BillboardRenderer? envTransitionRenderer;
        private BillboardRenderer? grindPathRenderer;
        private WireframeRenderer? grindPathSplineRenderer;
        private ToolRenderer toolRenderer;
        private DirectionalLightsBuffer dirLightsBuffer;

        public LevelRenderer(ShaderTable shaderTable, Dictionary<Texture, GLTexture> textureIDs)
        {
            this.shaderTable = shaderTable;
            this.textureIDs = textureIDs;

            this.toolRenderer = new ToolRenderer(shaderTable);
            this.dirLightsBuffer = new DirectionalLightsBuffer(shaderTable);
        }

        private void Add(Level level)
        {
            levelVariables = level.levelVariables;
            lights = level.lights;
            textures = level.textures;

            skyRenderer = new SkyRenderer(shaderTable, level.textures, textureIDs);
            skyRenderer.Include(level.skybox);

            collisionRenderer = new CollisionRenderer(shaderTable);
            if (level.collisionChunks.Count > 0)
            {
                collisionRenderer.Include(level.collisionChunks);
            }
            else
            {
                collisionRenderer.Include(level.collisionEngine);
            }

            foreach (Moby mob in level.mobs)
            {
                MeshRenderer mobRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
                mobRenderer.Include(mob);
                mobyRenderers.Add(mobRenderer);
            }

            foreach (Shrub shrub in level.shrubs)
            {
                MeshRenderer shrubRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
                shrubRenderer.Include(shrub);
                shrubRenderers.Add(shrubRenderer);
            }

            foreach (Tie tie in level.ties)
            {
                MeshRenderer tieRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
                tieRenderer.Include(tie);
                tieRenderers.Add(tieRenderer);
            }

            if (level.terrainChunks.Count > 0)
            {
                foreach (Terrain terrainChunk in level.terrainChunks)
                {
                    List<MeshRenderer> terrainRenderer = new List<MeshRenderer>();
                    foreach (TerrainFragment fragment in terrainChunk.fragments)
                    {
                        MeshRenderer fragmentRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
                        fragmentRenderer.Include(fragment);
                        terrainRenderer.Add(fragmentRenderer);
                    }
                    terrainRenderers.Add(terrainRenderer);
                }
            }
            else
            {
                List<MeshRenderer> terrainRenderer = new List<MeshRenderer>();
                foreach (TerrainFragment fragment in level.terrainEngine.fragments)
                {
                    MeshRenderer fragmentRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
                    fragmentRenderer.Include(fragment);
                    terrainRenderer.Add(fragmentRenderer);
                }
                terrainRenderers.Add(terrainRenderer);
            }

            cuboidRenderer = new WireframeRenderer(shaderTable);
            cuboidRenderer.Include(level.cuboids);

            sphereRenderer = new WireframeRenderer(shaderTable);
            sphereRenderer.Include(level.spheres);

            cylinderRenderer = new WireframeRenderer(shaderTable);
            cylinderRenderer.Include(level.cylinders);

            pillRenderer = new WireframeRenderer(shaderTable);
            pillRenderer.Include(level.pills);

            gameCameraRenderer = new WireframeRenderer(shaderTable);
            gameCameraRenderer.Include(level.gameCameras);

            splineRenderer = new WireframeRenderer(shaderTable);
            foreach (Spline spline in level.splines)
            {
                splineRenderer.Include(spline);
            }

            soundInstanceRenderer = new BillboardRenderer(shaderTable);
            soundInstanceRenderer.Include(level.soundInstances);

            pointLightRenderer = new BillboardRenderer(shaderTable);
            pointLightRenderer.Include(level.pointLights);

            envSamplesRenderer = new BillboardRenderer(shaderTable);
            envSamplesRenderer.Include(level.envSamples);

            envTransitionRenderer = new BillboardRenderer(shaderTable);
            envTransitionRenderer.Include(level.envTransitions);

            grindPathRenderer = new BillboardRenderer(shaderTable);
            grindPathRenderer.Include(level.grindPaths);

            grindPathSplineRenderer = new WireframeRenderer(shaderTable);
            foreach (GrindPath path in level.grindPaths)
            {
                grindPathSplineRenderer.Include(path.spline);
            }
        }

        private void Add(Shrub shrub)
        {
            if (textures == null)
            {
                throw new NullReferenceException();
            }

            MeshRenderer shrubRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
            shrubRenderer.Include(shrub);
            shrubRenderers.Add(shrubRenderer);
        }

        private void Add(Tie tie)
        {
            if (textures == null)
            {
                throw new NullReferenceException();
            }

            MeshRenderer tieRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
            tieRenderer.Include(tie);
            tieRenderers.Add(tieRenderer);
        }

        private void Add(Moby mob)
        {
            if (textures == null)
            {
                throw new NullReferenceException();
            }

            MeshRenderer mobRenderer = new MeshRenderer(shaderTable, textures, textureIDs);
            mobRenderer.Include(mob);
            mobyRenderers.Add(mobRenderer);
        }

        public override void Include<T>(T obj)
        {
            switch (obj)
            {
                case Level level:
                    Add(level);
                    return;
                case Shrub shrub:
                    Add(shrub);
                    return;
                case Tie tie:
                    Add(tie);
                    return;
                case Moby mob:
                    Add(mob);
                    return;
            }

            throw new NotImplementedException();
        }
        public override void Include<T>(List<T> list) => throw new NotImplementedException();
        public override void Render(RendererPayload payload)
        {
            GL.ClearColor((levelVariables != null) ? levelVariables.fogColor : Color.SkyBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.DepthFunc(DepthFunction.Lequal);

            if (payload.visibility.enableSkybox)
                skyRenderer?.Render(payload);

            bool useFog = payload.visibility.enableFog && levelVariables != null;

            shaderTable.animationShader.UseShader();
            shaderTable.animationShader.SetUniform1(UniformName.useFog, (useFog) ? 1 : 0);

            if (useFog && levelVariables != null)
            {
                shaderTable.animationShader.SetUniform4(UniformName.fogColor, levelVariables.fogColor);
                shaderTable.animationShader.SetUniform4(UniformName.fogParams, levelVariables.fogNearDistance / 1024.0f,
                        1024.0f / (levelVariables.fogFarDistance - levelVariables.fogNearDistance),
                        1.0f - levelVariables.fogNearIntensity / 255.0f,
                        1.0f - levelVariables.fogFarIntensity / 255.0f);
            }

            shaderTable.meshShader.UseShader();
            shaderTable.meshShader.SetUniform1(UniformName.useFog, (useFog) ? 1 : 0);

            if (useFog && levelVariables != null)
            {
                shaderTable.meshShader.SetUniform4(UniformName.fogColor, levelVariables.fogColor);
                shaderTable.meshShader.SetUniform4(UniformName.fogParams, levelVariables.fogNearDistance / 1024.0f,
                        1024.0f / (levelVariables.fogFarDistance - levelVariables.fogNearDistance),
                        1.0f - levelVariables.fogNearIntensity / 255.0f,
                        1.0f - levelVariables.fogFarIntensity / 255.0f);
            }

            dirLightsBuffer.Update(lights);
            dirLightsBuffer.Bind();

            if (payload.visibility.enableTerrain)
            {
                for (int i = 0; i < terrainRenderers.Count; i++)
                {
                    if (payload.visibility.chunks[i])
                    {
                        foreach (MeshRenderer meshRenderer in terrainRenderers[i])
                        {
                            meshRenderer.Render(payload);
                        }
                    }
                }
            }

            if (payload.visibility.enableTie)
            {
                foreach (MeshRenderer meshRenderer in tieRenderers)
                {
                    meshRenderer.Render(payload);
                }
            }

            if (payload.visibility.enableShrub)
            {
                foreach (MeshRenderer meshRenderer in shrubRenderers)
                {
                    meshRenderer.Render(payload);
                }
            }

            if (payload.visibility.enableMoby)
            {
                foreach (MeshRenderer meshRenderer in mobyRenderers)
                {
                    meshRenderer.Render(payload);
                }
            }

            if (payload.visibility.enableCollision)
                collisionRenderer?.Render(payload);

            if (payload.visibility.enableCuboid)
                cuboidRenderer?.Render(payload);

            if (payload.visibility.enableSpheres)
                sphereRenderer?.Render(payload);

            if (payload.visibility.enableCylinders)
                cylinderRenderer?.Render(payload);

            if (payload.visibility.enablePills)
                pillRenderer?.Render(payload);

            if (payload.visibility.enableGameCameras)
                gameCameraRenderer?.Render(payload);

            if (payload.visibility.enableSpline)
                splineRenderer?.Render(payload);

            if (payload.visibility.enableSoundInstances)
                soundInstanceRenderer?.Render(payload);

            if (payload.visibility.enablePointLights)
                pointLightRenderer?.Render(payload);

            if (payload.visibility.enableEnvSamples)
                envSamplesRenderer?.Render(payload);

            if (payload.visibility.enableEnvTransitions)
                envTransitionRenderer?.Render(payload);

            if (payload.visibility.enableGrindPaths)
            {
                grindPathRenderer?.Render(payload);
                grindPathSplineRenderer?.Render(payload);
            }

            toolRenderer.Render(payload);
        }

        public override void Dispose()
        {

        }
    }
}
