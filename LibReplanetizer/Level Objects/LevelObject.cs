// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using System.ComponentModel;

namespace LibReplanetizer.LevelObjects
{
    public abstract class LevelObject : ITransformable, ISerializable
    {
        public static Vector4 NORMAL_COLOR = new Vector4(1, 1, 1, 1); // White
        public static Vector4 SELECTED_COLOR = new Vector4(1, 0, 1, 1); // Purple

        public Matrix4 modelMatrix = Matrix4.Identity;

        [Category("Attributes"), DisplayName("Position")]
        public Vector3 position { get; set; } = new Vector3();
        [Category("Attributes"), DisplayName("Scale")]
        public Vector3 scale { get; set; } = new Vector3();
        [Category("Attributes"), DisplayName("Rotation"), TypeConverter(typeof(Level_Objects.QuaternionTypeConverter))]
        public Quaternion rotation { get; set; } = new Quaternion();
        [Category("Attributes"), DisplayName("Reflection")]
        public Matrix4 reflection { get; set; } = Matrix4.Identity;


        public abstract byte[] ToByteArray();

        public abstract LevelObject Clone();

        // Virtual, since some objects (moby) override it
        public virtual void UpdateTransformMatrix()
        {
            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = reflection * scaleMatrix * rot * translationMatrix;
        }

        public virtual void SetFromMatrix(Matrix4 mat)
        {
            position = mat.ExtractTranslation();
            rotation = mat.ExtractRotation();
            scale = mat.ExtractScale();
            modelMatrix = mat;
        }

        public void Translate(Vector3 vector)
        {
            position += vector;
            UpdateTransformMatrix();
        }

        public void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z));
        }

        public void Rotate(Vector3 vector)
        {
            rotation *= Quaternion.FromEulerAngles(vector);
            UpdateTransformMatrix();
        }

        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }

        public void Scale(Vector3 scale)
        {
            this.scale *= scale;
            UpdateTransformMatrix();
        }

        public void Scale(float val)
        {
            // To uniformly scale object
            Scale(new Vector3(val));
        }

        public void Scale(float x, float y, float z)
        {
            Scale(new Vector3(x, y, z));
        }
    }
}
