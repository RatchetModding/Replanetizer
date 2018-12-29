using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using RatchetEdit.Serializers;
using static RatchetEdit.Utilities;

namespace RatchetEdit
{
    public partial class Main : Form
    {
        public enum Tool
        {
            None,
            Translate,
            Rotate,
            Scale
        }
        Tool currentTool = Tool.Translate;

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
        int currentSplineVertex = 0;
        //Input variables
        bool rMouse = false;
        bool lMouse = false;
        int lastMouseX = 0;
        int lastMouseY = 0;
        public LevelObject selectedObject;
        Vector3 mouseRay;
        Vector3 prevMouseRay;

        bool xLock = false, yLock = false, zLock = false;

        List<string> modelNames;

        Camera camera;

        TreeNode primedTreeNode = null;

        bool invalidate = false;
        bool suppressTreeViewSelectEvent = false;
        bool cancelSelection = false;
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            camera = new Camera();
            //Generate vertex array
            int VAO;
            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            //Setup openGL variables
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.LineWidth(5.0f);

            //Setup general shader
            shaderID = GL.CreateProgram();
            LoadShader("shaders/vs.glsl", ShaderType.VertexShader, shaderID);
            LoadShader("shaders/fs.glsl", ShaderType.FragmentShader, shaderID);
            GL.LinkProgram(shaderID);

            //Setup color shader
            colorShaderID = GL.CreateProgram();
            LoadShader("shaders/colorshadervs.glsl", ShaderType.VertexShader, colorShaderID);
            LoadShader("shaders/colorshaderfs.glsl", ShaderType.FragmentShader, colorShaderID);
            GL.LinkProgram(colorShaderID);

            matrixID = GL.GetUniformLocation(shaderID, "MVP");
            colorID = GL.GetUniformLocation(colorShaderID, "incolor");
            GetModelNames();

            toolLabel.Text = currentTool.ToString();
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

        private void mapOpenBtn_Click(object sender, EventArgs e)
        {
            if (mapOpenDialog.ShowDialog() == DialogResult.OK)
            {
                LoadLevel(mapOpenDialog.FileName);
            }
        }

        void LoadLevel(string fileName)
        {
            level = new Level(fileName);
            InvalidateView();
            mobyCheck.Enabled = true;
            tieCheck.Enabled = true;
            shrubCheck.Enabled = true;
            collCheck.Enabled = true;
            terrainCheck.Enabled = true;
            splineCheck.Enabled = true;
            skyboxCheck.Enabled = true;

            GenerateObjectTree();

            Moby ratchet = level.mobs[0];
            camera.MoveBehind(ratchet);
        }

        public void GenerateObjectTree()
        {
            objectTree.CollapseAll();
            objectTree.Nodes[0].Nodes.Clear();
            objectTree.Nodes[1].Nodes.Clear();
            objectTree.Nodes[2].Nodes.Clear();
            objectTree.Nodes[3].Nodes.Clear();

            foreach (Moby moby in level.mobs)
            {
                string modelName = modelNames != null ? modelNames.Find(x => x.Substring(0, 4).ToUpper() == moby.modelID.ToString("X4")) : null;
                objectTree.Nodes[0].Nodes.Add(modelName != null ? modelName.Split('=')[1].Substring(1) : moby.modelID.ToString("X"));
            }
            foreach (Tie tie in level.ties)
            {
                string tieName = tie.modelID.ToString("X");
                objectTree.Nodes[1].Nodes.Add(tieName);
            }
            foreach (Tie shrub in level.shrubs)
            {
                string shrubName = shrub.modelID.ToString("X");
                objectTree.Nodes[2].Nodes.Add(shrubName);
            }
            foreach (Spline spline in level.splines)
            {
                string splineName = spline.name.ToString("X");
                objectTree.Nodes[3].Nodes.Add(splineName);
            }
        }

        public void GetModelNames()
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
        private void OpenModelViewer()
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
            if (selectedObject == null) return;
            OpenModelViewer();
        }

        private void openModelViewerBtn_Click(object sender, EventArgs e)
        {
            if (selectedObject == null) return;
            OpenModelViewer();
        }

        private void exitToolBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateEditorValues()
        {
            properties.Refresh();
        }
        private void glControl1_Paint(object sender, PaintEventArgs e) { Render(); }

