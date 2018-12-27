using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class Moby : LevelObject
    {
        public Vector3 rotation = new Vector3();
        public float scale;
        float rotx, roty, rotz;
        public Moby(byte[] mobyBlock, int offset, List<Model> mobyModels)
        {
            modelID = ReadInt(mobyBlock, offset + 0x18);

            float x = ReadFloat(mobyBlock, offset + 0x30);
            float y = ReadFloat(mobyBlock, offset + 0x34);
            float z = ReadFloat(mobyBlock, offset + 0x38);
            position = new Vector3(x, y, z);

            scale = ReadFloat(mobyBlock, offset + 0x1C);

            rotx = ReadFloat(mobyBlock, offset + 0x3C);
            roty = ReadFloat(mobyBlock, offset + 0x40);
            rotz = ReadFloat(mobyBlock, offset + 0x44);
            rotation = new Vector3(rotx, roty, rotz);


            model = mobyModels.Find(mobyModel => mobyModel.ID == modelID);
            updateTransform();
        }

        public override void updateTransform()
        {
            Matrix4 rotMatrix = Matrix4.CreateRotationX(rotx) * Matrix4.CreateRotationY(roty) * Matrix4.CreateRotationZ(rotz);
            //Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotMatrix * translationMatrix;
        }
    }
}
