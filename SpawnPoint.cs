using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class SpawnPoint
    {
        public int ID;
        public float x;
        public float y;
        public float z;
        public float LR;
        public byte[] hitBox;
        public byte[] shape;

        public SpawnPoint(byte[] spawnPointBlock, int index)
        {
            ID = index;
            x = ReadFloat(spawnPointBlock, (index * 0x80) + 0x30);
            y = ReadFloat(spawnPointBlock, (index * 0x80) + 0x34);
            z = ReadFloat(spawnPointBlock, (index * 0x80) + 0x38);
            LR = ReadFloat(spawnPointBlock, (index * 0x80) + 0x78);
            hitBox = new byte[0x30];
            shape = new byte[0x30];
        }
    }
}