        private void Render()
        {
            //Update ui label texts
            camXLabel.Text = String.Format("X: {0}", fRound(camera.position.X, 2).ToString());
            camYLabel.Text = String.Format("Y: {0}", fRound(camera.position.Y, 2).ToString());
            camZLabel.Text = String.Format("Z: {0}", fRound(camera.position.Z, 2).ToString());
            pitchLabel.Text = String.Format("Pitch: {0}", fRound(fToDegrees(camera.rotation.X), 2).ToString());
            yawLabel.Text = String.Format("Yaw: {0}", fRound(fToDegrees(camera.rotation.Z), 2).ToString());

            //Render gl surface
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            worldView = view * projection;

            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);

            if (splineCheck.Checked && splineCheck.Enabled)
            {
                foreach (Spline spline in level.splines)
                {
                    drawSpline(spline);
                }
            }

            if (selectedObject as ModelObject != null)
            {
                drawModelMesh((ModelObject)selectedObject);
                drawTool(selectedObject.position);
            }

            GL.EnableVertexAttribArray(1);
            GL.UseProgram(shaderID);

            if (mobyCheck.Checked && mobyCheck.Enabled)
                foreach (Moby mob in level.mobs) drawModelObject(mob);

            if (tieCheck.Checked && tieCheck.Enabled)
                foreach (Tie tie in level.ties) drawModelObject(tie);

            if (shrubCheck.Checked && splineCheck.Enabled)
                foreach (Tie shrub in level.shrubs) drawModelObject(shrub);
            if (skyboxCheck.Checked && skyboxCheck.Enabled)
                drawModelModel(level.skybox);

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

