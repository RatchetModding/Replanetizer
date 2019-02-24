using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    class GameplayParser
    {
        FileStream gameplayFileStream;
        GameplayHeader gameplayHeader;

        public GameplayParser(GameType game, string gameplayFilepath)
        {
            try
            {
                gameplayFileStream = File.OpenRead(gameplayFilepath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("gameplay file missing!", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }
            gameplayHeader = new GameplayHeader(game, gameplayFileStream);
        }

        public List<Spline> GetSplines()
        {
            if (gameplayHeader.splinePointer == 0) { return null; }

            List<Spline> splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer, 4), 0);
            int splineOffset = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 8, 4), 0);

            byte[] splineHeadBlock = ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 0x10, splineCount * 4);
            byte[] splineBlock = ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + splineOffset, splineSectionSize);

            for (int i = 0; i < splineCount; i++)
            {
                int offset = ReadInt(splineHeadBlock, (i * 4));
                splines.Add(new Spline(splineBlock, offset));
            }
            return splines;
        }

        public LevelVariables GetLevelVariables()
        {
            if (gameplayHeader.levelVarPointer == 0) { return null; }

            byte[] levelVarBlock = ReadBlock(gameplayFileStream, gameplayHeader.levelVarPointer, 0x50);

            LevelVariables levelVariables = new LevelVariables(levelVarBlock);
            return levelVariables;
        }



        public byte[] GetLang(int offset)
        {
            if (offset == 0) { return null; }
            int langLength = ReadInt(ReadBlock(gameplayFileStream, offset + 4, 4), 0);
            return ReadBlock(gameplayFileStream, offset, langLength);
        }

        public byte[] GetEnglish()
        {
            return GetLang(gameplayHeader.englishPointer);
        }

        public byte[] GetLang2()
        {
            return GetLang(gameplayHeader.lang2Pointer);
        }

        public byte[] GetFrench()
        {
            return GetLang(gameplayHeader.frenchPointer);
        }

        public byte[] GetGerman()
        {
            return GetLang(gameplayHeader.germanPointer);
        }

        public byte[] GetSpanish()
        {
            return GetLang(gameplayHeader.spanishPointer);
        }

        public byte[] GetItalian()
        {
            return GetLang(gameplayHeader.italianPointer);
        }

        public byte[] GetLang7()
        {
            return GetLang(gameplayHeader.lang7Pointer);
        }

        public byte[] GetLang8()
        {
            return GetLang(gameplayHeader.lang8Pointer);
        }


        //TODO consolidate all these into a single function, as they work pretty much the same
        public List<Moby> GetMobies(GameType game, List<Model> mobyModels)
        {
            List<Moby> mobs = new List<Moby>();

            if (gameplayHeader.mobyPointer == 0) { return mobs; }

            int mobyCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.mobyPointer, 4), 0);

            byte[] mobyBlock = ReadBlock(gameplayFileStream, gameplayHeader.mobyPointer + 0x10, mobyCount * game.mobyElemSize);
            for (int i = 0; i < mobyCount; i++)
            {
                mobs.Add(new Moby(game, mobyBlock, i, mobyModels));
            }
            return mobs;
        }


        public List<Cuboid> GetSpawnPoints()
        {
            List<Cuboid> spawnPoints = new List<Cuboid>();

            if (gameplayHeader.spawnPointPointer == 0) { return spawnPoints; }

            int spawnPointCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.spawnPointPointer, 4), 0);
            byte[] spawnPointBlock = ReadBlock(gameplayFileStream, gameplayHeader.spawnPointPointer + 0x10, spawnPointCount * Cuboid.ELEMENTSIZE);
            for (int i = 0; i < spawnPointCount; i++)
            {
                spawnPoints.Add(new Cuboid(spawnPointBlock, i));
            }
            return spawnPoints;
        }

        public List<GameCamera> GetGameCameras()
        {
            List<GameCamera> cameraList = new List<GameCamera>();
            if (gameplayHeader.cameraPointer == 0) { return cameraList; }

            int cameraCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.cameraPointer, 4), 0);
            byte[] cameraBlock = ReadBlock(gameplayFileStream, gameplayHeader.cameraPointer + 0x10, cameraCount * GameCamera.ELEMENTSIZE);
            for (int i = 0; i < cameraCount; i++)
            {
                cameraList.Add(new GameCamera(cameraBlock, i));
            }

            return cameraList;
        }

        public List<Type04> GetType04s()
        {
            List<Type04> type04s = new List<Type04>();
            if (gameplayHeader.type04Pointer == 0) { return type04s; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type04Pointer, 4), 0);
            byte[] type04Block = ReadBlock(gameplayFileStream, gameplayHeader.type04Pointer + 0x10, Type04.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type04s.Add(new Type04(type04Block, i));
            }

            return type04s;
        }

        public List<Type0C> GetType0Cs()
        {
            List<Type0C> type0Cs = new List<Type0C>();
            if (gameplayHeader.type0CPointer == 0) { return type0Cs; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type0CPointer, 4), 0);
            byte[] type0CBlock = ReadBlock(gameplayFileStream, gameplayHeader.type0CPointer + 0x10, Type0C.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type0Cs.Add(new Type0C(type0CBlock, i));
            }

            return type0Cs;
        }

        public List<Type64> GetType64s()
        {
            List<Type64> type64s = new List<Type64>();
            if (gameplayHeader.type64Pointer == 0) { return type64s; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type64Pointer, 4), 0);
            byte[] type64Block = ReadBlock(gameplayFileStream, gameplayHeader.type64Pointer + 0x10, Type64.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type64s.Add(new Type64(type64Block, i));
            }

            return type64s;
        }

        public List<Type68> GetType68s()
        {
            List<Type68> type68s = new List<Type68>();
            if (gameplayHeader.type68Pointer == 0) { return type68s; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type68Pointer, 4), 0);
            byte[] type68Block = ReadBlock(gameplayFileStream, gameplayHeader.type68Pointer + 0x10, Type68.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type68s.Add(new Type68(type68Block, i));
            }

            return type68s;
        }

        public List<Type7C> GetType7Cs()
        {
            List<Type7C> type7Cs = new List<Type7C>();
            if (gameplayHeader.type7CPointer == 0) { return type7Cs; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type7CPointer, 4), 0);
            byte[] type7CBlock = ReadBlock(gameplayFileStream, gameplayHeader.type7CPointer + 0x10, Type7C.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type7Cs.Add(new Type7C(type7CBlock, i));
            }

            return type7Cs;
        }

        public List<Type88> GetType88s()
        {
            List<Type88> type88s = new List<Type88>();
            if (gameplayHeader.type88Pointer == 0) { return type88s; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type88Pointer, 4), 0);
            byte[] type88Block = ReadBlock(gameplayFileStream, gameplayHeader.type88Pointer + 0x10, Type88.ELEMENTSIZE * count);
            for (int i = 0; i < count; i++)
            {
                type88s.Add(new Type88(type88Block, i));
            }

            return type88s;
        }

        public List<Type80> GetType80()
        {
            List<Type80> type80s = new List<Type80>();
            if (gameplayHeader.type80Pointer == 0) { return type80s; }

            int count = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.type80Pointer, 4), 0);
            byte[] headBlock = ReadBlock(gameplayFileStream, gameplayHeader.type80Pointer + 0x10, Type80.HEADSIZE * count);
            byte[] dataBlock = ReadBlock(gameplayFileStream, gameplayHeader.type80Pointer + 0x10 + Type80.HEADSIZE * count, Type80.DATASIZE * count);

            for (int i = 0; i < count; i++)
            {
                type80s.Add(new Type80(headBlock, dataBlock, i));
            }

            return type80s;
        }

        public byte[] GetUnk6()
        {
            if (gameplayHeader.unkPointer6 == 0) { return null; }
            int count1 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6 + 0x00, 4), 0);
            int count2 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6 + 0x04, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6, count1 * 4 + count2 + 0x10);
        }

        public byte[] GetUnk7()
        {
            if (gameplayHeader.unkPointer7 == 0) { return null; }
            int count1 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer7, 4), 0);
            int count2 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer7 + 4, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer7, count1 + count2 * 8 + 0x10);
        }

        public List<KeyValuePair<int, int>> GetType5Cs()
        {
            List<KeyValuePair<int, int>> keyValuePairs = new List<KeyValuePair<int, int>>();
            byte[] bytes;
            for (int i = 0; (bytes = ReadBlock(gameplayFileStream, gameplayHeader.type5CPointer + i * 8, 8))[0] != 0xFF; i++)
            {
                int id = ReadInt(bytes, 0);
                int value = ReadInt(bytes, 4);
                keyValuePairs.Add(new KeyValuePair<int, int>(id, value));
            }
            return keyValuePairs;
        }


        public List<KeyValuePair<int, int>> GetType50s()
        {
            List<KeyValuePair<int, int>> keyValuePairs = new List<KeyValuePair<int, int>>();
            byte[] bytes;
            for (int i = 0; (bytes = ReadBlock(gameplayFileStream, gameplayHeader.type50Pointer + i * 8, 8))[0] != 0xFF; i++)
            {
                int id = ReadInt(bytes, 0);
                int value = ReadInt(bytes, 4);
                keyValuePairs.Add(new KeyValuePair<int, int>(id, value));
            }
            return keyValuePairs;
        }

        public byte[] GetUnk13()
        {
            int sectionLength = gameplayHeader.occlusionPointer - gameplayHeader.unkPointer13;
            if(sectionLength > 0)
            {
                return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer13, sectionLength);
            }
            else
            {
                return new byte[0x10];
            }

        }

        public byte[] GetUnk17()
        {
            if (gameplayHeader.unkPointer17 == 0) { return null; }
            int sectionLength = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer17, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer17, sectionLength);
        }

        public byte[] GetUnk14()
        {
            if (gameplayHeader.unkPointer14 == 0) { return null; }
            int sectionLength = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer14, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer14, sectionLength);
        }


        public List<int> GetMobyIds()
        {
            if (gameplayHeader.mobyIdPointer == 0) { return null; }

            int count = BitConverter.ToInt32(ReadBlock(gameplayFileStream, gameplayHeader.mobyIdPointer, 4), 0);

            byte[] mobyIdBlock = ReadBlock(gameplayFileStream, gameplayHeader.mobyIdPointer + 0x04, count * 0x04);

            List<int> mobyIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                mobyIds.Add(BitConverter.ToInt32(mobyIdBlock, (i * 0x04)));
            }

            return mobyIds;
        }

        public List<int> GetTieIds()
        {
            if (gameplayHeader.tieIdPointer == 0) { return null; }

            int count = BitConverter.ToInt32(ReadBlock(gameplayFileStream, gameplayHeader.tieIdPointer, 4), 0);

            byte[] tieIdBlock = ReadBlock(gameplayFileStream, gameplayHeader.tieIdPointer + 0x04, count * 0x04);

            List<int> tieIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                tieIds.Add(BitConverter.ToInt32(tieIdBlock, (i * 0x04)));
            }

            return tieIds;
        }

        public List<int> GetShrubIds()
        {
            if (gameplayHeader.shrubIdPointer == 0) { return null; }

            int count = BitConverter.ToInt32(ReadBlock(gameplayFileStream, gameplayHeader.shrubIdPointer, 4), 0);

            byte[] shrubIdBlock = ReadBlock(gameplayFileStream, gameplayHeader.shrubIdPointer + 0x04, count * 0x04);

            List<int> shrubIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                shrubIds.Add(BitConverter.ToInt32(shrubIdBlock, (i * 0x04)));
            }

            return shrubIds;
        }

        public OcclusionData GetOcclusionData()
        {
            if (gameplayHeader.occlusionPointer == 0) { return null; }

            byte[] headBlock = ReadBlock(gameplayFileStream, gameplayHeader.occlusionPointer, 0x10);
            OcclusionDataHeader head = new OcclusionDataHeader(headBlock);

            byte[] occlusionBlock = ReadBlock(gameplayFileStream, gameplayHeader.occlusionPointer + 0x10, head.totalCount * 0x08);
            OcclusionData data = new OcclusionData(occlusionBlock, head);


            return data;
        }

        public byte[] GetTieData(int tieCount)
        {
            return ReadBlock(gameplayFileStream, gameplayHeader.tiePointer, 0x10 + 0xE0 * tieCount);
        }

        public byte[] getShrubData(int shrubCount)
        {
            return ReadBlock(gameplayFileStream, gameplayHeader.shrubPointer, 0x10 + 0x70 * shrubCount);
        }



        public List<byte[]> GetPvars(List<Moby> mobs)
        {
            int pvarCount = 0;
            foreach (Moby mob in mobs)
            {
                if(mob.pvarIndex > pvarCount)
                {
                    pvarCount = mob.pvarIndex;
                }
            }

            pvarCount++;

            List<byte[]> pVars = new List<byte[]>();

            byte[] pVarHeadBlock = ReadBlock(gameplayFileStream, gameplayHeader.pvarSizePointer, pvarCount * 8);
            uint pVarSectionLength = 0;
            for (int i = 0; i < pvarCount; i++)
            {
                pVarSectionLength += ReadUint(pVarHeadBlock, (i * 8) + 0x04);
            }

            byte[] pVarBlock = ReadBlock(gameplayFileStream, gameplayHeader.pvarPointer, (int)pVarSectionLength);
            for (int i = 0; i < pvarCount; i++)
            {
                uint mobpVarsStart = ReadUint(pVarHeadBlock, (i * 8));
                uint mobpVarsCount = ReadUint(pVarHeadBlock, (i * 8) + 0x04);
                byte[] mobpVars = getBytes(pVarBlock, (int)mobpVarsStart, (int)mobpVarsCount);
                pVars.Add(mobpVars);
            }

            return pVars;
        }


        public void Close()
        {
            gameplayFileStream.Close();
        }
    }
}
