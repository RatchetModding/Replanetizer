using LibReplanetizer.Models;
using System.ComponentModel;

namespace LibReplanetizer.LevelObjects
{
    public abstract class ModelObject : LevelObject, IRenderable
    {

        [Category("Attributes"), DisplayName("Model ID")]
        public int modelID { get; set; }


        [Category("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Model")]
        public Model model { get; set; }

        public ushort[] GetIndices()
        {
            return model.GetIndices();
        }

        public float[] GetVertices()
        {
            return model.GetVertices();
        }

        public bool IsDynamic() { 
            return model.IsDynamic();  
        }

    }
}
