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
using RatchetEdit.Tools;
using static RatchetEdit.Utilities;

namespace RatchetEdit
{
    public partial class Main : Form
    {

        Tool currentTool;
        Dictionary<Tool.ToolType, Tool> tools = new Dictionary<Tool.ToolType, Tool>();

        public Level level;
        public ModelViewer modelViewer;

        //Input variables
        bool rMouse = false;
        bool lMouse = false;
        int lastMouseX = 0;
        int lastMouseY = 0;
        public LevelObject selectedObject;
        Vector3 prevMouseRay;

        bool xLock = false, yLock = false, zLock = false;

        List<string> mobyNames;
        List<string> tieNames;

        Camera camera;

        TreeNode primedTreeNode = null;

        int currentSplineVertex = 0;

        bool invalidate = false;
        bool suppressTreeViewSelectEvent = false;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            glControl1.MakeCurrent();
            glControl1.InitializeGLConfig();
            camera = new Camera();
            InitializeObjectTree();
            InitializeToolList();

            GetModelNames();

            SelectTool(Tool.ToolType.Translate);
        }

        private void InitializeObjectTree() {
            TreeNode treeNode1 = new TreeNode("Mobys") { Name = "mobyNode" };
            TreeNode treeNode2 = new TreeNode("Ties") { Name = "tieNode" };
            TreeNode treeNode3 = new TreeNode("Shrubs") { Name = "shrubNode" };
            TreeNode treeNode4 = new TreeNode("Splines") { Name = "splineNode" };
            TreeNode treeNode5 = new TreeNode("Game Cameras") { Name = "gameCameraNode" };
            TreeNode treeNode6 = new TreeNode("Spawn points") { Name = "spawnPointNode" };
            TreeNode treeNode7 = new TreeNode("Type0Cs") { Name = "Type0CNode" };
            TreeNode[] objectTypes = new TreeNode[] { treeNode1, treeNode2, treeNode3, treeNode4, treeNode5, treeNode6, treeNode7 };
            objectTree.Nodes.AddRange(objectTypes);

        }

        private void InitializeToolList() {
            tools.Add(Tool.ToolType.Translate, new TranslationTool());
            tools.Add(Tool.ToolType.Rotate, new RotationTool());
            tools.Add(Tool.ToolType.Scale, new ScalingTool());
            tools.Add(Tool.ToolType.VertexTranslator, new VertexTranslationTool());
            tools.Add(Tool.ToolType.None, new TranslationTool());
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

            //Enable all the buttons in the view tab
            mobyCheck.Enabled = true;
            tieCheck.Enabled = true;
            shrubCheck.Enabled = true;
            collCheck.Enabled = true;
            terrainCheck.Enabled = true;
            splineCheck.Enabled = true;
            skyboxCheck.Enabled = true;
            cuboidCheck.Enabled = true;

            glControl1.textures = level.textures;

            GenerateObjectTree();

            Moby ratchet = level.mobs[0];
            camera.MoveBehind(ratchet);

            UpdateEditorValues();
            InvalidateView();
        }

