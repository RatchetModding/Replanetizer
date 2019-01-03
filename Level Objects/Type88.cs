using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Type88
    {
        public const int TYPE88ELEMSIZE = 0x30;

        public float off_00;
        public float off_04;
        public float off_08;
        public float off_0C;

        public int off_10;
        public int off_14;
        public int off_18;
        public int off_1C;

        public int off_20;
        public int off_24;
        public int off_28;
        public int off_2C;

        public Type88(byte[] type88Block, int num)
        {
            off_00 = ReadFloat(type88Block, (TYPE88ELEMSIZE * num) + 0x00);
            off_04 = ReadFloat(type88Block, (TYPE88ELEMSIZE * num) + 0x04);
            off_08 = ReadFloat(type88Block, (TYPE88ELEMSIZE * num) + 0x08);
            off_0C = ReadFloat(type88Block, (TYPE88ELEMSIZE * num) + 0x0C);

            off_10 = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x10);
            off_14 = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x14);
            off_18 = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x18);
            off_1C = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x1C);

            off_20 = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x20);
            off_24 = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x24);
            off_28 = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x28);
            off_2C = ReadInt(type88Block, (TYPE88ELEMSIZE * num) + 0x2C);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[TYPE88ELEMSIZE];

            WriteFloat(ref bytes, 0x00, off_00);
            WriteFloat(ref bytes, 0x04, off_04);
            WriteFloat(ref bytes, 0x08, off_08);
            WriteFloat(ref bytes, 0x0C, off_0C);

            WriteInt(ref bytes, 0x10, off_10);
            WriteInt(ref bytes, 0x14, off_14);
            WriteInt(ref bytes, 0x18, off_18);
            WriteInt(ref bytes, 0x1C, off_1C);

            WriteInt(ref bytes, 0x20, off_20);
            WriteInt(ref bytes, 0x24, off_24);
            WriteInt(ref bytes, 0x28, off_28);
            WriteInt(ref bytes, 0x2C, off_2C);

            return bytes;
        }
    }
}
