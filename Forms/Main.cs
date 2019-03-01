using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using RatchetEdit.Serializers;
using RatchetEdit.LevelObjects;
using static RatchetEdit.Utilities;

namespace RatchetEdit
{
    public partial class Main : Form
    {
        Dictionary<int, string> mobNames, tieNames;

        public Level level;
        public ModelViewer modelViewer;
        public TextureViewer textureViewer;
        public SpriteViewer spriteViewer;
        public UIViewer uiViewer;

        bool suppressTreeViewSelectEvent = false;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            mobNames = GetModelNames("/ModelListRC1.txt");
            tieNames = GetModelNames("/TieModelsRC1.txt");
            objectTree.Init(mobNames, tieNames);
            glControl.SelectTool(glControl.translateTool);
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
            if (!level.valid) return;

            glControl.LoadLevel(level);

            //Enable all the buttons in the view tab
            foreach (ToolStripMenuItem menuButton in ViewToolStipItem.DropDownItems)
            {
                menuButton.Enabled = true;
            }

            objectTree.UpdateEntries(level);
            UpdateProperties(null);
        }

        private Dictionary<int, string> GetModelNames(string fileName) {
            var modelNames = new Dictionary<int, string>();

            try
            {
                using (StreamReader stream = new StreamReader(Application.StartupPath + fileName))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        string[] stringPart = line.Split('=');
                        int modelId = int.Parse(stringPart[0], NumberStyles.HexNumber);
                        modelNames.Add(modelId, stringPart[1]);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Model list file not found! No names for you!");
            }

            return modelNames;
        }

        public LevelObject GetSelectedObject()
        {
            return glControl.selectedObject;
        }

        public void UpdateProperties(LevelObject obj)
        {
            properties.SelectedObject = obj;
            properties.Refresh();
        }

        #region Open Viewers
        private void OpenModelViewer()
        {
            if ((modelViewer == null || modelViewer.IsDisposed))
            {
                if((GetSelectedObject() is ModelObject modelObj))
                {
                    modelViewer = new ModelViewer(this, modelObj.model);
                    modelViewer.Show();
                }
            }
            else
            {
                modelViewer.BringToFront();
            }
        }

        public void OpenTextureViewer()
        {
            if (textureViewer == null || textureViewer.IsDisposed)
            {
                textureViewer = new TextureViewer(this);
                textureViewer.Show();
            }
            else
            {
                textureViewer.BringToFront();
            }
        }

        public void OpenSpriteViewer()
        {
            if (spriteViewer == null || spriteViewer.IsDisposed)
            {
                spriteViewer = new SpriteViewer(this);
                spriteViewer.Show();
            }
            else
            {
                spriteViewer.BringToFront();
            }
        }

        public void OpenUISpriteViewer()
        {
            if (uiViewer == null || uiViewer.IsDisposed)
            {
                uiViewer = new UIViewer(this);
                uiViewer.Show();
            }
            else
            {
                uiViewer.BringToFront();
            }
        }
        #endregion

        #region MenuButtons
        private void UISpriteToolBtn_Click(object sender, EventArgs e)
        {
            OpenUISpriteViewer();
        }

        private void spriteViewerToolBtn_Click(object sender, EventArgs e)
        {
            OpenSpriteViewer();
        }

        private void modelViewerToolBtn_Click(object sender, EventArgs e)
        {
            OpenModelViewer();
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            OpenTextureViewer();
        }

