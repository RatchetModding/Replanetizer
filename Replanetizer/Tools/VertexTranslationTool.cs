using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Frames;

namespace Replanetizer.Tools
{
    class VertexTranslationTool : Tool
    {
        public VertexTranslationTool()
        {
            float length = 0.7f;

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
            GL.UniformMatrix4(frame.shaderIDTable.UniformModelToWorldMatrix, false, ref modelMatrix);

            GL.Uniform1(frame.shaderIDTable.UniformColorLevelObjectNumber, 0);
            GL.Uniform4(frame.shaderIDTable.UniformColor, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 0, 2);

            GL.Uniform1(frame.shaderIDTable.UniformColorLevelObjectNumber, 1);
            GL.Uniform4(frame.shaderIDTable.UniformColor, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 2, 2);

            GL.Uniform1(frame.shaderIDTable.UniformColorLevelObjectNumber, 2);
            GL.Uniform4(frame.shaderIDTable.UniformColor, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 4, 2);
        }

        public override ToolType GetToolType()
        {
            return ToolType.VertexTranslator;
        }
    }
}
