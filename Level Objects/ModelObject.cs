using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;

namespace RatchetEdit {
    public abstract class ModelObject : LevelObject {

        [Category("Attributes"), DisplayName("Model ID")]
        public int modelID { get; set; }

        [Browsable(false)]
        public Matrix4 modelMatrix { get; set; }

        [Category("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Model")]
        public Model model { get; set; }

        public override void Render(CustomGLControl glControl, bool selected = false) {
            if (model == null || model.vertexBuffer == null) return;
            GL.UseProgram(glControl.shaderID);
            Matrix4 mvp = modelMatrix * glControl.worldView;  //Has to be done in this order to work correctly
            GL.UniformMatrix4(glControl.matrixID, false, ref mvp);
            model.Draw(glControl.textures);

            if (selected) {
                RenderModelMesh(glControl);
            }
        }

        public void RenderModelMesh(CustomGLControl glControl) {
            if (model == null || model.vertexBuffer == null || modelMatrix == null) return;

            GL.UseProgram(glControl.colorShaderID);
            Matrix4 mvp = modelMatrix * glControl.worldView;
            GL.Uniform4(glControl.colorID, new Vector4(1, 1, 1, 1));
            GL.UniformMatrix4(glControl.matrixID, false, ref mvp);
            model.GetVBO();
            model.GetIBO();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.DrawElements(PrimitiveType.Triangles, model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

        }
    }

}
