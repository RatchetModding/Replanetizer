using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;
using static RatchetEdit.DataFunctions;


namespace RatchetEdit
{
    public class Shrub : ModelObject
    {
        const int ELEMENTSIZE = 0x70;

        public short off_50 { get; set; }
        public uint off_54 { get; set; }
        public uint off_58 { get; set; }
        public uint off_5C { get; set; }

        public int colors { get; set; }
        public uint off_64 { get; set; }
        public uint off_68 { get; set; }
        public uint off_6C { get; set; }

        public byte[] colorBytes;

        float rotationMultiplier = 2.2f;

        public override Vector3 position
        {
            get { return _position; }
            set
            {
                Translate(value - _position);
            }
        }
        public override Vector3 rotation
        {
            get { return _rotation; }
            set
            {
                Rotate(_rotation - value);
            }
        }

        public override Vector3 scale
        {
            get { return _scale; }
            set
            {
                Scale(Vector3.Divide(value, _scale));
            }
        }

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

            _rotation = modelMatrix.ExtractRotation().Xyz * rotationMultiplier;
            _position = modelMatrix.ExtractTranslation();
            _scale = modelMatrix.ExtractScale();
        }

        public override byte[] ToByteArray()
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(ref bytes, 0x00, modelMatrix);

            WriteShort(ref bytes, 0x50, off_50);
            WriteShort(ref bytes, 0x52, (short)modelID);
            WriteUint(ref bytes, 0x54, off_54);
            WriteUint(ref bytes, 0x58, off_58);
            WriteUint(ref bytes, 0x5C, off_5C);

            WriteInt(ref bytes, 0x60, colors);
            WriteUint(ref bytes, 0x64, off_64);
            WriteUint(ref bytes, 0x68, off_68);
            WriteUint(ref bytes, 0x6C, off_6C);

            return bytes;
        }

        public override LevelObject Clone()
        {
            return new Tie(modelMatrix);
        }

        public override void Translate(Vector3 vector)
        {
            modelMatrix = Utilities.TranslateMatrixTo(modelMatrix, vector + position);
            _position = modelMatrix.ExtractTranslation();
        }

        public override void Rotate(Vector3 vector)
        {
            Vector3 newRotation = vector + _rotation;
            modelMatrix = Utilities.RotateMatrixTo(modelMatrix, vector + rotation);
            _rotation = newRotation;
        }

        public override void Scale(Vector3 vector)
        {
            modelMatrix = Utilities.ScaleMatrixTo(modelMatrix, vector * scale);
            _scale = modelMatrix.ExtractScale();
        }
    }
}