        private void exitToolBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion MenuButtons

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.initialized) return;

            //Update ui label texts
            camXLabel.Text = String.Format("X: {0}", fRound(glControl.camera.position.X, 2).ToString());
            camYLabel.Text = String.Format("Y: {0}", fRound(glControl.camera.position.Y, 2).ToString());
            camZLabel.Text = String.Format("Z: {0}", fRound(glControl.camera.position.Z, 2).ToString());
            pitchLabel.Text = String.Format("Pitch: {0}", fRound(fToDegrees(glControl.camera.rotation.X), 2).ToString());
            yawLabel.Text = String.Format("Yaw: {0}", fRound(fToDegrees(glControl.camera.rotation.Z), 2).ToString());


        }

        //Called every frame
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            glControl.Tick();
        }

        private void cloneBtn_Click(object sender, EventArgs e)
        {
            if (!(GetSelectedObject() is Moby moby)) return;
            glControl.CloneMoby(moby);
        }

        public int GetShaderID()
        {
            return glControl.shaderID;
        }

        void InvalidateView()
        {
            glControl.invalidate = true;
        }

        private void EnableCheck(object sender, EventArgs e)
        {

        }

        private void translateToolBtn_Click(object sender, EventArgs e)
        {
            glControl.SelectTool(glControl.translateTool);
        }

        private void rotateToolBtn_Click(object sender, EventArgs e)
        {
            glControl.SelectTool(glControl.rotationTool);
        }

        private void scaleToolBtn_Click(object sender, EventArgs e)
        {
            glControl.SelectTool(glControl.scalingTool);
        }

        private void splineToolBtn_Click(object sender, EventArgs e)
        {
            glControl.SelectTool(glControl.vertexTranslator);
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            glControl.DeleteObject(GetSelectedObject());
        }


        private void objectTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (suppressTreeViewSelectEvent)
            {
                suppressTreeViewSelectEvent = false;
                return;
            }

            if (e.Node.Parent == null) return;
            

            if (e.Node.Parent == objectTree.splineNode)
            {
                glControl.SelectObject(level.splines[e.Node.Index]);
            }
            else if (e.Node.Parent == objectTree.cameraNode)
            {
                glControl.SelectObject(level.gameCameras[e.Node.Index]);
            }
            else if (e.Node.Parent == objectTree.cuboidNode)
            {
                glControl.SelectObject(level.cuboids[e.Node.Index]);
            }
            else if (e.Node.Parent == objectTree.type0CNode)
            {
                glControl.SelectObject(level.type0Cs[e.Node.Index]);
            }

            if (e.Node.Parent.Parent == null) return;

            if (e.Node.Parent.Parent == objectTree.mobyNode)
            {
                glControl.SelectObject(level.mobs[(int)e.Node.Tag]);
            }
            else if (e.Node.Parent.Parent == objectTree.tieNode)
            {
                glControl.SelectObject(level.ties[(int)e.Node.Tag]);
            }
            else if (e.Node.Parent == objectTree.shrubNode)
            {
                glControl.SelectObject(level.shrubs[e.Node.Index]);
            }

            glControl.camera.MoveBehind(GetSelectedObject());
        }


        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            InvalidateView();
        }

        private void mobyCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableMoby = mobyCheck.Checked;
            InvalidateView();
        }

        private void tieCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableTie = tieCheck.Checked;
            InvalidateView();
        }

        private void shrubCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableShrub = shrubCheck.Checked;
            InvalidateView();
        }

        private void collCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableCollision = collCheck.Checked;
            InvalidateView();
        }

        private void terrainCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableTerrain = terrainCheck.Checked;
            InvalidateView();
        }

        private void splineCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableSpline = splineCheck.Checked;
            InvalidateView();
        }

        private void skyboxCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableSkybox = skyboxCheck.Checked;
            InvalidateView();
        }

        private void cuboidCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableCuboid = cuboidCheck.Checked;
            InvalidateView();
        }

        private void type0CCheck_CheckedChanged(object sender, EventArgs e)
        {
            glControl.enableType0C = type0CCheck.Checked;
            InvalidateView();
        }

        private void glControl_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OpenModelViewer();
        }

        private void glControl_ObjectClick(object sender, RatchetEventArgs e)
        {
            /*if(e.SelectedObject is ModelObject modelObject)
            {
                modelViewer = new ModelViewer(this, modelObject.model);
                modelViewer.Show();
            }*/
            UpdateProperties(e.Object);
        }

        private void glControl_ObjectDeleted(object sender, RatchetEventArgs e)
        {
            switch (e.Object)
            {
                case Moby moby:
                    objectTree.mobyNode.Nodes[level.mobs.IndexOf(moby)].Remove();
                    level.mobs.Remove(moby);
                    break;
                case Tie tie:
                    objectTree.tieNode.Nodes[level.ties.IndexOf(tie)].Remove();
                    level.ties.Remove(tie);
                    break;
                case Shrub shrub:
                    level.shrubs.Remove(shrub);
                    break;
            }
            UpdateProperties(e.Object);
        }

        private void mapSaveAsBtn_Click(object sender, EventArgs e)
        {
            if (mapSaveDialog.ShowDialog() == DialogResult.OK)
            {
                string pathName = Path.GetDirectoryName(mapSaveDialog.FileName);

                GameplaySerializer gameplaySerializer = new GameplaySerializer();
                gameplaySerializer.Save(level, mapSaveDialog.FileName);
                EngineSerializer engineSerializer = new EngineSerializer();
                engineSerializer.Save(level, pathName);
                Console.WriteLine(pathName);
            }
            InvalidateView();
        }
    }
}
