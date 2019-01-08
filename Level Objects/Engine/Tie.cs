using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;
using static RatchetEdit.DataFunctions;


namespace RatchetEdit
{
    public class Tie : ModelObject
    {
        const int ELEMENTSIZE = 0x70;

        public short off_50 { get; set; }
        public uint off_54 { get; set; }
        public uint off_58 { get; set; }
        public uint off_5C { get; set; }

        public int colorOffset { get; set; }
        public uint off_64 { get; set; }
        public uint off_68 { get; set; }
        public uint off_6C { get; set; }

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
                Rotate(value - _rotation);
            }
        }

        public override float scale
        {
            get { return _scale; }
            set
            {
                Scale(value - _scale);
            }
        }

        float rotationMultiplier = 2.2f;

        public Tie(Matrix4 matrix4)
        {
            modelMatrix = Matrix4.Add(matrix4, new Matrix4());
        }

        public Tie(byte[] levelBlock, int num, List<Model> tieModels)
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

            colorOffset = ReadInt(levelBlock, offset + 0x60);
            off_64 = ReadUint(levelBlock, offset + 0x64);
            off_68 = ReadUint(levelBlock, offset + 0x68);
            off_6C = ReadUint(levelBlock, offset + 0x6C);

            model = tieModels.Find(tieModel => tieModel.id == modelID);
            _rotation = modelMatrix.ExtractRotation().Xyz * rotationMultiplier;
            _position = modelMatrix.ExtractTranslation();
            _scale = modelMatrix.ExtractScale().Length / 1.7f;
        }

        public byte[] Serialize()
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(ref bytes, 0x00, modelMatrix);

            WriteShort(ref bytes, 0x50, off_50);
            WriteShort(ref bytes, 0x52, (short)modelID);
            WriteUint(ref bytes, 0x54, off_54);
            WriteUint(ref bytes, 0x58, off_58);
            WriteUint(ref bytes, 0x5C, off_5C);

            WriteInt(ref bytes, 0x60, colorOffset);
            WriteUint(ref bytes, 0x64, off_64);
            WriteUint(ref bytes, 0x68, off_68);
            WriteUint(ref bytes, 0x6C, off_6C);

            return bytes;
        }

        public override void UpdateTransformMatrix()
        {
        }

        public override LevelObject Clone()
        {
            return new Tie(modelMatrix);
        }

        void UpdateMatrixVariables(Matrix4 matrix)
        {
            modelMatrix = matrix;
        }

        //Transformable methods
        public override void Translate(float x, float y, float z)
        {
            Vector3 rot = new Vector3(rotation);
            Rotate(-rot); //Rotate to 0,0,0 to do translation in world space.

            Matrix4 translationMatrix = Matrix4.CreateTranslation(x, y, z);
            Matrix4 result = translationMatrix * modelMatrix;
            _position = result.ExtractTranslation();
            UpdateMatrixVariables(result);
            UpdateTransformMatrix();

            Rotate(rot); //Rotate back to keep orientation

        }

        public override void Translate(Vector3 vector)
        {
            Translate(vector.X, vector.Y, vector.Z);
        }

        public override void Rotate(float x, float y, float z)
        {
            Vector3 newRotation = new Vector3(
                x + _rotation.X,
                y + _rotation.Y,
                z + _rotation.Z
            );
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(newRotation));
            Matrix4 result = rotationMatrix * modelMatrix.ClearRotation();
            _rotation = newRotation;//result.ExtractRotation().Xyz * rotationMultiplier;

            UpdateMatrixVariables(result);
            UpdateTransformMatrix();
        }

        public override void Rotate(Vector3 vector)
        {
            Rotate(vector.X, vector.Y, vector.Z);
        }

        public override void Scale(float scale)
        {
            Console.WriteLine(scale);

            Matrix4 scaleMatrix = Matrix4.CreateScale(this.scale * scale);
            Matrix4 result = scaleMatrix * modelMatrix.ClearScale();
            _scale = _scale * scale;

            UpdateMatrixVariables(result);
            UpdateTransformMatrix();
        }
    }
}
