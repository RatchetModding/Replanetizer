using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Type04 : LevelObject
    {
        public const int ELEMENTSIZE = 0x40;

        public float off_00;
        public float off_04;
        public float off_08;
        public float off_0C;

        public float off_10;
        public float off_14;
        public float off_18;
        public float off_1C;

        public float off_20;
        public float off_24;
        public float off_28;
        public float off_2C;

        public float off_30;
        public float off_34;
        public float off_38;
        public float off_3C;



        public Type04(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;

            off_00 = ReadFloat(block, offset + 0x00);
            off_04 = ReadFloat(block, offset + 0x04);
            off_08 = ReadFloat(block, offset + 0x08);
            off_0C = ReadFloat(block, offset + 0x0C);

            off_10 = ReadFloat(block, offset + 0x10);
            off_14 = ReadFloat(block, offset + 0x14);
            off_18 = ReadFloat(block, offset + 0x18);
            off_1C = ReadFloat(block, offset + 0x1C);

            off_20 = ReadFloat(block, offset + 0x20);
            off_24 = ReadFloat(block, offset + 0x24);
            off_28 = ReadFloat(block, offset + 0x28);
            off_2C = ReadFloat(block, offset + 0x2C);

            off_30 = ReadFloat(block, offset + 0x30);
            off_34 = ReadFloat(block, offset + 0x34);
            off_38 = ReadFloat(block, offset + 0x38);
            off_3C = ReadFloat(block, offset + 0x3C);
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteFloat(bytes, 0x00, off_00);
            WriteFloat(bytes, 0x04, off_04);
            WriteFloat(bytes, 0x08, off_08);
            WriteFloat(bytes, 0x0C, off_0C);

            WriteFloat(bytes, 0x10, off_10);
            WriteFloat(bytes, 0x14, off_14);
            WriteFloat(bytes, 0x18, off_18);
            WriteFloat(bytes, 0x1C, off_1C);

            WriteFloat(bytes, 0x20, off_20);
            WriteFloat(bytes, 0x24, off_24);
            WriteFloat(bytes, 0x28, off_28);
            WriteFloat(bytes, 0x2C, off_2C);

            WriteFloat(bytes, 0x30, off_30);
            WriteFloat(bytes, 0x34, off_34);
            WriteFloat(bytes, 0x38, off_38);
            WriteFloat(bytes, 0x3C, off_3C);

            return bytes;
        }


        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

    }
}