        public void GenerateObjectTree()
        {
            objectTree.CollapseAll();

            foreach (TreeNode treeNode in objectTree.Nodes)
            {
                treeNode.Nodes.Clear();
            }
            TreeNode mobyNode = objectTree.Nodes.Find("mobyNode", false)[0];
            TreeNode tieNode = objectTree.Nodes.Find("tieNode", false)[0];
            TreeNode shrubNode = objectTree.Nodes.Find("shrubNode", false)[0];

            foreach (Moby levelObject in level.mobs)
            {
                string line = mobyNames != null ? mobyNames.Find(x => x.Substring(0, 4).ToUpper() == levelObject.modelID.ToString("X4")) : null;
                string modelName = line != null ? line.Split('=')[1].Substring(1) : levelObject.modelID.ToString("X");
                mobyNode.Nodes.Add(modelName);
            }
            foreach (Tie levelObject in level.ties)
            {
                string line = tieNames != null ? tieNames.Find(x => x.Substring(0, 4).ToUpper() == levelObject.modelID.ToString("X4")) : null;
                string modelName = line != null ? line.Split('=')[1].Substring(1) : levelObject.modelID.ToString("X");
                tieNode.Nodes.Add(modelName);
            }
            foreach (Tie levelObject in level.shrubs)
            {
                string line = tieNames != null ? tieNames.Find(x => x.Substring(0, 4).ToUpper() == levelObject.modelID.ToString("X4")) : null;
                string modelName = line != null ? line.Split('=')[1].Substring(1) : levelObject.modelID.ToString("X");
                shrubNode.Nodes.Add(modelName);
            }
            foreach (Spline spline in level.splines)
            {
                string splineName = spline.name.ToString("X");
                objectTree.Nodes[3].Nodes.Add(splineName);
            }
            foreach (GameCamera gameCamera in level.gameCameras)
            {
                string name = gameCamera.id.ToString("X");
                objectTree.Nodes[4].Nodes.Add(name);
            }
            foreach (SpawnPoint spawnPoints in level.spawnPoints)
            {
                string name = level.spawnPoints.IndexOf(spawnPoints).ToString("X");
                objectTree.Nodes[5].Nodes.Add(name);
            }
            foreach (Type0C objs in level.type0Cs)
            {
                string name = level.type0Cs.IndexOf(objs).ToString("X");
                objectTree.Nodes[6].Nodes.Add(name);
            }
        }

        public void GetModelNames()
        {
            GetMobyNames();
            GetTieNames();
        }

        private void GetTieNames() {
            tieNames = new List<string>();
            string stringCounter;
            StreamReader stream = null;
            try {
                stream = new StreamReader(Application.StartupPath + "/TieModelsRC1.txt");
                //Console.WriteLine("Loaded model names for Ratchet & Clank.");

            }
            catch (FileNotFoundException e) {
                Console.WriteLine(e);
                Console.WriteLine("Model list file not found! No names for you!");
                tieNames = null;
                return;
            }
            while ((stringCounter = stream.ReadLine()) != null) {
                tieNames.Add(stringCounter);
            }
            stream.Close();
        }

