using System.Collections.Generic;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit.LevelObjects
{
    public class UiElement
    {
        public short id;
        public List<int> sprites;

        public UiElement(byte[] headBlock, int num, byte[] texBlock)
        {

            int offset = num * 8;
            id = ReadShort(headBlock, offset + 0x00);

            short spriteCount = ReadShort(headBlock, offset + 0x02);
            short spriteOffset = ReadShort(headBlock, offset + 0x04);

            sprites = new List<int>();
            for(int i = 0; i < spriteCount; i++)
            {
                sprites.Add(ReadInt(texBlock, (spriteOffset + i) * 4));
            }
        }

        /*public byte[] ToByteArray()
        {

        }*/
    }
}
