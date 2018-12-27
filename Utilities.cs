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
        public static float toDegrees(float radians) {
            return radians * 180 / (float)Math.PI;
        }
        public static float toRadians(float angle) {
            return (float)Math.PI / 180 * angle;
        }
        public static float round(float value, float numberOfDecimals) {
            float multiplier = (float)Math.Pow(10, numberOfDecimals);
            return (float)Math.Ceiling(value * multiplier) / multiplier;
        }
    }

    public class Vector3Converter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
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

        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            Vector3 p = (Vector3)value;
            return Math.Round(p.X, 2) + " " + Math.Round(p.Y, 2) + " " + Math.Round(p.Z, 2);
        }
    }
}
