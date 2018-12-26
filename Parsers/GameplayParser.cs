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
        }

        public List<Moby> GetMobies(List<Model> mobyModels) {
            uint mobyPointer = ReadUint(ReadBlock(gameplayFileStream, 0x44, 4), 0);
            List<Moby> mobs = new List<Moby>();
            int mobyCount = ReadInt(ReadBlock(gameplayFileStream, mobyPointer, 4), 0);
            byte[] mobyBlock = ReadBlock(gameplayFileStream, mobyPointer + 0x10, mobyCount * 0x78);
            for (int i = 0; i < mobyCount * 0x78; i += 0x78) {
                Moby moby = new Moby(mobyBlock, i, mobyModels);
                mobs.Add(moby);
            }
            return mobs;
        }

        public List<Spline> GetSplines() {
            uint splinePointer = ReadUint(ReadBlock(gameplayFileStream, 0x70, 4), 0);
            List<Spline> splines = new List<Spline>();
            int splineCount = ReadInt(ReadBlock(gameplayFileStream, splinePointer, 4), 0);
            uint splineOffset = ReadUint(ReadBlock(gameplayFileStream, splinePointer + 4, 4), 0);
            int splineSectionSize = ReadInt(ReadBlock(gameplayFileStream, splinePointer + 8, 4), 0);
            byte[] splineHeadBlock = ReadBlock(gameplayFileStream, splinePointer + 0x10, splineCount * 4);
            byte[] splineBlock = ReadBlock(gameplayFileStream, splinePointer + splineOffset, splineSectionSize);
            for (int i = 0; i < splineCount; i++) {
                int offset = ReadInt(splineHeadBlock, (i * 4));
                splines.Add(new Spline(splineBlock, offset));
            }
            return splines;
        }

        public void Close() {
            gameplayFileStream.Close();
        }
    }
}
