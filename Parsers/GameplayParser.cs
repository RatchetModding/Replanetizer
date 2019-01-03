using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit {
    class GameplayParser {
        FileStream gameplayFileStream;
        GameplayHeader gameplayHeader;

        public GameplayParser(string gameplayFilepath) {
            try {
                gameplayFileStream = File.OpenRead(gameplayFilepath);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                MessageBox.Show("gameplay file missing!", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }
            gameplayHeader = new GameplayHeader(gameplayFileStream);
        }



        public List<Spline> GetSplines() {
            List<Spline> splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer, 4), 0);
            int splineOffset = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 8, 4), 0);

            byte[] splineHeadBlock = ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 0x10, splineCount * 4);
            byte[] splineBlock = ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + splineOffset, splineSectionSize);

            for (int i = 0; i < splineCount; i++) {
                int offset = ReadInt(splineHeadBlock, (i * 4));
                splines.Add(new Spline(splineBlock, offset));
            }
            return splines;
        }

        public LevelVariables GetLevelVariables()
        {
            byte[] levelVarBlock = ReadBlock(gameplayFileStream, gameplayHeader.levelVarPointer, 0x50);

            LevelVariables levelVariables = new LevelVariables(levelVarBlock);
            return levelVariables;
        }



        public byte[] getLang(int offset)
        {
            int langLength = ReadInt(ReadBlock(gameplayFileStream, offset + 4, 4), 0);
            return ReadBlock(gameplayFileStream, offset, langLength);
        }

        public byte[] getEnglish()
        {
            return getLang(gameplayHeader.englishPointer);
        }

        public byte[] getLang2()
        {
            return getLang(gameplayHeader.lang2Pointer);
        }

        public byte[] getFrench()
        {
            return getLang(gameplayHeader.frenchPointer);
        }

        public byte[] getGerman()
        {
            return getLang(gameplayHeader.germanPointer);
        }

        public byte[] getSpanish()
        {
            return getLang(gameplayHeader.spanishPointer);
        }

        public byte[] getItalian()
        {
            return getLang(gameplayHeader.italianPointer);
        }

        public byte[] getLang7()
        {
            return getLang(gameplayHeader.lang7Pointer);
        }

        public byte[] getLang8()
        {
            return getLang(gameplayHeader.lang8Pointer);
        }


        //TODO consolidate all these into a single function, as they work pretty much the same
        public List<Moby> GetMobies(List<MobyModel> mobyModels)
        {
            List<Moby> mobs = new List<Moby>();
            int mobyCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.mobyPointer, 4), 0);
            byte[] mobyBlock = ReadBlock(gameplayFileStream, gameplayHeader.mobyPointer + 0x10, mobyCount * 0x78);
            for (int i = 0; i < mobyCount * 0x78; i += 0x78)
            {
                Moby moby = new Moby(mobyBlock, i, mobyModels);
                mobs.Add(moby);
            }
            return mobs;
        }


        public List<SpawnPoint> GetSpawnPoints()
        {
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
            int spawnPointCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.spawnPointPointer, 4), 0);
            byte[] spawnPointBlock = ReadBlock(gameplayFileStream, gameplayHeader.spawnPointPointer + 0x10, spawnPointCount * 0x80);
            for (int i = 0; i < spawnPointCount; i++)
            {
                spawnPoints.Add(new SpawnPoint(spawnPointBlock, i));
            }
            return spawnPoints;
        }

        public List<GameCamera> GetGameCameras()
        {
            int cameraCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.cameraPointer, 4), 0);

            byte[] cameraBlock = ReadBlock(gameplayFileStream, gameplayHeader.cameraPointer + 0x10, cameraCount * GameCamera.ELEMSIZE);

            List<GameCamera> cameraList = new List<GameCamera>();
            for (int i = 0; i < cameraCount; i++)
            {
                cameraList.Add(new GameCamera(cameraBlock, i));
            }

            return cameraList;
        }

        public List<Type04> getType04()
        {
            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type04Pointer, 4), 0);

            byte[] type04Block = ReadBlock(gameplayFileStream, gameplayHeader.type04Pointer + 0x10, Type04.ELEMENTSIZE * count);

            List<Type04> type04s = new List<Type04>();
            for (int i = 0; i < count; i++)
            {
                type04s.Add(new Type04(type04Block, i));
            }

            return type04s;
        }

        public List<Type0C> getType0C()
        {
            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type0CPointer, 4), 0);

            byte[] type0CBlock = ReadBlock(gameplayFileStream, gameplayHeader.type0CPointer + 0x10, Type0C.TYPE0CELEMSIZE * count);

            List<Type0C> type0Cs = new List<Type0C>();
            for(int i = 0; i < count; i++)
            {
                type0Cs.Add(new Type0C(type0CBlock, i));
            }

            return type0Cs;
        }

        public List<Type64> GetType64s()
        {
            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type64Pointer, 4), 0);

            byte[] type64Block = ReadBlock(gameplayFileStream, gameplayHeader.type64Pointer + 0x10, Type64.ELEMENTSIZE * count);

            List<Type64> type64s = new List<Type64>();
            for (int i = 0; i < count; i++)
            {
                type64s.Add(new Type64(type64Block, i));
            }

            return type64s;
        }

        public List<Type68> getType68()
        {
            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type68Pointer, 4), 0);

            byte[] type68Block = ReadBlock(gameplayFileStream, gameplayHeader.type68Pointer + 0x10, Type68.TYPE68ELEMSIZE * count);

            List<Type68> type68s = new List<Type68>();
            for (int i = 0; i < count; i++)
            {
                type68s.Add(new Type68(type68Block, i));
            }

            return type68s;
        }

        public List<Type88> getType88()
        {
            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type88Pointer, 4), 0);

            byte[] type88Block = ReadBlock(gameplayFileStream, gameplayHeader.type88Pointer + 0x10, Type88.TYPE88ELEMSIZE * count);

            List<Type88> type88s = new List<Type88>();
            for (int i = 0; i < count; i++)
            {
                type88s.Add(new Type88(type88Block, i));
            }

            return type88s;
        }




        public List<Type80> getType80()
        {
            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type80Pointer, 4), 0);

            byte[] headBlock = ReadBlock(gameplayFileStream, gameplayHeader.type80Pointer + 0x10, Type80.HEADSIZE * count);
            byte[] dataBlock = ReadBlock(gameplayFileStream, gameplayHeader.type80Pointer + 0x10 + Type80.HEADSIZE * count, Type80.DATASIZE * count);

            List<Type80> type80s = new List<Type80>();
            for (int i = 0; i < count; i++)
            {
                type80s.Add(new Type80(headBlock, dataBlock, i));
            }

            return type80s;
        }

        public byte[] getUnk6()
        {
            int count1 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6 + 0x00, 4), 0);
            int count2 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6 + 0x04, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6, count1 * 4 + count2 + 0x10);
        }

        public byte[] getUnk7()
        {
            int count1 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer7, 4), 0);
            int count2 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer7 + 4, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer7, count1 + count2 * 8 + 0x10);
        }

        public byte[] getUnk9()
        {
            int length = gameplayHeader.unkPointer6 - gameplayHeader.unkPointer9;
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer9, length);
        }

        public byte[] getUnk17()
        {
            int sectionLength = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer17, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer17, sectionLength + 0x10);
        }

        public List<int> getMobyIds()
        {
            int count = BitConverter.ToInt32(ReadBlock(gameplayFileStream, gameplayHeader.mobyIdPointer, 4), 0);

            byte[] mobyIdBlock = ReadBlock(gameplayFileStream, gameplayHeader.mobyIdPointer + 0x04, count * 0x04);

            List<int> mobyIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                mobyIds.Add(BitConverter.ToInt32(mobyIdBlock, (i * 0x04)));
            }

            return mobyIds;
        }

        public List<int> getTieIds()
        {
            int count = BitConverter.ToInt32(ReadBlock(gameplayFileStream, gameplayHeader.tieIdPointer, 4), 0);

            byte[] tieIdBlock = ReadBlock(gameplayFileStream, gameplayHeader.tieIdPointer + 0x04, count * 0x04);

            List<int> tieIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                tieIds.Add(BitConverter.ToInt32(tieIdBlock, (i * 0x04)));
            }

            return tieIds;
        }

        public List<int> getShrubIds()
        {
            int count = BitConverter.ToInt32(ReadBlock(gameplayFileStream, gameplayHeader.shrubIdPointer, 4), 0);

            byte[] shrubIdBlock = ReadBlock(gameplayFileStream, gameplayHeader.shrubIdPointer + 0x04, count * 0x04);

            List<int> shrubIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                shrubIds.Add(BitConverter.ToInt32(shrubIdBlock, (i * 0x04)));
            }

            return shrubIds;
        }

        public OcclusionData getOcclusionData()
        {
            byte[] headBlock = ReadBlock(gameplayFileStream, gameplayHeader.occlusionPointer, 0x10);
            OcclusionDataHeader head = new OcclusionDataHeader(headBlock);

            byte[] occlusionBlock = ReadBlock(gameplayFileStream, gameplayHeader.occlusionPointer + 0x10, head.totalCount * 0x08);
            OcclusionData data = new OcclusionData(occlusionBlock, head);


            return data;
        }

        public byte[] getTieData(int tieCount)
        {
            return ReadBlock(gameplayFileStream, gameplayHeader.tiePointer, 0xE0 * tieCount);
        }

        public byte[] getShrubData(int shrubCount)
        {
            return ReadBlock(gameplayFileStream, gameplayHeader.tiePointer, 0x70 * shrubCount);
        }



        public List<byte[]> getPvars()
        {
            List<byte[]> pVars = new List<byte[]>();
            int numpVars = (int)(gameplayHeader.pvarPointer - gameplayHeader.pvarSizePointer) / 8;

            byte[] pVarHeadBlock = ReadBlock(gameplayFileStream, gameplayHeader.pvarSizePointer, numpVars * 8);
            uint pVarSectionLength = 0;
            for (int i = 0; i < numpVars; i++)
            {
                pVarSectionLength += ReadUint(pVarHeadBlock, (i * 8) + 0x04);
            }

            byte[] pVarBlock = ReadBlock(gameplayFileStream, gameplayHeader.pvarPointer, (int)pVarSectionLength);
            for (int i = 0; i < numpVars; i++)
            {
                uint mobpVarsStart = ReadUint(pVarHeadBlock, (i * 8));
                uint mobpVarsCount = ReadUint(pVarHeadBlock, (i * 8) + 0x04);
                byte[] mobpVars = getBytes(pVarBlock, (int)mobpVarsStart, (int)mobpVarsCount);
                pVars.Add(mobpVars);
            }

            return pVars;
        }


        public void Close() {
            gameplayFileStream.Close();
        }
    }
}
