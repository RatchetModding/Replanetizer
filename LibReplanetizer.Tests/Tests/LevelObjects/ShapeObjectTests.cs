// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using Xunit;
using LibReplanetizer.LevelObjects;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests.LevelObjects
{
    public class GrindPathTests
    {
        private static byte[] BuildBlock(int index, float x, float y, float z, float radius, int unk10, int wrap, int inactive)
        {
            byte[] block = new byte[(index + 1) * GrindPath.ELEMENTSIZE];
            int offset = index * GrindPath.ELEMENTSIZE;
            WriteFloat(block, offset + 0x00, x);
            WriteFloat(block, offset + 0x04, y);
            WriteFloat(block, offset + 0x08, z);
            WriteFloat(block, offset + 0x0C, radius);
            WriteInt(block, offset + 0x10, unk10);
            WriteInt(block, offset + 0x14, wrap);
            WriteInt(block, offset + 0x18, inactive);
            return block;
        }

        private static Spline DummySpline()
        {
            // Single-vertex spline
            byte[] splineBlock = new byte[0x10 + 0x10];
            WriteUint(splineBlock, 0x00, 1);
            WriteFloat(splineBlock, 0x10, 0f);
            WriteFloat(splineBlock, 0x14, 0f);
            WriteFloat(splineBlock, 0x18, 0f);
            WriteFloat(splineBlock, 0x1C, 0f);
            return new Spline(splineBlock, 0);
        }

        [Fact]
        public void Constructor_ParsesPosition()
        {
            byte[] block = BuildBlock(0, 1f, 2f, 3f, 5f, 0, 0, 0);
            var gp = new GrindPath(block, 0, DummySpline());

            Assert.Equal(1f, gp.position.X);
            Assert.Equal(2f, gp.position.Y);
            Assert.Equal(3f, gp.position.Z);
        }

        [Fact]
        public void Constructor_ParsesRadius()
        {
            byte[] block = BuildBlock(0, 0f, 0f, 0f, 7.5f, 0, 0, 0);
            var gp = new GrindPath(block, 0, DummySpline());
            Assert.Equal(7.5f, gp.radius);
        }

        [Fact]
        public void Constructor_ParsesWrapAndInactive()
        {
            byte[] block = BuildBlock(0, 0f, 0f, 0f, 0f, 0xAA, 1, 0);
            var gp = new GrindPath(block, 0, DummySpline());
            Assert.Equal(1, gp.wrap);
            Assert.Equal(0, gp.inactive);
        }

        [Fact]
        public void ToByteArray_RoundTrip_MatchesOriginal()
        {
            byte[] original = BuildBlock(0, 10f, 20f, 30f, 4.0f, 0x12, 1, 0);
            var gp = new GrindPath(original, 0, DummySpline());
            byte[] serialized = gp.ToByteArray();

            Assert.Equal(GrindPath.ELEMENTSIZE, serialized.Length);
            Assert.Equal(ReadFloat(original, 0x00), ReadFloat(serialized, 0x00));
            Assert.Equal(ReadFloat(original, 0x04), ReadFloat(serialized, 0x04));
            Assert.Equal(ReadFloat(original, 0x08), ReadFloat(serialized, 0x08));
            Assert.Equal(ReadFloat(original, 0x0C), ReadFloat(serialized, 0x0C));
            Assert.Equal(ReadInt(original, 0x10), ReadInt(serialized, 0x10));
            Assert.Equal(ReadInt(original, 0x14), ReadInt(serialized, 0x14));
            Assert.Equal(ReadInt(original, 0x18), ReadInt(serialized, 0x18));
        }

        [Fact]
        public void Constructor_SetsId()
        {
            byte[] block = BuildBlock(2, 0f, 0f, 0f, 0f, 0, 0, 0);
            var gp = new GrindPath(block, 2, DummySpline());
            Assert.Equal(2, gp.id);
        }

        [Fact]
        public void Constructor_AssignsSpline()
        {
            var spline = DummySpline();
            byte[] block = BuildBlock(0, 0f, 0f, 0f, 0f, 0, 0, 0);
            var gp = new GrindPath(block, 0, spline);
            Assert.Same(spline, gp.spline);
        }
    }

    public class CylinderTests
    {
        private static byte[] BuildBlock(int index, Matrix4 transform, Matrix4 inverse)
        {
            byte[] block = new byte[(index + 1) * Cylinder.ELEMENTSIZE];
            int offset = index * Cylinder.ELEMENTSIZE;
            WriteMatrix4(block, offset + 0x00, transform);
            WriteMatrix4(block, offset + 0x40, inverse);
            return block;
        }

        [Fact]
        public void Constructor_SetsId()
        {
            byte[] block = BuildBlock(3, Matrix4.Identity, Matrix4.Identity);
            var cyl = new Cylinder(block, 3);
            Assert.Equal(3, cyl.id);
        }

        [Fact]
        public void Constructor_ExtractsTranslation()
        {
            var transform = Matrix4.CreateTranslation(5f, 6f, 7f);
            byte[] block = BuildBlock(0, transform, Matrix4.Identity);
            var cyl = new Cylinder(block, 0);

            Assert.Equal(5f, cyl.position.X, 4);
            Assert.Equal(6f, cyl.position.Y, 4);
            Assert.Equal(7f, cyl.position.Z, 4);
        }

        [Fact]
        public void ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var cyl = new Cylinder(block, 0);
            Assert.Equal(Cylinder.ELEMENTSIZE, cyl.ToByteArray().Length);
        }

        [Fact]
        public void ToByteArray_TranslationPreserved()
        {
            var transform = Matrix4.CreateTranslation(1f, 2f, 3f);
            byte[] block = BuildBlock(0, transform, Matrix4.Identity);
            var cyl = new Cylinder(block, 0);
            byte[] serialized = cyl.ToByteArray();

            Matrix4 recovered = ReadMatrix4(serialized, 0x00);
            Assert.Equal(transform.Row3.X, recovered.Row3.X, 4);
            Assert.Equal(transform.Row3.Y, recovered.Row3.Y, 4);
            Assert.Equal(transform.Row3.Z, recovered.Row3.Z, 4);
        }

        [Fact]
        public void GetVertices_ReturnsNonEmpty()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var cyl = new Cylinder(block, 0);
            Assert.NotEmpty(cyl.GetVertices());
        }

        [Fact]
        public void GetIndices_ReturnsNonEmpty()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var cyl = new Cylinder(block, 0);
            Assert.NotEmpty(cyl.GetIndices());
        }
    }

    public class PillTests
    {
        private static byte[] BuildBlock(int index, Matrix4 transform, Matrix4 inverse)
        {
            byte[] block = new byte[(index + 1) * Pill.ELEMENTSIZE];
            int offset = index * Pill.ELEMENTSIZE;
            WriteMatrix4(block, offset + 0x00, transform);
            WriteMatrix4(block, offset + 0x40, inverse);
            return block;
        }

        [Fact]
        public void Constructor_SetsId()
        {
            byte[] block = BuildBlock(1, Matrix4.Identity, Matrix4.Identity);
            var pill = new Pill(block, 1);
            Assert.Equal(1, pill.id);
        }

        [Fact]
        public void Constructor_ExtractsTranslation()
        {
            var transform = Matrix4.CreateTranslation(9f, 8f, 7f);
            byte[] block = BuildBlock(0, transform, Matrix4.Identity);
            var pill = new Pill(block, 0);

            Assert.Equal(9f, pill.position.X, 4);
            Assert.Equal(8f, pill.position.Y, 4);
            Assert.Equal(7f, pill.position.Z, 4);
        }

        [Fact]
        public void ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var pill = new Pill(block, 0);
            Assert.Equal(Pill.ELEMENTSIZE, pill.ToByteArray().Length);
        }

        [Fact]
        public void GetVertices_ReturnsNonEmpty()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var pill = new Pill(block, 0);
            Assert.NotEmpty(pill.GetVertices());
        }
    }

    public class SphereTests
    {
        private static byte[] BuildBlock(int index, Matrix4 transform, Matrix4 inverse)
        {
            byte[] block = new byte[(index + 1) * Sphere.ELEMENTSIZE];
            int offset = index * Sphere.ELEMENTSIZE;
            WriteMatrix4(block, offset + 0x00, transform);
            WriteMatrix4(block, offset + 0x40, inverse);
            return block;
        }

        [Fact]
        public void Constructor_SetsId()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var sphere = new Sphere(block, 0);
            Assert.Equal(0, sphere.id);
        }

        [Fact]
        public void Constructor_ExtractsTranslationFromMatrix()
        {
            var transform = Matrix4.CreateTranslation(3f, 4f, 5f);
            byte[] block = BuildBlock(0, transform, Matrix4.Identity);
            var sphere = new Sphere(block, 0);

            Assert.Equal(3f, sphere.position.X, 4);
            Assert.Equal(4f, sphere.position.Y, 4);
            Assert.Equal(5f, sphere.position.Z, 4);
        }

        [Fact]
        public void ToByteArray_HasCorrectLength()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var sphere = new Sphere(block, 0);
            Assert.Equal(Sphere.ELEMENTSIZE, sphere.ToByteArray().Length);
        }

        [Fact]
        public void GetVertices_ReturnsNonEmpty()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var sphere = new Sphere(block, 0);
            Assert.NotEmpty(sphere.GetVertices());
        }

        [Fact]
        public void GetIndices_ReturnsNonEmpty()
        {
            byte[] block = BuildBlock(0, Matrix4.Identity, Matrix4.Identity);
            var sphere = new Sphere(block, 0);
            Assert.NotEmpty(sphere.GetIndices());
        }
    }
}
