// Copyright (C) 2018-2023, The Replanetizer Contributors.
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

        [Category("Attributes"), DisplayName("Draw Distance"), Description("The distance at which an object will start fading out. After a distance of 32 or more units, the object will stop being drawn.")]
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
        public Moby(GameType game)
        {
            this.game = game;
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

            this.unk3A = referenceMoby.unk3A;
            this.unk3B = referenceMoby.unk3B;
            this.unk6 = referenceMoby.unk6;
            this.occlusion = referenceMoby.occlusion;
            this.unk7A = referenceMoby.unk7A;
            this.unk7B = referenceMoby.unk7B;
            this.unk8A = referenceMoby.unk8A;
            this.unk8B = referenceMoby.unk8B;
            this.unk9 = referenceMoby.unk9;
            this.unk12A = referenceMoby.unk12A;
            this.unk12B = referenceMoby.unk12B;
            this.exp = referenceMoby.exp;
            this.mode = referenceMoby.mode;

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
            WriteFloat(buffer, 0x3C, eulerAngles.Z);

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
            public struct AnimationLayer
            {
                public ushort unk0x00 { get; set; }
                public ushort boneCount { get; set; }
                public uint unk0x04 { get; set; }
                public float animationBlend { get; set; }
                public uint unk0x0C { get; set; }
                public uint pAnimation { get; set; }
                public uint unk0x14 { get; set; }
                public uint unk0x18 { get; set; }
                public uint pNextAnimationLayer { get; set; }
                public List<AnimationData> animationData { get; set; }
            }

            public struct AnimationData
            {
                public Vector4 rotation { get; set; }
                public Vector4 scale { get; set; }
                public Vector3 translation { get; set; }
                public uint boneIndex { get; set; }
            }

            public struct AnimationManipulator
            {
                public byte animationBone { get; set; }
                public byte state { get; set; }
                public byte scaleOn { get; set; }
                public byte absolute { get; set; }
                public ushort boneID { get; set; }
                public uint pNext { get; set; }
                public float animationBlend { get; set; }
                public Vector4 rotation { get; set; }
                public Vector4 scale { get; set; }
                public Vector4 translation { get; set; }
            }

            public sealed class RuntimeAnimationData
            {
                public RuntimeAnimationData(
                    float speed,
                    Quaternion[] rotations,
                    Vector3[] scalings,
                    bool[] hasScalings,
                    Vector3[] translations,
                    bool[] hasTranslations)
                {
                    this.speed = speed;
                    this.rotations = rotations;
                    this.scalings = scalings;
                    this.hasScalings = hasScalings;
                    this.translations = translations;
                    this.hasTranslations = hasTranslations;
                }

                public float speed { get; internal set; }
                public readonly Quaternion[] rotations;
                public readonly Vector3[] scalings;
                public readonly bool[] hasScalings;
                public readonly Vector3[] translations;
                public readonly bool[] hasTranslations;

                internal void CopyFrom(RuntimeAnimationData source)
                {
                    speed = source.speed;
                    Array.Copy(source.rotations, rotations, rotations.Length);
                    Array.Copy(source.scalings, scalings, scalings.Length);
                    Array.Copy(source.hasScalings, hasScalings, hasScalings.Length);
                    Array.Copy(source.translations, translations, translations.Length);
                    Array.Copy(source.hasTranslations, hasTranslations, hasTranslations.Length);
                }
            }

            private const int ANIMATION_LAYER_SIZE = 0x20;
            private const int ANIMATION_DATA_SIZE = 0x30;
            private const int ANIMATION_MANIPULATOR_SIZE = 0x40;

            public Vector4 collPos { get; set; }
            public Vector4 position { get; set; }
            public byte state { get; set; }
            public byte group { get; set; }
            public byte mClass { get; set; }
            public byte alpha { get; set; }
            public uint pClass { get; set; }
            public uint pChain { get; set; }
            public float scale { get; set; }
            public byte updateDistance { get; set; }
            public byte visible { get; set; }
            public short drawDistance { get; set; }
            public ushort modeBits { get; set; }
            public ushort unk36 { get; set; }
            public Rgb24 color { get; set; }
            public int light { get; set; }
            public Vector4 rotation { get; set; }
            public byte previousAnimationFrame { get; set; }
            public byte animationFrame { get; set; }
            public byte updateID { get; set; }
            public byte previousAnimationID { get; set; }
            public byte animationID { get; set; }
            public float animationBlend { get; set; }
            public float unk58 { get; set; }
            public float frameSpeed { get; set; }
            public uint pPreviousAnimationData { get; set; }
            public uint pCurrentAnimationData { get; set; }
            public RuntimeAnimationData? previousAnimationData { get; private set; }
            public RuntimeAnimationData? currentAnimationData { get; private set; }
            public uint pAnimationLayers { get; set; }
            public uint pManipulators { get; set; }
            public uint pUpdate { get; set; }
            public uint pVars { get; set; }
            public byte unk7C { get; set; }
            public byte unk7D { get; set; }
            public byte unk7E { get; set; }
            public byte shadow { get; set; }
            public uint unk80 { get; set; }
            public int unk84 { get; set; }
            public int unk88 { get; set; }
            public uint collCount { get; set; }
            public ushort oClass { get; set; }
            public ushort UID { get; set; }
            public Matrix3x4 transformation { get; set; }
            public List<AnimationLayer> animationLayers { get; } = new List<AnimationLayer>();
            public List<AnimationManipulator> manipulators { get; } = new List<AnimationManipulator>();

            private readonly byte[] animationLayerBuffer = new byte[ANIMATION_LAYER_SIZE];
            private readonly byte[] animationDataBuffer = new byte[ANIMATION_DATA_SIZE];
            private readonly byte[] manipulatorBuffer = new byte[ANIMATION_MANIPULATOR_SIZE];
            private readonly byte[] runtimeAnimationHeaderBuffer = new byte[0x10];
            private readonly HashSet<uint> visitedAddresses = new HashSet<uint>();
            private byte[] runtimeAnimationDataBuffer = Array.Empty<byte>();
            private RuntimeAnimationData? previousAnimationDataStorage;
            private RuntimeAnimationData? currentAnimationDataStorage;

            public IngameMobyMemory()
            {
            }

            public bool IsDead()
            {
                // TODO: moby->collCnt <= worldUpdateTime is also a necessary condition for dead mobies!
                return state > 0xFD;
            }

            public void SetDead()
            {
                state = 0xFE;
            }

            public void LoadFromMemory(
                GameType game,
                byte[] memory,
                int offset,
                Func<uint, byte[], bool>? readMemory = null)
            {
                switch (game.num)
                {
                    case 1:
                        UpdateRC1(memory, offset);
                        break;
                    case 2:
                    case 3:
                        UpdateRC23(memory, offset);
                        break;
                    default:
                        animationLayers.Clear();
                        manipulators.Clear();
                        return;
                }

                if (readMemory != null)
                {
                    LoadRuntimeAnimationData(readMemory);
                    LoadAnimationData(readMemory);
                }
                else
                {
                    previousAnimationData = null;
                    currentAnimationData = null;
                    animationLayers.Clear();
                    manipulators.Clear();
                }
            }

            public void CopyFrom(IngameMobyMemory source)
            {
                collPos = source.collPos;
                position = source.position;
                state = source.state;
                group = source.group;
                mClass = source.mClass;
                alpha = source.alpha;
                pClass = source.pClass;
                pChain = source.pChain;
                scale = source.scale;
                updateDistance = source.updateDistance;
                visible = source.visible;
                drawDistance = source.drawDistance;
                modeBits = source.modeBits;
                unk36 = source.unk36;
                color = source.color;
                light = source.light;
                rotation = source.rotation;
                previousAnimationFrame = source.previousAnimationFrame;
                animationFrame = source.animationFrame;
                previousAnimationID = source.previousAnimationID;
                updateID = source.updateID;
                animationID = source.animationID;
                animationBlend = source.animationBlend;
                unk58 = source.unk58;
                frameSpeed = source.frameSpeed;
                pPreviousAnimationData = source.pPreviousAnimationData;
                pCurrentAnimationData = source.pCurrentAnimationData;
                previousAnimationData = CopyRuntimeAnimationData(
                    source.previousAnimationData,
                    previousAnimationDataStorage);
                previousAnimationDataStorage = previousAnimationData ?? previousAnimationDataStorage;
                currentAnimationData = CopyRuntimeAnimationData(
                    source.currentAnimationData,
                    currentAnimationDataStorage);
                currentAnimationDataStorage = currentAnimationData ?? currentAnimationDataStorage;
                pAnimationLayers = source.pAnimationLayers;
                pManipulators = source.pManipulators;
                pUpdate = source.pUpdate;
                pVars = source.pVars;
                unk7C = source.unk7C;
                unk7D = source.unk7D;
                unk7E = source.unk7E;
                shadow = source.shadow;
                unk80 = source.unk80;
                unk84 = source.unk84;
                unk88 = source.unk88;
                collCount = source.collCount;
                oClass = source.oClass;
                UID = source.UID;
                transformation = source.transformation;

                int layerCount = source.animationLayers.Count;
                for (int i = 0; i < layerCount; i++)
                {
                    AnimationLayer sourceLayer = source.animationLayers[i];
                    List<AnimationData> targetAnimationData;
                    if (i < animationLayers.Count)
                    {
                        AnimationLayer targetLayer = animationLayers[i];
                        targetAnimationData = targetLayer.animationData;
                        targetAnimationData.Clear();
                    }
                    else
                    {
                        targetAnimationData = new List<AnimationData>();
                    }

                    targetAnimationData.AddRange(sourceLayer.animationData);
                    sourceLayer.animationData = targetAnimationData;
                    if (i < animationLayers.Count)
                    {
                        animationLayers[i] = sourceLayer;
                    }
                    else
                    {
                        animationLayers.Add(sourceLayer);
                    }
                }
                if (animationLayers.Count > layerCount)
                {
                    animationLayers.RemoveRange(layerCount, animationLayers.Count - layerCount);
                }
                manipulators.Clear();
                manipulators.AddRange(source.manipulators);
            }

            private static RuntimeAnimationData? CopyRuntimeAnimationData(
                RuntimeAnimationData? source,
                RuntimeAnimationData? destination)
            {
                if (source == null)
                {
                    return null;
                }

                if (destination == null || destination.rotations.Length != source.rotations.Length)
                {
                    destination = new RuntimeAnimationData(
                        source.speed,
                        new Quaternion[source.rotations.Length],
                        new Vector3[source.scalings.Length],
                        new bool[source.hasScalings.Length],
                        new Vector3[source.translations.Length],
                        new bool[source.hasTranslations.Length]);
                }

                destination.CopyFrom(source);
                return destination;
            }

            private void LoadAnimationLayers(Func<uint, byte[], bool> readMemory)
            {
                visitedAddresses.Clear();
                uint address = pAnimationLayers;
                int layerIndex = 0;

                while (address != 0 && visitedAddresses.Add(address))
                {
                    if (!readMemory(address, animationLayerBuffer)) break;

                    List<AnimationData> animationData;
                    if (layerIndex < animationLayers.Count)
                    {
                        animationData = animationLayers[layerIndex].animationData;
                        animationData.Clear();
                    }
                    else
                    {
                        animationData = new List<AnimationData>();
                    }

                    AnimationLayer layer = new AnimationLayer
                    {
                        unk0x00 = ReadUshort(animationLayerBuffer, 0x00),
                        boneCount = ReadUshort(animationLayerBuffer, 0x02),
                        unk0x04 = ReadUint(animationLayerBuffer, 0x04),
                        animationBlend = ReadFloat(animationLayerBuffer, 0x08),
                        unk0x0C = ReadUint(animationLayerBuffer, 0x0C),
                        pAnimation = ReadUint(animationLayerBuffer, 0x10),
                        unk0x14 = ReadUint(animationLayerBuffer, 0x14),
                        unk0x18 = ReadUint(animationLayerBuffer, 0x18),
                        pNextAnimationLayer = ReadUint(animationLayerBuffer, 0x1C),
                        animationData = animationData
                    };

                    for (int i = 0; i < layer.boneCount; i++)
                    {
                        uint animationDataAddress = layer.pAnimation + (uint) (i * ANIMATION_DATA_SIZE);
                        if (!readMemory(animationDataAddress, animationDataBuffer)) break;

                        layer.animationData.Add(new AnimationData
                        {
                            rotation = ReadVector4(animationDataBuffer, 0x00),
                            scale = ReadVector4(animationDataBuffer, 0x10),
                            translation = ReadVector3(animationDataBuffer, 0x20),
                            boneIndex = ReadUint(animationDataBuffer, 0x2C)
                        });
                    }

                    if (layerIndex < animationLayers.Count)
                    {
                        animationLayers[layerIndex] = layer;
                    }
                    else
                    {
                        animationLayers.Add(layer);
                    }
                    layerIndex++;
                    address = layer.pNextAnimationLayer;
                }

                if (animationLayers.Count > layerIndex)
                {
                    animationLayers.RemoveRange(layerIndex, animationLayers.Count - layerIndex);
                }
            }

            private RuntimeAnimationData? ReadRuntimeAnimationData(
                Func<uint, byte[], bool> readMemory,
                uint address,
                RuntimeAnimationData? existing)
            {
                if (address == 0 || !readMemory(address, runtimeAnimationHeaderBuffer))
                {
                    return null;
                }

                ushort scalingDataOffset = ReadUshort(runtimeAnimationHeaderBuffer, 0x08);
                ushort scalingCount = ReadUshort(runtimeAnimationHeaderBuffer, 0x0A);
                ushort translationDataOffset = ReadUshort(runtimeAnimationHeaderBuffer, 0x0C);
                ushort translationCount = ReadUshort(runtimeAnimationHeaderBuffer, 0x0E);
                int declaredDataSize = ReadUshort(runtimeAnimationHeaderBuffer, 0x06) * 0x10;
                int scalingDataEnd = scalingDataOffset + scalingCount * 0x08;
                int translationDataEnd = translationDataOffset + translationCount * 0x08;
                int frameDataSize = Math.Max(declaredDataSize, Math.Max(scalingDataEnd, translationDataEnd));
                int rotationCount = scalingDataOffset / 0x08;

                if (scalingDataOffset % 0x08 != 0
                    || frameDataSize < 0x00
                    || frameDataSize > 0x10000
                    || scalingDataEnd > frameDataSize
                    || translationDataEnd > frameDataSize)
                {
                    return null;
                }

                if (runtimeAnimationDataBuffer.Length != frameDataSize)
                {
                    runtimeAnimationDataBuffer = new byte[frameDataSize];
                }

                if (!readMemory(address + 0x10, runtimeAnimationDataBuffer))
                {
                    return null;
                }

                RuntimeAnimationData? result = existing;
                if (result == null || result.rotations.Length != rotationCount)
                {
                    result = new RuntimeAnimationData(
                        0.0f,
                        new Quaternion[rotationCount],
                        new Vector3[rotationCount],
                        new bool[rotationCount],
                        new Vector3[rotationCount],
                        new bool[rotationCount]);
                }

                for (int i = 0; i < rotationCount; i++)
                {
                    int offset = i * 0x08;
                    result.rotations[i] = new Quaternion(
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x00) / 32768.0f,
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x02) / 32768.0f,
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x04) / 32768.0f,
                        -ReadShort(runtimeAnimationDataBuffer, offset + 0x06) / 32768.0f);
                }

                Array.Clear(result.scalings, 0, result.scalings.Length);
                Array.Clear(result.hasScalings, 0, result.hasScalings.Length);
                for (int i = 0; i < scalingCount; i++)
                {
                    int offset = scalingDataOffset + i * 0x08;
                    int bone = runtimeAnimationDataBuffer[offset + 0x06];
                    if (bone >= result.scalings.Length) continue;

                    result.scalings[bone] += new Vector3(
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x00) / 4096.0f,
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x02) / 4096.0f,
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x04) / 4096.0f);
                    result.hasScalings[bone] = true;
                }

                Array.Clear(result.translations, 0, result.translations.Length);
                Array.Clear(result.hasTranslations, 0, result.hasTranslations.Length);
                for (int i = 0; i < translationCount; i++)
                {
                    int offset = translationDataOffset + i * 0x08;
                    int bone = runtimeAnimationDataBuffer[offset + 0x06];
                    if (bone >= result.translations.Length) continue;

                    result.translations[bone] += new Vector3(
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x00) / 1024.0f,
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x02) / 1024.0f,
                        ReadShort(runtimeAnimationDataBuffer, offset + 0x04) / 1024.0f);
                    result.hasTranslations[bone] = true;
                }

                result.speed = ReadFloat(runtimeAnimationHeaderBuffer, 0x00);
                return result;
            }

            private void LoadRuntimeAnimationData(Func<uint, byte[], bool> readMemory)
            {
                previousAnimationData = ReadRuntimeAnimationData(
                    readMemory,
                    pPreviousAnimationData,
                    previousAnimationDataStorage);
                previousAnimationDataStorage = previousAnimationData ?? previousAnimationDataStorage;
                currentAnimationData = ReadRuntimeAnimationData(
                    readMemory,
                    pCurrentAnimationData,
                    currentAnimationDataStorage);
                currentAnimationDataStorage = currentAnimationData ?? currentAnimationDataStorage;
            }

            private void LoadManipulators(Func<uint, byte[], bool> readMemory)
            {
                visitedAddresses.Clear();
                uint address = pManipulators;

                while (address != 0 && visitedAddresses.Add(address))
                {
                    if (!readMemory(address, manipulatorBuffer)) break;

                    manipulators.Add(new AnimationManipulator
                    {
                        animationBone = manipulatorBuffer[0x00],
                        state = manipulatorBuffer[0x01],
                        scaleOn = manipulatorBuffer[0x02],
                        absolute = manipulatorBuffer[0x03],
                        boneID = ReadUshort(manipulatorBuffer, 0x06),
                        pNext = ReadUint(manipulatorBuffer, 0x08),
                        animationBlend = ReadFloat(manipulatorBuffer, 0x0C),
                        rotation = ReadVector4(manipulatorBuffer, 0x10),
                        scale = ReadVector4(manipulatorBuffer, 0x20),
                        translation = ReadVector4(manipulatorBuffer, 0x30)
                    });
                    address = ReadUint(manipulatorBuffer, 0x08);
                }
            }

            public void LoadAnimationData(Func<uint, byte[], bool> readMemory)
            {
                LoadAnimationLayers(readMemory);
                manipulators.Clear();
                LoadManipulators(readMemory);
            }

            private static Vector4 ReadVector4(byte[] memory, int offset)
            {
                return new Vector4(
                    ReadFloat(memory, offset + 0x00),
                    ReadFloat(memory, offset + 0x04),
                    ReadFloat(memory, offset + 0x08),
                    ReadFloat(memory, offset + 0x0C));
            }

            private static Vector3 ReadVector3(byte[] memory, int offset)
            {
                return new Vector3(
                    ReadFloat(memory, offset + 0x00),
                    ReadFloat(memory, offset + 0x04),
                    ReadFloat(memory, offset + 0x08));
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
                pClass = ReadUint(memory, offset + 0x24);
                pChain = ReadUint(memory, offset + 0x28);
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

                previousAnimationFrame = memory[offset + 0x50];
                animationFrame = memory[offset + 0x51];
                updateID = memory[offset + 0x52];
                animationID = memory[offset + 0x53];
                previousAnimationID = memory[offset + 0xA5];
                animationBlend = ReadFloat(memory, offset + 0x54);
                unk58 = ReadFloat(memory, offset + 0x58);
                frameSpeed = ReadFloat(memory, offset + 0x5C);

                pAnimationLayers = ReadUint(memory, offset + 0x60);
                pManipulators = ReadUint(memory, offset + 0x64);
                pPreviousAnimationData = ReadUint(memory, offset + 0x68);
                pCurrentAnimationData = ReadUint(memory, offset + 0x6C);

                pUpdate = ReadUint(memory, offset + 0x74);
                pVars = ReadUint(memory, offset + 0x78);
                unk7C = memory[offset + 0x7C];
                unk7D = memory[offset + 0x7D];
                unk7E = memory[offset + 0x7E];
                shadow = memory[offset + 0x7F];

                unk80 = ReadUint(memory, offset + 0x80);
                unk84 = ReadInt(memory, offset + 0x84);
                unk88 = ReadInt(memory, offset + 0x88);

                collCount = ReadUint(memory, offset + 0xA0);
                oClass = ReadUshort(memory, offset + 0xA6);

                UID = ReadUshort(memory, offset + 0xB2);

                transformation = ReadMatrix3x4(memory, offset + 0xC0);

                collPos = new Vector4(collX, collY, collZ, collW);
                position = new Vector4(X, Y, Z, W);
                rotation = new Vector4(rotX, rotY, rotZ, rotW);
                color = Color.FromRgb((byte) red, (byte) green, (byte) blue).ToPixel<Rgb24>();

                if (updateID == byte.MaxValue)
                    Utilities.DebugAssert(pPreviousAnimationData == 0x00A2C5C0u + previousAnimationFrame * 0x800, "Pointer should have originated from cache!");
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

                previousAnimationFrame = memory[offset + 0x40];
                animationFrame = memory[offset + 0x41];
                updateID = memory[offset + 0x42];
                animationID = memory[offset + 0x43];
                previousAnimationID = memory[offset + 0xA9];
                animationBlend = ReadFloat(memory, offset + 0x44);
                unk58 = ReadFloat(memory, offset + 0x48);
                frameSpeed = ReadFloat(memory, offset + 0x4C);

                pAnimationLayers = ReadUint(memory, offset + 0x50);
                pManipulators = ReadUint(memory, offset + 0x54);
                pPreviousAnimationData = ReadUint(memory, offset + 0x58);
                pCurrentAnimationData = ReadUint(memory, offset + 0x5C);

                collCount = ReadUint(memory, offset + 0xA0);
                oClass = ReadUshort(memory, offset + 0xAA);

                UID = ReadUshort(memory, offset + 0xB2);

                transformation = ReadMatrix3x4(memory, offset + 0xC0);

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

            memory.animationLayers.Clear();
            memory.manipulators.Clear();
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

            ApplyMemoryState(models);
        }

        public void ApplyMemory(IngameMobyMemory snapshot, List<Model> models)
        {
            if (memory == null)
            {
                memory = new IngameMobyMemory();
            }

            memory.CopyFrom(snapshot);
            ApplyMemoryState(models);
        }

        private void ApplyMemoryState(List<Model> models)
        {
            if (memory == null) return;

            pVarMemoryAddress = 0x300000000 + memory.pVars;

            // If dead
            if (memory.IsDead())
            {
                memory.animationLayers.Clear();
                memory.manipulators.Clear();
                model = null;
                modelID = -1;
                return;
            }

            if (memory.oClass != modelID || model == null)
            {
                Model? mod = models.Find(x => x.id == memory.oClass);

                model = mod;
                modelID = memory.oClass;
            }

            mobyID = memory.UID;
            groupIndex = memory.group;
            color = memory.color;
            light = memory.light;

            modelMatrix.M11 = memory.transformation.M11 * memory.scale;
            modelMatrix.M12 = memory.transformation.M12 * memory.scale;
            modelMatrix.M13 = memory.transformation.M13 * memory.scale;
            modelMatrix.M14 = 0.0f;
            modelMatrix.M21 = memory.transformation.M21 * memory.scale;
            modelMatrix.M22 = memory.transformation.M22 * memory.scale;
            modelMatrix.M23 = memory.transformation.M23 * memory.scale;
            modelMatrix.M24 = 0.0f;
            modelMatrix.M31 = memory.transformation.M31 * memory.scale;
            modelMatrix.M32 = memory.transformation.M32 * memory.scale;
            modelMatrix.M33 = memory.transformation.M33 * memory.scale;
            modelMatrix.M34 = 0.0f;
            modelMatrix.M41 = memory.position.X;
            modelMatrix.M42 = memory.position.Y;
            modelMatrix.M43 = memory.position.Z;
            modelMatrix.M44 = 1.0f;

            position = modelMatrix.ExtractTranslation();
            rotation = modelMatrix.ExtractRotation();
            scale = modelMatrix.ExtractScale();
        }
    }
}
