// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System;
using System.ComponentModel;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.LevelObjects
{
    public class SoundInstance : LevelObject
    {
        //Looks like 0C can be some sort of trigger that is tripped off when you go near them. They're generally placed in rivers with current and in front of unlockable doors.
        public const int ELEMENTSIZE = 0x90;

        [Category("Attributes"), DisplayName("ID")]
        public ushort id { get; set; }
        [Category("Attributes"), DisplayName("ID2")]
        public ushort id2 { get; set; }
        [Category("Attributes"), DisplayName("Function Pointer")]
        public int functionPointer { get; set; }
        [Category("Attributes"), DisplayName("Pvar Index")]
        public int pvarIndex { get; set; }
        [Category("Attributes"), DisplayName("Update Distance")]
        public float updateDistance { get; set; }

        public SoundInstance(byte[] block, int num)
        {
            int offset = num * ELEMENTSIZE;
            id = ReadUshort(block, offset + 0x00);
            id2 = ReadUshort(block, offset + 0x02);
            functionPointer = ReadInt(block, offset + 0x04);
            pvarIndex = ReadInt(block, offset + 0x08);
            updateDistance = ReadFloat(block, offset + 0x0C);

            Matrix4 transformMatrix = ReadMatrix4(block, offset + 0x10);
            Matrix4 inverseRotationMatrix = ReadMatrix4(block, offset + 0x50);

            modelMatrix = transformMatrix;
            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();
            UpdateTransformMatrix();
        }

        public override byte[] ToByteArray()
        {
            byte[] bytes = new byte[ELEMENTSIZE];

            WriteUshort(bytes, 0x00, id);
            WriteUshort(bytes, 0x02, id2);
            WriteInt(bytes, 0x04, functionPointer);
            WriteInt(bytes, 0x08, pvarIndex);
            WriteFloat(bytes, 0x0C, updateDistance);

            WriteMatrix4(bytes, 0x10, modelMatrix);
            WriteMatrix4(bytes, 0x50, Matrix4.CreateFromQuaternion(rotation).Inverted());

            return bytes;
        }

        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsDynamic()
        {
            return false;
        }
    }
}
