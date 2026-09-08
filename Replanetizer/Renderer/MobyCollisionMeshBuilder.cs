using System;
using System.Collections.Generic;
using LibReplanetizer.Models;
using OpenTK.Mathematics;

namespace Replanetizer.Renderer
{
    public sealed class MobyCollisionMesh
    {
        public MobyCollisionMeshPart triangleMesh { get; }
        public MobyCollisionMeshPart primitiveMesh { get; }

        public MobyCollisionMesh(MobyCollisionMeshPart triangleMesh, MobyCollisionMeshPart primitiveMesh)
        {
            this.triangleMesh = triangleMesh;
            this.primitiveMesh = primitiveMesh;
        }
    }

    public sealed class MobyCollisionMeshPart
    {
        public float[] vertexBuffer { get; }
        public uint[] indexBuffer { get; }

        public MobyCollisionMeshPart(float[] vertexBuffer, uint[] indexBuffer)
        {
            this.vertexBuffer = vertexBuffer;
            this.indexBuffer = indexBuffer;
        }
    }

    public static class MobyCollisionMeshBuilder
    {
        private const int VERTEX_STRIDE = 4;
        private const int SPHERE_SEGMENTS = 16;
        private const int CAPSULE_STACKS = 8;

        private static readonly uint PrimitiveColor = PackColor(0, 255, 255, 255);

        public static MobyCollisionMesh Build(MobyModelCollision collision)
        {
            List<float> triangleVertices = new List<float>();
            List<uint> triangleIndices = new List<uint>();
            List<float> primitiveVertices = new List<float>();
            List<uint> primitiveIndices = new List<uint>();

            foreach (MobyModelCollisionTriangle triangle in collision.triangles)
            {
                AddTriangleIfValid(triangleVertices, triangleIndices, collision, triangle);
            }

            foreach (MobyModelCollisionPrimitive primitive in collision.primitives)
            {
                AddPrimitive(primitiveVertices, primitiveIndices, collision, primitive);
            }

            return new MobyCollisionMesh(
                new MobyCollisionMeshPart(triangleVertices.ToArray(), triangleIndices.ToArray()),
                new MobyCollisionMeshPart(primitiveVertices.ToArray(), primitiveIndices.ToArray()));
        }

