// Copyright (C) 2018-2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models
{
    public struct CollisionType
    {
        public byte data { get; private set; }

        public int materialID { get { return data & 0x1F; } }
        public int groupID { get { return (data >> 5) & 0x3; } }
        public bool ignoreCameraCollision { get { return (data >> 7) != 0; } }

        public string GetMaterialName()
        {
            int id = materialID;
            switch (id)
            {
                case 0x00: return "Water";
                case 0x01: return "Lava";
                case 0x02: return "Magnetic Ramp";
                case 0x03: return "Mud/Quicksand";
                case 0x04: return "Water Slide";
                case 0x05: return "Grindrail Jump";
                case 0x07: return "Ice Sliding";
                case 0x08: return "Sliding Off";
                case 0x09: return "Disable Ledge Grab";
                case 0x0A: return "Disable Wall Jump";
                case 0x0B: return "Death Mud";
                case 0x0C: return "Sliding Off (No Wall Jump / Ledge Grab)";
                case 0x0D: return "Death Ice";
                case 0x1F: return "Default";
                default:
                    return "Unknown";
            }
        }

        public CollisionType(byte data) { this.data = data; }
    }

    internal readonly struct CollisionTriangle
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }
        public CollisionType Type { get; }

        public CollisionTriangle(Vector3 a, Vector3 b, Vector3 c, CollisionType type)
        {
            A = a;
            B = b;
            C = c;
            Type = type;
        }
    }

    internal readonly struct CollisionFace
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }
        public Vector3 D { get; }
        public CollisionType Type { get; }
        public bool IsQuad { get; }

        public CollisionFace(Vector3 a, Vector3 b, Vector3 c, CollisionType type)
        {
            A = a;
            B = b;
            C = c;
            D = default;
            Type = type;
            IsQuad = false;
        }

        public CollisionFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, CollisionType type)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            Type = type;
            IsQuad = true;
        }

        public Vector3 GetCorner(int index)
        {
            return index switch
            {
                0 => A,
                1 => B,
                2 => C,
                3 when IsQuad => D,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
    }

    public class Collision : Model, IRenderable
    {
        private const int MaximumGridEntries = 1_000_000;
        private const long MaximumCellFaceReferences = 5_000_000;

        private readonly struct CollisionFaceOccurrence
        {
            public CollisionFace Face { get; }
            private Vector3 ErrorA { get; }
            private Vector3 ErrorB { get; }
            private Vector3 ErrorC { get; }
            private Vector3 ErrorD { get; }

            public CollisionFaceOccurrence(CollisionFace face, Vector3 errorA, Vector3 errorB, Vector3 errorC, Vector3 errorD)
            {
                Face = face;
                ErrorA = errorA;
                ErrorB = errorB;
                ErrorC = errorC;
                ErrorD = errorD;
            }

            public Vector3 GetError(int index)
            {
                return index switch
                {
                    0 => ErrorA,
                    1 => ErrorB,
                    2 => ErrorC,
                    3 when Face.IsQuad => ErrorD,
                    _ => throw new ArgumentOutOfRangeException(nameof(index))
                };
            }
        }

        private readonly record struct StandardFaceKey(Vector3 A, Vector3 B, Vector3 C, Vector3 D, bool IsQuad, byte Type)
        {
            public static StandardFaceKey Create(CollisionFace face)
            {
                int cornerCount = face.IsQuad ? 4 : 3;
                int bestRotation = 0;
                for (int rotation = 1; rotation < cornerCount; rotation++)
                {
                    if (CompareRotation(face, rotation, bestRotation, cornerCount) < 0)
                        bestRotation = rotation;
                }

                return new StandardFaceKey(
                    face.GetCorner(bestRotation),
                    face.GetCorner((bestRotation + 1) % cornerCount),
                    face.GetCorner((bestRotation + 2) % cornerCount),
                    face.IsQuad ? face.GetCorner((bestRotation + 3) % cornerCount) : default,
                    face.IsQuad,
                    face.Type.data);
            }

            private static int CompareRotation(CollisionFace face, int rotation, int otherRotation, int cornerCount)
            {
                for (int corner = 0; corner < cornerCount; corner++)
                {
                    int comparison = Compare(
                        face.GetCorner((rotation + corner) % cornerCount),
                        face.GetCorner((otherRotation + corner) % cornerCount));
                    if (comparison != 0)
                        return comparison;
                }

                return 0;
            }

            private static int Compare(Vector3 first, Vector3 second)
            {
                int comparison = first.X.CompareTo(second.X);
                if (comparison != 0) return comparison;
                comparison = first.Y.CompareTo(second.Y);
                return comparison != 0 ? comparison : first.Z.CompareTo(second.Z);
            }
        }

        private readonly record struct FaceBucketKey(long X, long Y, long Z, byte CornerCount, byte Type);

        private sealed class StandardFaceGroup
        {
            public CollisionFaceOccurrence Occurrence { get; }
            public int MaximumMultiplicity { get; set; }
            public int LastCellIndex { get; set; }

            public StandardFaceGroup(CollisionFaceOccurrence occurrence, int multiplicity, int cellIndex)
            {
                Occurrence = occurrence;
                MaximumMultiplicity = multiplicity;
                LastCellIndex = cellIndex;
            }
        }

        private bool hasStandardCollision = false;
        private bool hasHeroCollision = false;
        private bool hasUnkCollision = false;

        // Model

        public uint[] indBuff = { };

        // Standard Collision

        private ushort zShift = 0;
        private ushort[] yShift = [];
        private List<ushort[]> xShift = new List<ushort[]>();
        private ushort zCount = 0;
        private ushort[] yCount = [];
        private List<ushort[]> xCount = new List<ushort[]>();

        private class CollisionCell
        {
            private ushort faceCount;
            private byte vertexCount;
            private byte quadCount;
            public ushort xCell;
            public ushort yCell;
            public ushort zCell;

            private struct CollisionCellEntry
            {
                public byte vertexIndex0;
                public byte vertexIndex1;
                public byte vertexIndex2;
                public byte vertexIndex3;
                public CollisionType collisionType;
            }

            CollisionCellEntry[] entries = [];
            Vector3[] vertices = [];

            public CollisionCell(List<CollisionFace> faces, ushort x, ushort y, ushort z, int worldX, int worldY, int worldZ)
            {
                xCell = x;
                yCell = y;
                zCell = z;
                int inputFaceCount = checked((ushort) faces.Count);
                int inputQuadCount = faces.Count(face => face.IsQuad);
                if (inputQuadCount > byte.MaxValue)
                    throw new InvalidOperationException("A collision cell cannot contain more than 255 quads.");
                quadCount = (byte) inputQuadCount;
                entries = new CollisionCellEntry[inputFaceCount];

                var sharedVertices = new List<Vector3>();
                var vertexIndices = new Dictionary<Vector3, byte>();
                float centerX = 4.0f * (worldX + 0.5f);
                float centerY = 4.0f * (worldY + 0.5f);
                float centerZ = 4.0f * (worldZ + 0.5f);

                Vector3 GetLocalPosition(Vector3 position)
                {
                    return new Vector3(
                        (position.X - centerX) * 1024.0f,
                        (position.Y - centerY) * 1024.0f,
                        (position.Z - centerZ) * 1024.0f);
                }

                byte GetVertexIndex(Vector3 local)
                {
                    if (vertexIndices.TryGetValue(local, out byte index))
                        return index;
                    if (sharedVertices.Count == byte.MaxValue)
                        throw new InvalidOperationException("A collision cell cannot contain more than 255 unique vertices.");

                    index = (byte) sharedVertices.Count;
                    sharedVertices.Add(local);
                    vertexIndices.Add(local, index);
                    return index;
                }

                foreach (CollisionFace face in faces.Where(face => face.IsQuad).Concat(faces.Where(face => !face.IsQuad)))
                {
                    Vector3 localA = GetLocalPosition(face.A);
                    Vector3 localB = GetLocalPosition(face.B);
                    Vector3 localC = GetLocalPosition(face.C);
                    Vector3 localD = face.IsQuad ? GetLocalPosition(face.D) : default;
                    int newVertexCount = 0;
                    if (!vertexIndices.ContainsKey(localA)) newVertexCount++;
                    if (!vertexIndices.ContainsKey(localB) && !localB.Equals(localA)) newVertexCount++;
                    if (!vertexIndices.ContainsKey(localC) && !localC.Equals(localA) && !localC.Equals(localB)) newVertexCount++;
                    if (face.IsQuad && !vertexIndices.ContainsKey(localD) &&
                        !localD.Equals(localA) && !localD.Equals(localB) && !localD.Equals(localC))
                        newVertexCount++;
                    if (sharedVertices.Count + newVertexCount > byte.MaxValue)
                        throw new InvalidOperationException("A collision cell cannot contain more than 255 unique vertices.");

                    entries[faceCount] = new CollisionCellEntry
                    {
                        vertexIndex0 = GetVertexIndex(localA),
                        vertexIndex1 = GetVertexIndex(localB),
                        vertexIndex2 = GetVertexIndex(localC),
                        vertexIndex3 = face.IsQuad ? GetVertexIndex(localD) : (byte) 0xFF,
                        collisionType = face.Type
                    };
                    faceCount++;
                }

                if (faceCount != entries.Length)
                    Array.Resize(ref entries, faceCount);
                vertexCount = checked((byte) sharedVertices.Count);
                vertices = sharedVertices.ToArray();
            }

            public CollisionCell(FileStream fs, int baseOffset, ushort x, ushort y, ushort z)
            {
                xCell = x;
                yCell = y;
                zCell = z;

                byte[] headBlock = ReadBlock(fs, baseOffset, 0x04);

                faceCount = ReadUshort(headBlock, 0x00);
                vertexCount = headBlock[0x02];
                quadCount = headBlock[0x03];

                entries = new CollisionCellEntry[faceCount];
                vertices = new Vector3[vertexCount];

                byte[] dataBlock = ReadBlock(fs, baseOffset + 0x04, faceCount * 0x04 + vertexCount * 0x0C + quadCount);
                for (int vertex = 0; vertex < vertexCount; vertex++)
                {
                    float xPos = ReadFloat(dataBlock, vertex * 0x0C + 0x00);
                    float yPos = ReadFloat(dataBlock, vertex * 0x0C + 0x04);
                    float zPos = ReadFloat(dataBlock, vertex * 0x0C + 0x08);
                    vertices[vertex] = new Vector3(xPos, yPos, zPos);
                }

                for (int f = 0; f < faceCount; f++)
                {
                    CollisionCellEntry entry = new CollisionCellEntry();

                    // Collision Type
                    int fOffset = (vertexCount * 0x0C) + (f * 0x04);

                    entry.vertexIndex0 = dataBlock[fOffset];
                    entry.vertexIndex1 = dataBlock[fOffset + 1];
                    entry.vertexIndex2 = dataBlock[fOffset + 2];
                    entry.collisionType = new CollisionType(dataBlock[fOffset + 3]);
                    entry.vertexIndex3 = (f < quadCount) ? dataBlock[vertexCount * 0x0C + faceCount * 0x04 + f] : (byte) 0xFF;

                    entries[f] = entry;
                }
            }

            private Vector3 GetWorldPosition(byte vertexIndex, ushort xShift, ushort yShift, ushort zShift)
            {
                Vector3 local = vertices[vertexIndex];
                return new Vector3(
                    local.X / 1024.0f + 4.0f * (xShift + xCell + 0.5f),
                    local.Y / 1024.0f + 4.0f * (yShift + yCell + 0.5f),
                    local.Z / 1024.0f + 4.0f * (zShift + zCell + 0.5f));
            }

            private (Vector3 position, Vector3 error) GetWorldPositionAndError(byte vertexIndex, ushort xShift, ushort yShift, ushort zShift)
            {
                Vector3 local = vertices[vertexIndex];
                Vector3 position = GetWorldPosition(vertexIndex, xShift, yShift, zShift);
                Vector3 error = new Vector3(
                    GetPositionError(local.X, position.X),
                    GetPositionError(local.Y, position.Y),
                    GetPositionError(local.Z, position.Z));
                return (position, error);
            }

            public float GetMaximumPositionError(ushort xShift, ushort yShift, ushort zShift)
            {
                float maximumError = 0.0f;
                for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
                {
                    var worldPosition = GetWorldPositionAndError(
                        checked((byte) vertexIndex), xShift, yShift, zShift);
                    Vector3 error = worldPosition.error;
                    maximumError = Math.Max(maximumError, Math.Max(error.X, Math.Max(error.Y, error.Z)));
                }

                return maximumError;
            }

            public void GetModelData(ushort xShift, ushort yShift, ushort zShift, List<float> vertexList, List<uint> indexList, ref uint totalVertexCount)
            {
                void AddRenderVertex(Vector3 position, CollisionType collisionType)
                {
                    vertexList.Add(position.X);
                    vertexList.Add(position.Y);
                    vertexList.Add(position.Z);
                    vertexList.Add(CollisionVertexMetadata.Pack(collisionType, CollisionGeometryCategory.Standard));
                }

                for (int face = 0; face < faceCount; face++)
                {
                    CollisionCellEntry entry = entries[face];
                    Vector3 position0 = GetWorldPosition(entry.vertexIndex0, xShift, yShift, zShift);
                    Vector3 position1 = GetWorldPosition(entry.vertexIndex1, xShift, yShift, zShift);
                    Vector3 position2 = GetWorldPosition(entry.vertexIndex2, xShift, yShift, zShift);
                    uint faceVertexStart = totalVertexCount;

                    AddRenderVertex(position0, entry.collisionType);
                    AddRenderVertex(position1, entry.collisionType);
                    AddRenderVertex(position2, entry.collisionType);
                    indexList.Add(faceVertexStart + 1);
                    indexList.Add(faceVertexStart);
                    indexList.Add(faceVertexStart + 2);
                    totalVertexCount += 3;

                    if (face < quadCount)
                    {
                        Vector3 position3 = GetWorldPosition(entry.vertexIndex3, xShift, yShift, zShift);
                        AddRenderVertex(position3, entry.collisionType);
                        indexList.Add(faceVertexStart + 2);
                        indexList.Add(faceVertexStart);
                        indexList.Add(faceVertexStart + 3);
                        totalVertexCount++;
                    }
                }
            }

            public void AddStandardFaces(List<CollisionFaceOccurrence> faces, ushort xShift, ushort yShift, ushort zShift)
            {
                for (int face = 0; face < faceCount; face++)
                {
                    CollisionCellEntry entry = entries[face];
                    (Vector3 a, Vector3 errorA) = GetWorldPositionAndError(entry.vertexIndex0, xShift, yShift, zShift);
                    (Vector3 b, Vector3 errorB) = GetWorldPositionAndError(entry.vertexIndex1, xShift, yShift, zShift);
                    (Vector3 c, Vector3 errorC) = GetWorldPositionAndError(entry.vertexIndex2, xShift, yShift, zShift);
                    if (face < quadCount)
                    {
                        (Vector3 d, Vector3 errorD) = GetWorldPositionAndError(entry.vertexIndex3, xShift, yShift, zShift);
                        faces.Add(new CollisionFaceOccurrence(
                            new CollisionFace(a, b, c, d, entry.collisionType), errorA, errorB, errorC, errorD));
                    }
                    else
                    {
                        faces.Add(new CollisionFaceOccurrence(
                            new CollisionFace(a, b, c, entry.collisionType), errorA, errorB, errorC, default));
                    }
                }
            }

            public byte[] Serialize()
            {
                byte[] bytes = new byte[0x04 + faceCount * 0x04 + vertexCount * 0x0C + quadCount];

                WriteUshort(bytes, 0x00, faceCount);
                bytes[0x02] = vertexCount;
                bytes[0x03] = quadCount;

                for (int vertex = 0; vertex < vertexCount; vertex++)
                {
                    WriteFloat(bytes, 0x04 + vertex * 0x0C + 0x00, vertices[vertex].X);
                    WriteFloat(bytes, 0x04 + vertex * 0x0C + 0x04, vertices[vertex].Y);
                    WriteFloat(bytes, 0x04 + vertex * 0x0C + 0x08, vertices[vertex].Z);
                }

                for (int f = 0; f < faceCount; f++)
                {
                    CollisionCellEntry entry = entries[f];

                    // Collision Type
                    int fOffset = (vertexCount * 0x0C) + (f * 0x04);

                    bytes[0x04 + fOffset + 0x00] = entry.vertexIndex0;
                    bytes[0x04 + fOffset + 0x01] = entry.vertexIndex1;
                    bytes[0x04 + fOffset + 0x02] = entry.vertexIndex2;
                    bytes[0x04 + fOffset + 0x03] = entry.collisionType.data;

                    if (f < quadCount)
                    {
                        bytes[0x04 + vertexCount * 0x0C + faceCount * 0x04 + f] = entry.vertexIndex3;
                    }
                }

                return bytes;
            }
        }

        private List<CollisionCell> cells = new List<CollisionCell>();

        // Hero Collision

        private struct HeroCollisionCell
        {
            private ushort bsphereX;
            private ushort bsphereY;
            private ushort bsphereZ;
            private ushort bsphereRad;
            private ushort triCount;
            private ushort vertCount;
            private ushort[] vertices = [];
            private byte[] indices = [];

            public HeroCollisionCell(IReadOnlyList<CollisionTriangle> triangles, int start, int count)
            {
                triCount = checked((ushort) count);
                vertCount = checked((ushort) (count * 3));
                vertices = new ushort[vertCount * 3];
                indices = new byte[triCount * 3];

                for (int triangleIndex = 0; triangleIndex < count; triangleIndex++)
                {
                    CollisionTriangle triangle = triangles[start + triangleIndex];
                    int vertexIndex = triangleIndex * 3;
                    StorePosition(triangle.A, vertexIndex);
                    StorePosition(triangle.B, vertexIndex + 1);
                    StorePosition(triangle.C, vertexIndex + 2);
                    indices[triangleIndex * 3 + 0] = checked((byte) vertexIndex);
                    indices[triangleIndex * 3 + 1] = checked((byte) (vertexIndex + 1));
                    indices[triangleIndex * 3 + 2] = checked((byte) (vertexIndex + 2));
                }

                ApproxBsphere();
            }

            // Referenced from wrench
            // https://github.com/chaoticgd/wrench/blob/master/src/core/mesh.cpp#L295
            private void ApproxBsphere()
            {
                if (vertCount == 0)
                {
                    bsphereX = 0;
                    bsphereY = 0;
                    bsphereZ = 0;
                    bsphereRad = 0;
                    return;
                }

                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                for (int v = 0; v < vertCount; v++)
                {
                    min.X = Math.Min(vertices[v * 3 + 0], min.X);
                    min.Y = Math.Min(vertices[v * 3 + 1], min.Y);
                    min.Z = Math.Min(vertices[v * 3 + 2], min.Z);
                }

                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                for (int v = 0; v < vertCount; v++)
                {
                    max.X = Math.Max(vertices[v * 3 + 0], max.X);
                    max.Y = Math.Max(vertices[v * 3 + 1], max.Y);
                    max.Z = Math.Max(vertices[v * 3 + 2], max.Z);
                }

                Vector3 centre = (min + max) / 2f;

                float radius = 0f;
                for (int v = 0; v < vertCount; v++)
                {
                    Vector3 pos = new Vector3(vertices[v * 3 + 0], vertices[v * 3 + 1], vertices[v * 3 + 2]);
                    Vector3 delta = pos - centre;
                    radius = Math.Max((float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z), radius);
                }

                bsphereX = (ushort)centre.X;
                bsphereY = (ushort)centre.Y;
                bsphereZ = (ushort)centre.Z;
                bsphereRad = (ushort)radius;
            }

            private void StorePosition(System.Numerics.Vector3 position, int vertexIndex)
            {
                vertices[vertexIndex * 3 + 0] = ToSpecialCoordinate(position.X);
                vertices[vertexIndex * 3 + 1] = ToSpecialCoordinate(position.Y);
                vertices[vertexIndex * 3 + 2] = ToSpecialCoordinate(position.Z);
            }

            private static ushort ToSpecialCoordinate(float coordinate)
            {
                double scaled = coordinate * 64.0;
                double rounded = Math.Round(scaled);
                if (!double.IsFinite(scaled) || rounded < 0 || rounded > ushort.MaxValue)
                    throw new InvalidOperationException("Hero and unknown collision coordinates must be non-negative multiples of 1/64 within the unsigned 16-bit range.");
                return checked((ushort) rounded);
            }

            public HeroCollisionCell(FileStream fs, int baseOffset, int num, byte[] headerBlock)
            {
                int entryOffset = num * 0x10;

                bsphereX = ReadUshort(headerBlock, entryOffset + 0x00);
                bsphereY = ReadUshort(headerBlock, entryOffset + 0x02);
                bsphereZ = ReadUshort(headerBlock, entryOffset + 0x04);
                bsphereRad = ReadUshort(headerBlock, entryOffset + 0x06);
                triCount = ReadUshort(headerBlock, entryOffset + 0x08);
                vertCount = ReadUshort(headerBlock, entryOffset + 0x0A);
                int dataOffset = ReadInt(headerBlock, entryOffset + 0x0C);

                byte[] dataBlock = ReadBlock(fs, baseOffset + dataOffset, triCount * 0x04 + vertCount * 0x08);

                vertices = new ushort[vertCount * 3];
                indices = new byte[triCount * 3];

                for (int v = 0; v < vertCount; v++)
                {
                    int vOff = v * 0x08;
                    vertices[v * 3 + 0] = ReadUshort(dataBlock, vOff + 0x00);
                    vertices[v * 3 + 1] = ReadUshort(dataBlock, vOff + 0x02);
                    vertices[v * 3 + 2] = ReadUshort(dataBlock, vOff + 0x04);
                }

                for (int t = 0; t < triCount; t++)
                {
                    int tOff = vertCount * 0x08 + t * 0x04;
                    indices[t * 3 + 0] = dataBlock[tOff + 0x00];
                    indices[t * 3 + 1] = dataBlock[tOff + 0x01];
                    indices[t * 3 + 2] = dataBlock[tOff + 0x02];
                    //type = dataBlock[tOff + 0x03]; // Unused by the game
                }
            }

            public void GetModelData(List<float> vertexList, List<uint> indexList, ref uint totalVertexCount, CollisionGeometryCategory category)
            {
                for (int v = 0; v < vertCount; v++)
                {
                    vertexList.Add(vertices[v * 3 + 0] / 64.0f);
                    vertexList.Add(vertices[v * 3 + 1] / 64.0f);
                    vertexList.Add(vertices[v * 3 + 2] / 64.0f);
                    vertexList.Add(CollisionVertexMetadata.Pack(new CollisionType(0x80), category));
                }

                for (int t = 0; t < triCount; t++)
                {
                    indexList.Add(totalVertexCount + indices[t * 3 + 1]);
                    indexList.Add(totalVertexCount + indices[t * 3 + 0]);
                    indexList.Add(totalVertexCount + indices[t * 3 + 2]);
                }

                totalVertexCount += vertCount;
            }

            public void AddFaces(List<(CollisionFace face, CollisionGeometryCategory category)> faces, CollisionGeometryCategory category)
            {
                for (int triangleIndex = 0; triangleIndex < triCount; triangleIndex++)
                {
                    int indexOffset = triangleIndex * 3;
                    var face = new CollisionFace(
                        GetStoredPosition(indices[indexOffset + 0]),
                        GetStoredPosition(indices[indexOffset + 1]),
                        GetStoredPosition(indices[indexOffset + 2]),
                        new CollisionType(0x80));
                    faces.Add((face, category));
                }
            }

            private Vector3 GetStoredPosition(byte vertexIndex)
            {
                return new Vector3(
                    vertices[vertexIndex * 3 + 0] / 64.0f,
                    vertices[vertexIndex * 3 + 1] / 64.0f,
                    vertices[vertexIndex * 3 + 2] / 64.0f);
            }

            public byte[] Serialize(int num, byte[] headerBytes, ref int dataOffset)
            {
                int entryOffset = num * 0x10;

                WriteUshort(headerBytes, entryOffset + 0x00, bsphereX);
                WriteUshort(headerBytes, entryOffset + 0x02, bsphereY);
                WriteUshort(headerBytes, entryOffset + 0x04, bsphereZ);
                WriteUshort(headerBytes, entryOffset + 0x06, bsphereRad);
                WriteUshort(headerBytes, entryOffset + 0x08, triCount);
                WriteUshort(headerBytes, entryOffset + 0x0A, vertCount);
                WriteInt(headerBytes, entryOffset + 0x0C, dataOffset);

                byte[] bytes = new byte[triCount * 0x04 + vertCount * 0x08];

                for (int v = 0; v < vertCount; v++)
                {
                    int vOff = v * 0x08;
                    WriteUshort(bytes, vOff + 0x00, vertices[v * 3 + 0]);
                    WriteUshort(bytes, vOff + 0x02, vertices[v * 3 + 1]);
                    WriteUshort(bytes, vOff + 0x04, vertices[v * 3 + 2]);
                }

                for (int t = 0; t < triCount; t++)
                {
                    int tOff = vertCount * 0x08 + t * 0x04;
                    bytes[tOff + 0x00] = indices[t * 3 + 0];
                    bytes[tOff + 0x01] = indices[t * 3 + 1];
                    bytes[tOff + 0x02] = indices[t * 3 + 2];
                }

                dataOffset += bytes.Length;

                return bytes;
            }
        }

        private List<HeroCollisionCell> heroCells = new List<HeroCollisionCell>();
        private List<HeroCollisionCell> unkCells = new List<HeroCollisionCell>();

        public Collision()
        {
            vertexStride = 4;
        }

        public Collision(FileStream fs, int collisionPointer, bool import = false)
        {
            vertexStride = 4;

            // RaC 1 title screen has no collision
            if (collisionPointer == 0 && !import)
                return;

            byte[] headBlock = ReadBlock(fs, collisionPointer, 0x10);
            int standardCollisionStart = ReadInt(headBlock, 0x00);
            int heroCollisionStart = ReadInt(headBlock, 0x04);
            int unkCollisionStart = ReadInt(headBlock, 0x08);
            Utilities.DebugAssert(ReadInt(headBlock, 0x0C) == 0, "Header[0x0C] is not 0!");

            hasStandardCollision = standardCollisionStart > 0;
            hasHeroCollision = heroCollisionStart > 0;
            hasUnkCollision = unkCollisionStart > 0;

            if (hasStandardCollision)
                ParseStandardCollision(fs, collisionPointer + standardCollisionStart);

            if (hasHeroCollision)
                heroCells = ParseSpecialCollision(fs, collisionPointer + heroCollisionStart);

            if (hasUnkCollision)
                unkCells = ParseSpecialCollision(fs, collisionPointer + unkCollisionStart);

            var vertexList = new List<float>();
            var indexList = new List<uint>();
            uint totalVertexCount = 0;

            GenerateModelData(vertexList, indexList, ref totalVertexCount);

            vertexBuffer = vertexList.ToArray();
            indBuff = indexList.ToArray();
        }

        private static bool GetsFaceForCell(CollisionFace face, int x, int y, int z, out CollisionFace cellFace)
        {
            cellFace = face;
            var center = new Vector3(4 * x + 2, 4 * y + 2, 4 * z + 2);

            bool first = TestTriCellIntersection(center, face.A, face.B, face.C);
            if (!face.IsQuad)
                return first;

            bool second = TestTriCellIntersection(center, face.C, face.D, face.A);
            if (first && !second)
            {
                cellFace = new CollisionFace(face.A, face.B, face.C, face.Type);
                return true;
            }
            if (second && !first)
            {
                cellFace = new CollisionFace(face.C, face.D, face.A, face.Type);
                return true;
            }
            return first || second;
        }

        // Referenced from wrench
        // https://github.com/chaoticgd/wrench/blob/master/src/engine/collision.cpp#L680
        private static bool TestTriCellIntersection(Vector3 center, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = a - center, v1 = b - center, v2 = c - center;

            Vector3 f0 = v1 - v0;
            Vector3 f1 = v2 - v1;
            Vector3 f2 = v0 - v2;

            Vector3 u0 = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 u1 = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 u2 = new Vector3(0.0f, 0.0f, 1.0f);

            Vector3[] axes = new[]
            {
                Vector3.Cross(u0, f0), Vector3.Cross(u0, f1), Vector3.Cross(u0, f2),
                Vector3.Cross(u1, f0), Vector3.Cross(u1, f1), Vector3.Cross(u1, f2),
                Vector3.Cross(u2, f0), Vector3.Cross(u2, f1), Vector3.Cross(u2, f2),
                u0, u1, u2, Vector3.Cross(f0, f1)
            };

            foreach (Vector3 axis in axes)
            {
                float p0 = Vector3.Dot(v0, axis);
                float p1 = Vector3.Dot(v1, axis);
                float p2 = Vector3.Dot(v2, axis);

                float r = 0.0f;

                r += MathF.Abs(Vector3.Dot(u0, axis));
                r += MathF.Abs(Vector3.Dot(u1, axis));
                r += MathF.Abs(Vector3.Dot(u2, axis));
                r *= 2.0f;

                if (MathF.Max(-MathF.Max(p0, MathF.Max(p1, p2)), MathF.Min(p0, MathF.Min(p1, p2))) > r)
                    return false;
            }

            return true;
        }

        private static Collision FromFaces(IEnumerable<CollisionFace> faces, bool generateRenderData = true)
        {
            if (faces == null)
                throw new ArgumentNullException(nameof(faces));

            var cellFaces = new Dictionary<(int x, int y, int z), List<CollisionFace>>();
            long faceReferences = 0;

            foreach (CollisionFace face in faces)
            {
                int cornerCount = face.IsQuad ? 4 : 3;
                Vector3 first = face.GetCorner(0);
                float minX = first.X, maxX = first.X;
                float minY = first.Y, maxY = first.Y;
                float minZ = first.Z, maxZ = first.Z;
                for (int corner = 1; corner < cornerCount; corner++)
                {
                    Vector3 position = face.GetCorner(corner);
                    minX = Math.Min(minX, position.X);
                    maxX = Math.Max(maxX, position.X);
                    minY = Math.Min(minY, position.Y);
                    maxY = Math.Max(maxY, position.Y);
                    minZ = Math.Min(minZ, position.Z);
                    maxZ = Math.Max(maxZ, position.Z);
                }

                int minGridX = GetGridCoordinate(minX);
                int maxGridX = GetGridCoordinate(maxX);
                int minGridY = GetGridCoordinate(minY);
                int maxGridY = GetGridCoordinate(maxY);
                int faceMinGridZ = GetGridCoordinate(minZ);
                int faceMaxGridZ = GetGridCoordinate(maxZ);

                long coveredCells = (long) (maxGridX - minGridX + 1) * (maxGridY - minGridY + 1) * (faceMaxGridZ - faceMinGridZ + 1);
                faceReferences = checked(faceReferences + coveredCells);
                if (faceReferences > MaximumCellFaceReferences)
                    throw new InvalidOperationException("The collision geometry spans too many grid cells.");

                for (int z = faceMinGridZ; z <= faceMaxGridZ; z++)
                    for (int y = minGridY; y <= maxGridY; y++)
                        for (int x = minGridX; x <= maxGridX; x++)
                        {
                            if (!GetsFaceForCell(face, x, y, z, out CollisionFace cellFace))
                                continue;

                            var key = (x, y, z);
                            if (!cellFaces.TryGetValue(key, out List<CollisionFace>? cell))
                            {
                                cell = new List<CollisionFace>();
                                cellFaces.Add(key, cell);
                                if (cellFaces.Count > MaximumGridEntries)
                                    throw new InvalidOperationException("The collision geometry requires too many grid cells.");
                            }

                            if (cell.Count == ushort.MaxValue)
                                throw new InvalidOperationException("A collision cell cannot contain more than 65535 faces.");
                            cell.Add(cellFace);
                        }
            }

            var collision = new Collision();
            if (cellFaces.Count == 0)
                return collision;

            var byZ = new Dictionary<int, Dictionary<int, List<KeyValuePair<int, List<CollisionFace>>>>>();
            foreach (var cell in cellFaces)
            {
                if (!byZ.TryGetValue(cell.Key.z, out Dictionary<int, List<KeyValuePair<int, List<CollisionFace>>>>? byY))
                {
                    byY = new Dictionary<int, List<KeyValuePair<int, List<CollisionFace>>>>();
                    byZ.Add(cell.Key.z, byY);
                }
                if (!byY.TryGetValue(cell.Key.y, out List<KeyValuePair<int, List<CollisionFace>>>? byX))
                {
                    byX = new List<KeyValuePair<int, List<CollisionFace>>>();
                    byY.Add(cell.Key.y, byX);
                }
                byX.Add(new KeyValuePair<int, List<CollisionFace>>(cell.Key.x, cell.Value));
            }

            int minGridZ = byZ.Keys.Min();
            int maxGridZ = byZ.Keys.Max();
            int zCount = GetGridCount(minGridZ, maxGridZ);
            long gridEntries = zCount;
            var yRanges = new Dictionary<int, (int min, int max)>();
            var xRanges = new Dictionary<(int z, int y), (int min, int max)>();

            foreach (var zEntry in byZ)
            {
                int minY = zEntry.Value.Keys.Min();
                int maxY = zEntry.Value.Keys.Max();
                yRanges.Add(zEntry.Key, (minY, maxY));
                gridEntries += GetGridCount(minY, maxY);

                foreach (var yEntry in zEntry.Value)
                {
                    int minX = yEntry.Value.Min(cell => cell.Key);
                    int maxX = yEntry.Value.Max(cell => cell.Key);
                    xRanges.Add((zEntry.Key, yEntry.Key), (minX, maxX));
                    gridEntries += GetGridCount(minX, maxX);
                }
            }

            if (gridEntries > MaximumGridEntries)
                throw new InvalidOperationException("The collision grid is too sparse to serialize safely.");

            collision.zShift = checked((ushort) minGridZ);
            collision.zCount = checked((ushort) zCount);
            collision.yShift = new ushort[zCount];
            collision.yCount = new ushort[zCount];
            collision.xShift = new List<ushort[]>(zCount);
            collision.xCount = new List<ushort[]>(zCount);

            for (int zIndex = 0; zIndex < zCount; zIndex++)
            {
                int worldZ = minGridZ + zIndex;
                if (!byZ.TryGetValue(worldZ, out Dictionary<int, List<KeyValuePair<int, List<CollisionFace>>>>? byY))
                {
                    collision.xShift.Add([]);
                    collision.xCount.Add([]);
                    continue;
                }

                (int minY, int maxY) = yRanges[worldZ];
                int yCount = GetGridCount(minY, maxY);
                collision.yShift[zIndex] = checked((ushort) minY);
                collision.yCount[zIndex] = checked((ushort) yCount);
                var xShifts = new ushort[yCount];
                var xCounts = new ushort[yCount];

                foreach (var yEntry in byY)
                {
                    int yIndex = yEntry.Key - minY;
                    (int minX, int maxX) = xRanges[(worldZ, yEntry.Key)];
                    int xCount = GetGridCount(minX, maxX);
                    xShifts[yIndex] = checked((ushort) minX);
                    xCounts[yIndex] = checked((ushort) xCount);

                    foreach (var cell in yEntry.Value)
                    {
                        int xIndex = cell.Key - minX;
                        collision.cells.Add(new CollisionCell(
                            cell.Value,
                            checked((ushort) xIndex),
                            checked((ushort) yIndex),
                            checked((ushort) zIndex),
                            cell.Key,
                            yEntry.Key,
                            worldZ));
                    }
                }

                collision.xShift.Add(xShifts);
                collision.xCount.Add(xCounts);
            }

            collision.hasStandardCollision = true;
            if (generateRenderData)
                collision.GenerateRenderData();
            return collision;
        }

        internal static Collision FromFaces(
            IEnumerable<CollisionFace> standardFaces,
            IEnumerable<CollisionTriangle> heroTriangles,
            IEnumerable<CollisionTriangle> unknownTriangles)
        {
            if (standardFaces == null)
                throw new ArgumentNullException(nameof(standardFaces));
            if (heroTriangles == null)
                throw new ArgumentNullException(nameof(heroTriangles));
            if (unknownTriangles == null)
                throw new ArgumentNullException(nameof(unknownTriangles));

            List<CollisionTriangle> hero = heroTriangles.ToList();
            List<CollisionTriangle> unknown = unknownTriangles.ToList();
            if (hero.Any(triangle => triangle.Type.data != 0x80) || unknown.Any(triangle => triangle.Type.data != 0x80))
                throw new ArgumentException("Hero and unknown collision triangles must use collision type 0x80.");

            Collision collision = FromFaces(standardFaces, generateRenderData: false);
            AddSpecialCells(collision.heroCells, hero);
            AddSpecialCells(collision.unkCells, unknown);
            collision.hasHeroCollision = hero.Count > 0;
            collision.hasUnkCollision = unknown.Count > 0;
            collision.GenerateRenderData();
            return collision;
        }

        private static void AddSpecialCells(List<HeroCollisionCell> cells, List<CollisionTriangle> triangles)
        {
            const int trianglesPerCell = byte.MaxValue / 3;
            for (int start = 0; start < triangles.Count; start += trianglesPerCell)
            {
                int count = Math.Min(trianglesPerCell, triangles.Count - start);
                cells.Add(new HeroCollisionCell(triangles, start, count));
            }
        }

        private List<CollisionFace> GetStandardFaces()
        {
            var groups = new List<StandardFaceGroup>();
            float maximumPositionError = 0.0f;
            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                CollisionCell cell = cells[cellIndex];
                float cellError = cell.GetMaximumPositionError(
                    xShift[cell.zCell][cell.yCell], yShift[cell.zCell], zShift);
                maximumPositionError = Math.Max(maximumPositionError, cellError);
            }

            double bucketSize = Math.Max(maximumPositionError * 2.0, double.Epsilon);
            var buckets = new Dictionary<FaceBucketKey, List<int>>();
            var candidateGroups = new List<int>();

            long GetBucketCoordinate(double coordinate)
            {
                return checked((long) Math.Floor(coordinate / bucketSize));
            }

            FaceBucketKey GetBucket(CollisionFace face)
            {
                int cornerCount = face.IsQuad ? 4 : 3;
                double centroidX = 0.0;
                double centroidY = 0.0;
                double centroidZ = 0.0;
                for (int corner = 0; corner < cornerCount; corner++)
                {
                    Vector3 position = face.GetCorner(corner);
                    centroidX += position.X;
                    centroidY += position.Y;
                    centroidZ += position.Z;
                }

                return new FaceBucketKey(
                    GetBucketCoordinate(centroidX / cornerCount),
                    GetBucketCoordinate(centroidY / cornerCount),
                    GetBucketCoordinate(centroidZ / cornerCount),
                    checked((byte) cornerCount),
                    face.Type.data);
            }

            void AddNearbyGroups(CollisionFace face)
            {
                FaceBucketKey bucket = GetBucket(face);
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                        for (int z = -1; z <= 1; z++)
                        {
                            var neighbor = new FaceBucketKey(bucket.X + x, bucket.Y + y, bucket.Z + z, bucket.CornerCount, bucket.Type);
                            if (!buckets.TryGetValue(neighbor, out List<int>? matchingGroups))
                                continue;
                            candidateGroups.AddRange(matchingGroups);
                        }
            }

            void IndexGroup(int groupIndex)
            {
                CollisionFaceOccurrence occurrence = groups[groupIndex].Occurrence;
                FaceBucketKey bucket = GetBucket(occurrence.Face);
                if (!buckets.TryGetValue(bucket, out List<int>? matchingGroups))
                {
                    matchingGroups = new List<int>();
                    buckets.Add(bucket, matchingGroups);
                }
                matchingGroups.Add(groupIndex);
            }

            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                CollisionCell cell = cells[cellIndex];
                var cellFaces = new List<CollisionFaceOccurrence>();
                cell.AddStandardFaces(cellFaces,
                    xShift[cell.zCell][cell.yCell], yShift[cell.zCell], zShift);

                var cellMultiplicity = new Dictionary<StandardFaceKey, (CollisionFaceOccurrence occurrence, int count)>();
                foreach (CollisionFaceOccurrence occurrence in cellFaces)
                {
                    StandardFaceKey key = StandardFaceKey.Create(occurrence.Face);
                    if (cellMultiplicity.TryGetValue(key, out var current))
                        cellMultiplicity[key] = (current.occurrence, current.count + 1);
                    else
                        cellMultiplicity.Add(key, (occurrence, 1));
                }

                foreach (var entry in cellMultiplicity)
                {
                    CollisionFaceOccurrence occurrence = entry.Value.occurrence;
                    candidateGroups.Clear();
                    AddNearbyGroups(occurrence.Face);

                    int bestGroupIndex = -1;
                    double bestScore = double.PositiveInfinity;
                    bool ambiguousMatch = false;
                    foreach (int candidateGroupIndex in candidateGroups)
                    {
                        StandardFaceGroup candidate = groups[candidateGroupIndex];
                        if (candidate.LastCellIndex == cellIndex)
                            continue;

                        double score = GetStandardFaceMatchScore(occurrence, candidate.Occurrence);
                        if (score < 0.0)
                            continue;
                        if (score < bestScore - 1e-9)
                        {
                            bestGroupIndex = candidateGroupIndex;
                            bestScore = score;
                            ambiguousMatch = false;
                        }
                        else if (Math.Abs(score - bestScore) <= 1e-9)
                        {
                            ambiguousMatch = true;
                        }
                    }

                    if (bestGroupIndex >= 0 && !ambiguousMatch)
                    {
                        StandardFaceGroup group = groups[bestGroupIndex];
                        group.MaximumMultiplicity = Math.Max(group.MaximumMultiplicity, entry.Value.count);
                        group.LastCellIndex = cellIndex;
                        continue;
                    }

                    int newGroupIndex = groups.Count;
                    groups.Add(new StandardFaceGroup(occurrence, entry.Value.count, cellIndex));
                    IndexGroup(newGroupIndex);
                }
            }

            var faces = new List<CollisionFace>();
            foreach (StandardFaceGroup group in groups)
                for (int copy = 0; copy < group.MaximumMultiplicity; copy++)
                    faces.Add(group.Occurrence.Face);

            return faces;
        }

        private static double GetFloatSpacing(float value)
        {
            float increment = MathF.BitIncrement(value);
            float decrement = MathF.BitDecrement(value);
            double incrementSpacing = float.IsFinite(increment) ? Math.Abs((double) increment - value) : 0.0;
            double decrementSpacing = float.IsFinite(decrement) ? Math.Abs((double) value - decrement) : 0.0;
            return Math.Max(incrementSpacing, decrementSpacing);
        }

        private static float GetPositionError(float local, float world)
        {
            double localError = 2.0 * GetFloatSpacing(local) / 1024.0;
            double worldRoundingError = GetFloatSpacing(world);
            return (float) (localError + worldRoundingError);
        }

        private static double GetVertexMatchScore(Vector3 first, Vector3 second, Vector3 firstError, Vector3 secondError)
        {
            double xTolerance = (double) firstError.X + secondError.X;
            double yTolerance = (double) firstError.Y + secondError.Y;
            double zTolerance = (double) firstError.Z + secondError.Z;
            double xDifference = Math.Abs((double) first.X - second.X);
            double yDifference = Math.Abs((double) first.Y - second.Y);
            double zDifference = Math.Abs((double) first.Z - second.Z);
            if (xDifference > xTolerance || yDifference > yTolerance || zDifference > zTolerance)
                return double.PositiveInfinity;

            double xScore = xTolerance == 0.0 ? 0.0 : xDifference / xTolerance;
            double yScore = yTolerance == 0.0 ? 0.0 : yDifference / yTolerance;
            double zScore = zTolerance == 0.0 ? 0.0 : zDifference / zTolerance;
            return Math.Max(xScore, Math.Max(yScore, zScore));
        }

        private static double GetStandardFaceMatchScore(CollisionFaceOccurrence first, CollisionFaceOccurrence second)
        {
            if (first.Face.Type.data != second.Face.Type.data || first.Face.IsQuad != second.Face.IsQuad)
                return -1.0;

            int cornerCount = first.Face.IsQuad ? 4 : 3;
            double score = double.PositiveInfinity;
            for (int rotation = 0; rotation < cornerCount; rotation++)
            {
                double rotationScore = 0.0;
                for (int corner = 0; corner < cornerCount; corner++)
                {
                    int otherCorner = (corner + rotation) % cornerCount;
                    double vertexScore = GetVertexMatchScore(
                        first.Face.GetCorner(corner),
                        second.Face.GetCorner(otherCorner),
                        first.GetError(corner),
                        second.GetError(otherCorner));
                    rotationScore = Math.Max(rotationScore, vertexScore);
                }

                score = Math.Min(score, rotationScore);
            }

            return double.IsPositiveInfinity(score) ? -1.0 : score;
        }

        internal List<(CollisionFace face, CollisionGeometryCategory category)> GetFacesForExport()
        {
            var faces = GetStandardFaces()
                .Select(face => (face, CollisionGeometryCategory.Standard))
                .ToList();

            foreach (HeroCollisionCell cell in heroCells)
                cell.AddFaces(faces, CollisionGeometryCategory.Hero);
            foreach (HeroCollisionCell cell in unkCells)
                cell.AddFaces(faces, CollisionGeometryCategory.Unknown);

            return faces;
        }

        private static int GetGridCoordinate(float coordinate)
        {
            if (!float.IsFinite(coordinate))
                throw new ArgumentOutOfRangeException(nameof(coordinate), "Collision coordinates must be finite.");

            double gridCoordinate = Math.Floor(coordinate / 4.0);
            if (gridCoordinate < 0 || gridCoordinate > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(coordinate), "Collision coordinates must map to a non-negative 16-bit grid cell.");
            return (int) gridCoordinate;
        }

        private static int GetGridCount(int min, int max)
        {
            long count = (long) max - min + 1;
            if (count <= 0 || count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(max), "Collision grid dimensions must fit in an unsigned 16-bit count.");
            return (int) count;
        }

        private void GenerateRenderData()
        {
            var vertexList = new List<float>();
            var indexList = new List<uint>();
            uint totalVertexCount = 0;
            GenerateModelData(vertexList, indexList, ref totalVertexCount);
            vertexBuffer = vertexList.ToArray();
            indBuff = indexList.ToArray();
        }

        private void ParseStandardCollision(FileStream fs, int baseOffset)
        {
            byte[] headZBlock = ReadBlock(fs, baseOffset, 0x04);

            zShift = ReadUshort(headZBlock, 0);
            zCount = ReadUshort(headZBlock, 2);
            yShift = new ushort[zCount];
            yCount = new ushort[zCount];
            xShift = new List<ushort[]>(zCount);
            xCount = new List<ushort[]>(zCount);

            byte[] zBlock = ReadBlock(fs, baseOffset + 0x04, zCount * 0x04);

            for (ushort z = 0; z < zCount; z++)
            {
                int yOffset = ReadInt(zBlock, z * 0x04);
                if (yOffset == 0)
                {
                    xShift.Add([]);
                    xCount.Add([]);
                    continue;
                }

                byte[] headYBlock = ReadBlock(fs, baseOffset + yOffset, 0x04);

                yShift[z] = ReadUshort(headYBlock, 0x00);
                yCount[z] = ReadUshort(headYBlock, 0x02);

                Utilities.DebugAssert(yCount[z] != 0, "Unexpected yCount.");

                byte[] yBlock = ReadBlock(fs, baseOffset + yOffset + 0x04, yCount[z] * 0x04);

                xShift.Add(new ushort[yCount[z]]);
                xCount.Add(new ushort[yCount[z]]);

                for (ushort y = 0; y < yCount[z]; y++)
                {
                    int xOffset = ReadInt(yBlock, y * 0x04);
                    if (xOffset == 0) continue;

                    byte[] headXBlock = ReadBlock(fs, baseOffset + xOffset, 0x04);

                    xShift[z][y] = ReadUshort(headXBlock, 0x00);
                    xCount[z][y] = ReadUshort(headXBlock, 0x02);

                    Utilities.DebugAssert(xCount[z][y] != 0, "Unexpected xCount.");

                    byte[] xBlock = ReadBlock(fs, baseOffset + xOffset + 0x04, xCount[z][y] * 0x04);

                    for (ushort x = 0; x < xCount[z][y]; x++)
                    {
                        int vOffset = ReadInt(xBlock, x * 0x04);
                        if (vOffset == 0) continue;

                        cells.Add(new CollisionCell(fs, baseOffset + vOffset, x, y, z));
                    }
                }
            }
        }

        private static List<HeroCollisionCell> ParseSpecialCollision(FileStream fs, int baseOffset)
        {
            byte[] headBlock = ReadBlock(fs, baseOffset, 0x10);

            int cellCount = ReadInt(headBlock, 0x00);
            Utilities.DebugAssert(ReadInt(headBlock, 0x04) == 0, "Header[0x04] is not 0!");
            Utilities.DebugAssert(ReadInt(headBlock, 0x08) == 0, "Header[0x08] is not 0!");
            Utilities.DebugAssert(ReadInt(headBlock, 0x0C) == 0, "Header[0x0C] is not 0!");

            var cells = new List<HeroCollisionCell>(cellCount);
            byte[] cellHeaderBlock = ReadBlock(fs, baseOffset + 0x10, cellCount * 0x10);

            for (int i = 0; i < cellCount; i++)
                cells.Add(new HeroCollisionCell(fs, baseOffset, i, cellHeaderBlock));

            return cells;
        }

        private void GenerateModelData(List<float> vertexList, List<uint> indexList, ref uint totalVertexCount)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                CollisionCell cell = cells[i];
                cell.GetModelData(xShift[cell.zCell][cell.yCell], yShift[cell.zCell], zShift, vertexList, indexList, ref totalVertexCount);
            }

            for (int i = 0; i < heroCells.Count; i++)
            {
                heroCells[i].GetModelData(vertexList, indexList, ref totalVertexCount, CollisionGeometryCategory.Hero);
            }

            for (int i = 0; i < unkCells.Count; i++)
            {
                unkCells[i].GetModelData(vertexList, indexList, ref totalVertexCount, CollisionGeometryCategory.Unknown);
            }
        }

        public byte[] Serialize()
        {
            if (hasStandardCollision == false && hasHeroCollision == false && hasUnkCollision == false) return [];

            byte[] standardCollisionBlock = hasStandardCollision ? SerializeStandardCollision() : [];
            byte[] heroCollisionBlock = hasHeroCollision ? SerializeSpecialCollision(heroCells) : [];
            byte[] unkCollisionBlock = hasUnkCollision ? SerializeSpecialCollision(unkCells) : [];

            int offset = 0x10;

            int standardCollisionStart = 0;
            if (hasStandardCollision)
            {
                standardCollisionStart = AlignAddressUp(offset);
                offset = standardCollisionStart + standardCollisionBlock.Length;
            }

            int heroCollisionStart = 0;
            if (hasHeroCollision)
            {
                heroCollisionStart = AlignAddressUp(offset);
                offset = heroCollisionStart + heroCollisionBlock.Length;
            }

            int unkCollisionStart = 0;
            if (hasUnkCollision)
            {
                unkCollisionStart = AlignAddressUp(offset);
                offset = unkCollisionStart + unkCollisionBlock.Length;
            }

            byte[] bytes = new byte[offset];

            WriteInt(bytes, 0x00, standardCollisionStart);
            WriteInt(bytes, 0x04, heroCollisionStart);
            WriteInt(bytes, 0x08, unkCollisionStart);

            if (hasStandardCollision)
                standardCollisionBlock.CopyTo(bytes, standardCollisionStart);

            if (hasHeroCollision)
                heroCollisionBlock.CopyTo(bytes, heroCollisionStart);

            if (hasUnkCollision)
                unkCollisionBlock.CopyTo(bytes, unkCollisionStart);

            return bytes;
        }

        private byte[] SerializeStandardCollision()
        {
            // Serialize all cells
            var cellBytes = new Dictionary<(ushort z, ushort y, ushort x), byte[]>();
            foreach (var cell in cells)
            {
                cellBytes[(cell.zCell, cell.yCell, cell.xCell)] = cell.Serialize();
            }

            // Compute total size: Z header + Z block + Y headers/blocks + X headers/blocks + cell data
            int totalSize = 0x04 + zCount * 0x04;

            for (ushort z = 0; z < zCount; z++)
            {
                if (yCount[z] == 0) continue;
                totalSize += 0x04 + yCount[z] * 0x04;
                for (ushort y = 0; y < yCount[z]; y++)
                {
                    if (xCount[z][y] == 0) continue;
                    totalSize += 0x04 + xCount[z][y] * 0x04;
                    for (ushort x = 0; x < xCount[z][y]; x++)
                    {
                        if (cellBytes.TryGetValue((z, y, x), out byte[]? cellEntryBytes))
                        {
                            totalSize = AlignAddressUp(totalSize, 0x04);
                            totalSize += cellEntryBytes.Length;
                        }
                    }
                }
            }

            byte[] bytes = new byte[totalSize];

            // Z header
            WriteUshort(bytes, 0x00, zShift);
            WriteUshort(bytes, 0x02, zCount);

            // Z block + Y data
            int currentOffset = 0x04 + zCount * 0x04;

            for (ushort z = 0; z < zCount; z++)
            {
                if (yCount[z] == 0)
                {
                    WriteInt(bytes, 0x04 + z * 0x04, 0);
                    continue;
                }

                WriteInt(bytes, 0x04 + z * 0x04, currentOffset);

                // Y header
                WriteUshort(bytes, currentOffset, yShift[z]);
                WriteUshort(bytes, currentOffset + 0x02, yCount[z]);

                // Y block (offsets to X headers)
                int yBlockOffset = currentOffset + 0x04;
                int xDataOffset = yBlockOffset + yCount[z] * 0x04;

                for (ushort y = 0; y < yCount[z]; y++)
                {
                    if (xCount[z][y] == 0)
                    {
                        WriteInt(bytes, yBlockOffset + y * 0x04, 0);
                        continue;
                    }

                    WriteInt(bytes, yBlockOffset + y * 0x04, xDataOffset);

                    // X header
                    WriteUshort(bytes, xDataOffset, xShift[z][y]);
                    WriteUshort(bytes, xDataOffset + 0x02, xCount[z][y]);

                    // X block (offsets to cell data)
                    int xBlockOffset = xDataOffset + 0x04;
                    int cellDataOffset = xBlockOffset + xCount[z][y] * 0x04;

                    for (ushort x = 0; x < xCount[z][y]; x++)
                    {
                        if (cellBytes.TryGetValue((z, y, x), out byte[]? cellData))
                        {
                            cellDataOffset = AlignAddressUp(cellDataOffset, 0x04);
                            WriteInt(bytes, xBlockOffset + x * 0x04, cellDataOffset);
                            Buffer.BlockCopy(cellData, 0, bytes, cellDataOffset, cellData.Length);
                            cellDataOffset += cellData.Length;
                        }
                        else
                        {
                            WriteInt(bytes, xBlockOffset + x * 0x04, 0);
                        }
                    }

                    cellDataOffset = AlignAddressUp(cellDataOffset, 0x04);
                    xDataOffset = cellDataOffset;
                }

                currentOffset = xDataOffset;
            }

            return bytes;
        }

        private byte[] SerializeSpecialCollision(List<HeroCollisionCell> cells)
        {
            byte[] headerBytes = new byte[cells.Count * 0x10];

            List<byte[]> cellBytes = new List<byte[]>(cells.Count);

            int dataOffset = 0x10 + headerBytes.Length;

            for (int i = 0; i < cells.Count; i++)
            {
                dataOffset = AlignAddressUp(dataOffset);
                cellBytes.Add(cells[i].Serialize(i, headerBytes, ref dataOffset));
            }

            byte[] bytes = new byte[dataOffset];

            WriteInt(bytes, 0x00, cells.Count);

            int offset = 0x10;
            headerBytes.CopyTo(bytes, offset);

            offset += headerBytes.Length;

            for (int i = 0; i < cells.Count; i++)
            {
                offset = AlignAddressUp(offset);
                cellBytes[i].CopyTo(bytes, offset);
                offset += cellBytes[i].Length;
            }

            return bytes;
        }
    }
}
