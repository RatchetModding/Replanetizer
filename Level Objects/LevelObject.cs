using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RatchetEdit
{
    public class LevelObject
    {
        private Vector3 _position = new Vector3();

        [CategoryAttribute("Position"), TypeConverter(typeof(Vector3Converter)), DisplayName("Position")]
        public Vector3 position {
            get { return _position; }
            set {
                _position = value;
                updateTransform();
            }
        }
        
        public virtual void updateTransform(){ }  //Override me

    }
}
