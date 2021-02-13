using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{
    public class Animation
    {
        public float unk1 { get; set; }
        public float unk2 { get; set; }
        public float unk3 { get; set; }
        public float unk4 { get; set; }
        public byte unk5 { get; set; }
        public byte unk7 { get; set; }

        public uint null1 { get; set; }
        public float speed { get; set; }

        public List<Frame> frames { get; set; } = new List<Frame>();
        public List<int> sounds { get; set; } = new List<int>();

        public List<byte> unknownBytes { get; set; } = new List<byte>();


        public Animation()
        {

        }
        public Animation(FileStream fs, int modelOffset, int animationOffset, int boneCount, bool force = false)
        {
            //Only try to parse if the offset is non-zero
            if (animationOffset == 0 && !force)
                return;

            if (modelOffset == 0)
                return;

            // Header
            byte[] header = ReadBlock(fs, modelOffset + animationOffset, 0x1C);
            unk1 = ReadFloat(header, 0x00);
            unk2 = ReadFloat(header, 0x04);
            unk3 = ReadFloat(header, 0x08);
            unk4 = ReadFloat(header, 0x0C);

            byte frameCount = header[0x10];
            unk5 = header[0x11];
            byte soundsCount = header[0x12];
            unk7 = header[0x13];

            null1 = ReadUint(header, 0x14);
            speed = ReadFloat(header, 0x18);

            if (null1 != 0)
            {
                unknownBytes.AddRange(ReadBlock(fs, modelOffset + null1, 0x60));
            }


            // Frames
            byte[] animationPointerBlock = ReadBlock(fs, modelOffset + animationOffset + 0x1C, frameCount * 0x04);
            for (int i = 0; i < frameCount; i++)
            {
                frames.Add(new Frame(fs, modelOffset + ReadInt(animationPointerBlock, i * 0x04), boneCount));
            }

            // Sound configs
            byte[] extrasBlock = ReadBlock(fs, (modelOffset + animationOffset) + 0x1C + frameCount * 0x04, soundsCount * 4);
            for (int i = 0; i < soundsCount; i++)
            {
                sounds.Add(ReadInt(extrasBlock, i * 4));
            }
        }

        public byte[] Serialize(int baseOffset = 0, int fileOffset = 0)
        {
            // Head
            byte[] head = new byte[0x1C];
            WriteFloat(head, 0x00, unk1);
            WriteFloat(head, 0x04, unk2);
            WriteFloat(head, 0x08, unk3);
            WriteFloat(head, 0x0C, unk4);
            head[0x10] = (byte)frames.Count;
            head[0x11] = unk5;
            head[0x12] = (byte)sounds.Count;
            head[0x13] = unk7;
            WriteUint(head, 0x14, null1);
            WriteFloat(head, 0x18, speed);

            // Sound configs
            byte[] soundBytes = new byte[sounds.Count * 4];
            for (int i = 0; i < sounds.Count; i++)
            {
                WriteInt(soundBytes, i * 4, sounds[i]);
            }

            // Frames
            int framesSize = 0;
            var frameBytes = new List<byte[]>();
            foreach (Frame frame in frames)
            {
                byte[] frameByte = frame.Serialize();
                frameBytes.Add(frameByte);
                framesSize += frameByte.Length;
            }

            // The end of the list needs to be aligned to 0x20
            int offs;
            if ((fileOffset + baseOffset) % 0x20 == 0)
            {
                offs = GetLength20(0x1C + frames.Count * 4 + soundBytes.Length);
            }
            else
            {
                offs = GetLength20(0x1C + frames.Count * 4 + soundBytes.Length + 0x10) - 0x10;
            }

            // Make out array and copy to it
            byte[] outBytes = new byte[offs + framesSize + unknownBytes.Count];
            head.CopyTo(outBytes, 0);
            soundBytes.CopyTo(outBytes, 0x1C + frames.Count * 4);
            unknownBytes.CopyTo(outBytes, offs);
            offs += unknownBytes.Count;
            for (int i = 0; i < frameBytes.Count; i++)
            {
                WriteInt(outBytes, 0x1C + i * 4, offs + baseOffset);
                frameBytes[i].CopyTo(outBytes, offs);
                offs += frameBytes[i].Length;
            }

            return outBytes;
        }
    }
}
