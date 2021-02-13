using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models
{
    public class Attachment
    {
        List<byte> aBones;
        List<byte> bBones;

        public Attachment(FileStream fs, int offset)
        {
            byte[] counts = ReadBlock(fs, offset, 4);
            short aBoneCount = ReadShort(counts, 0);
            short bBoneCount = ReadShort(counts, 2);
            byte[] data = ReadBlock(fs, offset + 4, aBoneCount + bBoneCount);

            aBones = new List<byte>();
            for (int i = 0; i < aBoneCount; i++)
            {
                aBones.Add(data[i]);
            }

            bBones = new List<byte>();
            for (int i = 0; i < bBoneCount; i++)
            {
                bBones.Add(data[aBoneCount + i]);
            }
        }
        public byte[] Serialize()
        {
            byte[] outBytes = new byte[4 + GetLength4(aBones.Count + bBones.Count + 1)];

            WriteShort(outBytes, 0, (short)aBones.Count);
            WriteShort(outBytes, 2, (short)bBones.Count);

            int offs = 4;
            for (int i = 0; i < aBones.Count; i++)
            {
                outBytes[offs] = aBones[i];
                offs++;
            }
            for (int i = 0; i < bBones.Count; i++)
            {
                outBytes[offs] = bBones[i];
                offs++;
            }
            outBytes[offs] = 0xff;

            return outBytes;
        }

        public int GetLength4(int length)
        {
            while (length % 4 != 0)
            {
                length++;
            }
            return length;
        }
    }
}
