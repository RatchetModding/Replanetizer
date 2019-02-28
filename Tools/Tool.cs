using OpenTK;

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
