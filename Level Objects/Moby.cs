using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
            UpdateTransformMatrix();
        }

        public override LevelObject Clone() {
            return new Moby(model, new Vector3(position), new Vector3(rotation), scale);
        }

        public override void UpdateTransformMatrix() {
            if (model == null) return;
            Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotMatrix * translationMatrix;
        }



        //Transformable methods
        public override void Rotate(float x, float y, float z) {
            Rotate(new Vector3(x, y, z));
        }

        public override void Rotate(Vector3 vector) {
            rotation += vector;
        }

        public override void Scale(float scale) {
            scale += scale;
        }

        public override void Translate(float x, float y, float z) {
            Translate(new Vector3(x, y, z));
        }

        public override void Translate(Vector3 vector) {
            _position += vector;
        }
    }
}
