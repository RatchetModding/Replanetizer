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
            public Moby moby = null!;
            public int vao;
            public int vbo;
            public int ibo;
            public int indexCount;
            public bool triangleTransform;
        }

        private readonly ShaderTable shaderTable;
        private readonly List<CollisionMeshHandle> meshes = new List<CollisionMeshHandle>();
        private readonly Dictionary<Moby, MobyModel?> cachedModels = new Dictionary<Moby, MobyModel?>();

        public MobyCollisionRenderer(ShaderTable shaderTable)
        {
            this.shaderTable = shaderTable;
        }

        public override void Include<T>(T obj)
        {
            if (obj is not Moby moby)
                throw new NotImplementedException();

            Update(moby);
        }

        public override void Include<T>(List<T> list)
        {
            if (list is not List<Moby> mobies)
                throw new NotImplementedException();

            foreach (Moby moby in mobies)
            {
                Include(moby);
            }
        }

        public void Remove(Moby moby)
        {
            for (int i = meshes.Count - 1; i >= 0; i--)
            {
                if (meshes[i].moby != moby) continue;

                DeleteMesh(meshes[i]);
                meshes.RemoveAt(i);
            }

            cachedModels.Remove(moby);
        }

        public void Update(Moby moby)
        {
            if (moby.memory?.IsDead() == true)
            {
                Remove(moby);
                return;
            }

            MobyModel? mobyModel = moby.model as MobyModel;
            if (cachedModels.TryGetValue(moby, out MobyModel? cachedModel)
                && ReferenceEquals(cachedModel, mobyModel))
            {
                return;
            }

            Remove(moby);
            cachedModels[moby] = mobyModel;

            if (mobyModel?.collisionData == null)
                return;

            MobyCollisionMesh mesh = MobyCollisionMeshBuilder.Build(mobyModel.collisionData);
            AddMesh(moby, mesh.triangleMesh, true);
            AddMesh(moby, mesh.primitiveMesh, false);
        }

        public override void Render(RendererPayload payload)
        {
            Matrix4 worldToView = payload.camera.GetWorldViewMatrix();

            shaderTable.collisionShader.UseShader();
            shaderTable.collisionShader.SetUniformMatrix4(UniformName.worldToView, ref worldToView);
            shaderTable.collisionShader.SetUniform3(UniformName.cameraPosition, payload.camera.position);

            foreach (CollisionMeshHandle mesh in meshes)
            {
                Matrix4 modelToWorld = mesh.triangleTransform
                    ? mesh.moby.collisionTriangleMatrix
                    : mesh.moby.collisionMatrix;
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
            cachedModels.Clear();
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
            GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.indexBuffer.Length * sizeof(uint), mesh.indexBuffer, BufferUsageHint.StaticDraw);

            GLUtil.ActivateNumberOfVertexAttribArrays(2);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);

            meshes.Add(new CollisionMeshHandle
            {
                moby = moby,
                vao = vao,
                vbo = vbo,
                ibo = ibo,
                indexCount = mesh.indexBuffer.Length,
                triangleTransform = triangleTransform
            });
        }
    }
}
