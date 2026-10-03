// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace LibReplanetizer
{
    public static class DataFunctions
    {
        [StructLayout(LayoutKind.Explicit)]
        struct FloatUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;

            [FieldOffset(0)]
            public float value;
        }

        public static float ReadFloat(byte[] buf, int offset)
        {
            FloatUnion floatBytes = default;
            floatBytes.byte0 = buf[offset + 3];
            floatBytes.byte1 = buf[offset + 2];
            floatBytes.byte2 = buf[offset + 1];
            floatBytes.byte3 = buf[offset];
            return floatBytes.value;
        }

        public static int ReadInt(byte[] buf, int offset)
        {
            return buf[offset + 0] << 24 | buf[offset + 1] << 16 | buf[offset + 2] << 8 | buf[offset + 3];
        }

        public static short ReadShort(byte[] buf, int offset)
        {
            return (short) (buf[offset + 0] << 8 | buf[offset + 1]);
        }

        public static uint ReadUint(byte[] buf, int offset)
        {
            return (uint) (buf[offset + 0] << 24 | buf[offset + 1] << 16 | buf[offset + 2] << 8 | buf[offset + 3]);
        }

        public static ushort ReadUshort(byte[] buf, int offset)
        {
            return (ushort) (buf[offset + 0] << 8 | buf[offset + 1]);
        }

        public static Matrix4 ReadMatrix4(byte[] buf, int offset)
        {
            return new Matrix4(
                ReadFloat(buf, offset + 0x00),
                ReadFloat(buf, offset + 0x04),
                ReadFloat(buf, offset + 0x08),
                ReadFloat(buf, offset + 0x0C),

                ReadFloat(buf, offset + 0x10),
                ReadFloat(buf, offset + 0x14),
                ReadFloat(buf, offset + 0x18),
                ReadFloat(buf, offset + 0x1C),

                ReadFloat(buf, offset + 0x20),
                ReadFloat(buf, offset + 0x24),
                ReadFloat(buf, offset + 0x28),
                ReadFloat(buf, offset + 0x2C),

                ReadFloat(buf, offset + 0x30),
                ReadFloat(buf, offset + 0x34),
                ReadFloat(buf, offset + 0x38),
                ReadFloat(buf, offset + 0x3C)
                );
        }

        public static Matrix3x4 ReadMatrix3x4(byte[] buf, int offset)
        {
            return new Matrix3x4(
                ReadFloat(buf, offset + 0x00),
                ReadFloat(buf, offset + 0x04),
                ReadFloat(buf, offset + 0x08),
                ReadFloat(buf, offset + 0x0C),

                ReadFloat(buf, offset + 0x10),
                ReadFloat(buf, offset + 0x14),
                ReadFloat(buf, offset + 0x18),
                ReadFloat(buf, offset + 0x1C),

                ReadFloat(buf, offset + 0x20),
                ReadFloat(buf, offset + 0x24),
                ReadFloat(buf, offset + 0x28),
                ReadFloat(buf, offset + 0x2C)
                );
        }

        public static byte[] ReadBlock(FileStream fs, long offset, int length)
        {
            if (length == 0)
                return [];

            fs.Seek(offset, SeekOrigin.Begin);
            byte[] returnBytes = new byte[length];
            fs.Read(returnBytes, 0, length);
            return returnBytes;
        }

        public static byte[] ReadString(FileStream fs, int offset)
        {
            var output = new List<byte>();

            fs.Seek(offset, SeekOrigin.Begin);

            byte[] buffer = new byte[4];
            do
            {
                fs.Read(buffer, 0, 4);
                output.AddRange(buffer);
            }
            while (buffer[3] != '\0');

            output.RemoveAll(item => item == 0);

            return output.ToArray();
        }

        public static void WriteUint(byte[] byteArr, int offset, uint input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[3];
            byteArr[offset + 1] = byt[2];
            byteArr[offset + 2] = byt[1];
            byteArr[offset + 3] = byt[0];
        }

        public static void WriteInt(byte[] byteArr, int offset, int input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[3];
            byteArr[offset + 1] = byt[2];
            byteArr[offset + 2] = byt[1];
            byteArr[offset + 3] = byt[0];
        }

        public static void WriteFloat(byte[] byteArr, int offset, float input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[3];
            byteArr[offset + 1] = byt[2];
            byteArr[offset + 2] = byt[1];
            byteArr[offset + 3] = byt[0];
        }

        public static void WriteShort(byte[] byteArr, int offset, short input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[1];
            byteArr[offset + 1] = byt[0];
        }

        public static void WriteUshort(byte[] byteArr, int offset, ushort input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[1];
            byteArr[offset + 1] = byt[0];
        }

        public static void WriteMatrix4(byte[] byteArray, int offset, Matrix4 input)
        {
            WriteFloat(byteArray, offset + 0x00, input.M11);
            WriteFloat(byteArray, offset + 0x04, input.M12);
            WriteFloat(byteArray, offset + 0x08, input.M13);
            WriteFloat(byteArray, offset + 0x0C, input.M14);

            WriteFloat(byteArray, offset + 0x10, input.M21);
            WriteFloat(byteArray, offset + 0x14, input.M22);
            WriteFloat(byteArray, offset + 0x18, input.M23);
            WriteFloat(byteArray, offset + 0x1C, input.M24);

            WriteFloat(byteArray, offset + 0x20, input.M31);
            WriteFloat(byteArray, offset + 0x24, input.M32);
            WriteFloat(byteArray, offset + 0x28, input.M33);
            WriteFloat(byteArray, offset + 0x2C, input.M34);

            WriteFloat(byteArray, offset + 0x30, input.M41);
            WriteFloat(byteArray, offset + 0x34, input.M42);
            WriteFloat(byteArray, offset + 0x38, input.M43);
            WriteFloat(byteArray, offset + 0x3C, input.M44);
        }

        public static void WriteMatrix3x4(byte[] byteArray, int offset, Matrix3x4 input)
        {
            WriteFloat(byteArray, offset + 0x00, input.M11);
            WriteFloat(byteArray, offset + 0x04, input.M12);
            WriteFloat(byteArray, offset + 0x08, input.M13);
            WriteFloat(byteArray, offset + 0x0C, input.M14);

            WriteFloat(byteArray, offset + 0x10, input.M21);
            WriteFloat(byteArray, offset + 0x14, input.M22);
            WriteFloat(byteArray, offset + 0x18, input.M23);
            WriteFloat(byteArray, offset + 0x1C, input.M24);

            WriteFloat(byteArray, offset + 0x20, input.M31);
            WriteFloat(byteArray, offset + 0x24, input.M32);
            WriteFloat(byteArray, offset + 0x28, input.M33);
            WriteFloat(byteArray, offset + 0x2C, input.M34);
        }

        public static byte[] GetBytes(byte[] array, int ind, int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = array[ind + i];
            }
            return data;
        }

        [Obsolete]
        public static int GetLength(int length, int alignment = 0)
        {
            while (length % 0x10 != alignment)
            {
                length++;
            }
            return length;
        }

        [Obsolete]
        // vertexbuffers are often aligned to the nearest 0x80 in the file
        public static int DistToFile80(int length, int alignment = 0)
        {
            int added = 0;
            while (length % 0x80 != alignment)
            {
                length++;
                added++;
            }
            return added;
        }

        [Obsolete]
        public static int GetLength40(int length, int alignment = 0)
        {
            while (length % 0x40 != alignment)
            {
                length++;
            }
            return length;
        }

        [Obsolete]
        public static void Pad(List<byte> arr)
        {
            while (arr.Count % 0x10 != 0)
            {
                arr.Add(0);
            }
        }

        public static int AlignAddressUp(int offset, int alignment = 0x10)
        {
            return ((offset + alignment - 1) / alignment) * alignment;
        }

        public static int AlignAddressDown(int offset, int alignment = 0x10)
        {
            return offset - offset % alignment;
        }

        public static int GetRelativeOffset(int offset, int baseOffset)
        {
            return (offset > 0) ? offset - baseOffset : offset;
        }
    }
}