        private void GetMobyNames() {
            mobyNames = new List<string>();
            string stringCounter;
            StreamReader stream = null;
            try {
                stream = new StreamReader(Application.StartupPath + "/ModelListRC1.txt");
                //Console.WriteLine("Loaded model names for Ratchet & Clank.");

            }
            catch (FileNotFoundException e) {
                Console.WriteLine(e);
                Console.WriteLine("Model list file not found! No names for you!");
                mobyNames = null;
                return;
            }
            while ((stringCounter = stream.ReadLine()) != null) {
                mobyNames.Add(stringCounter);
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

        private void exitToolBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateEditorValues()
        {
            properties.Refresh();
        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            //Update ui label texts
            camXLabel.Text = String.Format("X: {0}", fRound(camera.position.X, 2).ToString());
            camYLabel.Text = String.Format("Y: {0}", fRound(camera.position.Y, 2).ToString());
            camZLabel.Text = String.Format("Z: {0}", fRound(camera.position.Z, 2).ToString());
            pitchLabel.Text = String.Format("Pitch: {0}", fRound(fToDegrees(camera.rotation.X), 2).ToString());
            yawLabel.Text = String.Format("Yaw: {0}", fRound(fToDegrees(camera.rotation.Z), 2).ToString());

            glControl1.MakeCurrent();

            if (mobyCheck.Checked && mobyCheck.Enabled)
                foreach (Moby mob in level.mobs)
                    mob.Render(glControl1, mob == selectedObject);

            if (tieCheck.Checked && tieCheck.Enabled)
                foreach (Tie tie in level.ties)
                    tie.Render(glControl1, tie == selectedObject);

            if (shrubCheck.Checked && splineCheck.Enabled)
                foreach (Tie shrub in level.shrubs)
                    shrub.Render(glControl1, shrub == selectedObject);

            if (splineCheck.Checked && splineCheck.Enabled)
                foreach (Spline spline in level.splines)
                    spline.Render(glControl1, spline == selectedObject);

			if(cuboidCheck.Checked && cuboidCheck.Enabled)
                foreach (SpawnPoint cuboid in level.spawnPoints)
                    cuboid.Render(glControl1, cuboid == selectedObject);

            if (cuboidCheck.Checked && cuboidCheck.Enabled)
                foreach (Type0C cuboid in level.type0Cs)
                    cuboid.Render(glControl1, cuboid == selectedObject);


            if (skyboxCheck.Checked && skyboxCheck.Enabled)
                level.skybox.Draw(glControl1);

            if (terrainCheck.Checked && terrainCheck.Enabled)
                foreach (TerrainModel terrainModel in level.terrains)
                    terrainModel.Draw(glControl1);

            RenderTool();

             invalidate = false;
        }

        //Called every frame
        public void Tick()
        {
            float deltaTime = 0.016f;

            float moveSpeed = 10;
            float boostMultiplier = 4;
            float multiplier = ModifierKeys == Keys.Shift ? boostMultiplier : 1;

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

            Vector3 moveDir = GetInputAxes();
            if (moveDir.Length > 0)
            {
                moveDir *= moveSpeed * multiplier * deltaTime;
                InvalidateView();
            }
            camera.Translate(Vector3.Transform(moveDir, camera.GetRotationMatrix()));

            glControl1.view = camera.GetViewMatrix();

            Vector3 mouseRay = MouseToWorldRay(glControl1.projection, glControl1.view, new Size(glControl1.Width, glControl1.Height), new Vector2(Cursor.Position.X, Cursor.Position.Y));
            bool toolIsBeingDragged = xLock || yLock || zLock;
            if (toolIsBeingDragged)
            {
                Vector3 direction = Vector3.Zero;
                if (xLock) direction = Vector3.UnitX;
                else if (yLock) direction = Vector3.UnitY;
                else if (zLock) direction = Vector3.UnitZ;
                float magnitudeMultiplier = 20;
                Vector3 magnitude = (mouseRay - prevMouseRay) * magnitudeMultiplier;
                Tool.ToolType toolType = currentTool.GetToolType();

                if (toolType == Tool.ToolType.Translate) {
                     selectedObject.Translate(direction * magnitude);
                }
                else if (toolType == Tool.ToolType.Rotate) {
                    selectedObject.Rotate(direction * magnitude);
                }
                else if (toolType == Tool.ToolType.Scale) {
                    selectedObject.Scale(direction * magnitude + Vector3.One);
                }
                else if (toolType == Tool.ToolType.VertexTranslator)
                {
                    if (selectedObject as Spline == null) return;
                    Spline spline = (Spline)selectedObject;
                    spline.TranslateVertex(currentSplineVertex, direction * magnitude);
                }
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

            if (glControl1.Focused) {
                if (keyState.IsKeyDown(Key.W)) yAxis++;
                if (keyState.IsKeyDown(Key.S)) yAxis--;
                if (keyState.IsKeyDown(Key.A)) xAxis--;
                if (keyState.IsKeyDown(Key.D)) xAxis++;
                if (keyState.IsKeyDown(Key.Q)) zAxis--;
                if (keyState.IsKeyDown(Key.E)) zAxis++;
            }


            return new Vector3(xAxis, yAxis, zAxis);
        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            rMouse = e.Button == MouseButtons.Right;
            lMouse = e.Button == MouseButtons.Left;

            if (e.Button == MouseButtons.Left && level != null)
            {
                bool cancelSelection;
                LevelObject obj = GetObjectAtScreenPosition(e.Location.X, e.Location.Y, out cancelSelection);

                if (cancelSelection) return;

                SelectObject(obj);
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

        private void glControl1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (selectedObject as Spline == null) return;
            if (currentTool.GetToolType() != Tool.ToolType.VertexTranslator) return;

            int delta = e.Delta / 120;
            if (delta > 0)
            {
                if (currentSplineVertex < ((Spline)selectedObject).GetVertexCount() - 1)
                {
                    currentSplineVertex += 1;
                }
            }
            else if (currentSplineVertex > 0)
            {
                currentSplineVertex -= 1;
            }
            InvalidateView();
        }


        #region RenderFunctions

        public LevelObject GetObjectAtScreenPosition(int x, int y, out bool hitTool)
        {
            LevelObject returnObject = null;
            TreeNode returnNode = null;
            int mobyOffset = 0, tieOffset = 0, shrubOffset = 0, splineOffset = 0, cuboidOffset = 0;
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(glControl1.colorShaderID);
            GL.EnableVertexAttribArray(0);
            GL.ClearColor(0, 0, 0, 0);

            glControl1.worldView = glControl1.view * glControl1.projection;

            int offset = 0;
            if (mobyCheck.Checked && mobyCheck.Enabled)
            {
                mobyOffset = offset;
                fakeDrawObjects(level.mobs.Cast<ModelObject>().ToList(), mobyOffset);
                offset += level.mobs.Count;
            }
            if (tieCheck.Checked && tieCheck.Enabled)
            {
                tieOffset = offset;
                fakeDrawObjects(level.ties.Cast<ModelObject>().ToList(), tieOffset);
                offset += level.ties.Count;
            }
            if (shrubCheck.Checked && shrubCheck.Enabled)
            {
                shrubOffset = offset;
                fakeDrawObjects(level.shrubs.Cast<ModelObject>().ToList(), shrubOffset);
                offset += level.shrubs.Count;
            }
            if (splineCheck.Checked && splineCheck.Enabled) {
                splineOffset = offset;
                fakeDrawSplines(level.splines, splineOffset);
                offset += level.splines.Count;
            }
            if (cuboidCheck.Checked && cuboidCheck.Enabled) {
                cuboidOffset = offset;
                fakeDrawCuboids(level.spawnPoints, cuboidOffset);
                offset += level.spawnPoints.Count;
            }
            
            RenderTool();

            Pixel pixel = new Pixel();
            GL.ReadPixels(x, glControl1.Height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
            GL.ClearColor(Color.SkyBlue);
            if (pixel.A == 0)
            {
                bool didHitTool = false;
                if (pixel.R == 255 && pixel.G == 0 && pixel.B == 0)
                {
                    didHitTool = true;
                    xLock = true;
                }
                else if (pixel.R == 0 && pixel.G == 255 && pixel.B == 0)
                {
                    didHitTool = true;
                    yLock = true;
                }
                else if (pixel.R == 0 && pixel.G == 0 && pixel.B == 255)
                {
                    didHitTool = true;
                    zLock = true;
                }

                if (didHitTool)
                {
                    InvalidateView();
                    hitTool = true;
                    return null;
                }

                int id = (int)pixel.ToUInt32();
                if (mobyCheck.Checked && id < level.mobs?.Count)
                {
                    returnObject = level.mobs[id];
                    returnNode = objectTree.Nodes[0].Nodes[id];
                }
                else if (tieCheck.Checked && id - tieOffset < level.ties.Count)
                {
                    returnObject = level.ties[id - tieOffset];
                    returnNode = objectTree.Nodes[1].Nodes[id - tieOffset];
                }
                else if (shrubCheck.Checked && id - shrubOffset < level.shrubs.Count)
                {
                    returnObject = level.shrubs[id - shrubOffset];
                    returnNode = objectTree.Nodes[2].Nodes[id - shrubOffset];
                }
                else if (splineCheck.Checked && id - splineOffset < level.splines.Count) {
                    returnObject = level.splines[id - splineOffset];
                    returnNode = objectTree.Nodes[3].Nodes[id - splineOffset];
                }
                else if (cuboidCheck.Checked && id - cuboidOffset < level.spawnPoints.Count) {
                    returnObject = level.spawnPoints[id - cuboidOffset];
                    returnNode = objectTree.Nodes[5].Nodes[id - cuboidOffset];
                }
            }

            primedTreeNode = returnNode;

            hitTool = false;
            return returnObject;
        }

        public void RenderTool() {
            GL.Clear(ClearBufferMask.DepthBufferBit); //Makes sure tool is rendered on top.

            if (selectedObject != null) {
                if (currentTool.GetToolType() == Tool.ToolType.VertexTranslator) {
                    if (selectedObject as Spline != null) {
                        Spline spline = (Spline)selectedObject;
                        currentTool.Render(spline.GetVertex(currentSplineVertex), glControl1);
                    }
                }
                else {
                    currentTool.Render(selectedObject.position, glControl1);
                }
            }
        }


        public void fakeDrawSplines(List<Spline> splines, int offset) {
            foreach (Spline spline in splines) {
                GL.UseProgram(glControl1.colorShaderID);
                GL.EnableVertexAttribArray(0);
                var worldView = glControl1.worldView;
                GL.UniformMatrix4(glControl1.matrixID, false, ref worldView);
                int objectIndex = splines.IndexOf(spline);
                byte[] cols = BitConverter.GetBytes(objectIndex + offset);
                GL.Uniform4(glControl1.colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                spline.GetVBO();
                GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
            }
        }
        public void fakeDrawCuboids(List<SpawnPoint> cuboids, int offset) {
            foreach (SpawnPoint cuboid in cuboids) {
                GL.UseProgram(glControl1.colorShaderID);
                GL.EnableVertexAttribArray(0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                Matrix4 mvp = cuboid.modelMatrix * glControl1.worldView;
                GL.UniformMatrix4(glControl1.matrixID, false, ref mvp);
                int objectIndex = cuboids.IndexOf(cuboid);
                byte[] cols = BitConverter.GetBytes(objectIndex + offset);
                GL.Uniform4(glControl1.colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));

                cuboid.GetVBO();
                cuboid.GetIBO();

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawElements(PrimitiveType.Triangles, SpawnPoint.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }
        public void fakeDrawObjects(List<ModelObject> levelObjects, int offset)
        {
            foreach (ModelObject levelObject in levelObjects)
            {
                if (levelObject.model == null || levelObject.model.vertexBuffer == null)
                    continue;

                Matrix4 mvp = levelObject.modelMatrix * glControl1.worldView;  //Has to be done in this order to work correctly
                GL.UniformMatrix4(glControl1.matrixID, false, ref mvp);

                levelObject.model.GetVBO();
                levelObject.model.GetIBO();

                int objectIndex = levelObjects.IndexOf(levelObject);
                byte[] cols = BitConverter.GetBytes(objectIndex + offset);
                GL.Uniform4(glControl1.colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                GL.DrawElements(PrimitiveType.Triangles, levelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);

            }
        }
        #endregion

        void SelectObject(LevelObject levelObject = null)
        {
            if (levelObject == null)
            {
                selectedObject = null;
                UpdateEditorValues();
                InvalidateView();
                return;
            }

            if (ReferenceEquals(levelObject, selectedObject) && levelObject as ModelObject != null)
            {
                OpenModelViewer();
                return;
            }

            if (selectedObject as Spline != null && levelObject as Spline == null) 
            {
                //Previous object was spline, new isn't
                if (currentTool.GetToolType() == Tool.ToolType.VertexTranslator) SelectTool(Tool.ToolType.None);
            }

            selectedObject = levelObject;
            properties.SelectedObject = selectedObject;
            if (primedTreeNode != null)
            {
                suppressTreeViewSelectEvent = true;
                objectTree.SelectedNode = primedTreeNode;
                primedTreeNode = null;
            }
            UpdateEditorValues();
            InvalidateView();
        }

        void SelectTool(Tool.ToolType tool) {
            translateToolBtn.Checked = (tool == Tool.ToolType.Translate);
            rotateToolBtn.Checked = (tool == Tool.ToolType.Rotate);
            scaleToolBtn.Checked = (tool == Tool.ToolType.Scale);
            splineToolBtn.Checked = (tool == Tool.ToolType.VertexTranslator);
            currentTool = tools[tool];

            currentSplineVertex = 0;

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
            int index = level.mobs.IndexOf(moby);
            level.mobs.Remove(moby);
            objectTree.Nodes[0].Nodes[index].Remove();
            //GenerateObjectTree();
            SelectObject(null);
            InvalidateView();
        }


        public int GetShaderID()
        {
            return glControl1.shaderID;
        }

        void InvalidateView()
        {
            invalidate = true;
        }

        #region Misc Input Events

        private void glControl1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
            char key = Char.ToUpper(e.KeyChar);

            if (key == (char)Keys.D1) {
                SelectTool(Tool.ToolType.Translate);
            }
            else if (key == (char)Keys.D2) {
                SelectTool(Tool.ToolType.Rotate);
            }
            else if (key == (char)Keys.D3) {
                SelectTool(Tool.ToolType.Scale);
            }
            else if (key == (char)Keys.D4) {
                SelectTool(Tool.ToolType.VertexTranslator);
            }
            else if (key == (char)Keys.D5) {
                SelectTool(Tool.ToolType.None);
            }

        }

        private void EnableCheck(object sender, EventArgs e)
        {
            InvalidateView();
        }

        private void objectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (suppressTreeViewSelectEvent)
            {
                suppressTreeViewSelectEvent = false;
                return;
            }

            if (e.Node.Parent == objectTree.Nodes[0])
            {
                SelectObject(level.mobs[e.Node.Index]);
            }
            if (e.Node.Parent == objectTree.Nodes[1])
            {
                SelectObject(level.ties[e.Node.Index]);
            }
            if (e.Node.Parent == objectTree.Nodes[2])
            {
                SelectObject(level.shrubs[e.Node.Index]);
            }
            if (e.Node.Parent == objectTree.Nodes[3])
            {
                SelectObject(level.splines[e.Node.Index]);
            }
            if (e.Node.Parent == objectTree.Nodes[4])
            {
                SelectObject(level.gameCameras[e.Node.Index]);
            }
            if (e.Node.Parent == objectTree.Nodes[5])
            {
                SelectObject(level.spawnPoints[e.Node.Index]);
            }
            if (e.Node.Parent == objectTree.Nodes[6])
            {
                SelectObject(level.type0Cs[e.Node.Index]);
            }

            camera.MoveBehind(selectedObject);
        }

        private void cloneButton_Click(object sender, EventArgs e)
        {
            if (selectedObject as Moby == null) return;

            Moby moby = (Moby)selectedObject;
            CloneMoby(moby);
        }

        private void splineVertex_ValueChanged(object sender, EventArgs e)
        {
            //currentSplineVertex = (int)splineVertex.Value;
            UpdateEditorValues();
        }

        private void tickTimer_Tick(object sender, EventArgs e)
        {
            Tick();
        }

        private void translateToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.ToolType.Translate);
        }

        private void rotateToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.ToolType.Rotate);
        }

        private void scaleToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.ToolType.Scale);
        }

        private void splineToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.ToolType.VertexTranslator);
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if (selectedObject as Moby == null) return;

            Moby moby = (Moby)selectedObject;
            DeleteMoby(moby);
        }

        private void cloneBtn_Click(object sender, EventArgs e)
        {
            if (selectedObject as Moby == null) return;

            Moby moby = (Moby)selectedObject;
            CloneMoby(moby);
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
        #endregion
    }
}
