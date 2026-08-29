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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LibReplanetizer.Models;
using Replanetizer.Renderer;
using Replanetizer.Utils;
using LibReplanetizer.Models.Animations;

namespace Replanetizer.Renderer
{

    /*
     * A container to store IBO and VBO references for a Model
     */
    public class AnimationRenderer : Renderer
    {
        private static readonly int ALLOCATED_LIGHTS = 20;

        private Moby? mob;
        private MobyModel? mobyModelStandalone;

        private int loadedModelID = -1;
        private MobyModel? loadedModel;
        private bool loadedModelHasMeshData = false;

        private bool emptyModel = true;


        private int light { get; set; }
        private Rgb24 ambient { get; set; }
        private float renderDistance { get; set; }
        private static Vector4 SELECTED_COLOR = new Vector4(2.0f, 2.0f, 2.0f, 1.0f);

        private bool selected;
        private float blendDistance = 0.0f;
        private float mobyAlpha = 1.0f;

        private List<Texture> textures;
        private Dictionary<Texture, GLTexture> textureIds;
        private readonly GLTexture metalTexture;
        private ShaderTable shaderTable;

        // The Ratchet moby does not contain its own animations.
        private List<Animation>? ratchetAnimations = null;

        private int currentFrameID = 0;
        private int currentAnimationID = 0;
        private Frame? currentFrame = null;
        private Frame? previousFrame = null;
        private int runtimeTransitionCacheKey = -1;
        private BoneTransform[]? runtimePose = null;
        private BoneTransform[]? runtimeTransitionPose = null;
        private Matrix4[]? boneMatrices = null;
        private BoneTransform[]? localBoneTransforms = null;
        private BoneTransform[]? runtimePreviousPose = null;
        private BoneTransform[]? runtimeCurrentPose = null;
        private bool runtimePoseValid = false;
        private bool runtimeTransitionPoseValid = false;
        private float frameBlend = 0.0f;
        private readonly ModelGPUDataCache gpuDataCache;
        private ModelGPUData? gpuData;

        public AnimationRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, GLTexture> textureIds, GLTexture metalTexture, List<Animation>? ratchetAnimations = null, ModelGPUDataCache? gpuDataCache = null)
        {
            this.shaderTable = shaderTable;
            this.textureIds = textureIds;
            this.textures = textures;
            this.metalTexture = metalTexture;
            this.ratchetAnimations = ratchetAnimations;
            this.gpuDataCache = gpuDataCache ?? new ModelGPUDataCache();
        }

        public void ChangeTextures(List<Texture> textures, Dictionary<Texture, GLTexture>? textureIds = null)
        {
            this.textures = textures;

            if (textureIds != null)
            {
                this.textureIds = textureIds;
            }
        }

