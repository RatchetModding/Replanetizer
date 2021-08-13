using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;
using static LibReplanetizer.DataFunctions;


namespace LibReplanetizer.LevelObjects
{
    public class Tie : ModelObject
    {
        const int ELEMENTSIZE = 0x70;

        public short off_50 { get; set; }
        public uint off_54 { get; set; }
        public uint off_58 { get; set; }
        public uint off_5C { get; set; }

        public uint off_64 { get; set; }
        public uint off_68 { get; set; }
        public uint off_6C { get; set; }

        public byte[] colorBytes;

        public Tie(Matrix4 matrix4)
        {
            modelMatrix = matrix4;
        }

        public Tie(byte[] levelBlock, int num, List<Model> tieModels, FileStream fs)
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

            int colorOffset = ReadInt(levelBlock, offset + 0x60);
            off_64 = ReadUint(levelBlock, offset + 0x64);
            off_68 = ReadUint(levelBlock, offset + 0x68);
            off_6C = ReadUint(levelBlock, offset + 0x6C);

            model = tieModels.Find(tieModel => tieModel.id == modelID);
            colorBytes = ReadBlock(fs, colorOffset, (model.vertexBuffer.Length / 8) * 4);

            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();

            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 attributes = scaleMatrix * rot * translationMatrix;
            try
            {
                attributes.Invert();
            }
            catch
            {
                attributes = Matrix4.Identity;
            }

            reflection = modelMatrix * attributes;
        }

        public byte[] ToByteArray(int colorOffset)
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, modelMatrix);

            WriteShort(bytes, 0x50, off_50);
            WriteShort(bytes, 0x52, (short)modelID);
            WriteUint(bytes, 0x54, off_54);
            WriteUint(bytes, 0x58, off_58);
            WriteUint(bytes, 0x5C, off_5C);

            WriteInt(bytes, 0x60, colorOffset);
            WriteUint(bytes, 0x64, off_64);
            WriteUint(bytes, 0x68, off_68);
            WriteUint(bytes, 0x6C, off_6C);

            return bytes;
        }

        public override byte[] ToByteArray()
        {
            var bytes = new byte[ELEMENTSIZE];

            return bytes;
        }

        public override LevelObject Clone()
        {
            return new Tie(modelMatrix);
        }
    }
}
