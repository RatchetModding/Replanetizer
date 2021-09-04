using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImGuiNET;
using LibReplanetizer;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Replanetizer.Utils;
using Texture = LibReplanetizer.Texture;


namespace Replanetizer.Frames
{
    public class ModelFrame : LevelSubFrame
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override string frameName { get; set; } = "Models";

        private Level level => levelFrame.level;
        private Model selectedModel;
        private List<Texture> selectedTextureSet;
        private List<Texture> modelTextureList;

        private int shaderID, matrixID;

        private int lastMouseX;
        private float xDelta;

        // We use an exponential function to convert zoomRaw to zoom:
        //   e^(zoomExpCoeff * zoomRaw)
        // This should feel more natural compared to a linear approach where
        // it takes forever to zoom in from far away. Higher values for
        // zoomExpCoeff make it zoom in more rapidly and vice-versa.
        //
        // zoom is then multiplied by zoomScale to give us the distance from
        // the model to position the camera, so at default zoom we will be
        // viewing from 10 units away.
        private float zoomRaw;
        private float zoom = 1;
        private const float zoomScale = 10;
        private const float zoomExpCoeff = 0.4f;

        private bool invalidate = true, initialized = false;

        private Matrix4 trans, scale, worldView, rot = Matrix4.Identity;

        private BufferContainer container;
        private Rectangle contentRegion;
        private Vector2 mousePos;
        private int Width, Height;
        private int targetTexture;
        private PropertyFrame propertyFrame;

        public ModelFrame(Window wnd, LevelFrame levelFrame, Model model = null) : base(wnd, levelFrame)
        {
            modelTextureList = new List<Texture>();
            propertyFrame = new PropertyFrame(wnd, listenToCallbacks: true, hideCallbackButton: true);
            SelectModel(model);
        }

        private void RenderModelEntry(Model mod, List<Texture> textureSet, string name)
        {
            if (ImGui.Selectable(name, selectedModel == mod))
            {
                selectedModel = mod;
                selectedTextureSet = textureSet;
                UpdateModel();
            }
        }

        private void RenderSubTree(string name, List<Model> models, List<Texture> textureSet)
        {
            if (ImGui.TreeNode(name))
            {
                for (int i = 0; i < models.Count; i++)
                {
                    Model mod = models[i];
                    name = $"{mod.id:X}## {i}";
                    RenderModelEntry(mod, textureSet, name);
                }
                ImGui.TreePop();
            }
        }

        private void RenderTree()
        {
            var colW = ImGui.GetColumnWidth() - 10;
            var childSize = new System.Numerics.Vector2(colW, Height);
            if (ImGui.BeginChild("TreeView",  childSize, false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                RenderSubTree("Moby", level.mobyModels, level.textures);
                RenderSubTree("Tie", level.tieModels, level.textures);
                RenderSubTree("Shrub", level.shrubModels, level.textures);
                RenderSubTree("Gadget", level.gadgetModels, level.gadgetTextures);
                if (ImGui.TreeNode("Armor"))
                {
                    for (int i = 0; i < level.armorModels.Count; i++)
                    {
                        Model armor = level.armorModels[i];
                        RenderModelEntry(armor, level.armorTextures[i], i.ToString("X"));
                    }
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Missions"))
                {
                    for (int i = 0; i < level.missions.Count; i++)
                    {
                        var mission = level.missions[i];
                        RenderSubTree("Mission " + i, mission.models, mission.textures);
                    }
                    ImGui.TreePop();
                }
                ImGui.EndChild();
            }
        }

        public override void Render(float deltaTime)
        {
            UpdateWindowSize();
            if (!initialized) ModelViewer_Load();

            ImGui.Columns(3);
            ImGui.SetColumnWidth(0, 200);
            ImGui.SetColumnWidth(1, (float) Width);
            RenderTree();
            ImGui.NextColumn();

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

            ImGui.NextColumn();
            var colW = ImGui.GetColumnWidth() - 10;
            var colSize = new System.Numerics.Vector2(colW, Height);

            if (ImGui.BeginChild("TextureAndPropertyView", colSize, false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                if (selectedModel != null)
                {
                    if (modelTextureList.Count > 0)
                    {
                        TextureFrame.RenderTextureList(modelTextureList, 64, levelFrame.textureIds);
                        ImGui.Separator();
                    }
                    if (ImGui.Button("Export model"))
                    {
                        ExportModel(selectedModel);
                    }
                }

                ImGui.Separator();
                propertyFrame.Render(deltaTime);
            }

            ImGui.Columns(1);
        }

        private void UpdateWindowSize()
        {
            int prevWidth = Width, prevHeight = Height;

            System.Numerics.Vector2 vMin = ImGui.GetWindowContentRegionMin();
            System.Numerics.Vector2 vMax = ImGui.GetWindowContentRegionMax();

            vMin.X += 220;
            vMax.X -= 300;

            Width = (int) (vMax.X - vMin.X);
            Height = (int) (vMax.Y - vMin.Y);

            if (Width <= 0 || Height <= 0)
            {
                Width = 0;
                Height = 0;
                return;
            }

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

        public void UpdateModel()
        {
            scale = Matrix4.CreateScale(selectedModel.size);
            invalidate = true;
            propertyFrame.SelectionCallback(selectedModel);
            UpdateTextures();

            container = BufferContainer.FromRenderable(selectedModel);
            container.Bind();
        }

        private void UpdateTextures()
        {
            modelTextureList.Clear();

            for (int i = 0; i < selectedModel.textureConfig.Count; i++)
            {
                int textureId = selectedModel.textureConfig[i].ID;
                if (textureId < 0 || textureId >= selectedTextureSet.Count) continue;

                modelTextureList.Add(selectedTextureSet[textureId]);
            }
        }

        private void SelectModel(Model model)
        {
            if (model == null) return;

            selectedModel = model;
            selectedTextureSet = level.textures;

            UpdateModel();
        }

        private Matrix4 CreateWorldView()
        {
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)Width / Height, 0.1f, 100.0f);
            Matrix4 view = Matrix4.LookAt(new Vector3(zoomScale * zoom), Vector3.Zero, Vector3.UnitZ);
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

        private void Tick(float deltaTime)
        {
            Point absoluteMousePos = new Point((int)wnd.MousePosition.X, (int)wnd.MousePosition.Y);
            if (!ImGui.IsWindowHovered() || !contentRegion.Contains(absoluteMousePos)) return;

            if (wnd.MouseState.ScrollDelta.Y != 0)
            {
                zoomRaw -= wnd.MouseState.ScrollDelta.Y;
                zoom = (float)Math.Exp(zoomExpCoeff * zoomRaw);
                worldView = CreateWorldView();
                invalidate = true;
            }

            if (wnd.IsMouseButtonDown(MouseButton.Right))
            {
                xDelta += (wnd.MousePosition.X - lastMouseX) * deltaTime;
                rot = Matrix4.CreateRotationZ(xDelta);
                invalidate = true;
            }
            lastMouseX = (int) wnd.MousePosition.X;

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

        private void ExportModel(Model model)
        {
            if (selectedModel == null) return;

            var fileName = CrossFileDialog.SaveFile(filter: ".obj;.iqe");
            if (fileName.Length > 0)
            {
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
