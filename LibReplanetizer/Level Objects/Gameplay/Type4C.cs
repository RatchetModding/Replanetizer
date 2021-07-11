using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type4C : LevelObject
    {
        public const int ELEMENTSIZE = 0x08;

        public ushort off_00;
        public ushort off_02;
        public ushort off_04;
        public ushort off_06;

        public Type4C(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            off_00 = ReadUshort(block, offset + 0x00);
            off_02 = ReadUshort(block, offset + 0x02);
            off_04 = ReadUshort(block, offset + 0x04);
            off_06 = ReadUshort(block, offset + 0x06);
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteUshort(bytes, 0x00, off_00);
            WriteUshort(bytes, 0x02, off_02);
            WriteUshort(bytes, 0x04, off_04);
            WriteUshort(bytes, 0x06, off_06);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

    }
}
