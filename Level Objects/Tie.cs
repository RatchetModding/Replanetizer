using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;
using static RatchetEdit.DataFunctions;


namespace RatchetEdit
{
    public class Tie : ModelObject
    {
        const int TIEELEMSIZE = 0x70;

        float v1x;
        float v1y;
        float v1z;
        float v1w;

        float v2x;
        float v2y;
        float v2z;
        float v2w;

        float v3x;
        float v3y;
        float v3z;
        float v3w;

        float x,y,z,w;

        public ushort off_50 { get; set; }
        public uint off_54 { get; set; }
        public uint off_58 { get; set; }
        public uint off_5C { get; set; }

        public int colorOffset { get; set; }
        public uint off_64 { get; set; }
        public uint off_68 { get; set; }
        public uint off_6C{ get; set; }

        public override Vector3 position {
            get { return _position; }
            set {
                Translate(value - _position);
            }
        }
        public override Vector3 rotation {
            get { return _rotation; }
            set {
                Rotate(value - _rotation);
            }
        }

        public override float scale {
            get { return _scale; }
            set {
                Scale(value);
            }
        }

        public Tie(Matrix4 matrix4) {
            modelMatrix = Matrix4.Add(matrix4, new Matrix4());
        }

        public Tie(byte[] levelBlock, int num, List<Model> tieModels)
        {
            v1x = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x00);
            v1y = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x04);
            v1z = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x08);
            v1w = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x0C);

            v2x = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x10);
            v2y = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x14);
            v2z = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x18);
            v2w = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x1C);

            v3x = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x20);
            v3y = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x24);
            v3z = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x28);
            v3w = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x2C);

            x = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x30);
            y = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x34);
            z = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x38);
            w = ReadFloat(levelBlock, (TIEELEMSIZE * num) + 0x3C);

            

            /* These offsets are just placeholders for the render distance quaternion which is set in-game
            off_40 =    BAToUInt32(levelBlock, (TIEELEMSIZE * num) + 0x40);
            off_44 =    BAToUInt32(levelBlock, (TIEELEMSIZE * num) + 0x44);
            off_48 =    BAToUInt32(levelBlock, (TIEELEMSIZE * num) + 0x48);
            off_4C =    BAToUInt32(levelBlock, (TIEELEMSIZE * num) + 0x4C);
            */

            off_50 = ReadUshort(levelBlock, (TIEELEMSIZE * num) + 0x50);
            modelID = ReadUshort(levelBlock, (TIEELEMSIZE * num) + 0x52);
            off_54 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x54);
            off_58 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x58);
            off_5C = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x5C);

            colorOffset = ReadInt(levelBlock, (TIEELEMSIZE * num) + 0x60);
            off_64 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x64);
            off_68 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x68);
            off_6C = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x6C);

            model = tieModels.Find(tieModel => tieModel.ID == modelID);
            UpdateTransformMatrix();
            _rotation = modelMatrix.ExtractRotation().Xyz * 2;
            _position = modelMatrix.ExtractTranslation();

        }

        public override void UpdateTransformMatrix() {
            modelMatrix = new Matrix4(v1x, v1y, v1z, v1w, v2x, v2y, v2z, v2w, v3x, v3y, v3z, v3w, x, y, z, w);
        }

        public override LevelObject Clone() {
            return new Tie(modelMatrix);
        }

        void UpdateMatrixVariables(Matrix4 matrix) {
            this.v1x = matrix.M11;
            this.v1y = matrix.M12;
            this.v1z = matrix.M13;
            this.v1w = matrix.M14;

            this.v2x = matrix.M21;
            this.v2y = matrix.M22;
            this.v2z = matrix.M23;
            this.v2w = matrix.M24;

            this.v3x = matrix.M31;
            this.v3y = matrix.M32;
            this.v3z = matrix.M33;
            this.v3w = matrix.M34;

            this.x = matrix.M41;
            this.y = matrix.M42;
            this.z = matrix.M43;
            this.w = matrix.M44;
        }

        //Transformable methods
        public override void Translate(float x, float y, float z) {
            Vector3 rot = new Vector3(rotation);
            Rotate(-rot); //Rotate to 0,0,0 to do translation in world space.

            Matrix4 translationMatrix = Matrix4.CreateTranslation(x, y, z);
            Matrix4 result = translationMatrix * modelMatrix;
            _position = result.ExtractTranslation();
            UpdateMatrixVariables(result);
            UpdateTransformMatrix();

            Rotate(rot); //Rotate back to keep orientation

        }

        public override void Translate(Vector3 vector) {
            Translate(vector.X, vector.Y, vector.Z);
        }

        public override void Rotate(float x, float y, float z) {
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(x,y,z));
            Matrix4 result = rotationMatrix * modelMatrix.ClearRotation();
            _rotation = result.ExtractRotation().Xyz;

            UpdateMatrixVariables(result);
            UpdateTransformMatrix();
        }

        public override void Rotate(Vector3 vector) {
            Rotate(vector.X, vector.Y, vector.Z);
        }

        public override void Scale(float scale) {
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 result = scaleMatrix * modelMatrix.ClearScale();
            _scale = scale;

            UpdateMatrixVariables(result);
            UpdateTransformMatrix();
        }
    }
}
