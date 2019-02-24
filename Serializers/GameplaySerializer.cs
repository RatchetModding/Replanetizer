using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.Serializers
{
    class GameplaySerializer
    {
        public const int MOBYLENGTH = 0x78;


        public void Save(Level level, String fileName)
        {
            FileStream fs = File.Open(fileName, FileMode.Create);
            GameplayHeader gameplayHeader = new GameplayHeader();

            byte[] mobs = GetMobies(level.mobs);
            byte[] levelVariables = level.levelVariables.serialize();
            byte[] pVarSizes = GetPvarSizes(level.pVars);
            byte[] pVars = GetPvars(level.pVars);
            byte[] splines = GetSplines(level.splines);
            byte[] spawnPoints = GetSpawnPoints(level.cuboids);
            byte[] gameCameras = GetGameCameras(level.gameCameras);
            byte[] type04s = GetType04s(level.type04s);
            byte[] type0Cs = GetType0Cs(level.type0Cs);
            byte[] type64s = GetType64s(level.type64s);
            byte[] type68s = GetType68s(level.type68s);
            byte[] type80s = GetType80s(level.type80s);
            byte[] type88s = GetType88s(level.type88s);
            byte[] type50s = GetType50s(level.type50s);
            byte[] type5Cs = GetType50s(level.type5Cs);
            byte[] type7Cs = GetType7Cs(level.type7Cs);
            byte[] mobyIds = GetIds(level.mobyIds);
            byte[] tieIds = GetIds(level.tieIds);
            byte[] shrubIds = GetIds(level.shrubIds);
            byte[] off_6C = new byte[0x10];
            byte[] occlusionData = GetOcclusionData(level.occlusionData);

            //Seek past the header
            fs.Seek(0xA0, SeekOrigin.Begin);

            gameplayHeader.type88Pointer = (int)fs.Position;
            fs.Write(type88s, 0, type88s.Length);

            SeekPast(fs);
            gameplayHeader.levelVarPointer = (int)fs.Position;
            fs.Write(levelVariables, 0, levelVariables.Length);

            SeekPast(fs);
            gameplayHeader.englishPointer = (int)fs.Position;
            fs.Write(level.english, 0, level.english.Length);

            SeekPast(fs);
            gameplayHeader.lang2Pointer = (int)fs.Position;
            fs.Write(level.lang2, 0, level.lang2.Length);

            SeekPast(fs);
            gameplayHeader.frenchPointer = (int)fs.Position;
            fs.Write(level.french, 0, level.french.Length);

            SeekPast(fs);
            gameplayHeader.germanPointer = (int)fs.Position;
            fs.Write(level.german, 0, level.german.Length);

            SeekPast(fs);
            gameplayHeader.spanishPointer = (int)fs.Position;
            fs.Write(level.spanish, 0, level.spanish.Length);

            SeekPast(fs);
            gameplayHeader.italianPointer = (int)fs.Position;
            fs.Write(level.italian, 0, level.italian.Length);

            SeekPast(fs);
            gameplayHeader.lang7Pointer = (int)fs.Position;
            fs.Write(level.lang7, 0, level.lang7.Length);

            SeekPast(fs);
            gameplayHeader.lang8Pointer = (int)fs.Position;
            fs.Write(level.lang8, 0, level.lang8.Length);

            SeekPast(fs);
            gameplayHeader.type04Pointer = (int)fs.Position;
            fs.Write(type04s, 0, type04s.Length);

            SeekPast(fs);
            gameplayHeader.type80Pointer = (int)fs.Position;
            fs.Write(type80s, 0, type80s.Length);

            SeekPast(fs);
            gameplayHeader.cameraPointer = (int)fs.Position;
            fs.Write(gameCameras, 0, gameCameras.Length);

            SeekPast(fs);
            gameplayHeader.type0CPointer = (int)fs.Position;
            fs.Write(type0Cs, 0, type0Cs.Length);

            SeekPast(fs);
            gameplayHeader.mobyIdPointer = (int)fs.Position;
            fs.Write(mobyIds, 0, mobyIds.Length);

            SeekPast(fs);
            gameplayHeader.mobyPointer = (int)fs.Position;
            fs.Write(mobs, 0, mobs.Length);

            SeekPast(fs);
            gameplayHeader.pvarSizePointer = (int)fs.Position;
            fs.Write(pVarSizes, 0, pVarSizes.Length);

            SeekPast(fs);
            gameplayHeader.pvarPointer = (int)fs.Position;
            fs.Write(pVars, 0, pVars.Length);

            SeekPast(fs);
            gameplayHeader.type50Pointer = (int)fs.Position;
            fs.Write(type50s, 0, type50s.Length);

            SeekPast(fs);
            gameplayHeader.type5CPointer = (int)fs.Position;
            fs.Write(type5Cs, 0, type5Cs.Length);

            SeekPast(fs);
            gameplayHeader.unkPointer6 = (int)fs.Position;
            fs.Write(level.unk6, 0, level.unk6.Length);

            SeekPast(fs);
            gameplayHeader.unkPointer7 = (int)fs.Position;
            fs.Write(level.unk7, 0, level.unk7.Length);

            SeekPast(fs);
            gameplayHeader.tieIdPointer = (int)fs.Position;
            fs.Write(tieIds, 0, tieIds.Length);

            SeekPast(fs);
            gameplayHeader.tiePointer = (int)fs.Position;
            fs.Write(level.tieData, 0, level.tieData.Length);

            SeekPast(fs);
            gameplayHeader.shrubIdPointer = (int)fs.Position;
            fs.Write(shrubIds, 0, shrubIds.Length);

            SeekPast(fs);
            gameplayHeader.shrubPointer = (int)fs.Position;
            fs.Write(level.shrubData, 0, level.shrubData.Length);

            SeekPast(fs);
            gameplayHeader.splinePointer = (int)fs.Position;
            fs.Write(splines, 0, splines.Length);

            SeekPast(fs);
            gameplayHeader.spawnPointPointer = (int)fs.Position;
            fs.Write(spawnPoints, 0, spawnPoints.Length);

            SeekPast(fs);
            gameplayHeader.type64Pointer = (int)fs.Position;
            fs.Write(type64s, 0, type64s.Length);

            SeekPast(fs);
            gameplayHeader.type68Pointer = (int)fs.Position;
            fs.Write(type68s, 0, type68s.Length);

            SeekPast(fs);
            gameplayHeader.unkPointer12 = (int)fs.Position;
            fs.Write(off_6C, 0, off_6C.Length);

            SeekPast(fs);
            gameplayHeader.unkPointer17 = (int)fs.Position;
            fs.Write(level.unk17, 0, level.unk17.Length);

            SeekPast(fs);
            gameplayHeader.type7CPointer = (int)fs.Position;
            fs.Write(type7Cs, 0, type7Cs.Length);

            SeekPast(fs);
            gameplayHeader.unkPointer14 = (int)fs.Position;
            fs.Write(level.unk14, 0, level.unk14.Length);

            SeekPast(fs);
            gameplayHeader.unkPointer13 = (int)fs.Position;
            fs.Write(level.unk13, 0, level.unk13.Length);

            SeekPast(fs);
            gameplayHeader.occlusionPointer = (int)fs.Position;
            fs.Write(occlusionData, 0, occlusionData.Length);

            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
        }

        private void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        public byte[] GetMobies(List<Moby> mobs)
        {
            if (mobs == null) { return new byte[0x10]; }

            byte[] bytes = new byte[MOBYLENGTH * mobs.Count + 0x10];

            //Header
            WriteUint(ref bytes, 0, (uint)mobs.Count);
            WriteUint(ref bytes, 4, 0x100);

            int index = 0;
            foreach (Moby moby in mobs)
            {
                byte[] mobyBytes = moby.ToByteArray();
                Array.Copy(mobyBytes, 0, bytes, index * MOBYLENGTH + 0x10, MOBYLENGTH);
                index++;
            }
            return bytes;
        }


        public byte[] GetSpawnPoints(List<Cuboid> spawnPoints)
        {
            if (spawnPoints == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x80 * spawnPoints.Count + 0x10];

            //Header
            WriteUint(ref bytes, 0, (uint)spawnPoints.Count);

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                byte[] spawnPointBytes = spawnPoints[i].ToByteArray();
                spawnPointBytes.CopyTo(bytes, 0x10 + i * 0x80);
            }
            return bytes;
        }

        public byte[] GetGameCameras(List<GameCamera> gameCameras)
        {
            if (gameCameras == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + gameCameras.Count * GameCamera.ELEMENTSIZE];

            //Header
            WriteUint(ref bytes, 0, (uint)gameCameras.Count);

            for (int i = 0; i < gameCameras.Count; i++)
            {
                byte[] cameraBytes = gameCameras[i].ToByteArray();
                cameraBytes.CopyTo(bytes, 0x10 + i * GameCamera.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType04s(List<Type04> type04s)
        {
            if (type04s == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type04s.Count * Type04.ELEMENTSIZE];

            //Header
            WriteInt(ref bytes, 0, type04s.Count);

            for (int i = 0; i < type04s.Count; i++)
            {
                byte[] type04Byte = type04s[i].ToByteArray();
                type04Byte.CopyTo(bytes, 0x10 + i * Type04.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType0Cs(List<Type0C> type0Cs)
        {
            if (type0Cs == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type0Cs.Count * Type0C.ELEMENTSIZE];

            //Header
            WriteInt(ref bytes, 0, type0Cs.Count);

            for (int i = 0; i < type0Cs.Count; i++)
            {
                byte[] type0CByte = type0Cs[i].ToByteArray();
                type0CByte.CopyTo(bytes, 0x10 + i * Type0C.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType64s(List<Type64> type64s)
        {
            if (type64s == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type64s.Count * Type64.ELEMENTSIZE];

            //Header
            WriteInt(ref bytes, 0, type64s.Count);

            for (int i = 0; i < type64s.Count; i++)
            {
                byte[] type64Bytes = type64s[i].ToByteArray();
                type64Bytes.CopyTo(bytes, 0x10 + i * Type64.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType68s(List<Type68> type68s)
        {
            if (type68s == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type68s.Count * Type68.ELEMENTSIZE];

            //Header
            WriteInt(ref bytes, 0, type68s.Count);

            for (int i = 0; i < type68s.Count; i++)
            {
                byte[] type68Bytes = type68s[i].ToByteArray();
                type68Bytes.CopyTo(bytes, 0x10 + i * Type68.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType7Cs(List<Type7C> type7Cs)
        {
            if (type7Cs == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type7Cs.Count * Type7C.ELEMENTSIZE];

            //Header
            WriteInt(ref bytes, 0, type7Cs.Count);

            for (int i = 0; i < type7Cs.Count; i++)
            {
                byte[] type7CBytes = type7Cs[i].Serialize();
                type7CBytes.CopyTo(bytes, 0x10 + i * Type7C.ELEMENTSIZE);
            }

            return bytes;
        }

        public byte[] GetType88s(List<Type88> type88s)
        {
            if (type88s == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type88s.Count * Type88.ELEMENTSIZE];

            //Header
            WriteInt(ref bytes, 0, type88s.Count);

            for (int i = 0; i < type88s.Count; i++)
            {
                byte[] type88Bytes = type88s[i].ToByteArray();
                type88Bytes.CopyTo(bytes, 0x10 + i * Type88.ELEMENTSIZE);
            }

            return bytes;
        }


        public byte[] GetType80s(List<Type80> type80s)
        {
            if (type80s == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x10 + type80s.Count * (Type80.HEADSIZE + Type80.DATASIZE)];

            //Header
            WriteInt(ref bytes, 0, type80s.Count);

            for (int i = 0; i < type80s.Count; i++)
            {
                byte[] headBytes = type80s[i].SerializeHead();
                byte[] dataBytes = type80s[i].SerializeData();

                headBytes.CopyTo(bytes, 0x10 + i * Type80.HEADSIZE);
                dataBytes.CopyTo(bytes, 0x10 + Type80.HEADSIZE * type80s.Count + i * Type80.DATASIZE);
            }
            return bytes;
        }

        public byte[] GetType50s(List<KeyValuePair<int,int>> type50s)
        {
            if (type50s == null) { return new byte[0x10]; }

            byte[] bytes = new byte[type50s.Count * 8 + 0x08];

            for (int i = 0; i < type50s.Count; i++)
            {
                WriteInt(ref bytes, i * 8 + 0, type50s[i].Key);
                WriteInt(ref bytes, i * 8 + 4, type50s[i].Value);
            }

            WriteInt(ref bytes, bytes.Length - 8, -1);
            WriteInt(ref bytes, bytes.Length - 4, -1);
            return bytes;
        }

        public byte[] GetIds(List<int> ids)
        {
            if (ids == null) { return new byte[0x10]; }

            byte[] bytes = new byte[0x04 + ids.Count * 4];
            BitConverter.GetBytes(ids.Count).CopyTo(bytes, 0);
            for (int i = 0; i < ids.Count; i++)
            {
                BitConverter.GetBytes(ids[i]).CopyTo(bytes, 0x04 + i * 0x04);
            }
            return bytes;
        }


        public byte[] GetSplines(List<Spline> splines)
        {
            if (splines == null) { return new byte[0x10]; }

            List<byte[]> splineData = new List<byte[]>();
            List<int> offsets = new List<int>();
            int offset = 0;
            foreach (Spline spline in splines)
            {
                byte[] splineBytes = spline.ToByteArray();
                splineData.Add(splineBytes);
                offsets.Add(offset);
                offset += splineBytes.Length;
            }

            int offsetBlockLength = offsets.Count * 4;
            while (offsetBlockLength % 0x10 != 0)
            {
                offsetBlockLength += 0x4;
            }
            byte[] offsetBlock = new byte[offsetBlockLength];
            for (int i = 0; i < offsets.Count; i++)
            {
                WriteUint(ref offsetBlock, i * 4, (uint)offsets[i]);
            }


            var bytes = new byte[0x10 + offsetBlock.Length + splineData.Sum(arr => arr.Length)];
            WriteUint(ref bytes, 0, (uint)splines.Count);
            WriteUint(ref bytes, 0x04, (uint)offsetBlock.Length + 0x10);
            WriteUint(ref bytes, 0x08, (uint)(splineData.Sum(arr => arr.Length)));
            offsetBlock.CopyTo(bytes, 0x10);

            int index = offsetBlock.Length + 0x10;
            foreach (var spline in splineData)
            {
                spline.CopyTo(bytes, index);
                index += spline.Length;
            }

            return bytes;
        }

        public byte[] GetPvarSizes(List<byte[]> pVars)
        {
            if (pVars == null) { return new byte[0x10]; }

            byte[] bytes = new byte[pVars.Count * 8];
            uint offset = 0;
            for (int i = 0; i < pVars.Count; i++)
            {
                WriteUint(ref bytes, (i * 8) + 0x00, offset);
                WriteUint(ref bytes, (i * 8) + 0x04, (uint)pVars[i].Length);
                offset += (uint)pVars[i].Length;
            }
            return bytes;
        }

        public byte[] GetPvars(List<byte[]> pVars)
        {
            if (pVars == null) { return new byte[0x10]; }

            var bytes = new byte[pVars.Sum(arr => arr.Length)];
            int index = 0;
            foreach (var pVar in pVars)
            {
                pVar.CopyTo(bytes, index);
                index += pVar.Length;
            }

            return bytes;
        }

        public byte[] GetOcclusionData(OcclusionData occlusionData)
        {
            if (occlusionData == null) { return new byte[0x10]; }

            return occlusionData.ToByteArray();
        }

    }
}
