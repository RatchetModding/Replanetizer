using OpenTK;

namespace LibReplanetizer.LevelObjects
{
    public interface ITransformable
    {
        void Translate(float x, float y, float z);
        void Translate(Vector3 vector);

        void Rotate(float x, float y, float z);
        void Rotate(Vector3 vector);

        void Scale(Vector3 scale);
        void Scale(float x, float y, float z);
    }
}
