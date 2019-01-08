using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Animation
    {
        public float unk1 { get; set; }
        public float unk2 { get; set; }
        public float unk3 { get; set; }
        public float unk4 { get; set; }

        public byte frameCount { get; set; }
        public byte unk5 { get; set; }
        public byte unk6 { get; set; }
        public byte unk7 { get; set; }

        public uint null1 { get; set; }
        public float speed { get; set; }

        public List<Frame> frames { get; set; }

        public Animation(FileStream fs, int modelOffset, int animationOffset)
        {
            //We only want to parse this data if the offset is not 0
            if (animationOffset > 0)
            {
                byte[] header = ReadBlock(fs, modelOffset + animationOffset, 0x1C);
                unk1 = ReadFloat(header, 0x00);
                unk2 = ReadFloat(header, 0x04);
                unk3 = ReadFloat(header, 0x08);
                unk4 = ReadFloat(header, 0x0C);

                frameCount = header[0x10];
                unk5 = header[0x11];
                unk6 = header[0x12];
                unk7 = header[0x13];

                null1 = ReadUint(header, 0x14);
                speed = ReadFloat(header, 0x18);

                byte[] animationPointerBlock = ReadBlock(fs, modelOffset + animationOffset + 0x1C, frameCount * 0x04);
                frames = new List<Frame>();
                for (int i = 0; i < frameCount; i++)
                {
                    frames.Add(new Frame(fs, modelOffset + ReadInt(animationPointerBlock, i * 0x04)));
                }
            }
        }
    }
}