        private static void AddPrimitive(List<float> vertices, List<uint> indices, MobyModelCollision collision, MobyModelCollisionPrimitive primitive)
        {
            switch (primitive.shape)
            {
                case MobyModelCollisionShape.Sphere:
                case MobyModelCollisionShape.SphereVariant:
                    AddSphere(vertices, indices,
                        new Vector3(primitive.sphereCenterX, primitive.sphereCenterY, primitive.sphereCenterZ),
                        primitive.sphereRadius, PrimitiveColor);
                    break;
                case MobyModelCollisionShape.IndexedSphere:
                    if (TryGetVertex(collision, primitive.indexedSphereVertex, out Vector3 indexedSphereCenter))
                    {
                        AddSphere(vertices, indices,
                            indexedSphereCenter + new Vector3(primitive.indexedSphereOffsetX, primitive.indexedSphereOffsetY, primitive.indexedSphereOffsetZ),
                            primitive.indexedSphereRadius, PrimitiveColor);
                    }
                    break;
                case MobyModelCollisionShape.Capsule:
                    float capsuleLength = primitive.capsuleLength;
                    AddCapsule(vertices, indices,
                        new Vector3(primitive.capsuleCenterX, primitive.capsuleCenterY,
                            primitive.capsuleCenterZ + capsuleLength * 0.5f),
                        Vector3.UnitZ, capsuleLength * 0.5f, primitive.capsuleRadius, PrimitiveColor);
                    break;
                case MobyModelCollisionShape.IndexedCapsule:
                    if (TryGetVertex(collision, primitive.indexedCapsuleVertex0, out Vector3 capsuleStart)
                        && TryGetVertex(collision, primitive.indexedCapsuleVertex1, out Vector3 capsuleEnd))
                    {
                        Vector3 axis = capsuleEnd - capsuleStart;
                        float length = axis.Length;
                        if (length > float.Epsilon)
                        {
                            AddCapsule(vertices, indices, (capsuleStart + capsuleEnd) * 0.5f,
                                axis / length, length * 0.5f, primitive.indexedCapsuleRadius, PrimitiveColor);
                        }
                        else
                        {
                            AddSphere(vertices, indices, capsuleStart, primitive.indexedCapsuleRadius, PrimitiveColor);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private static void AddSphere(List<float> vertices, List<uint> indices, Vector3 center, float radius, uint color)
        {
            if (radius <= 0.0f || float.IsNaN(radius) || float.IsInfinity(radius)) return;

            uint firstVertex = (uint) (vertices.Count / VERTEX_STRIDE);
            for (int stack = 0; stack <= CAPSULE_STACKS; stack++)
            {
                float latitude = MathF.PI * stack / CAPSULE_STACKS;
                float sinLatitude = MathF.Sin(latitude);
                float cosLatitude = MathF.Cos(latitude);

                for (int segment = 0; segment < SPHERE_SEGMENTS; segment++)
                {
                    float longitude = MathF.Tau * segment / SPHERE_SEGMENTS;
                    Vector3 direction = new Vector3(
                        sinLatitude * MathF.Cos(longitude),
                        sinLatitude * MathF.Sin(longitude),
                        cosLatitude);
                    AddVertex(vertices, center + direction * radius, color);
                }
            }

            AddRingIndices(indices, firstVertex, CAPSULE_STACKS, SPHERE_SEGMENTS);
        }

        private static void AddCapsule(List<float> vertices, List<uint> indices, Vector3 center, Vector3 axis,
            float halfLength, float radius, uint color)
        {
            if (halfLength < 0.0f) halfLength = -halfLength;
            if (radius <= 0.0f || float.IsNaN(radius) || float.IsInfinity(radius)) return;

            axis = axis.Normalized();
            Vector3 basis = MathF.Abs(Vector3.Dot(axis, Vector3.UnitZ)) < 0.9f ? Vector3.UnitZ : Vector3.UnitX;
            Vector3 side = Vector3.Cross(axis, basis).Normalized();
            Vector3 up = Vector3.Cross(axis, side).Normalized();
            uint firstVertex = (uint) (vertices.Count / VERTEX_STRIDE);
            List<(float axial, float radial)> rings = new List<(float axial, float radial)>();
            for (int stack = 0; stack <= CAPSULE_STACKS; stack++)
            {
                float theta = MathF.PI * 0.5f * stack / CAPSULE_STACKS;
                rings.Add((halfLength + MathF.Cos(theta) * radius, MathF.Sin(theta) * radius));
            }

            rings.Add((-halfLength, radius));
            for (int stack = 1; stack <= CAPSULE_STACKS; stack++)
            {
                float theta = MathF.PI * 0.5f * stack / CAPSULE_STACKS;
                rings.Add((-halfLength - MathF.Sin(theta) * radius, MathF.Cos(theta) * radius));
            }

            for (int stack = 0; stack < rings.Count; stack++)
            {
                (float axial, float radial) = rings[stack];
                Vector3 ringCenter = center + axis * axial;

                for (int segment = 0; segment < SPHERE_SEGMENTS; segment++)
                {
                    float longitude = MathF.Tau * segment / SPHERE_SEGMENTS;
                    Vector3 direction = side * MathF.Cos(longitude) + up * MathF.Sin(longitude);
                    AddVertex(vertices, ringCenter + direction * radial, color);
                }
            }

            AddRingIndices(indices, firstVertex, rings.Count - 1, SPHERE_SEGMENTS);
        }

        private static void AddRingIndices(List<uint> indices, uint firstVertex, int stacks, int segments)
        {
            for (int stack = 0; stack < stacks; stack++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    uint current = firstVertex + (uint) (stack * segments + segment);
                    uint next = firstVertex + (uint) (stack * segments + (segment + 1) % segments);
                    uint above = current + (uint) segments;
                    uint aboveNext = next + (uint) segments;
                    indices.Add(current);
                    indices.Add(above);
                    indices.Add(next);
                    indices.Add(next);
                    indices.Add(above);
                    indices.Add(aboveNext);
                }
            }
        }

        private static bool TryGetVertex(MobyModelCollision collision, int index, out Vector3 vertex)
        {
            if (index >= 0 && index < collision.vertices.Count)
            {
                MobyModelCollisionVertex source = collision.vertices[index];
                vertex = new Vector3(source.x, source.y, source.z);
                return true;
            }

            vertex = Vector3.Zero;
            return false;
        }

        private static void AddTriangleIfValid(List<float> vertices, List<uint> indices,
            MobyModelCollision collision, MobyModelCollisionTriangle triangle)
        {
            if (!TryGetVertex(collision, triangle.vertex0, out Vector3 vertex0)
                || !TryGetVertex(collision, triangle.vertex1, out Vector3 vertex1)
                || !TryGetVertex(collision, triangle.vertex2, out Vector3 vertex2))
                return;

            uint firstVertex = (uint) (vertices.Count / VERTEX_STRIDE);
            uint color = PackCollisionTypeColor(triangle.collisionType);
            AddVertex(vertices, vertex0, color);
            AddVertex(vertices, vertex1, color);
            AddVertex(vertices, vertex2, color);

            indices.Add(firstVertex);
            indices.Add(firstVertex + 1);
            indices.Add(firstVertex + 2);
        }

        private static void AddVertex(List<float> vertices, Vector3 position, uint color)
        {
            vertices.Add(position.X);
            vertices.Add(position.Y);
            vertices.Add(position.Z);
            vertices.Add(BitConverter.UInt32BitsToSingle(color));
        }

        private static uint PackColor(byte red, byte green, byte blue, byte alpha)
        {
            return (uint) (red | (green << 8) | (blue << 16) | (alpha << 24));
        }

        private static uint PackCollisionTypeColor(byte collisionType)
        {
            byte red = (byte) ((collisionType & 0x03) << 6);
            byte green = (byte) ((collisionType & 0x0C) << 4);
            byte blue = (byte) (collisionType & 0xF0);
            return PackColor(red, green, blue, 255);
        }
    }
}
