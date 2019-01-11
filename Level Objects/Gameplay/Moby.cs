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
        public const int ELEMENTSIZE = 0x78;
        
        [Category("Attributes"), DisplayName("Mission ID")]
        public int missionID { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 1")]
        public int unk1 { get; set; }

        [Category("Attributes"), DisplayName("Data Value")]
        public int dataval { get; set; }

        [Category("Attributes"), DisplayName("Bolt Drop")]
        public int drop { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 2")]
        public int unk2 { get; set; }

        [Category("Attributes"), DisplayName("Render Distance 1")]
        public int rend1 { get; set; }

        [Category("Attributes"), DisplayName("Render Distance 2")]
        public int rend2 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 3")]
        public int unk3 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 4")]
        public int unk4 { get; set; }

        [Category("Attributes"), DisplayName("Group Index")]
        public int unk5 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 6")]
        public int unk6 { get; set; }

        [Category("Attributes"), DisplayName("Secondary Z")]
        public float z2 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 7")]
        public int unk7 { get; set; }

        [Category("Attributes"), DisplayName("pVar Index")]
        public int pvarIndex { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 8")]
        public int unk8 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 9")]
        public int unk9 { get; set; }

        [Category("Attributes"), DisplayName("Color")]
        public Color color { get; set; }

        [Category("Attributes"), DisplayName("Light")]
        public int light { get; set; }

        [Category("Attributes"), DisplayName("Cutscene")]
        public int cutscene { get; set; }

        [Category("Attributes"), DisplayName("pVars")]
        public byte[] pVars { get; set; }

        public int unk10;
        public int unk11;
        public int unk12;
        public int unk13;
        public int unk14;

        public Moby(Model model, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;

        }

        public Moby(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels)
        {
            switch (game.num)
            {
                case 1:
                    GetRC1Vals(game, mobyBlock, num, mobyModels);
                    break;
                case 2:
                case 3:
                    GetRC23Vals(game, mobyBlock, num, mobyModels);
                    break;
            }
        }

        private void GetRC1Vals(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels)
        {
            int offset = num * game.mobyElemSize;
            missionID = ReadInt(mobyBlock, offset + 0x04);
            unk1 = ReadInt(mobyBlock, offset + 0x08);
            dataval = ReadInt(mobyBlock, offset + 0x0C);

            drop = ReadInt(mobyBlock, offset + 0x10);
            unk2 = ReadInt(mobyBlock, offset + 0x14);
            modelID = ReadInt(mobyBlock, offset + 0x18);
            float scaleHolder = ReadFloat(mobyBlock, offset + 0x1C);

            rend1 = ReadInt(mobyBlock, offset + 0x20);
            rend2 = ReadInt(mobyBlock, offset + 0x24);
            unk3 = ReadInt(mobyBlock, offset + 0x28);
            unk4 = ReadInt(mobyBlock, offset + 0x2C);

            float x = ReadFloat(mobyBlock, offset + 0x30);
            float y = ReadFloat(mobyBlock, offset + 0x34);
            float z = ReadFloat(mobyBlock, offset + 0x38);
            float rotx = ReadFloat(mobyBlock, offset + 0x3C);

            float roty = ReadFloat(mobyBlock, offset + 0x40);
            float rotz = ReadFloat(mobyBlock, offset + 0x44);
            unk5 = ReadInt(mobyBlock, offset + 0x48); //Group index?
            unk6 = ReadInt(mobyBlock, offset + 0x4C); //Enables Z2

            z2 = ReadFloat(mobyBlock, offset + 0x50);
            unk7 = ReadInt(mobyBlock, offset + 0x54);
            pvarIndex = ReadInt(mobyBlock, offset + 0x58);
            unk8 = ReadInt(mobyBlock, offset + 0x5C);

            unk9 = ReadInt(mobyBlock, offset + 0x60);  //Breakability?
            int r = ReadInt(mobyBlock, offset + 0x64);
            int g = ReadInt(mobyBlock, offset + 0x68);
            int b = ReadInt(mobyBlock, offset + 0x6C);

            light = ReadInt(mobyBlock, offset + 0x70);
            cutscene = ReadInt(mobyBlock, offset + 0x74);

            color = Color.FromArgb(r, g, b);
            position = new Vector3(x, y, z);
            rotation = new Vector3(rotx, roty, rotz);
            scale = new Vector3(scaleHolder, scaleHolder, scaleHolder);

            model = mobyModels.Find(mobyModel => mobyModel.id == modelID);
            UpdateTransformMatrix();
        }

        private void GetRC23Vals(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels)
        {
            int offset = num * game.mobyElemSize;

            missionID = ReadInt(mobyBlock, offset + 0x04);
            unk1 = ReadInt(mobyBlock, offset + 0x08);
            dataval = ReadInt(mobyBlock, offset + 0x0C);

            unk2 = ReadInt(mobyBlock, offset + 0x10);
            drop = ReadInt(mobyBlock, offset + 0x14);
            unk3 = ReadInt(mobyBlock, offset + 0x18);
            unk4 = ReadInt(mobyBlock, offset + 0x1C);

            unk5 = ReadInt(mobyBlock, offset + 0x20); //Group index?
            unk6 = ReadInt(mobyBlock, offset + 0x24); //Enables Z2
            modelID = ReadInt(mobyBlock, offset + 0x28);

            float scaleHolder = ReadFloat(mobyBlock, offset + 0x2C);

            rend1 = ReadInt(mobyBlock, offset + 0x30);
            rend2 = ReadInt(mobyBlock, offset + 0x34);
            unk7 = ReadInt(mobyBlock, offset + 0x38);
            unk8 = ReadInt(mobyBlock, offset + 0x3C);

            float x = ReadFloat(mobyBlock, offset + 0x40);
            float y = ReadFloat(mobyBlock, offset + 0x44);
            float z = ReadFloat(mobyBlock, offset + 0x48);

            float rotx = ReadFloat(mobyBlock, offset + 0x4C);
            float roty = ReadFloat(mobyBlock, offset + 0x50);
            float rotz = ReadFloat(mobyBlock, offset + 0x54);

            unk9 = ReadInt(mobyBlock, offset + 0x58);  //Breakability?
            unk10 = ReadInt(mobyBlock, offset + 0x5C);
            unk11 = ReadInt(mobyBlock, offset + 0x60);
            unk12 = ReadInt(mobyBlock, offset + 0x64);
            pvarIndex = ReadInt(mobyBlock, offset + 0x68);
            unk13 = ReadInt(mobyBlock, offset + 0x6C);

            unk14 = ReadInt(mobyBlock, offset + 0x70);
            int r = ReadInt(mobyBlock, offset + 0x74);
            int g = ReadInt(mobyBlock, offset + 0x78);
            int b = ReadInt(mobyBlock, offset + 0x7C);

            light = ReadInt(mobyBlock, offset + 0x80);
            cutscene = ReadInt(mobyBlock, offset + 0x84);

            color = Color.FromArgb(r, g, b);
            position = new Vector3(x, y, z);
            rotation = new Vector3(rotx, roty, rotz);
            scale = new Vector3(scaleHolder, scaleHolder, scaleHolder); //Mobys only use the X axis of scale

            model = mobyModels.Find(mobyModel => mobyModel.id == modelID);
            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] buffer = new byte[ELEMENTSIZE];

            WriteInt(ref buffer, 0x00, ELEMENTSIZE);
            WriteInt(ref buffer, 0x04, missionID);
            WriteInt(ref buffer, 0x08, unk1);
            WriteInt(ref buffer, 0x0C, dataval);

            WriteInt(ref buffer, 0x10, drop);
            WriteInt(ref buffer, 0x14, unk2);
            WriteInt(ref buffer, 0x18, modelID);
            WriteFloat(ref buffer, 0x1C, scale.X);

            WriteInt(ref buffer, 0x20, rend1);
            WriteInt(ref buffer, 0x24, rend2);
            WriteInt(ref buffer, 0x28, unk3);
            WriteInt(ref buffer, 0x2C, unk4);

            WriteFloat(ref buffer, 0x30, position.X);
            WriteFloat(ref buffer, 0x34, position.Y);
            WriteFloat(ref buffer, 0x38, position.Z);
            WriteFloat(ref buffer, 0x3C, rotation.X);
            WriteFloat(ref buffer, 0x40, rotation.Y);
            WriteFloat(ref buffer, 0x44, rotation.Z);
            WriteInt(ref buffer, 0x48, unk5);
            WriteInt(ref buffer, 0x4C, unk6);

            WriteFloat(ref buffer, 0x50, z2);
            WriteInt(ref buffer, 0x54, unk7);
            WriteInt(ref buffer, 0x58, pvarIndex);
            WriteInt(ref buffer, 0x5C, unk8);

            WriteInt(ref buffer, 0x60, unk9);
            WriteUint(ref buffer, 0x64, color.R);
            WriteUint(ref buffer, 0x68, color.G);
            WriteUint(ref buffer, 0x6C, color.B);

            WriteInt(ref buffer, 0x70, light);
            WriteInt(ref buffer, 0x74, cutscene);

            return buffer;
        }

        public override void UpdateTransformMatrix() 
        {
            if (model == null) return;
            Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale.X * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotMatrix * translationMatrix;
        }

        public override LevelObject Clone()
        {
            return new Moby(model, new Vector3(position), new Vector3(rotation), scale);
        }

        public override void Rotate(Vector3 vector)
        {
            rotation += vector;
        }

        public override void Scale(Vector3 scale) //Mobys only use the X axis of scale
        {
            this.scale *= Vector3.UnitX * scale;
        }

        public override void Translate(Vector3 vector)
        {
            position += vector;
        }
    }
}
