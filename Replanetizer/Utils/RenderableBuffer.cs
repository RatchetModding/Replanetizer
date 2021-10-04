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
    public class RenderableBuffer
    {
        private ModelObject modelObject;

        private int ibo = 0;
        private int vbo = 0;

        public int ID { get; }
        public RenderedObjectType type { get; }
        public int light { get; set; }
        public Color ambient { get; set; }
        public float renderDistance { get; set; }

        private bool selected;
        private bool culled;

        private Level level;
        private Dictionary<Texture, int> textureIds;

        public static ShaderIDTable shaderIDTable { get; set; }

        public RenderableBuffer(ModelObject modelObject, RenderedObjectType type, int ID, Level level, Dictionary<Texture, int> textureIds)
        {
            this.modelObject = modelObject;
            this.ID = ID;
            this.type = type;
            this.textureIds = textureIds;
            this.level = level;

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
            }

            // VBO
            int vboLength = modelObject.GetVertices().Length * sizeof(float);
            if (type == RenderedObjectType.Terrain || type == RenderedObjectType.Tie) vboLength += modelObject.GetAmbientRGBAs().Length * sizeof(Byte);
            if (vboLength > 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vboLength, IntPtr.Zero, hint);
            }

            UpdateBuffers();
            UpdateVars();
        }

        /// <summary>
        /// Updates the buffers. This is not actually needed as long as mesh manipulations are not possible.
        /// </summary>
        public void UpdateBuffers()
        {
            if (BindIBO())
            {
                ushort[] iboData = modelObject.GetIndices();
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, iboData.Length * sizeof(ushort), iboData);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }

            if (BindVBO())
            {
                float[] vboData = modelObject.GetVertices();
                if (type == RenderedObjectType.Terrain || type == RenderedObjectType.Tie)
                {
                    byte[] rgbas = modelObject.GetAmbientRGBAs();
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
                }
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vboData.Length * sizeof(float), vboData);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        /// <summary>
        /// Updates the light and ambient variables which can then be used to update the shader.
        /// </summary>
        public void UpdateVars()
        {
            switch (type)
            {
                case RenderedObjectType.Terrain:
                    TerrainModel terrain = (TerrainModel) modelObject.model;
                    light = (int) terrain.lights[0];
                    renderDistance = float.MaxValue;
                    break;
                case RenderedObjectType.Moby:
                    Moby mob = (Moby) modelObject;
                    light = mob.light;
                    ambient = mob.color;
                    renderDistance = mob.drawDistance;
                    break;
                case RenderedObjectType.Tie:
                    Tie tie = (Tie) modelObject;
                    light = tie.light;
                    renderDistance = float.MaxValue;
                    break;
                case RenderedObjectType.Shrub:
                    Shrub shrub = (Shrub) modelObject;
                    light = shrub.light;
                    ambient = shrub.color;
                    renderDistance = shrub.drawDistance;
                    break;
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
        /// Sets an internal variable to true if the object is to be culled.
        /// Mobies and shrubs are culled by their drawDistance.
        /// (TODO) Ties and terrain is culled by frustum culling.
        /// </summary>
        public void ComputeCulling(Camera camera, bool distanceCulling, bool frustumCulling)
        {
            if (distanceCulling)
            {
                float dist = (modelObject.position - camera.position).Length;

                if (dist > renderDistance)
                {
                    culled = true;
                    return;
                }
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
                            ShrubModel shrub = (ShrubModel) modelObject.model;
                            center = new Vector3(shrub.cullingX, shrub.cullingY, shrub.cullingZ);
                            center = modelObject.rotation * center;
                            center += modelObject.position;
                            float shrubScale = MathF.MaxMagnitude(modelObject.scale.X, MathF.MaxMagnitude(modelObject.scale.Y, modelObject.scale.Z));
                            size = shrub.cullingRadius * shrubScale;
                            break;
                        case RenderedObjectType.Tie:
                            TieModel tie = (TieModel) modelObject.model;
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
        public void Render(bool switchBlends)
        {
            if (culled) return;
            if (!BindIBO() || !BindVBO()) return;
            GL.UniformMatrix4(shaderIDTable.UniformModelToWorldMatrix, false, ref modelObject.modelMatrix);

            SetupVertexAttribPointers();

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in modelObject.model.textureConfig)
            {
                GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? textureIds[level.textures[conf.ID]] : 0);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }

            if (selected)
            {
                if (switchBlends)
                    GL.Disable(EnableCap.Blend);

                GL.UseProgram(shaderIDTable.ShaderColor);
                GL.Enable(EnableCap.LineSmooth);
                GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref modelObject.modelMatrix);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, modelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.UseProgram(shaderIDTable.ShaderMain);
                GL.Disable(EnableCap.LineSmooth);

                if (switchBlends)
                    GL.Enable(EnableCap.Blend);
            }
        }

        /// <summary>
        /// Attempts to bind the index buffer object. Returns true on success, false else.
        /// </summary>
        public bool BindIBO()
        {
            bool success = ibo != 0;
            if (success)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

            return success;
        }

        /// <summary>
        /// Attempts to bind the vertex buffer object. Returns true on success, false else.
        /// </summary>
        public bool BindVBO()
        {
            bool success = vbo != 0;
            if (success)
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            return success;
        }

        private void SetupVertexAttribPointers()
        {
            if (type == RenderedObjectType.Terrain || type == RenderedObjectType.Tie)
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 9, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 9, sizeof(float) * 3);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 9, sizeof(float) * 6);
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 9, sizeof(float) * 8);
            }
            else
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
            }
        }
    }
}
