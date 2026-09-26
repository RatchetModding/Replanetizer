using System;
using System.Collections.Generic;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;
using Replanetizer.Renderer;
using Xunit;

namespace Replanetizer.Tests
{
    public class MobyCollisionTransformTests
    {
        [Fact]
        public void SingularZeroTransformUsesWorldVerticalCapsuleAxis()
        {
            Vector3 axis = MobyCollisionMeshBuilder.GetLocalCapsuleAxis(Matrix4.Zero);

            Assert.Equal(Vector3.UnitZ, axis);
        }

        [Fact]
        public void SingularTransformWithNonVerticalZAxisUsesWorldVerticalCapsuleAxis()
        {
            Matrix4 modelToWorld = Matrix4.Identity;
            modelToWorld.M11 = 1.0f;
            modelToWorld.M12 = 0.0f;
            modelToWorld.M13 = 1.0f;
            modelToWorld.M21 = 1.0f;
            modelToWorld.M22 = 0.0f;
            modelToWorld.M23 = 0.0f;
            modelToWorld.M31 = 1.0f;
            modelToWorld.M32 = 0.0f;
            modelToWorld.M33 = 1.0f;

            Vector3 axis = MobyCollisionMeshBuilder.GetLocalCapsuleAxis(modelToWorld);

            Assert.Equal(Vector3.UnitZ, axis);
        }

        [Fact]
        public void RuntimeCollisionMatrixUsesMobyTransformationBasis()
        {
            var moby = new Moby(GameType.RaC1);
            var memory = new Moby.IngameMobyMemory
            {
                scale = 1.0f,
                position = new Vector4(4.0f, -3.0f, 7.0f, 1.0f),
                rotation = Vector4.Zero,
                transformation = new Matrix3x4(
                    0.0f, 1.0f, 0.0f, 0.0f,
                    -1.0f, 0.0f, 0.0f, 0.0f,
                    0.0f, 0.0f, 1.0f, 0.0f)
            };

            moby.ApplyMemory(memory, new List<Model>());

            Assert.Equal(moby.modelMatrix, moby.collisionMatrix);
            Assert.Equal(0.0f, moby.collisionMatrix.M11);
            Assert.Equal(1.0f, moby.collisionMatrix.M12);
            Assert.Equal(-1.0f, moby.collisionMatrix.M21);
            Assert.Equal(0.0f, moby.collisionMatrix.M22);
        }

        [Fact]
        public void IndexedCapsuleFollowsAnimatedEndpointsWhenMobyIsRotated()
        {
            const float radius = 0.25f;
            var collision = new MobyModelCollision();
            collision.primitives.Add(new MobyModelCollisionPrimitive
            {
                shape = MobyModelCollisionShape.IndexedCapsule,
                indexedCapsuleVertex0 = 0,
                indexedCapsuleVertex1 = 1,
                indexedCapsuleRadius = radius
            });
            var model = new MobyModel { collisionData = collision };
            IReadOnlyList<Vector3> bonePositions = new[]
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(2.0f, 0.0f, 2.0f)
            };

            Matrix4 modelToWorld = Matrix4.CreateRotationX(MathF.PI * 0.5f)
                * Matrix4.CreateTranslation(4.0f, -3.0f, 7.0f);
            MobyCollisionMeshPart mesh = MobyCollisionMeshBuilder.Build(model, bonePositions, modelToWorld).primitiveMesh;
            Vector3 capsuleDirection = (bonePositions[1] - bonePositions[0]).Normalized();
            Vector3 expectedStartTip = Vector3.TransformPosition(
                bonePositions[0] - capsuleDirection * radius, modelToWorld);
            Vector3 expectedEndTip = Vector3.TransformPosition(
                bonePositions[1] + capsuleDirection * radius, modelToWorld);
            bool foundStartTip = false;
            bool foundEndTip = false;

            for (int i = 0; i < mesh.vertexBuffer.Length; i += 4)
            {
                Vector3 localPosition = new Vector3(
                    mesh.vertexBuffer[i],
                    mesh.vertexBuffer[i + 1],
                    mesh.vertexBuffer[i + 2]);
                Vector3 worldPosition = Vector3.TransformPosition(localPosition, modelToWorld);
                foundStartTip |= (worldPosition - expectedStartTip).LengthSquared < 1e-6f;
                foundEndTip |= (worldPosition - expectedEndTip).LengthSquared < 1e-6f;
            }

            Assert.True(foundStartTip);
            Assert.True(foundEndTip);
        }

        [Fact]
        public void StandardCapsuleAxisRemainsWorldVerticalWhenMobyIsRotated()
        {
            const float radius = 0.25f;
            const float length = 2.0f;
            var collision = new MobyModelCollision();
            collision.primitives.Add(new MobyModelCollisionPrimitive
            {
                shape = MobyModelCollisionShape.Capsule,
                capsuleLength = length,
                capsuleRadius = radius
            });
            var model = new MobyModel { collisionData = collision };
            Matrix4 modelToWorld = Matrix4.CreateRotationX(MathF.PI * 0.5f)
                * Matrix4.CreateTranslation(4.0f, -3.0f, 7.0f);

            MobyCollisionMeshPart mesh = MobyCollisionMeshBuilder.Build(model, modelToWorld).primitiveMesh;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < mesh.vertexBuffer.Length; i += 4)
            {
                Vector3 localPosition = new Vector3(
                    mesh.vertexBuffer[i],
                    mesh.vertexBuffer[i + 1],
                    mesh.vertexBuffer[i + 2]);
                Vector3 worldPosition = Vector3.TransformPosition(localPosition, modelToWorld);
                minZ = MathF.Min(minZ, worldPosition.Z);
                maxZ = MathF.Max(maxZ, worldPosition.Z);
            }

            Assert.Equal(7.0f - radius, minZ, 3);
            Assert.Equal(7.0f + length + radius, maxZ, 3);
        }
    }
}
