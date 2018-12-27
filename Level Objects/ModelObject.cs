using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit {
    public class ModelObject : LevelObject {

        [CategoryAttribute("Attributes"), DisplayName("Model ID")]
        public int modelID { get; set; }

        [Browsable(false)]
        public Matrix4 modelMatrix { get; set; }

        [Browsable(false)]
        public Model model { get; set; }
    }
}
