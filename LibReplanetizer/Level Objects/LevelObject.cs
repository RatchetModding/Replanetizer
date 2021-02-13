using OpenTK;

namespace LibReplanetizer.LevelObjects
{
    public abstract class LevelObject : ITransformable, ISerializable
    {
        public static Vector4 normalColor = new Vector4(1, 1, 1, 1); // White
        public static Vector4 selectedColor = new Vector4(1, 0, 1, 1); // Purple

        public Matrix4 modelMatrix;


        public Vector3 position = new Vector3();
        public Vector3 scale = new Vector3();
        public Quaternion rotation = new Quaternion();


        public abstract byte[] ToByteArray();

        public abstract LevelObject Clone();

        // Virtual, since some objects (moby) override it
        public virtual void UpdateTransformMatrix()
        {
            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            modelMatrix = scaleMatrix * rot * translationMatrix;
        }

        public void Translate(Vector3 vector)
        {
            position += vector;
            UpdateTransformMatrix();
        }

        public void Rotate(Vector3 vector)
        {
            rotation *= Quaternion.FromEulerAngles(vector);
            UpdateTransformMatrix();
        }

        public void Scale(Vector3 scale)
        {
            this.scale *= scale;
            UpdateTransformMatrix();
        }


        public void Scale(float val)
        { //To uniformly scale object
            Scale(new Vector3(val));
        }
        public void Scale(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }
        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }
        public void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z));
        }

    }
}
