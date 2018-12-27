using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public abstract class LevelObject : Transformable
    {
        private Vector3 _position = new Vector3();
        public Vector3 position {
            get { return _position; }
            set {
                _position = value;
                updateTransform();
            }
        }

        public abstract LevelObject Clone();

        public abstract void Rotate(float x, float y, float z);
        public abstract void Rotate(Vector3 vector);
        public abstract void Scale(float scale);
        public abstract void Translate(float x, float y, float z);
        public abstract void Translate(Vector3 vector);
        public abstract void updateTransform();

    }
}
