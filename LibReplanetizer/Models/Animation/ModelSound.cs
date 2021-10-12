// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Models.Animations
{
    public class ModelSound
    {
        public int off00;

        //How far away can you hear the sound
        public float distance;

        //Setting a value here will negate distance and volume
        //The animation will play a sound unless the moby is unrendered
        public int masterVolume;

        public int volume;
        public int distortion;
        public int distortion2;
        public short off18;
        public short listIndex;
        public int off1C;

        public ModelSound(byte[] soundBlock, int num)
        {
            int offset = num * 0x20;
            off00 = ReadInt(soundBlock, offset + 0x00);
            distance = ReadFloat(soundBlock, offset + 0x04);
            masterVolume = ReadInt(soundBlock, offset + 0x08);
            volume = ReadInt(soundBlock, offset + 0x0C);
            distortion = ReadInt(soundBlock, offset + 0x10);
            distortion2 = ReadInt(soundBlock, offset + 0x14);
            off18 = ReadShort(soundBlock, offset + 0x18);
            listIndex = ReadShort(soundBlock, offset + 0x1A);
            off1C = ReadInt(soundBlock, offset + 0x1C);
        }

        public byte[] Serialize()
        {
            byte[] outBytes = new byte[0x20];

            WriteInt(outBytes, 0x00, off00);
            WriteFloat(outBytes, 0x04, distance);
            WriteInt(outBytes, 0x08, masterVolume);
            WriteInt(outBytes, 0x0C, volume);
            WriteInt(outBytes, 0x10, distortion);
            WriteInt(outBytes, 0x14, distortion2);
            WriteShort(outBytes, 0x18, off18);
            WriteShort(outBytes, 0x1A, listIndex);
            WriteInt(outBytes, 0x1C, off1C);

            return outBytes;
        }
    }
}
