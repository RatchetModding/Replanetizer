﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected override string frameName { get; set; } = "Models";

        private FramebufferRenderer renderer;
        private Level level => levelFrame.level;
        private Model selectedModel;
        private int selectedModelIndex;
        private List<Model> selectedModelList;
        private List<Texture> selectedModelTexturesSet;
        private List<List<Texture>> selectedModelArmorTexturesSet;
        private List<Texture> selectedTextureSet;
        private List<Texture> modelTextureList;

        private readonly KeyHeldHandler KEY_HELD_HANDLER = new()
        {
            watchedKeys = { Keys.Up, Keys.Down },
            holdDelay = 0.45f,
            repeatDelay = 0.06f
        };

        private readonly MouseGrabHandler MOUSE_GRAB_HANDLER = new()
        {
            mouseButton = MouseButton.Right
        };

        private ShaderIDTable shaderIDTable;

        private float xDelta;

        // We use an exponential function to convert zoomRaw to zoom:
        //   e^(ZOOM_EXP_COEFF * zoomRaw)
        // This should feel more natural compared to a linear approach where
        // it takes forever to zoom in from far away. Higher values for
        // ZOOM_EXP_COEFF make it zoom in more rapidly and vice-versa.
        //
        // zoom is then multiplied by ZOOM_SCALE to give us the distance from
        // the model to position the camera
        private const float ZOOM_SCALE = 4;
        private const float ZOOM_EXP_COEFF = 0.4f;
        private float zoomRaw;
        private float zoom = 1;

        // Projection matrix settings
        private const float CLIP_NEAR = 0.1f;
        private const float CLIP_FAR = 100f;
        private const float FIELD_OF_VIEW = MathF.PI / 3;  // 60 degrees

        private bool invalidate = true, initialized = false;

        private Matrix4 trans, scale, worldView, rot = Matrix4.Identity;

        private BufferContainer container;
        private Rectangle contentRegion;
        private Vector2 mousePos;
        private int width, height;
        private PropertyFrame propertyFrame;

        public ModelFrame(Window wnd, LevelFrame levelFrame, ShaderIDTable shaderIDTable, Model model = null) : base(wnd, levelFrame)
        {
            modelTextureList = new List<Texture>();
            propertyFrame = new PropertyFrame(wnd, listenToCallbacks: true, hideCallbackButton: true);
            this.shaderIDTable = shaderIDTable;
            UpdateWindowSize();
            OnResize();
            SelectModel(model);
        }

        private void RenderModelEntry(Model mod, List<Texture> textureSet, string name)
        {
            if (ImGui.Selectable(name, selectedModel == mod))
            {
                SelectModel(mod, textureSet);
                PrepareForArrowInput();
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
            var childSize = new System.Numerics.Vector2(colW, height);
            if (ImGui.BeginChild("TreeView", childSize, false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
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
            ImGui.SetColumnWidth(1, (float) width);
            RenderTree();
            ImGui.NextColumn();

            Tick(deltaTime);

            if (invalidate)
            {
                renderer.RenderToTexture(() =>
                {
                    //Setup openGL variables
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                    GL.Enable(EnableCap.DepthTest);
                    GL.LineWidth(5.0f);
                    GL.Viewport(0, 0, width, height);

                    OnPaint();
                });
                invalidate = false;
            }
            ImGui.Image((IntPtr) renderer.outputTexture, new System.Numerics.Vector2(width, height),
                System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);

            ImGui.NextColumn();
            var colW = ImGui.GetColumnWidth() - 10;
            var colSize = new System.Numerics.Vector2(colW, height);

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
                        ExportSelectedModel();
                    if (ImGui.Button("Export textures"))
                        ExportSelectedModelTextures();
                }

                ImGui.Separator();
                propertyFrame.Render(deltaTime);
            }

            ImGui.Columns(1);
        }

        private void UpdateWindowSize()
        {
            int prevWidth = width, prevHeight = height;

            System.Numerics.Vector2 vMin = ImGui.GetWindowContentRegionMin();
            System.Numerics.Vector2 vMax = ImGui.GetWindowContentRegionMax();

            vMin.X += 220;
            vMax.X -= 300;

            width = (int) (vMax.X - vMin.X);
            height = (int) (vMax.Y - vMin.Y);

            if (width <= 0 || height <= 0)
            {
                width = 0;
                height = 0;
                return;
            }

            if (width != prevWidth || height != prevHeight)
            {
                invalidate = true;
                OnResize();
            }

            System.Numerics.Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowZero = new Vector2(windowPos.X + vMin.X, windowPos.Y + vMin.Y);
            mousePos = wnd.MousePosition - windowZero;
            contentRegion = new Rectangle((int) windowZero.X, (int) windowZero.Y, width, height);
        }

        private void ModelViewer_Load()
        {
            GL.ClearColor(Color.SkyBlue);

            GL.Enable(EnableCap.DepthTest);
            GL.EnableClientState(ArrayCap.VertexArray);

            worldView = CreateWorldView();
            trans = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);

            GL.GenVertexArrays(1, out int vao);
            GL.BindVertexArray(vao);
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
                int textureId = selectedModel.textureConfig[i].id;
                if (textureId < 0 || textureId >= selectedTextureSet.Count) continue;

                modelTextureList.Add(selectedTextureSet[textureId]);
            }
        }

        /// <summary>
        /// Use selectedModel to prepare selectedModelIndex and
        /// selectedModelList for using arrows to navigate through models.
        /// </summary>
        private void PrepareForArrowInput()
        {
            List<Model>[] modelLists = {
                level.mobyModels, level.tieModels, level.shrubModels,
                level.gadgetModels, level.armorModels
            };
            foreach (var models in modelLists)
            {
                var idx = models.FindIndex(m => ReferenceEquals(m, selectedModel));
                if (idx == -1) continue;

                selectedModelIndex = idx;
                selectedModelList = models;

                // This is a little weird because armorTextures is a list
                // of a list of textures -- one list per armor set.
                selectedModelTexturesSet = null;
                selectedModelArmorTexturesSet = null;
                if (ReferenceEquals(models, level.gadgetModels))
                    selectedModelTexturesSet = level.gadgetTextures;
                else if (ReferenceEquals(models, level.armorModels))
                    selectedModelArmorTexturesSet = level.armorTextures;
                else
                    selectedModelTexturesSet = level.textures;

                return;
            }
        }

        private void SelectModel(Model model)
        {
            SelectModel(model, level.textures);
        }

        private void SelectModel(Model model, List<Texture> textures)
        {
            if (model == null) return;

            selectedModel = model;
            selectedTextureSet = textures;
            UpdateModel();
        }

        /// <summary>
        /// Cycle through the currently selected list of models (useful for
        /// using arrow keys to navigate)
        /// </summary>
        /// <param name="offset">offset from the current model to select</param>
        private void CycleModels(int offset)
        {
            if (selectedModelList == null) return;
            var idx = selectedModelIndex + offset;
            var count = selectedModelList.Count;
            // Wrap the new index around the count (modulus can give negatives)
            idx = (idx % count + count) % count;
            selectedModelIndex = (idx % count + count) % count;
            var model = selectedModelList[idx];

            List<Texture> textureSet;
            if (selectedModelArmorTexturesSet != null)
                // This model is armor, so get its texture set from the list
                // of armor texture sets
                textureSet = selectedModelArmorTexturesSet[idx];
            else if (selectedModelTexturesSet != null)
                textureSet = selectedModelTexturesSet;
            else
            {
                LOGGER.Warn(
                    $"Either {nameof(selectedModelTexturesSet)} or " +
                    $"{nameof(selectedModelArmorTexturesSet)} should be " +
                    "non-null. We'll default to level.textures."
                    );
                textureSet = level.textures;
            }

            SelectModel(model, textureSet);
        }

        private Matrix4 CreateWorldView()
        {
            // To scale the zoom value to make a vector of that magnitude
            // magnitude == sqrt(3*zoom^2)
            const float INV_SQRT3 = 0.57735f;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(FIELD_OF_VIEW, (float) width / height, CLIP_NEAR, CLIP_FAR);
            Matrix4 view = Matrix4.LookAt(new Vector3(INV_SQRT3 * ZOOM_SCALE * zoom), Vector3.Zero, Vector3.UnitZ);
            return view * projection;
        }

        private void OnPaint()
        {
            GL.ClearColor(Color.SkyBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (selectedModel != null)
            {
                // Has to be done in this order to work correctly
                Matrix4 mvp = trans * scale * rot;

                GL.UseProgram(shaderIDTable.shaderMain);
                GL.UniformMatrix4(shaderIDTable.uniformModelToWorldMatrix, false, ref mvp);
                GL.UniformMatrix4(shaderIDTable.uniformWorldToViewMatrix, false, ref worldView);
                GL.Uniform1(shaderIDTable.uniformLevelObjectType, (int) RenderedObjectType.Null);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                container.Bind();
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);

                //Bind textures one by one, applying it to the relevant vertices based on the index array
                foreach (TextureConfig conf in selectedModel.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.id >= 0 && conf.id < selectedTextureSet.Count) ? levelFrame.textureIds[selectedTextureSet[conf.id]] : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }

                GL.DisableVertexAttribArray(2);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(0);
            }

            invalidate = false;
        }

        private void Tick(float deltaTime)
        {
            Point absoluteMousePos = new Point((int) wnd.MousePosition.X, (int) wnd.MousePosition.Y);
            var isWindowHovered = ImGui.IsWindowHovered();
            var isMouseInContentRegion = contentRegion.Contains(absoluteMousePos);
            // Allow rotation if the cursor is directly over the level frame,
            // otherwise defer handling to any foreground frames
            var isRotating = CheckForRotationInput(deltaTime, isWindowHovered);
            // If we're holding right click and rotating, the mouse can
            // leave the bounds of the window (GLFW CursorDisabled mode
            // allows the mouse to move freely). We want to keep rendering
            // in that case
            if (!isRotating && !(isWindowHovered && isMouseInContentRegion))
                return;

            KEY_HELD_HANDLER.Update(wnd.KeyboardState, deltaTime);

            if (wnd.MouseState.ScrollDelta.Y != 0)
            {
                var prevZoomRaw = zoomRaw;
                var prevZoom = zoom;
                zoomRaw -= wnd.MouseState.ScrollDelta.Y;
                zoom = MathF.Exp(ZOOM_EXP_COEFF * zoomRaw);
                if (zoom * ZOOM_SCALE is < CLIP_NEAR or > CLIP_FAR)
                {
                    // Don't zoom beyond our clipping distances
                    zoomRaw = prevZoomRaw;
                    zoom = prevZoom;
                }
                worldView = CreateWorldView();
                invalidate = true;
            }

            if (KEY_HELD_HANDLER.IsKeyHeld(Keys.Down))
                CycleModels(1);
            else if (KEY_HELD_HANDLER.IsKeyHeld(Keys.Up))
                CycleModels(-1);
        }

        /// <param name="deltaTime">time since last tick</param>
        /// <param name="allowNewGrab">whether a new click will begin grabbing</param>
        /// <returns>whether the cursor is being grabbed</returns>
        private bool CheckForRotationInput(float deltaTime, bool allowNewGrab)
        {
            if (MOUSE_GRAB_HANDLER.TryGrabMouse(wnd, allowNewGrab))
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NoMouse;
            else
            {
                ImGui.GetIO().ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
                return false;
            }

            xDelta += wnd.MouseState.Delta.X * deltaTime;
            rot = Matrix4.CreateRotationZ(xDelta);
            invalidate = true;
            return true;
        }

        private void OnResize()
        {
            worldView = CreateWorldView();
            invalidate = true;

            renderer?.Dispose();
            renderer = new FramebufferRenderer(width, height);
        }

        private void ExportSelectedModel()
        {
            var model = selectedModel;
            if (model == null) return;

            var fileName = CrossFileDialog.SaveFile(filter: ".obj;.iqe");
            if (fileName.Length == 0) return;

            var extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".obj":
                    ModelWriter.WriteObj(fileName, model);
                    break;
                case ".iqe":
                    ModelWriter.WriteIqe(fileName, level, model);
                    break;
            }
        }

        private void ExportSelectedModelTextures()
        {
            var textureConfig = selectedModel?.textureConfig;
            if (textureConfig == null) return;

            var folder = CrossFileDialog.OpenFolder();
            if (folder.Length == 0) return;

            foreach (var config in selectedModel.textureConfig)
            {
                var textureId = config.id;
                if (textureId < 0 || textureId >= selectedTextureSet.Count) continue;

                var texture = selectedTextureSet[textureId];
                TextureIO.ExportTexture(texture, Path.Combine(folder, $"{textureId}.png"));
            }
        }
    }
}
