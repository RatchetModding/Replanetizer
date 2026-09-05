// Copyright (C) 2018-2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.ComponentModel;
using LibReplanetizer.Models.Animations;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Serializers.SerializerFunctions;

namespace LibReplanetizer.Models
{
    public class MobyModelCollisionPrimitive
    {
        public const int SIZE = 0x20;
        public byte shape { get; set; }
        public byte unk1 { get; set; }
        public ushort collisionMask { get; set; }
        public int value0 { get; set; }
        public float value1 { get; set; }
        public float value2 { get; set; }
        public float value3 { get; set; }
        public float value4 { get; set; }
        public float value5 { get; set; }
        public float value6 { get; set; }

        public MobyModelCollisionPrimitive() { }

        public MobyModelCollisionPrimitive(byte[] data, int offset)
        {
            shape = data[offset + 0x00];
            unk1 = data[offset + 0x01];
            collisionMask = ReadUshort(data, offset + 0x02);
            value0 = ReadInt(data, offset + 0x04);
            value1 = ReadFloat(data, offset + 0x08);
            value2 = ReadFloat(data, offset + 0x0C);
            value3 = ReadFloat(data, offset + 0x10);
            value4 = ReadFloat(data, offset + 0x14);
            value5 = ReadFloat(data, offset + 0x18);
            value6 = ReadFloat(data, offset + 0x1C);
        }

        public byte[] Serialize()
        {
            byte[] outbytes = new byte[32];
            outbytes[0x00] = shape;
            outbytes[0x01] = unk1;
            WriteUshort(outbytes, 0x02, collisionMask);
            WriteInt(outbytes, 0x04, value0);
            WriteFloat(outbytes, 0x08, value1);
            WriteFloat(outbytes, 0x0C, value2);
            WriteFloat(outbytes, 0x10, value3);
            WriteFloat(outbytes, 0x14, value4);
            WriteFloat(outbytes, 0x18, value5);
            WriteFloat(outbytes, 0x1C, value6);
            return outbytes;
        }
    }

    public class MobyModelCollisionTriangle
    {
        public const int SIZE = 0x04;
        public byte vertex0 { get; set; }
        public byte vertex1 { get; set; }
        public byte vertex2 { get; set; }
        public byte flags { get; set; }

        public MobyModelCollisionTriangle() { }

        public MobyModelCollisionTriangle(byte[] data, int offset)
        {
            vertex0 = data[offset + 0x00];
            vertex1 = data[offset + 0x01];
            vertex2 = data[offset + 0x02];
            flags = data[offset + 0x03];
        }

        public byte[] Serialize()
        {
            byte[] outbytes = new byte[4];
            outbytes[0x00] = vertex0;
            outbytes[0x01] = vertex1;
            outbytes[0x02] = vertex2;
            outbytes[0x03] = flags;
            return outbytes;
        }
    }

    public class MobyModelCollisionVertex
    {
        public const int SIZE = 0x10;
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public MobyModelCollisionVertex() { }

        public MobyModelCollisionVertex(byte[] data, int offset)
        {
            x = ReadFloat(data, offset + 0x00);
            y = ReadFloat(data, offset + 0x04);
            z = ReadFloat(data, offset + 0x08);
            w = ReadFloat(data, offset + 0x0C);
        }

        public byte[] Serialize()
        {
            byte[] outbytes = new byte[16];
            WriteFloat(outbytes, 0x00, x);
            WriteFloat(outbytes, 0x04, y);
            WriteFloat(outbytes, 0x08, z);
            WriteFloat(outbytes, 0x0C, w);
            return outbytes;
        }
    }

    public class MobyModelCollision
    {
        const int HEADERSIZE = 0x10;

        public ushort meta0 { get; set; }
        public ushort meta2 { get; set; }

        public List<MobyModelCollisionPrimitive> primitives { get; set; }
        public List<MobyModelCollisionTriangle> triangles { get; set; }
        public List<MobyModelCollisionVertex> vertices { get; set; }

