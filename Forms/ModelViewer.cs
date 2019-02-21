using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

                modelView.Nodes.Add(GetModelNodes("Weapon", level.weaponModels));
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

            UpdateTextures();
        }

        private void modelView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                switch (e.Node.Parent.Text)
                {
                    case "Moby":
                        selectedModel = level.mobyModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                    case "Tie":
                        selectedModel = level.tieModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                    case "Shrub":
                        selectedModel = level.shrubModels[modelView.SelectedNode.Index];
                        LoadObjBtn.Enabled = true;
                        break;
                    case "Weapon":
                        selectedModel = level.weaponModels[modelView.SelectedNode.Index];
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

        private void UpdateTextures()
        {
            textureList.Images.Clear();
            textureView.Items.Clear();
            int index = 0;
            foreach (TextureConfig conf in selectedModel.textureConfig)
            {
                if (conf.ID == -1) continue;

                textureList.Images.Add(mainForm.level.textures[conf.ID].getTextureImage());
                ListViewItem item = new ListViewItem();
                item.ImageIndex = index;
                item.Text = conf.ID.ToString();
                this.textureView.Items.Add(item);
                index++;
            }
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

        private void LoadObjBtn_Click(object sender, EventArgs e)
        {
            if (objOpen.ShowDialog() != DialogResult.OK) return;

            var vertexList = new List<float>();
            var nomalList = new List<float>();
            var uvList = new List<float>();

            var vertexBufferList = new List<float>();
            var conf = new List<TextureConfig>();
            var indBuff = new List<ushort>();

            string line;
            StreamReader file = new StreamReader(objOpen.FileName);

            ushort indCnt = 0, prevCnt = 0;
            int mod = selectedModel.textureConfig[0].mode;

            while ((line = file.ReadLine()) != null)
            {
                string[] g = line.Split(' ');
                switch (g[0])
                {
                    case "v":
                        vertexList.Add(float.Parse(g[1]));
                        vertexList.Add(float.Parse(g[2]));
                        vertexList.Add(float.Parse(g[3]));
                        break;
                    case "vn":
                        nomalList.Add(float.Parse(g[1]));
                        nomalList.Add(float.Parse(g[2]));
                        nomalList.Add(float.Parse(g[3]));
                        break;
                    case "vt":
                        uvList.Add(float.Parse(g[1]));
                        uvList.Add(float.Parse(g[2]));
                        break;
                    case "usemtl":
                        TextureConfig c1 = new TextureConfig();
                        c1.ID = 0x2d8;
                        c1.start = prevCnt;
                        c1.size = indCnt - prevCnt;
                        c1.mode = mod;
                        conf.Add(c1);

                        prevCnt = indCnt;
                        break;

                    case "f":
                        string[] f1 = g[1].Split('/');
                        string[] f2 = g[2].Split('/');
                        string[] f3 = g[3].Split('/');

                        ushort vert1 = (ushort)(ushort.Parse(f1[0]) - 1);
                        ushort vert2 = (ushort)(ushort.Parse(f2[0]) - 1);
                        ushort vert3 = (ushort)(ushort.Parse(f3[0]) - 1);

                        ushort normal1 = (ushort)(ushort.Parse(f1[2]) - 1);
                        ushort normal2 = (ushort)(ushort.Parse(f2[2]) - 1);
                        ushort normal3 = (ushort)(ushort.Parse(f3[2]) - 1);

                        ushort uv1 = (ushort)(ushort.Parse(f1[1]) - 1);
                        ushort uv2 = (ushort)(ushort.Parse(f2[1]) - 1);
                        ushort uv3 = (ushort)(ushort.Parse(f3[1]) - 1);


                        indBuff.Add((ushort)(indCnt + 0));
                        indBuff.Add((ushort)(indCnt + 1));
                        indBuff.Add((ushort)(indCnt + 2));

                        vertexBufferList.Add(vertexList[(vert1) * 3 + 0]);
                        vertexBufferList.Add(vertexList[(vert1) * 3 + 1]);
                        vertexBufferList.Add(vertexList[(vert1) * 3 + 2]);

                        vertexBufferList.Add(nomalList[(normal1) * 3 + 0]);
                        vertexBufferList.Add(nomalList[(normal1) * 3 + 1]);
                        vertexBufferList.Add(nomalList[(normal1) * 3 + 2]);

                        vertexBufferList.Add(uvList[uv1 * 2 + 0]);
                        vertexBufferList.Add(1f - uvList[uv1 * 2 + 1]);


                        vertexBufferList.Add(vertexList[(vert2) * 3 + 0]);
                        vertexBufferList.Add(vertexList[(vert2) * 3 + 1]);
                        vertexBufferList.Add(vertexList[(vert2) * 3 + 2]);

                        vertexBufferList.Add(nomalList[(normal2) * 3 + 0]);
                        vertexBufferList.Add(nomalList[(normal2) * 3 + 1]);
                        vertexBufferList.Add(nomalList[(normal2) * 3 + 2]);

                        vertexBufferList.Add(uvList[uv2 * 2 + 0]);
                        vertexBufferList.Add(1f - uvList[uv2 * 2 + 1]);


                        vertexBufferList.Add(vertexList[(vert3) * 3 + 0]);
                        vertexBufferList.Add(vertexList[(vert3) * 3 + 1]);
                        vertexBufferList.Add(vertexList[(vert3) * 3 + 2]);

                        vertexBufferList.Add(nomalList[(normal3) * 3 + 0]);
                        vertexBufferList.Add(nomalList[(normal3) * 3 + 1]);
                        vertexBufferList.Add(nomalList[(normal3) * 3 + 2]);

                        vertexBufferList.Add(uvList[uv3 * 2 + 0]);
                        vertexBufferList.Add(1f - uvList[uv3 * 2 + 1]);

                        indCnt += 3;
                        break;
                }
            }

            TextureConfig cc = new TextureConfig();
            cc.ID = 428;
            cc.start = prevCnt;
            cc.size = indCnt - prevCnt;
            cc.mode = mod;
            conf.Add(cc);

            prevCnt = indCnt;

            selectedModel.vertexBuffer = vertexBufferList.ToArray();
            selectedModel.indexBuffer = indBuff.ToArray();

            selectedModel.textureConfig = conf;

            selectedModel.VBO = 0;
            selectedModel.IBO = 0;

            UpdateModel();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateTextures();
        }

        private void textureView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textureView_DoubleClick(object sender, EventArgs e)
        {
            Console.WriteLine("G");
            TextureViewer texView = new TextureViewer(mainForm, this, selectedModel.textureConfig[textureView.SelectedIndices[0]]);
            texView.Show();
        }
    }
}
