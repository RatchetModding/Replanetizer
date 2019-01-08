using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public class MatrixObject : LevelObject
    {
        [Browsable(false)]
        public Matrix4 modelMatrix { get; set; }

        public override Vector3 position
        {
            get { return _position; }
            set
            {
                Translate(value - _position);
            }
        }
        public override Vector3 rotation
        {
            get { return _rotation; }
            set
            {
                Rotate(value - _rotation);
            }
        }

        public override float scale
        {
            get { return _scale; }
            set
            {
                Scale(value);
            }
        }


        public override LevelObject Clone()
        {
            throw new NotImplementedException();
        }

        public override void Render(CustomGLControl glControl, bool selected)
        {
        }

        void UpdateMatrixVariables(Matrix4 matrix)
        {
            modelMatrix = matrix;
        }

        public override void UpdateTransformMatrix()
        {
            //modelMatrix = new Matrix4(v1x, v1y, v1z, v1w, v2x, v2y, v2z, v2w, v3x, v3y, v3z, v3w, x, y, z, w);
        }

        //Transformable methods
        public override void Translate(float x, float y, float z)
        {
            Vector3 rot = new Vector3(rotation);
            Rotate(-rot); //Rotate to 0,0,0 to do translation in world space.

            Matrix4 translationMatrix = Matrix4.CreateTranslation(x, y, z);
            Matrix4 result = translationMatrix * modelMatrix;
            _position = result.ExtractTranslation();
            UpdateMatrixVariables(result);
            UpdateTransformMatrix();

            Rotate(rot); //Rotate back to keep orientation

        }

        public override void Translate(Vector3 vector)
        {
            Translate(vector.X, vector.Y, vector.Z);
        }

        public override void Rotate(float x, float y, float z)
        {
            Vector3 newRotation = new Vector3(
                x + _rotation.X,
                y + _rotation.Y,
                z + _rotation.Z
            );
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(newRotation));
            Matrix4 result = rotationMatrix * modelMatrix.ClearRotation();
            _rotation = newRotation;//result.ExtractRotation().Xyz * rotationMultiplier;

            UpdateMatrixVariables(result);
            UpdateTransformMatrix();
        }

        public override void Rotate(Vector3 vector)
        {
            Rotate(vector.X, vector.Y, vector.Z);
        }

        public override void Scale(float scale)
        {
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 result = scaleMatrix * modelMatrix.ClearScale();
            _scale = scale;

            UpdateMatrixVariables(result);
            UpdateTransformMatrix();
        }
    }
}
