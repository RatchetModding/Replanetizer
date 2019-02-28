using System.ComponentModel;
using System.Drawing;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.LevelObjects
{
    public class LevelVariables
    {
        [Category("Unknown"), DisplayName("Unknown 01")]
        public uint unk1 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 02")]
        public uint unk2 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 03")]
        public uint unk3 { get; set; }

        [Category("Attributes"), DisplayName("Fog Color")]
        public Color fogColor { get; set; }

        [Category("Unknown"), DisplayName("Unknown 04")]
        public uint unk4 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 05")]
        public uint unk5 { get; set; }

        [Category("Attributes"), DisplayName("Fog Distance")]
        public float fogDistance { get; set; }

        [Category("Unknown"), DisplayName("Unknown 06")]
        public float unk6 { get; set; }

        [Category("Attributes"), DisplayName("Deathplane Z")]
        public float deathPlaneZ { get; set; }

        [Category("Unknown"), DisplayName("Unknown 07")]
        public float unk7 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 08")]
        public float unk8 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 09")]
        public float unk9 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 10")]
        public uint unk10 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 11")]
        public int unk11 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 12")]
        public uint unk12 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 13")]
        public uint unk13 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 14")]
        public uint unk14 { get; set; }

        [Category("Unknown"), DisplayName("Unknown 15")]
        public uint unk15 { get; set; }

        public LevelVariables(byte[] levelVarBlock)
        {
            unk1 = ReadUint(levelVarBlock, 0x00);
            unk2 = ReadUint(levelVarBlock, 0x04);
            unk3 = ReadUint(levelVarBlock, 0x08);
            int r = ReadInt(levelVarBlock, 0x0c);

            int g = ReadInt(levelVarBlock, 0x10);
            int b = ReadInt(levelVarBlock, 0x14);
            unk4 = ReadUint(levelVarBlock, 0x18);
            unk5 = ReadUint(levelVarBlock, 0x1C);

            fogDistance = ReadFloat(levelVarBlock, 0x20);
            unk6 = ReadFloat(levelVarBlock, 0x24);
            deathPlaneZ = ReadFloat(levelVarBlock, 0x28);
            unk7 = ReadFloat(levelVarBlock, 0x2C);

            unk8 = ReadFloat(levelVarBlock, 0x30);
            unk9 = ReadFloat(levelVarBlock, 0x34);
            unk10 = ReadUint(levelVarBlock, 0x38);
            unk11 = ReadInt(levelVarBlock, 0x3C);

            unk12 = ReadUint(levelVarBlock, 0x40);
            unk13 = ReadUint(levelVarBlock, 0x44);
            unk14 = ReadUint(levelVarBlock, 0x48);
            unk15 = ReadUint(levelVarBlock, 0x4C);

            fogColor = Color.FromArgb(r, g, b);
        }

        public byte[] serialize()
        {
            byte[] bytes = new byte[0x50];

            WriteUint(ref bytes, 0x00, unk1);
            WriteUint(ref bytes, 0x04, unk2);
            WriteUint(ref bytes, 0x08, unk3);
            WriteUint(ref bytes, 0x0C, fogColor.R);

            WriteUint(ref bytes, 0x10, fogColor.G);
            WriteUint(ref bytes, 0x14, fogColor.B);
            WriteUint(ref bytes, 0x18, unk4);
            WriteUint(ref bytes, 0x1C, unk5);

            WriteFloat(ref bytes, 0x20, fogDistance);
            WriteFloat(ref bytes, 0x24, unk6);
            WriteFloat(ref bytes, 0x28, deathPlaneZ);
            WriteFloat(ref bytes, 0x2C, unk7);

            WriteFloat(ref bytes, 0x30, unk8);
            WriteFloat(ref bytes, 0x34, unk9);
            WriteUint(ref bytes, 0x38, unk10);
            WriteUint(ref bytes, 0x3C, (uint)unk11);

            WriteUint(ref bytes, 0x40, unk12);
            WriteUint(ref bytes, 0x44, unk13);
            WriteUint(ref bytes, 0x48, unk14);
            WriteUint(ref bytes, 0x4C, unk15);

            return bytes;
        }

    }
}
