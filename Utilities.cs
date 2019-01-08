using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public static class Utilities
    {
        public static float fToDegrees(float radians)
        {
            return radians * 180 / (float)Math.PI;
        }
        public static float fToRadians(float angle)
        {
            return (float)Math.PI / 180 * angle;
        }
        public static float fRound(float value, float numberOfDecimals)
        {
            float multiplier = (float)Math.Pow(10, numberOfDecimals);
            return (float)Math.Ceiling(value * multiplier) / multiplier;
        }
        public static float fSin(float input)
        {
            return (float)Math.Sin(input);
        }
        public static float fCos(float input)
        {
            return (float)Math.Cos(input);
        }

        public static Vector3 MouseToWorldRay(Matrix4 projection, Matrix4 view, Size viewport, Vector2 mouse)
        {
            Vector3 pos1 = UnProject(ref projection, view, viewport, new Vector3(mouse.X, mouse.Y, 0.1f)); // near
            Vector3 pos2 = UnProject(ref projection, view, viewport, new Vector3(mouse.X, mouse.Y, 800f));  // far
            return pos1 - pos2;
        }

        public static Vector3 UnProject(ref Matrix4 projection, Matrix4 view, Size viewport, Vector3 mouse)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / (float)viewport.Width - 1;
            vec.Y = -(2.0f * mouse.Y / (float)viewport.Height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < -float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return new Vector3(vec.X, vec.Y, vec.Z);
        }
    }

    struct Pixel
    {
        public byte R, G, B, A;

        public Pixel(byte[] input)
        {
            R = input[0];
            G = input[1];
            B = input[2];
            A = input[3];
        }

        public uint ToUInt32()
        {
            byte[] temp = new byte[] { R, G, B, A };
            return BitConverter.ToUInt32(temp, 0);
        }

        public override string ToString()
        {
            return R + ", " + G + ", " + B + ", " + A;
        }
    }

    public class Vector3Converter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            try
            {
                string[] tokens = ((string)value).Split(' ');
                return new Vector3(
                    Utilities.fToRadians(float.Parse(tokens[0])),
                    Utilities.fToRadians(float.Parse(tokens[1])),
                    Utilities.fToRadians(float.Parse(tokens[2]))
                );
            }
            catch
            {
                return context.PropertyDescriptor.GetValue(context.Instance);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            Vector3 p = (Vector3)value;
            return String.Format(
                "{0} {1} {2}",
                Math.Round(Utilities.fToDegrees(p.X), 2),
                Math.Round(Utilities.fToDegrees(p.Y), 2),
                Math.Round(Utilities.fToDegrees(p.Z), 2)
            );
        }
    }
}
