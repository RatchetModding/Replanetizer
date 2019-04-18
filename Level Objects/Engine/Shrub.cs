using System.Collections.Generic;
using OpenTK;
using RatchetEdit.Models;
using static RatchetEdit.DataFunctions;


namespace RatchetEdit.LevelObjects
{
    public class Shrub : ModelObject
    {
        public const int ELEMENTSIZE = 0x70;

        public short off_50 { get; set; }
        public uint off_54 { get; set; }
        public uint off_58 { get; set; }
        public uint off_5C { get; set; }

        public int colors { get; set; }
        public uint off_64 { get; set; }
        public uint off_68 { get; set; }
        public uint off_6C { get; set; }

        public byte[] colorBytes;


        public Shrub(Matrix4 matrix4)
        {
            modelMatrix = Matrix4.Add(matrix4, new Matrix4());
        }

        public Shrub(byte[] levelBlock, int num, List<Model> shrubModels)
        {
            int offset = num * ELEMENTSIZE;

            modelMatrix = ReadMatrix4(levelBlock, offset + 0x00);

            /* These offsets are just placeholders for the render distance quaternion which is set in-game
            off_40 =    BAToUInt32(levelBlock, offset + 0x40);
            off_44 =    BAToUInt32(levelBlock, offset + 0x44);
            off_48 =    BAToUInt32(levelBlock, offset + 0x48);
            off_4C =    BAToUInt32(levelBlock, offset + 0x4C);
            */

            off_50 = ReadShort(levelBlock, offset + 0x50);
            modelID = ReadUshort(levelBlock, offset + 0x52);
            off_54 = ReadUint(levelBlock, offset + 0x54);
            off_58 = ReadUint(levelBlock, offset + 0x58);
            off_5C = ReadUint(levelBlock, offset + 0x5C);

            colors = ReadInt(levelBlock, offset + 0x60);
            off_64 = ReadUint(levelBlock, offset + 0x64);
            off_68 = ReadUint(levelBlock, offset + 0x68);
            off_6C = ReadUint(levelBlock, offset + 0x6C);

            model = shrubModels.Find(shrubModel => shrubModel.id == modelID);

            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();
        }

        public override byte[] ToByteArray()
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, modelMatrix);

            WriteShort(bytes, 0x50, off_50);
            WriteShort(bytes, 0x52, (short)modelID);
            WriteUint(bytes, 0x54, off_54);
            WriteUint(bytes, 0x58, off_58);
            WriteUint(bytes, 0x5C, off_5C);

            WriteInt(bytes, 0x60, colors);
            WriteUint(bytes, 0x64, off_64);
            WriteUint(bytes, 0x68, off_68);
            WriteUint(bytes, 0x6C, off_6C);

            return bytes;
        }

        public override LevelObject Clone()
        {
            return new Tie(modelMatrix);
        }
    }
}
