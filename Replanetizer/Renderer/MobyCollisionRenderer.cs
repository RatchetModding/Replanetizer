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
        public Moby? moby { get; private set; }
        private MobyModel? cachedModel;
        private bool modelCached;

        public MobyCollisionRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        public override void Include<T>(T obj)
        {
            if (obj is not Moby moby)
                throw new NotImplementedException();

            if (this.moby != null && !ReferenceEquals(this.moby, moby))
            {
                DeleteMeshes();
                cachedModel = null;
                modelCached = false;
            }

            this.moby = moby;
        }

        public override void Include<T>(List<T> list) => throw new NotImplementedException();

        private void Update()
        {
            if (moby == null)
                return;

            if (moby.memory?.IsDead() == true)
            {
                DeleteMeshes();
                cachedModel = null;
                modelCached = false;
                return;
            }

            MobyModel? mobyModel = moby.model as MobyModel;
            if (modelCached && ReferenceEquals(cachedModel, mobyModel))
            {
                return;
            }

            DeleteMeshes();
            cachedModel = mobyModel;
            modelCached = true;

            if (mobyModel?.collisionData == null)
                return;

            MobyCollisionMesh mesh = MobyCollisionMeshBuilder.Build(mobyModel);
            AddMesh(moby, mesh.triangleMesh, true);
            AddMesh(moby, mesh.primitiveMesh, false);
        }

        public override void Render(RendererPayload payload)
        {
            Update();

            if (moby == null || moby.memory?.IsDead() == true)
                return;

            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();

            shaderTable.collisionShader.UseShader();
            shaderTable.collisionShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.collisionShader.SetUniform3(UniformName.cameraPosition, payload.camera.position);

            foreach (CollisionMeshHandle mesh in meshes)
            {
                Matrix4 modelToWorld = mesh.triangleTransform
                    ? moby.collisionTriangleMatrix
                    : moby.collisionMatrix;
                shaderTable.collisionShader.SetUniformMatrix4(UniformName.modelToWorld, ref modelToWorld);
                GL.BindVertexArray(mesh.vao);
                GL.DrawElements(PrimitiveType.Triangles, mesh.indexCount, DrawElementsType.UnsignedInt, 0);
            }

            GLUtil.CheckGlError("MobyCollisionRenderer");
        }

        public override void Dispose()
        {
            foreach (CollisionMeshHandle mesh in meshes)
            {
                DeleteMesh(mesh);
            }

            meshes.Clear();
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
        }

        private static void DeleteMesh(CollisionMeshHandle mesh)
        {
            GL.DeleteBuffer(mesh.ibo);
            GL.DeleteBuffer(mesh.vbo);
            GL.DeleteVertexArray(mesh.vao);
        }

        private void AddMesh(Moby moby, MobyCollisionMeshPart mesh, bool triangleTransform)
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

            meshes.Add(new CollisionMeshHandle
            {
                vao = vao,
                vbo = vbo,
                ibo = ibo,
                indexCount = mesh.indexBuffer.Length,
                triangleTransform = triangleTransform
            });
        }
    }
}
