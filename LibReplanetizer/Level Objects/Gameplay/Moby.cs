// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Moby : ModelObject
    {
        private GameType game;

        [Category("Attributes"), DisplayName("Mission ID")]
        public int missionID { get; set; }

        public enum SpawnType
        {
            NoRespawn, NoRespawnAtRevisitAfterInteraction, OnlySpawnAtRevisit, AlwaysRespawn, CrateNoRespawnAfterBreaking,
            CrateAlwaysRespawnAfterBreaking, UnknownType6, UnknownCrateType7, UnknownType8, UnknownType9,
            UnknownType10, UnknownType11, CrateAlwaysRespawnAtRevisit
        }

        [Category("Unknowns"), DisplayName("Spawn Type")]
        public SpawnType spawnType { get; set; }

        [Category("Unknowns"), DisplayName("Data Value")]
        public int dataval { get; set; }

        [Category("Attributes"), DisplayName("Bolt Drop")]
        public int bolts { get; set; }

        [Category("Attributes"), DisplayName("Moby ID")]
        public int mobyID { get; set; }

        [Category("Attributes"), DisplayName("Draw Distance")]
        public int drawDistance { get; set; }

        [Category("Attributes"), DisplayName("Update Distance")]
        public int updateDistance { get; set; }

        /*
         * Unknown3A
         * Always 0
         * 
         * Unknown3B
         * Always 0 in RAC1
         * Appears as none zero in RAC2 or RAC3 but only on enemies when the 2nd bit of Unknown1 is set
         * RAC2 has 5, 11, 24, 48, 72, 120, 163, 240
         * RAC3 has 398, 1667
         * It is now always lower than unknown4 but very often
         * It may correspond to HP change on act tuning
         */
        [Category("Unknowns"), DisplayName("aUnknown 3A")]
        public short unk3A { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 3B")]
        public short unk3B { get; set; }

        /*
         * 1 or 0 (possibly a has animation flag? or has AI?)
         */
        [Category("Unknowns"), DisplayName("aUnknown 4")]
        public int unk4 { get; set; }

        [Category("Attributes"), DisplayName("Group Index")]
        public int groupIndex { get; set; }

        [Category("Attributes"), DisplayName("Is Rooted?")]
        public int isRooted { get; set; }

        [Category("Attributes"), DisplayName("Rooted Distance")]
        public float rootedDistance { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 6")]
        public int unk6 { get; set; }

        [Category("Attributes"), DisplayName("Secondary Z")]
        public float z2 { get; set; }

        /*
         * Unknown7A
         * Always 8192
         * 
         * Unknown7B
         * Always 0
         */
        [Category("Unknowns"), DisplayName("7A: Always 8192")]
        public short unk7A { get; set; }

        [Category("Unknowns"), DisplayName("7B: Always 0")]
        public short unk7B { get; set; }

        [Category("Attributes"), DisplayName("pVar Index")]
        public int pvarIndex { get; set; }

        /*
         * Unknown8A
         * Always 16384 
         * 
         * Unknown8B
         * Always 0
         */
        [Category("Unknowns"), DisplayName("8A: Always 16384")]
        public short unk8A { get; set; }

        [Category("Unknowns"), DisplayName("8B: Always 0")]
        public short unk8B { get; set; }

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

        [Category("Unknowns"), DisplayName("aUnknown 10")]
        public int unk10 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 11")]
        public int unk11 { get; set; }

        /*
         * Unknown12A
         * Always 256
         * 
         * Unknown12B
         * Always 0
         */
        [Category("Unknowns"), DisplayName("12A: Always 256")]
        public short unk12A { get; set; }

        [Category("Unknowns"), DisplayName("12B: Always 0")]
        public short unk12B { get; set; }

        /*
         * Seems to correspond to HP in RAC2 and RAC3 (It does not)
         */
        [Category("Unknowns"), DisplayName("aUnknown 13")]
        public int unk13 { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 14")]
        public int unk14 { get; set; }

        public Moby()
        {

        }

        public Moby(GameType game, Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.game = game;
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Moby(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels, bool fromMemory = false)
        {
            this.game = game;

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
            spawnType = (SpawnType) ReadInt(mobyBlock, offset + 0x08);
            mobyID = ReadInt(mobyBlock, offset + 0x0C);

            bolts = ReadInt(mobyBlock, offset + 0x10);
            dataval = ReadInt(mobyBlock, offset + 0x14);
            modelID = ReadInt(mobyBlock, offset + 0x18);
            float scaleHolder = ReadFloat(mobyBlock, offset + 0x1C);

            drawDistance = ReadInt(mobyBlock, offset + 0x20);
            updateDistance = ReadInt(mobyBlock, offset + 0x24);
            unk7A = ReadShort(mobyBlock, offset + 0x28);
            unk7B = ReadShort(mobyBlock, offset + 0x2A);
            unk8A = ReadShort(mobyBlock, offset + 0x2C);
            unk8B = ReadShort(mobyBlock, offset + 0x2E);

            float x = ReadFloat(mobyBlock, offset + 0x30);
            float y = ReadFloat(mobyBlock, offset + 0x34);
            float z = ReadFloat(mobyBlock, offset + 0x38);
            float rotx = ReadFloat(mobyBlock, offset + 0x3C);

            float roty = ReadFloat(mobyBlock, offset + 0x40);
            float rotz = ReadFloat(mobyBlock, offset + 0x44);
            groupIndex = ReadInt(mobyBlock, offset + 0x48);
            unk6 = ReadInt(mobyBlock, offset + 0x4C); //Enables Z2

            z2 = ReadFloat(mobyBlock, offset + 0x50);
            unk12A = ReadShort(mobyBlock, offset + 0x54);
            unk12B = ReadShort(mobyBlock, offset + 0x56);
            pvarIndex = ReadInt(mobyBlock, offset + 0x58);
            unk4 = ReadInt(mobyBlock, offset + 0x5C);

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
            dataval = ReadInt(mobyBlock, offset + 0x08);
            spawnType = (SpawnType) ReadInt(mobyBlock, offset + 0x0C);

            mobyID = ReadInt(mobyBlock, offset + 0x10);
            bolts = ReadInt(mobyBlock, offset + 0x14);
            unk3A = ReadShort(mobyBlock, offset + 0x18);
            unk3B = ReadShort(mobyBlock, offset + 0x1A);
            unk13 = ReadInt(mobyBlock, offset + 0x1C);

            unk9 = ReadInt(mobyBlock, offset + 0x20);
            unk6 = ReadInt(mobyBlock, offset + 0x24); //Enables Z2
            modelID = ReadInt(mobyBlock, offset + 0x28);
            float scaleHolder = ReadFloat(mobyBlock, offset + 0x2C);

            drawDistance = ReadInt(mobyBlock, offset + 0x30);
            updateDistance = ReadInt(mobyBlock, offset + 0x34);
            unk7A = ReadShort(mobyBlock, offset + 0x38);
            unk7B = ReadShort(mobyBlock, offset + 0x3A);
            unk8A = ReadShort(mobyBlock, offset + 0x3C);
            unk8B = ReadShort(mobyBlock, offset + 0x3E);

            float x = ReadFloat(mobyBlock, offset + 0x40);
            float y = ReadFloat(mobyBlock, offset + 0x44);
            float z = ReadFloat(mobyBlock, offset + 0x48);
            float rotx = ReadFloat(mobyBlock, offset + 0x4C);

            float roty = ReadFloat(mobyBlock, offset + 0x50);
            float rotz = ReadFloat(mobyBlock, offset + 0x54);
            groupIndex = ReadInt(mobyBlock, offset + 0x58);
            unk10 = ReadInt(mobyBlock, offset + 0x5C);

            unk11 = ReadInt(mobyBlock, offset + 0x60);
            unk12A = ReadShort(mobyBlock, offset + 0x64);
            unk12B = ReadShort(mobyBlock, offset + 0x66);
            pvarIndex = ReadInt(mobyBlock, offset + 0x68);
            unk4 = ReadInt(mobyBlock, offset + 0x6C);

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
            mobyID = ReadInt(mobyBlock, offset + 0x08);
            bolts = ReadInt(mobyBlock, offset + 0x0C);

            modelID = ReadInt(mobyBlock, offset + 0x10);
            float scaleHolder = ReadFloat(mobyBlock, offset + 0x14);
            drawDistance = ReadInt(mobyBlock, offset + 0x18);
            updateDistance = ReadInt(mobyBlock, offset + 0x1C);

            unk7A = ReadShort(mobyBlock, offset + 0x20);
            unk7B = ReadShort(mobyBlock, offset + 0x22);
            unk8A = ReadShort(mobyBlock, offset + 0x24);
            unk8B = ReadShort(mobyBlock, offset + 0x26);
            float x = ReadFloat(mobyBlock, offset + 0x28);
            float y = ReadFloat(mobyBlock, offset + 0x2C);

            float z = ReadFloat(mobyBlock, offset + 0x30);
            float rotx = ReadFloat(mobyBlock, offset + 0x34);
            float roty = ReadFloat(mobyBlock, offset + 0x38);
            float rotz = ReadFloat(mobyBlock, offset + 0x3C);

            groupIndex = ReadInt(mobyBlock, offset + 0x40);
            isRooted = ReadInt(mobyBlock, offset + 0x44);
            rootedDistance = ReadFloat(mobyBlock, offset + 0x48);
            unk12A = ReadShort(mobyBlock, offset + 0x4C);
            unk12B = ReadShort(mobyBlock, offset + 0x4E);

            pvarIndex = ReadInt(mobyBlock, offset + 0x50);
            unk3A = ReadShort(mobyBlock, offset + 0x54);
            unk3B = ReadShort(mobyBlock, offset + 0x56);
            unk9 = ReadInt(mobyBlock, offset + 0x58);  //Breakability?
            int r = ReadInt(mobyBlock, offset + 0x5C);

            int g = ReadInt(mobyBlock, offset + 0x60);
            int b = ReadInt(mobyBlock, offset + 0x64);
            light = ReadInt(mobyBlock, offset + 0x68);
            unk14 = ReadInt(mobyBlock, offset + 0x6C);

            cutscene = 0;
            z2 = 0;

            color = Color.FromArgb(r, g, b);
            position = new Vector3(x, y, z);
            rotation = new Quaternion(rotx, roty, rotz);
            scale = new Vector3(scaleHolder); //Mobys only use the X axis of scale

            model = mobyModels.Find(mobyModel => mobyModel.id == modelID);
            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            switch (game.num)
            {
                case 1:
                    return ToByteArrayRC1();
                case 2:
                case 3:
                    return ToByteArrayRC23();
                case 4:
                    return ToByteArrayDL();
                default:
                    return ToByteArrayRC23();
            }
        }

        private byte[] ToByteArrayRC1()
        {
            Vector3 eulerAngles = rotation.ToEulerAngles();

            byte[] buffer = new byte[game.mobyElemSize];

            WriteInt(buffer, 0x00, game.mobyElemSize);
            WriteInt(buffer, 0x04, missionID);
            WriteInt(buffer, 0x08, (int) spawnType);
            WriteInt(buffer, 0x0C, mobyID);

            WriteInt(buffer, 0x10, bolts);
            WriteInt(buffer, 0x14, dataval);
            WriteInt(buffer, 0x18, modelID);
            WriteFloat(buffer, 0x1C, scale.X);

            WriteInt(buffer, 0x20, drawDistance);
            WriteInt(buffer, 0x24, updateDistance);
            WriteShort(buffer, 0x28, unk7A);
            WriteShort(buffer, 0x2A, unk7B);
            WriteShort(buffer, 0x2C, unk8A);
            WriteShort(buffer, 0x2E, unk8B);

            WriteFloat(buffer, 0x30, position.X);
            WriteFloat(buffer, 0x34, position.Y);
            WriteFloat(buffer, 0x38, position.Z);
            WriteFloat(buffer, 0x3C, eulerAngles.X);

            WriteFloat(buffer, 0x40, eulerAngles.Y);
            WriteFloat(buffer, 0x44, eulerAngles.Z);
            WriteInt(buffer, 0x48, groupIndex);
            WriteInt(buffer, 0x4C, unk6);

            WriteFloat(buffer, 0x50, z2);
            WriteShort(buffer, 0x54, unk12A);
            WriteShort(buffer, 0x56, unk12B);
            WriteInt(buffer, 0x58, pvarIndex);
            WriteInt(buffer, 0x5C, unk4);

            WriteInt(buffer, 0x60, unk9);
            WriteUint(buffer, 0x64, color.R);
            WriteUint(buffer, 0x68, color.G);
            WriteUint(buffer, 0x6C, color.B);

            WriteInt(buffer, 0x70, light);
            WriteInt(buffer, 0x74, cutscene);

            return buffer;
        }

        private byte[] ToByteArrayRC23()
        {
            Vector3 eulerAngles = rotation.ToEulerAngles();

            byte[] buffer = new byte[game.mobyElemSize];

            WriteInt(buffer, 0x00, game.mobyElemSize);
            WriteInt(buffer, 0x04, missionID);
            WriteInt(buffer, 0x08, dataval);
            WriteInt(buffer, 0x0C, (int) spawnType);

            WriteInt(buffer, 0x10, mobyID);
            WriteInt(buffer, 0x14, bolts);
            WriteShort(buffer, 0x18, unk3A);
            WriteShort(buffer, 0x1A, unk3B);
            WriteInt(buffer, 0x1C, unk13);

            WriteInt(buffer, 0x20, unk9);
            WriteInt(buffer, 0x24, unk6);
            WriteInt(buffer, 0x28, modelID);
            WriteFloat(buffer, 0x2C, scale.X);

            WriteInt(buffer, 0x30, drawDistance);
            WriteInt(buffer, 0x34, updateDistance);
            WriteShort(buffer, 0x38, unk7A);
            WriteShort(buffer, 0x3A, unk7B);
            WriteShort(buffer, 0x3C, unk8A);
            WriteShort(buffer, 0x3E, unk8B);

            WriteFloat(buffer, 0x40, position.X);
            WriteFloat(buffer, 0x44, position.Y);
            WriteFloat(buffer, 0x48, position.Z);
            WriteFloat(buffer, 0x4C, eulerAngles.X);

            WriteFloat(buffer, 0x50, eulerAngles.Y);
            WriteFloat(buffer, 0x54, eulerAngles.Z);
            WriteInt(buffer, 0x58, groupIndex);
            WriteInt(buffer, 0x5C, unk10);

            WriteInt(buffer, 0x60, unk11);
            WriteShort(buffer, 0x64, unk12A);
            WriteShort(buffer, 0x66, unk12B);
            WriteInt(buffer, 0x68, pvarIndex);
            WriteInt(buffer, 0x6C, unk4);

            WriteInt(buffer, 0x70, unk14);
            WriteUint(buffer, 0x74, color.R);
            WriteUint(buffer, 0x78, color.G);
            WriteUint(buffer, 0x7C, color.B);

            WriteInt(buffer, 0x80, light);
            WriteInt(buffer, 0x84, cutscene);

            return buffer;
        }

        private byte[] ToByteArrayDL()
        {
            Vector3 eulerAngles = rotation.ToEulerAngles();

            byte[] buffer = new byte[game.mobyElemSize];

            WriteInt(buffer, 0x00, game.mobyElemSize);
            WriteInt(buffer, 0x04, missionID);
            WriteInt(buffer, 0x08, mobyID);
            WriteInt(buffer, 0x0C, bolts);

            WriteInt(buffer, 0x10, modelID);
            WriteFloat(buffer, 0x14, scale.X);
            WriteInt(buffer, 0x18, drawDistance);
            WriteInt(buffer, 0x1C, updateDistance);

            WriteShort(buffer, 0x20, unk7A);
            WriteShort(buffer, 0x22, unk7B);
            WriteShort(buffer, 0x24, unk8A);
            WriteShort(buffer, 0x26, unk8B);
            WriteFloat(buffer, 0x28, position.X);
            WriteFloat(buffer, 0x2C, position.Y);

            WriteFloat(buffer, 0x30, position.Z);
            WriteFloat(buffer, 0x34, eulerAngles.X);
            WriteFloat(buffer, 0x38, eulerAngles.Y);
            WriteFloat(buffer, 0x3A, eulerAngles.Z);

            WriteInt(buffer, 0x40, groupIndex);
            WriteInt(buffer, 0x44, isRooted);
            WriteFloat(buffer, 0x48, rootedDistance);
            WriteShort(buffer, 0x4C, unk12A);
            WriteShort(buffer, 0x4E, unk12B);

            WriteInt(buffer, 0x50, pvarIndex);
            WriteShort(buffer, 0x54, unk3A);
            WriteShort(buffer, 0x56, unk3B);
            WriteInt(buffer, 0x58, unk9);
            WriteInt(buffer, 0x5C, color.R);

            WriteInt(buffer, 0x60, color.G);
            WriteInt(buffer, 0x64, color.B);
            WriteInt(buffer, 0x68, light);
            WriteInt(buffer, 0x6C, unk14);

            return buffer;
        }


        public override LevelObject Clone()
        {
            return new Moby(game, model, new Vector3(position), rotation, scale);
        }

        public override void UpdateTransformMatrix()
        {
            Vector3 euler = rotation.ToEulerAngles();
            Matrix4 rotZ = Matrix4.CreateFromAxisAngle(Vector3.UnitZ, euler.Z);
            Matrix4 rotY = Matrix4.CreateFromAxisAngle(Vector3.UnitY, euler.Y);
            Matrix4 rotX = Matrix4.CreateFromAxisAngle(Vector3.UnitX, euler.X);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale * model.size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotX * rotY * rotZ * translationMatrix;
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
