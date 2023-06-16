// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Replanetizer.Renderer
{
    public class GLUniform
    {
        private int location;


        private Matrix4 matrix4Value = new Matrix4(float.NaN, float.NaN, float.NaN, float.NaN,
                                                    float.NaN, float.NaN, float.NaN, float.NaN,
                                                    float.NaN, float.NaN, float.NaN, float.NaN,
                                                    float.NaN, float.NaN, float.NaN, float.NaN);
        private int intValue = Int32.MaxValue;
        private float floatValue = float.NaN;
        private Vector3 vec3Value = new Vector3(float.NaN);
        private Vector4 vec4Value = new Vector4(float.NaN);

        public GLUniform(int location)
        {
            this.location = location;
        }

        public int GetLocation()
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMatrix4(ref Matrix4 value)
        {
            if (!Matrix4.Equals(matrix4Value, value))
            {
                GL.UniformMatrix4(location, false, ref value);
                matrix4Value = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMatrix4(int count, float[] value)
        {
            GL.UniformMatrix4(location, count, false, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMatrix4(int count, ref float value)
        {
            GL.UniformMatrix4(location, count, false, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set1(int value)
        {
            if (intValue != value)
            {
                GL.Uniform1(location, value);
                intValue = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set1(float value)
        {
            if (floatValue != value)
            {
                GL.Uniform1(location, value);
                floatValue = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set3(Vector3 value)
        {
            if (!Vector3.Equals(vec3Value, value))
            {
                GL.Uniform3(location, value);
                vec3Value = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set3(float v0, float v1, float v2)
        {
            Vector3 value = new Vector3(v0, v1, v2);
            Set3(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set4(Vector4 value)
        {
            if (!Vector4.Equals(vec4Value, value))
            {
                GL.Uniform4(location, value);
                vec4Value = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set4(Color color)
        {
            Vector4 value = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            Set4(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set4(float v0, float v1, float v2, float v3)
        {
            Vector4 value = new Vector4(v0, v1, v2, v3);
            Set4(value);
        }
    }
}
