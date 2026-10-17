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

    public class Collision : Model, IRenderable
    {
        private const int MaximumGridEntries = 1_000_000;
        private const long MaximumCellTriangleReferences = 5_000_000;

        private readonly record struct StandardTriangleKey(Vector3 A, Vector3 B, Vector3 C, byte Type)
        {
            public static StandardTriangleKey Create(CollisionTriangle triangle)
            {
                Vector3 originalA = triangle.A;
                Vector3 originalB = triangle.B;
                Vector3 originalC = triangle.C;
                Vector3 keyA = originalA;
                Vector3 keyB = originalB;
                Vector3 keyC = originalC;
                if (CompareRotation(originalB, originalC, originalA, keyA, keyB, keyC) < 0)
                    (keyA, keyB, keyC) = (originalB, originalC, originalA);
                if (CompareRotation(originalC, originalA, originalB, keyA, keyB, keyC) < 0)
                    (keyA, keyB, keyC) = (originalC, originalA, originalB);
                return new StandardTriangleKey(keyA, keyB, keyC, triangle.Type.data);
            }

            private static int CompareRotation(Vector3 a, Vector3 b, Vector3 c, Vector3 otherA, Vector3 otherB, Vector3 otherC)
            {
                int comparison = Compare(a, otherA);
                if (comparison != 0) return comparison;
                comparison = Compare(b, otherB);
                return comparison != 0 ? comparison : Compare(c, otherC);
            }

            private static int Compare(Vector3 first, Vector3 second)
            {
                int comparison = first.X.CompareTo(second.X);
                if (comparison != 0) return comparison;
                comparison = first.Y.CompareTo(second.Y);
                return comparison != 0 ? comparison : first.Z.CompareTo(second.Z);
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

            public CollisionCell(List<CollisionTriangle> triangles, ushort x, ushort y, ushort z, int worldX, int worldY, int worldZ)
            {
                xCell = x;
                yCell = y;
                zCell = z;
                int inputFaceCount = checked((ushort) triangles.Count);
                quadCount = 0;
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

                for (int i = 0; i < inputFaceCount; i++)
                {
                    CollisionTriangle triangle = triangles[i];
                    Vector3 localA = GetLocalPosition(triangle.A);
                    Vector3 localB = GetLocalPosition(triangle.B);
                    Vector3 localC = GetLocalPosition(triangle.C);
                    int newVertexCount = 0;
                    if (!vertexIndices.ContainsKey(localA)) newVertexCount++;
                    if (!vertexIndices.ContainsKey(localB) && !localB.Equals(localA)) newVertexCount++;
                    if (!vertexIndices.ContainsKey(localC) && !localC.Equals(localA) && !localC.Equals(localB)) newVertexCount++;
                    if (sharedVertices.Count + newVertexCount > byte.MaxValue)
                        throw new InvalidOperationException("A collision cell cannot contain more than 255 unique vertices.");

                    entries[faceCount] = new CollisionCellEntry
                    {
                        vertexIndex0 = GetVertexIndex(localA),
                        vertexIndex1 = GetVertexIndex(localB),
                        vertexIndex2 = GetVertexIndex(localC),
                        vertexIndex3 = 0xFF,
                        collisionType = triangle.Type
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

            public void GetModelData(ushort xShift, ushort yShift, ushort zShift, List<float> vertexList, List<uint> indexList, ref uint totalVertexCount)
            {
                Vector3 GetWorldPosition(byte vertexIndex)
                {
                    Vector3 local = vertices[vertexIndex];
                    return new Vector3(
                        local.X / 1024.0f + 4 * (xShift + xCell + 0.5f),
                        local.Y / 1024.0f + 4 * (yShift + yCell + 0.5f),
                        local.Z / 1024.0f + 4 * (zShift + zCell + 0.5f));
                }

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
                    Vector3 position0 = GetWorldPosition(entry.vertexIndex0);
                    Vector3 position1 = GetWorldPosition(entry.vertexIndex1);
                    Vector3 position2 = GetWorldPosition(entry.vertexIndex2);
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
                        Vector3 position3 = GetWorldPosition(entry.vertexIndex3);
                        AddRenderVertex(position3, entry.collisionType);
                        indexList.Add(faceVertexStart + 2);
                        indexList.Add(faceVertexStart);
                        indexList.Add(faceVertexStart + 3);
                        totalVertexCount++;
                    }
                }
            }

            public void AddStandardTriangles(List<CollisionTriangle> triangles, ushort xShift, ushort yShift, ushort zShift)
            {
                Vector3 GetVertex(byte vertexIndex)
                {
                    Vector3 local = vertices[vertexIndex];
                    return new Vector3(
                        local.X / 1024.0f + 4.0f * (xShift + xCell + 0.5f),
                        local.Y / 1024.0f + 4.0f * (yShift + yCell + 0.5f),
                        local.Z / 1024.0f + 4.0f * (zShift + zCell + 0.5f));
                }

                for (int face = 0; face < faceCount; face++)
                {
                    CollisionCellEntry entry = entries[face];
                    Vector3 a = GetVertex(entry.vertexIndex0);
                    Vector3 b = GetVertex(entry.vertexIndex1);
                    Vector3 c = GetVertex(entry.vertexIndex2);
                    triangles.Add(new CollisionTriangle(a, b, c, entry.collisionType));

                    if (face < quadCount)
                    {
                        Vector3 d = GetVertex(entry.vertexIndex3);
                        triangles.Add(new CollisionTriangle(a, c, d, entry.collisionType));
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
            private float unk0x00;
            private ushort unk0x04;
            private ushort unk0x06;
            private ushort triCount;
            private ushort vertCount;
            private ushort[] vertices = [];
            private byte[] indices = [];

            public HeroCollisionCell(IReadOnlyList<CollisionTriangle> triangles, int start, int count)
            {
                // OBJ does not encode these section-specific header values.
                unk0x00 = 0;
                unk0x04 = 0;
                unk0x06 = 0;
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
                if (!double.IsFinite(scaled) || scaled < 0 || rounded > ushort.MaxValue || Math.Abs(scaled - rounded) > 0.0001)
                    throw new InvalidOperationException("Hero and unknown collision coordinates must be non-negative multiples of 1/64 within the unsigned 16-bit range.");
                return checked((ushort) rounded);
            }

            public HeroCollisionCell(FileStream fs, int baseOffset, int num, byte[] headerBlock)
            {
                int entryOffset = num * 0x10;

                unk0x00 = ReadFloat(headerBlock, entryOffset + 0x00);
                unk0x04 = ReadUshort(headerBlock, entryOffset + 0x04);
                unk0x06 = ReadUshort(headerBlock, entryOffset + 0x06);
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

            public void AddTriangles(List<(CollisionTriangle triangle, CollisionGeometryCategory category)> triangles, CollisionGeometryCategory category)
            {
                for (int triangleIndex = 0; triangleIndex < triCount; triangleIndex++)
                {
                    int indexOffset = triangleIndex * 3;
                    var triangle = new CollisionTriangle(
                        GetStoredPosition(indices[indexOffset + 0]),
                        GetStoredPosition(indices[indexOffset + 1]),
                        GetStoredPosition(indices[indexOffset + 2]),
                        new CollisionType(0x80));
                    triangles.Add((triangle, category));
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

                WriteFloat(headerBytes, entryOffset + 0x00, unk0x00);
                WriteUshort(headerBytes, entryOffset + 0x04, unk0x04);
                WriteUshort(headerBytes, entryOffset + 0x06, unk0x06);
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
                ParseHeroCollision(fs, collisionPointer + heroCollisionStart);

            if (hasUnkCollision)
                ParseUnkCollision(fs, collisionPointer + unkCollisionStart);

            var vertexList = new List<float>();
            var indexList = new List<uint>();
            uint totalVertexCount = 0;

            GenerateModelData(vertexList, indexList, ref totalVertexCount);

            vertexBuffer = vertexList.ToArray();
            indBuff = indexList.ToArray();
        }

        internal static Collision FromTriangles(IEnumerable<CollisionTriangle> triangles, bool generateRenderData = true)
        {
            if (triangles == null)
                throw new ArgumentNullException(nameof(triangles));

            var cellTriangles = new Dictionary<(int x, int y, int z), List<CollisionTriangle>>();
            long triangleReferences = 0;

            foreach (CollisionTriangle triangle in triangles)
            {
                int minX = GetGridCoordinate(Math.Min(triangle.A.X, Math.Min(triangle.B.X, triangle.C.X)));
                int maxX = GetGridCoordinate(Math.Max(triangle.A.X, Math.Max(triangle.B.X, triangle.C.X)));
                int minY = GetGridCoordinate(Math.Min(triangle.A.Y, Math.Min(triangle.B.Y, triangle.C.Y)));
                int maxY = GetGridCoordinate(Math.Max(triangle.A.Y, Math.Max(triangle.B.Y, triangle.C.Y)));
                int minZ = GetGridCoordinate(Math.Min(triangle.A.Z, Math.Min(triangle.B.Z, triangle.C.Z)));
                int maxZ = GetGridCoordinate(Math.Max(triangle.A.Z, Math.Max(triangle.B.Z, triangle.C.Z)));

                long coveredCells = (long) (maxX - minX + 1) * (maxY - minY + 1) * (maxZ - minZ + 1);
                triangleReferences = checked(triangleReferences + coveredCells);
                if (triangleReferences > MaximumCellTriangleReferences)
                    throw new InvalidOperationException("The collision geometry spans too many grid cells.");

                for (int z = minZ; z <= maxZ; z++)
                    for (int y = minY; y <= maxY; y++)
                        for (int x = minX; x <= maxX; x++)
                        {
                            var key = (x, y, z);
                            if (!cellTriangles.TryGetValue(key, out List<CollisionTriangle>? cell))
                            {
                                cell = new List<CollisionTriangle>();
                                cellTriangles.Add(key, cell);
                                if (cellTriangles.Count > MaximumGridEntries)
                                    throw new InvalidOperationException("The collision geometry requires too many grid cells.");
                            }

                            if (cell.Count == ushort.MaxValue)
                                throw new InvalidOperationException("A collision cell cannot contain more than 65535 faces.");
                            cell.Add(triangle);
                        }
            }

            var collision = new Collision();
            if (cellTriangles.Count == 0)
                return collision;

            var byZ = new Dictionary<int, Dictionary<int, List<KeyValuePair<int, List<CollisionTriangle>>>>>();
            foreach (var cell in cellTriangles)
            {
                if (!byZ.TryGetValue(cell.Key.z, out Dictionary<int, List<KeyValuePair<int, List<CollisionTriangle>>>>? byY))
                {
                    byY = new Dictionary<int, List<KeyValuePair<int, List<CollisionTriangle>>>>();
                    byZ.Add(cell.Key.z, byY);
                }
                if (!byY.TryGetValue(cell.Key.y, out List<KeyValuePair<int, List<CollisionTriangle>>>? byX))
                {
                    byX = new List<KeyValuePair<int, List<CollisionTriangle>>>();
                    byY.Add(cell.Key.y, byX);
                }
                byX.Add(new KeyValuePair<int, List<CollisionTriangle>>(cell.Key.x, cell.Value));
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
                if (!byZ.TryGetValue(worldZ, out Dictionary<int, List<KeyValuePair<int, List<CollisionTriangle>>>>? byY))
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

        internal static Collision FromTriangles(
            IEnumerable<CollisionTriangle> standardTriangles,
            IEnumerable<CollisionTriangle> heroTriangles,
            IEnumerable<CollisionTriangle> unknownTriangles)
        {
            if (standardTriangles == null)
                throw new ArgumentNullException(nameof(standardTriangles));
            if (heroTriangles == null)
                throw new ArgumentNullException(nameof(heroTriangles));
            if (unknownTriangles == null)
                throw new ArgumentNullException(nameof(unknownTriangles));

            List<CollisionTriangle> hero = heroTriangles.ToList();
            List<CollisionTriangle> unknown = unknownTriangles.ToList();
            if (hero.Any(triangle => triangle.Type.data != 0x80) || unknown.Any(triangle => triangle.Type.data != 0x80))
                throw new ArgumentException("Hero and unknown collision triangles must use collision type 0x80.");

            Collision collision = FromTriangles(standardTriangles, generateRenderData: false);
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

        public static Collision Concatenate(Collision first, Collision second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var triangles = first.GetStandardTriangles();
            triangles.AddRange(second.GetStandardTriangles());
            Collision collision = FromTriangles(triangles, generateRenderData: false);

            collision.heroCells.AddRange(first.heroCells);
            collision.heroCells.AddRange(second.heroCells);
            collision.unkCells.AddRange(first.unkCells);
            collision.unkCells.AddRange(second.unkCells);
            collision.hasHeroCollision = first.hasHeroCollision || second.hasHeroCollision;
            collision.hasUnkCollision = first.hasUnkCollision || second.hasUnkCollision;
            collision.GenerateRenderData();
            return collision;
        }

        private List<CollisionTriangle> GetStandardTriangles()
        {
            var triangles = new List<CollisionTriangle>();
            var maximumMultiplicity = new Dictionary<StandardTriangleKey, int>();
            var representativeTriangles = new Dictionary<StandardTriangleKey, CollisionTriangle>();
            foreach (CollisionCell cell in cells)
            {
                var cellTriangles = new List<CollisionTriangle>();
                cell.AddStandardTriangles(cellTriangles,
                    xShift[cell.zCell][cell.yCell],
                    yShift[cell.zCell],
                    zShift);

                var cellMultiplicity = new Dictionary<StandardTriangleKey, int>();
                foreach (CollisionTriangle triangle in cellTriangles)
                {
                    StandardTriangleKey key = StandardTriangleKey.Create(triangle);
                    cellMultiplicity.TryGetValue(key, out int count);
                    cellMultiplicity[key] = count + 1;
                    representativeTriangles.TryAdd(key, triangle);
                }

                foreach (var entry in cellMultiplicity)
                {
                    maximumMultiplicity.TryGetValue(entry.Key, out int previousMaximum);
                    if (entry.Value <= previousMaximum)
                        continue;

                    for (int copy = previousMaximum; copy < entry.Value; copy++)
                        triangles.Add(representativeTriangles[entry.Key]);
                    maximumMultiplicity[entry.Key] = entry.Value;
                }
            }

            return triangles;
        }

        internal List<(CollisionTriangle triangle, CollisionGeometryCategory category)> GetTrianglesForExport()
        {
            var triangles = GetStandardTriangles()
                .Select(triangle => (triangle, CollisionGeometryCategory.Standard))
                .ToList();

            foreach (HeroCollisionCell cell in heroCells)
                cell.AddTriangles(triangles, CollisionGeometryCategory.Hero);
            foreach (HeroCollisionCell cell in unkCells)
                cell.AddTriangles(triangles, CollisionGeometryCategory.Unknown);

            return triangles;
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

        private void ParseHeroCollision(FileStream fs, int baseOffset)
        {
            byte[] headBlock = ReadBlock(fs, baseOffset, 0x10);

            int heroCellCount = ReadInt(headBlock, 0x00);
            Utilities.DebugAssert(ReadInt(headBlock, 0x04) == 0, "Header[0x04] is not 0!");
            Utilities.DebugAssert(ReadInt(headBlock, 0x08) == 0, "Header[0x08] is not 0!");
            Utilities.DebugAssert(ReadInt(headBlock, 0x0C) == 0, "Header[0x0C] is not 0!");

            heroCells = new List<HeroCollisionCell>(heroCellCount);

            byte[] cellHeaderBlock = ReadBlock(fs, baseOffset + 0x10, heroCellCount * 0x10);

            for (int i = 0; i < heroCellCount; i++)
                heroCells.Add(new HeroCollisionCell(fs, baseOffset, i, cellHeaderBlock));
        }

        private void ParseUnkCollision(FileStream fs, int baseOffset)
        {
            byte[] headBlock = ReadBlock(fs, baseOffset, 0x10);

            int unkCount = ReadInt(headBlock, 0x00);
            Utilities.DebugAssert(ReadInt(headBlock, 0x04) == 0, "Header[0x04] is not 0!");
            Utilities.DebugAssert(ReadInt(headBlock, 0x08) == 0, "Header[0x08] is not 0!");
            Utilities.DebugAssert(ReadInt(headBlock, 0x0C) == 0, "Header[0x0C] is not 0!");

            unkCells = new List<HeroCollisionCell>(unkCount);

            byte[] cellHeaderBlock = ReadBlock(fs, baseOffset + 0x10, unkCount * 0x10);

            for (int i = 0; i < unkCount; i++)
            {
                unkCells.Add(new HeroCollisionCell(fs, baseOffset, i, cellHeaderBlock));
            }
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

            byte[] standardCollisionBlock = SerializeStandardCollision();
            byte[] heroCollisionBlock = SerializeHeroCollision();
            byte[] unkCollisionBlock = SerializeUnkCollision();

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

        private byte[] SerializeHeroCollision()
        {
            byte[] headerBytes = new byte[heroCells.Count * 0x10];

            List<byte[]> cellBytes = new List<byte[]>(heroCells.Count);

            int dataOffset = 0x10 + headerBytes.Length;

            for (int i = 0; i < heroCells.Count; i++)
            {
                dataOffset = AlignAddressUp(dataOffset);
                cellBytes.Add(heroCells[i].Serialize(i, headerBytes, ref dataOffset));
            }

            byte[] bytes = new byte[dataOffset];

            WriteInt(bytes, 0x00, heroCells.Count);

            int offset = 0x10;
            headerBytes.CopyTo(bytes, offset);

            offset += headerBytes.Length;

            for (int i = 0; i < heroCells.Count; i++)
            {
                offset = AlignAddressUp(offset);
                cellBytes[i].CopyTo(bytes, offset);
                offset += cellBytes[i].Length;
            }

            return bytes;
        }

        private byte[] SerializeUnkCollision()
        {
            byte[] headerBytes = new byte[unkCells.Count * 0x10];

            List<byte[]> cellBytes = new List<byte[]>(unkCells.Count);

            int dataOffset = 0x10 + headerBytes.Length;

            for (int i = 0; i < unkCells.Count; i++)
            {
                dataOffset = AlignAddressUp(dataOffset);
                cellBytes.Add(unkCells[i].Serialize(i, headerBytes, ref dataOffset));
            }

            byte[] bytes = new byte[dataOffset];

            WriteInt(bytes, 0x00, unkCells.Count);

            int offset = 0x10;
            headerBytes.CopyTo(bytes, offset);

            offset += headerBytes.Length;

            for (int i = 0; i < unkCells.Count; i++)
            {
                offset = AlignAddressUp(offset);
                cellBytes[i].CopyTo(bytes, offset);
                offset += cellBytes[i].Length;
            }

            return bytes;
        }
    }
}
