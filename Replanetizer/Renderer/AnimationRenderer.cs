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
using System.Drawing;
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

        private int loadedModelID = -1;

        private bool emptyModel = false;

        private int ibo = 0;
        private int vbo = 0;
        private int vao = 0;

        private bool iboAllocated = false;
        private bool vboAllocated = false;
        private bool vaoAllocated = false;

        private int light { get; set; }
        private Color ambient { get; set; }
        private float renderDistance { get; set; }
        private static Vector4 SELECTED_COLOR = new Vector4(2.0f, 2.0f, 2.0f, 1.0f);

        private bool selected;
        private float blendDistance = 0.0f;

        private List<Texture> textures;
        private Dictionary<Texture, GLTexture> textureIds;
        private ShaderTable shaderTable;

        private Frame? previousFrame = null;

        public AnimationRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, GLTexture> textureIds)
        {
            this.shaderTable = shaderTable;
            this.textureIds = textureIds;
            this.textures = textures;
        }

        public override void Include<T>(T obj)
        {
            if (obj is Moby mob)
            {
                this.mob = mob;

                GenerateBuffers();
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

            if (mob == null)
            {
                emptyModel = true;
                return;
            }

            loadedModelID = mob.modelID;

            if (mob.GetIndices().Length == 0)
            {
                emptyModel = true;
                return;
            }

            if (mob.model == null)
            {
                emptyModel = true;
                return;
            }

            MobyModel mobyModel = (MobyModel) mob.model;

            if (mobyModel.boneCount == 0)
            {
                emptyModel = true;
                return;
            }

            // This is a camera object that only exist at runtime and blocks vision in interactive mode.
            // We simply don't draw it.
            if (mob.modelID == 0x3EF)
            {
                loadedModelID = -1;
                emptyModel = true;
                mob = null;
                return;
            }

            emptyModel = false;

            BufferUsageHint hint = BufferUsageHint.StaticDraw;
            if (mob.IsDynamic())
            {
                hint = BufferUsageHint.DynamicDraw;
            }

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // IBO
            int iboLength = mob.GetIndices().Length * sizeof(ushort);
            if (iboLength > 0)
            {
                GL.GenBuffers(1, out ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, iboLength, IntPtr.Zero, hint);
                iboAllocated = true;
            }

            // VBO
            int vboLength = mob.GetVertices().Length * sizeof(float);
            vboLength += mobyModel.ids.Length * sizeof(uint);
            vboLength += mobyModel.weights.Length * sizeof(uint);
            if (vboLength > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboLength, IntPtr.Zero, hint);
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
            if (mob == null || mob.model == null)
                return;

            MobyModel mobyModel = (MobyModel) mob.model;

            GL.BindVertexArray(vao);

            ushort[] iboData = mob.GetIndices();
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, iboData.Length * sizeof(ushort), iboData);

            float[] vboData = mob.GetVertices();
            uint[] boneIDs = mobyModel.ids;
            uint[] boneWeights = mobyModel.weights;

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
        }

        /// <summary>
        /// Updates the light and ambient variables which can then be used to update the shader. Check if the modelID
        /// has changed and update the buffers if necessary.
        /// </summary>
        private void UpdateVars()
        {
            if (mob == null)
                return;

            light = Math.Max(0, Math.Min(ALLOCATED_LIGHTS, mob.light));
            ambient = mob.color;
            renderDistance = mob.drawDistance;

            if (loadedModelID != mob.modelID)
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
            shaderTable.animationShader.SetUniform1("useTransparency", (config.IgnoresTransparency()) ? 0 : 1);
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

        public override void Render(RendererPayload payload)
        {
            if (mob == null || mob.model == null || mob.memory == null) return;

            UpdateVars();

            if (emptyModel)
            {
                return;
            }

            if (ComputeCulling(payload.camera, payload.visibility.enableDistanceCulling)) return;

            MobyModel mobyModel = (MobyModel) mob.model;

            GL.BindVertexArray(vao);

            Select(payload.selection);

            shaderTable.animationShader.UseShader();

            Matrix4 worldToView = (payload == null) ? Matrix4.Identity : payload.camera.GetWorldViewMatrix();
            shaderTable.animationShader.SetUniformMatrix4("modelToWorld", ref mob.modelMatrix);
            shaderTable.animationShader.SetUniformMatrix4("worldToView", ref worldToView);
            shaderTable.animationShader.SetUniform1("levelObjectNumber", mob.globalID);
            if (selected)
            {
                shaderTable.animationShader.SetUniform4("staticColor", SELECTED_COLOR);
            }
            else
            {
                shaderTable.animationShader.SetUniform4("staticColor", ambient);
            }
            shaderTable.animationShader.SetUniform1("lightIndex", light);
            shaderTable.animationShader.SetUniform1("objectBlendDistance", blendDistance);

            int animationID = mob.memory.animationID;

            Matrix4[] boneMatrices = new Matrix4[mobyModel.boneCount];

            Animation? anim = (animationID >= 0 && animationID < mobyModel.animations.Count) ? mobyModel.animations[animationID] : null;

            int animationFrame = mob.memory.animationFrame;

            Frame? frame = (anim != null && anim.frames.Count > animationFrame) ? anim.frames[animationFrame] : null;

            if (frame != null && previousFrame != null)
            {
                float blend = mob.memory.animationBlending;

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
                    BoneMatrix boneMatrix = mobyModel.boneMatrices[i];

                    Vector3 off = boneMatrix.cumulativeOffset;

                    Matrix3x4 origTrans = boneMatrix.transformation;
                    Matrix3 mat = new Matrix3(origTrans.Row0.Xyz, origTrans.Row1.Xyz, origTrans.Row2.Xyz);

                    Matrix4 invBindMatrix = new Matrix4();
                    invBindMatrix.M11 = mat.M11;
                    invBindMatrix.M12 = mat.M12;
                    invBindMatrix.M13 = mat.M13;
                    invBindMatrix.M14 = 0.0f;
                    invBindMatrix.M21 = mat.M21;
                    invBindMatrix.M22 = mat.M22;
                    invBindMatrix.M23 = mat.M23;
                    invBindMatrix.M24 = 0.0f;
                    invBindMatrix.M31 = mat.M31;
                    invBindMatrix.M32 = mat.M32;
                    invBindMatrix.M33 = mat.M33;
                    invBindMatrix.M34 = 0.0f;
                    invBindMatrix.M41 = off.X;
                    invBindMatrix.M42 = off.Y;
                    invBindMatrix.M43 = off.Z;
                    invBindMatrix.M44 = 1.0f;

                    boneMatrices[i] = invBindMatrix * boneMatrices[i];
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

            previousFrame = frame;

            shaderTable.animationShader.SetUniformMatrix4("bones", mobyModel.boneCount, ref boneMatrices[0].Row0.X);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in mobyModel.textureConfig)
            {
                if (conf.id > 0)
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

            GLUtil.CheckGlError("AnimationRenderer");
        }

        public override void Dispose()
        {
            DeleteBuffers();
        }
    }
}
