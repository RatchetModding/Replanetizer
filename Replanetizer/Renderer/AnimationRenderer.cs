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
        private MobyModel? model;

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
        private Dictionary<Texture, int> textureIds;
        private ShaderTable shaderTable;

        public AnimationRenderer(ShaderTable shaderTable, List<Texture> textures, Dictionary<Texture, int> textureIds)
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

            model = (MobyModel?) mob.model;

            if (model == null || model.boneCount == 0)
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
            vboLength += model.ids.Length * sizeof(uint);
            vboLength += model.weights.Length * sizeof(uint);
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
            if (mob == null || model == null)
                return;

            GL.BindVertexArray(vao);

            ushort[] iboData = mob.GetIndices();
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, iboData.Length * sizeof(ushort), iboData);

            float[] vboData = mob.GetVertices();
            uint[] boneIDs = model.ids;
            uint[] boneWeights = model.weights;

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
        private void SetTextureWrapMode(TextureConfig conf)
        {
            /*
             * There is an issue with opaque edges in some transparent objects
             * This can easily be observed on RaC 1 Kerwan where you have these ugly edges on some trees and the bottom
             * of the fading out buildings.
             */

            switch (conf.wrapModeS)
            {
                case TextureConfig.WrapMode.Repeat:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float) TextureWrapMode.Repeat);
                    break;
                case TextureConfig.WrapMode.ClampEdge:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float) TextureWrapMode.ClampToEdge);
                    break;
                default:
                    break;
            }

            switch (conf.wrapModeT)
            {
                case TextureConfig.WrapMode.Repeat:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float) TextureWrapMode.Repeat);
                    break;
                case TextureConfig.WrapMode.ClampEdge:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float) TextureWrapMode.ClampToEdge);
                    break;
                default:
                    break;
            }
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
            if (mob == null || model == null || mob.memory == null) return;

            if (emptyModel)
            {
                return;
            }

            UpdateVars();
            if (ComputeCulling(payload.camera, payload.visibility.enableDistanceCulling)) return;

            GL.BindVertexArray(vao);

            Select(payload.selection);

            shaderTable.animationShader.UseShader();

            Matrix4 worldToView = (payload == null) ? Matrix4.Identity : payload.camera.GetWorldViewMatrix();
            shaderTable.animationShader.SetUniformMatrix4("modelToWorld", false, ref mob.modelMatrix);
            shaderTable.animationShader.SetUniformMatrix4("worldToView", false, ref worldToView);
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
            int animationFrame = mob.memory.animationFrame;

            Frame frame = model.animations[animationID].frames[animationFrame];

            int boneCount = model.boneCount;
            Matrix4[] boneMatrices = new Matrix4[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                BoneMatrix boneMatrix = model.boneMatrices[i];

                Matrix3x4 origTrans = boneMatrix.transformation;
                Matrix3 mat = new Matrix3(origTrans.Row0.Xyz, origTrans.Row1.Xyz, origTrans.Row2.Xyz);
                mat.Transpose();

                Matrix4 invBindMatrix = new Matrix4();
                invBindMatrix.M11 = mat.M11;
                invBindMatrix.M12 = mat.M12;
                invBindMatrix.M13 = mat.M13;
                invBindMatrix.M14 = boneMatrix.transformation.M14 * model.size / 1024.0f;
                invBindMatrix.M21 = mat.M21;
                invBindMatrix.M22 = mat.M22;
                invBindMatrix.M23 = mat.M23;
                invBindMatrix.M24 = boneMatrix.transformation.M24 * model.size / 1024.0f;
                invBindMatrix.M31 = mat.M31;
                invBindMatrix.M32 = mat.M32;
                invBindMatrix.M33 = mat.M33;
                invBindMatrix.M34 = boneMatrix.transformation.M34 * model.size / 1024.0f;
                invBindMatrix.M41 = 0.0f;
                invBindMatrix.M42 = 0.0f;
                invBindMatrix.M43 = 0.0f;
                invBindMatrix.M44 = 1.0f;


                Matrix4 animationMatrix = frame.GetRotationMatrix(i);
                animationMatrix.Transpose();
                Vector3? scaling = frame.GetScaling(i);
                Vector3? translation = frame.GetTranslation(i);

                // Translations replace the bone data translation
                Vector3 translationVector = (translation != null) ? (Vector3) translation : model.boneDatas[i].translation;
                translationVector *= model.size;

                animationMatrix.M14 = translationVector.X;
                animationMatrix.M24 = translationVector.Y;
                animationMatrix.M34 = translationVector.Z;

                if (scaling != null)
                {
                    Vector3 s = (Vector3) scaling;
                    animationMatrix.M11 *= s.X;
                    animationMatrix.M21 *= s.X;
                    animationMatrix.M31 *= s.X;
                    animationMatrix.M12 *= s.Y;
                    animationMatrix.M22 *= s.Y;
                    animationMatrix.M32 *= s.Y;
                    animationMatrix.M13 *= s.Z;
                    animationMatrix.M23 *= s.Z;
                    animationMatrix.M33 *= s.Z;
                }

                boneMatrices[i] = animationMatrix * invBindMatrix;
            }

            shaderTable.animationShader.SetUniformMatrix4("bones", boneCount, true, ref boneMatrices[0].Row0.X);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in model.textureConfig)
            {
                GL.BindTexture(TextureTarget.Texture2D, (conf.id > 0) ? textureIds[textures[conf.id]] : 0);
                SetTransparencyMode(conf);
                SetTextureWrapMode(conf);
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
