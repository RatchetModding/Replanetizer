using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static LibReplanetizer.DataFunctions;


namespace LibReplanetizer.LevelObjects
{
    public class Shrub : ModelObject
    {
        public const int ELEMENTSIZE = 0x70;

        [Category("Attributes"), DisplayName("Draw Distance")]
        public float drawDistance { get; set; }
        [Category("Unknowns"), DisplayName("OFF_58: Always 0")]
        public uint off_58 { get; set; }
        [Category("Unknowns"), DisplayName("OFF_5C: Always 0")]
        public uint off_5C { get; set; }

        // Seems to be some kind of static lighting color
        // Changing it to red will make the shrub be very red
        // the texture remains visible so cleary the visible
        // color is some blend between texture and this
        [Category("Attributes"), DisplayName("Static Color")]
        public Color color { get; set; }
        [Category("Unknowns"), DisplayName("OFF_64: Always 0")]
        public uint off_64 { get; set; }
        [Category("Attributes"), DisplayName("Light")]
        public ushort light { get; set; }
        [Category("Unknowns"), DisplayName("OFF_6C: Always 0")]
        public uint off_6C { get; set; }


        public Shrub(Matrix4 matrix4)
        {
            modelMatrix = Matrix4.Add(matrix4, new Matrix4());
        }

        public Shrub(byte[] levelBlock, int num, List<Model> shrubModels)
        {
            int offset = num * ELEMENTSIZE;

            modelMatrix = ReadMatrix4(levelBlock, offset + 0x00);

            /* These offsets are just placeholders for the render distance quaternion which is set in-game
            off_40 =    BAToUInt32(levelBlock, offset + 0x40);
            off_44 =    BAToUInt32(levelBlock, offset + 0x44);
            off_48 =    BAToUInt32(levelBlock, offset + 0x48);
            off_4C =    BAToUInt32(levelBlock, offset + 0x4C);
            */

            modelID = ReadInt(levelBlock, offset + 0x50);
            drawDistance = ReadFloat(levelBlock, offset + 0x54);
            off_58 = ReadUint(levelBlock, offset + 0x58);
            off_5C = ReadUint(levelBlock, offset + 0x5C);

            byte r = levelBlock[offset + 0x60];
            byte g = levelBlock[offset + 0x61];
            byte b = levelBlock[offset + 0x62];
            byte a = levelBlock[offset + 0x63];
            off_64 = ReadUint(levelBlock, offset + 0x64);
            light = ReadUshort(levelBlock, offset + 0x68);
            off_6C = ReadUint(levelBlock, offset + 0x6C);

            model = shrubModels.Find(shrubModel => shrubModel.id == modelID);
            color = Color.FromArgb(a, r, g, b);

            rotation = modelMatrix.ExtractRotation();
            position = modelMatrix.ExtractTranslation();
            scale = modelMatrix.ExtractScale();

            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 attributes = scaleMatrix * rot * translationMatrix;
            try
            {
                attributes.Invert();
            }
            catch
            {
                attributes = Matrix4.Identity;
            }

            reflection = modelMatrix * attributes;
        }

        public override byte[] ToByteArray()
        {
            var bytes = new byte[ELEMENTSIZE];

            WriteMatrix4(bytes, 0x00, modelMatrix);

            WriteInt(bytes, 0x50, modelID);
            WriteFloat(bytes, 0x54, drawDistance);
            WriteUint(bytes, 0x58, off_58);
            WriteUint(bytes, 0x5C, off_5C);

            bytes[0x60] = color.R;
            bytes[0x61] = color.G;
            bytes[0x62] = color.B;
            bytes[0x63] = color.A;
            WriteUint(bytes, 0x64, off_64);
            WriteUshort(bytes, 0x68, light);
            WriteUint(bytes, 0x6C, off_6C);

            return bytes;
        }

        public override LevelObject Clone()
        {
            return new Tie(modelMatrix);
        }
    }
}
