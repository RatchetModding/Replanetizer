using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit {
    class Camera : ITransformable {
        //Camera variables
        public float speed = 0.2f;
        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3(0,0,-0.75f);

        public void SetPosition(Vector3 position) {
            this.position = position;
        }

        public void SetPosition(float x, float y, float z) {
            SetPosition(new Vector3(x,y,z));
        }

        public void SetRotation(float pitch, float yaw) {
            SetRotation(new Vector3(pitch, 0, yaw));
        }
        public void SetRotation(Vector3 rotation) {
            this.rotation = rotation;
        }

        public void MoveBehind(LevelObject levelObject, float distanceToObject = 5) {
            float yaw = 0;

            if (levelObject as Moby != null) { //If object is moby, load its rotation.
                yaw = ((Moby)levelObject).rotation.Z;
            }

            yaw = yaw - (float)Math.PI / 2;
            SetRotation(0, yaw);

            float ypos = (float)-Math.Cos(yaw);
            float xpos = (float)Math.Sin(yaw);
            Vector3 cameraPosition = new Vector3(
                levelObject.position.X + xpos * distanceToObject,
                levelObject.position.Y + ypos * distanceToObject,
                levelObject.position.Z + distanceToObject / 2
            );
            SetPosition(cameraPosition);
        }

        public void Translate(float x, float y, float z) {
            Translate(new Vector3(x, y, z));
        }

        public void Translate(Vector3 vector) {
            position += vector;
        }

        public void Rotate(float x, float y, float z) {
            Rotate(new Vector3(x, y, z));
        }

        public void Rotate(Vector3 vector) {
            rotation += vector;
        }

        public void Scale(float scale) {
            // N/A
        }
    }
}
