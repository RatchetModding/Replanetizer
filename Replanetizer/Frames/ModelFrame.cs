// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using LibReplanetizer;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Replanetizer.Utils;
using Replanetizer.Renderer;
using Texture = LibReplanetizer.Texture;
using SixLabors.ImageSharp;
using LibReplanetizer.LevelObjects;
using SixLabors.ImageSharp.PixelFormats;
using LibReplanetizer.Models.Animations;

namespace Replanetizer.Frames
{
    public class ModelFrame : LevelSubFrame
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected override string frameName { get; set; } = "Model Viewer";
        private static readonly Rgb24 CLEAR_COLOR = Color.FromRgb(0x9d, 0xab, 0xc7).ToPixel<Rgb24>();

        private string filter = "";
        private string filterUpper = "";
        private bool showIDInHex = false;
        private FramebufferRenderer? renderer;
        private MeshRenderer meshRenderer;
        private RendererPayload rendererPayload;
        private Camera camera;
        private Level level => levelFrame.level;
        private Model? selectedModel;
        private int selectedModelIndex;
        private List<Model>? selectedModelList;
        private List<Texture>? selectedModelTexturesSet;
        private List<List<Texture>>? selectedModelArmorTexturesSet;
        private List<Texture>? selectedTextureSet;
        private List<Texture>? modelTextureList;

        private List<Model> sortedMobyModels;
        private List<Model> sortedTieModels;
        private List<Model> sortedShrubModels;
        private List<Model> sortedGadgetModels;
        private List<List<Model>> sortedMissionModels;

        private List<ModelObject> selectedObjectInstances = new List<ModelObject>();

        private static ExporterModelSettings lastUsedExportSettings = new ExporterModelSettings();
        private ExporterModelSettings exportSettings;

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

        private ShaderTable shaderTable;

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
        private float zoomRaw = 6.0f;
        private float zoom;
        private float cameraAzimuth = MathF.PI * 0.5f;
        private float cameraAltitude = MathF.PI * 0.25f;

        // Projection matrix settings
        private const float CLIP_NEAR = 0.1f;
        private const float CLIP_FAR = 1024.0f;
        private const float FIELD_OF_VIEW = MathF.PI / 3;  // 60 degrees

        private Rectangle contentRegion;
        private Vector2 mousePos;
        private int width, height;
        private PropertyFrame propertyFrame;
        private bool firstFrame = true;
        private Vector2 startSize;

        public ModelFrame(Window wnd, LevelFrame levelFrame, ShaderTable shaderIDTable, Model? model = null) : base(wnd, levelFrame)
        {
            startSize = wnd.Size;
            modelTextureList = new List<Texture>();
            propertyFrame = new PropertyFrame(wnd, listenToCallbacks: true, hideCallbackButton: true);
            this.shaderTable = shaderIDTable;
            exportSettings = new ExporterModelSettings(lastUsedExportSettings);

            camera = new Camera();
            UpdateZoom(0.0f);
            UpdateCamera();

            meshRenderer = new MeshRenderer(shaderIDTable, levelFrame.level.textures, levelFrame.textureIds, levelFrame.level.playerAnimations);
            rendererPayload = new RendererPayload(camera);

            sortedMobyModels = new List<Model>(level.mobyModels);
            sortedTieModels = new List<Model>(level.tieModels);
            sortedShrubModels = new List<Model>(level.shrubModels);
            sortedGadgetModels = new List<Model>(level.gadgetModels);
            sortedMissionModels = new List<List<Model>>();
            for (int i = 0; i < level.missions.Count; i++)
            {
                sortedMissionModels.Add(new List<Model>(level.missions[i].models));
            }

            sortedMobyModels.Sort((x, y) => (x.id < y.id) ? -1 : 1);
            sortedTieModels.Sort((x, y) => (x.id < y.id) ? -1 : 1);
            sortedShrubModels.Sort((x, y) => (x.id < y.id) ? -1 : 1);
            sortedGadgetModels.Sort((x, y) => (x.id < y.id) ? -1 : 1);
            foreach (List<Model> list in sortedMissionModels)
            {
                list.Sort((x, y) => (x.id < y.id) ? -1 : 1);
            }

            UpdateWindowSize();
            OnResize();
            SelectModel(model);
        }

