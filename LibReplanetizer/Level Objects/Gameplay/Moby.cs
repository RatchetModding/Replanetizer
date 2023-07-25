// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class Moby : ModelObject
    {
        private GameType game;

        private static int MAX_ID = 0;

        [Category("Attributes"), DisplayName("Ingame Memory"), Description("This field contains the current variable state if the memory hook is active.")]
        public IngameMobyMemory? memory { get; set; } = null;

        [Category("Attributes"), DisplayName("Mission ID"), Description("Every planet has a set of missions. If a moby is assigned to a mission, its spawning behaviour can be based on whether the mission is completed.")]
        public int missionID { get; set; }

        [Category("Attributes"), DisplayName("Spawn Type Bitmask"), Description("Each bit corresponds to a spawn related boolean. If this value is zero then the game determines through other means how to spawn this moby.")]
        public Bitmask spawnType { get; set; } = 0;

        [Category("Attributes"), DisplayName("Spawn Before Mission Completion?"), Description("Moby will still spawn after mission completion if there was no interaction with it yet.")]
        public bool spawnBeforeMissionCompletion
        {
            get
            {
                return (spawnType & 0b00001) > 0;
            }
            set
            {
                if (value)
                {
                    spawnType |= 0b00001;
                }
                else
                {
                    spawnType &= ~0b00001;
                }
            }
        }

        [Category("Attributes"), DisplayName("Spawn After Mission Completion?")]
        public bool spawnAfterMissionCompletion
        {
            get
            {
                return (spawnType & 0b00010) > 0;
            }
            set
            {
                if (value)
                {
                    spawnType |= 0b00010;
                }
                else
                {
                    spawnType &= ~0b00010;
                }
            }
        }

        [Category("Attributes"), DisplayName("Is Crate?")]
        public bool isCrate
        {
            get
            {
                return (spawnType & 0b00100) > 0;
            }
            set
            {
                if (value)
                {
                    spawnType |= 0b00100;
                }
                else
                {
                    spawnType &= ~0b00100;
                }
            }
        }

        [Category("Attributes"), DisplayName("Spawn Before Death?"), Description("Moby will still spawn after death if there was no interaction with it yet. This moby will always spawn when the level is loaded.")]
        public bool spawnBeforeDeath
        {
            get
            {
                return (spawnType & 0b01000) > 0;
            }
            set
            {
                if (value)
                {
                    spawnType |= 0b01000;
                }
                else
                {
                    spawnType &= ~0b01000;
                }
            }
        }

        [Category("Attributes"), DisplayName("Is Spawner?")]
        public bool isSpawner
        {
            get
            {
                return (spawnType & 0b10000) > 0;
            }
            set
            {
                if (value)
                {
                    spawnType |= 0b10000;
                }
                else
                {
                    spawnType &= ~0b10000;
                }
            }
        }

        [Category("Attributes"), DisplayName("Data Value"), Description("This value probably defines instance specific behaviour. The exact behaviour any value corresponds to probably depends on the specific moby class.")]
        public int dataval { get; set; }

        [Category("Attributes"), DisplayName("Bolt Drop")]
        public int bolts { get; set; }

        [Category("Attributes"), DisplayName("Moby ID")]
        public int mobyID { get; set; }

        [Category("Attributes"), DisplayName("Draw Distance"), Description("The distance at which an object will start fading out. After a distance of 8 more units, the object will stop being drawn.")]
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

        [Category("Unknowns"), DisplayName("Occlusion")]
        public bool occlusion { get; set; }

        [Category("Attributes"), DisplayName("Group Index")]
        public int groupIndex { get; set; }

        [Category("Attributes"), DisplayName("Is Rooted?")]
        public int isRooted { get; set; }

        [Category("Attributes"), DisplayName("Rooted Distance")]
        public float rootedDistance { get; set; }

        [Category("Unknowns"), DisplayName("aUnknown 6")]
        public int unk6 { get; set; }

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
        public int pvarIndex { get; set; } = -1;

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

        [Category("Attributes"), DisplayName("Color"), Description("Static diffuse lighting applied to the moby.")]
        public Rgb24 color { get; set; }

        [Category("Attributes"), DisplayName("Light"), Description("Index of the directional light that is applied to the moby.")]
        public int light { get; set; }

        [Category("Attributes"), DisplayName("Cutscene")]
        public int cutscene { get; set; }

        [Category("Attributes"), DisplayName("pVars")]
        public byte[] pVars { get; set; }

        private long pVarMemoryAddress;

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

        [Category("Attributes"), DisplayName("EXP value")]
        public int exp { get; set; }

        [Category("Attributes"), DisplayName("Mode Bits")]
        public Bitmask mode { get; set; } = 0;

        // This should probably get removed, not enough information are available to construct a moby like that
        public Moby()
        {
            this.game = GameType.RaC1;
            this.pVars = new byte[0];
            this.mobyID = MAX_ID++;
        }

        public Moby(GameType game, Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.game = game;
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.mobyID = MAX_ID++;
            this.pVars = new byte[0];

            UpdateTransformMatrix();
        }

        public Moby(Moby referenceMoby)
        {
            this.game = referenceMoby.game;
            this.cutscene = referenceMoby.cutscene;
            this.missionID = referenceMoby.missionID;
            this.bolts = referenceMoby.bolts;
            this.dataval = referenceMoby.dataval;
            this.model = referenceMoby.model;
            this.modelID = referenceMoby.modelID;
            this.spawnType = referenceMoby.spawnType;
            this.updateDistance = referenceMoby.updateDistance;
            this.light = referenceMoby.light;
            this.color = referenceMoby.color;
            this.position = referenceMoby.position;
            this.rotation = referenceMoby.rotation;
            this.scale = referenceMoby.scale;
            this.drawDistance = referenceMoby.drawDistance;
            this.groupIndex = referenceMoby.groupIndex;
            this.pvarIndex = referenceMoby.pvarIndex;
            this.pVars = referenceMoby.pVars;
            this.isRooted = referenceMoby.isRooted;
            this.rootedDistance = referenceMoby.rootedDistance;
            this.mobyID = MAX_ID++;

            this.unk3B = referenceMoby.unk3B;
            this.occlusion = referenceMoby.occlusion;
            this.unk7A = referenceMoby.unk7A;
            this.unk8A = referenceMoby.unk8A;
            this.unk9 = referenceMoby.unk9;
            this.unk12A = referenceMoby.unk8A;

            UpdateTransformMatrix();
        }

        public Moby(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels, List<byte[]> pVars, bool fromMemory = false)
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

            if (this.mobyID >= MAX_ID)
            {
                MAX_ID = this.mobyID + 1;
            }

            if (this.pvarIndex != -1)
            {
                this.pVars = pVars[this.pvarIndex];
            }
            else
            {
                this.pVars = new byte[0];
            }
        }

        private void GetRC1Vals(GameType game, byte[] mobyBlock, int num, List<Model> mobyModels)
        {
            int offset = num * game.mobyElemSize;
            missionID = ReadInt(mobyBlock, offset + 0x04);
            spawnType = ReadInt(mobyBlock, offset + 0x08);
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
            isRooted = ReadInt(mobyBlock, offset + 0x4C);

            rootedDistance = ReadFloat(mobyBlock, offset + 0x50);
            unk12A = ReadShort(mobyBlock, offset + 0x54);
            unk12B = ReadShort(mobyBlock, offset + 0x56);
            pvarIndex = ReadInt(mobyBlock, offset + 0x58);
            occlusion = ReadInt(mobyBlock, offset + 0x5C) > 0;

            mode = ReadInt(mobyBlock, offset + 0x60);
            int r = ReadInt(mobyBlock, offset + 0x64);
            int g = ReadInt(mobyBlock, offset + 0x68);
            int b = ReadInt(mobyBlock, offset + 0x6C);

            light = ReadInt(mobyBlock, offset + 0x70);
            cutscene = ReadInt(mobyBlock, offset + 0x74);

            color = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
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
            spawnType = ReadInt(mobyBlock, offset + 0x0C);

            mobyID = ReadInt(mobyBlock, offset + 0x10);
            bolts = ReadInt(mobyBlock, offset + 0x14);
            unk3A = ReadShort(mobyBlock, offset + 0x18);
            unk3B = ReadShort(mobyBlock, offset + 0x1A);
            exp = ReadInt(mobyBlock, offset + 0x1C);

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
            isRooted = ReadInt(mobyBlock, offset + 0x5C);

            rootedDistance = ReadFloat(mobyBlock, offset + 0x60);
            unk12A = ReadShort(mobyBlock, offset + 0x64);
            unk12B = ReadShort(mobyBlock, offset + 0x66);
            pvarIndex = ReadInt(mobyBlock, offset + 0x68);
            occlusion = ReadInt(mobyBlock, offset + 0x6C) > 0;

            mode = ReadInt(mobyBlock, offset + 0x70);
            int r = ReadInt(mobyBlock, offset + 0x74);
            int g = ReadInt(mobyBlock, offset + 0x78);
            int b = ReadInt(mobyBlock, offset + 0x7C);

            light = ReadInt(mobyBlock, offset + 0x80);
            cutscene = ReadInt(mobyBlock, offset + 0x84);

            color = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
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
            mode = ReadInt(mobyBlock, offset + 0x58);
            int r = ReadInt(mobyBlock, offset + 0x5C);

            int g = ReadInt(mobyBlock, offset + 0x60);
            int b = ReadInt(mobyBlock, offset + 0x64);
            light = ReadInt(mobyBlock, offset + 0x68);
            unk9 = ReadInt(mobyBlock, offset + 0x6C);

            cutscene = 0;

            color = Color.FromRgb((byte) r, (byte) g, (byte) b).ToPixel<Rgb24>();
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
            WriteInt(buffer, 0x08, spawnType);
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
            WriteInt(buffer, 0x4C, isRooted);

            WriteFloat(buffer, 0x50, rootedDistance);
            WriteShort(buffer, 0x54, unk12A);
            WriteShort(buffer, 0x56, unk12B);
            WriteInt(buffer, 0x58, pvarIndex);
            WriteInt(buffer, 0x5C, occlusion ? 1 : 0);

            WriteInt(buffer, 0x60, mode);
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
            WriteInt(buffer, 0x0C, spawnType);

            WriteInt(buffer, 0x10, mobyID);
            WriteInt(buffer, 0x14, bolts);
            WriteShort(buffer, 0x18, unk3A);
            WriteShort(buffer, 0x1A, unk3B);
            WriteInt(buffer, 0x1C, exp);

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
            WriteInt(buffer, 0x5C, isRooted);

            WriteFloat(buffer, 0x60, rootedDistance);
            WriteShort(buffer, 0x64, unk12A);
            WriteShort(buffer, 0x66, unk12B);
            WriteInt(buffer, 0x68, pvarIndex);
            WriteInt(buffer, 0x6C, occlusion ? 1 : 0);

            WriteInt(buffer, 0x70, mode);
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
            WriteInt(buffer, 0x58, mode);
            WriteInt(buffer, 0x5C, color.R);

            WriteInt(buffer, 0x60, color.G);
            WriteInt(buffer, 0x64, color.B);
            WriteInt(buffer, 0x68, light);
            WriteInt(buffer, 0x6C, unk9);

            return buffer;
        }


        public override LevelObject Clone()
        {
            return new Moby(this);
        }

        public override void UpdateTransformMatrix()
        {
            Vector3 euler = rotation.ToEulerAngles();
            Matrix4 rotZ = Matrix4.CreateFromAxisAngle(Vector3.UnitZ, euler.Z);
            Matrix4 rotY = Matrix4.CreateFromAxisAngle(Vector3.UnitY, euler.Y);
            Matrix4 rotX = Matrix4.CreateFromAxisAngle(Vector3.UnitX, euler.X);
            Vector3 s = (model == null) ? scale : scale * model.size;
            Matrix4 scaleMatrix = Matrix4.CreateScale(s);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rotX * rotY * rotZ * translationMatrix;
        }

        public class IngameMobyMemory
        {
            public Vector4 collPos { get; set; }
            public Vector4 position { get; set; }
            public byte state { get; set; }
            public byte group { get; set; }
            public byte mClass { get; set; }
            public byte alpha { get; set; }
            public float scale { get; set; }
            public byte updateDistance { get; set; }
            public byte visible { get; set; }
            public short drawDistance { get; set; }
            public ushort modeBits { get; set; }
            public ushort unk36 { get; set; }
            public Rgb24 color { get; set; }
            public int light { get; set; }
            public Vector4 rotation { get; set; }
            public byte animationFrame { get; set; }
            public byte updateID { get; set; }
            public byte animationID { get; set; }
            public float unk54 { get; set; }
            public float unk58 { get; set; }
            public float framerate { get; set; }
            public uint pVars { get; set; }
            public byte unk7C { get; set; }
            public byte unk7D { get; set; }
            public byte unk7E { get; set; }
            public byte animState { get; set; }
            public uint unk80 { get; set; }
            public int unk84 { get; set; }
            public int unk88 { get; set; }
            public ushort oClass { get; set; }
            public ushort UID { get; set; }

            public IngameMobyMemory()
            {
            }

            public bool IsDead()
            {
                return (state & 0x80) > 0;
            }

            public void SetDead()
            {
                state |= 0x80;
            }

            public void UpdateRC1(byte[] memory, int offset)
            {
                float collX = ReadFloat(memory, offset + 0x00);
                float collY = ReadFloat(memory, offset + 0x04);
                float collZ = ReadFloat(memory, offset + 0x08);
                float collW = ReadFloat(memory, offset + 0x0C);

                float X = ReadFloat(memory, offset + 0x10);
                float Y = ReadFloat(memory, offset + 0x14);
                float Z = ReadFloat(memory, offset + 0x18);
                float W = ReadFloat(memory, offset + 0x1C);

                state = memory[offset + 0x20];
                group = memory[offset + 0x21];
                mClass = memory[offset + 0x22];
                alpha = memory[offset + 0x23];
                scale = ReadFloat(memory, offset + 0x2C);

                updateDistance = memory[offset + 0x30];
                visible = memory[offset + 0x31];
                drawDistance = ReadShort(memory, offset + 0x32);
                modeBits = ReadUshort(memory, offset + 0x34);
                unk36 = ReadUshort(memory, offset + 0x36);
                byte colorPadding = memory[offset + 0x38];
                byte blue = memory[offset + 0x39];
                byte green = memory[offset + 0x3A];
                byte red = memory[offset + 0x3B];
                light = ReadInt(memory, offset + 0x3C);

                float rotX = ReadFloat(memory, offset + 0x40);
                float rotY = ReadFloat(memory, offset + 0x44);
                float rotZ = ReadFloat(memory, offset + 0x48);
                float rotW = ReadFloat(memory, offset + 0x4C);

                animationFrame = memory[offset + 0x51];
                updateID = memory[offset + 0x52];
                animationID = memory[offset + 0x53];
                unk54 = ReadFloat(memory, offset + 0x54);
                unk58 = ReadFloat(memory, offset + 0x58);
                framerate = ReadFloat(memory, offset + 0x5C);

                pVars = ReadUint(memory, offset + 0x78);
                unk7C = memory[offset + 0x7C];
                unk7D = memory[offset + 0x7D];
                unk7E = memory[offset + 0x7E];
                animState = memory[offset + 0x7F];

                unk80 = ReadUint(memory, offset + 0x80);
                unk84 = ReadInt(memory, offset + 0x84);
                unk88 = ReadInt(memory, offset + 0x88);

                oClass = ReadUshort(memory, offset + 0xA6);

                UID = ReadUshort(memory, offset + 0xB2);

                collPos = new Vector4(collX, collY, collZ, collW);
                position = new Vector4(X, Y, Z, W);
                rotation = new Vector4(rotX, rotY, rotZ, rotW);
                color = Color.FromRgb((byte) red, (byte) green, (byte) blue).ToPixel<Rgb24>();

                // TODO: Understand this better, this is just a hack for now.
                if (oClass == 71 || oClass == 190 || oClass == 192 || oClass == 208 || oClass == 229)
                {
                    rotation = rotation + new Vector4(1.5707964f, 0.0f, 0.0f, 0.0f);
                }
            }

            public void UpdateRC23(byte[] memory, int offset)
            {
                float collX = ReadFloat(memory, offset + 0x00);
                float collY = ReadFloat(memory, offset + 0x04);
                float collZ = ReadFloat(memory, offset + 0x08);
                float collW = ReadFloat(memory, offset + 0x0C);

                float X = ReadFloat(memory, offset + 0x10);
                float Y = ReadFloat(memory, offset + 0x14);
                float Z = ReadFloat(memory, offset + 0x18);
                float W = ReadFloat(memory, offset + 0x1C);

                state = memory[offset + 0x20];
                group = memory[offset + 0x21];
                mClass = memory[offset + 0x22];
                alpha = memory[offset + 0x23];
                scale = ReadFloat(memory, offset + 0x2C);

                updateDistance = memory[offset + 0x30];
                visible = memory[offset + 0x31];
                drawDistance = ReadShort(memory, offset + 0x32);
                modeBits = ReadUshort(memory, offset + 0x34);
                unk36 = ReadUshort(memory, offset + 0x36);
                byte colorPadding = memory[offset + 0x38];
                byte blue = memory[offset + 0x39];
                byte green = memory[offset + 0x3A];
                byte red = memory[offset + 0x3B];
                light = ReadInt(memory, offset + 0x3C);

                animationFrame = memory[offset + 0x41];
                updateID = memory[offset + 0x42];
                animationID = memory[offset + 0x43];
                unk54 = ReadFloat(memory, offset + 0x44);
                unk58 = ReadFloat(memory, offset + 0x48);
                framerate = ReadFloat(memory, offset + 0x4C);

                oClass = ReadUshort(memory, offset + 0xAA);

                float rotX = ReadFloat(memory, offset + 0xF0);
                float rotY = ReadFloat(memory, offset + 0xF4);
                float rotZ = ReadFloat(memory, offset + 0xF8);
                float rotW = ReadFloat(memory, offset + 0xFC);

                collPos = new Vector4(collX, collY, -collZ, collW);
                position = new Vector4(X, Y, Z, W);
                rotation = new Vector4(rotX, rotY, rotZ, rotW);
                color = Color.FromRgb((byte) red, (byte) green, (byte) blue).ToPixel<Rgb24>();
            }
        }

        public void SetDead()
        {
            if (memory == null)
            {
                memory = new IngameMobyMemory();
            }

            memory.SetDead();
        }

        public void UpdateFromMemory(byte[] mobyMemory, int offset, List<Model> models)
        {
            if (memory == null)
            {
                memory = new IngameMobyMemory();
            }

            switch (game.num)
            {
                case 1:
                    memory.UpdateRC1(mobyMemory, offset);
                    break;
                case 2:
                case 3:
                    memory.UpdateRC23(mobyMemory, offset);
                    break;
                default:
                    return;
            }

            pVarMemoryAddress = 0x300000000 + memory.pVars;

            // If dead
            if (memory.IsDead())
            {
                model = null;
                return;
            }

            if (memory.oClass != modelID)
            {
                Model? mod = models.Find(x => x.id == memory.oClass);

                model = mod;
                modelID = memory.oClass;
            }

            float modelSize = (model != null) ? model.size : 1.0f;

            mobyID = memory.UID;
            groupIndex = memory.group;
            position = memory.position.Xyz;
            rotation = Quaternion.FromEulerAngles(memory.rotation.Xyz);
            scale = new Vector3(memory.scale / modelSize);
            color = memory.color;
            light = memory.light;

            UpdateTransformMatrix();
        }
    }
}