        public MobyModelCollision()
        {
            primitives = new List<MobyModelCollisionPrimitive>();
            triangles = new List<MobyModelCollisionTriangle>();
            vertices = new List<MobyModelCollisionVertex>();
        }

        public MobyModelCollision(FileStream fs, int offset)
        {
            byte[] headerBytes = ReadBlock(fs, offset, HEADERSIZE);

            meta0 = ReadUshort(headerBytes, 0x00);
            meta2 = ReadUshort(headerBytes, 0x02);
            int primitiveBytesLength = ReadInt(headerBytes, 0x04);
            int triangleBytesLength = ReadInt(headerBytes, 0x08);
            int vertexBytesLength = ReadInt(headerBytes, 0x0C);

            int numPrimitives = primitiveBytesLength / MobyModelCollisionPrimitive.SIZE;
            int numTriangles = triangleBytesLength / MobyModelCollisionTriangle.SIZE;
            int numVertices = vertexBytesLength / MobyModelCollisionVertex.SIZE;

            byte[] primitiveBytes = ReadBlock(fs, offset + HEADERSIZE, primitiveBytesLength);
            primitives = new List<MobyModelCollisionPrimitive>();
            for (int i = 0; i < numPrimitives; i++)
                primitives.Add(new MobyModelCollisionPrimitive(primitiveBytes, i * MobyModelCollisionPrimitive.SIZE));

            // RaC 1 Boot has some weird data here where vertexBytesLength is not a multiple of 16 but of 8.
            // We just ignore that, the boot level is full of patterns that deviate from everything else seen in the games.
            byte[] vertexBytes = ReadBlock(fs, offset + HEADERSIZE + primitiveBytesLength, vertexBytesLength);
            vertices = new List<MobyModelCollisionVertex>();
            for (int i = 0; i < numVertices; i++)
                vertices.Add(new MobyModelCollisionVertex(vertexBytes, i * MobyModelCollisionVertex.SIZE));

            byte[] triangleBytes = ReadBlock(fs, offset + HEADERSIZE + primitiveBytesLength + vertexBytesLength, triangleBytesLength);
            triangles = new List<MobyModelCollisionTriangle>();
            for (int i = 0; i < numTriangles; i++)
                triangles.Add(new MobyModelCollisionTriangle(triangleBytes, i * MobyModelCollisionTriangle.SIZE));
        }

        public byte[] Serialize()
        {
            int primitiveBytesLength = primitives.Count * MobyModelCollisionPrimitive.SIZE;
            int triangleBytesLength = triangles.Count * MobyModelCollisionTriangle.SIZE;
            int vertexBytesLength = vertices.Count * MobyModelCollisionVertex.SIZE;

            int totalSize = HEADERSIZE + primitiveBytesLength + triangleBytesLength + vertexBytesLength;

            byte[] outbytes = new byte[totalSize];
            WriteUshort(outbytes, 0x00, meta0);
            WriteUshort(outbytes, 0x02, meta2);
            WriteInt(outbytes, 0x04, primitiveBytesLength);
            WriteInt(outbytes, 0x08, triangleBytesLength);
            WriteInt(outbytes, 0x0C, vertexBytesLength);

            int offset = HEADERSIZE;
            foreach (MobyModelCollisionPrimitive primitive in primitives)
            {
                primitive.Serialize().CopyTo(outbytes, offset);
                offset += MobyModelCollisionPrimitive.SIZE;
            }

            foreach (MobyModelCollisionVertex vertex in vertices)
            {
                vertex.Serialize().CopyTo(outbytes, offset);
                offset += MobyModelCollisionVertex.SIZE;
            }

            foreach (MobyModelCollisionTriangle triangle in triangles)
            {
                triangle.Serialize().CopyTo(outbytes, offset);
                offset += MobyModelCollisionTriangle.SIZE;
            }

            return outbytes;
        }
    }
}