        public override void RenderAsWindow(float deltaTime)
        {
            // Standard window size
            // Is there a better way to do this in ImGui, this is ugly :(
            if (firstFrame)
            {
                System.Numerics.Vector2 startSize = new System.Numerics.Vector2(this.startSize.X, this.startSize.Y);

                startSize.X *= 0.75f;
                startSize.Y *= 0.75f;

                ImGui.SetNextWindowSize(startSize);
            }

            if (ImGui.Begin(frameName, ref isOpen, ImGuiWindowFlags.NoSavedSettings))
            {
                Render(deltaTime);
                ImGui.End();

                firstFrame = false;
            }
        }

        private void RenderModelEntry(Model mod, List<Texture> textureSet, string name)
        {
            if (filter != null && filter != "")
            {
                if (!name.ToUpper().Contains(filterUpper)) return;
            }
            if (ImGui.Selectable(name, selectedModel == mod))
            {
                SelectModel(mod, textureSet);
                PrepareForArrowInput();
            }
        }

        private string GetStringFromID(int id)
        {
            return (showIDInHex) ? $"0x{id:X3}" : $"{id}";
        }

        private string GetDisplayName(Model model)
        {
            string? modelName = ModelLists.ModelLists.GetModelName(model, level.game);
            string displayName = GetStringFromID(model.id);
            if (modelName != null)
            {
                displayName += " (" + modelName + ")";
            }

            return displayName;
        }

        private void RenderSubTree(string name, List<Model> models, List<Texture> textureSet)
        {
            if (ImGui.TreeNode(name))
            {
                for (int i = 0; i < models.Count; i++)
                {
                    Model model = models[i];
                    RenderModelEntry(model, textureSet, GetDisplayName(model));
                }
                ImGui.TreePop();
            }
        }

