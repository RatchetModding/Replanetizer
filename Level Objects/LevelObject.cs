﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public abstract class LevelObject : ITransformable
    {
        protected Vector3 _position = new Vector3();
        [CategoryAttribute("Transform"), TypeConverter(typeof(Vector3Converter)), DisplayName("Position")]
        public virtual Vector3 position {
            get { return _position; }
            set {
                _position = value;
                UpdateTransformMatrix();
            }
        }
        protected Vector3 _rotation = new Vector3();
        [CategoryAttribute("Transform"), TypeConverter(typeof(Vector3Converter)), DisplayName("Rotation")]
        public virtual Vector3 rotation {
            get { return _rotation; }
            set {
                _rotation = value;
                UpdateTransformMatrix();
            }
        }

        protected float _scale = 1;
        [CategoryAttribute("Transform"), DisplayName("Scale")]
        public virtual float scale {
            get { return _scale; }
            set {
                _scale = value;
                UpdateTransformMatrix();
            }
        }


        public abstract LevelObject Clone();

        public abstract void Rotate(float x, float y, float z);
        public abstract void Rotate(Vector3 vector);
        public abstract void Scale(float scale);
        public abstract void Translate(float x, float y, float z);
        public abstract void Translate(Vector3 vector);
        public abstract void UpdateTransformMatrix();

    }
}
