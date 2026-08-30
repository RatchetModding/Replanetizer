using System.IO;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Serializers.SerializerFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class PrecipitationMap
    {
        public const int HEADERSIZE = 0x10;
        private GameType game;

        public float unk00 { get; set; }
        public float unk04 { get; set; }

        public int rowStride { get; set; }
        public int numColumns { get; set; }
        public float lowerBound { get; set; }
        public float upperBound { get; set; }
        private byte[] rawRasterHeights = [];
        private float[] rasterHeigths = [];

        public PrecipitationMap(FileStream fs, GameType game, int offset)
        {
            this.game = game;
            if (game == GameType.RaC1)
            {
                GetRC1Vals(fs, offset);
            }
            else
            {
                GetRC23DLVals(fs, offset);
            }
        }

        private void GetRC1Vals(FileStream fs, int offset)
        {
            byte[] headBlock = ReadBlock(fs, offset, HEADERSIZE);

            rowStride = ReadInt(headBlock, 0x00);
            numColumns = ReadInt(headBlock, 0x04);
            lowerBound = ReadFloat(headBlock, 0x08);
            upperBound = ReadFloat(headBlock, 0x0C);

            rawRasterHeights = ReadBlock(fs, offset + HEADERSIZE, rowStride * numColumns);

            rasterHeigths = new float[rowStride * numColumns];
            for (int i = 0; i < rowStride * numColumns; i++)
            {
                float normalizedHeight = rawRasterHeights[i] / 255.0f;
                rasterHeigths[i] = lowerBound + (upperBound - lowerBound) * (1.0f - normalizedHeight);
            }
        }

        private void GetRC23DLVals(FileStream fs, int offset)
        {
            byte[] headBlock = ReadBlock(fs, offset, HEADERSIZE);

            unk00 = ReadFloat(headBlock, 0x00);
            unk04 = ReadFloat(headBlock, 0x04);
            lowerBound = ReadFloat(headBlock, 0x08);
            upperBound = ReadFloat(headBlock, 0x0C);

            rowStride = 256;
            numColumns = 256;

            rawRasterHeights = ReadBlock(fs, offset + HEADERSIZE, rowStride * numColumns + 0x30);

            rasterHeigths = new float[rowStride * numColumns];
            for (int i = 0; i < rowStride * numColumns; i++)
            {
                float normalizedHeight = rawRasterHeights[i] / 255.0f;
                rasterHeigths[i] = lowerBound + (upperBound - lowerBound) * (1.0f - normalizedHeight);
            }
        }

        public float GetHeight(int x, int y)
        {
            return rasterHeigths[rowStride * y + x];
        }

        public int WriteBytes(FileStream fs)
        {
            byte[] headerBytes = new byte[HEADERSIZE];

            if (game == GameType.RaC1)
            {
                WriteInt(headerBytes, 0x00, rowStride);
                WriteInt(headerBytes, 0x04, numColumns);
            }
            else
            {
                WriteFloat(headerBytes, 0x00, unk00);
                WriteFloat(headerBytes, 0x04, unk04);
            }

            WriteFloat(headerBytes, 0x08, lowerBound);
            WriteFloat(headerBytes, 0x0C, upperBound);

            int headerOffset = SeekWrite(fs, headerBytes, 0x10);

            SeekWrite(fs, rawRasterHeights, 0x10);

            return headerOffset;
        }
    }
}
