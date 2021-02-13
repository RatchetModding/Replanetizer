using OpenTK;
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

        static FloatUnion flt;

        public static float ReadFloat(byte[] buf, int offset)
        {
            flt.byte0 = buf[offset + 3];
            flt.byte1 = buf[offset + 2];
            flt.byte2 = buf[offset + 1];
            flt.byte3 = buf[offset];
            return flt.value;
        }

        public static int ReadInt(byte[] buf, int offset)
        {
            return buf[offset + 0] << 24 | buf[offset + 1] << 16 | buf[offset + 2] << 8 | buf[offset + 3];
        }

        public static short ReadShort(byte[] buf, int offset)
        {
            return (short)(buf[offset + 0] << 8 | buf[offset + 1]);
        }

        public static uint ReadUint(byte[] buf, int offset)
        {
            return (uint)(buf[offset + 0] << 24 | buf[offset + 1] << 16 | buf[offset + 2] << 8 | buf[offset + 3]);
        }

        public static ushort ReadUshort(byte[] buf, int offset)
        {
            return (ushort)(buf[offset + 0] << 8 | buf[offset + 1]);
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

        public static byte[] ReadBlock(FileStream fs, long offset, int length)
        {
            if (length > 0)
            {
                fs.Seek(offset, SeekOrigin.Begin);
                byte[] returnBytes = new byte[length];
                fs.Read(returnBytes, 0, length);
                return returnBytes;
            }
            else
            {
                byte[] returnBytes = new byte[0x10];
                return returnBytes;
            }
        }

        public static byte[] ReadBlockNopad(FileStream fs, long offset, int length)
        {
            if (length > 0)
            {
                fs.Seek(offset, SeekOrigin.Begin);
                byte[] returnBytes = new byte[length];
                fs.Read(returnBytes, 0, length);
                return returnBytes;
            }
            return new byte[0];
        }

        public static String ReadString(FileStream fs, int offset)
        {
            String output = "";
            fs.Seek(offset, SeekOrigin.Begin);
            int pos = offset;

            byte[] buffer = new byte[4];
            do
            {
                fs.Read(buffer, 0, 4);
                output += System.Text.Encoding.ASCII.GetString(buffer);
            }
            while (buffer[3] != '\0');

            return output.Substring(0, output.IndexOf('\0'));
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

        public static byte[] GetBytes(byte[] array, int ind, int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = array[ind + i];
            }
            return data;
        }

        public static int GetLength(int length, int alignment = 0)
        {
            while (length % 0x10 != alignment)
            {
                length++;
            }
            return length;
        }

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

        public static int GetLength20(int length, int alignment = 0)
        {
            while (length % 0x20 != alignment)
            {
                length++;
            }
            return length;
        }

        public static int GetLength100(int length)
        {
            while (length % 0x100 != 0)
            {
                length++;
            }
            return length;
        }

        public static void Pad(List<byte> arr)
        {
            while (arr.Count % 0x10 != 0)
            {
                arr.Add(0);
            }
        }

        // "Polyfill" for function in upcoming OpenTK version
        public static Vector3 ToEulerAngles(Quaternion q)
        {
            /*
            reference
            http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
            */

            Vector3 eulerAngles;

            // Threshold for the singularities found at the north/south poles.
            const float SINGULARITY_THRESHOLD = 0.4999995f;

            var sqw = q.W * q.W;
            var sqx = q.X * q.X;
            var sqy = q.Y * q.Y;
            var sqz = q.Z * q.Z;
            var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            var singularityTest = (q.X * q.Z) + (q.W * q.Y);

            if (singularityTest > SINGULARITY_THRESHOLD * unit)
            {
                eulerAngles.Z = (float)(2 * Math.Atan2(q.X, q.W));
                eulerAngles.Y = MathHelper.PiOver2;
                eulerAngles.X = 0;
            }
            else if (singularityTest < -SINGULARITY_THRESHOLD * unit)
            {
                eulerAngles.Z = (float)(-2 * Math.Atan2(q.X, q.W));
                eulerAngles.Y = -MathHelper.PiOver2;
                eulerAngles.X = 0;
            }
            else
            {
                eulerAngles.Z = (float)Math.Atan2(2 * ((q.W * q.Z) - (q.X * q.Y)), sqw + sqx - sqy - sqz);
                eulerAngles.Y = (float)Math.Asin(2 * singularityTest / unit);
                eulerAngles.X = (float)Math.Atan2(2 * ((q.W * q.X) - (q.Y * q.Z)), sqw - sqx - sqy + sqz);
            }

            return eulerAngles;
        }
    }
}
