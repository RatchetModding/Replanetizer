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
    public class Moby : ModelObject
    {
        public float scale;
        private Vector3 _rotation = new Vector3();
        public Vector3 rotation {
            get { return _rotation; }
            set {
                _rotation = value;
                updateTransform();
            }
        }
        
        public Moby(Model model, Vector3 position, Vector3 rotation, float scale) {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            
        }

        public Moby(byte[] mobyBlock, int offset, List<Model> mobyModels)
        {
            modelID = ReadInt(mobyBlock, offset + 0x18);

            float x = ReadFloat(mobyBlock, offset + 0x30);
            float y = ReadFloat(mobyBlock, offset + 0x34);
            float z = ReadFloat(mobyBlock, offset + 0x38);
            position = new Vector3(x, y, z);

            scale = ReadFloat(mobyBlock, offset + 0x1C);

            float rotx = ReadFloat(mobyBlock, offset + 0x3C);
            float roty = ReadFloat(mobyBlock, offset + 0x40);
            float rotz = ReadFloat(mobyBlock, offset + 0x44);
            rotation = new Vector3(rotx, roty, rotz);


            model = mobyModels.Find(mobyModel => mobyModel.ID == modelID);
            updateTransform();
        }

        public override void updateTransform() {
            if (model == null) return;
            Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotMatrix * translationMatrix;
        }

        public Moby CloneMoby() {
            return new Moby(model, new Vector3(position), new Vector3(rotation), scale);
        }
    }
}
