// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using LibReplanetizer.Headers;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    // https://github.com/vgmstream/vgmstream/blob/master/src/meta/bnk_sony.c
    public class SoundBankParser : RatchetFileParser, IDisposable
    {
        SoundBankHeader soundbankHead;

        public SoundBankParser(string soundbankFile) : base(soundbankFile)
        {
            soundbankHead = new SoundBankHeader(fileStream);
        }

        public List<List<(byte[] sample, int sampleRate)>> GetSounds()
        {
            var sounds = new List<List<(byte[] sample, int sampleRate)>>();

            int soundPointer = soundbankHead.sblkOffset + (int)soundbankHead.soundPointer;
            int grainPointer = soundbankHead.sblkOffset + (int)soundbankHead.grainPointer;
            int grainDataPointer = soundbankHead.sblkOffset + (int)soundbankHead.grainDataPointer; 

            for (int i = 0; i < soundbankHead.soundsEntries; i++)
            {
                byte[] soundBlock = ReadBlock(fileStream, soundPointer + i * 0x0C, 0x0C);
                int grainEntryCount = soundBlock[0x04];
                int grainEntry = ReadInt(soundBlock, 0x08);

                var samples = new List<(byte[] sample, int sampleRate)>();
                for (int grain = 0; grain < grainEntryCount; grain++)
                {
                    byte[] grainBlock = ReadBlock(fileStream, grainPointer + grainEntry + grain * 0x08, 0x08);

                    uint grainValue = ReadUint(grainBlock, 0x00);
                    uint grainDataEntry = grainValue & 0xFFFF;
                    uint grainSubType = grainValue >> 16;
                    if (grainSubType != 0x0100)
                        continue;

                    byte[] sndhBlock = ReadBlock(fileStream, grainDataPointer + (int)grainDataEntry, 0x18);
                    byte centerNote = sndhBlock[0x02];
                    byte centerFine = sndhBlock[0x03];
                    int streamOffset = ReadInt(sndhBlock, 0x10);
                    int streamSize = ReadInt(sndhBlock, 0x14);

                    byte[] stream = ReadBlock(fileStream, soundbankHead.dataOffset + streamOffset, streamSize);

                    int sampleRate = GetSampleRate(centerNote, centerFine);
                    samples.Add((stream, sampleRate));
                }
                sounds.Add(samples);
            }
            return sounds;
        }

        private static readonly ushort[] NOTE_PITCH_TABLE =
        {
            0x8000, 0x879C, 0x8FAC, 0x9837, 0xA145, 0xAADC,
            0xB504, 0xBFC8, 0xCB2F, 0xD744, 0xE411, 0xF1A1
        };

        private static readonly ushort[] FINE_PITCH_TABLE =
        {
            0x8000, 0x800E, 0x801D, 0x802C, 0x803B, 0x804A, 0x8058, 0x8067,
            0x8076, 0x8085, 0x8094, 0x80A3, 0x80B1, 0x80C0, 0x80CF, 0x80DE,
            0x80ED, 0x80FC, 0x810B, 0x811A, 0x8129, 0x8138, 0x8146, 0x8155,
            0x8164, 0x8173, 0x8182, 0x8191, 0x81A0, 0x81AF, 0x81BE, 0x81CD,
            0x81DC, 0x81EB, 0x81FA, 0x8209, 0x8218, 0x8227, 0x8236, 0x8245,
            0x8254, 0x8263, 0x8272, 0x8282, 0x8291, 0x82A0, 0x82AF, 0x82BE,
            0x82CD, 0x82DC, 0x82EB, 0x82FA, 0x830A, 0x8319, 0x8328, 0x8337,
            0x8346, 0x8355, 0x8364, 0x8374, 0x8383, 0x8392, 0x83A1, 0x83B0,
            0x83C0, 0x83CF, 0x83DE, 0x83ED, 0x83FD, 0x840C, 0x841B, 0x842A,
            0x843A, 0x8449, 0x8458, 0x8468, 0x8477, 0x8486, 0x8495, 0x84A5,
            0x84B4, 0x84C3, 0x84D3, 0x84E2, 0x84F1, 0x8501, 0x8510, 0x8520,
            0x852F, 0x853E, 0x854E, 0x855D, 0x856D, 0x857C, 0x858B, 0x859B,
            0x85AA, 0x85BA, 0x85C9, 0x85D9, 0x85E8, 0x85F8, 0x8607, 0x8617,
            0x8626, 0x8636, 0x8645, 0x8655, 0x8664, 0x8674, 0x8683, 0x8693,
            0x86A2, 0x86B2, 0x86C1, 0x86D1, 0x86E0, 0x86F0, 0x8700, 0x870F,
            0x871F, 0x872E, 0x873E, 0x874E, 0x875D, 0x876D, 0x877D, 0x878C
        };
        private static ushort NoteToPitch(int centerNote, int centerFine, int note, int fine)
        {
            int unk2;
            ushort pitch;

            int fineIndex = fine + centerFine;
            int fineAdjust = fineIndex;

            if(fineIndex < 0)
                fineAdjust = fineIndex + 0x7f;

            fineAdjust /= 128;
            int noteAdjust = note + fineAdjust - centerNote;
            int unk3 = noteAdjust / 6;

            if (noteAdjust < 0)
                unk3--;

            fineIndex -= fineAdjust * 128;

            if (noteAdjust < 0)
                unk2 = -1;
            else
                unk2 = 0;
            if (unk3 < 0)
                unk3--;

            unk2 = (unk3 / 2) - unk2;
            int unk1 = unk2 - 2;

            int noteIndex = noteAdjust - (unk2 * 12);

            if((noteIndex < 0) || (noteIndex == 0) && (fineIndex < 0)) {
                noteIndex += 12;
                unk1 = unk2 - 3;
            }

            if(fineIndex < 0)
            {
                noteIndex = (noteIndex - 1) + fineAdjust;
                fineIndex += (fineAdjust + 1) * 128;
            }

            noteIndex = Math.Clamp(noteIndex, 0, 12 - 1);
            fineIndex = Math.Clamp(fineIndex, 0, 128 - 1);

            pitch = (ushort)(NOTE_PITCH_TABLE[noteIndex] * FINE_PITCH_TABLE[fineIndex] >> 16);

            if (unk1 < 0)
                pitch = (ushort)((pitch + (1 << (-unk1 - 1))) >> -unk1);


            return pitch;
        }

        private static int GetSampleRate(byte centerNote, byte centerFine)
        {
            bool isNegative = (centerNote >> 7) != 0;
            int note = isNegative ? 0x100 - centerNote : centerNote;

            int pitch = NoteToPitch(note, centerFine, 60, 0);

            if (pitch > 0x4000)
                pitch = 0x4000;

            if (!isNegative)
                pitch = (pitch * 44100) / 48000;

            return (48000 * pitch) / 4096;
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
