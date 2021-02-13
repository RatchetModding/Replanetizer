using System.ComponentModel;
using System.Drawing;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class LevelVariables
    {
        [Category("Unknown")]
        public uint off_00 { get; set; }

        [Category("Unknown")]
        public uint off_04 { get; set; }

        [Category("Unknown")]
        public uint off_08 { get; set; }

        [Category("Attributes"), DisplayName("Fog Color")]
        public Color fogColor { get; set; }

        [Category("Unknown")]
        public float off_18 { get; set; }

        [Category("Unknown")]
        public float off_1c { get; set; }

        [Category("Attributes"), DisplayName("Fog Distance")]
        public float fogDistance { get; set; }

        [Category("Unknown")]
        public float off_24 { get; set; }

        [Category("Attributes"), DisplayName("Deathplane Z")]
        public float deathPlaneZ { get; set; }

        [Category("Attributes"), DisplayName("Ship X")]
        public float shipX { get; set; }

        [Category("Attributes"), DisplayName("Ship Y")]
        public float shipY { get; set; }

        [Category("Attributes"), DisplayName("Ship Z")]
        public float shipZ { get; set; }

        [Category("Attributes"), DisplayName("Ship Rotation")]
        public float shipRotation { get; set; }

        [Category("Unknown")]
        public int off_3c { get; set; }

        [Category("Unknown")]
        public uint off_40 { get; set; }

        [Category("Unknown")]
        public uint off_44 { get; set; }

        [Category("Unknown")]
        public uint off_48 { get; set; }

        [Category("Unknown")]
        public uint off_4c { get; set; }

        public LevelVariables(byte[] levelVarBlock)
        {
            off_00 = ReadUint(levelVarBlock, 0x00);
            off_04 = ReadUint(levelVarBlock, 0x04);
            off_08 = ReadUint(levelVarBlock, 0x08);
            int r = ReadInt(levelVarBlock, 0x0c);

            int g = ReadInt(levelVarBlock, 0x10);
            int b = ReadInt(levelVarBlock, 0x14);
            off_18 = ReadFloat(levelVarBlock, 0x18);
            off_1c = ReadFloat(levelVarBlock, 0x1C);

            fogDistance = ReadFloat(levelVarBlock, 0x20);
            off_24 = ReadFloat(levelVarBlock, 0x24);
            deathPlaneZ = ReadFloat(levelVarBlock, 0x28);
            shipX = ReadFloat(levelVarBlock, 0x2C);

            shipY = ReadFloat(levelVarBlock, 0x30);
            shipZ = ReadFloat(levelVarBlock, 0x34);
            shipRotation = ReadFloat(levelVarBlock, 0x38);
            off_3c = ReadInt(levelVarBlock, 0x3C);

            off_40 = ReadUint(levelVarBlock, 0x40);
            off_44 = ReadUint(levelVarBlock, 0x44);
            off_48 = ReadUint(levelVarBlock, 0x48);
            off_4c = ReadUint(levelVarBlock, 0x4C);

            fogColor = Color.FromArgb(r, g, b);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[0x50];

            WriteUint(bytes, 0x00, off_00);
            WriteUint(bytes, 0x04, off_04);
            WriteUint(bytes, 0x08, off_08);
            WriteUint(bytes, 0x0C, fogColor.R);

            WriteUint(bytes, 0x10, fogColor.G);
            WriteUint(bytes, 0x14, fogColor.B);
            WriteFloat(bytes, 0x18, off_18);
            WriteFloat(bytes, 0x1C, off_1c);

            WriteFloat(bytes, 0x20, fogDistance);
            WriteFloat(bytes, 0x24, off_24);
            WriteFloat(bytes, 0x28, deathPlaneZ);
            WriteFloat(bytes, 0x2C, shipX);

            WriteFloat(bytes, 0x30, shipY);
            WriteFloat(bytes, 0x34, shipZ);
            WriteFloat(bytes, 0x38, shipRotation);
            WriteUint(bytes, 0x3C, (uint)off_3c);

            WriteUint(bytes, 0x40, off_40);
            WriteUint(bytes, 0x44, off_44);
            WriteUint(bytes, 0x48, off_48);
            WriteUint(bytes, 0x4C, off_4c);

            return bytes;
        }

    }
}
