using LibReplanetizer;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Replanetizer.Frames;
using Replanetizer.Utils;
using Texture = LibReplanetizer.Texture;


namespace Replanetizer
{
    public class ModelFrame : LevelSubFrame
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override string frameName { get; set; } = "Models";
        
        private Level level => levelFrame.level;
        private Model selectedModel;
        private List<Texture> selectedTextureSet;

        private int shaderID, matrixID;

        private int lastMouseX;
        private float xDelta;
        private float zoom;

        private bool invalidate = true, initialized = false;

        private Matrix4 trans, scale, worldView, rot = Matrix4.Identity;

        //private TreeNode mobyNode, tieNode, shrubNode, gadgetNode, armorNode, missionsNode;

        private BufferContainer container;
        private Rectangle contentRegion;
        private Vector2 mousePos;
        private int Width, Height;
        private int targetTexture;

        public ModelFrame(Window wnd, LevelFrame levelFrame, Model model = null) : base(wnd, levelFrame)
        {
            //Setup model tree
            /*
            if (level != null)
            {
                mobyNode = GetModelNodes("Moby", level.mobyModels);
                tieNode = GetModelNodes("Tie", level.tieModels);
                shrubNode = GetModelNodes("Shrub", level.shrubModels);
                gadgetNode = GetModelNodes("Gadget", level.gadgetModels);
                armorNode = GetModelNodes("Armor", level.armorModels);
                missionsNode = GetMissionsNodes("Mission", level.missions);
                modelView.Nodes.AddRange(new TreeNode[] { mobyNode, tieNode, shrubNode, gadgetNode, armorNode, missionsNode });
            }
            */

            SelectModel(model);
        }
        
        public override void Render(float deltaTime)
        {
            if (!initialized) ModelViewer_Load();
            if (!ImGui.Begin(frameName, ref isOpen)) return;
            
            UpdateWindowSize();
            Tick(deltaTime);

            if (invalidate)
            {
                FramebufferRenderer.ToTexture(Width, Height, ref targetTexture, () => {
                    //Setup openGL variables
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                    GL.Enable(EnableCap.DepthTest);
                    GL.LineWidth(5.0f);
                    GL.Viewport(0, 0, Width, Height);
                    
                    OnPaint();
                });

                invalidate = false;
            }
            ImGui.Image((IntPtr) targetTexture, new System.Numerics.Vector2(Width, Height), 
                System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);

            ImGui.End();
        }

        private void UpdateWindowSize()
        {
            int prevWidth = Width, prevHeight = Height;

            System.Numerics.Vector2 vMin = ImGui.GetWindowContentRegionMin();
            System.Numerics.Vector2 vMax = ImGui.GetWindowContentRegionMax();

            Width = (int) (vMax.X - vMin.X);
            Height = (int) (vMax.Y - vMin.Y);

            if (Width <= 0 || Height <= 0) return; 

            if (Width != prevWidth || Height != prevHeight)
            {
                invalidate = true;
                OnResize();
            }

            System.Numerics.Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowZero = new Vector2(windowPos.X + vMin.X, windowPos.Y + vMin.Y);
            mousePos = wnd.MousePosition - windowZero;
            contentRegion = new Rectangle((int)windowZero.X, (int)windowZero.Y, Width, Height);
        }
        
