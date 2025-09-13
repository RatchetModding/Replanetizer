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

        private bool emptyModel = false;

        private int ibo = 0;
        private int vbo = 0;
        private int vao = 0;

        private List<int> subModelIBOs = new List<int>();
        private List<int> subModelVBOs = new List<int>();
        private List<int> subModelVAOs = new List<int>();

        private bool iboAllocated = false;
        private bool vboAllocated = false;
        private bool vaoAllocated = false;

        private int light { get; set; }
        private Rgb24 ambient { get; set; }
        private float renderDistance { get; set; }
        private static Vector4 SELECTED_COLOR = new Vector4(2.0f, 2.0f, 2.0f, 1.0f);

        private bool selected;
        private float blendDistance = 0.0f;

        private List<Texture> textures;
        private Dictionary<Texture, GLTexture> textureIds;
        private ShaderTable shaderTable;

        // The Ratchet moby does not contain its own animations.
        private List<Animation>? ratchetAnimations = null;

        private int currentFrameID = 0;
        private int currentAnimationID = 0;
        private Frame? currentFrame = null;
        private Frame? previousFrame = null;
        private float frameBlend = 0.0f;

        public AnimationRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, GLTexture> textureIds, List<Animation>? ratchetAnimations = null)
        {
            this.shaderTable = shaderTable;
            this.textureIds = textureIds;
            this.textures = textures;
            this.ratchetAnimations = ratchetAnimations;
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

            foreach (int subModelIBO in subModelIBOs)
            {
                if (subModelIBO != -1)
                {
                    GL.DeleteBuffer(subModelIBO);
                }
            }

            foreach (int subModelVBO in subModelVBOs)
            {
                if (subModelVBO != -1)
                {
                    GL.DeleteBuffer(subModelVBO);
                }
            }

            foreach (int subModelVAO in subModelVAOs)
            {
                if (subModelVAO != -1)
                {
                    GL.DeleteVertexArray(subModelVAO);
                }
            }

            subModelVAOs.Clear();
            subModelIBOs.Clear();
            subModelVBOs.Clear();
        }

        public bool IsValid()
        {
            UpdateVars();
            return (emptyModel) ? false : true;
        }

        /// <summary>
        /// Sets up the vertex attribute pointers according to the type.
        /// </summary>
        private void SetupVertexAttribPointers()
        {
            GLUtil.ActivateNumberOfVertexAttribArrays(5);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 40, 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 40, 12);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 40, 24);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, false, 40, 32);
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.UnsignedByte, true, 40, 36);
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
                emptyModel = true;
                return;
            }

            if (mobyModel.GetIndices().Length == 0)
            {
                emptyModel = true;
                return;
            }

            if (mobyModel.boneCount == 0)
            {
                emptyModel = true;
                return;
            }

            // This is a camera object that only exist at runtime and blocks vision in interactive mode.
            // We simply don't draw it.
            if (loadedModelID == 0x3EF)
            {
                loadedModelID = -1;
                emptyModel = true;
                mob = null;
                return;
            }

            emptyModel = false;

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // IBO
            int iboLength = mobyModel.GetIndices().Length * sizeof(ushort);
            if (iboLength > 0)
            {
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboLength, IntPtr.Zero, BufferUsageHint.StaticDraw);
                iboAllocated = true;
            }

            // VBO
            int vboLength = mobyModel.GetVertices().Length * sizeof(float);
            vboLength += mobyModel.vertexBoneIds.Length * sizeof(uint);
            vboLength += mobyModel.vertexBoneWeights.Length * sizeof(uint);
            if (vboLength > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboLength, IntPtr.Zero, BufferUsageHint.StaticDraw);
                vboAllocated = true;
            }

            UpdateBuffers(mobyModel, vao);

            // Initialize all submodels aswell, we can then dynamically decide to draw them or not during rendering.
            int subModelCount = mobyModel.GetSubModelCount();
            for (int i = 0; i < subModelCount; i++)
            {
                Model? subModel = mobyModel.GetSubModel(i);

                if (subModel == null)
                    continue;

                int subModelVao;
                GL.GenVertexArrays(1, out subModelVao);
                GL.BindVertexArray(subModelVao);

                // IBO
                int subModelIboLength = subModel.GetIndices().Length * sizeof(ushort);
                int subModelIbo = -1;
                if (subModelIboLength > 0)
                {
                    GL.GenBuffers(1, out subModelIbo);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, subModelIbo);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, subModelIboLength, IntPtr.Zero, BufferUsageHint.StaticDraw);
                }

                // VBO
                int subModelVboLength = subModel.GetVertices().Length * sizeof(float);
                subModelVboLength += subModel.vertexBoneIds.Length * sizeof(uint);
                subModelVboLength += subModel.vertexBoneWeights.Length * sizeof(uint);
                int subModelVbo = -1;
                if (subModelVboLength > 0)
                {
                    GL.GenBuffers(1, out subModelVbo);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, subModelVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, subModelVboLength, IntPtr.Zero, BufferUsageHint.StaticDraw);
                }

                subModelVAOs.Add(subModelVao);
                subModelIBOs.Add(subModelIbo);
                subModelVBOs.Add(subModelVbo);

                UpdateBuffers(subModel, subModelVao);
            }
        }

        /// <summary>
        /// Updates the buffers. This is not actually needed as long as mesh manipulations are not possible.
        /// </summary>
        private void UpdateBuffers(Model model, int modelVAO)
        {
            GL.BindVertexArray(modelVAO);

            ushort[] iboData = model.GetIndices();
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, iboData.Length * sizeof(ushort), iboData);

            float[] vboData = model.GetVertices();
            uint[] boneIDs = model.vertexBoneIds;
            uint[] boneWeights = model.vertexBoneWeights;

            float[] fullData = new float[vboData.Length + boneIDs.Length + boneWeights.Length];

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
                fullData[10 * i + 8] = BitConverter.UInt32BitsToSingle(boneIDs[i]);
                fullData[10 * i + 9] = BitConverter.UInt32BitsToSingle(boneWeights[i]);
            }
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, fullData.Length * sizeof(float), fullData);

            SetupVertexAttribPointers();
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
                renderDistance = mob.drawDistance;
                modelID = mob.modelID;
            }
            else if (mobyModelStandalone != null)
            {
                light = -1;
                ambient = Color.FromRgb(255, 255, 255).ToPixel<Rgb24>();
                modelID = mobyModelStandalone.id;
            }

            if (modelID != -1)
            {
                if (loadedModelID != modelID)
                {
                    previousFrame = null;
                    GenerateBuffers();
                }
            }
        }

        /// <summary>
        /// Takes a textureConfig mode as input and sets the transparency mode based on that.
        /// </summary>
        private void SetTransparencyMode(TextureConfig config)
        {
            shaderTable.animationShader.SetUniform1(UniformName.useTransparency, (config.IgnoresTransparency()) ? 0 : 1);
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
        private bool ComputeCulling(Camera camera, bool distanceCulling)
        {
            if (emptyModel) return true;
            if (mobyModelStandalone != null) return false;
            if (mob == null) return true;

            if (distanceCulling)
            {
                float dist = (mob.position - camera.position).Length;

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

            return false;
        }

        private void RenderModel(Model model, int modelVAO)
        {
            GL.BindVertexArray(modelVAO);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in model.textureConfig)
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
        }

        public override void Render(RendererPayload payload)
        {
            if (((mob == null || mob.model == null || mob.memory == null) && mobyModelStandalone == null)) return;

            UpdateVars();

            if (emptyModel)
            {
                return;
            }

            if (ComputeCulling(payload.camera, payload.visibility.enableDistanceCulling)) return;

            MobyModel? mobyModel = (MobyModel?) mob?.model ?? mobyModelStandalone;

            if (mobyModel == null) return;

            Select(payload.selection);

            shaderTable.animationShader.UseShader();

            Matrix4 modelToWorld = (mob != null) ? mob.modelMatrix : Matrix4.Identity;
            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();
            shaderTable.animationShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
            shaderTable.animationShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
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

            int animationID = (mob != null && mob.memory != null) ? mob.memory.animationID : payload.forcedAnimationID;

            if (animationID != currentAnimationID)
            {
                currentAnimationID = animationID;
                currentFrameID = 0;
                frameBlend = 0.0f;
            }

            Matrix4[] boneMatrices = new Matrix4[mobyModel.boneCount];

            List<Animation> animations = (loadedModelID == 0 && ratchetAnimations != null && ratchetAnimations.Count > 0) ? ratchetAnimations : mobyModel.animations;

            Animation? anim = (animationID >= 0 && animationID < animations.Count) ? animations[animationID] : null;

            int animationFrame = (mob != null && mob.memory != null) ? mob.memory.animationFrame : currentFrameID;

            // For Example: RaC 1 bomb glove idles in the last frame of the animation despite the first one being selected.
            // TODO: Understand what is happening in these cases.
            if (anim != null && mob != null && mob.memory != null)
            {
                animationFrame--;
                if (animationFrame < 0)
                {
                    animationFrame += anim.frames.Count;
                }
            }

            Frame? frame = (anim != null && animationFrame >= 0 && animationFrame < anim.frames.Count) ? anim.frames[animationFrame] : null;

            if (anim != null && frame != null)
            {
                float frameSpeed = (anim.speed != 0.0f) ? anim.speed : frame.speed;
                frameBlend += payload.deltaTime * frameSpeed * 60.0f;

                // If frameSpeed is zero then no interpolation is used and thus always display exactly the current frame.
                if (frameSpeed == 0.0f)
                {
                    frameBlend = 1.0f;
                }
            }

            if (frame != currentFrame)
            {
                frameBlend = 0.0f;
                previousFrame = (currentFrame != null) ? currentFrame : frame;
                currentFrame = frame;
            }

            if (frame != null && previousFrame != null)
            {
                float blend = frameBlend;

                if (blend > 1.0f) blend = 1.0f;
                if (blend < 0.0f) blend = 0.0f;

                for (int i = 0; i < mobyModel.boneCount; i++)
                {
                    Matrix4 animationMatrix = previousFrame.GetRotationMatrix(i, frame, blend);
                    Vector3? scaling = previousFrame.GetScaling(i, frame, blend);
                    Vector3? translation = previousFrame.GetTranslation(i, frame, blend);

                    // Translations replace the bone data translation
                    Vector3 translationVector = (translation != null) ? (Vector3) translation : mobyModel.boneDatas[i].translation;

                    animationMatrix.M41 = translationVector.X;
                    animationMatrix.M42 = translationVector.Y;
                    animationMatrix.M43 = translationVector.Z;

                    if (scaling != null)
                    {
                        Vector3 s = (Vector3) scaling;
                        animationMatrix.M11 *= s.X;
                        animationMatrix.M12 *= s.X;
                        animationMatrix.M13 *= s.X;
                        animationMatrix.M21 *= s.Y;
                        animationMatrix.M22 *= s.Y;
                        animationMatrix.M23 *= s.Y;
                        animationMatrix.M31 *= s.Z;
                        animationMatrix.M32 *= s.Z;
                        animationMatrix.M33 *= s.Z;
                    }

                    Matrix4 parentMatrix = (i == 0) ? Matrix4.Identity : boneMatrices[mobyModel.boneDatas[i].parent];

                    boneMatrices[i] = animationMatrix * parentMatrix;
                }

                for (int i = 0; i < mobyModel.boneCount; i++)
                {
                    boneMatrices[i] = mobyModel.boneMatrices[i].GetInvBindMatrix(true) * boneMatrices[i];
                }
            }
            else
            {
                // Animation is not present in Replanetizer (like in Cutscenes)

                for (int i = 0; i < mobyModel.boneCount; i++)
                {
                    boneMatrices[i] = Matrix4.Identity;
                }
            }

            // This causes the animation in the model viewer to loop.
            while (frameBlend >= 1.0f && mob == null && anim != null)
            {
                frameBlend -= 1.0f;
                currentFrameID++;
                if (currentFrameID >= anim.frames.Count)
                {
                    currentFrameID = 0;
                }
            }

            shaderTable.animationShader.SetUniformMatrix4(UniformName.bones, mobyModel.boneCount, ref boneMatrices[0].Row0.X);

            RenderModel(mobyModel, vao);

            int subModelCount = mobyModel.GetSubModelCount();
            for (int i = 0; i < subModelCount; i++)
            {
                if (payload.visibility.subModels[i] == false) continue;

                Model? subModel = mobyModel.GetSubModel(i);

                if (subModel == null) continue;

                RenderModel(subModel, subModelVAOs[i]);
            }

            GLUtil.CheckGlError("AnimationRenderer");
        }

        public override void Dispose()
        {
            DeleteBuffers();
        }
    }
}