        public override void Include<T>(T obj)
        {
            mob = null;
            mobyModelStandalone = null;

            if (obj is Moby moby)
            {
                mob = moby;
                UpdateVars();
                return;
            }

            if (obj is MobyModel mobyModel)
            {
                mobyModelStandalone = mobyModel;
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
            loadedModel = null;
            loadedModelHasMeshData = false;
            emptyModel = true;
            currentFrameID = 0;
            currentAnimationID = 0;
            currentFrame = null;
            previousFrame = null;
            runtimeTransitionCacheKey = -1;
            runtimePose = null;
            runtimeTransitionPose = null;
            boneMatrices = null;
            localBoneTransforms = null;
            runtimePreviousPose = null;
            runtimeCurrentPose = null;
            runtimePoseValid = false;
            runtimeTransitionPoseValid = false;
            frameBlend = 0.0f;
        }

        private static bool HasAnimationMeshData(MobyModel? model)
        {
            return model != null && model.GetIndices().Length > 0 && model.boneCount > 0;
        }

        public bool IsValid()
        {
            UpdateVars();
            return (emptyModel) ? false : true;
        }

        /// <summary>
        /// Generates IBO and VBO.
        /// </summary>
        private void GenerateBuffers()
        {
            DeleteBuffers();

            if (mob == null && mobyModelStandalone == null)
            {
                emptyModel = true;
                return;
            }

            MobyModel? mobyModel = (MobyModel?) mob?.model ?? mobyModelStandalone;

            if (mob != null)
            {
                loadedModelID = mob.modelID;
            }
            else if (mobyModelStandalone != null)
            {
                loadedModelID = mobyModelStandalone.id;
            }

            if (mobyModel == null)
            {
                return;
            }

            loadedModel = mobyModel;
            loadedModelHasMeshData = HasAnimationMeshData(mobyModel);

            if (!loadedModelHasMeshData)
                return;

            // This is a camera object that only exist at runtime and blocks vision in interactive mode.
            // We simply don't draw it.
            if (loadedModelID == 0x3EF)
            {
                loadedModelID = -1;
                loadedModel = null;
                loadedModelHasMeshData = false;
                emptyModel = true;
                mob = null;
                return;
            }

            emptyModel = false;

            boneMatrices = new Matrix4[mobyModel.boneCount];
            localBoneTransforms = new BoneTransform[mobyModel.boneCount];
            runtimePreviousPose = new BoneTransform[mobyModel.boneCount];
            runtimeCurrentPose = new BoneTransform[mobyModel.boneCount];
            runtimePose = new BoneTransform[mobyModel.boneCount];
            runtimeTransitionPose = new BoneTransform[mobyModel.boneCount];

            gpuData = gpuDataCache.Acquire(mobyModel, ModelGPULayout.Animated);
        }

        /// <summary>
        /// Updates the light and ambient variables which can then be used to update the shader. Check if the modelID
        /// has changed and update the buffers if necessary.
        /// </summary>
        private void UpdateVars()
        {
            int modelID = -1;

            if (mob != null)
            {
                light = Math.Max(0, Math.Min(ALLOCATED_LIGHTS, mob.light)); ;
                ambient = mob.color;
                renderDistance = (mob.drawDistance > 0.0f) ? mob.drawDistance : float.MaxValue;
                modelID = mob.modelID;
            }
            else if (mobyModelStandalone != null)
            {
                light = -1;
                ambient = Color.FromRgb(255, 255, 255).ToPixel<Rgb24>();
                modelID = mobyModelStandalone.id;
            }

            MobyModel? currentModel = (MobyModel?) mob?.model ?? mobyModelStandalone;
            bool currentModelHasMeshData = HasAnimationMeshData(currentModel);

            if (loadedModelID != modelID
                || loadedModel != currentModel
                || loadedModelHasMeshData != currentModelHasMeshData)
            {
                previousFrame = null;
                GenerateBuffers();
            }
        }

        /// <summary>
        /// Takes a textureConfig mode as input and sets the transparency mode based on that.
        /// </summary>
        private void SetTransparencyMode(TextureConfig config)
        {
            shaderTable.animationShader.SetUniform1(UniformName.useTransparency, (config.IgnoresTransparency()) ? 0 : 1);
        }

        private void SetTextureMode(TextureConfig config)
        {
            shaderTable.animationShader.SetUniform1(UniformName.useTexture, (config.id >= 0) ? 1 : 0);
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
            selected = mob == selectedObject;
        }

        /// <summary>
        /// Sets an internal variable to true if the corresponding modelObject is a member
        /// of selectedObjects in which case an outline will be rendered.
        /// </summary>
        private void Select(ICollection<LevelObject> selectedObjects)
        {
            if (mob == null) return;

            selected = selectedObjects.Contains(mob);
        }

        /// <summary>
        /// Returns true if the object is to be culled.
        /// Mobies and shrubs are culled by their drawDistance.
        /// Ties, terrain and shrubs are culled by frustum culling.
        /// </summary>
        private bool ComputeCulling(Camera camera, bool distanceCulling, bool visibleCulling)
        {
            if (emptyModel) return true;
            if (mobyModelStandalone != null) return false;
            if (mob == null) return true;

            if (visibleCulling && mob.memory != null)
            {
                if (mob.memory.visible == 0)
                    return true;
            }

            if (distanceCulling)
            {
                float dist = (mob.position - camera.position).Length;

                float blendScale = 32.0f;

                blendDistance = MathF.Max((dist - renderDistance) / blendScale, 0.0f);

                if (dist > renderDistance + blendScale)
                {
                    return true;
                }
            }
            else
            {
                blendDistance = 0.0f;
            }

            return false;
        }

        private void RenderModel(Model model, ModelGPUData modelGPUData)
        {
            GL.BindVertexArray(modelGPUData.vao);

            shaderTable.animationShader.SetUniform1(UniformName.useMetalShading, 0);

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
                shaderTable.animationShader.SetUniform1(UniformName.useMetalShading, 1);

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
        }

        private static float ClampBlend(float blend)
        {
            return MathF.Max(0.0f, MathF.Min(1.0f, blend));
        }

        private static Quaternion NormalizeQuaternion(Quaternion quaternion)
        {
            float lengthSquared = quaternion.X * quaternion.X
                + quaternion.Y * quaternion.Y
                + quaternion.Z * quaternion.Z
                + quaternion.W * quaternion.W;

            if (lengthSquared <= 0.0f)
            {
                return Quaternion.Identity;
            }

            return quaternion * (1.0f / MathF.Sqrt(lengthSquared));
        }

        private static Quaternion BlendQuaternion(Quaternion current, Quaternion target, float blend)
        {
            if (current.X * target.X
                + current.Y * target.Y
                + current.Z * target.Z
                + current.W * target.W < 0.0f)
            {
                target *= -1.0f;
            }

            return NormalizeQuaternion(current * (1.0f - blend) + target * blend);
        }

        private static Quaternion ToGameQuaternion(Quaternion rendererQuaternion)
        {
            return new Quaternion(
                rendererQuaternion.X,
                rendererQuaternion.Y,
                rendererQuaternion.Z,
                -rendererQuaternion.W);
        }

        private static Quaternion ToRendererQuaternion(Quaternion gameQuaternion)
        {
            return new Quaternion(
                gameQuaternion.X,
                gameQuaternion.Y,
                gameQuaternion.Z,
                -gameQuaternion.W);
        }

        private static Frame? GetAnimationFrame(Animation? animation, int frameID)
        {
            if (animation == null || animation.frames.Count == 0)
            {
                return null;
            }

            frameID %= animation.frames.Count;
            if (frameID < 0)
            {
                frameID += animation.frames.Count;
            }

            return animation.frames[frameID];
        }

        private struct BoneTransform
        {
            public Quaternion rotation;
            public Vector3 scale;
            public Vector3 translation;
        }

        private static BoneTransform BuildBoneTransform(
            MobyModel model,
            Frame? previousFrame,
            Frame? frame,
            int bone,
            float blend)
        {
            if (previousFrame == null || frame == null)
            {
                return new BoneTransform
                {
                    rotation = Quaternion.Identity,
                    scale = Vector3.One,
                    translation = model.boneDatas[bone].translation
                };
            }

            Quaternion baseRotation = previousFrame.GetRotationQuaternion(bone) ?? Quaternion.Identity;
            Quaternion nextRotation = frame.GetRotationQuaternion(bone) ?? Quaternion.Identity;
            Quaternion rotation = BlendQuaternion(baseRotation, nextRotation, blend);
            Vector3 baseScale = previousFrame.GetScaling(bone) ?? Vector3.One;
            Vector3 nextScale = frame.GetScaling(bone) ?? Vector3.One;
            Vector3 scaling = Vector3.Lerp(baseScale, nextScale, blend);
            Vector3 baseTranslation = previousFrame.GetTranslation(bone) ?? model.boneDatas[bone].translation;
            Vector3 nextTranslation = frame.GetTranslation(bone) ?? model.boneDatas[bone].translation;
            Vector3 translationVector = Vector3.Lerp(baseTranslation, nextTranslation, blend);

            return new BoneTransform
            {
                rotation = rotation,
                scale = scaling,
                translation = translationVector
            };
        }

        private static void BuildRuntimeSourcePose(
            MobyModel model,
            Moby.IngameMobyMemory.RuntimeAnimationData animationData,
            BoneTransform[] pose)
        {
            for (int bone = 0; bone < model.boneCount; bone++)
            {
                Quaternion rotation = bone < animationData.rotations.Length
                    ? animationData.rotations[bone]
                    : Quaternion.Identity;
                Vector3 scale = bone < animationData.hasScalings.Length && animationData.hasScalings[bone]
                    ? animationData.scalings[bone]
                    : Vector3.One;
                Vector3 translation = bone < animationData.hasTranslations.Length && animationData.hasTranslations[bone]
                    ? animationData.translations[bone]
                    : model.boneDatas[bone].translation;

                pose[bone] = new BoneTransform
                {
                    rotation = rotation,
                    scale = scale,
                    translation = translation
                };
            }

        }

        private static BoneTransform BlendBoneTransforms(
            BoneTransform previous,
            BoneTransform current,
            float blend)
        {
            return new BoneTransform
            {
                rotation = BlendQuaternion(
                    NormalizeQuaternion(previous.rotation),
                    NormalizeQuaternion(current.rotation),
                    blend),
                scale = Vector3.Lerp(previous.scale, current.scale, blend),
                translation = Vector3.Lerp(previous.translation, current.translation, blend)
            };
        }

        private static Matrix4 CreateTransform(Quaternion rotation, Vector3 scale, Vector3 translation)
        {
            Matrix4 transform = Matrix4.CreateFromQuaternion(rotation);
            transform.M11 *= scale.X;
            transform.M12 *= scale.X;
            transform.M13 *= scale.X;
            transform.M21 *= scale.Y;
            transform.M22 *= scale.Y;
            transform.M23 *= scale.Y;
            transform.M31 *= scale.Z;
            transform.M32 *= scale.Z;
            transform.M33 *= scale.Z;
            transform.M41 = translation.X;
            transform.M42 = translation.Y;
            transform.M43 = translation.Z;
            return transform;
        }

        private static void ApplyAnimationLayers(
            Moby.IngameMobyMemory memory,
            BoneTransform[] localBoneTransforms)
        {
            foreach (Moby.IngameMobyMemory.AnimationLayer layer in memory.animationLayers)
            {
                float blend = ClampBlend(layer.animationBlend);
                float inverseBlend = 1.0f - blend;

                foreach (Moby.IngameMobyMemory.AnimationData animationData in layer.animationData)
                {
                    int bone = (int) animationData.translation.W;
                    if (bone < 0 || bone >= localBoneTransforms.Length)
                    {
                        continue;
                    }

                    BoneTransform current = localBoneTransforms[bone];
                    Quaternion currentRotation = ToGameQuaternion(NormalizeQuaternion(current.rotation));
                    Quaternion layerRotation = new Quaternion(
                        animationData.rotation.X,
                        animationData.rotation.Y,
                        animationData.rotation.Z,
                        animationData.rotation.W);
                    Quaternion rotation = ToRendererQuaternion(BlendQuaternion(currentRotation, layerRotation, blend));
                    Vector3 scale = inverseBlend * current.scale + blend * new Vector3(
                        animationData.scale.X,
                        animationData.scale.Y,
                        animationData.scale.Z);
                    Vector3 translation = inverseBlend * current.translation + blend * new Vector3(
                        animationData.translation.X,
                        animationData.translation.Y,
                        animationData.translation.Z);

                    localBoneTransforms[bone] = new BoneTransform
                    {
                        rotation = rotation,
                        scale = scale,
                        translation = translation
                    };
                }
            }
        }

        private static void ApplyManipulators(
            Moby.IngameMobyMemory memory,
            BoneTransform[] localBoneTransforms)
        {
            foreach (Moby.IngameMobyMemory.AnimationManipulator manipulator in memory.manipulators)
            {
                uint boneOffset = unchecked((uint) manipulator.boneID) & 0x0fffffc0u;
                if (boneOffset % 0x40u != 0)
                {
                    continue;
                }

                uint boneIndex = boneOffset / 0x40u;
                if (boneIndex >= localBoneTransforms.Length)
                {
                    continue;
                }

                int bone = (int) boneIndex;
                float blend = manipulator.animationBlend;
                BoneTransform current = localBoneTransforms[bone];
                Quaternion currentRotation = ToGameQuaternion(current.rotation);
                Quaternion manipulatorRotation = new Quaternion(
                    manipulator.rotation.X,
                    manipulator.rotation.Y,
                    manipulator.rotation.Z,
                    manipulator.rotation.W);
                Vector3 currentScale = current.scale;
                Vector3 manipulatorScale = new Vector3(
                    manipulator.scale.X,
                    manipulator.scale.Y,
                    manipulator.scale.Z);
                Vector3 currentTranslation = current.translation;
                Vector3 manipulatorTranslation = (1.0f / 1024.0f) * new Vector3(
                    manipulator.translation.X,
                    manipulator.translation.Y,
                    manipulator.translation.Z);

                if (manipulator.absolute != 0)
                {
                    localBoneTransforms[bone] = new BoneTransform
                    {
                        rotation = ToRendererQuaternion(BlendQuaternion(currentRotation, manipulatorRotation, blend)),
                        scale = Vector3.Lerp(currentScale, manipulatorScale, blend),
                        translation = Vector3.Lerp(currentTranslation, manipulatorTranslation, blend)
                    };
                }
                else
                {
                    localBoneTransforms[bone] = new BoneTransform
                    {
                        rotation = ToRendererQuaternion(currentRotation * manipulatorRotation),
                        scale = currentScale * manipulatorScale,
                        translation = currentTranslation + manipulatorTranslation
                    };
                }
            }
        }

        private static void ComposeBoneHierarchy(
            MobyModel model,
            BoneTransform[] localBoneTransforms,
            Matrix4[] boneMatrices)
        {
            for (int i = 0; i < model.boneCount; i++)
            {
                int parent = model.boneDatas[i].parent;
                Matrix4 parentMatrix = (i == 0 || parent < 0 || parent >= i)
                    ? Matrix4.Identity
                    : boneMatrices[parent];

                BoneTransform localBoneTransform = localBoneTransforms[i];
                Matrix4 localBoneMatrix = CreateTransform(
                    localBoneTransform.rotation,
                    localBoneTransform.scale,
                    localBoneTransform.translation);
                boneMatrices[i] = localBoneMatrix * parentMatrix;
            }
        }

        private static void ApplyInverseBindMatrices(MobyModel model, Matrix4[] boneMatrices)
        {
            for (int i = 0; i < model.boneCount; i++)
            {
                boneMatrices[i] = model.boneMatrices[i].GetInvBindMatrix(true) * boneMatrices[i];
            }
        }

        private void ComputeBoneMatricesWithMemory(
            MobyModel mobyModel,
            List<Animation> animations,
            Moby.IngameMobyMemory memory,
            float deltaTime)
        {
            float blend = ClampBlend(memory.animationBlend);
            bool hasRuntimeAnimationData = memory.previousAnimationData != null && memory.currentAnimationData != null;
            if (hasRuntimeAnimationData)
            {
                BuildRuntimeSourcePose(
                    mobyModel,
                    memory.previousAnimationData!,
                    runtimePreviousPose!);

                BuildRuntimeSourcePose(
                    mobyModel,
                    memory.currentAnimationData!,
                    runtimeCurrentPose!);
            }
            else
            {
                int animationID = memory.animationID;
                Animation? anim = (animationID >= 0 && animationID < animations.Count) ? animations[animationID] : null;
                Frame? frame = GetAnimationFrame(anim, memory.animationFrame);
                Frame? runtimePreviousFrame = null;

                if (memory.updateID == byte.MaxValue)
                {
                    int previousAnimationID = memory.previousAnimationID;
                    int transitionCacheKey = (previousAnimationID << 8) | memory.previousAnimationFrame;
                    if (runtimeTransitionCacheKey != transitionCacheKey)
                    {
                        runtimeTransitionCacheKey = transitionCacheKey;
                        runtimeTransitionPoseValid = runtimePoseValid;
                        if (runtimePoseValid)
                        {
                            Array.Copy(runtimePose!, runtimeTransitionPose!, mobyModel.boneCount);
                        }
                    }
                }
                else
                {
                    runtimeTransitionCacheKey = -1;
                    runtimeTransitionPoseValid = false;
                    Animation? previousAnim = (memory.updateID < animations.Count)
                        ? animations[memory.updateID]
                        : null;
                    runtimePreviousFrame = GetAnimationFrame(previousAnim, memory.previousAnimationFrame);
                }

                previousFrame = runtimePreviousFrame ?? frame;
                currentFrame = frame;

                if (previousFrame == null && frame != null)
                {
                    previousFrame = frame;
                }
            }

            for (int i = 0; i < mobyModel.boneCount; i++)
            {
                if (hasRuntimeAnimationData)
                {
                    localBoneTransforms![i] = BlendBoneTransforms(
                        runtimePreviousPose![i],
                        runtimeCurrentPose![i],
                        blend);
                }
                else if (memory.updateID == byte.MaxValue && runtimeTransitionPoseValid)
                {
                    BoneTransform current = BuildBoneTransform(mobyModel, currentFrame, currentFrame, i, 1.0f);
                    localBoneTransforms![i] = BlendBoneTransforms(runtimeTransitionPose![i], current, blend);
                }
                else
                {
                    localBoneTransforms![i] = BuildBoneTransform(mobyModel, previousFrame, currentFrame, i, blend);
                }
            }

            Array.Copy(localBoneTransforms!, runtimePose!, mobyModel.boneCount);
            runtimePoseValid = true;

            ApplyAnimationLayers(memory, localBoneTransforms!);
            ApplyManipulators(memory, localBoneTransforms!);

            ComposeBoneHierarchy(mobyModel, localBoneTransforms!, boneMatrices!);
            ApplyInverseBindMatrices(mobyModel, boneMatrices!);
        }

        private void ComputeBoneMatricesWithoutMemory(
            MobyModel mobyModel,
            List<Animation> animations,
            int animationID,
            float deltaTime)
        {
            if (animationID != currentAnimationID)
            {
                currentAnimationID = animationID;
                currentFrameID = 0;
                frameBlend = 0.0f;
            }

            Animation? anim = (animationID >= 0 && animationID < animations.Count) ? animations[animationID] : null;
            Frame? frame = GetAnimationFrame(anim, currentFrameID);

            if (anim != null && frame != null)
            {
                float frameSpeed = (anim.speed != 0.0f) ? anim.speed : frame.speed;

                if (frameSpeed == 0.0f || float.IsNaN(frameSpeed) || float.IsInfinity(frameSpeed))
                {
                    frameBlend = 1.0f;
                }
                else
                {
                    frameBlend += deltaTime * frameSpeed * 60.0f;
                }
            }

            if (frame != currentFrame)
            {
                previousFrame = (currentFrame != null) ? currentFrame : frame;
                currentFrame = frame;
            }

            if (previousFrame == null && frame != null)
            {
                previousFrame = frame;
            }

            float blend = ClampBlend(frameBlend);
            for (int i = 0; i < mobyModel.boneCount; i++)
            {
                localBoneTransforms![i] = BuildBoneTransform(mobyModel, previousFrame, frame, i, blend);
            }

            if (frame != null && previousFrame != null)
            {
                ComposeBoneHierarchy(mobyModel, localBoneTransforms!, boneMatrices!);
                ApplyInverseBindMatrices(mobyModel, boneMatrices!);
            }
            else
            {
                for (int i = 0; i < boneMatrices!.Length; i++)
                {
                    boneMatrices[i] = Matrix4.Identity;
                }
            }

            while (frameBlend >= 1.0f && anim != null)
            {
                frameBlend -= 1.0f;
                currentFrameID++;
                if (currentFrameID >= anim.frames.Count)
                {
                    currentFrameID = 0;
                }
            }
        }

        public override void Render(RendererPayload payload)
        {
            if ((mob == null || mob.model == null) && mobyModelStandalone == null) return;

            UpdateVars();

            mobyAlpha = 1.0f;
            if (mob != null && mob.memory != null)
            {
                mobyAlpha = mob.memory.alpha / 128.0f;
            }

            if (emptyModel || gpuData == null) return;

            if (ComputeCulling(payload.camera, payload.visibility.enableDistanceCulling, payload.visibility.enableVisibleCulling)) return;

            MobyModel? mobyModel = (MobyModel?) mob?.model ?? mobyModelStandalone;

            if (mobyModel == null) return;

            Select(payload.selection);

            shaderTable.animationShader.UseShader();

            Matrix4 modelToWorld = (mob != null) ? mob.modelMatrix : Matrix4.Identity;
            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            Matrix4 viewMatrix = payload.camera.GetViewMatrix();
            shaderTable.animationShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
            shaderTable.animationShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.animationShader.SetUniformMatrix4(UniformName.viewMatrix, ref viewMatrix);
            shaderTable.animationShader.SetUniform1(UniformName.useMetalShading, 0);
            shaderTable.animationShader.SetUniform1(UniformName.mainTexture, 0);
            shaderTable.animationShader.SetUniform1(UniformName.blueNoiseTexture, 1);
            shaderTable.animationShader.SetUniform1(UniformName.ssaaLevelLog, FramebufferRenderer.SSAA_LEVEL_LOG);
            shaderTable.animationShader.SetUniform1(UniformName.levelObjectNumber, (mob != null) ? mob.globalID : 0);
            if (selected)
            {
                shaderTable.animationShader.SetUniform4(UniformName.staticColor, SELECTED_COLOR);
            }
            else
            {
                shaderTable.animationShader.SetUniform4(UniformName.staticColor, ambient);
            }
            shaderTable.animationShader.SetUniform1(UniformName.lightIndex, light);
            shaderTable.animationShader.SetUniform1(UniformName.objectBlendDistance, blendDistance);
            shaderTable.animationShader.SetUniform1(UniformName.mobyAlpha, mobyAlpha);

            GLTexture.blueNoiseTexture.Bind(1);

            List<Animation> animations = (loadedModelID == 0 && ratchetAnimations != null && ratchetAnimations.Count > 0) ? ratchetAnimations : mobyModel.animations;
            if (mob != null && mob.memory != null)
            {
                ComputeBoneMatricesWithMemory(mobyModel, animations, mob.memory, payload.deltaTime);
            }
            else
            {
                ComputeBoneMatricesWithoutMemory(mobyModel, animations, payload.forcedAnimationID, payload.deltaTime);
            }

            shaderTable.animationShader.SetUniformMatrix4(UniformName.bones, mobyModel.boneCount, ref boneMatrices![0].Row0.X);

            RenderModel(mobyModel, gpuData);

            int subModelCount = mobyModel.GetSubModelCount();
            for (int i = 0; i < subModelCount; i++)
            {
                if ((payload.visibility.subModelsMask & (1u << i)) == 0) continue;

                Model? subModel = mobyModel.GetSubModel(i);
                ModelGPUData? modelGPUData = gpuData.subModels[i];

                if (subModel == null || modelGPUData == null) continue;

                RenderModel(subModel, modelGPUData);
            }

            GLUtil.CheckGlError("AnimationRenderer");
        }

        public override void Dispose()
        {
            DeleteBuffers();
        }
    }
}
