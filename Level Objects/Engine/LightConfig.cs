using static RatchetEdit.DataFunctions;


namespace RatchetEdit.LevelObjects
{
    public class LightConfig
    {
        public float off_00 { get; set; }
        public float off_04 { get; set; }
        public float off_08 { get; set; }
        public float off_0C { get; set; }

        public float off_10 { get; set; }
        public float off_14 { get; set; }
        public float off_18 { get; set; }
        public float off_1C { get; set; }

        public float off_20 { get; set; }
        public float off_24 { get; set; }
        public float off_28 { get; set; }
        public float off_2C { get; set; }

        int length;

        public LightConfig(byte[] block, int len)
        {
            length = len;

            off_00 = ReadFloat(block, 0x00);
            off_04 = ReadFloat(block, 0x04);
            off_08 = ReadFloat(block, 0x08);
            off_0C = ReadFloat(block, 0x0C);

            off_10 = ReadFloat(block, 0x10);
            off_14 = ReadFloat(block, 0x14);
            off_18 = ReadFloat(block, 0x18);
            off_1C = ReadFloat(block, 0x1C);

            off_20 = ReadFloat(block, 0x20);
            off_24 = ReadFloat(block, 0x24);
            off_28 = ReadFloat(block, 0x28);
            off_2C = ReadFloat(block, 0x2C);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[length];

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

            return bytes;
        }
    }
}
