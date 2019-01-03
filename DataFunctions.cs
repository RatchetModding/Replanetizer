using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit
{
    public static class DataFunctions
    {
        public static int ReadInt(byte[] buf, int offset)
        {
            return (buf[offset + 0] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | (buf[offset + 3]);
        }

        public static short ReadShort(byte[] buf, int offset)
        {
            return (short)((buf[offset + 0] << 8) |(buf[offset + 1]));
        }

        public static uint ReadUint(byte[] buf, int offset)
        {
            return (uint)( (buf[offset + 0] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | (buf[offset + 3]) );
        }

        public static ushort ReadUshort(byte[] buf, int offset)
        {
            return (ushort)((buf[offset + 0] << 8) | (buf[offset + 1]));
        }


        public static float ReadFloat(byte[] buf, int offset)
        {
            byte[] temp = new byte[4];
            Array.Copy(buf, offset, temp, 0, 4);
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }

        public static byte[] ReadBlock(FileStream fs, int offset, int length)
        {
            if(length > 0)
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

        public static void WriteUint(ref byte[] byteArr, int offset, uint input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[3];
            byteArr[offset + 1] = byt[2];
            byteArr[offset + 2] = byt[1];
            byteArr[offset + 3] = byt[0];
        }

        public static void WriteInt(ref byte[] byteArr, int offset, int input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[3];
            byteArr[offset + 1] = byt[2];
            byteArr[offset + 2] = byt[1];
            byteArr[offset + 3] = byt[0];
        }

        public static void WriteFloat(ref byte[] byteArr, int offset, float input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[3];
            byteArr[offset + 1] = byt[2];
            byteArr[offset + 2] = byt[1];
            byteArr[offset + 3] = byt[0];
        }

        public static void WriteShort(ref byte[] byteArr, int offset, short input)
        {
            byte[] byt = BitConverter.GetBytes(input);
            byteArr[offset + 0] = byt[1];
            byteArr[offset + 1] = byt[0];
        }

        public static byte[] getBytes(byte[] array, int ind, int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = array[ind + i];
            }
            return data;
        }
    }
}
