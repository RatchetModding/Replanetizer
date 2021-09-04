using OpenTK.Mathematics;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    /*
     * Lights are used for everything but terrain
     * Lights are applied differently for different things
     * i.e. increasing the magnitude to the direction increases brightness of the light for shrubs a lot but not for mobies/ties etc.
     * 
     * Color channels are in [0,1]
     */
    public class Light
    {
        public Vector4 color1;
        public Vector4 direction1;
        public Vector4 color2;
        public Vector4 direction2;

        public Light(byte[] block, int num)
        {
            int offset = num * 0x40;

            float c1R = ReadFloat(block, offset + 0x00);
            float c1G = ReadFloat(block, offset + 0x04);
            float c1B = ReadFloat(block, offset + 0x08);
            float c1A = ReadFloat(block, offset + 0x0C);

            color1 = new Vector4(c1R, c1G, c1B, c1A);

            float d1X = ReadFloat(block, offset + 0x10);
            float d1Y = ReadFloat(block, offset + 0x14);
            float d1Z = ReadFloat(block, offset + 0x18);
            float d1W = ReadFloat(block, offset + 0x1C);

            direction1 = new Vector4(d1X, d1Y, d1Z, d1W);

            float c2R = ReadFloat(block, offset + 0x20);
            float c2G = ReadFloat(block, offset + 0x24);
            float c2B = ReadFloat(block, offset + 0x28);
            float c2A = ReadFloat(block, offset + 0x2C);

            color2 = new Vector4(c2R, c2G, c2B, c2A);

            float d2X = ReadFloat(block, offset + 0x30);
            float d2Y = ReadFloat(block, offset + 0x34);
            float d2Z = ReadFloat(block, offset + 0x38);
            float d2W = ReadFloat(block, offset + 0x3C);

            direction2 = new Vector4(d2X, d2Y, d2Z, d2W);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[0x40];

            WriteFloat(bytes, 0x00, color1.X);
            WriteFloat(bytes, 0x04, color1.Y);
            WriteFloat(bytes, 0x08, color1.Z);
            WriteFloat(bytes, 0x0C, color1.W);

            WriteFloat(bytes, 0x10, direction1.X);
            WriteFloat(bytes, 0x14, direction1.Y);
            WriteFloat(bytes, 0x18, direction1.Z);
            WriteFloat(bytes, 0x1C, direction1.W);

            WriteFloat(bytes, 0x20, color2.X);
            WriteFloat(bytes, 0x24, color2.Y);
            WriteFloat(bytes, 0x28, color2.Z);
            WriteFloat(bytes, 0x2C, color2.W);

            WriteFloat(bytes, 0x30, direction2.X);
            WriteFloat(bytes, 0x34, direction2.Y);
            WriteFloat(bytes, 0x38, direction2.Z);
            WriteFloat(bytes, 0x3C, direction2.W);

            return bytes;
        }
    }
}
