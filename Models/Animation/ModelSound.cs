using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Models.Animations
{
    public class ModelSound
    {
        public int off_00;

        //How far away can you hear the sound
        public float distance;

        //Setting a value here will negate distance and volume
        //The animation will play a sound unless the moby is unrendered
        public int masterVolume;

        public int volume;
        public int distortion;
        public int distortion2;
        public short off_18;
        public short listIndex;
        public int off_1C;

        public ModelSound(byte[] soundBlock, int num)
        {
            int offset = num * 0x20;
            off_00 = ReadInt(soundBlock, offset+ 0x00);
            distance = ReadFloat(soundBlock, offset + 0x04);
            masterVolume = ReadInt(soundBlock, offset + 0x08);
            volume = ReadInt(soundBlock, offset + 0x0C);
            distortion = ReadInt(soundBlock, offset + 0x10);
            distortion2 = ReadInt(soundBlock, offset + 0x14);
            off_18 = ReadShort(soundBlock, offset + 0x18);
            listIndex = ReadShort(soundBlock, offset + 0x1A);
            off_1C = ReadInt(soundBlock, offset + 0x1C);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x20];

            WriteInt(ref outBytes, 0x00, off_00);
            WriteFloat(ref outBytes, 0x04, distance);
            WriteInt(ref outBytes, 0x08, masterVolume);
            WriteInt(ref outBytes, 0x0C, volume);
            WriteInt(ref outBytes, 0x10, distortion);
            WriteInt(ref outBytes, 0x14, distortion2);
            WriteShort(ref outBytes, 0x18, off_18);
            WriteShort(ref outBytes, 0x1A, listIndex);
            WriteInt(ref outBytes, 0x1C, off_1C);

            return outBytes;
        }
    }
}
