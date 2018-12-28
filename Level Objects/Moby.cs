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
        private Vector3 _rotation = new Vector3();
        private float _scale;

        [CategoryAttribute("Attributes"), DisplayName("Mission ID")]
        public int missionID { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 1")]
        public uint unk1 { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Data value")]
        public uint dataval { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Bolt Drop")]
        public uint drop { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 2")]
        public uint unk2 { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Render distance 1")]
        public uint rend1 { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Render distance 2")]
        public uint rend2 { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 3")]
        public uint unk3 { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 4")]
        public uint unk4 { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 5")]
        public int unk5 { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 6")]
        public uint unk6 { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Secondary Z")]
        public float z2 { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 7")]
        public uint unk7 { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Pvar Index")]
        public int pvarIndex { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 8")]
        public uint unk8 { get; set; }

        [CategoryAttribute("Unknowns"), DisplayName("aUnknown 9")]
        public uint unk9 { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Color")]
        public Color color { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Light")]
        public uint light { get; set; }

        [CategoryAttribute("Attributes"), DisplayName("Cutscene")]
        public int cutscene { get; set; }


        public override float scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                UpdateTransformMatrix();
            }
        }

        public override Vector3 rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                UpdateTransformMatrix();
            }
        }

        public Moby(Model model, Vector3 position, Vector3 rotation, float scale)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;

        }

        public Moby(byte[] mobyBlock, int offset, List<Model> mobyModels)
        {
            missionID = ReadInt(mobyBlock, offset + 0x04);
            unk1 = ReadUint(mobyBlock, offset + 0x08);
            dataval = ReadUint(mobyBlock, offset + 0x0C);

            drop = ReadUint(mobyBlock, offset + 0x10);
            unk2 = ReadUint(mobyBlock, offset + 0x14);
            modelID = ReadInt(mobyBlock, offset + 0x18);
            scale = ReadFloat(mobyBlock, offset + 0x1C);

            rend1 = ReadUint(mobyBlock, offset + 0x20);
            rend2 = ReadUint(mobyBlock, offset + 0x24);
            unk3 = ReadUint(mobyBlock, offset + 0x28);
            unk4 = ReadUint(mobyBlock, offset + 0x2C);

            float x = ReadFloat(mobyBlock, offset + 0x30);
            float y = ReadFloat(mobyBlock, offset + 0x34);
            float z = ReadFloat(mobyBlock, offset + 0x38);
            float rotx = ReadFloat(mobyBlock, offset + 0x3C);

            float roty = ReadFloat(mobyBlock, offset + 0x40);
            float rotz = ReadFloat(mobyBlock, offset + 0x44);
            unk5 = ReadInt(mobyBlock, offset + 0x48);
            unk6 = ReadUint(mobyBlock, offset + 0x4C);

            z2 = ReadFloat(mobyBlock, offset + 0x50);
            unk7 = ReadUint(mobyBlock, offset + 0x54);
            pvarIndex = ReadInt(mobyBlock, offset + 0x58);
            unk8 = ReadUint(mobyBlock, offset + 0x5C);

            unk9 = ReadUint(mobyBlock, offset + 0x60);
            int r = ReadInt(mobyBlock, offset + 0x64);
            int g = ReadInt(mobyBlock, offset + 0x68);
            int b = ReadInt(mobyBlock, offset + 0x6C);

            light = ReadUint(mobyBlock, offset + 0x70);
            cutscene = ReadInt(mobyBlock, offset + 0x74);

            color = Color.FromArgb(r, g, b);
            position = new Vector3(x, y, z);
            rotation = new Vector3(rotx, roty, rotz);

            model = mobyModels.Find(mobyModel => mobyModel.ID == modelID);
            UpdateTransformMatrix();
        }

        public byte[] serialize()
        {
            byte[] buffer = new byte[0x78];

            WriteUint(ref buffer, 0x00, 0x78);
            WriteUint(ref buffer, 0x04, (uint)missionID);
            WriteUint(ref buffer, 0x08, unk1);
            WriteUint(ref buffer, 0x0C, dataval);

            WriteUint(ref buffer, 0x10, drop);
            WriteUint(ref buffer, 0x14, unk2);
            WriteUint(ref buffer, 0x18, (uint)modelID);
            WriteFloat(ref buffer, 0x1C, scale);

            WriteUint(ref buffer, 0x20, rend1);
            WriteUint(ref buffer, 0x24, rend2);
            WriteUint(ref buffer, 0x28, unk3);
            WriteUint(ref buffer, 0x2C, unk4);

            WriteFloat(ref buffer, 0x30, position.X);
            WriteFloat(ref buffer, 0x34, position.Y);
            WriteFloat(ref buffer, 0x38, position.Z);
            WriteFloat(ref buffer, 0x3C, rotation.X);

            WriteFloat(ref buffer, 0x40, rotation.Y);
            WriteFloat(ref buffer, 0x44, rotation.Z);
            WriteUint(ref buffer, 0x48, (uint)unk5);
            WriteUint(ref buffer, 0x4C, unk6);

            WriteFloat(ref buffer, 0x50, z2);
            WriteUint(ref buffer, 0x54, unk7);
            WriteUint(ref buffer, 0x58, (uint)pvarIndex);
            WriteUint(ref buffer, 0x5C, unk8);

            WriteUint(ref buffer, 0x60, unk9);
            WriteUint(ref buffer, 0x64, color.R);
            WriteUint(ref buffer, 0x68, color.G);
            WriteUint(ref buffer, 0x6C, color.B);

            WriteUint(ref buffer, 0x70, light);
            WriteUint(ref buffer, 0x74, (uint)cutscene);

            return buffer;
        }
        public override LevelObject Clone()
        {
            return new Moby(model, new Vector3(position), new Vector3(rotation), scale);
        }

        public override void UpdateTransformMatrix()
        {
            if (model == null) return;
            Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotMatrix * translationMatrix;
        }



        //Transformable methods
        public override void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }

        public override void Rotate(Vector3 vector)
        {
            rotation += vector;
        }

        public override void Scale(float scale)
        {
            scale += scale;
        }

        public override void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z));
        }

        public override void Translate(Vector3 vector)
        {
            position += vector;
        }
    }
}
