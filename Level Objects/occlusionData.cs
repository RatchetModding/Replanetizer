using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class OcclusionData
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

        public byte[] serialize()
        {
            byte[] bytes = new byte[0x10 + mobyData.Count * 0x08 + tieData.Count * 0x08 + shrubData.Count * 0x08];
            WriteInt(ref bytes, 0x00, mobyData.Count);
            WriteInt(ref bytes, 0x04, tieData.Count);
            WriteInt(ref bytes, 0x08, shrubData.Count);

            int offset = 0;
            for(int i = 0; i < mobyData.Count; i++)
            {
                WriteInt(ref bytes, 0x10 + i * 0x08, mobyData[i].Key);
                WriteInt(ref bytes, 0x14 + i * 0x08, mobyData[i].Value);
            }
            offset += mobyData.Count * 0x08;
            for (int i = 0; i < tieData.Count; i++)
            {
                WriteInt(ref bytes, 0x10 + offset + i * 0x08, tieData[i].Key);
                WriteInt(ref bytes, 0x14 + offset + i * 0x08, tieData[i].Value);
            }
            offset += tieData.Count * 0x08;
            for (int i = 0; i < shrubData.Count; i++)
            {
                WriteInt(ref bytes, 0x10 + offset + i * 0x08, shrubData[i].Key);
                WriteInt(ref bytes, 0x14 + offset + i * 0x08, shrubData[i].Value);
            }

            return bytes;
        }
    }
}
