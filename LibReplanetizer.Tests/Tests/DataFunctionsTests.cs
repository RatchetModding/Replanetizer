// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using Xunit;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Tests
{
    public class DataFunctionsTests
    {
        // Big-endian encoding: MSB first.

        #region ReadInt / WriteInt

        [Theory]
        [InlineData(0x00000000, new byte[] { 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(0x01020304, new byte[] { 0x01, 0x02, 0x03, 0x04 })]
        [InlineData(-1,        new byte[] { 0xFF, 0xFF, 0xFF, 0xFF })]
        [InlineData(int.MaxValue, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF })]
        [InlineData(int.MinValue, new byte[] { 0x80, 0x00, 0x00, 0x00 })]
        public void ReadInt_KnownBytes_ReturnsExpected(int expected, byte[] buf)
        {
            Assert.Equal(expected, ReadInt(buf, 0));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x01020304)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void WriteReadInt_RoundTrip(int value)
        {
            byte[] buf = new byte[4];
            WriteInt(buf, 0, value);
            Assert.Equal(value, ReadInt(buf, 0));
        }

        [Fact]
        public void WriteReadInt_WithOffset_RoundTrip()
        {
            byte[] buf = new byte[8];
            WriteInt(buf, 4, unchecked((int) 0xDEADBEEF));
            Assert.Equal(unchecked((int) 0xDEADBEEF), ReadInt(buf, 4));
        }

        #endregion

        #region ReadUint / WriteUint

        [Theory]
        [InlineData(0u)]
        [InlineData(0xDEADBEEFu)]
        [InlineData(uint.MaxValue)]
        public void WriteReadUint_RoundTrip(uint value)
        {
            byte[] buf = new byte[4];
            WriteUint(buf, 0, value);
            Assert.Equal(value, ReadUint(buf, 0));
        }

        #endregion

        #region ReadShort / WriteShort

        [Theory]
        [InlineData((short) 0)]
        [InlineData((short) 0x0102)]
        [InlineData(short.MinValue)]
        [InlineData(short.MaxValue)]
        [InlineData((short) -1)]
        public void WriteReadShort_RoundTrip(short value)
        {
            byte[] buf = new byte[2];
            WriteShort(buf, 0, value);
            Assert.Equal(value, ReadShort(buf, 0));
        }

        #endregion

        #region ReadUshort / WriteUshort

        [Theory]
        [InlineData((ushort) 0)]
        [InlineData((ushort) 0xABCD)]
        [InlineData(ushort.MaxValue)]
        public void WriteReadUshort_RoundTrip(ushort value)
        {
            byte[] buf = new byte[2];
            WriteUshort(buf, 0, value);
            Assert.Equal(value, ReadUshort(buf, 0));
        }

        #endregion

        #region ReadFloat / WriteFloat

        // 1.0f = 0x3F800000 big-endian → [0x3F, 0x80, 0x00, 0x00]
        [Fact]
        public void ReadFloat_OnePointZero_ReturnsOne()
        {
            byte[] buf = { 0x3F, 0x80, 0x00, 0x00 };
            Assert.Equal(1.0f, ReadFloat(buf, 0));
        }

        // 0.0f = 0x00000000
        [Fact]
        public void ReadFloat_Zero_ReturnsZero()
        {
            byte[] buf = { 0x00, 0x00, 0x00, 0x00 };
            Assert.Equal(0.0f, ReadFloat(buf, 0));
        }

        [Theory]
        [InlineData(0.0f)]
        [InlineData(1.0f)]
        [InlineData(-1.5f)]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        [InlineData(float.Epsilon)]
        public void WriteReadFloat_RoundTrip(float value)
        {
            byte[] buf = new byte[4];
            WriteFloat(buf, 0, value);
            Assert.Equal(value, ReadFloat(buf, 0));
        }

        #endregion

        #region ReadMatrix4 / WriteMatrix4

        [Fact]
        public void WriteReadMatrix4_IdentityRoundTrip()
        {
            byte[] buf = new byte[0x40];
            WriteMatrix4(buf, 0, Matrix4.Identity);
            Matrix4 result = ReadMatrix4(buf, 0);
            Assert.Equal(Matrix4.Identity, result);
        }

        [Fact]
        public void WriteReadMatrix4_ArbitraryValues_RoundTrip()
        {
            var m = new Matrix4(
                1f, 2f, 3f, 4f,
                5f, 6f, 7f, 8f,
                9f, 10f, 11f, 12f,
                13f, 14f, 15f, 16f);

            byte[] buf = new byte[0x40];
            WriteMatrix4(buf, 0, m);
            Matrix4 result = ReadMatrix4(buf, 0);

            Assert.Equal(m, result);
        }

        #endregion
    }

    /// <summary>Additional DataFunctions tests: Matrix3x4, byte-order, boundary values.</summary>
    public class DataFunctionsExtendedTests
    {
        #region ReadMatrix3x4 / WriteMatrix3x4

        [Fact]
        public void WriteReadMatrix3x4_AllDistinctValues_RoundTrip()
        {
            var m = new Matrix3x4(
                1f, 2f, 3f, 4f,
                5f, 6f, 7f, 8f,
                9f, 10f, 11f, 12f);

            byte[] buf = new byte[0x30];
            WriteMatrix3x4(buf, 0, m);
            Matrix3x4 result = ReadMatrix3x4(buf, 0);

            Assert.Equal(m.M11, result.M11);
            Assert.Equal(m.M12, result.M12);
            Assert.Equal(m.M13, result.M13);
            Assert.Equal(m.M14, result.M14);
            Assert.Equal(m.M21, result.M21);
            Assert.Equal(m.M24, result.M24);
            Assert.Equal(m.M31, result.M31);
            Assert.Equal(m.M34, result.M34);
        }

        [Fact]
        public void WriteMatrix3x4_WithOffset_RoundTrip()
        {
            var m = new Matrix3x4(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f);
            byte[] buf = new byte[0x30 + 8];
            WriteMatrix3x4(buf, 8, m);
            Matrix3x4 result = ReadMatrix3x4(buf, 8);
            Assert.Equal(m.M11, result.M11);
            Assert.Equal(m.M22, result.M22);
            Assert.Equal(m.M33, result.M33);
        }

        #endregion

        #region Big-endian byte-order verification

        [Fact]
        public void WriteInt_ByteOrderIsBigEndian()
        {
            byte[] buf = new byte[4];
            WriteInt(buf, 0, 0x01020304);
            Assert.Equal(0x01, buf[0]);
            Assert.Equal(0x02, buf[1]);
            Assert.Equal(0x03, buf[2]);
            Assert.Equal(0x04, buf[3]);
        }

        [Fact]
        public void WriteUint_ByteOrderIsBigEndian()
        {
            byte[] buf = new byte[4];
            WriteUint(buf, 0, 0xAABBCCDDu);
            Assert.Equal(0xAA, buf[0]);
            Assert.Equal(0xBB, buf[1]);
            Assert.Equal(0xCC, buf[2]);
            Assert.Equal(0xDD, buf[3]);
        }

        [Fact]
        public void WriteShort_ByteOrderIsBigEndian()
        {
            byte[] buf = new byte[2];
            WriteShort(buf, 0, 0x0102);
            Assert.Equal(0x01, buf[0]);
            Assert.Equal(0x02, buf[1]);
        }

        [Fact]
        public void WriteUshort_ByteOrderIsBigEndian()
        {
            byte[] buf = new byte[2];
            WriteUshort(buf, 0, 0xABCD);
            Assert.Equal(0xAB, buf[0]);
            Assert.Equal(0xCD, buf[1]);
        }

        [Fact]
        public void WriteFloat_OnePointZero_ByteOrderIsBigEndian()
        {
            // IEEE 754 1.0f = 0x3F800000 → big-endian [3F, 80, 00, 00]
            byte[] buf = new byte[4];
            WriteFloat(buf, 0, 1.0f);
            Assert.Equal(0x3F, buf[0]);
            Assert.Equal(0x80, buf[1]);
            Assert.Equal(0x00, buf[2]);
            Assert.Equal(0x00, buf[3]);
        }

        #endregion

        #region Boundary / special float values

        [Fact]
        public void WriteReadFloat_PositiveInfinity_RoundTrip()
        {
            byte[] buf = new byte[4];
            WriteFloat(buf, 0, float.PositiveInfinity);
            Assert.Equal(float.PositiveInfinity, ReadFloat(buf, 0));
        }

        [Fact]
        public void WriteReadFloat_NegativeInfinity_RoundTrip()
        {
            byte[] buf = new byte[4];
            WriteFloat(buf, 0, float.NegativeInfinity);
            Assert.Equal(float.NegativeInfinity, ReadFloat(buf, 0));
        }

        [Fact]
        public void WriteReadFloat_NaN_RoundTrip()
        {
            byte[] buf = new byte[4];
            WriteFloat(buf, 0, float.NaN);
            Assert.True(float.IsNaN(ReadFloat(buf, 0)));
        }

        #endregion

        #region Multi-value at various offsets

        [Fact]
        public void WriteReadInt_MultipleValues_IndependentSlots()
        {
            byte[] buf = new byte[12];
            WriteInt(buf, 0, 0x11111111);
            WriteInt(buf, 4, 0x22222222);
            WriteInt(buf, 8, 0x33333333);

            Assert.Equal(0x11111111, ReadInt(buf, 0));
            Assert.Equal(0x22222222, ReadInt(buf, 4));
            Assert.Equal(0x33333333, ReadInt(buf, 8));
        }

        [Fact]
        public void WriteReadFloat_MultipleValues_IndependentSlots()
        {
            byte[] buf = new byte[12];
            WriteFloat(buf, 0, 1.1f);
            WriteFloat(buf, 4, 2.2f);
            WriteFloat(buf, 8, 3.3f);

            Assert.Equal(1.1f, ReadFloat(buf, 0));
            Assert.Equal(2.2f, ReadFloat(buf, 4));
            Assert.Equal(3.3f, ReadFloat(buf, 8));
        }

        #endregion
    }
}
