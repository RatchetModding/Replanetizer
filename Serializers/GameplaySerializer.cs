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
            byte[] spawnPoints = GetSpawnPoints(level.spawnPoints);
            byte[] gameCameras = GetGameCameras(level.gameCameras);

            //Seek past the header
            fs.Seek(0xB0, SeekOrigin.Begin);

            gameplayHeader.levelVarPointer = (int)fs.Position;
            fs.Write(levelVariables, 0, levelVariables.Length);

            seekPast(fs);
            gameplayHeader.englishPointer = (int)fs.Position;
            fs.Write(level.english, 0, level.english.Length);

            seekPast(fs);
            gameplayHeader.lang2Pointer = (int)fs.Position;
            fs.Write(level.lang2, 0, level.lang2.Length);

            seekPast(fs);
            gameplayHeader.frenchPointer = (int)fs.Position;
            fs.Write(level.french, 0, level.french.Length);

            seekPast(fs);
            gameplayHeader.germanPointer = (int)fs.Position;
            fs.Write(level.german, 0, level.german.Length);

            seekPast(fs);
            gameplayHeader.spanishPointer = (int)fs.Position;
            fs.Write(level.spanish, 0, level.spanish.Length);

            seekPast(fs);
            gameplayHeader.italianPointer = (int)fs.Position;
            fs.Write(level.italian, 0, level.italian.Length);




            seekPast(fs);
            gameplayHeader.cameraPointer = (int)fs.Position;
            fs.Write(gameCameras, 0, gameCameras.Length);

            seekPast(fs);
            gameplayHeader.mobyPointer = (int)fs.Position;
            fs.Write(mobs, 0, mobs.Length);

            seekPast(fs);
            gameplayHeader.pvarSizePointer = (int)fs.Position;
            fs.Write(pVarSizes, 0, pVarSizes.Length);

            seekPast(fs);
            gameplayHeader.pvarPointer = (int)fs.Position;
            fs.Write(pVars, 0, pVars.Length);

            seekPast(fs);
            gameplayHeader.unkPointer9 = (int)fs.Position;
            fs.Write(level.unk9, 0, level.unk9.Length);

            seekPast(fs);
            gameplayHeader.unkPointer6 = (int)fs.Position;
            fs.Write(level.unk6, 0, level.unk6.Length);

            seekPast(fs);
            gameplayHeader.unkPointer7 = (int)fs.Position;
            fs.Write(level.unk7, 0, level.unk7.Length);

            seekPast(fs);
            gameplayHeader.splinePointer = (int)fs.Position;
            fs.Write(splines, 0, splines.Length);

            seekPast(fs);
            gameplayHeader.spawnPointPointer = (int)fs.Position;
            fs.Write(spawnPoints, 0, spawnPoints.Length);

            seekPast(fs);
            gameplayHeader.unkPointer17 = (int)fs.Position;
            fs.Write(level.unk17, 0, level.unk17.Length);



            //Seek to the beginning of the file to append the updated header
            byte[] head = gameplayHeader.serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
        }

        private void seekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        public byte[] GetMobies(List<Moby> mobs)
        {
            byte[] bytes = new byte[MOBYLENGTH * mobs.Count + 0x10];

            //Header
            WriteUint(ref bytes, 0, (uint)mobs.Count);
            WriteUint(ref bytes, 4, 0x100);

            int index = 0;
            foreach(Moby moby in mobs)
            {
                byte[] mobyBytes = moby.serialize();
                Array.Copy(mobyBytes, 0, bytes, index * MOBYLENGTH + 0x10, MOBYLENGTH);
                index++;
            }            
            return bytes;
        }


        public byte[] GetSpawnPoints(List<SpawnPoint> spawnPoints)
        {
            byte[] bytes = new byte[0x80 * spawnPoints.Count + 0x10];

            //Header
            WriteUint(ref bytes, 0, (uint)spawnPoints.Count);

            for(int i = 0; i < spawnPoints.Count; i++)
            {
                byte[] spawnPointBytes = spawnPoints[i].serialize();
                spawnPointBytes.CopyTo(bytes, 0x10 + i * 0x80);
            }
            return bytes;
        }

        public byte[] GetGameCameras(List<GameCamera> gameCameras)
        {
            byte[] bytes = new byte[0x10 + gameCameras.Count * GameCamera.ELEMSIZE];

            //Header
            WriteUint(ref bytes, 0, (uint)gameCameras.Count);

            for(int i = 0; i < gameCameras.Count; i++)
            {
                byte[] cameraBytes = gameCameras[i].serialize();
                cameraBytes.CopyTo(bytes, 0x10 + i * GameCamera.ELEMSIZE);
            }

            return bytes;
        }

        public byte[] GetSplines(List<Spline> splines)
        {
            List<byte[]> splineData = new List<byte[]>();
            List<int> offsets = new List<int>();
            int offset = 0;
            foreach(Spline spline in splines)
            {
                byte[] splineBytes = spline.serialize();
                splineData.Add(splineBytes);
                offsets.Add(offset);
                offset += splineBytes.Length;
            }

            int offsetBlockLength = offsets.Count * 4;
            while(offsetBlockLength % 0x10 != 0)
            {
                offsetBlockLength += 0x4;
            }
            byte[] offsetBlock = new byte[offsetBlockLength];
            for(int i = 0; i < offsets.Count; i++)
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
            byte[] bytes = new byte[pVars.Count * 8];
            uint offset = 0;
            for(int i = 0; i < pVars.Count; i++)
            {
                WriteUint(ref bytes, (i * 8) + 0x00, offset);
                WriteUint(ref bytes, (i * 8) + 0x04, (uint)pVars[i].Length);
                offset += (uint)pVars[i].Length;
            }
            return bytes;
        }

        public byte[] GetPvars(List<byte[]> pVars)
        {
            var bytes = new byte[pVars.Sum(arr => arr.Length)];
            int index = 0;
            foreach (var pVar in pVars)
            {
                pVar.CopyTo(bytes, index);
                index += pVar.Length;
            }

            return bytes;
        }

    }
}
