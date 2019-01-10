using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public abstract class MatrixObject : LevelObject
    {
        [Browsable(false)]
        public Matrix4 modelMatrix { get; set; }

        public override Vector3 position {
            get { return _position; }
            set {
                Translate(value - _position);
            }
        }
        public override Vector3 rotation {
            get { return _rotation; }
            set {
                Rotate(_rotation - value);
            }
        }

        public override Vector3 scale {
            get { return _scale; }
            set {
                Scale(Vector3.Divide(value, _scale));
            }
        }

        public override void Translate(Vector3 vector) {
            modelMatrix = Utilities.TranslateMatrixTo(modelMatrix, vector + position);
            _position = modelMatrix.ExtractTranslation();
        }

        public override void Rotate(Vector3 vector) {
            Vector3 newRotation = vector + _rotation;
            modelMatrix = Utilities.RotateMatrixTo(modelMatrix, vector + rotation);
            _rotation = newRotation;
        }

        public override void Scale(Vector3 vector) {
            modelMatrix = Utilities.ScaleMatrixTo(modelMatrix, vector * scale);
            _scale = modelMatrix.ExtractScale();
        }
    }
}