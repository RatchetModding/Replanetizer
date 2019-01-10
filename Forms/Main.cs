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
        public enum Tool
        {
            None,
            Translate,
            Rotate,
            Scale,
            SplineEditor
        }
        Tool currentTool;

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

        List<string> modelNames;

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
            //Generate vertex array

            GetModelNames();

            SelectTool(Tool.Translate);
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



            if (skyboxCheck.Checked && skyboxCheck.Enabled)
                level.skybox.Draw(glControl1);

            if (terrainCheck.Checked && terrainCheck.Enabled)
                foreach (TerrainModel terrainModel in level.terrains)
                    terrainModel.Draw(glControl1);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (selectedObject != null) {
                if (currentTool == Tool.Translate) {
                    TranslationTool.Render(selectedObject.position, glControl1);
                }
                else if (currentTool == Tool.Rotate) {
                    RotationTool.Render(selectedObject.position, glControl1);
                }
                else if (currentTool == Tool.Scale) {
                    ScalingTool.Render(selectedObject.position, glControl1);
                }
                else if (currentTool == Tool.SplineEditor) {
                    if (selectedObject as Spline != null) {
                        Spline spline = (Spline)selectedObject;
                        TranslationTool.Render(spline.GetVertex(currentSplineVertex), glControl1);
                    }
                }
            }

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
                if (currentTool == Tool.Translate) {
                     selectedObject.Translate(direction * magnitude);
                }
                else if (currentTool == Tool.Rotate) {
                    selectedObject.Rotate(direction * magnitude);
                }
                else if (currentTool == Tool.Scale) {
                    selectedObject.Scale(direction * magnitude + Vector3.One);
                }
                else if (currentTool == Tool.SplineEditor)
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
            if (currentTool != Tool.SplineEditor) return;

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
            int mobyOffset = 0, tieOffset = 0, shrubOffset = 0, splineOffset = 0;
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
            if (splineCheck.Checked && splineCheck.Enabled)
            {
                splineOffset = offset;
                fakeDrawSplines(level.splines, splineOffset);
                offset += level.splines.Count;
            }

            GL.Clear(ClearBufferMask.DepthBufferBit); //Makes sure tool is rendered on top.

            if (selectedObject != null) {
                if (currentTool == Tool.Translate) {
                    TranslationTool.Render(selectedObject.position, glControl1);
                }
                else if (currentTool == Tool.Rotate) {
                    RotationTool.Render(selectedObject.position, glControl1);
                }
                else if (currentTool == Tool.Scale) {
                    ScalingTool.Render(selectedObject.position, glControl1);
                }
                else if (currentTool == Tool.SplineEditor)
                {
                    if (selectedObject as Spline != null)
                    {
                        Spline spline = (Spline)selectedObject;
                        TranslationTool.Render(spline.GetVertex(currentSplineVertex), glControl1);
                    }
                }
            }

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
                else if (splineCheck.Checked && id - splineOffset < level.splines.Count)
                {
                    returnObject = level.splines[id - splineOffset];
                    returnNode = objectTree.Nodes[3].Nodes[id - splineOffset];
                }
            }

            primedTreeNode = returnNode;

            hitTool = false;
            return returnObject;
        }


        public void fakeDrawSplines(List<Spline> splines, int offset)
        {
            foreach (Spline spline in splines)
            {
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
                if (currentTool == Tool.SplineEditor) SelectTool(Tool.None);
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

        void SelectTool(Tool tool) {
            translateToolBtn.Checked = (tool == Tool.Translate);
            rotateToolBtn.Checked = (tool == Tool.Rotate);
            scaleToolBtn.Checked = (tool == Tool.Scale);
            splineToolBtn.Checked = (tool == Tool.SplineEditor);
            currentTool = tool;

            if (tool == Tool.SplineEditor) currentSplineVertex = 0;

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
                SelectTool(Tool.Translate);
            }
            else if (key == (char)Keys.D2) {
                SelectTool(Tool.Rotate);
            }
            else if (key == (char)Keys.D3) {
                SelectTool(Tool.Scale);
            }
            else if (key == (char)Keys.D4) {
                SelectTool(Tool.SplineEditor);
            }
            else if (key == (char)Keys.D5) {
                SelectTool(Tool.None);
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
            SelectTool(Tool.Translate);
        }

        private void rotateToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.Rotate);
        }

        private void scaleToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.Scale);
        }

        private void splineToolBtn_Click(object sender, EventArgs e)
        {
            SelectTool(Tool.SplineEditor);
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