        //Called every frame
        public void Tick()
        {
            float deltaTime = 0.016f;

            float moveSpeed = 10;
            float boostMultiplier = 4;
            float multiplier = ModifierKeys == Keys.Shift ? boostMultiplier : 1;

            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.F1))
            {
                currentTool = Tool.Translate;
                toolLabel.Text = currentTool.ToString();
            }
            else if (keyState.IsKeyDown(Key.F2))
            {
                currentTool = Tool.Rotate;
                toolLabel.Text = currentTool.ToString();
            }
            else if (keyState.IsKeyDown(Key.F3))
            {
                currentTool = Tool.Scale;
                toolLabel.Text = currentTool.ToString();
            }
            else if (keyState.IsKeyDown(Key.F4))
            {
                currentTool = Tool.None;
                toolLabel.Text = currentTool.ToString();
            }

            Vector3 moveDir = GetInputAxes();
            if (moveDir.Length > 0)
            {
                moveDir *= moveSpeed * multiplier * deltaTime;
                InvalidateView();
            }


            if (rMouse)
            {
                float pitch = camera.rotation.X;
                float yaw = camera.rotation.Z;
                yaw -= (Cursor.Position.X - lastMouseX) * camera.speed * 0.016f;
                pitch -= (Cursor.Position.Y - lastMouseY) * camera.speed * 0.016f;
                pitch = MathHelper.Clamp(pitch, MathHelper.DegreesToRadians(-89.9f), MathHelper.DegreesToRadians(89.9f));
                camera.SetRotation(pitch, yaw);
                InvalidateView();
            }



            Matrix3 rot = Matrix3.CreateRotationX(camera.rotation.X) * Matrix3.CreateRotationY(camera.rotation.Y) * Matrix3.CreateRotationZ(camera.rotation.Z);
            camera.Translate(Vector3.Transform(moveDir, rot));
            Vector3 forward = Vector3.Transform(Vector3.UnitY, rot);

            view = Matrix4.LookAt(camera.position, camera.position + forward, Vector3.UnitZ);


            mouseRay = MouseToWorldRay(projection, view, new Size(glControl1.Width, glControl1.Height), new Vector2(Cursor.Position.X, Cursor.Position.Y));

            if (xLock)
            {
                selectedObject.Translate((mouseRay.X -prevMouseRay.X) * 20, 0, 0);
                InvalidateView();
            }

            if (yLock)
            {
                selectedObject.Translate(0, (mouseRay.Y - prevMouseRay.Y) * 20, 0);
                InvalidateView();
            }

            if (zLock)
            {
                selectedObject.Translate(0, 0, (mouseRay.Z - prevMouseRay.Z) * 20);
                InvalidateView();
            }

            prevMouseRay = mouseRay;

            lastMouseX = Cursor.Position.X;
            lastMouseY = Cursor.Position.Y;

            if (invalidate)
            {
                glControl1.Invalidate();
            }
        }

        private Vector3 GetInputAxes()
        {
            KeyboardState keyState = Keyboard.GetState();

            float xAxis = 0, yAxis = 0, zAxis = 0;

            if (keyState.IsKeyDown(Key.W)) yAxis++;
            if (keyState.IsKeyDown(Key.S)) yAxis--;
            if (keyState.IsKeyDown(Key.A)) xAxis--;
            if (keyState.IsKeyDown(Key.D)) xAxis++;
            if (keyState.IsKeyDown(Key.Q)) zAxis--;
            if (keyState.IsKeyDown(Key.E)) zAxis++;


            return new Vector3(xAxis, yAxis, zAxis);
        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            rMouse = e.Button == MouseButtons.Right;
            lMouse = e.Button == MouseButtons.Left;

            if (e.Button == MouseButtons.Left && level != null)
            {
                LevelObject obj = GetObjectAtScreenPosition(e.Location.X, e.Location.Y);
                if (!cancelSelection)
                    SelectObject(obj);
                cancelSelection = false;

            }
        }

        private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            rMouse = false;
            lMouse = false;
            xLock = false;
            yLock = false;
            zLock = false;
            UpdateEditorValues();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL.Viewport(glControl1.Location.X, glControl1.Location.Y, glControl1.Width, glControl1.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)glControl1.Width / glControl1.Height, 0.1f, 800.0f);
        }

        private void enableCheck(object sender, EventArgs e)
        {
            InvalidateView();
        }

        public LevelObject GetObjectAtScreenPosition(int x, int y)
        {
            LevelObject returnObject = null;
            TreeNode returnNode = null;
            int offset = 0;
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);
            worldView = view * projection;

            if (mobyCheck.Checked)
            {
                GL.ClearColor(0, 0, 0, 0);
                offset = 0;
                fakeDrawObjects(level.mobs.Cast<ModelObject>().ToList(), offset);
            }
            if (tieCheck.Checked)
            {
                offset += level.mobs.Count;
                fakeDrawObjects(level.ties.Cast<ModelObject>().ToList(), offset);
            }
            if (shrubCheck.Checked)
            {
                offset += level.ties.Count;
                fakeDrawObjects(level.shrubs.Cast<ModelObject>().ToList(), offset);
            }
            if (splineCheck.Checked)
            {
                offset += level.shrubs.Count;
                fakeDrawSplines(level.splines, offset);
            }
            if (currentTool == Tool.Translate)
            {
                Console.WriteLine("Current tool is translate");
                if (selectedObject != null) drawTool(selectedObject.position);
            }

            Pixel pixel = new Pixel();
            GL.ReadPixels(x, glControl1.Height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
            GL.ClearColor(Color.SkyBlue);
            if (pixel.A == 0)
            {
                if (pixel.R == 255 && pixel.G == 0 && pixel.B == 0)
                {
                    Console.WriteLine("HIT RED!");
                    xLock = true;
                    cancelSelection = true;
                    InvalidateView();

                    return null;
                }
                else if (pixel.R == 0 && pixel.G == 255 && pixel.B == 0)
                {
                    Console.WriteLine("HIT GREEN!");
                    yLock = true;
                    cancelSelection = true;
                    InvalidateView();
                    return null;
                }
                else if (pixel.R == 0 && pixel.G == 0 && pixel.B == 255)
                {
                    Console.WriteLine("HIT BLUE!");
                    zLock = true;
                    cancelSelection = true;
                    InvalidateView();
                    return null;
                }

                int id = (int)pixel.ToUInt32();
                if (id < level.mobs?.Count)
                {
                    returnObject = level.mobs[id];
                    returnNode = objectTree.Nodes[0].Nodes[id];
                }
                else if (id - level.mobs?.Count < level.ties?.Count)
                {
                    returnObject = level.ties[id - level.mobs.Count];
                    returnNode = objectTree.Nodes[1].Nodes[id - level.mobs.Count];
                }
                else if (id - (level.mobs?.Count + level.ties?.Count) < level.shrubs?.Count)
                {
                    returnObject = level.shrubs[id - (level.mobs.Count + level.ties.Count)];
                    returnNode = objectTree.Nodes[2].Nodes[id - offset];
                }
                else if (id - offset < level.splines?.Count)
                {
                    returnObject = level.splines[id - offset];
                    returnNode = objectTree.Nodes[3].Nodes[id - offset];
                }
            }

            primedTreeNode = returnNode;

            return returnObject;
        }

        void SelectObject(LevelObject levelObject = null)
        {
            if (levelObject == null)
            {
                selectedObject = null;
                UpdateEditorValues();
                InvalidateView();
                currentSplineVertex = 0;
                return;
            }

            if (ReferenceEquals(levelObject, selectedObject) && levelObject as ModelObject != null)
            {
                OpenModelViewer();
                return;
            }

            selectedObject = levelObject;
            properties.SelectedObject = selectedObject;
            if (primedTreeNode != null)
            {
                suppressTreeViewSelectEvent = true;
                objectTree.SelectedNode = primedTreeNode;
                primedTreeNode = null;
            }
            currentSplineVertex = 0;
            UpdateEditorValues();
            InvalidateView();
        }

        public void CloneMoby(Moby moby)
        {
            Moby newMoby = moby.Clone() as Moby;
            if (newMoby == null) return;

            level.mobs.Add(newMoby);
            GenerateObjectTree();
            SelectObject(newMoby);
            InvalidateView();
        }

        public void DeleteMoby(Moby moby)
        {
            level.mobs.Remove(moby);
            GenerateObjectTree();
            SelectObject(null);
            InvalidateView();
        }
        #region RenderFunctions
        public void drawSpline(Spline spline)
        {
            Vector4 color;
            if (spline == selectedObject) color = new Vector4(1, 0, 1, 1);
            else color = new Vector4(1, 1, 1, 1);

            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);
            GL.UniformMatrix4(matrixID, false, ref worldView);
            GL.Uniform4(colorID, color);
            spline.getVBO();
            GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
        }

        public void drawTool(Vector3 position)
        {
            float[] test = new float[18];
            float length = 2;
            test[0] = position.X - length;
            test[1] = position.Y;
            test[2] = position.Z;

            test[3] = position.X + length;
            test[4] = position.Y;
            test[5] = position.Z;

            test[6] = position.X;
            test[7] = position.Y - length;
            test[8] = position.Z;

            test[9] = position.X;
            test[10] = position.Y + length;
            test[11] = position.Z;

            test[12] = position.X;
            test[13] = position.Y;
            test[14] = position.Z - length;

            test[15] = position.X;
            test[16] = position.Y;
            test[17] = position.Z + length;

            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);
            GL.UniformMatrix4(matrixID, false, ref worldView);
            int VBO;
            GL.GenBuffers(1, out VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.BufferData(BufferTarget.ArrayBuffer, test.Length * sizeof(float), test, BufferUsageHint.DynamicDraw);

            GL.Uniform4(colorID, new Vector4(1, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 0, 2);

            GL.Uniform4(colorID, new Vector4(0, 1, 0, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 2, 2);

            GL.Uniform4(colorID, new Vector4(0, 0, 1, 1));
            GL.DrawArrays(PrimitiveType.LineStrip, 4, 2);
        }

        private void drawModelObject(ModelObject obj)
        {
            if (obj.model.vertexBuffer != null)
            {
                Matrix4 mvp = obj.modelMatrix * worldView;  //Has to be done in this order to work correctly
                GL.UniformMatrix4(matrixID, false, ref mvp);
                drawMesh(obj.model);
            }
        }

        private void drawModelModel(Model model)
        {
            Matrix4 mvp = worldView;
            GL.UniformMatrix4(matrixID, false, ref mvp);
            drawMesh(model);
        }

        private void drawMesh(Model model)
        {
            model.getVBO();
            model.getIBO();

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in model.textureConfig)
            {
                GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? level.textures[conf.ID].getTexture() : 0);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }
        }

        public void drawModelMesh(ModelObject levelObject)
        {
            if (levelObject != null)
            {
                if (levelObject.model != null && levelObject.model.vertexBuffer != null && levelObject.modelMatrix != null)
                {
                    GL.UseProgram(colorShaderID);
                    GL.EnableVertexAttribArray(0);
                    Matrix4 mvp = levelObject.modelMatrix * worldView;
                    GL.Uniform4(colorID, new Vector4(1, 1, 1, 1));
                    GL.UniformMatrix4(matrixID, false, ref mvp);
                    levelObject.model.getVBO();
                    levelObject.model.getIBO();
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.DrawElements(PrimitiveType.Triangles, levelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }
        }

        public void fakeDrawSplines(List<Spline> splines, int offset)
        {
            foreach (Spline spline in splines)
            {
                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                GL.UniformMatrix4(matrixID, false, ref worldView);
                int objectIndex = splines.IndexOf(spline);
                byte[] cols = BitConverter.GetBytes(objectIndex + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                spline.getVBO();
                GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
            }
        }
        public void fakeDrawObjects(List<ModelObject> levelObjects, int offset)
        {
            foreach (ModelObject levelObject in levelObjects)
            {
                if (levelObject.model.vertexBuffer != null)
                {
                    Matrix4 mvp = levelObject.modelMatrix * worldView;  //Has to be done in this order to work correctly
                    GL.UniformMatrix4(matrixID, false, ref mvp);

                    levelObject.model.getVBO();
                    levelObject.model.getIBO();

                    int objectIndex = levelObjects.IndexOf(levelObject);
                    byte[] cols = BitConverter.GetBytes(objectIndex + offset);
                    GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                    GL.DrawElements(PrimitiveType.Triangles, levelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                }
            }
        }
        #endregion
        #region Misc Input Events
        private void objectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == objectTree.Nodes[0])
            {
                if (!suppressTreeViewSelectEvent)
                {
                    SelectObject(level.mobs[e.Node.Index]);
                    camera.MoveBehind(selectedObject);
                }
                suppressTreeViewSelectEvent = false;
            }
            if (e.Node.Parent == objectTree.Nodes[1])
            {
                if (!suppressTreeViewSelectEvent)
                {
                    SelectObject(level.ties[e.Node.Index]);
                    camera.MoveBehind(selectedObject);
                }
                suppressTreeViewSelectEvent = false;
            }
            if (e.Node.Parent == objectTree.Nodes[2])
            {
                if (!suppressTreeViewSelectEvent)
                {
                    SelectObject(level.shrubs[e.Node.Index]);
                    camera.MoveBehind(selectedObject);
                }
                suppressTreeViewSelectEvent = false;
            }
            if (e.Node.Parent == objectTree.Nodes[3])
            {
                if (!suppressTreeViewSelectEvent)
                {
                    SelectObject(level.splines[e.Node.Index]);
                    camera.MoveBehind(selectedObject);
                }
                suppressTreeViewSelectEvent = false;
            }
        }

        private void gotoPositionBtn_Click(object sender, EventArgs e)
        {
            if (selectedObject == null) return;
            camera.MoveBehind(selectedObject);
            InvalidateView();
        }

        private void cloneButton_Click(object sender, EventArgs e)
        {
            if (selectedObject as Moby != null)
            {
                Moby moby = (Moby)selectedObject;
                CloneMoby(moby);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (selectedObject as Moby != null)
            {
                Moby moby = (Moby)selectedObject;
                DeleteMoby(moby);
            }
        }
        private void splineVertex_ValueChanged(object sender, EventArgs e)
        {
            currentSplineVertex = (int)splineVertex.Value;
            UpdateEditorValues();
        }
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            Tick();
        }
        #endregion

        void InvalidateView()
        {
            invalidate = true;
        }

        struct Pixel
        {
            public byte R, G, B, A;

            public Pixel(byte[] input)
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

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            InvalidateView();
        }

        private void mapSaveAsBtn_Click(object sender, EventArgs e)
        {
            if (mapSaveDialog.ShowDialog() == DialogResult.OK)
            {
                GameplaySerializer gameplaySerializer = new GameplaySerializer();
                gameplaySerializer.Save(level, mapSaveDialog.FileName);
            }
            InvalidateView();
        }

        public static Vector3 MouseToWorldRay(Matrix4 projection, Matrix4 view, Size viewport, Vector2 mouse)
        {
            Vector3 pos1 = UnProject(ref projection, view, viewport, new Vector3(mouse.X, mouse.Y, 0.1f)); // near
            Vector3 pos2 = UnProject(ref projection, view, viewport, new Vector3(mouse.X, mouse.Y, 800f));  // far
            return pos1 - pos2;
        }

        public static Vector3 UnProject(ref Matrix4 projection, Matrix4 view, Size viewport, Vector3 mouse)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / (float)viewport.Width - 1;
            vec.Y = -(2.0f * mouse.Y / (float)viewport.Height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < -float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return new Vector3(vec.X, vec.Y, vec.Z);
        }
    }
}
