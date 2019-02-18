using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class BoneData
    {
        public float unk1;
        public float unk2;
        public float unk3;
        public float unk4;

        public BoneData(byte[] boneDataBlock, int num)
        {
            int offset = num * 0x10;
            unk1 = ReadFloat(boneDataBlock, offset + 0x00);
            unk2 = ReadFloat(boneDataBlock, offset + 0x04);
            unk3 = ReadFloat(boneDataBlock, offset + 0x08);
            unk4 = ReadFloat(boneDataBlock, offset + 0x0C);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x10];

            WriteFloat(ref outBytes, 0x00, unk1);
            WriteFloat(ref outBytes, 0x04, unk2);
            WriteFloat(ref outBytes, 0x08, unk3);
            WriteFloat(ref outBytes, 0x0C, unk4);

            return outBytes;
        }
    }
}
