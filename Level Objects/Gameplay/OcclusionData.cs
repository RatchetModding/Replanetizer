using System;
using System.Collections.Generic;
using RatchetEdit.Headers;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.LevelObjects
{
    public class OcclusionData : ISerializable
    {
        public List<KeyValuePair<int, int>> mobyData;
        public List<KeyValuePair<int, int>> tieData;
        public List<KeyValuePair<int, int>> shrubData;

        public OcclusionData(byte[] occlusionBlock, OcclusionDataHeader head)
        {
            mobyData = new List<KeyValuePair<int, int>>();
            tieData = new List<KeyValuePair<int, int>>();
            shrubData = new List<KeyValuePair<int, int>>();

            int offset = 0;
            for (int i = 0; i < head.mobyCount; i++)
            {
                mobyData.Add(new KeyValuePair<int, int>(BitConverter.ToInt32(occlusionBlock, (i * 0x08) + 0x00), BitConverter.ToInt32(occlusionBlock, (i * 0x08) + 0x04)));
            }

            offset += head.mobyCount * 0x08;

            for (int i = 0; i < head.tieCount; i++)
            {
                tieData.Add(new KeyValuePair<int, int>(BitConverter.ToInt32(occlusionBlock, offset + (i * 0x08) + 0x00), BitConverter.ToInt32(occlusionBlock, offset + (i * 0x08) + 0x04)));
            }

            offset += head.tieCount * 0x08;

            for (int i = 0; i < head.shrubCount; i++)
            {
                shrubData.Add(new KeyValuePair<int, int>(BitConverter.ToInt32(occlusionBlock, offset + (i * 0x08) + 0x00), BitConverter.ToInt32(occlusionBlock, offset + (i * 0x08) + 0x04)));
            }


        }

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[0x10 + mobyData.Count * 0x08 + tieData.Count * 0x08 + shrubData.Count * 0x08];
            WriteInt(bytes, 0x00, mobyData.Count);
            WriteInt(bytes, 0x04, tieData.Count);
            WriteInt(bytes, 0x08, shrubData.Count);

            int offset = 0;
            for (int i = 0; i < mobyData.Count; i++)
            {
                BitConverter.GetBytes(mobyData[i].Key).CopyTo(bytes, 0x10 + i * 0x08);
                BitConverter.GetBytes(mobyData[i].Value).CopyTo(bytes, 0x14 + i * 0x08);
            }
            offset += mobyData.Count * 0x08;
            for (int i = 0; i < tieData.Count; i++)
            {
                BitConverter.GetBytes(tieData[i].Key).CopyTo(bytes, 0x10 + offset + i * 0x08);
                BitConverter.GetBytes(tieData[i].Value).CopyTo(bytes, 0x14 + offset + i * 0x08);
            }
            offset += tieData.Count * 0x08;
            for (int i = 0; i < shrubData.Count; i++)
            {
                BitConverter.GetBytes(shrubData[i].Key).CopyTo(bytes, 0x10 + offset + i * 0x08);
                BitConverter.GetBytes(shrubData[i].Value).CopyTo(bytes, 0x14 + offset + i * 0x08);
            }

            return bytes;
        }
    }
}
