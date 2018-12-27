using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit {
    public interface Transformable {
        void Translate(float x, float y, float z);
        void Translate(Vector3 vector);

        void Rotate(float x, float y, float z);
        void Rotate(Vector3 vector);

        void Scale(float scale);
    }
}