        private void ModelViewer_Load()
        {
            GL.ClearColor(Color.SkyBlue);
            shaderID = levelFrame.shaderID;

            matrixID = GL.GetUniformLocation(shaderID, "MVP");

            GL.Enable(EnableCap.DepthTest);
            GL.EnableClientState(ArrayCap.VertexArray);



            worldView = CreateWorldView();
            trans = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);

            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);
        }

        /*
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

        public TreeNode GetMissionsNodes(string name, List<Mission> missions)
        {
            TreeNode newNode = new TreeNode(name);

            foreach (Mission mission in missions)
            {
                newNode.Nodes.Add(GetModelNodes("mission_" + mission.missionID, mission.models));
            }

            return newNode;
        }
        */

        public void UpdateModel()
        {
            scale = Matrix4.CreateScale(selectedModel.size);
            invalidate = true;
            //modelProperties.SelectedObject = selectedModel;
            UpdateTextures();

            container = BufferContainer.FromRenderable(selectedModel);
            container.Bind();
        }

        private void UpdateTextures()
        {
            /*
            textureList.Images.Clear();
            textureView.Items.Clear();

            for (int i = 0; i < selectedModel.textureConfig.Count; i++)
            {
                int textureId = selectedModel.textureConfig[i].ID;
                if (textureId < 0 || textureId >= selectedTextureSet.Count) continue;

                textureList.Images.Add(selectedTextureSet[textureId].getTextureImage());
                textureView.Items.Add(new ListViewItem
                {
                    ImageIndex = i,
                    Text = textureId.ToString()
                });
            }
            */
        }

        /*
        private void modelView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                switch (e.Node.Parent.Text)
                {
                    case "Moby":
                        selectedModel = level.mobyModels[modelView.SelectedNode.Index];
                        selectedTextureSet = level.textures;
                        UpdateModel();
                        break;
                    case "Tie":
                        selectedModel = level.tieModels[modelView.SelectedNode.Index];
                        selectedTextureSet = level.textures;
                        UpdateModel();
                        break;
                    case "Shrub":
                        selectedModel = level.shrubModels[modelView.SelectedNode.Index];
                        selectedTextureSet = level.textures;
                        UpdateModel();
                        break;
                    case "Gadget":
                        selectedModel = level.gadgetModels[modelView.SelectedNode.Index];
                        selectedTextureSet = (level.game.num == 1) ? level.textures : level.gadgetTextures;
                        UpdateModel();
                        break;
                    case "Armor":
                        selectedModel = level.armorModels[modelView.SelectedNode.Index];
                        selectedTextureSet = level.armorTextures[modelView.SelectedNode.Index];
                        UpdateModel();
                        break;
                }       
            } else if (e.Node.Level == 2)
            {
                int mission = int.Parse(Regex.Match(e.Node.Parent.Text,@"\d+$").Value);

                selectedModel = level.missions[mission].models[modelView.SelectedNode.Index];
                selectedTextureSet = level.missions[mission].textures;

                UpdateModel();
            }
        }
        */

        private void SelectModel(Model model)
        {
            if (model == null) return;

            selectedModel = model;
            selectedTextureSet = level.textures;

            /*
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
            */

            UpdateModel();
        }

        private Matrix4 CreateWorldView()
        {
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)Width / Height, 0.1f, 100.0f);
            Matrix4 view = Matrix4.LookAt(new Vector3(10 + zoom, 10 + zoom, 10 + zoom), Vector3.Zero, Vector3.UnitZ);
            return view * projection;
        }

        private void OnPaint()
        {
            GL.ClearColor(Color.SkyBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (selectedModel != null)
            {
                // Has to be done in this order to work correctly
                Matrix4 mvp = trans * scale * rot * worldView;

                GL.UseProgram(shaderID);
                GL.UniformMatrix4(matrixID, false, ref mvp);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                container.Bind();
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);

                //Bind textures one by one, applying it to the relevant vertices based on the index array
                foreach (TextureConfig conf in selectedModel.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.ID >= 0 && conf.ID < selectedTextureSet.Count) ? levelFrame.textureIds[selectedTextureSet[conf.ID]] : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }

                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(0);
            }

            invalidate = false;
        }

        private void glControl1_MouseWheel(float deltaTime)
        {
            zoom += (deltaTime > 0) ? -0.1f : 0.1f;
            worldView = CreateWorldView();
            invalidate = true;
        }


        private void Tick(float deltaTime)
        {
            Point absoluteMousePos = new Point((int)wnd.MousePosition.X, (int)wnd.MousePosition.Y);
            if (!ImGui.IsWindowHovered() || !contentRegion.Contains(absoluteMousePos)) return;
            
            if (wnd.IsMouseButtonDown(MouseButton.Right))
            {
                xDelta += (wnd.MousePosition.X - lastMouseX) * deltaTime;
                rot = Matrix4.CreateRotationZ(xDelta);
                lastMouseX = (int) wnd.MousePosition.X;
                invalidate = true;
            }
            if (invalidate)
            {
                invalidate = true;
            }
        }

        private void OnResize()
        {
            worldView = CreateWorldView();
            invalidate = true;
        }

        /*
         * Opens TextureViewer and allows to switch out textures
         * Does not allow to switch textures between texture sources
         */
        /*
        private void textureView_DoubleClick(object sender, EventArgs e)
        {
            using (TextureViewer textureViewer = new TextureViewer(mainForm))
            {
                if (textureViewer.ShowDialog() == DialogResult.OK)
                {
                    int val = textureViewer.returnVal;

                    if (val < selectedTextureSet.Count)
                    {
                        selectedModel.textureConfig[textureView.SelectedIndices[0]].ID = val;
                        UpdateModel();
                    }
                }
            }
        }

        private void importFromobjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedModel == null) return;

            if (objOpen.ShowDialog() == DialogResult.OK)
            {
                ModelReader.ReadObj(objOpen.FileName, selectedModel);
                UpdateModel();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedModel == null) return;

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
        */
    }
}
