
namespace LibReplanetizer
{
    public class GameType
    {
        readonly int[] mobySizes = { 0x78, 0x88, 0x88, 0x70 };

        public int num;
        public int mobyElemSize;
        public int engineSize;

        public GameType(int gameNum)
        {
            num = gameNum;

            mobyElemSize = mobySizes[gameNum - 1];
        }
    }
}
