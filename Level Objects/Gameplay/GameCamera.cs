using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OpenTK;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class GameCamera : LevelObject
    {
        public const int ELEMENTSIZE = 0x20;

        [Browsable(false)]
        public Matrix4 modelMatrix { get; set; }

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

        public byte[] Serialize() {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteUint(ref bytes, 0x00, (uint)id);
            WriteFloat(ref bytes, 0x04, position.X);
            WriteFloat(ref bytes, 0x08, position.Y);
            WriteFloat(ref bytes, 0x0C, position.Z);
            WriteUint(ref bytes, 0x10, (uint)unk1);
            WriteUint(ref bytes, 0x14, (uint)unk2);
            WriteUint(ref bytes, 0x18, (uint)unk3);
            WriteUint(ref bytes, 0x1C, (uint)id2);

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public override void UpdateTransformMatrix()
        {
            Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotMatrix * translationMatrix;
        }

        //Transformable methods
        public override void Rotate(Vector3 vector)
        {
            rotation += vector;
        }

        public override void Translate(Vector3 vector)
        {
            position += vector;
        }
        public override void Scale(Vector3 scale) {
            this.scale *= scale;
        }


        public override void Render(CustomGLControl glControl, bool selected)
        {
            throw new NotImplementedException();
        }


    }
}
