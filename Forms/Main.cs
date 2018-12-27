using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        int currentSplineVertex = 0;
        //Input variables
        bool rMouse = false;
        bool lMouse = false;
        int lastMouseX = 0;
        int lastMouseY = 0;
        public LevelObject selectedObject;

        List<string> modelNames;

        Camera camera;

        TreeNode primedTreeNode = null;

        bool invalidate = false;
        bool suppressTreeViewSelectEvent = false;
        
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

            Matrix3 rot = Matrix3.CreateRotationX(camera.rotation.X) * Matrix3.CreateRotationZ(camera.rotation.Z);
            Vector3 forward = Vector3.Transform(Vector3.UnitY, rot);
            view = Matrix4.LookAt(camera.position, camera.position + forward, Vector3.UnitZ);

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

        private void mapOpenBtn_Click(object sender, EventArgs e) {
            if (mapOpenDialog.ShowDialog() == DialogResult.OK) {
                loadLevel(mapOpenDialog.FileName);
            }
        }

        void loadLevel(string fileName) {
            level = new Level(fileName);
            invalidate = true;
            mobyCheck.Enabled = true;
            tieCheck.Enabled = true;
            shrubCheck.Enabled = true;
            collCheck.Enabled = true;
            terrainCheck.Enabled = true;
            splineCheck.Enabled = true;

            GenerateObjectTree();

            Moby ratchet = level.mobs[0];
            camera.moveBehind(ratchet);
        }

        public void GenerateObjectTree() {
            objectTree.CollapseAll();
            objectTree.Nodes[0].Nodes.Clear();
            objectTree.Nodes[1].Nodes.Clear();
            objectTree.Nodes[2].Nodes.Clear();
            objectTree.Nodes[3].Nodes.Clear();

            foreach (Moby moby in level.mobs) {
                string modelName = modelNames != null ? modelNames.Find(x => x.Substring(0, 4).ToUpper() == moby.modelID.ToString("X4")) : null;
                objectTree.Nodes[0].Nodes.Add(modelName != null ? modelName.Split('=')[1].Substring(1) : moby.modelID.ToString("X"));
            }
            foreach (Tie tie in level.ties) {
                string tieName = tie.modelID.ToString("X");
                objectTree.Nodes[1].Nodes.Add(tieName);
            }
            foreach (Tie shrub in level.shrubs) {
                string shrubName = shrub.modelID.ToString("X");
                objectTree.Nodes[2].Nodes.Add(shrubName);
            }
            foreach (Spline spline in level.splines) {
                string splineName = spline.name.ToString("X");
                objectTree.Nodes[3].Nodes.Add(splineName);
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

        private void modelViewerToolBtn_Click(object sender, EventArgs e) {
            if (selectedObject == null) return;
            openModelViewer();
        }

        private void openModelViewerBtn_Click(object sender, EventArgs e) {
            if (selectedObject == null) return;
            openModelViewer();
        }

        private void exitToolBtn_Click(object sender, EventArgs e) {
            Close();
        }

        private void updateEditorValues() {
            if(selectedObject != null) {
                if (selectedObject as ModelObject != null) {
                    modelIDBox.Text = ((ModelObject)selectedObject).modelID.ToString("X");
                }

                if (selectedObject as Moby != null) {
                    Moby mob = (Moby)selectedObject;
                    rotxBox.Value = (decimal)Utilities.toDegrees(mob.rotation.X);
                    rotyBox.Value = (decimal)Utilities.toDegrees(mob.rotation.Y);
                    rotzBox.Value = (decimal)Utilities.toDegrees(mob.rotation.Z);
                    scaleBox.Value = (decimal)mob.scale;
                }
                else {
                    rotxBox.Value = 0;
                    rotyBox.Value = 0;
                    rotzBox.Value = 0;
                    scaleBox.Value = 1;
                }

                if (selectedObject as Spline != null) {
                    Spline spline = (Spline)selectedObject;
                    Console.WriteLine(currentSplineVertex.ToString());
                    xBox.Value = (decimal)spline.vertexBuffer[(currentSplineVertex * 3) + 0];
                    yBox.Value = (decimal)spline.vertexBuffer[(currentSplineVertex * 3) + 1];
                    zBox.Value = (decimal)spline.vertexBuffer[(currentSplineVertex * 3) + 2];
                }
                else {
                    xBox.Value = (decimal)selectedObject.position.X;
                    yBox.Value = (decimal)selectedObject.position.Y;
                    zBox.Value = (decimal)selectedObject.position.Z;
                }
            }
            else {
                modelIDBox.Text = "0";
                splineVertex.Value = 0;
                xBox.Value = 0;
                yBox.Value = 0;
                zBox.Value = 0;
                rotxBox.Value = 0;
                rotyBox.Value = 0;
                rotzBox.Value = 0;
                scaleBox.Value = 1;
            }
        }
        private void glControl1_Paint(object sender, PaintEventArgs e) { render(); }

        private void render() {
            //Update ui label texts
            camXLabel.Text = String.Format("X: {0}", Utilities.round(camera.position.X,2).ToString());
            camYLabel.Text = String.Format("Y: {0}", Utilities.round(camera.position.Y, 2).ToString());
            camZLabel.Text = String.Format("Z: {0}", Utilities.round(camera.position.Z, 2).ToString());
            pitchLabel.Text = String.Format("Pitch: {0}", Utilities.round(Utilities.toDegrees(camera.rotation.X), 2).ToString());
            yawLabel.Text = String.Format("Yaw: {0}", Utilities.round(Utilities.toDegrees(camera.rotation.Z), 2).ToString());
            

            //Render gl surface
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            worldView = view * projection;

            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);

            if (splineCheck.Checked && splineCheck.Enabled) {
                foreach (Spline spline in level.splines) {
                    drawSpline(spline);
                }
            }

            if (selectedObject as ModelObject != null) {
                drawModelMesh((ModelObject)selectedObject);
            }

            GL.EnableVertexAttribArray(1);
            GL.UseProgram(shaderID);

            if (mobyCheck.Checked && mobyCheck.Enabled) 
                foreach (Moby mob in level.mobs) drawModelObject(mob);

            if (tieCheck.Checked && tieCheck.Enabled)
                foreach (Tie tie in level.ties) drawModelObject(tie);

            if (shrubCheck.Checked && splineCheck.Enabled)
                foreach (Tie shrub in level.shrubs)  drawModelObject(shrub);
            
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
        public void tick() {
            float deltaTime = 0.016f;

            float moveSpeed = 25;
            float shiftMultiplier = 3;
            float nonShiftMultiplier = 0.5f;

            Vector3 moveDir = getInputAxes();
            if (moveDir.Length > 0) {
                moveDir *= (ModifierKeys == Keys.Shift ? shiftMultiplier : nonShiftMultiplier) * deltaTime * moveSpeed;
                invalidate = true;
            }


            if (rMouse) {
                float pitch = camera.rotation.X;
                float yaw = camera.rotation.Z;
                yaw -= (Cursor.Position.X - lastMouseX) * camera.speed * 0.016f;
                pitch -= (Cursor.Position.Y - lastMouseY) * camera.speed * 0.016f;
                pitch = MathHelper.Clamp(pitch, MathHelper.DegreesToRadians(-89.9f), MathHelper.DegreesToRadians(89.9f));
                camera.setRotation(pitch, yaw);
                invalidate = true;
            }

            Matrix3 rot = Matrix3.CreateRotationX(camera.rotation.X) * Matrix3.CreateRotationZ(camera.rotation.Z);
            camera.setPosition(camera.position + Vector3.Transform(moveDir, rot));

            Vector3 forward = Vector3.Transform(Vector3.UnitY, rot);

            lastMouseX = Cursor.Position.X;
            lastMouseY = Cursor.Position.Y;

            view = Matrix4.LookAt(camera.position, camera.position + forward, Vector3.UnitZ);

            if (invalidate) {
                glControl1.Invalidate();
            }
        }

        private Vector3 getInputAxes() {
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

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            rMouse = e.Button == MouseButtons.Right;
            lMouse = e.Button == MouseButtons.Left;
        }

        private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            rMouse = false;
            lMouse = false;
        }

        private void glControl1_Resize(object sender, EventArgs e) {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)glControl1.Width / glControl1.Height, 0.1f, 800.0f);
        }

        private void enableCheck(object sender, EventArgs e) {
            invalidate = true;
        }

        private void glControl1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Left && level != null)
            {
                LevelObject obj = GetObjectAtScreenPosition(e.Location.X, e.Location.Y);
                SelectObject(obj);

            }
        }

        public LevelObject GetObjectAtScreenPosition(int x, int y) {
            LevelObject returnObject = null;
            TreeNode returnNode = null;
            int offset = 0;
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);
            worldView = view * projection;

            if (mobyCheck.Checked) {
                GL.ClearColor(0, 0, 0, 0);
                offset = 0;
                fakeDrawObjects(level.mobs.Cast<ModelObject>().ToList(), offset);
            }
            if (tieCheck.Checked) {
                offset += level.mobs.Count;
                fakeDrawObjects(level.ties.Cast<ModelObject>().ToList(), offset);
            }
            if (shrubCheck.Checked) {
                offset += level.ties.Count;
                fakeDrawObjects(level.shrubs.Cast<ModelObject>().ToList(), offset);
            }
            if (splineCheck.Checked) {
                offset += level.shrubs.Count;
                fakeDrawSplines(level.splines, offset);
            }

            Pixel pixel = new Pixel();
            GL.ReadPixels(x, glControl1.Height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
            GL.ClearColor(Color.SkyBlue);
            if (pixel.A == 0) {
                int id = (int)pixel.ToUInt32();
                if (id < level.mobs?.Count) {
                    returnObject = level.mobs[id];
                    returnNode = objectTree.Nodes[0].Nodes[id];
                }
                else if (id - level.mobs?.Count < level.ties?.Count) {
                    returnObject = level.ties[id - level.mobs.Count];
                    returnNode = objectTree.Nodes[1].Nodes[id - level.mobs.Count];
                }
                else if (id - (level.mobs?.Count + level.ties?.Count) < level.shrubs?.Count) {
                    returnObject = level.shrubs[id - offset];
                    returnNode = objectTree.Nodes[2].Nodes[id - offset];
                }
                else if (id - offset < level.splines?.Count) {
                    returnObject = level.splines[id - offset];
                    returnNode = objectTree.Nodes[3].Nodes[id - offset];
                }
            }

           primedTreeNode = returnNode;

            return returnObject;
        }
        
        void SelectObject(LevelObject levelObject = null) {
            Console.WriteLine(levelObject);
            if (levelObject == null) {
                selectedObject = null;
                updateEditorValues();
                invalidate = true;
                currentSplineVertex = 0;
                return;
            }
            
            if (ReferenceEquals(levelObject,selectedObject) && levelObject as ModelObject != null) {
                openModelViewer();
                return;
            }

            selectedObject = levelObject;
            if (primedTreeNode != null) {
                suppressTreeViewSelectEvent = true;
                objectTree.SelectedNode = primedTreeNode;
                primedTreeNode = null;
            }
            currentSplineVertex = 0;
            updateEditorValues();
            invalidate = true;
        }

        public void CloneMoby(Moby moby) {
            Moby newMoby = moby.CloneMoby();
            level.mobs.Add(newMoby);
            GenerateObjectTree();
            SelectObject(newMoby);
            invalidate = true;
        }

        public void DeleteMoby(Moby moby) {
            level.mobs.Remove(moby);
            GenerateObjectTree();
            SelectObject(null);
            invalidate = true;
        }
        #region RenderFunctions
        public void drawSpline(Spline spline) {
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

        private void drawModelObject(ModelObject obj) {
            if (obj.model.vertexBuffer != null) {
                Matrix4 mvp = obj.modelMatrix * worldView;  //Has to be done in this order to work correctly
                GL.UniformMatrix4(matrixID, false, ref mvp);

                obj.model.getVBO();
                obj.model.getIBO();

                //Bind textures one by one, applying it to the relevant vertices based on the index array
                foreach (TextureConfig conf in obj.model.textureConfig) {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? level.textures[conf.ID].getTexture() : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }
            }
        }

        public void drawModelMesh(ModelObject levelObject) {
            if (levelObject != null) {
                if (levelObject.model != null && levelObject.model.vertexBuffer != null && levelObject.modelMatrix != null) {
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

        public void fakeDrawSplines(List<Spline> splines, int offset) {
            foreach (Spline spline in splines) {
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
        public void fakeDrawObjects(List<ModelObject> levelObjects, int offset) {
            foreach (ModelObject levelObject in levelObjects) {
                if (levelObject.model.vertexBuffer != null) {
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

        #region Position Input Events
        private void xBox_ValueChanged(object sender, EventArgs e) {
            if (selectedObject == null) return;

            if(selectedObject as Spline != null) {
                Spline spline = (Spline)selectedObject;
                spline.vertexBuffer[(currentSplineVertex * 3) + 0] = (float)xBox.Value;
            }
            else {
                Vector3 position = selectedObject.position;
                selectedObject.position = new Vector3((float)xBox.Value, position.Y, position.Z);
            }
            invalidate = true;
        }

        private void yBox_ValueChanged(object sender, EventArgs e) {
            if (selectedObject == null) return;
            if (selectedObject as Spline != null) {
                Spline spline = (Spline)selectedObject;
                spline.vertexBuffer[(currentSplineVertex * 3) + 1] = (float)yBox.Value;
            }
            else {
                Vector3 position = selectedObject.position;
                selectedObject.position = new Vector3(position.X, (float)yBox.Value, position.Z);
            }

            invalidate = true;
        }

        private void zBox_ValueChanged(object sender, EventArgs e)
        {
            if (selectedObject == null) return;
            if (selectedObject as Spline != null) {
                Spline spline = (Spline)selectedObject;
                spline.vertexBuffer[(currentSplineVertex * 3) + 2] = (float)zBox.Value;
            }
            else {
                Vector3 position = selectedObject.position;
                selectedObject.position = new Vector3(position.X, position.Y, (float)zBox.Value);
            }
            invalidate = true;
        }
        #endregion
        #region Rotation Input Events
        private void rotxBox_ValueChanged(object sender, EventArgs e) {
            if (selectedObject as Moby != null)
            {
                Moby moby = (Moby)selectedObject;
                Vector3 rotation = moby.rotation;
                float value = Utilities.toRadians((float)rotxBox.Value);
                moby.rotation = new Vector3(value, rotation.Y, rotation.Z);
                invalidate = true;
            }
        }

        private void rotyBox_ValueChanged(object sender, EventArgs e) {
            if (selectedObject as Moby != null)
            {
                Moby moby = (Moby)selectedObject;
                Vector3 rotation = moby.rotation;
                float value = Utilities.toRadians((float)rotyBox.Value);
                moby.rotation = new Vector3(rotation.X, value, rotation.Z);
                invalidate = true;
            }
        }

        private void rotzBox_ValueChanged(object sender, EventArgs e) {
            if (selectedObject as Moby != null)
            {
                Moby moby = (Moby)selectedObject;
                Vector3 rotation = moby.rotation;
                float value = Utilities.toRadians((float)rotzBox.Value);
                moby.rotation = new Vector3(rotation.X, rotation.Y, value);
                invalidate = true;
            }
        }
        #endregion
        #region Misc Input Events
        private void objectTree_AfterSelect(object sender, TreeViewEventArgs e) {
            if (e.Node.Parent == objectTree.Nodes[0]) {
                if (!suppressTreeViewSelectEvent) {
                    SelectObject(level.mobs[e.Node.Index]);
                }
                suppressTreeViewSelectEvent = false;
            }
        }

        private void gotoPositionBtn_Click(object sender, EventArgs e) {
            if (selectedObject == null) return;
            camera.moveBehind(selectedObject);
            invalidate = true;
        }

        private void scaleBox_ValueChanged(object sender, EventArgs e) {
            if (selectedObject as Moby != null) {
                Moby moby = (Moby)selectedObject;
                moby.scale = (float)scaleBox.Value;
                moby.updateTransform();
                invalidate = true;
            }
        }

        private void cloneButton_Click(object sender, EventArgs e) {
            if (selectedObject as Moby != null) {
                Moby moby = (Moby)selectedObject;
                CloneMoby(moby);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e) {
            if (selectedObject as Moby != null) {
                Moby moby = (Moby)selectedObject;
                DeleteMoby(moby);
            }
        }
        private void tickTimer_Tick(object sender, EventArgs e) {
            tick();
        }
        #endregion

        struct Pixel {
            public byte R, G, B, A;

            public Pixel(byte[] input) {
                R = input[0];
                G = input[1];
                B = input[2];
                A = input[3];
            }

            public uint ToUInt32() {
                byte[] temp = new byte[] { R, G, B, A };
                return BitConverter.ToUInt32(temp, 0);
            }

            public override string ToString() {
                return R + ", " + G + ", " + B + ", " + A;
            }
        }

        private void splineVertex_ValueChanged(object sender, EventArgs e) {
            currentSplineVertex = (int)splineVertex.Value;
            Console.WriteLine(splineVertex.Value.ToString());
            updateEditorValues();
        }
    }
}
