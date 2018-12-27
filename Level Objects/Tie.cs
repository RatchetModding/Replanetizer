using System.Collections.Generic;
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

        public ushort off_50;
        public uint off_54;
        public uint off_58;
        public uint off_5C;

        public uint off_60;
        public uint off_64;
        public uint off_68;
        public uint off_6C;

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

            off_60 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x60);
            off_64 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x64);
            off_68 = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x68);
            off_6C = ReadUint(levelBlock, (TIEELEMSIZE * num) + 0x6C);

            model = tieModels.Find(tieModel => tieModel.ID == modelID);
            updateTransform();
        }

        public override void updateTransform() {
            modelMatrix = new Matrix4(v1x, v1y, v1z, v1w, v2x, v2y, v2z, v2w, v3x, v3y, v3z, v3w, x, y, z, w);
        }

        public override LevelObject Clone() {
            return new Tie(modelMatrix);
        }

        //Transformable methods
        public override void Rotate(float x, float y, float z) {
            throw new System.NotImplementedException();
        }

        public override void Rotate(Vector3 vector) {
            throw new System.NotImplementedException();
        }

        public override void Scale(float scale) {
            throw new System.NotImplementedException();
        }

        public override void Translate(float x, float y, float z) {
            throw new System.NotImplementedException();
        }

        public override void Translate(Vector3 vector) {
            throw new System.NotImplementedException();
        }
    }
}
