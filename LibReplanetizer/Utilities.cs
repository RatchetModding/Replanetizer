// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System;
using System.ComponentModel;
using SixLabors.ImageSharp;

namespace LibReplanetizer
{
    public class Bitmask
    {
        private int _v;

        public static implicit operator Bitmask(int value) => new Bitmask { _v = value };
        public static implicit operator int(Bitmask value) => value._v;
    }

    public static class Utilities
    {
        public static float ToDegreesF(float radians)
        {
            return radians * 180 / (float) Math.PI;
        }
        public static float ToRadiansF(float angle)
        {
            return (float) Math.PI / 180 * angle;
        }
        public static float RoundF(float value, float numberOfDecimals)
        {
            float multiplier = (float) Math.Pow(10, numberOfDecimals);
            return (float) Math.Ceiling(value * multiplier) / multiplier;
        }
        public static float SinF(float input)
        {
            return (float) Math.Sin(input);
        }
        public static float CosF(float input)
        {
            return (float) Math.Cos(input);
        }

        public static Vector3 MouseToWorldRay(Matrix4 projection, Matrix4 view, Size viewport, Vector2 mouse)
        {
            Vector3 pos1 = UnProject(ref projection, view, viewport, new Vector3(mouse.X, mouse.Y, 0.1f));      // near
            Vector3 pos2 = UnProject(ref projection, view, viewport, new Vector3(mouse.X, mouse.Y, 1024.0f));   // far
            return pos1 - pos2;
        }

        public static Vector3 UnProject(ref Matrix4 projection, Matrix4 view, Size viewport, Vector3 mouse)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / (float) viewport.Width - 1;
            vec.Y = -(2.0f * mouse.Y / (float) viewport.Height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            // TODO: validate this works
            Vector4.TransformRow(in vec, in projInv, out vec);
            Vector4.TransformRow(in vec, in viewInv, out vec);

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

        public static float PiClamp(float input)
        {
            float pi = MathF.PI;
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
        public byte r, g, b, a;

        public Pixel(byte[] input)
        {
            r = input[0];
            g = input[1];
            b = input[2];
            a = input[3];
        }

        public uint ToUInt32()
        {
            byte[] temp = new byte[] { r, g, b, a };
            return BitConverter.ToUInt32(temp, 0);
        }

        public override string ToString()
        {
            return r + ", " + g + ", " + b + ", " + a;
        }
    }

    public class Vector3Converter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            try
            {
                string[] tokens = ((string) value).Split(' ');
                return new Vector3(
                    float.Parse(tokens[0]),
                    float.Parse(tokens[1]),
                    float.Parse(tokens[2])
                );
            }
            catch
            {
                if (context == null)
                {
                    return new object();
                }
                object? o = context.PropertyDescriptor.GetValue(context.Instance);
                return (o == null) ? new object() : o;
            }
        }

        public override object ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value == null) return "0.0 0.0 0.0";

            Vector3 p = (Vector3) value;
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
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            try
            {
                string[] tokens = ((string) value).Split(' ');
                return new Vector3(
                    Utilities.ToRadiansF(float.Parse(tokens[0])),
                    Utilities.ToRadiansF(float.Parse(tokens[1])),
                    Utilities.ToRadiansF(float.Parse(tokens[2]))
                );
            }
            catch
            {
                if (context == null)
                {
                    return new object();
                }
                object? o = context.PropertyDescriptor.GetValue(context.Instance);
                return (o == null) ? new object() : o;
            }
        }

        public override object ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value == null) return "0.0 0.0 0.0";

            Vector3 p = (Vector3) value;
            return String.Format(
                "{0} {1} {2}",
                Math.Round(Utilities.ToDegreesF(p.X), 2),
                Math.Round(Utilities.ToDegreesF(p.Y), 2),
                Math.Round(Utilities.ToDegreesF(p.Z), 2)
            );
        }
    }
}
