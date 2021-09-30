using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Frames;

namespace Replanetizer.Tools
{
    class RotationTool : Tool
    {
        public RotationTool()
        {
            float length = 1.5f;

            vb = new float[]{
                -length,    0,          0,
                length,     0,          0,
                0,          -length,    0,
                0,          length,     0,
                0,          0,          -length,
                0,          0,          length,
            };
        }

        public override void Render(Vector3 position, LevelFrame frame)
        {
            GetVBO();

            Matrix4 modelMatrix = Matrix4.CreateTranslation(position);
            var mvp = modelMatrix * frame.worldView;
            GL.UniformMatrix4(frame.matrixID, false, ref mvp);

            GL.Uniform1(frame.uniformLevelObjectNumberColorID, 0);
            GL.Uniform4(frame.colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 0, 2);

            GL.Uniform1(frame.uniformLevelObjectNumberColorID, 1);
            GL.Uniform4(frame.colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 2, 2);

            GL.Uniform1(frame.uniformLevelObjectNumberColorID, 2);
            GL.Uniform4(frame.colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 4, 2);
        }
        public override ToolType GetToolType()
        {
            return ToolType.Rotate;
        }
    }
}
