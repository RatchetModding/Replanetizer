using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit {
    public static class Utilities {
        public static float fToDegrees(float radians) {
            return radians * 180 / (float)Math.PI;
        }
        public static float fToRadians(float angle) {
            return (float)Math.PI / 180 * angle;
        }
        public static float fRound(float value, float numberOfDecimals) {
            float multiplier = (float)Math.Pow(10, numberOfDecimals);
            return (float)Math.Ceiling(value * multiplier) / multiplier;
        }
        public static float fSin(float input) {
            return (float)Math.Sin(input);
        }
        public static float fCos(float input) {
            return (float)Math.Cos(input);
        }
    }

    public class Vector3Converter : TypeConverter
    {
        public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            try
            {
                string[] tokens = ((string)value).Split(' ');
                return new Vector3(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2]));
            }
            catch
            {
                return context.PropertyDescriptor.GetValue(context.Instance);
            }
        }

        public override object ConvertTo( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            Vector3 p = (Vector3)value;
            return Math.Round(p.X, 2) + " " + Math.Round(p.Y, 2) + " " + Math.Round(p.Z, 2);
        }
    }
}
