using OpenTK;
using System;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class GameCamera : LevelObject
    {
        public const int ELEMENTSIZE = 0x20;

        public int id;
        public int unk1;
        public int unk2;
        public int unk3;
        public int id2;

        public GameCamera(byte[] cameraBlock, int num)
        {
            int offset = num * ELEMENTSIZE;

            id = ReadInt(cameraBlock, offset + 0x00);
            float x = ReadFloat(cameraBlock, offset + 0x04);
            float y = ReadFloat(cameraBlock, offset + 0x08);
            float z = ReadFloat(cameraBlock, offset + 0x0C);
            unk1 = ReadInt(cameraBlock, offset + 0x10);
            unk2 = ReadInt(cameraBlock, offset + 0x14);
            unk3 = ReadInt(cameraBlock, offset + 0x18);
            id2 = ReadInt(cameraBlock, offset + 0x1C);

            position = new Vector3(x, y, z);
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteInt(bytes, 0x00, id);
            WriteFloat(bytes, 0x04, position.X);
            WriteFloat(bytes, 0x08, position.Y);
            WriteFloat(bytes, 0x0C, position.Z);
            WriteInt(bytes, 0x10, unk1);
            WriteInt(bytes, 0x14, unk2);
            WriteInt(bytes, 0x18, unk3);
            WriteInt(bytes, 0x1C, id2);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }


    }
}
