using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit.Tools
{
    public abstract class Tool
    {
        public enum ToolType {
            None,
            Translate,
            Rotate,
            Scale,
            VertexTranslator
        }

        public abstract ToolType GetToolType();
        public abstract void Render(Vector3 position, CustomGLControl control);
    }
}
