using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{
    public class Frame
    {
        public float speed { get; set; }
        public ushort frameIndex { get; set; }
        public ushort frameLength { get; set; }

        public List<short[]> rotations { get; set; }
        public List<short[]> sec0s { get; set; }
        public List<short[]> translations { get; set; }

        public Frame(FileStream fs, int offset, int boneCount)
        {
            byte[] header = ReadBlock(fs, offset, 0x10);
            speed = ReadFloat(header, 0x00);
            frameIndex = ReadUshort(header, 0x04);
            frameLength = ReadUshort(header, 0x06);
            ushort sec0Pointer = ReadUshort(header, 0x08);
            ushort sec0Count = ReadUshort(header, 0x0A);
            ushort translationPointer = ReadUshort(header, 0x0C);
            ushort translationCount = ReadUshort(header, 0x0E);


            byte[] frameBlock = ReadBlock(fs, offset + 0x10, frameLength * 0x10);
            rotations = new List<short[]>();
            for (int i = 0; i < boneCount; i++)
            {
                short[] rot = new short[4];
                rot[0] = ReadShort(frameBlock, i * 8 + 0x00);
                rot[1] = ReadShort(frameBlock, i * 8 + 0x02);
                rot[2] = ReadShort(frameBlock, i * 8 + 0x04);
                rot[3] = ReadShort(frameBlock, i * 8 + 0x06);
                rotations.Add(rot);
            }


            sec0s = new List<short[]>();
            for (int i = 0; i < sec0Count; i++)
            {
                short[] rot = new short[4];
                rot[0] = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x00);
                rot[1] = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x02);
                rot[2] = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x04);
                rot[3] = ReadShort(frameBlock, sec0Pointer + i * 8 + 0x06);
                sec0s.Add(rot);
            }

            translations = new List<short[]>();
            for (int i = 0; i < translationCount; i++)
            {
                short[] rot = new short[4];
                rot[0] = ReadShort(frameBlock, translationPointer + i * 8 + 0x00);
                rot[1] = ReadShort(frameBlock, translationPointer + i * 8 + 0x02);
                rot[2] = ReadShort(frameBlock, translationPointer + i * 8 + 0x04);
                rot[3] = ReadShort(frameBlock, translationPointer + i * 8 + 0x06);
                translations.Add(rot);
            }
        }

        public byte[] Serialize()
        {

            byte[] rotationBytes = new byte[rotations.Count * 8];
            for (int i = 0; i < rotations.Count; i++)
            {
                WriteShort(rotationBytes, i * 8 + 0x00, rotations[i][0]);
                WriteShort(rotationBytes, i * 8 + 0x02, rotations[i][1]);
                WriteShort(rotationBytes, i * 8 + 0x04, rotations[i][2]);
                WriteShort(rotationBytes, i * 8 + 0x06, rotations[i][3]);
            }
            byte[] sec0Bytes = new byte[sec0s.Count * 8];
            for (int i = 0; i < sec0s.Count; i++)
            {
                WriteShort(sec0Bytes, i * 8 + 0x00, sec0s[i][0]);
                WriteShort(sec0Bytes, i * 8 + 0x02, sec0s[i][1]);
                WriteShort(sec0Bytes, i * 8 + 0x04, sec0s[i][2]);
                WriteShort(sec0Bytes, i * 8 + 0x06, sec0s[i][3]);
            }
            byte[] translationBytes = new byte[translations.Count * 8];
            for (int i = 0; i < translations.Count; i++)
            {
                WriteShort(translationBytes, i * 8 + 0x00, translations[i][0]);
                WriteShort(translationBytes, i * 8 + 0x02, translations[i][1]);
                WriteShort(translationBytes, i * 8 + 0x04, translations[i][2]);
                WriteShort(translationBytes, i * 8 + 0x06, translations[i][3]);
            }


            ushort sec0Pointer = (ushort)rotationBytes.Length;
            ushort translationPointer = (ushort)(rotationBytes.Length + sec0Bytes.Length);

            byte[] header = new byte[0x10];
            WriteFloat(header, 0x00, speed);
            WriteUshort(header, 0x04, frameIndex);
            WriteUshort(header, 0x06, frameLength);
            WriteUshort(header, 0x08, sec0Pointer);
            WriteUshort(header, 0x0A, (ushort)sec0s.Count);
            WriteUshort(header, 0x0C, translationPointer);
            WriteUshort(header, 0x0E, (ushort)translations.Count);

            byte[] outBytes = new byte[GetLength(0x10 + rotationBytes.Length + sec0Bytes.Length + translationBytes.Length)];
            header.CopyTo(outBytes, 0);
            rotationBytes.CopyTo(outBytes, 0x10);
            sec0Bytes.CopyTo(outBytes, 0x10 + rotationBytes.Length);
            translationBytes.CopyTo(outBytes, 0x10 + rotationBytes.Length + sec0Bytes.Length);

            return outBytes;
        }
    }
}
