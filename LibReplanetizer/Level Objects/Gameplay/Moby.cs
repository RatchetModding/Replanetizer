using LibReplanetizer.Models;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
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

        public long pVarMemoryAddress;

        public int unk10;
        public int unk11;
        public int unk12;
        public int unk13;
        public int unk14;

        public Moby()
        {

        }

        public Moby(Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Moby(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels, bool fromMemory = false)
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
                case 4:
                    GetDLVals(game, mobyBlock, num, mobyModels);
                    break;
                default:
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
            rotation = new Quaternion(rotx, roty, rotz);
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
            rotation = new Quaternion(rotx, roty, rotz);
            scale = new Vector3(scaleHolder); //Mobys only use the X axis of scale

            model = mobyModels.Find(mobyModel => mobyModel.id == modelID);
            UpdateTransformMatrix();
        }

        private void GetDLVals(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels)
        {
            int offset = num * game.mobyElemSize;

            missionID = ReadInt(mobyBlock, offset + 0x04);
            dataval = ReadInt(mobyBlock, offset + 0x08);
            unk1 = ReadInt(mobyBlock, offset + 0x0C);

            modelID = ReadInt(mobyBlock, offset + 0x10);
            float scaleHolder = ReadFloat(mobyBlock, offset + 0x14);
            rend1 = ReadInt(mobyBlock, offset + 0x18);
            rend2 = ReadInt(mobyBlock, offset + 0x1C);

            unk2 = ReadInt(mobyBlock, offset + 0x20);
            unk3 = ReadInt(mobyBlock, offset + 0x24);
            float x = ReadFloat(mobyBlock, offset + 0x28);
            float y = ReadFloat(mobyBlock, offset + 0x2C);

            float z = ReadFloat(mobyBlock, offset + 0x30);
            float rotx = ReadFloat(mobyBlock, offset + 0x34);
            float roty = ReadFloat(mobyBlock, offset + 0x38);
            float rotz = ReadFloat(mobyBlock, offset + 0x3C);

            unk4 = ReadInt(mobyBlock, offset + 0x40);
            unk5 = ReadInt(mobyBlock, offset + 0x44); //Group index?
            unk6 = ReadInt(mobyBlock, offset + 0x48); //Enables Z2
            unk7 = ReadInt(mobyBlock, offset + 0x4C);

            pvarIndex = ReadInt(mobyBlock, offset + 0x50);
            unk8 = ReadInt(mobyBlock, offset + 0x54);
            unk9 = ReadInt(mobyBlock, offset + 0x58);  //Breakability?
            int r = ReadInt(mobyBlock, offset + 0x5C);

            int g = ReadInt(mobyBlock, offset + 0x60);
            int b = ReadInt(mobyBlock, offset + 0x64);
            light = ReadInt(mobyBlock, offset + 0x68);
            unk14 = ReadInt(mobyBlock, offset + 0x6C);

            z2 = 0;
            cutscene = 0;

            color = Color.FromArgb(r, g, b);
            position = new Vector3(x, y, z);
            rotation = new Quaternion(rotx, roty, rotz);
            scale = new Vector3(scaleHolder); //Mobys only use the X axis of scale

            model = mobyModels.Find(mobyModel => mobyModel.id == modelID);
            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            //Mobies differ for each game, there is no universal method
            throw new NotImplementedException();
        }

        public byte[] ToByteArrayRC1()
        {
            Vector3 eulerAngles = ToEulerAngles(modelMatrix.ExtractRotation());

            byte[] buffer = new byte[GameType.mobySizes[0]];

            WriteInt(buffer, 0x00, GameType.mobySizes[0]);
            WriteInt(buffer, 0x04, missionID);
            WriteInt(buffer, 0x08, unk1);
            WriteInt(buffer, 0x0C, dataval);

            WriteInt(buffer, 0x10, drop);
            WriteInt(buffer, 0x14, unk2);
            WriteInt(buffer, 0x18, modelID);
            WriteFloat(buffer, 0x1C, scale.X);

            WriteInt(buffer, 0x20, rend1);
            WriteInt(buffer, 0x24, rend2);
            WriteInt(buffer, 0x28, unk3);
            WriteInt(buffer, 0x2C, unk4);

            WriteFloat(buffer, 0x30, position.X);
            WriteFloat(buffer, 0x34, position.Y);
            WriteFloat(buffer, 0x38, position.Z);
            WriteFloat(buffer, 0x3C, eulerAngles.X);

            WriteFloat(buffer, 0x40, eulerAngles.Y);
            WriteFloat(buffer, 0x44, eulerAngles.Z);
            WriteInt(buffer, 0x48, unk5);
            WriteInt(buffer, 0x4C, unk6);

            WriteFloat(buffer, 0x50, z2);
            WriteInt(buffer, 0x54, unk7);
            WriteInt(buffer, 0x58, pvarIndex);
            WriteInt(buffer, 0x5C, unk8);

            WriteInt(buffer, 0x60, unk9);
            WriteUint(buffer, 0x64, color.R);
            WriteUint(buffer, 0x68, color.G);
            WriteUint(buffer, 0x6C, color.B);

            WriteInt(buffer, 0x70, light);
            WriteInt(buffer, 0x74, cutscene);

            return buffer;
        }

        public byte[] ToByteArrayRC23()
        {
            Vector3 eulerAngles = ToEulerAngles(modelMatrix.ExtractRotation());

            byte[] buffer = new byte[GameType.mobySizes[1]];

            WriteInt(buffer, 0x00, GameType.mobySizes[1]);
            WriteInt(buffer, 0x04, missionID);
            WriteInt(buffer, 0x08, unk1);
            WriteInt(buffer, 0x0C, dataval);

            WriteInt(buffer, 0x10, unk2);
            WriteInt(buffer, 0x14, drop);
            WriteInt(buffer, 0x18, unk3);
            WriteInt(buffer, 0x1C, unk4);

            WriteInt(buffer, 0x20, unk5);
            WriteInt(buffer, 0x24, unk6);
            WriteInt(buffer, 0x28, modelID);
            WriteFloat(buffer, 0x2C, scale.X);

            WriteInt(buffer, 0x30, rend1);
            WriteInt(buffer, 0x34, rend2);
            WriteInt(buffer, 0x38, unk7);
            WriteInt(buffer, 0x3C, unk8);

            WriteFloat(buffer, 0x40, position.X);
            WriteFloat(buffer, 0x44, position.Y);
            WriteFloat(buffer, 0x48, position.Z);
            WriteFloat(buffer, 0x4C, eulerAngles.X);

            WriteFloat(buffer, 0x50, eulerAngles.Y);
            WriteFloat(buffer, 0x54, eulerAngles.Z);
            WriteInt(buffer, 0x58, unk9);
            WriteInt(buffer, 0x5C, unk10);

            WriteInt(buffer, 0x60, unk11);
            WriteInt(buffer, 0x64, unk12);
            WriteInt(buffer, 0x68, pvarIndex);
            WriteInt(buffer, 0x6C, unk13);

            WriteInt(buffer, 0x70, unk14);
            WriteUint(buffer, 0x74, color.R);
            WriteUint(buffer, 0x78, color.G);
            WriteUint(buffer, 0x7C, color.B);

            WriteInt(buffer, 0x80, light);
            WriteInt(buffer, 0x84, cutscene);

            return buffer;
        }


        public override LevelObject Clone()
        {
            return new Moby(model, new Vector3(position), rotation, scale);
        }

        public override void UpdateTransformMatrix()
        {
            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rot * translationMatrix;
        }

        public void UpdateFromMemory(byte[] mobyMemory, int offset, List<Model> models)
        {
            pVarMemoryAddress = 0x300000000 + ReadUint(mobyMemory, offset + 0x78);

            // If dead
            if (mobyMemory[offset + 0x20] > 0x7F)
            {
                model = null;
                return;
            }

            ushort modId = ReadUshort(mobyMemory, offset + 0xA6);
            float mobScale = ReadFloat(mobyMemory, offset + 0x2C);
            Model mod = models.Find(x => x.id == modId);

            if (mod == null)
            {
                mod = models.Find(x => x.id == 500);
            }

            model = mod;
            position = new Vector3(ReadFloat(mobyMemory, offset + 0x10), ReadFloat(mobyMemory, offset + 0x14), ReadFloat(mobyMemory, offset + 0x18));
            rotation = Quaternion.FromEulerAngles(ReadFloat(mobyMemory, offset + 0x40), ReadFloat(mobyMemory, offset + 0x44), ReadFloat(mobyMemory, offset + 0x48));
            scale = new Vector3(mobScale / model.size);
            UpdateTransformMatrix();
            modelID = modId;
        }
    }
}
