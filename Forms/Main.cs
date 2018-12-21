using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace RatchetEdit
{
    public partial class Main : Form
    {
        public Level level;
        public ModelViewer modelViewer;

        //OpenGL variables
        Matrix4 worldView;
        Matrix4 projection;
        Matrix4 view;
        public int shaderID;
        int colorShaderID;
        int matrixID;
        int colorID;

        //Input variables
        bool rMouse = false;
        bool lMouse = false;
        int lastMouseX = 0;
        int lastMouseY = 0;

        //Camera variables
        float cSpeed = 0.2f;
        float pitch = 0f;
        float yaw = -0.75f;
        Vector3 cameraPosition = new Vector3(0, 0, 0);
        Vector3 cameraTarget = new Vector3(0, 0, 0);

        LevelObject prevObject;
        public LevelObject selectedObject;

        List<string> modelNames;

        bool invalidate = false;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();

            //Generate vertex array
            int VAO;
            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            //Setup openGL variables
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.LineWidth(5.0f);
            view = Matrix4.LookAt(cameraPosition, cameraTarget, Vector3.UnitZ);

            //Experimental transparency blend
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            //Setup general shader
            shaderID = GL.CreateProgram();
            loadShader("shaders/vs.glsl", ShaderType.VertexShader, shaderID);
            loadShader("shaders/fs.glsl", ShaderType.FragmentShader, shaderID);
            GL.LinkProgram(shaderID);

            //Setup color shader
            colorShaderID = GL.CreateProgram();
            loadShader("shaders/colorshadervs.glsl", ShaderType.VertexShader, colorShaderID);
            loadShader("shaders/colorshaderfs.glsl", ShaderType.FragmentShader, colorShaderID);
            GL.LinkProgram(colorShaderID);

            matrixID = GL.GetUniformLocation(shaderID, "MVP");
            colorID = GL.GetUniformLocation(colorShaderID, "incolor");
            getModelNames();

        }

        void loadShader(String filename, ShaderType type, int program)
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

        private void mapOpenBtn_Click(object sender, EventArgs e)
        {
            if (mapOpenDialog.ShowDialog() == DialogResult.OK)
            {
                level = new Level(mapOpenDialog.FileName);
                invalidate = true;
                mobyCheck.Enabled = true;
                tieCheck.Enabled = true;
                shrubCheck.Enabled = true;
                collCheck.Enabled = true;
                terrainCheck.Enabled = true;
                splineCheck.Enabled = true;
                objectTree.CollapseAll();
                objectTree.Nodes[0].Nodes.Clear();
                objectTree.Nodes[1].Nodes.Clear();
                objectTree.Nodes[2].Nodes.Clear();
                foreach (Moby mob in level.mobs)
                {
                    string modelName = modelNames != null ? modelNames.Find(x => x.Substring(0, 4).ToUpper() == mob.modelID.ToString("X4")) : null;
                    objectTree.Nodes[0].Nodes.Add(modelName != null ? modelName.Split('=')[1].Substring(1) : mob.modelID.ToString("X"));
                }
                foreach (Tie tie in level.ties)
                {
                    objectTree.Nodes[1].Nodes.Add(tie.modelID.ToString("X"));
                }
                foreach (Tie shrub in level.shrubs)
                {
                    objectTree.Nodes[2].Nodes.Add(shrub.modelID.ToString("X"));
                }



                Moby ratchet = level.mobs[0];


                yaw = ratchet.rotz - (float)Math.PI / 2;

                float xpos = (float)-Math.Cos(ratchet.rotz);
                float ypos = (float)-Math.Sin(ratchet.rotz);
                float distanceToRatchet = 5;

                cameraPosition = new Vector3(ratchet.x + xpos * distanceToRatchet, ratchet.y + ypos * distanceToRatchet, ratchet.z + 2);
            }
        }

        public void getModelNames()
        {
            modelNames = new List<string>();
            string stringCounter;
            StreamReader stream = null;
            try
            {

                stream = new StreamReader(Application.StartupPath + "/ModelListRC1.txt");
                //Console.WriteLine("Loaded model names for Ratchet & Clank.");

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Model list file not found! No names for you!");
                modelNames = null;
                return;
            }
            while ((stringCounter = stream.ReadLine()) != null)
            {
                modelNames.Add(stringCounter);
            }
            stream.Close();
        }

        #region Open Viewers
        private void openModelViewer()
        {
            if (modelViewer == null || modelViewer.IsDisposed)
            {
                modelViewer = new ModelViewer(this);
                modelViewer.Show();
            }
            else
            {
                modelViewer.BringToFront();
            }
        }
        #endregion

        private void modelViewerToolBtn_Click(object sender, EventArgs e)
        {
            openModelViewer();
        }

        private void openModelViewerBtn_Click(object sender, EventArgs e)
        {
            openModelViewer();
        }

        private void exitToolBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void updateMoby()
        {
            if (selectedObject as Moby != null)
            {
                Moby mob = (Moby)selectedObject;
                rotxBox.Value = (decimal)toDegrees(mob.rotx);
                rotyBox.Value = (decimal)toDegrees(mob.roty);
                rotzBox.Value = (decimal)toDegrees(mob.rotz);
                scaleBox.Value = (decimal)mob.scale;
            }

            modelIDBox.Text = selectedObject.modelID.ToString("X");
            xBox.Value = (decimal)selectedObject.x;
            yBox.Value = (decimal)selectedObject.y;
            zBox.Value = (decimal)selectedObject.z;

        }

        private float toDegrees(float radians) {
            return radians * 180 / (float)Math.PI;
        }
        public float toRadians(float angle) {
            return (float)Math.PI / 180 * angle;
        }


        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            camXLabel.Text = cameraPosition.X.ToString();
            camYLabel.Text = cameraPosition.Y.ToString();
            camZLabel.Text = cameraPosition.Z.ToString();

            targetxLabel.Text = cameraTarget.X.ToString();
            targetyLabel.Text = cameraTarget.Y.ToString();
            targetzLabel.Text = cameraTarget.Z.ToString();

            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            worldView = view * projection;

            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);

            if (splineCheck.Checked && splineCheck.Enabled)
            {
                GL.UniformMatrix4(matrixID, false, ref worldView);
                GL.Uniform4(colorID, new Vector4(1, 1, 1, 1));
                foreach (Spline spline in level.splines)
                {
                    spline.getVBO();
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
                }
            }

            if (selectedObject != null && selectedObject.model.vertexBuffer != null && selectedObject.modelMatrix != null)
            {
                Matrix4 mvp = selectedObject.modelMatrix * worldView;
                GL.Uniform4(colorID, new Vector4(1, 1, 1, 1));
                GL.UniformMatrix4(matrixID, false, ref mvp);
                selectedObject.model.getVBO();
                selectedObject.model.getIBO();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, selectedObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            GL.EnableVertexAttribArray(1);
            GL.UseProgram(shaderID);

            if (mobyCheck.Checked && mobyCheck.Enabled)
            {
                foreach (Moby mob in level.mobs)
                {
                    drawObject(mob);
                }
            }

            if (tieCheck.Checked && tieCheck.Enabled)
            {
                foreach (Tie tie in level.ties)
                {
                    drawObject(tie);
                }
            }

            if (shrubCheck.Checked && splineCheck.Enabled)
            {
                foreach (Tie shrub in level.shrubs)
                {
                    drawObject(shrub);
                }
            }

            if (terrainCheck.Checked && terrainCheck.Enabled)
            {
                GL.UniformMatrix4(matrixID, false, ref worldView);

                foreach (TerrainModel hd in level.terrains)
                {
                    if (hd.vertexBuffer != null)
                    {
                        hd.getVBO();
                        hd.getIBO();

                        //Bind textures one by one, applying it to the relevant vertices based on the index array
                        foreach (TextureConfig conf in hd.textureConfig)
                        {
                            GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? level.textures[conf.ID].getTexture() : 0);
                            GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                        }

                    }
                }
            }

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);
            glControl1.SwapBuffers();
            //Console.WriteLine("Painted" + cnt++.ToString());
            invalidate = false;
        }


        #region input
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            float deltaTime = 0.016f;

            float moveSpeed = 25;
            float shiftMultiplier = 3;
            float nonShiftMultiplier = 0.5f;

            Vector3 moveDir = getInputAxes();
            if (moveDir.Length != 0)
            {
                moveDir *= (ModifierKeys == Keys.Shift ? shiftMultiplier : nonShiftMultiplier) * deltaTime * moveSpeed;
                invalidate = true;
            }


            if (rMouse)
            {
                yaw -= (Cursor.Position.X - lastMouseX) * cSpeed * 0.016f;
                pitch -= (Cursor.Position.Y - lastMouseY) * cSpeed * 0.016f;
                pitch = MathHelper.Clamp(pitch, MathHelper.DegreesToRadians(-89.9f), MathHelper.DegreesToRadians(89.9f));

                invalidate = true;
            }

            Matrix3 rot = Matrix3.CreateRotationX(pitch) * Matrix3.CreateRotationZ(yaw);
            Vector3 forward = Vector3.Transform(Vector3.UnitY, rot);

            cameraPosition += Vector3.Transform(moveDir, rot);
            cameraTarget = cameraPosition + forward;

            lastMouseX = Cursor.Position.X;
            lastMouseY = Cursor.Position.Y;

            view = Matrix4.LookAt(cameraPosition, cameraTarget, Vector3.UnitZ);

            if (invalidate)
            {
                glControl1.Invalidate();
            }

        }

        private Vector3 getInputAxes()
        {
            KeyboardState keyState = Keyboard.GetState();

            float xAxis = 0, yAxis = 0, zAxis = 0;

            if (keyState.IsKeyDown(Key.W))
            {
                yAxis += 1;
            }
            if (keyState.IsKeyDown(Key.S))
            {
                yAxis -= 1;
            }

            if (keyState.IsKeyDown(Key.A))
            {
                xAxis -= 1;
            }
            if (keyState.IsKeyDown(Key.D))
            {
                xAxis += 1;
            }

            if (keyState.IsKeyDown(Key.Q))
            {
                zAxis -= 1;
            }
            if (keyState.IsKeyDown(Key.E))
            {
                zAxis += 1;
            }

            return new Vector3(xAxis, yAxis, zAxis);

        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            rMouse = e.Button == MouseButtons.Right;
            lMouse = e.Button == MouseButtons.Left;
        }

        private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            rMouse = false;
            lMouse = false;
        }
        #endregion

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)glControl1.Width / glControl1.Height, 0.1f, 800.0f);
        }

        private void enableCheck(object sender, EventArgs e)
        {
            invalidate = true;
        }

        private void glControl1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && level != null)
            {
                int offset = 0;
                glControl1.MakeCurrent();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                worldView = view * projection;

                if (mobyCheck.Checked)
                {
                    GL.ClearColor(0, 0, 0, 0);
                    foreach (Moby mob in level.mobs)
                    {
                        if (mob.model.vertexBuffer != null)
                        {
                            Matrix4 mvp = mob.modelMatrix * worldView;  //Has to be done in this order to work correctly
                            GL.UniformMatrix4(matrixID, false, ref mvp);

                            mob.model.getVBO();
                            mob.model.getIBO();

                            int mobyNum = level.mobs.IndexOf(mob);
                            byte[] cols = BitConverter.GetBytes(mobyNum);
                            GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                            GL.DrawElements(PrimitiveType.Triangles, mob.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                        }
                    }
                }

                if (tieCheck.Checked)
                {
                    offset += level.mobs.Count;
                    foreach (Tie tie in level.ties)
                    {
                        if (tie.model.vertexBuffer != null)
                        {
                            Matrix4 mvp = tie.modelMatrix * worldView;  //Has to be done in this order to work correctly
                            GL.UniformMatrix4(matrixID, false, ref mvp);

                            tie.model.getVBO();
                            tie.model.getIBO();

                            int tieNum = level.ties.IndexOf(tie);
                            byte[] cols = BitConverter.GetBytes(tieNum + offset);
                            GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                            GL.DrawElements(PrimitiveType.Triangles, tie.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                        }
                    }
                }

                if (shrubCheck.Checked)
                {
                    offset += level.ties.Count;
                    foreach (Tie shrub in level.shrubs)
                    {
                        if (shrub.model.vertexBuffer != null)
                        {
                            Matrix4 mvp = shrub.modelMatrix * worldView;  //Has to be done in this order to work correctly
                            GL.UniformMatrix4(matrixID, false, ref mvp);

                            shrub.model.getVBO();
                            shrub.model.getIBO();

                            int shrubNum = level.shrubs.IndexOf(shrub);
                            byte[] cols = BitConverter.GetBytes(shrubNum + offset);
                            GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                            GL.DrawElements(PrimitiveType.Triangles, shrub.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                        }
                    }
                }

                Byte4 pixel = new Byte4();
                GL.ReadPixels(e.Location.X, glControl1.Height - e.Location.Y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
                GL.ClearColor(Color.SkyBlue);
                if (pixel.A == 0)
                {
                    int id = (int)pixel.ToUInt32();
                    if (id < level.mobs?.Count)
                    {
                        selectedObject = level.mobs[id];
                        objectTree.SelectedNode = objectTree.Nodes[0].Nodes[id];
                    }
                    else if (id - level.mobs?.Count < level.ties?.Count)
                    {
                        selectedObject = level.ties[id - level.mobs.Count];
                        objectTree.SelectedNode = objectTree.Nodes[1].Nodes[id - level.mobs.Count];
                    }

                    else if (id - offset < level.shrubs?.Count)
                    {
                        selectedObject = level.shrubs[id - offset];
                        objectTree.SelectedNode = objectTree.Nodes[2].Nodes[id - offset];
                    }
                    if (selectedObject == prevObject)
                    {
                        openModelViewer();
                    }
                    prevObject = selectedObject;
                    updateMoby();
                }

                invalidate = true;
            }
        }

        struct Byte4
        {
            public byte R, G, B, A;

            public Byte4(byte[] input)
            {
                R = input[0];
                G = input[1];
                B = input[2];
                A = input[3];
            }

            public uint ToUInt32()
            {
                byte[] temp = new byte[] { R, G, B, A };
                return BitConverter.ToUInt32(temp, 0);
            }

            public override string ToString()
            {
                return R + ", " + G + ", " + B + ", " + A;
            }
        }

        private void drawObject(LevelObject obj)
        {
            if (obj.model.vertexBuffer != null)
            {
                Matrix4 mvp = obj.modelMatrix * worldView;  //Has to be done in this order to work correctly
                GL.UniformMatrix4(matrixID, false, ref mvp);

                obj.model.getVBO();
                obj.model.getIBO();

                //Bind textures one by one, applying it to the relevant vertices based on the index array
                foreach (TextureConfig conf in obj.model.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? level.textures[conf.ID].getTexture() : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }
            }
        }

        private void xBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject == null) return;
            selectedObject.x = (float)xBox.Value;
            selectedObject.updateTransform();
            invalidate = true;
        }

        private void yBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject == null) return;
            selectedObject.y = (float)yBox.Value;
            selectedObject.updateTransform();
            invalidate = true;
        }

        private void zBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject == null) return;
            selectedObject.z = (float)zBox.Value;
            selectedObject.updateTransform();
            invalidate = true;
        }

        private void rotxBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject as Moby != null)
            {
                Moby selectedObj = (Moby)selectedObject;
                selectedObj.rotx = toRadians((float)rotxBox.Value);
                selectedObj.updateTransform();
                invalidate = true;
            }
        }

        private void rotyBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject as Moby != null)
            {
                Moby selectedObj = (Moby)selectedObject;
                selectedObj.roty = toRadians((float)rotyBox.Value);
                selectedObj.updateTransform();
                invalidate = true;
            }
        }

        private void rotzBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject as Moby != null)
            {
                Moby selectedObj = (Moby)selectedObject;
                selectedObj.rotz = toRadians((float)rotzBox.Value);
                selectedObj.updateTransform();
                invalidate = true;
            }
        }
        private void objectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == objectTree.Nodes[0])
            {
                selectedObject = level.mobs[e.Node.Index];
                updateMoby();
            }
        }

        public void setCamPos(float x, float y, float z)
        {
            cameraPosition = new Vector3(x, y, z);
        }

        private void gotoPositionBtn_Click(object sender, EventArgs e)
        {
            setCamPos(selectedObject.x, selectedObject.y, selectedObject.z + 10);
            invalidate = true;
        }

        private void scaleBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject as Moby != null)
            {
                Moby selectedObj = (Moby)selectedObject;
                selectedObj.scale = (float)scaleBox.Value;
                selectedObj.updateTransform();
                invalidate = true;
            }
        }
    }
}
