using OpenTK;
using System;
using System.ComponentModel;
using System.Drawing;

namespace LibReplanetizer
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

        public static Matrix3 GetRotationMatrix(Vector3 rotation)
        {
            return Matrix3.CreateRotationX(rotation.X) * Matrix3.CreateRotationY(rotation.Y) * Matrix3.CreateRotationZ(rotation.Z);
        }

        public static Matrix4 TranslateMatrixTo(Matrix4 sourceMatrix, Vector3 position)
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 sourceWithoutTranslation = sourceMatrix.ClearTranslation();
            sourceWithoutTranslation.M41 = position.X;
            sourceWithoutTranslation.M42 = position.Y;
            sourceWithoutTranslation.M43 = position.Z;
            return sourceWithoutTranslation;
        }

        public static float piClamp(float input)
        {
            float pi = (float)Math.PI;
            float val = input;
            while (val < -2 * pi)
            {
                val += 2 * pi;
            }
            while (val > 2 * pi)
            {
                val -= 2 * pi;
            }
            return val;
        }

        public static Matrix4 RotateMatrixTo(Matrix4 sourceMatrix, Vector3 rotation)
        {
            Matrix4 xrot = Matrix4.CreateRotationX(rotation.X);
            //Matrix4 yrot = Matrix4.CreateRotationY(rotation.Y);
            //Matrix4 zrot = Matrix4.CreateRotationZ(rotation.Z);


            //Matrix4 rotationMatrix = xrot * yrot * zrot;

            Matrix4 result = xrot * sourceMatrix.ClearRotation();
            return result;
        }

        public static Matrix4 ScaleMatrixTo(Matrix4 sourceMatrix, Vector3 scale)
        {
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 sourceWithoutScale = sourceMatrix.ClearScale();
            sourceWithoutScale.M11 = scale.X;
            sourceWithoutScale.M22 = scale.Y;
            sourceWithoutScale.M33 = scale.Z;
            //Matrix4 output = scaleMatrix * sourceWithoutScale;
            return sourceWithoutScale;
        }
    }

    public struct Pixel
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
                    float.Parse(tokens[0]),
                    float.Parse(tokens[1]),
                    float.Parse(tokens[2])
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
                Math.Round(p.X, 2),
                Math.Round(p.Y, 2),
                Math.Round(p.Z, 2)
            );
        }
    }
    public class Vector3RadiansConverter : TypeConverter
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
