using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class GameCamera
    {
        public const int ELEMSIZE = 0x20;

        public int id;
        public float x;
        public float y;
        public float z;
        public int unk1;
        public int unk2;
        public int unk3;
        public int id2;

        public GameCamera(byte[] cameraBlock, int num)
        {
            id = ReadInt(cameraBlock, (num * ELEMSIZE) + 0x00);
            x = ReadFloat(cameraBlock, (num * ELEMSIZE) + 0x04);
            y = ReadFloat(cameraBlock, (num * ELEMSIZE) + 0x08);
            z = ReadFloat(cameraBlock, (num * ELEMSIZE) + 0x0C);
            unk1 = ReadInt(cameraBlock, (num * ELEMSIZE) + 0x10);
            unk2 = ReadInt(cameraBlock, (num * ELEMSIZE) + 0x14);
            unk3 = ReadInt(cameraBlock, (num * ELEMSIZE) + 0x18);
            id2 = ReadInt(cameraBlock, (num * ELEMSIZE) + 0x1C);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[ELEMSIZE];

            WriteUint(ref bytes, 0x00, (uint)id);
            WriteFloat(ref bytes, 0x04, x);
            WriteFloat(ref bytes, 0x08, y);
            WriteFloat(ref bytes, 0x0C, z);
            WriteUint(ref bytes, 0x10, (uint)unk1);
            WriteUint(ref bytes, 0x14, (uint)unk2);
            WriteUint(ref bytes, 0x18, (uint)unk3);
            WriteUint(ref bytes, 0x1C, (uint)id2);

            return bytes;
        }
    }
}
