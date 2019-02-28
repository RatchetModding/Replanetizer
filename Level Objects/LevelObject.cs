using System.ComponentModel;
using OpenTK;

namespace RatchetEdit.LevelObjects
{
    public abstract class LevelObject : ITransformable, ISerializable
    {
		protected static Vector4 normalColor = new Vector4(1, 1, 1, 1); // White
		protected static Vector4 selectedColor = new Vector4(1, 0, 1, 1); // Purple

        protected Vector3 _position = new Vector3();
        [Category("\tTransform"), TypeConverter(typeof(Vector3Converter)), DisplayName("Position")]
        public virtual Vector3 position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateTransformMatrix();
            }
        }
        protected Vector3 _rotation = new Vector3();
        [Category("\tTransform"), TypeConverter(typeof(Vector3RadiansConverter)), DisplayName("Rotation")]
        public virtual Vector3 rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                UpdateTransformMatrix();
            }
        }

        protected Vector3 _scale = new Vector3();
        [Category("\tTransform"), TypeConverter(typeof(Vector3Converter)), DisplayName("Scale")]
        public virtual Vector3 scale {
            get { return _scale; }
            set {
                _scale = value;
                UpdateTransformMatrix();
            }
        }


        public abstract byte[] ToByteArray();

        public abstract LevelObject Clone();
        public abstract void Rotate(Vector3 vector);
        public abstract void Translate(Vector3 vector);
        public abstract void Scale(Vector3 scale);
        public abstract void Render(CustomGLControl glControl, bool selected);

        public virtual void UpdateTransformMatrix() { } // Not required to implement


        public void Scale(float val) { //To uniformly scale object
            Scale(new Vector3(val, val, val));
        }
        public void Scale(float x, float y, float z) {
            Rotate(new Vector3(x, y, z));
        }
        public void Rotate(float x, float y, float z) {
            Rotate(new Vector3(x, y, z));
        }
        public void Translate(float x, float y, float z) {
            Translate(new Vector3(x, y, z));
        }

    }
}
