using System;
using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;

namespace Replanetizer.Utils
{
    public class Camera : ITransformable
    {
        //Camera variables
        public float speed = 0.2f;
        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3(0, 0, -0.75f);

        public float fovy { get; set; } = (float) Math.PI / 3;
        public float aspect { get; set; } = (float) 16 / 9;
        public float near { get; set; } = 0.1f;
        public float far { get; set; } = 10000.0f;

        /// <summary>
        /// The frustum is defined by 6 planes which are each defined by a point and a normal.
        /// The order of the planes is Near, Far, Right, Left, Up, Down.
        /// </summary>
        public Vector3[] frustumPlanePoints;
        public Vector3[] frustumPlaneNormals;

        private Matrix4 projectionMatrix;

        public Matrix3 GetRotationMatrix()
        {
            return Matrix3.CreateRotationX(rotation.X) * Matrix3.CreateRotationY(rotation.Y) * Matrix3.CreateRotationZ(rotation.Z);
        }

        private static Vector3 LegacyTransform(Vector3 vec, Matrix3 mat)
        {
            Vector3 result = new Vector3();
            result.X = vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X;
            result.Y = vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y;
            result.Z = vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z;
            return result;
        }

        public Matrix4 GetViewMatrix()
        {
            Vector3 forward = LegacyTransform(Vector3.UnitY, GetRotationMatrix());
            return Matrix4.LookAt(position, position + forward, Vector3.UnitZ);
        }

        public void ComputeProjectionMatrix()
        {
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, near, far);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return projectionMatrix;
        }

        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void SetRotation(float pitch, float yaw)
        {
            SetRotation(new Vector3(pitch, 0, yaw));
        }
        public void SetRotation(Vector3 rotation)
        {
            this.rotation = rotation;
        }

        public void MoveBehind(LevelObject levelObject, float distanceToObject = 5)
        {
            if (levelObject == null) return;

            float yaw = 0;

            // If object is moby, load its rotation.
            if (levelObject is Moby moby)
            {
                yaw = moby.rotation.Z;
                if (double.IsNaN(yaw)) yaw = 0;
            }

            yaw = yaw - (float) Math.PI / 2;
            SetRotation(-(float) Math.PI / 10, yaw);

            float ypos = (float) -Math.Cos(yaw);
            float xpos = (float) Math.Sin(yaw);

            SetPosition(new Vector3(
                levelObject.position.X + xpos * distanceToObject,
                levelObject.position.Y + ypos * distanceToObject,
                levelObject.position.Z + distanceToObject / 2
            ));
        }

        public void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z));
        }

        public void Translate(Vector3 vector)
        {
            position += vector;
        }

        public void TransformedTranslate(Vector3 vector)
        {
            position += LegacyTransform(vector, GetRotationMatrix());
        }

        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }

        public void Rotate(Vector3 vector)
        {
            rotation += vector;
        }

        public void Scale(Vector3 scale)
        {
            //Not used
        }

        public void Scale(float x, float y, float z)
        {
            //Not used
        }

        public void ComputeFrustum()
        {
            frustumPlanePoints = new Vector3[6];
            frustumPlaneNormals = new Vector3[6];

            float Hfar = 2.0f * MathF.Tan(fovy * 0.5f) * far;
            float Wfar = Hfar * aspect;

            Vector3 forward = LegacyTransform(Vector3.UnitY, GetRotationMatrix());
            forward.Normalize();

            Vector3 up = Vector3.UnitZ;
            Vector3 right = Vector3.Cross(forward, up);

            Vector3 nc = position + forward * near;
            frustumPlanePoints[0] = nc;
            frustumPlaneNormals[0] = forward;

            Vector3 fc = position + forward * far;
            frustumPlanePoints[1] = fc;
            frustumPlaneNormals[1] = -forward;

            Vector3 a = (fc + right * Wfar * 0.5f) - position;
            a.Normalize();
            frustumPlanePoints[2] = position;
            frustumPlaneNormals[2] = Vector3.Cross(up, a);

            Vector3 b = (fc - right * Wfar * 0.5f) - position;
            b.Normalize();
            frustumPlanePoints[3] = position;
            frustumPlaneNormals[3] = Vector3.Cross(b, up);

            Vector3 c = (fc + up * Hfar * 0.5f) - position;
            c.Normalize();
            frustumPlanePoints[4] = position;
            frustumPlaneNormals[4] = Vector3.Cross(c, right);

            Vector3 d = (fc - up * Hfar * 0.5f) - position;
            d.Normalize();
            frustumPlanePoints[5] = position;
            frustumPlaneNormals[5] = Vector3.Cross(right, d);

        }
    }
}
