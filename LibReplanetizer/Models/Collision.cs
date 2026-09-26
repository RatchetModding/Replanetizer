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

    public class Collision : Model, IRenderable
    {
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
                vertices = new Vector3[faceCount * vertexCount];

                byte[] dataBlock = ReadBlock(fs, baseOffset + 0x04, faceCount * 0x04 + vertexCount * 0x0C + quadCount);
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

                    // Vertices
                    for (int v = 0; v < vertexCount; v++)
                    {
                        float xPos = ReadFloat(dataBlock, v * 0x0C + 0x00);
                        float yPos = ReadFloat(dataBlock, v * 0x0C + 0x04);
                        float zPos = ReadFloat(dataBlock, v * 0x0C + 0x08);

                        vertices[f * vertexCount + v] = new Vector3(xPos, yPos, zPos);
                    }

                    entries[f] = entry;
                }

                // The game sometimes stores just one vertex in a cell, no idea what that is useful for.
                if (faceCount == 0 && vertexCount > 0)
                {
                    vertices = new Vector3[vertexCount];

                    // Vertices
                    for (int v = 0; v < vertexCount; v++)
                    {
                        float xPos = ReadFloat(dataBlock, v * 0x0C + 0x00);
                        float yPos = ReadFloat(dataBlock, v * 0x0C + 0x04);
                        float zPos = ReadFloat(dataBlock, v * 0x0C + 0x08);

                        vertices[v] = new Vector3(xPos, yPos, zPos);
                    }
                }
            }

            public void GetModelData(ushort xShift, ushort yShift, ushort zShift, List<float> vertexList, List<uint> indexList, ref uint totalVertexCount)
            {
                CollisionType[] collisionType = new CollisionType[vertexCount];
                for (int f = 0; f < faceCount; f++)
                {
                    CollisionCellEntry entry = entries[f];

                    collisionType[entry.vertexIndex0] = entry.collisionType;
                    collisionType[entry.vertexIndex1] = entry.collisionType;
                    collisionType[entry.vertexIndex2] = entry.collisionType;

                    uint f1 = totalVertexCount + entry.vertexIndex0;
                    uint f2 = totalVertexCount + entry.vertexIndex1;
                    uint f3 = totalVertexCount + entry.vertexIndex2;
                    indexList.Add(f2);
                    indexList.Add(f1);
                    indexList.Add(f3);

                    if (f < quadCount)
                    {
                        uint f4 = totalVertexCount + entry.vertexIndex3;
                        indexList.Add(f3);
                        indexList.Add(f1);
                        indexList.Add(f4);
                        collisionType[entry.vertexIndex3] = entry.collisionType;
                    }

                    // Vertices
                    for (int v = 0; v < vertexCount; v++)
                    {
                        float xPos = vertices[f * vertexCount + v].X / 1024.0f;
                        float yPos = vertices[f * vertexCount + v].Y / 1024.0f;
                        float zPos = vertices[f * vertexCount + v].Z / 1024.0f;

                        xPos += 4 * (xShift + xCell + 0.5f);
                        yPos += 4 * (yShift + yCell + 0.5f);
                        zPos += 4 * (zShift + zCell + 0.5f);

                        vertexList.Add(xPos);
                        vertexList.Add(yPos);
                        vertexList.Add(zPos);

                        vertexList.Add(CollisionVertexMetadata.Pack(collisionType[v], CollisionGeometryCategory.Standard));
                        totalVertexCount++;
                    }
                }
            }

            public byte[] Serialize()
            {
                byte[] bytes = new byte[0x04 + faceCount * 0x04 + vertexCount * 0x0C + quadCount];

                WriteUshort(bytes, 0x00, faceCount);
                bytes[0x02] = vertexCount;
                bytes[0x03] = quadCount;

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

                    // Vertices
                    for (int v = 0; v < vertexCount; v++)
                    {
                        WriteFloat(bytes, 0x04 + v * 0x0C + 0x00, vertices[f * vertexCount + v].X);
                        WriteFloat(bytes, 0x04 + v * 0x0C + 0x04, vertices[f * vertexCount + v].Y);
                        WriteFloat(bytes, 0x04 + v * 0x0C + 0x08, vertices[f * vertexCount + v].Z);
                    }

                    entries[f] = entry;
                }

                if (faceCount == 0 && vertexCount > 0)
                {
                    // Vertices
                    for (int v = 0; v < vertexCount; v++)
                    {
                        WriteFloat(bytes, 0x04 + v * 0x0C + 0x00, vertices[v].X);
                        WriteFloat(bytes, 0x04 + v * 0x0C + 0x04, vertices[v].Y);
                        WriteFloat(bytes, 0x04 + v * 0x0C + 0x08, vertices[v].Z);
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

        public Collision(FileStream fs, int collisionPointer)
        {
            vertexStride = 4;

            // RaC 1 title screen has no collision
            if (collisionPointer == 0)
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
