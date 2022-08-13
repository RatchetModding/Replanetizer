// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

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
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == null) return false;

            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Quaternion q)
            {
                Vector3 v = new Vector3(
                    x: (float) Math.Atan2(2.0 * (q.W * q.X + q.Y * q.Z), 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y)),
                    y: (float) Math.Asin(2.0 * (q.W * q.Y - q.Z * q.X)),
                    z: (float) Math.Atan2(2.0 * (q.W * q.Z + q.X * q.Y), 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z)));
                return "(" + v.X + ". " + v.Y + ". " + v.Z + ")";
            }
            object? o = base.ConvertTo(context, culture, value, destinationType);

            return (o == null) ? new object() : o;
        }
    }
}
