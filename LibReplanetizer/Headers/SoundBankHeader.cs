// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;
using static LibReplanetizer.DataFunctions;


namespace LibReplanetizer.Headers
{
    public class SoundBankHeader
    {
        // https://github.com/vgmstream/vgmstream/blob/master/src/meta/bnk_sony.c

        public int version;
        public int sections;
        public int sblkOffset;
        public int sblkSize;
        public int dataOffset;
        public int dataSize;

        public int sblkMagic;
        public int sblkVersion;
        public ushort soundsEntries;
        public ushort grainsEntries;
        public ushort streamEntries;
        public uint soundPointer;
        public uint grainPointer;
        public uint grainDataPointer;
        public uint blockNamesPointer;

        public SoundBankHeader(FileStream soundBnkFile)
        {
            byte[] head = ReadBlock(soundBnkFile, 0x00, 0x18);

            version = ReadInt(head, 0x00);
            sections = ReadInt(head, 0x04);
            sblkOffset = ReadInt(head, 0x08);
            sblkSize = ReadInt(head, 0x0c);
            dataOffset = ReadInt(head, 0x10);
            dataSize = ReadInt(head, 0x14);

            byte[] sblkHead = ReadBlock(soundBnkFile, sblkOffset, 0x3c);

            sblkMagic = ReadInt(sblkHead, 0x00);
            sblkVersion = ReadInt(sblkHead, 0x04);

            soundsEntries = ReadUshort(sblkHead, 0x16);
            grainsEntries = ReadUshort(sblkHead, 0x18);
            streamEntries = ReadUshort(sblkHead, 0x1a);

            soundPointer = ReadUint(sblkHead, 0x1c);      // table1 
            grainPointer = ReadUint(sblkHead, 0x20);      // table2
            grainDataPointer = ReadUint(sblkHead, 0x34);  // table3
            blockNamesPointer = ReadUint(sblkHead, 0x38); // table4
        }

        public static string FindSoundBankFile(string enginePath)
        {
            return Path.Join(Path.GetDirectoryName(enginePath), "sound.bnk");
        }
    }
}
