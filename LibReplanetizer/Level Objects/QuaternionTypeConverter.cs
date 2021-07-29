using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace LibReplanetizer.Level_Objects
{
    class QuaternionTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Quaternion)
            {
                Quaternion q = (Quaternion)value;
                Vector3 v = new Vector3(
                    x: (float)Math.Atan2(2.0 * (q.W * q.X + q.Y * q.Z), 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y)),
                    y: (float)Math.Asin(2.0 * (q.W * q.Y - q.Z * q.X)),
                    z: (float)Math.Atan2(2.0 * (q.W * q.Z + q.X * q.Y), 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z)));
                return "(" + v.X + ". " + v.Y + ". " + v.Z + ")";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
