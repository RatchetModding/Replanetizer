using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RatchetEdit
{
    public partial class ModelViewer : Form
    {
        Main mainForm;
        Level level;
        public Model selectedModel;

        public int matrixID;
        public int shaderID;

        int lastMouseX = 0;

        bool ready = false;
        bool invalidate = false;
        bool rMouse = false;

        float xDelta = 0;

        public Matrix4 projection;
        public Matrix4 trans;
        public Matrix4 scale;
        public Matrix4 rot = Matrix4.Identity;
        public Matrix4 view;

        public Vector3 UP = new Vector3(0, 0, 1);

        public float zoom = 0;

        public ModelViewer(Main main)
        {
            InitializeComponent();
            mainForm = main;
            level = main.level;


            #region model list setup
            if (level != null)
            {
                List<TreeNode> mobyNodes = new List<TreeNode>();
                foreach (Model mod in level.mobyModels)
                {
                    mobyNodes.Add(new TreeNode() {
                        Text = mod?.ID.ToString("X"),
                        ForeColor = mod.vertexBuffer != null ? Color.Black : Color.Red
                    });
                }
                TreeNode mobyNode = new TreeNode("Moby", mobyNodes.ToArray());
                modelView.Nodes.Add(mobyNode);


                List<TreeNode> levelModelNodes = new List<TreeNode>();
                foreach (Model mod in level.tieModels)
                {
                    levelModelNodes.Add(new TreeNode()
                    {
                        Text = mod?.ID.ToString("X"),
                        ForeColor = mod.vertexBuffer != null ? Color.Black : Color.Red
                    });
                }
                TreeNode levelNode = new TreeNode("Level", levelModelNodes.ToArray());
                modelView.Nodes.Add(levelNode);


                List<TreeNode> sceneryModelNodes = new List<TreeNode>();
                foreach (Model mod in level.shrubModels)
                {
                    sceneryModelNodes.Add(new TreeNode()
                    {
                        Text = mod?.ID.ToString("X"),
                        ForeColor = mod.vertexBuffer != null ? Color.Black : Color.Red
                    });
                }
                TreeNode sceneryNode = new TreeNode("Scenery", sceneryModelNodes.ToArray());
                modelView.Nodes.Add(sceneryNode);
            }
            #endregion
        }

        private void modelView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Level == 1)
            {
                switch (e.Node.Parent.Text)
                {
                    case "Moby":
                        selectedModel = level.mobyModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = false;
                        break;
                    case "Level":
                        selectedModel = level.tieModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                    case "Scenery":
                        selectedModel = level.shrubModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                }

                updateModel();
            }
        }

        private void ModelViewer_Load(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            GL.ClearColor(Color.SkyBlue);
            shaderID = mainForm.shaderID;

            matrixID = GL.GetUniformLocation(shaderID, "MVP");

            GL.Enable(EnableCap.DepthTest);
            GL.EnableClientState(ArrayCap.VertexArray);

            selectedModel = mainForm.selectedObject?.model;

            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)glControl1.Width / glControl1.Height, 0.1f, 100.0f);
            trans = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);

            int VAO;
            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            updateModel();
        }

        public void updateModel()
        {
            vertCountBox.Text = selectedModel.vertexBuffer?.Length.ToString();
            faceCountBox.Text = selectedModel.indexBuffer?.Length.ToString();
            IDBox.Text = selectedModel.ID.ToString("X");
            sizeBox.Text = selectedModel.size.ToString();
            texCountBox.Text = selectedModel.textureConfig?.Count.ToString();
            if (selectedModel.vertexBuffer != null) //Check that there's actually vertex data to be rendered
            {
                scale = Matrix4.CreateScale(selectedModel.size);

                ready = true;
                invalidate = true;
            }
            else
            {
                ready = false;
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (ready)
            {
                glControl1.MakeCurrent();
                view = Matrix4.LookAt(new Vector3(10 + zoom, 10 + zoom, 10 + zoom), Vector3.Zero, UP);
                Matrix4 mvp = trans * scale * rot * view * projection;  //Has to be done in this order to work correctly

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.UseProgram(shaderID);
                GL.UniformMatrix4(matrixID, false, ref mvp);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                selectedModel.getIBO();
                selectedModel.getVBO();

                //Bind textures one by one, applying it to the relevant vertices based on the index array
                foreach (TextureConfig texConfig in selectedModel.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (texConfig.ID > 0) ? level.textures[texConfig.ID].getTexture() : 0);
                    GL.DrawElements(PrimitiveType.Triangles, texConfig.size, DrawElementsType.UnsignedShort, texConfig.start * sizeof(ushort));
                }

                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(0);
                glControl1.SwapBuffers();
            }
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom += (e.Delta > 0) ? -0.1f : 0.1f;
            
            invalidate = true;
        }


        private void tickTimer_Tick(object sender, EventArgs e)
        {
            if (rMouse)
            {
                xDelta += (Cursor.Position.X - lastMouseX) * 0.02f;
                rot = Matrix4.CreateRotationZ(xDelta);
                lastMouseX = Cursor.Position.X;
                invalidate = true;
            }
            if (invalidate)
            {
                glControl1.Invalidate();
            }
        }


        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMouseX = Cursor.Position.X;
            rMouse = true;
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            rMouse = false;
        }
    }
}
