using LibReplanetizer;
using LibReplanetizer.Models;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RatchetEdit
{
    public partial class ModelViewer : Form
    {
        private Main mainForm;
        private Level level;
        private Model selectedModel;

        private int shaderID, matrixID;

        private int lastMouseX;
        private float xDelta;
        private float zoom;

        private bool invalidate;
        private bool rMouse;

        private Matrix4 trans, scale, worldView, rot = Matrix4.Identity;

        private TreeNode mobyNode, tieNode, shrubNode, weaponNode;

        private BufferContainer container;

        public ModelViewer(Main main, Model model)
        {
            InitializeComponent();

            mainForm = main;
            level = main.level;

            //Setup model tree
            if (level != null)
            {
                mobyNode = GetModelNodes("Moby", level.mobyModels);
                tieNode = GetModelNodes("Tie", level.tieModels);
                shrubNode = GetModelNodes("Shrub", level.shrubModels);
                weaponNode = GetModelNodes("Weapon", level.weaponModels);
                modelView.Nodes.AddRange(new TreeNode[] { mobyNode, tieNode, shrubNode, weaponNode });
            }

            SelectModel(model);
        }

        private void ModelViewer_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
            GL.ClearColor(Color.CornflowerBlue);
            shaderID = mainForm.GetShaderID();

            matrixID = GL.GetUniformLocation(shaderID, "MVP");

            GL.Enable(EnableCap.DepthTest);
            GL.EnableClientState(ArrayCap.VertexArray);



            worldView = CreateWorldView();
            trans = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);

            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);
        }

        public TreeNode GetModelNodes(string name, List<Model> models)
        {
            TreeNode newNode = new TreeNode(name);
            foreach (Model mod in models)
            {
                newNode.Nodes.Add(new TreeNode()
                {
                    Text = mod?.id.ToString("X"),
                    ForeColor = mod.vertexBuffer.Length == 0 ? Color.Red : Color.Black
                });
            }
            return newNode;
        }

        public void UpdateModel()
        {
            scale = Matrix4.CreateScale(selectedModel.size);
            invalidate = true;
            modelProperties.SelectedObject = selectedModel;
            UpdateTextures();

            container = BufferContainer.FromRenderable(selectedModel);
            container.Bind();
        }

        private void UpdateTextures()
        {
            textureList.Images.Clear();
            textureView.Items.Clear();

            for (int i = 0; i < selectedModel.textureConfig.Count; i++)
            {
                int textureId = selectedModel.textureConfig[i].ID;
                if (textureId == -1) continue;

                textureList.Images.Add(mainForm.level.textures[textureId].getTextureImage());
                textureView.Items.Add(new ListViewItem
                {
                    ImageIndex = i,
                    Text = textureId.ToString()
                });
            }
        }

        private void modelView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                switch (e.Node.Parent.Text)
                {
                    case "Moby":
                        selectedModel = level.mobyModels[modelView.SelectedNode.Index];
                        break;
                    case "Tie":
                        selectedModel = level.tieModels[modelView.SelectedNode.Index];
                        break;
                    case "Shrub":
                        selectedModel = level.shrubModels[modelView.SelectedNode.Index];
                        break;
                    case "Weapon":
                        selectedModel = level.weaponModels[modelView.SelectedNode.Index];
                        break;
                }
                UpdateModel();
            }
        }

        private void SelectModel(Model model)
        {
            if (model == null) return;

            selectedModel = model;

            switch (model)
            {
                case MobyModel mobyModel:
                    modelView.SelectedNode = mobyNode.Nodes[level.mobyModels.IndexOf(mobyModel)];
                    break;
                case TieModel tieModel:
                    modelView.SelectedNode = tieNode.Nodes[level.tieModels.IndexOf(tieModel)];
                    break;
                case ShrubModel shrubModel:
                    modelView.SelectedNode = shrubNode.Nodes[level.shrubModels.IndexOf(shrubModel)];
                    break;
            }

            UpdateModel();
        }

        private Matrix4 CreateWorldView()
        {
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)glControl.Width / glControl.Height, 0.1f, 100.0f);
            Matrix4 view = Matrix4.LookAt(new Vector3(10 + zoom, 10 + zoom, 10 + zoom), Vector3.Zero, Vector3.UnitZ);
            return view * projection;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (selectedModel == null) return;

            glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Has to be done in this order to work correctly
            Matrix4 mvp = trans * scale * rot * worldView;

            GL.UseProgram(shaderID);
            GL.UniformMatrix4(matrixID, false, ref mvp);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            container.Bind();
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in selectedModel.textureConfig)
            {
                GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? mainForm.GetTextureIds()[level.textures[conf.ID]] : 0);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);
            glControl.SwapBuffers();

            invalidate = false;
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom += (e.Delta > 0) ? -0.1f : 0.1f;
            worldView = CreateWorldView();
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
                glControl.Invalidate();
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

        private void glControl1_Resize(object sender, EventArgs e)
        {
            worldView = CreateWorldView();
            invalidate = true;
        }

        private void textureView_DoubleClick(object sender, EventArgs e)
        {
            using (TextureViewer textureViewer = new TextureViewer(mainForm))
            {
                if (textureViewer.ShowDialog() == DialogResult.OK)
                {
                    int val = textureViewer.returnVal;
                    selectedModel.textureConfig[textureView.SelectedIndices[0]].ID = val;
                    UpdateModel();
                }
            }
        }

        private void importFromobjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objOpen.ShowDialog() == DialogResult.OK)
            {
                ModelReader.ReadObj(objOpen.FileName, selectedModel);
                UpdateModel();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (modelSave.ShowDialog() == DialogResult.OK)
            {
                string fileName = modelSave.FileName;
                string extension = Path.GetExtension(fileName);

                switch (extension)
                {
                    case ".obj":
                        ModelWriter.WriteObj(fileName, selectedModel);
                        break;
                    case ".iqe":
                        ModelWriter.WriteIqe(fileName, level, selectedModel);
                        break;
                }
            }
        }
    }
}
