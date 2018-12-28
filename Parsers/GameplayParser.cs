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

        public List<Moby> GetMobies(List<Model> mobyModels) {
            List<Moby> mobs = new List<Moby>();
            int mobyCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.mobyPointer, 4), 0);
            byte[] mobyBlock = ReadBlock(gameplayFileStream, gameplayHeader.mobyPointer + 0x10, mobyCount * 0x78);
            for (int i = 0; i < mobyCount * 0x78; i += 0x78) {
                Moby moby = new Moby(mobyBlock, i, mobyModels);
                mobs.Add(moby);
            }
            return mobs;
        }

        public List<Spline> GetSplines() {
            List<Spline> splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer, 4), 0);
            uint splineOffset = ReadUint(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 8, 4), 0);

            byte[] splineHeadBlock = ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + 0x10, splineCount * 4);
            byte[] splineBlock = ReadBlock(gameplayFileStream, gameplayHeader.splinePointer + splineOffset, splineSectionSize);

            for (int i = 0; i < splineCount; i++) {
                int offset = ReadInt(splineHeadBlock, (i * 4));
                splines.Add(new Spline(splineBlock, offset));
            }
            return splines;
        }

        public List<SpawnPoint> GetSpawnPoints()
        {
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
            int spawnPointCount = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.spawnPointPointer, 4), 0);
            byte[] spawnPointBlock = ReadBlock(gameplayFileStream, gameplayHeader.spawnPointPointer + 0x10, spawnPointCount * 0x80);
            for(int i = 0; i < spawnPointCount; i++)
            {
                spawnPoints.Add(new SpawnPoint(spawnPointBlock, i));
            }
            return spawnPoints;
        }

        public LevelVariables GetLevelVariables()
        {
            byte[] levelVarBlock = ReadBlock(gameplayFileStream, gameplayHeader.levelVarPointer, 0x50);

            LevelVariables levelVariables = new LevelVariables(levelVarBlock);
            return levelVariables;
        }

        public byte[] getLang(uint offset)
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

        public byte[] getUnk6()
        {
            int count1 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6, 4), 0);
            int count2 = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer6 + 4, 4), 0);
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
            int length = (int)(gameplayHeader.unkPointer6 - gameplayHeader.unkPointer9);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer9, length);
        }

        public byte[] getUnk17()
        {
            int sectionLength = ReadInt(ReadBlock(gameplayFileStream, gameplayHeader.unkPointer17, 4), 0);
            return ReadBlock(gameplayFileStream, gameplayHeader.unkPointer17, sectionLength + 0x10);
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
