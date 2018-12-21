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

        public static byte[] ReadBlock(FileStream fs, uint offset, int length)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            byte[] returnBytes = new byte[length];
            fs.Read(returnBytes, 0, length);
            return returnBytes;
        }
    }
}
