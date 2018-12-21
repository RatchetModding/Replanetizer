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
        public float rotx;
        public float roty;
        public float rotz;
        public float scale;

        public Moby(byte[] mobyBlock, int offset, List<Model> mobyModels)
        {
            modelID =   ReadInt(mobyBlock, offset + 0x18);
            x =         ReadFloat(mobyBlock, offset + 0x30);
            y =         ReadFloat(mobyBlock, offset + 0x34);
            z =         ReadFloat(mobyBlock, offset + 0x38);
            scale =     ReadFloat(mobyBlock, offset + 0x1C);

            rotx = ReadFloat(mobyBlock, offset + 0x3C);
            roty = ReadFloat(mobyBlock, offset + 0x40);
            rotz = ReadFloat(mobyBlock, offset + 0x44);


            model = mobyModels.Find(mobyModel => mobyModel.ID == modelID);
            updateTranslation();
        }

        public override void updateTranslation()
        {
            Matrix4 rotMatrix = Matrix4.CreateRotationX(rotx) * Matrix4.CreateRotationY(roty) * Matrix4.CreateRotationZ(rotz);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            modelMatrix = scaleMatrix * rotMatrix * Matrix4.CreateTranslation(x, y, z);
        }
    }
}
