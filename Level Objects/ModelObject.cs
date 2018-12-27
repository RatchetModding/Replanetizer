using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit {
    public class ModelObject : LevelObject {
        public int modelID;
        public Matrix4 modelMatrix;
        public Model model;
    }
}
