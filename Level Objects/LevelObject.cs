using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public class LevelObject
    {
        public int modelID;
        public Matrix4 modelMatrix;
        public Model model;
        public Vector3 position = new Vector3();
        
        public virtual void updateTransform(){ }  //Override me

    }
}
