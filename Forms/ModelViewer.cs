using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RatchetEdit.Models;
using RatchetEdit.Models.Animations;
using System.Linq;

namespace RatchetEdit
{
    public partial class ModelViewer : Form
    {
        private Main mainForm;
        private Level level;
        private Model selectedModel;

        private int shaderID, matrixID, jointID;

        private int lastMouseX;
        private float xDelta;
        private float zoom;

        private bool invalidate;
        private bool rMouse;

        private Matrix4 trans, scale, worldView, rot = Matrix4.Identity;

        private TreeNode mobyNode, tieNode, shrubNode, weaponNode;

        private int frameNum = 0;
        private int animationNum = 0;

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
            shaderID = mainForm.GetAnimationShader();

            matrixID = GL.GetUniformLocation(shaderID, "MVP");
            jointID = GL.GetUniformLocation(shaderID, "Bone");

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
            //



            GL.UseProgram(shaderID);


            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);


            MobyModel drawModel = (MobyModel)level.mobyModels.Where(c => c.id == 0x424).FirstOrDefault();
            Matrix4 modelScale = Matrix4.CreateScale(0.01f);

            int maxPar = 0;

            Matrix4[] translationMatrices = new Matrix4[100];

            if (selectedModel is MobyModel model)
            {

                for (int i = 0; i < model.boneDatas.Count; i++)
                {
                    Matrix4 mat = model.boneMatrices[i].mat1;
                    float x = mat.M41 / 1024f;
                    float y = mat.M42 / 1024f;
                    float z = mat.M43 / 1024f;

                    float x2 = model.boneDatas[i].unk1 / 1024f;
                    float y2 = model.boneDatas[i].unk2 / 1024f;
                    float z2 = model.boneDatas[i].unk3 / 1024f;

                    Matrix4 matt = Matrix4.CreateTranslation(x2, y2, z2);
                    //matt.Invert();
                    translationMatrices[i] = matt;

                }

                Console.WriteLine("Bone count: " + model.boneMatrices.Count);
                for (int i = 0; i < model.boneDatas.Count; i++)
                {
                    Matrix4 mat = model.boneMatrices[i].mat1;
                    float x = mat.M41 / 1024f;
                    float y = mat.M42 / 1024f;
                    float z = mat.M43 / 1024f;

                    float x2 = model.boneDatas[i].unk1 / 1024f;
                    float y2 = model.boneDatas[i].unk2 / 1024f;
                    float z2 = model.boneDatas[i].unk3 / 1024f;

                    Matrix4 matt = Matrix4.CreateTranslation(x2, y2, z2);
                    matt.Invert();

                    int parent = model.boneMatrices[i].bb / 0x40;

                    //Matrix4 mat2 = Matrix4.CreateScale(1 / 1024f);
                    //Matrix4 mat4 = mat * mat2;

                    Animation anim = model.animations[animationNum];
                    Frame frame = anim.frames[frameNum];
                    short[] rots = frame.rotations[i];

                    Console.WriteLine(rots[0] / 32767f);

                    Quaternion quat = new Quaternion((rots[0] / 32767f) * 180f, (rots[1] / 32767f) * 180f, (rots[2] / 32767f) *180f, (-rots[3] / 32767f) *180f);

                    Matrix4 animationRotation = Matrix4.CreateFromQuaternion(quat);

                    /*
                    float xx = model.boneDatas[i].unk1 / 1024f;
                    float yy = model.boneDatas[i].unk2 / 1024f;
                    float zz = model.boneDatas[i].unk3 / 1024f;
                    */

                    Matrix4 translation = Matrix4.CreateTranslation(x, y, z);
                    Matrix4 postTranslation = Matrix4.CreateTranslation(x2, y2, z2);

                    //Matrix4 mvpa =  modelScale * translation * scale * rot * worldView;
                    //GL.UniformMatrix4(matrixID, false, ref mvpa);

                    

                    if (parent == 0)
                    {
                        translationMatrices[i] = animationRotation;
                        Console.WriteLine("Parent bone");
                    }
                    else
                    {
                        Matrix4 parentIvert = translationMatrices[0];
                        parentIvert.Invert();
                        translationMatrices[i] += animationRotation + translationMatrices[parent];
                    }

                    if (parent > maxPar) maxPar = parent;

                    //translationMatrices[i] = Matrix4.Identity;

                    //translationMatrices[i] = Matrix4.Identity;
                }

                
                /*for (int i = 0; i < model.boneDatas.Count; i++)
                {
                    Matrix4 mat = model.boneMatrices[i].mat1;
                    float x = mat.M41 / 1024f;
                    float y = mat.M42 / 1024f;
                    float z = mat.M43 / 1024f;

                    float x2 = model.boneDatas[i].unk1 / 1024f;
                    float y2 = model.boneDatas[i].unk2 / 1024f;
                    float z2 = model.boneDatas[i].unk3 / 1024f;

                    Matrix4 matt = Matrix4.CreateTranslation(x, y, z);
                   matt.Invert();


                    Matrix4 scaler = Matrix4.CreateScale(1 / 1024f);

                    translationMatrices[i] *= mat * scaler;
                }*/

            }
            GL.UniformMatrix4(jointID, 100, false, ref translationMatrices[0].Row0.X);

            //Console.WriteLine(translationMatrices[50].ToString());
            Console.WriteLine("Max parent: " + maxPar);
           


            Matrix4 mvp =  scale * rot * worldView;
            GL.UniformMatrix4(matrixID, false, ref mvp);


            selectedModel.DrawAnimated(level.textures);

            //selectedModel.Draw(level.textures);

            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(2);
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

        private void Button1_Click(object sender, EventArgs e)
        {
            if (frameNum > 0)
            {
                frameNum--;
                label1.Text = frameNum.ToString();
                invalidate = true;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {

            if (selectedModel is MobyModel model)
            {
                if(frameNum < model.animations[animationNum].frames.Count - 1)
                {
                    frameNum++;
                } else
                {
                    frameNum = 0;
                }
                label1.Text = frameNum.ToString();
                invalidate = true;
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(numericUpDown1.Value < 0)
            {
                numericUpDown1.Value = 0;
            }

            if(selectedModel is MobyModel model)
            {
                if (numericUpDown1.Value > model.animations.Count - 1)
                {
                    numericUpDown1.Value = model.animations.Count - 1;
                }
            }

            animationNum = (int)numericUpDown1.Value;
            frameNum = 0;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (selectedModel is MobyModel model)
            {
                if (frameNum < model.animations[animationNum].frames.Count - 1)
                {
                    frameNum++;
                }
                else
                {
                    frameNum = 0;
                }
                label1.Text = frameNum.ToString();
                invalidate = true;
            }
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
