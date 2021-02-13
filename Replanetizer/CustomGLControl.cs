using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace RatchetEdit
{
    public class CustomGLControl : GLControl
    {
        public Matrix4 worldView;
        public Matrix4 projection;
        public Matrix4 view;
        public int shaderID;
        public int colorShaderID;
        public int matrixID;
        public int colorID;
        public int currentSplineVertex = 0;

        public List<Texture> textures;

        private bool initialized = false;

        public void InitializeGLConfig()
        {
            Console.WriteLine("Initialized");
            int VAO;
            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            //Setup openGL variables
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.LineWidth(5.0f);

            //Setup general shader
            shaderID = GL.CreateProgram();
            LoadShader("Shaders/vs.glsl", ShaderType.VertexShader, shaderID);
            LoadShader("Shaders/fs.glsl", ShaderType.FragmentShader, shaderID);
            GL.LinkProgram(shaderID);

            //Setup color shader
            colorShaderID = GL.CreateProgram();
            LoadShader("Shaders/colorshadervs.glsl", ShaderType.VertexShader, colorShaderID);
            LoadShader("Shaders/colorshaderfs.glsl", ShaderType.FragmentShader, colorShaderID);
            GL.LinkProgram(colorShaderID);

            matrixID = GL.GetUniformLocation(shaderID, "MVP");
            colorID = GL.GetUniformLocation(colorShaderID, "incolor");

            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)Width / Height, 0.1f, 800.0f);

            initialized = true;
        }

        void LoadShader(String filename, ShaderType type, int program)
        {
            int address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode) { base.OnPaint(e); return; }
            worldView = view * projection;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            base.OnPaint(e);


            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            SwapBuffers();

        }

        protected override void OnResize(EventArgs e)
        {
            if (DesignMode) { base.OnResize(e); return; }

            base.OnResize(e);
            if (!initialized) return;
            GL.Viewport(Location.X, Location.Y, Width, Height);
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)Width / Height, 0.1f, 800.0f);

        }
    }
}
