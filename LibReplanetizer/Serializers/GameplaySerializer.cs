using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Serializers
{
    public class GameplaySerializer
    {
        public const int MOBYLENGTH = 0x78;

        public void Save(Level level, string fileName)
        {
            FileStream fs = File.Open(fileName, FileMode.Create);

            //Seek past the header
            fs.Seek(0xA0, SeekOrigin.Begin);

            GameplayHeader gameplayHeader = new GameplayHeader
            {
                type88Pointer = SeekWrite(fs, SerializeLevelObjects(level.type88s, Type88.ELEMENTSIZE)),
                levelVarPointer = SeekWrite(fs, level.levelVariables.serialize()),
                englishPointer = SeekWrite(fs, GetLangBytes(level.english)),
                lang2Pointer = SeekWrite(fs, GetLangBytes(level.lang2)),
                frenchPointer = SeekWrite(fs, GetLangBytes(level.french)),
                germanPointer = SeekWrite(fs, GetLangBytes(level.german)),
                spanishPointer = SeekWrite(fs, GetLangBytes(level.spanish)),
                italianPointer = SeekWrite(fs, GetLangBytes(level.italian)),
                lang7Pointer = SeekWrite(fs, GetLangBytes(level.lang7)),
                lang8Pointer = SeekWrite(fs, GetLangBytes(level.lang8)),
                type04Pointer = SeekWrite(fs, SerializeLevelObjects(level.type04s, Type04.ELEMENTSIZE)),
                type80Pointer = SeekWrite(fs, GetType80Bytes(level.type80s)),
                cameraPointer = SeekWrite(fs, SerializeLevelObjects(level.gameCameras, GameCamera.ELEMENTSIZE)),
                type0CPointer = SeekWrite(fs, SerializeLevelObjects(level.type0Cs, Type0C.ELEMENTSIZE)),
                mobyIdPointer = SeekWrite(fs, GetIdBytes(level.mobyIds)),
                mobyPointer = SeekWrite(fs, GetMobyBytes(level.mobs)),
                pvarSizePointer = SeekWrite(fs, GetPvarSizeBytes(level.pVars)),
                pvarPointer = SeekWrite(fs, GetPvarBytes(level.pVars)),
                type50Pointer = SeekWrite(fs, GetKeyValueBytes(level.type50s)),
                type5CPointer = SeekWrite(fs, GetKeyValueBytes(level.type5Cs)),
                unkPointer6 = SeekWrite(fs, level.unk6),
                unkPointer7 = SeekWrite(fs, level.unk7),
                tieIdPointer = SeekWrite(fs, GetIdBytes(level.tieIds)),
                tiePointer = SeekWrite(fs, level.tieData),
                shrubIdPointer = SeekWrite(fs, GetIdBytes(level.shrubIds)),
                shrubPointer = SeekWrite(fs, level.shrubData),
                splinePointer = SeekWrite(fs, GetSplineBytes(level.splines)),
                cuboidPointer = SeekWrite(fs, SerializeLevelObjects(level.cuboids, Cuboid.ELEMENTSIZE)),
                type64Pointer = SeekWrite(fs, SerializeLevelObjects(level.type64s, Type64.ELEMENTSIZE)),
                type68Pointer = SeekWrite(fs, SerializeLevelObjects(level.type68s, Type68.ELEMENTSIZE)),
                unkPointer12 = SeekWrite(fs, new byte[0x10]),
                unkPointer17 = SeekWrite(fs, level.unk17),
                type7CPointer = SeekWrite(fs, GetType7CBytes(level.type7Cs)),
                unkPointer14 = SeekWrite(fs, level.unk14),
                unkPointer13 = SeekWrite(fs, level.unk13),
                occlusionPointer = SeekWrite(fs, GetOcclusionBytes(level.occlusionData))
            };

            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
        }

        private int SeekWrite(FileStream fs, byte[] bytes)
        {
            if (bytes != null)
            {
                SeekPast(fs);
                int pos = (int)fs.Position;
                fs.Write(bytes, 0, bytes.Length);
                return pos;
            }
            else return 0;
        }

        private void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        public static byte[] GetLangBytes(Dictionary<int, String> languageData)
        {
            int headerSize = (languageData.Count() * 16) + 8;
            int dataSize = 0;
            foreach (KeyValuePair<int, String> entry in languageData)
            {
                int entrySize = entry.Value.Length + 1;
                if (entrySize % 4 != 0)
                {
                    entrySize += (4 - entrySize % 4);
                }
                dataSize += entrySize;
            }

            int totalSize = headerSize + dataSize;
            byte[] bytes = new byte[totalSize];

            WriteUint(bytes, 0, (uint)languageData.Count());
            WriteUint(bytes, 4, (uint)totalSize);

            int textPos = headerSize;
            int headerPos = 8;

            foreach (KeyValuePair<int, String> entry in languageData)
            {
                int entrySize = entry.Value.Length + 1;
                if (entrySize % 4 != 0)
                {
                    entrySize += 4 - (entrySize % 4);
                }

                System.Text.Encoding.ASCII.GetBytes(entry.Value, 0, entry.Value.Length, bytes, textPos);

                WriteUint(bytes, headerPos, (uint)textPos);
                WriteUint(bytes, headerPos + 4, (uint)entry.Key);
                WriteUint(bytes, headerPos + 8, 0xFFFFFFFF);
                WriteUint(bytes, headerPos + 12, 0);
                headerPos += 16;
                textPos += entrySize;
            }

            return bytes;
        }

        public byte[] GetMobyBytes(List<Moby> mobs)
        {
            if (mobs == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + mobs.Count * MOBYLENGTH];

            //Header
            WriteUint(bytes, 0, (uint)mobs.Count);
            WriteUint(bytes, 4, 0x100);

            for (int i = 0; i < mobs.Count; i++)
            {
                mobs[i].ToByteArray().CopyTo(bytes, 0x10 + i * MOBYLENGTH);
            }
            return bytes;
        }

        public byte[] SerializeLevelObjects<T>(List<T> levelobjects, int elementSize) where T : LevelObject
        {
            if (levelobjects == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + levelobjects.Count * elementSize];

            //Header
            WriteInt(bytes, 0, levelobjects.Count);

            for (int i = 0; i < levelobjects.Count; i++)
            {
                levelobjects[i].ToByteArray().CopyTo(bytes, 0x10 + i * elementSize);
            }

            return bytes;
        }

        public byte[] GetType7CBytes(List<Type7C> type7Cs)
        {
            if (type7Cs == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + type7Cs.Count * Type7C.ELEMENTSIZE];

            //Header
            WriteInt(bytes, 0, type7Cs.Count);

            for (int i = 0; i < type7Cs.Count; i++)
            {
                type7Cs[i].Serialize().CopyTo(bytes, 0x10 + i * Type7C.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType80Bytes(List<Type80> type80s)
        {
            if (type80s == null) return new byte[0x10];

            byte[] bytes = new byte[0x10 + type80s.Count * (Type80.HEADSIZE + Type80.DATASIZE)];

            //Header
            WriteInt(bytes, 0, type80s.Count);

            for (int i = 0; i < type80s.Count; i++)
            {
                byte[] headBytes = type80s[i].SerializeHead();
                byte[] dataBytes = type80s[i].SerializeData();

                headBytes.CopyTo(bytes, 0x10 + i * Type80.HEADSIZE);
                dataBytes.CopyTo(bytes, 0x10 + Type80.HEADSIZE * type80s.Count + i * Type80.DATASIZE);
            }
            return bytes;
        }

        public byte[] GetKeyValueBytes(List<KeyValuePair<int, int>> type50s)
        {
            if (type50s == null) return new byte[0x10];

            byte[] bytes = new byte[type50s.Count * 8 + 0x08];

            int idx = 0;
            foreach (KeyValuePair<int, int> pair in type50s)
            {
                WriteInt(bytes, idx * 8 + 0, pair.Key);
                WriteInt(bytes, idx * 8 + 4, pair.Value);
                idx++;
            }

            WriteInt(bytes, bytes.Length - 8, -1);
            WriteInt(bytes, bytes.Length - 4, -1);
            return bytes;
        }

        public byte[] GetIdBytes(List<int> ids)
        {
            if (ids == null) return new byte[0x10];

            byte[] bytes = new byte[0x04 + ids.Count * 4];
            BitConverter.GetBytes(ids.Count).CopyTo(bytes, 0);
            for (int i = 0; i < ids.Count; i++)
            {
                BitConverter.GetBytes(ids[i]).CopyTo(bytes, 0x04 + i * 0x04);
            }
            return bytes;
        }


        public byte[] GetSplineBytes(List<Spline> splines)
        {
            if (splines == null) return new byte[0x10];

            List<byte> splineData = new List<byte>();
            List<int> offsets = new List<int>();

            int offset = 0;
            foreach (Spline spline in splines)
            {
                byte[] splineBytes = spline.ToByteArray();
                splineData.AddRange(splineBytes);
                offsets.Add(offset);
                offset += splineBytes.Length;
            }

            byte[] offsetBlock = new byte[GetLength(offsets.Count * 4)];
            for (int i = 0; i < offsets.Count; i++)
            {
                WriteUint(offsetBlock, i * 4, (uint)offsets[i]);
            }

            var bytes = new byte[0x10 + offsetBlock.Length + splineData.Count];
            WriteUint(bytes, 0, (uint)splines.Count);
            WriteUint(bytes, 0x04, (uint)(0x10 + offsetBlock.Length));
            WriteUint(bytes, 0x08, (uint)(splineData.Count));
            offsetBlock.CopyTo(bytes, 0x10);
            splineData.CopyTo(bytes, 0x10 + offsetBlock.Length);

            return bytes;
        }

        public byte[] GetPvarSizeBytes(List<byte[]> pVars)
        {
            if (pVars == null) return new byte[0x10];

            byte[] bytes = new byte[pVars.Count * 8];
            uint offset = 0;
            for (int i = 0; i < pVars.Count; i++)
            {
                WriteUint(bytes, (i * 8) + 0x00, offset);
                WriteUint(bytes, (i * 8) + 0x04, (uint)pVars[i].Length);
                offset += (uint)pVars[i].Length;
            }
            return bytes;
        }

        public byte[] GetPvarBytes(List<byte[]> pVars)
        {
            if (pVars == null) return new byte[0x10];

            var bytes = new byte[pVars.Sum(arr => arr.Length)];
            int index = 0;
            foreach (var pVar in pVars)
            {
                pVar.CopyTo(bytes, index);
                index += pVar.Length;
            }

            return bytes;
        }

        public byte[] GetOcclusionBytes(OcclusionData occlusionData)
        {
            if (occlusionData == null) return new byte[0x10];

            return occlusionData.ToByteArray();
        }

    }
}
