using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Serializers;
using RatchetEdit.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Utilities;

namespace RatchetEdit
{
    public partial class Main : Form
    {
        // Read and write acceess
        const int PROCESS_WM_READ = 0x38;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);


        Dictionary<int, string> mobNames, tieNames;

        public Level level;
        public ModelViewer modelViewer;
        public TextureViewer textureViewer;
        public SpriteViewer spriteViewer;
        public UIViewer uiViewer;
        public LanguageViewer languageViewer;
        public LightConfigViewer lightConfigViewer;
        public LevelVariableViewer levelVariableViewer;

        bool suppressTreeViewSelectEvent = false;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            mobNames = GetModelNames("/ModelLists/ModelListRC1.txt");
            tieNames = GetModelNames("/ModelLists/TieModelsRC1.txt");
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

        private Dictionary<int, string> GetModelNames(string fileName)
        {
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
                if ((GetSelectedObject() is ModelObject modelObj))
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

        public void OpenLanguageViewer()
        {
            if (languageViewer == null || languageViewer.IsDisposed)
            {
                languageViewer = new LanguageViewer(this);
                languageViewer.Show();
            }
            else
            {
                languageViewer.BringToFront();
            }
        }


        private void OpenLightConfigViewer()
        {
            if (lightConfigViewer == null || lightConfigViewer.IsDisposed)
            {
                lightConfigViewer = new LightConfigViewer(this);
                lightConfigViewer.Show();
            }
            else
            {
                lightConfigViewer.BringToFront();
            }
        }

        private void OpenLevelVariableViewer()
        {
            if (levelVariableViewer == null || levelVariableViewer.IsDisposed)
            {
                levelVariableViewer = new LevelVariableViewer(this);
                levelVariableViewer.Show();
            }
            else
            {
                levelVariableViewer.BringToFront();
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

        private void languageDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLanguageViewer();
        }


        private void exitToolBtn_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void lightConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLightConfigViewer();
        }

        private void levelVariablesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenLevelVariableViewer();
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

        // Called every frame
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            glControl.Tick();
            properties.Refresh();

            /*if (GetSelectedObject() is Moby moby)
            {
                byte[] pvars = new byte[0x100];
                int gg = 0;
                ReadProcessMemory(processHandle, moby.pVarMemoryAddress, pvars, pvars.Length, ref gg);
                for (int i = 0; i < pvars.Length / 4; i++)
                {
                    //pvarView.Items.Add(pvars[i].ToString("X2"));
                    pvarBox.Items[i] = ReadUint(pvars, i * 4).ToString("X");
                }
            } */

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
                    //objectTree.mobyNode.Nodes[level.mobs.IndexOf(moby)].Remove();
                    level.mobs.Remove(moby);
                    break;
                case Tie tie:
                    //objectTree.tieNode.Nodes[level.ties.IndexOf(tie)].Remove();
                    level.ties.Remove(tie);
                    //level.ties.RemoveRange(1, level.ties.Count - 1);
                    //level.tieModels.RemoveRange(1, level.tieModels.Count - 1);
                    //level.ties.Clear();
                    break;
                case Shrub shrub:
                    level.shrubs.Remove(shrub);
                    //level.shrubs.Clear();
                    //level.shrubModels.RemoveAt(level.shrubModels.Count -1);
                    //level.shrubModels.RemoveRange(5, level.shrubModels.Count - 5);
                    break;
                case TerrainFragment tFrag:
                    level.terrains.Remove(tFrag);
                    break;
                case Spline spline:
                    level.splines.Remove(spline);
                    break;
                case Cuboid cuboid:
                    level.cuboids.Remove(cuboid);
                    break;
                case Type0C type0C:
                    level.type0Cs.Remove(type0C);
                    break;
            }
            UpdateProperties(e.Object);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.Show();
        }

        private void collisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (collisionSaveDialog.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = File.Open(collisionSaveDialog.FileName, FileMode.Create);
                fs.Write(level.collBytes, 0, level.collBytes.Length);
                fs.Close();
            }
        }

        private void collisionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (collisionOpenDialog.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = File.Open(collisionOpenDialog.FileName, FileMode.Open);
                level.collBytes = ReadBlock(fs, 0, (int)fs.Length);
                fs.Close();
            }
            InvalidateView();
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
            }
            InvalidateView();
        }

        public Dictionary<Texture, int> GetTextureIds()
        {
            return glControl.textureIds;
        }
    }
}
