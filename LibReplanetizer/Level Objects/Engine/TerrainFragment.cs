using LibReplanetizer.Headers;
using LibReplanetizer.Models;
using OpenTK;
using System;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{


    public class TerrainFragment : ModelObject
    {
        public float off_00;
        public float off_04;
        public float off_08;
        public float off_0C;

        // 0x10 = pointer to TextureConfig
        // 0x14 = TextureConfig count
        // 0x18 = vertex offset, vertex count
        public ushort off_1C;   // Always 0xffff
        public ushort off_1E;   // 0 in rac1, index in rac2/3

        public ushort off_20;   // Always 0xff00
        // 0x22 = which rgba, uv and index pointer to use (0 for the first, 1 for the second)
        public uint off_24;     // Always 0
        public uint off_28;     // Always 0
        public uint off_2C;     // Always 0


        public TerrainFragment(FileStream fs, TerrainHead head, byte[] tfragBlock, int num)
        {
            int offset = num * 0x30;

            off_00 = ReadFloat(tfragBlock, offset + 0x00);
            off_04 = ReadFloat(tfragBlock, offset + 0x04);
            off_08 = ReadFloat(tfragBlock, offset + 0x08);
            off_0C = ReadFloat(tfragBlock, offset + 0x0C);


            off_1C = ReadUshort(tfragBlock, offset + 0x1C);
            off_1E = ReadUshort(tfragBlock, offset + 0x1E);
            off_20 = ReadUshort(tfragBlock, offset + 0x20);

            off_24 = ReadUint(tfragBlock, offset + 0x24);
            off_28 = ReadUint(tfragBlock, offset + 0x28);
            off_2C = ReadUint(tfragBlock, offset + 0x2C);

            model = new TerrainModel(fs, head, tfragBlock, num);
            modelMatrix = Matrix4.Identity;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public override byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

    }

}
