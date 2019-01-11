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

            //Setup model tree
            if (level != null)
            {
                modelView.Nodes.Add(GetModelNodes("Moby", level.mobyModels));

                modelView.Nodes.Add(GetModelNodes("Tie", level.tieModels));

                modelView.Nodes.Add(GetModelNodes("Shrub", level.shrubModels));
            }
        }

        public TreeNode GetModelNodes(string name, List<Model> models)
        {
            TreeNode newNode = new TreeNode(name);
            foreach (Model mod in models)
            {
                newNode.Nodes.Add(new TreeNode()
                {
                    Text = mod?.id.ToString("X"),
                    ForeColor = mod.vertexBuffer != null ? Color.Black : Color.Red
                });
            }
            return newNode;
        }

        public void UpdateModel()
        {
            if (selectedModel.vertexBuffer != null) //Check that there's actually vertex data to be rendered
            {
                scale = Matrix4.CreateScale(selectedModel.size);

                ready = true;
                invalidate = true;
            }
            else
            {
                ready = false;
                invalidate = true;
            }
            propertyGrid1.SelectedObject = selectedModel;
        }

        private void modelView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                switch (e.Node.Parent.Text)
                {
                    case "Moby":
                        selectedModel = level.mobyModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = false;
                        break;
                    case "Tie":
                        selectedModel = level.tieModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                    case "Shrub":
                        selectedModel = level.shrubModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                }
                UpdateModel();
            }
        }

        private void ModelViewer_Load(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            GL.ClearColor(Color.SkyBlue);
            shaderID = mainForm.GetShaderID();

            matrixID = GL.GetUniformLocation(shaderID, "MVP");

            GL.Enable(EnableCap.DepthTest);
            GL.EnableClientState(ArrayCap.VertexArray);

            selectedModel = (mainForm.selectedObject as ModelObject)?.model;

            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)glControl1.Width / glControl1.Height, 0.1f, 100.0f);
            trans = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);

            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);

            UpdateModel();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (ready)
            {
                view = Matrix4.LookAt(new Vector3(10 + zoom, 10 + zoom, 10 + zoom), Vector3.Zero, UP);
                Matrix4 mvp = trans * scale * rot * view * projection;  //Has to be done in this order to work correctly

                GL.UseProgram(shaderID);
                GL.UniformMatrix4(matrixID, false, ref mvp);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                selectedModel.Draw(level.textures);

                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(0);

            }

            glControl1.SwapBuffers();
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

        private void saveObjBtn_Click(object sender, EventArgs e)
        {
            if (objSave.ShowDialog() != DialogResult.OK) return;

            string fileName = objSave.FileName;
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            Model model = selectedModel;

            StreamWriter OBJfs = new StreamWriter(fileName);
            StreamWriter MTLfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            OBJfs.WriteLine("o Object_" + model.id.ToString("X4"));
            if (model.textureConfig != null)
                OBJfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");


            List<uint> usedMtls = new List<uint>(); //To prevent it from making double mtl entries
            for (int i = 0; i < model.textureConfig.Count; i++)
            {
                uint modelTextureID = (uint)model.textureConfig[0].ID;
                if (!usedMtls.Contains(modelTextureID))
                {
                    MTLfs.WriteLine("newmtl mtl_" + modelTextureID);
                    MTLfs.WriteLine("Ns 1000");
                    MTLfs.WriteLine("Ka 1.000000 1.000000 1.000000");
                    MTLfs.WriteLine("Kd 1.000000 1.000000 1.000000");
                    MTLfs.WriteLine("Ni 1.000000");
                    MTLfs.WriteLine("d 1.000000");
                    MTLfs.WriteLine("illum 1");
                    MTLfs.WriteLine("map_Kd tex_" + model.textureConfig[i].ID + ".png");
                    usedMtls.Add(modelTextureID);
                }
            }
            MTLfs.Close();


            //Vertices, normals, UV's
            for (int x = 0; x < model.vertexBuffer.Length / 8; x++)
            {
                float px = model.vertexBuffer[(x * 0x08) + 0x0];
                float py = model.vertexBuffer[(x * 0x08) + 0x1];
                float pz = model.vertexBuffer[(x * 0x08) + 0x2];
                float nx = model.vertexBuffer[(x * 0x08) + 0x3];
                float ny = model.vertexBuffer[(x * 0x08) + 0x4];
                float nz = model.vertexBuffer[(x * 0x08) + 0x5];
                float tu = model.vertexBuffer[(x * 0x08) + 0x6];
                float tv = 1f - model.vertexBuffer[(x * 0x08) + 0x7];
                OBJfs.WriteLine("v " + px.ToString("G") + " " + py.ToString("G") + " " + pz.ToString("G"));
                OBJfs.WriteLine("vn " + nx.ToString("G") + " " + ny.ToString("G") + " " + nz.ToString("G"));
                OBJfs.WriteLine("vt " + tu.ToString("G") + " " + tv.ToString("G"));
            }


            //Faces
            int tCnt = 0;
            for (int i = 0; i < model.indexBuffer.Length / 3; i++)
            {
                if (model.textureConfig != null && tCnt < model.textureConfig.Count)
                {
                    if (i * 3 >= model.textureConfig[tCnt].start)
                    {
                        OBJfs.WriteLine("usemtl mtl_" + model.textureConfig[tCnt].ID.ToString(""));
                        OBJfs.WriteLine("g Texture_" + model.textureConfig[tCnt].ID.ToString(""));
                        tCnt++;
                    }
                }
                int f1 = model.indexBuffer[i * 3 + 0] + 1;
                int f2 = model.indexBuffer[i * 3 + 1] + 1;
                int f3 = model.indexBuffer[i * 3 + 2] + 1;
                OBJfs.WriteLine("f " + (f1 + "/" + f1 + "/" + f1) + " " + (f2 + "/" + f2 + "/" + f2) + " " + (f3 + "/" + f3 + "/" + f3));
                //OBJfs.WriteLine("f " + (f3 + "/" + f3 + "/" + f3) + " " + (f2 + "/" + f2 + "/" + f2) + " " + (f1 + "/" + f1 + "/" + f1));
            }
            OBJfs.Close();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)glControl1.Width / glControl1.Height, 0.1f, 100.0f);
            invalidate = true;
        }
    }
}
