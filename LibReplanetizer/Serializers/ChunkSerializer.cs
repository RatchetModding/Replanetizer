using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Serializers
{
    public class ChunkSerializer
    {

        public void Save(Level level, string directory, int chunk)
        {
            if (chunk >= level.terrainChunks.Count)
                throw new IndexOutOfRangeException("Chunk does not exist!");

            directory = Path.Join(directory, "chunk" + chunk + ".ps3");

            FileStream fs = File.Open(directory, FileMode.Create);

            // Seek past the header
            fs.Seek(0x10, SeekOrigin.Begin);

            ChunkHeader chunkHeader = new ChunkHeader()
            {
                terrainPointer = SeekWrite(fs, WriteTfrags(level.terrainChunks[chunk], (int)fs.Position)),
                collisionPointer = SeekWrite(fs, level.collBytesChunks[chunk])
            };

            byte[] head = chunkHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
        }

        private int SeekWrite(FileStream fs, byte[] bytes)
        {
            int pos = (int)fs.Position;
            fs.Write(bytes, 0, bytes.Length);
            SeekPast(fs);
            return pos;
        }

        private void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        private byte[] WriteTfrags(List<TerrainFragment> tFrags, int fileOffset)
        {
            List<List<byte>> vertBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> rgbaBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> uvBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };
            List<List<byte>> indexBytes = new List<List<byte>>() { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };

            List<byte> textureBytes = new List<byte>();

            byte[] tfragHeads = new byte[0x30 * tFrags.Count];

            ushort chunk = 0;

            for (int i = 0; i < tFrags.Count; i++)
            {
                TerrainModel mod = (TerrainModel)(tFrags[i].model);

                int offset = i * 0x30;
                WriteFloat(tfragHeads, offset + 0x00, tFrags[i].off_00);
                WriteFloat(tfragHeads, offset + 0x04, tFrags[i].off_04);
                WriteFloat(tfragHeads, offset + 0x08, tFrags[i].off_08);
                WriteFloat(tfragHeads, offset + 0x0C, tFrags[i].off_0C);

                WriteInt(tfragHeads, offset + 0x10, fileOffset + 0x60 + tfragHeads.Length + textureBytes.Count);
                WriteInt(tfragHeads, offset + 0x14, tFrags[i].model.textureConfig.Count);

                byte[] modelVertBytes = mod.SerializeVerts();
                if (((vertBytes[chunk].Count + modelVertBytes.Length) / 0x1c) > 0xffff)
                {
                    chunk++;
                }

                WriteUshort(tfragHeads, offset + 0x18, (ushort)(vertBytes[chunk].Count / 0x1c));
                WriteUshort(tfragHeads, offset + 0x1a, (ushort)(tFrags[i].model.vertexBuffer.Length / 8));

                WriteUshort(tfragHeads, offset + 0x1C, tFrags[i].off_1C);
                WriteUshort(tfragHeads, offset + 0x1E, tFrags[i].off_1E);
                WriteUshort(tfragHeads, offset + 0x20, tFrags[i].off_20);
                WriteUshort(tfragHeads, offset + 0x22, chunk);
                WriteUint(tfragHeads, offset + 0x24, tFrags[i].off_24);
                WriteUint(tfragHeads, offset + 0x28, tFrags[i].off_28);
                WriteUint(tfragHeads, offset + 0x2C, tFrags[i].off_2C);

                foreach (var texConf in tFrags[i].model.textureConfig)
                {
                    byte[] texBytes = new byte[0x10];
                    WriteInt(texBytes, 0x00, texConf.ID);
                    WriteInt(texBytes, 0x04, texConf.start + indexBytes[chunk].Count / 2);
                    WriteInt(texBytes, 0x08, texConf.size);
                    WriteInt(texBytes, 0x0C, texConf.mode);
                    textureBytes.AddRange(texBytes);
                }

                indexBytes[chunk].AddRange(tFrags[i].model.GetFaceBytes((ushort)(vertBytes[chunk].Count / 0x1C)));
                vertBytes[chunk].AddRange(modelVertBytes);
                rgbaBytes[chunk].AddRange(tFrags[i].model.rgbas);
                uvBytes[chunk].AddRange(tFrags[i].model.SerializeUVs());

            }

            List<byte> outBytes = new List<byte>();

            byte[] head = new byte[0x60];
            WriteInt(head, 0, fileOffset + 0x60);
            WriteUshort(head, 0x4, (ushort)tFrags.Count);
            WriteUshort(head, 0x6, (ushort)tFrags.Count);

            outBytes.AddRange(head);
            outBytes.AddRange(tfragHeads);
            outBytes.AddRange(textureBytes);

            int[] vertOffsets = { 0, 0, 0, 0 };
            int[] rgbaOffsets = { 0, 0, 0, 0 };
            int[] uvOffsets = { 0, 0, 0, 0 };
            int[] indexOffsets = { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (vertBytes[i].Count == 0)
                {
                    continue;
                }
                Pad(outBytes);
                vertOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(vertBytes[i]);
                Pad(outBytes);
                rgbaOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(rgbaBytes[i]);
                Pad(outBytes);
                uvOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(uvBytes[i]);
                Pad(outBytes);
                indexOffsets[i] = fileOffset + outBytes.Count;
                outBytes.AddRange(indexBytes[i]);
                Pad(outBytes);
            }


            byte[] outByteArr = outBytes.ToArray();

            for (int i = 0; i < 4; i++)
            {
                WriteInt(outByteArr, 0x08 + i * 4, vertOffsets[i]);
                WriteInt(outByteArr, 0x18 + i * 4, rgbaOffsets[i]);
                WriteInt(outByteArr, 0x28 + i * 4, uvOffsets[i]);
                WriteInt(outByteArr, 0x38 + i * 4, indexOffsets[i]);
            }

            return outByteArr;
        }
    }
}
