using System;
using System.Collections.Generic;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class MobyCollisionRenderer : Renderer
    {
        private sealed class CollisionMeshHandle
        {
            public int vao;
            public int vbo;
            public int ibo;
            public int indexCount;
            public bool triangleTransform;
        }

        private readonly ShaderTable shaderTable;
        private readonly List<CollisionMeshHandle> meshes = new List<CollisionMeshHandle>();
        private CollisionMeshHandle? primitiveMesh;
        public Moby? moby { get; private set; }
        public MobyModel? mobyModelStandalone { get; private set; }
        private MobyModel? cachedModel;
        private bool modelCached;
        private bool primitiveUsesBonePositions;

        public MobyCollisionRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        public override void Include<T>(T obj)
        {
            Moby? includedMoby = obj as Moby;
            MobyModel? includedModel = obj as MobyModel;
            if (includedMoby == null && includedModel == null)
                throw new NotImplementedException();

            if (!ReferenceEquals(this.moby, includedMoby)
                || !ReferenceEquals(this.mobyModelStandalone, includedModel))
            {
                DeleteMeshes();
                cachedModel = null;
                modelCached = false;
            }

            moby = includedMoby;
            mobyModelStandalone = includedModel;
        }

        public override void Include<T>(List<T> list) => throw new NotImplementedException();

        private void Update()
        {
            if (moby == null && mobyModelStandalone == null)
                return;

            if (moby?.memory?.IsDead() == true)
            {
                DeleteMeshes();
                cachedModel = null;
                modelCached = false;
                return;
            }

            MobyModel? mobyModel = (MobyModel?) moby?.model ?? mobyModelStandalone;
            if (modelCached && ReferenceEquals(cachedModel, mobyModel))
            {
                return;
            }

            DeleteMeshes();
            cachedModel = mobyModel;
            modelCached = true;
            primitiveUsesBonePositions = false;

            if (mobyModel?.collisionData == null)
                return;

            MobyCollisionMesh mesh = MobyCollisionMeshBuilder.Build(mobyModel);
            AddMesh(mesh.triangleMesh, true);
            AddMesh(mesh.primitiveMesh, false);
        }

        public void UpdateBonePositions(IReadOnlyList<Vector3> bonePositions)
        {
            if (cachedModel?.collisionData == null || primitiveMesh == null)
                return;

            MobyCollisionMeshPart mesh = MobyCollisionMeshBuilder.Build(cachedModel, bonePositions).primitiveMesh;
            UpdatePrimitiveMesh(mesh);
            primitiveUsesBonePositions = true;
        }

        private void RestoreBindPose()
        {
            if (!primitiveUsesBonePositions || cachedModel?.collisionData == null || primitiveMesh == null)
                return;

            MobyCollisionMeshPart mesh = MobyCollisionMeshBuilder.Build(cachedModel).primitiveMesh;
            UpdatePrimitiveMesh(mesh);
            primitiveUsesBonePositions = false;
        }

        private void UpdatePrimitiveMesh(MobyCollisionMeshPart mesh)
        {
            if (primitiveMesh == null)
                return;

            GL.BindVertexArray(primitiveMesh.vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, primitiveMesh.vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertexBuffer.Length * sizeof(float), mesh.vertexBuffer, BufferUsageHint.DynamicDraw);

            if (mesh.indexBuffer.Length != primitiveMesh.indexCount)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, primitiveMesh.ibo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.indexBuffer.Length * sizeof(uint), mesh.indexBuffer, BufferUsageHint.StaticDraw);
                primitiveMesh.indexCount = mesh.indexBuffer.Length;
            }
        }

        public override void Render(RendererPayload payload)
        {
            Update();
            RestoreBindPose();
            RenderMeshes(payload);
        }

        public void Render(RendererPayload payload, IReadOnlyList<Vector3>? bonePositions)
        {
            Update();
            if (bonePositions != null)
            {
                UpdateBonePositions(bonePositions);
            }
            else
            {
                RestoreBindPose();
            }

            RenderMeshes(payload);
        }

        public override void Dispose()
        {
            foreach (CollisionMeshHandle mesh in meshes)
            {
                DeleteMesh(mesh);
            }

            meshes.Clear();
            primitiveMesh = null;
            cachedModel = null;
            modelCached = false;
        }

        private void DeleteMeshes()
        {
            foreach (CollisionMeshHandle mesh in meshes)
            {
                DeleteMesh(mesh);
            }

            meshes.Clear();
            primitiveMesh = null;
            primitiveUsesBonePositions = false;
        }

        private static void DeleteMesh(CollisionMeshHandle mesh)
        {
            GL.DeleteBuffer(mesh.ibo);
            GL.DeleteBuffer(mesh.vbo);
            GL.DeleteVertexArray(mesh.vao);
        }

        private Matrix4 GetModelToWorld(bool triangleTransform)
        {
            if (moby != null)
            {
                return triangleTransform ? moby.collisionTriangleMatrix : moby.collisionMatrix;
            }

            return Matrix4.Identity;
        }

        private void AddMesh(MobyCollisionMeshPart mesh, bool triangleTransform)
        {
            if (mesh.indexBuffer.Length == 0)
                return;

            int vao;
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            int vbo;
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertexBuffer.Length * sizeof(float), mesh.vertexBuffer, BufferUsageHint.StaticDraw);

            int ibo;
            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            uint[] indexBuffer = mesh.indexBuffer;
            if (triangleTransform)
            {
                indexBuffer = (uint[]) mesh.indexBuffer.Clone();
                for (int index = 0; index < indexBuffer.Length; index += 3)
                {
                    uint second = indexBuffer[index + 1];
                    indexBuffer[index + 1] = indexBuffer[index + 2];
                    indexBuffer[index + 2] = second;
                }
            }

            GL.BufferData(BufferTarget.ElementArrayBuffer, indexBuffer.Length * sizeof(uint), indexBuffer, BufferUsageHint.StaticDraw);

            GLUtil.ActivateNumberOfVertexAttribArrays(2);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);

            CollisionMeshHandle handle = new CollisionMeshHandle
            {
                vao = vao,
                vbo = vbo,
                ibo = ibo,
                indexCount = mesh.indexBuffer.Length,
                triangleTransform = triangleTransform
            };
            meshes.Add(handle);
            if (!triangleTransform)
            {
                primitiveMesh = handle;
            }
        }

        private void RenderMeshes(RendererPayload payload)
        {
            if ((moby == null && mobyModelStandalone == null) || moby?.memory?.IsDead() == true)
                return;

            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();

            shaderTable.collisionShader.UseShader();
            shaderTable.collisionShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.collisionShader.SetUniform3(UniformName.cameraPosition, payload.camera.position);

            foreach (CollisionMeshHandle mesh in meshes)
            {
                Matrix4 modelToWorld = GetModelToWorld(mesh.triangleTransform);
                shaderTable.collisionShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
                GL.BindVertexArray(mesh.vao);
                GL.DrawElements(PrimitiveType.Triangles, mesh.indexCount, DrawElementsType.UnsignedInt, 0);
            }

            GLUtil.CheckGlError("MobyCollisionRenderer");
        }
    }
}