        private void RenderTree()
        {
            var colW = ImGui.GetColumnWidth() - 10;
            var childSize = new System.Numerics.Vector2(colW, height - 50);

            ImGui.Checkbox("Hexadecimals", ref showIDInHex);

            if (ImGui.InputText("Search", ref filter, 256))
            {
                filterUpper = filter.ToUpper();
            }

            if (ImGui.BeginChild("TreeView", childSize, false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                RenderSubTree("Moby", sortedMobyModels, level.textures);
                RenderSubTree("Tie", sortedTieModels, level.textures);
                RenderSubTree("Shrub", sortedShrubModels, level.textures);
                if (level.game == GameType.RaC1)
                {
                    RenderSubTree("Gadget", sortedGadgetModels, level.textures);
                }
                else
                {
                    if (ImGui.TreeNode("Gadget"))
                    {
                        for (int i = 0; i < sortedGadgetModels.Count; i++)
                        {
                            Model gadget = sortedGadgetModels[i];
                            RenderModelEntry(gadget, level.gadgetTextures, GetStringFromID(i));
                        }
                        ImGui.TreePop();
                    }
                }
                if (ImGui.TreeNode("Armor"))
                {
                    for (int i = 0; i < level.armorModels.Count; i++)
                    {
                        Model armor = level.armorModels[i];
                        RenderModelEntry(armor, level.armorTextures[i], GetDisplayName(armor));
                    }
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Missions"))
                {
                    for (int i = 0; i < sortedMissionModels.Count; i++)
                    {
                        var mission = level.missions[i];
                        RenderSubTree("Mission " + i, sortedMissionModels[i], mission.textures);
                    }
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Miscellaneous"))
                {
                    RenderModelEntry(level.skybox, level.textures, "Skybox");
                    ImGui.TreePop();
                }
                ImGui.EndChild();
            }
        }

        private void RenderInstanceList()
        {
            if (ImGui.CollapsingHeader("Instances"))
            {
                foreach (ModelObject obj in selectedObjectInstances)
                {
                    string objName = "Instance";
                    if (obj is Moby mob)
                    {
                        objName = $"Instance [" + GetStringFromID(mob.mobyID) + "]";
                    }

                    if (ImGui.Button(objName))
                    {
                        levelFrame.HandleSelect(obj, true, true);
                    }
                }
            }
        }

        private void UpdateWindowTitle()
        {
            string newTitle;
            if (selectedModel == null)
            {
                newTitle = "Model Viewer";
            }
            else
            {
                newTitle = "Model Viewer - " + GetDisplayName(selectedModel);
            }

            SetWindowTitle(newTitle);
        }

        public override void Render(float deltaTime)
        {
            UpdateWindowSize();
            UpdateWindowTitle();

            if (renderer == null) return;

            ImGui.Columns(3);
            ImGui.SetColumnWidth(0, 250);
            ImGui.SetColumnWidth(1, (float) width);
            ImGui.SetColumnWidth(2, 320);
            RenderTree();
            ImGui.NextColumn();

            Tick(deltaTime);

            renderer.RenderToTexture(() =>
            {
                //Setup openGL variables
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                GL.Enable(EnableCap.DepthTest);
                GL.Viewport(0, 0, width, height);

                OnPaint();
            });

            ImGui.Image((IntPtr) renderer.outputTexture, new System.Numerics.Vector2(width, height),
                System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);

            ImGui.NextColumn();
            var colW = ImGui.GetColumnWidth() - 10;
            var colSize = new System.Numerics.Vector2(colW, height);

            if (ImGui.BeginChild("TextureAndPropertyView", colSize, false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                if (selectedModel != null)
                {
                    UpdateTextures();
                    if (modelTextureList != null && modelTextureList.Count > 0)
                    {
                        TextureFrame.RenderTextureList(modelTextureList, 64, levelFrame.textureIds);
                        ImGui.Separator();
                    }
                    int fileFormat = (int) exportSettings.format;
                    if (ImGui.Combo("File Format", ref fileFormat, ExporterModelSettings.FORMAT_STRINGS, ExporterModelSettings.FORMAT_STRINGS.Length))
                    {
                        exportSettings.format = (ExporterModelSettings.Format) fileFormat;
                    }

                    // Collada specific settings
                    if (exportSettings.format == ExporterModelSettings.Format.Collada)
                    {
                        if (selectedModel is MobyModel mobyModel && mobyModel.animations.Count > 0)
                        {
                            int animationChoice = (int) exportSettings.animationChoice;
                            if (ImGui.Combo("Animations", ref animationChoice, ExporterModelSettings.ANIMATION_CHOICE_STRINGS, ExporterModelSettings.ANIMATION_CHOICE_STRINGS.Length))
                            {
                                exportSettings.animationChoice = (ExporterModelSettings.AnimationChoice) animationChoice;
                            }
                        }
                        else
                        {
                            ImGui.BeginDisabled();
                            int animationChoice = 0;
                            if (ImGui.Combo("Animations", ref animationChoice, ExporterModelSettings.ANIMATION_CHOICE_STRINGS, ExporterModelSettings.ANIMATION_CHOICE_STRINGS.Length))
                            {
                            }
                            ImGui.EndDisabled();
                        }
                    }

                    // Wavefront specific settings
                    if (exportSettings.format == ExporterModelSettings.Format.Wavefront)
                    {
                        ImGui.Checkbox("Include MTL File", ref exportSettings.exportMtlFile);
                        ImGui.Checkbox("Extended Features", ref exportSettings.extendedFeatures);
                        int orientation = (int) exportSettings.orientation;
                        if (ImGui.Combo("Orientation", ref orientation, ExporterModelSettings.ORIENTATION_STRINGS, ExporterModelSettings.ORIENTATION_STRINGS.Length))
                        {
                            exportSettings.orientation = (ExporterModelSettings.Orientation) orientation;
                        }
                    }

                    // glTF specific settings
                    if (exportSettings.format == ExporterModelSettings.Format.glTF)
                    {
                        bool embedTextures = false;
                        bool includeAnimations = false;

                        ImGui.BeginDisabled();
                        ImGui.Checkbox("Embed Textures", ref embedTextures);
                        ImGui.Checkbox("Include Animations", ref includeAnimations);
                        ImGui.EndDisabled();
                    }

                    if (ImGui.Button("Export model"))
                        ExportSelectedModel();
                    if (ImGui.Button("Export textures"))
                        ExportSelectedModelTextures();
                }

                ImGui.Separator();

                if (selectedModel is MobyModel mobModel && mobModel.animations.Count > 0)
                {
                    ImGui.Checkbox("Show Animations", ref rendererPayload.visibility.enableAnimations);

                    List<Animation> animations = (mobModel.id == 0) ? level.playerAnimations : mobModel.animations;

                    int animID = rendererPayload.forcedAnimationID;

                    if (ImGui.InputInt("Animation ID", ref animID))
                    {
                        if (animID >= animations.Count)
                        {
                            animID = 0;
                        }

                        if (animID < 0)
                        {
                            animID = animations.Count - 1;
                        }

                        rendererPayload.forcedAnimationID = animID;
                    }
                    ImGui.LabelText("Animation Framecount", (animID >= 0 && animID < animations.Count) ? animations[animID].frames.Count.ToString() : "0");
                    ImGui.Separator();
                }

                if (selectedModel != null)
                {
                    RenderInstanceList();
                    ImGui.Separator();
                }

                propertyFrame.Render(deltaTime);
            }

            ImGui.Columns(1);
        }

        private void UpdateWindowSize()
        {
            int prevWidth = width, prevHeight = height;

            System.Numerics.Vector2 vMin = ImGui.GetWindowContentRegionMin();
            System.Numerics.Vector2 vMax = ImGui.GetWindowContentRegionMax();

            vMin.X += 250;
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
                OnResize();
            }

            System.Numerics.Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowZero = new Vector2(windowPos.X + vMin.X, windowPos.Y + vMin.Y);
            mousePos = wnd.MousePosition - windowZero;
            contentRegion = new Rectangle((int) windowZero.X, (int) windowZero.Y, width, height);
        }

        private void UpdateInstanceList()
        {
            selectedObjectInstances.Clear();

            if (selectedModel == null)
            {
                return;
            }

            if (selectedModel is MobyModel)
            {
                foreach (Moby mob in level.mobs)
                {
                    if (mob.modelID == selectedModel.id)
                    {
                        selectedObjectInstances.Add(mob);
                    }
                }
            }
            else if (selectedModel is TieModel)
            {
                foreach (Tie tie in level.ties)
                {
                    if (tie.modelID == selectedModel.id)
                    {
                        selectedObjectInstances.Add(tie);
                    }
                }
            }
            else if (selectedModel is ShrubModel)
            {
                foreach (Shrub shrub in level.shrubs)
                {
                    if (shrub.modelID == selectedModel.id)
                    {
                        selectedObjectInstances.Add(shrub);
                    }
                }
            }
        }

        public void UpdateModel()
        {
            if (selectedModel == null) return;

            propertyFrame.selectedObject = selectedModel;
            UpdateTextures();

            if (selectedTextureSet != null)
            {
                meshRenderer.Include(selectedModel);
                meshRenderer.ChangeTextures(selectedTextureSet);
                rendererPayload.forcedAnimationID = 0;
            }

            UpdateInstanceList();
        }

        private void UpdateTextures()
        {
            if (selectedModel == null) return;
            if (selectedTextureSet == null) return;

            if (modelTextureList == null) modelTextureList = new List<Texture>();

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
                sortedMobyModels, sortedTieModels, sortedShrubModels,
                sortedGadgetModels, level.armorModels
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
                if (ReferenceEquals(models, sortedGadgetModels))
                    selectedModelTexturesSet = (level.game == GameType.RaC1) ? level.textures : level.gadgetTextures;
                else if (ReferenceEquals(models, level.armorModels))
                    selectedModelArmorTexturesSet = level.armorTextures;
                else
                    selectedModelTexturesSet = level.textures;

                return;
            }
        }

        private void SelectModel(Model? model)
        {
            SelectModel(model, level.textures);
        }

        private void SelectModel(Model? model, List<Texture> textures)
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

        private void OnPaint()
        {
            GL.ClearColor(CLEAR_COLOR.R / 255.0f, CLEAR_COLOR.G / 255.0f, CLEAR_COLOR.B / 255.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (selectedModel != null && selectedTextureSet != null && !(selectedModel is SkyboxModel))
            {
                if (rendererPayload.visibility.enableAnimations)
                {
                    shaderTable.animationShader.UseShader();
                    shaderTable.animationShader.SetUniform1(UniformName.useFog, 0);
                }
                meshRenderer.Render(rendererPayload);
            }
        }

        private void UpdateCamera()
        {
            Vector3 basePos = new Vector3(MathF.Sin(cameraAzimuth) * MathF.Cos(cameraAltitude), MathF.Cos(cameraAzimuth) * MathF.Cos(cameraAltitude), MathF.Sin(cameraAltitude)) * zoom * ZOOM_SCALE;
            camera.position = basePos;
            camera.rotation = new Vector3(-cameraAltitude, 0.0f, -cameraAzimuth + MathF.PI);
        }

        private void UpdateZoom(float scrollDelta)
        {
            float prevZoomRaw = zoomRaw;
            float prevZoom = zoom;
            zoomRaw -= wnd.MouseState.ScrollDelta.Y;
            zoom = MathF.Exp(ZOOM_EXP_COEFF * zoomRaw);
            if (zoom * ZOOM_SCALE is < CLIP_NEAR or > CLIP_FAR)
            {
                // Don't zoom beyond our clipping distances
                zoomRaw = prevZoomRaw;
                zoom = prevZoom;
            }
        }

        private void Tick(float deltaTime)
        {
            rendererPayload.deltaTime = deltaTime;

            // Handle scrolling with arrow keys regardless of whether the window is hovered etc
            // Our ImGui version does not seem to have anything to check for (window or child) focus
            KEY_HELD_HANDLER.Update(wnd.KeyboardState, deltaTime);

            if (KEY_HELD_HANDLER.IsKeyHeld(Keys.Down))
                CycleModels(1);
            else if (KEY_HELD_HANDLER.IsKeyHeld(Keys.Up))
                CycleModels(-1);

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

            if (wnd.MouseState.ScrollDelta.Y != 0)
            {
                UpdateZoom(wnd.MouseState.ScrollDelta.Y);
            }

            UpdateCamera();
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

            cameraAzimuth += wnd.MouseState.Delta.X * deltaTime;
            cameraAltitude += wnd.MouseState.Delta.Y * deltaTime;

            if (cameraAltitude > MathF.PI * 0.5f - 0.01f)
            {
                cameraAltitude = MathF.PI * 0.5f - 0.01f;
            }

            if (cameraAltitude < -MathF.PI * 0.5f + 0.01f)
            {
                cameraAltitude = -MathF.PI * 0.5f + 0.01f;
            }

            return true;
        }

        private void OnResize()
        {
            camera.aspect = ((float) width) / height;

            renderer?.Dispose();
            renderer = new FramebufferRenderer(width, height);
        }

        private void ExportSelectedModel()
        {
            var model = selectedModel;
            if (model == null) return;

            Exporter? exporter = Exporter.GetExporter(exportSettings);
            if (exporter == null) return;

            string filter = exporter.GetFileEnding();

            string fileName = CrossFileDialog.SaveFile("model" + filter, filter);
            if (fileName.Length == 0) return;

            // Save the settings so that modelsframes created in the future start with these settings
            lastUsedExportSettings = new ExporterModelSettings(exportSettings);

            exporter.ExportModel(fileName, level, model);
        }

        private void ExportSelectedModelTextures()
        {
            if (selectedModel == null) return;
            if (selectedTextureSet == null) return;

            List<TextureConfig>? textureConfig = selectedModel.textureConfig;
            if (textureConfig == null) return;

            String folder = CrossFileDialog.OpenFolder();
            if (folder.Length == 0) return;

            foreach (var config in selectedModel.textureConfig)
            {
                var textureId = config.id;
                if (textureId < 0 || textureId >= selectedTextureSet.Count) continue;

                var texture = selectedTextureSet[textureId];

                bool includeTransparency = (config.IgnoresTransparency()) ? false : true;

                TextureIO.ExportTexture(texture, Path.Combine(folder, $"{textureId}.png"), includeTransparency);
            }
        }
    }
}
