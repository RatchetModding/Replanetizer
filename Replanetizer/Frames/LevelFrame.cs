// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using SysVector2 = System.Numerics.Vector2;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImGuiNET;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Replanetizer.Tools;
using Replanetizer.Utils;
using static LibReplanetizer.DataFunctions;
using static LibReplanetizer.Utilities;
using Texture = LibReplanetizer.Texture;

namespace Replanetizer.Frames
{
    public class LevelFrame : Frame
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected override string frameName { get; set; } = "Level";

        private FramebufferRenderer renderer;
        public Level level { get; set; }

        private List<TerrainFragment> terrains = new List<TerrainFragment>();
        private List<Tuple<Model, int, int>> collisions = new List<Tuple<Model, int, int>>();

        private static Vector4 NORMAL_COLOR = new Vector4(1, 1, 1, 1); // White
        private static Vector4 SELECTED_COLOR = new Vector4(1, 0, 1, 1); // Purple

        public Matrix4 worldView;

        public ShaderIDTable shaderIDTable;

        private int lightsBufferObject;
        private float[][] lightsData;

        private int alignmentUbo = GL.GetInteger(GetPName.UniformBufferOffsetAlignment);

        private float movingAvgFrametime = 1.0f;

        private Matrix4 view { get; set; }

        private int currentSplineVertex;
        public readonly Selection selectedObjects;

        private int antialiasing = 1;
        private string[] antialiasingOptions = { "Off", "2x MSAA", "4x MSAA", "8x MSAA", "16x MSAA", "32x MSAA", "64x MSAA", "128x MSAA", "256x MSAA", "512x MSAA" };
        private int maxAntialiasing = 4;

        private Vector2 mousePos;
        private Vector3 prevMouseRay;
        private Rectangle contentRegion;
        private int lastMouseX, lastMouseY;
        private bool xLock = false, yLock = false, zLock = false;
        private MouseGrabHandler mouseGrabHandler = new()
        {
            mouseButton = MouseButton.Right
        };
        public readonly Keymap KEYMAP;

        public bool initialized, invalidate;
        public bool[] selectedChunks;
        public bool enableMoby = true, enableTie = true, enableShrub = true, enableSpline = false,
            enableCuboid = false, enableSpheres = false, enableCylinders = false, enableType0C = false,
            enableSkybox = true, enableTerrain = true, enableCollision = false, enableTransparency = true,
            enableDistanceCulling = true, enableFrustumCulling = true, enableFog = true, enableCameraInfo = true;

        public Camera camera;
        private Tool? currentTool;
        public Tool translateTool, rotationTool, scalingTool, vertexTranslator;

        private ConditionalWeakTable<IRenderable, BufferContainer> bufferTable;
        public Dictionary<Texture, int> textureIds;

        public List<RenderableBuffer> mobiesBuffers, tiesBuffers, shrubsBuffers, terrainBuffers;

        MemoryHook.MemoryHook hook;

        private List<int> collisionVbo = new List<int>();
        private List<int> collisionIbo = new List<int>();

        private int width, height;

        private List<Frame> subFrames;

        public LevelFrame(Window wnd) : base(wnd)
        {
            subFrames = new List<Frame>();
            bufferTable = new ConditionalWeakTable<IRenderable, BufferContainer>();
            camera = new Camera();

            maxAntialiasing = (int) Math.Log2((double) GL.GetInteger(GetPName.MaxSamples));

            KEYMAP = new Keymap(wnd, Keymap.DEFAULT_KEYMAP);

            selectedObjects = new Selection();
            selectedObjects.CollectionChanged += SelectedObjectsOnCollectionChanged;

            UpdateWindowSize();
            OnResize();
        }

        public static bool FrameMustClose(Frame frame)
        {
            return !frame.isOpen;
        }

        private void RenderMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Level"))
                {
                    if (ImGui.MenuItem("Save as"))
                    {
                        var res = CrossFileDialog.SaveFile();
                        if (res.Length > 0)
                        {
                            level.Save(res);
                        }
                    }

                    if (ImGui.BeginMenu("Export"))
                    {
                        if (ImGui.MenuItem("Collision"))
                        {
                            var res = CrossFileDialog.SaveFile();
                            if (res.Length > 0)
                            {
                                FileStream fs = File.Open(res, FileMode.Create);
                                fs.Write(level.collBytesEngine, 0, level.collBytesEngine.Length);
                                fs.Close();
                            }
                        }
                        if (ImGui.MenuItem("Level as Model"))
                        {
                            subFrames.Add(new LevelExportFrame(this.wnd, this));
                        }
                        if (ImGui.MenuItem("All textures"))
                        {
                            var res = CrossFileDialog.OpenFolder();
                            if (res.Length > 0)
                            {
                                TextureIO.ExportAllTextures(level, res);
                            }
                        }
                        if (ImGui.BeginMenu("Class IDs lists"))
                        {
                            if (ImGui.MenuItem("Mobies"))
                            {
                                var path = CrossFileDialog.SaveFile("mobyIDs.txt", ".txt");
                                ExportListOfProperties<Moby>(path, level.mobs, (m) => { return m.modelID.ToString(); });
                            }
                            if (ImGui.MenuItem("Ties"))
                            {
                                var path = CrossFileDialog.SaveFile("tieIDs.txt", ".txt");
                                ExportListOfProperties<Tie>(path, level.ties, (t) => { return t.modelID.ToString(); });
                            }
                            if (ImGui.MenuItem("Shrubs"))
                            {
                                var path = CrossFileDialog.SaveFile("shrubIDs.txt", ".txt");
                                ExportListOfProperties<Shrub>(path, level.shrubs, (s) => { return s.modelID.ToString(); });
                            }
                            ImGui.EndMenu();
                        }

                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Import"))
                    {
                        if (ImGui.MenuItem("Collision"))
                        {
                            var res = CrossFileDialog.OpenFile(filter: ".rcc");
                            if (res.Length > 0)
                            {
                                FileStream fs = File.Open(res, FileMode.Open);
                                level.collBytesEngine = ReadBlock(fs, 0, (int) fs.Length);
                                fs.Close();
                            }
                            InvalidateView();
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Windows"))
                {
                    if (ImGui.MenuItem("Object properties"))
                    {
                        var newFrame = new PropertyFrame(this.wnd, this, selectedObjects, listenToCallbacks: true);
                        // TODO callbacks
                        // RegisterCallback(newFrame.SelectionCallback);
                        subFrames.Add(newFrame);
                    }
                    if (ImGui.MenuItem("Model viewer"))
                    {
                        subFrames.Add(new ModelFrame(this.wnd, this, this.shaderIDTable));
                    }
                    if (ImGui.MenuItem("Texture viewer"))
                    {
                        subFrames.Add(new TextureFrame(this.wnd, this));
                    }
                    if (ImGui.MenuItem("Lights"))
                    {
                        subFrames.Add(new LightsFrame(this.wnd, this, level.lights, level.lightConfig));
                    }
                    if (ImGui.MenuItem("Level variables"))
                    {
                        subFrames.Add(new PropertyFrame(this.wnd, this, level.levelVariables, "Level variables"));
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Render"))
                {
                    if (ImGui.Checkbox("Moby", ref enableMoby)) InvalidateView();
                    if (ImGui.Checkbox("Tie", ref enableTie)) InvalidateView();
                    if (ImGui.Checkbox("Shrub", ref enableShrub)) InvalidateView();
                    if (ImGui.Checkbox("Spline", ref enableSpline)) InvalidateView();
                    if (ImGui.Checkbox("Cuboid", ref enableCuboid)) InvalidateView();
                    if (ImGui.Checkbox("Spheres", ref enableSpheres)) InvalidateView();
                    if (ImGui.Checkbox("Cylinders", ref enableCylinders)) InvalidateView();
                    if (ImGui.Checkbox("Type0C", ref enableType0C)) InvalidateView();
                    if (ImGui.Checkbox("Skybox", ref enableSkybox)) InvalidateView();
                    if (ImGui.Checkbox("Terrain", ref enableTerrain)) InvalidateView();
                    if (ImGui.Checkbox("Collision", ref enableCollision)) InvalidateView();
                    ImGui.Separator();
                    if (ImGui.Checkbox("Transparency", ref enableTransparency)) InvalidateView();
                    if (ImGui.Checkbox("Distance Culling", ref enableDistanceCulling)) InvalidateView();
                    if (ImGui.Checkbox("Frustum Culling", ref enableFrustumCulling)) InvalidateView();
                    if (ImGui.Checkbox("Fog", ref enableFog)) InvalidateView();
                    ImGui.PushItemWidth(90.0f);
                    if (ImGui.Combo("Antialiasing", ref antialiasing, antialiasingOptions, 1 + maxAntialiasing))
                    {
                        UpdateAaLevel();
                        InvalidateView();
                    }
                    ImGui.PopItemWidth();
                    ImGui.Separator();
                    ImGui.Checkbox("Camera Info", ref enableCameraInfo);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Tools"))
                {
                    if (ImGui.MenuItem("Translate        [1]")) SelectTool(translateTool);
                    if (ImGui.MenuItem("Rotate           [2]")) SelectTool(rotationTool);
                    if (ImGui.MenuItem("Scale            [3]")) SelectTool(scalingTool);
                    if (ImGui.MenuItem("Vertex translate [4]")) SelectTool(vertexTranslator);
                    if (ImGui.MenuItem("No tool          [5]")) SelectTool(null);
                    if (ImGui.MenuItem("Delete selected  [Del]")) DeleteObject(selectedObjects);
                    if (ImGui.MenuItem("Deselect all")) selectedObjects.Clear();

                    ImGui.EndMenu();
                }

                if (selectedChunks.Length > 0)
                {
                    if (ImGui.BeginMenu("Chunks"))
                    {
                        for (int i = 0; i < selectedChunks.Length; i++)
                        {
                            if (ImGui.Checkbox("Chunk " + i, ref selectedChunks[i])) SetSelectedChunks();
                        }

                        ImGui.EndMenu();
                    }
                }

                ImGui.EndMenuBar();
            }
        }

        private void RenderTextOverlay(float deltaTime)
        {
            if (!enableCameraInfo) return;

            const float PAD = 10f;
            const ImGuiWindowFlags WINDOW_FLAGS =
                ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.AlwaysAutoResize
                | ImGuiWindowFlags.NoSavedSettings
                | ImGuiWindowFlags.NoFocusOnAppearing
                | ImGuiWindowFlags.NoNav
                | ImGuiWindowFlags.NoMove;

            var viewport = ImGui.GetMainViewport();
            var workPos = viewport.GetWorkPos();
            var workSize = viewport.GetWorkSize();
            SysVector2 windowPos = new(
                workPos.X + PAD,
                workPos.Y + workSize.Y - PAD
            );
            SysVector2 windowPosPivot = new(0f, 1f);
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);

            ImGui.SetNextWindowBgAlpha(0.35f);

            if (ImGui.Begin("Info overlay", WINDOW_FLAGS))
            {
                ImGui.Text("Camera info");
                ImGui.Separator();
                ImGui.Text(
                    "Position: (" +
                    $"x: {camera.position.X:F4}, " +
                    $"y: {camera.position.Y:F4}, " +
                    $"z: {camera.position.Z:F4}" +
                    ")"
                );
                var camRotX = ToDegreesF(camera.rotation.X);
                var camRotZ = ToDegreesF(camera.rotation.Z);
                // Wrap around [0, 360)
                camRotZ = (camRotZ % 360f + 360f) % 360f;
                ImGui.Text(
                    $"Rotation: (yaw: {camRotZ:F4}, pitch: {camRotX:F4})"
                );

                movingAvgFrametime = movingAvgFrametime * 0.95f + deltaTime * 0.05f;
                float fps = MathF.Round(1.0f / movingAvgFrametime);
                float frametime = ((float) (MathF.Round(10000.0f * movingAvgFrametime))) / 10.0f;
                switch (fps)
                {
                    case < 30:
                        ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000ff);
                        break;
                    case < 50:
                        ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ffff);
                        break;
                    default:
                        ImGui.PushStyleColor(ImGuiCol.Text, 0xffffffff);
                        break;
                }
                ImGui.Text("FPS: " + fps + " (" + frametime + " ms)");
                ImGui.PopStyleColor();
            }
            ImGui.End();
        }

        private void UpdateWindowSize()
        {
            int prevWidth = width, prevHeight = height;

            System.Numerics.Vector2 vMin = ImGui.GetWindowContentRegionMin();
            System.Numerics.Vector2 vMax = ImGui.GetWindowContentRegionMax();

            width = (int) (vMax.X - vMin.X);
            height = (int) (vMax.Y - vMin.Y);

            camera.aspect = (float) width / height;

            if (width <= 0 || height <= 0) return;

            if (width != prevWidth || height != prevHeight)
            {
                InvalidateView();
                OnResize();
            }

            System.Numerics.Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowZero = new Vector2(windowPos.X + vMin.X, windowPos.Y + vMin.Y);
            mousePos = wnd.MousePosition - windowZero;
            contentRegion = new Rectangle((int) windowZero.X, (int) windowZero.Y, width, height);
        }

        public override void RenderAsWindow(float deltaTime)
        {
            if (!initialized) CustomGLControl_Load();

            var viewport = ImGui.GetMainViewport();
            var pos = viewport.Pos;
            var size = viewport.Size;

            ImGui.SetNextWindowPos(pos);
            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 0);

            ImGui.Begin(frameName,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus |
                ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking);

            ImGui.PopStyleVar(2);

            Render(deltaTime);
            ImGui.End();

            RenderSubFrames(deltaTime);
        }

        public override void Render(float deltaTime)
        {
            if (level == null) return;

            RenderMenuBar();
            RenderTextOverlay(deltaTime);
            UpdateWindowSize();
            Tick(deltaTime);

            if (invalidate)
            {
                renderer.RenderToTexture(() =>
                {
                    //Setup openGL variables
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                    GL.Enable(EnableCap.DepthTest);
                    GL.LineWidth(1.0f);
                    GL.Viewport(0, 0, width, height);

                    OnPaint();
                });
                invalidate = false;
            }
            ImGui.Image((IntPtr) renderer.outputTexture, new System.Numerics.Vector2(width, height),
                    System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);
        }

        private void RenderSubFrames(float deltaTime)
        {
            subFrames.RemoveAll(FrameMustClose);
            foreach (Frame levelSubFrame in subFrames)
            {
                levelSubFrame.RenderAsWindow(deltaTime);
            }
        }

        private void CustomGLControl_Load()
        {
            GL.GenVertexArrays(1, out int vao);
            GL.BindVertexArray(vao);

            //Setup openGL variables
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.LineWidth(5.0f);

            string applicationFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string shaderFolder = Path.Join(applicationFolder, "Shaders");

            shaderIDTable = new ShaderIDTable();

            shaderIDTable.shaderMain = LinkShader(shaderFolder, "vs.glsl", "fs.glsl");
            shaderIDTable.shaderColor = LinkShader(shaderFolder, "colorshadervs.glsl", "colorshaderfs.glsl");
            shaderIDTable.shaderCollision = LinkShader(shaderFolder, "collisionshadervs.glsl", "collisionshaderfs.glsl");

            shaderIDTable.uniformWorldToViewMatrix = GL.GetUniformLocation(shaderIDTable.shaderMain, "WorldToView");
            shaderIDTable.uniformModelToWorldMatrix = GL.GetUniformLocation(shaderIDTable.shaderMain, "ModelToWorld");
            shaderIDTable.uniformColorWorldToViewMatrix = GL.GetUniformLocation(shaderIDTable.shaderColor, "WorldToView");
            shaderIDTable.uniformColorModelToWorldMatrix = GL.GetUniformLocation(shaderIDTable.shaderColor, "ModelToWorld");
            shaderIDTable.uniformCollisionWorldToViewMatrix = GL.GetUniformLocation(shaderIDTable.shaderCollision, "WorldToView");

            shaderIDTable.uniformColor = GL.GetUniformLocation(shaderIDTable.shaderColor, "incolor");

            shaderIDTable.uniformFogColor = GL.GetUniformLocation(shaderIDTable.shaderMain, "fogColor");
            shaderIDTable.uniformFogNearDist = GL.GetUniformLocation(shaderIDTable.shaderMain, "fogNearDistance");
            shaderIDTable.uniformFogFarDist = GL.GetUniformLocation(shaderIDTable.shaderMain, "fogFarDistance");
            shaderIDTable.uniformFogNearIntensity = GL.GetUniformLocation(shaderIDTable.shaderMain, "fogNearIntensity");
            shaderIDTable.uniformFogFarIntensity = GL.GetUniformLocation(shaderIDTable.shaderMain, "fogFarIntensity");
            shaderIDTable.uniformUseFog = GL.GetUniformLocation(shaderIDTable.shaderMain, "useFog");

            shaderIDTable.uniformLevelObjectType = GL.GetUniformLocation(shaderIDTable.shaderMain, "levelObjectType");
            shaderIDTable.uniformLevelObjectNumber = GL.GetUniformLocation(shaderIDTable.shaderMain, "levelObjectNumber");
            shaderIDTable.uniformColorLevelObjectType = GL.GetUniformLocation(shaderIDTable.shaderColor, "levelObjectType");
            shaderIDTable.uniformColorLevelObjectNumber = GL.GetUniformLocation(shaderIDTable.shaderColor, "levelObjectNumber");

            shaderIDTable.uniformAmbientColor = GL.GetUniformLocation(shaderIDTable.shaderMain, "staticColor");
            shaderIDTable.uniformLightIndex = GL.GetUniformLocation(shaderIDTable.shaderMain, "lightIndex");

            RenderableBuffer.SHADER_ID_TABLE = shaderIDTable;

            LoadDirectionalLights(level.lights);

            camera.ComputeProjectionMatrix();
            view = camera.GetViewMatrix();

            translateTool = new TranslationTool();
            rotationTool = new RotationTool();
            scalingTool = new ScalingTool();
            vertexTranslator = new VertexTranslationTool();

            initialized = true;
        }

        private void LoadTexture(Texture t)
        {
            int texId;
            GL.GenTextures(1, out texId);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            int offset = 0;

            if (t.mipMapCount > 1)
            {
                int mipWidth = t.width;
                int mipHeight = t.height;

                for (int mipLevel = 0; mipLevel < t.mipMapCount; mipLevel++)
                {
                    if (mipWidth > 0 && mipHeight > 0)
                    {
                        int size = ((mipWidth + 3) / 4) * ((mipHeight + 3) / 4) * 16;
                        byte[] texPart = new byte[size];
                        Array.Copy(t.data, offset, texPart, 0, size);
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, mipLevel, InternalFormat.CompressedRgbaS3tcDxt5Ext, mipWidth, mipHeight, 0, size, texPart);
                        offset += size;
                        mipWidth /= 2;
                        mipHeight /= 2;
                    }
                }
            }
            else
            {
                int size = ((t.width + 3) / 4) * ((t.height + 3) / 4) * 16;
                GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedRgbaS3tcDxt5Ext, t.width, t.height, 0, size, t.data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            textureIds.Add(t, texId);
        }

        void LoadLevelTextures()
        {
            textureIds = new Dictionary<Texture, int>();
            foreach (Texture t in level.textures)
            {
                LoadTexture(t);
            }

            foreach (List<Texture> list in level.armorTextures)
            {
                foreach (Texture t in list)
                {
                    LoadTexture(t);
                }
            }

            foreach (Texture t in level.gadgetTextures)
            {
                LoadTexture(t);
            }

            foreach (Mission mission in level.missions)
            {
                foreach (Texture t in mission.textures)
                {
                    LoadTexture(t);
                }
            }
        }

        private void LoadSingleCollisionBo(Collision col)
        {
            int id;
            GL.GenBuffers(1, out id);
            GL.BindBuffer(BufferTarget.ArrayBuffer, id);
            GL.BufferData(BufferTarget.ArrayBuffer, col.vertexBuffer.Length * sizeof(float), col.vertexBuffer, BufferUsageHint.StaticDraw);

            collisionVbo.Add(id);

            GL.GenBuffers(1, out id);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, id);
            GL.BufferData(BufferTarget.ElementArrayBuffer, col.indBuff.Length * sizeof(int), col.indBuff, BufferUsageHint.StaticDraw);

            collisionIbo.Add(id);
        }

        void LoadCollisionBOs()
        {
            foreach (int id in collisionIbo)
            {
                GL.DeleteBuffer(id);
            }

            foreach (int id in collisionVbo)
            {
                GL.DeleteBuffer(id);
            }

            collisionVbo.Clear();
            collisionIbo.Clear();

            if (level.collisionChunks.Count == 0)
            {
                LoadSingleCollisionBo((Collision) level.collisionEngine);
            }
            else
            {
                foreach (Model collisionModel in level.collisionChunks)
                {
                    LoadSingleCollisionBo((Collision) collisionModel);
                }
            }
        }

        /// <summary>
        /// Updates the buffers of the lights.
        /// </summary>
        private void UpdateDirectionalLights(List<Light> lights)
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, lightsBufferObject);
            int lightSize = sizeof(float) * 16;

            for (int i = 0; i < lights.Count; i++)
            {
                lightsData[i][0] = lights[i].color1.X;
                lightsData[i][1] = lights[i].color1.Y;
                lightsData[i][2] = lights[i].color1.Z;
                lightsData[i][3] = lights[i].color1.W;
                lightsData[i][4] = lights[i].direction1.X;
                lightsData[i][5] = lights[i].direction1.Y;
                lightsData[i][6] = lights[i].direction1.Z;
                lightsData[i][7] = lights[i].direction1.W;
                lightsData[i][8] = lights[i].color2.X;
                lightsData[i][9] = lights[i].color2.Y;
                lightsData[i][10] = lights[i].color2.Z;
                lightsData[i][11] = lights[i].color2.W;
                lightsData[i][12] = lights[i].direction2.X;
                lightsData[i][13] = lights[i].direction2.Y;
                lightsData[i][14] = lights[i].direction2.Z;
                lightsData[i][15] = lights[i].direction2.W;
                GL.BufferSubData(BufferTarget.UniformBuffer, new IntPtr(sizeof(float) * 16 * i), lightSize, lightsData[i]);
            }

            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        /// <summary>
        /// Initializes buffers for the directional lights to be used in the shaders.
        /// An all black light is added additionally for all objects with out of bounds light indices.
        /// </summary>
        private void LoadDirectionalLights(List<Light> lights)
        {
            int loc = GL.GetUniformBlockIndex(shaderIDTable.shaderMain, "lights");
            GL.UniformBlockBinding(shaderIDTable.shaderMain, loc, 0);

            lightsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, lightsBufferObject);
            GL.BufferData(BufferTarget.UniformBuffer, sizeof(float) * 16 * ShaderIDTable.ALLOCATED_LIGHTS, IntPtr.Zero, BufferUsageHint.StaticRead);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            lightsData = new float[ShaderIDTable.ALLOCATED_LIGHTS][];

            for (int i = 0; i < ShaderIDTable.ALLOCATED_LIGHTS; i++)
            {
                lightsData[i] = new float[16];

                // Upload the all black light, all unused ones will remain black.
                GL.BufferSubData(BufferTarget.UniformBuffer, new IntPtr(sizeof(float) * 16 * i), sizeof(float) * 16, lightsData[i]);
            }

            UpdateDirectionalLights(lights);
        }

        /// <summary>
        /// Returns a list of RenderableBuffers for a collection objects with each object of the same type.
        /// </summary>
        private List<RenderableBuffer> GetRenderableBuffer(IEnumerable<ModelObject> objects, RenderedObjectType type)
        {
            List<RenderableBuffer> buffers = new List<RenderableBuffer>();
            for (int i = 0; i < objects.Count(); i++)
            {
                buffers.Add(new RenderableBuffer(objects.ElementAt(i), type, i, level, textureIds));
            }
            return buffers;
        }

        public void LoadLevel(Level level)
        {
            this.level = level;

            GL.ClearColor(level.levelVariables.fogColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            level.skybox.textureConfig.Sort((emp1, emp2) => emp1.start.CompareTo(emp2.start));

            LoadLevelTextures();
            LoadCollisionBOs();

            Array.Resize(ref selectedChunks, level.collisionChunks.Count);
            for (int i = 0; i < level.collisionChunks.Count; i++)
            {
                selectedChunks[i] = true;
            }


            if (level.mobs.Count > 0)
            {
                Moby ratchet = level.mobs[0];
                camera.MoveBehind(ratchet);
            }
            else
            {
                camera.SetPosition(0, 0, 0);
                camera.SetRotation(0, 0);
            }
            selectedObjects.Clear();
            SetSelectedChunks();
            shrubsBuffers = GetRenderableBuffer(level.shrubs, RenderedObjectType.Shrub);
            tiesBuffers = GetRenderableBuffer(level.ties, RenderedObjectType.Tie);
            mobiesBuffers = GetRenderableBuffer(level.mobs, RenderedObjectType.Moby);

            InvalidateView();
        }

        public void SetSelectedChunks()
        {
            if (level.terrainChunks.Count == 0)
            {
                terrains.Clear();
                terrains.AddRange(level.terrainEngine.fragments);
                collisions.Clear();
                collisions.Add(new Tuple<Model, int, int>(level.collisionEngine, collisionVbo[0], collisionIbo[0]));
            }
            else
            {
                terrains.Clear();
                collisions.Clear();

                for (int i = 0; i < level.terrainChunks.Count; i++)
                {
                    if (selectedChunks[i])
                        terrains.AddRange(level.terrainChunks[i].fragments);
                }

                for (int i = 0; i < level.collisionChunks.Count; i++)
                {
                    if (selectedChunks[i])
                        collisions.Add(new Tuple<Model, int, int>(level.collisionChunks[i], collisionVbo[i], collisionIbo[i]));
                }
            }

            if (terrainBuffers != null) foreach (RenderableBuffer buffer in terrainBuffers) buffer.Dispose();

            terrainBuffers = GetRenderableBuffer(terrains, RenderedObjectType.Terrain);
            InvalidateView();
        }

        public void SelectedObjectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!selectedObjects.isOnlySplines && currentTool is VertexTranslationTool)
                SelectTool();
            InvalidateView();
        }

        public void DeleteObject(IEnumerable<LevelObject> levelObjects)
        {
            foreach (var obj in levelObjects)
                DeleteObject(obj);
        }

        public void DeleteObject(LevelObject levelObject)
        {
            selectedObjects.Remove(levelObject);
            switch (levelObject)
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
                    foreach (Terrain terrain in level.terrainChunks)
                    {
                        terrain.fragments.Remove(tFrag);
                    }
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
        }

        private void HandleMouseWheelChanges()
        {
            if (currentTool is not VertexTranslationTool) return;
            if (!selectedObjects.TryGetOne(out var obj) || obj is not Spline spline)
                return;

            int delta = (int) wnd.MouseState.ScrollDelta.Length / 120;
            if (delta > 0)
            {
                if (currentSplineVertex < spline.GetVertexCount() - 1)
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

        public void CloneMoby(Moby moby)
        {
            if (!(moby.Clone() is Moby newMoby)) return;

            level.mobs.Add(newMoby);
            selectedObjects.Set(newMoby);
            InvalidateView();
        }

        private void HandleKeyboardShortcuts()
        {
            if (KEYMAP.IsPressed(Keybinds.ToolNone)) SelectTool();
            if (KEYMAP.IsPressed(Keybinds.ToolTranslate)) SelectTool(translateTool);
            if (KEYMAP.IsPressed(Keybinds.ToolRotation)) SelectTool(rotationTool);
            if (KEYMAP.IsPressed(Keybinds.ToolScaling)) SelectTool(scalingTool);
            if (KEYMAP.IsPressed(Keybinds.ToolVertexTranslator)) SelectTool(vertexTranslator);
            if (KEYMAP.IsPressed(Keybinds.DeleteObject)) DeleteObject(selectedObjects);
        }

        public void SelectTool(Tool tool = null)
        {
            currentTool = tool;
            currentSplineVertex = 0;
            InvalidateView();
        }

        /// <param name="allowNewGrab">whether a new click will begin grabbing</param>
        /// <returns>whether the cursor is being grabbed</returns>
        private bool CheckForRotationInput(float deltaTime, bool allowNewGrab)
        {
            if (mouseGrabHandler.TryGrabMouse(wnd, allowNewGrab))
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NoMouse;
            else
            {
                ImGui.GetIO().ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
                return false;
            }

            camera.rotation.Z -= (wnd.MouseState.Delta.X) * camera.speed * deltaTime;
            camera.rotation.X -= (wnd.MouseState.Delta.Y) * camera.speed * deltaTime;
            camera.rotation.X = MathHelper.Clamp(camera.rotation.X,
                MathHelper.DegreesToRadians(-89.9f), MathHelper.DegreesToRadians(89.9f));

            InvalidateView();
            return true;
        }

        private void CheckForMovementInput(float deltaTime)
        {
            float moveSpeed = KEYMAP.IsDown(Keybinds.MoveFastModifier) ? 40 : 10;
            Vector3 moveDir = GetInputAxes();
            if (moveDir.Length > 0)
            {
                moveDir *= moveSpeed * deltaTime;
                camera.TransformedTranslate(moveDir);

                InvalidateView();
            }
        }

        private void HandleToolUpdates(Vector3 mouseRay, Vector3 direction)
        {
            float magnitudeMultiplier = 50;
            Vector3 magnitude = (mouseRay - prevMouseRay) * magnitudeMultiplier;

            foreach (var obj in selectedObjects)
            {
                if (currentTool is TranslationTool)
                    obj.Translate(direction * magnitude);
                else if (currentTool is RotationTool)
                    obj.Rotate(direction * magnitude);
                else if (currentTool is ScalingTool)
                    obj.Scale(direction * magnitude + Vector3.One);
                else if (currentTool is VertexTranslationTool && obj is Spline spline)
                {
                    spline.TranslateVertex(currentSplineVertex, direction * magnitude);
                    if (hook is { hookWorking: true })
                    {
                        hook.HandleSplineTranslation(level, spline, currentSplineVertex);
                    }
                }
            }

            selectedObjects.Update();
            InvalidateView();
        }

        public void Tick(float deltaTime)
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

            HandleMouseWheelChanges();
            HandleKeyboardShortcuts();
            CheckForMovementInput(deltaTime);

            view = camera.GetViewMatrix();

            Vector3 mouseRay = MouseToWorldRay(camera.GetProjectionMatrix(), view, new Size(width, height), mousePos);

            if (!HandleLeftMouseDown(mouseRay))
            {
                xLock = false;
                yLock = false;
                zLock = false;
            }

            lastMouseX = (int) wnd.MousePosition.X;
            lastMouseY = (int) wnd.MousePosition.Y;
            prevMouseRay = mouseRay;
        }

        private bool HandleLeftMouseDown(Vector3 mouseRay)
        {
            if (!wnd.IsMouseButtonDown(MouseButton.Left))
                return false;

            LevelObject? obj = null;
            Vector3 direction = Vector3.Zero;

            renderer.ExposeFramebuffer(() => { obj = GetObjectAtScreenPosition(mousePos); });

            if (xLock)
                direction = Vector3.UnitX;
            else if (yLock)
                direction = Vector3.UnitY;
            else if (zLock)
                direction = Vector3.UnitZ;

            if (xLock || yLock || zLock)
                HandleToolUpdates(mouseRay, direction);
            else
                HandleSelect(obj);

            return true;
        }

        private bool HandleSelect(LevelObject? obj)
        {
            if (obj == null)
                return false;
            if (wnd.MouseState.WasButtonDown(MouseButton.Left))
                return false;

            bool isMultiSelect = KEYMAP.IsDown(Keybinds.MultiSelectModifier);

            if (isMultiSelect)
                selectedObjects.Toggle(obj);
            else
                selectedObjects.ToggleOne(obj);

            return true;
        }

        private Vector3 GetInputAxes()
        {
            float xAxis = 0, yAxis = 0, zAxis = 0;

            if (KEYMAP.IsDown(Keybinds.MoveRight)) xAxis++;
            if (KEYMAP.IsDown(Keybinds.MoveLeft)) xAxis--;
            if (KEYMAP.IsDown(Keybinds.MoveForward)) yAxis++;
            if (KEYMAP.IsDown(Keybinds.MoveBackward)) yAxis--;
            if (KEYMAP.IsDown(Keybinds.MoveUp)) zAxis++;
            if (KEYMAP.IsDown(Keybinds.MoveDown)) zAxis--;

            var inputAxes = new Vector3(xAxis, yAxis, zAxis);
            inputAxes.NormalizeFast();
            return inputAxes;
        }

        public void ActivateBuffersForModel(IRenderable renderable)
        {
            BufferContainer container = bufferTable.GetValue(renderable, BufferContainer.FromRenderable);
            container.Bind();
        }

        public void RenderTool()
        {
            if (currentTool == null || selectedObjects.Count == 0)
                return;

            // Render tool on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Tool);
            GL.LineWidth(5.0f);

            if (selectedObjects.TryGetOne(out var obj) && obj is Spline spline &&
                currentTool is VertexTranslationTool)
            {
                currentTool.Render(spline.GetVertex(currentSplineVertex), this);
            }
            else
                currentTool.Render(selectedObjects.pivot, this);

            GL.LineWidth(1.0f);
        }

        private int LinkShader(string shaderFolder, string vsname, string fsname)
        {
            int shaderID = GL.CreateProgram();
            LoadShader(Path.Join(shaderFolder, vsname), ShaderType.VertexShader, shaderID);
            LoadShader(Path.Join(shaderFolder, fsname), ShaderType.FragmentShader, shaderID);
            GL.LinkProgram(shaderID);

            return shaderID;
        }

        private void LoadShader(string filename, ShaderType type, int program)
        {
            int address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            LOGGER.Debug("Compiled shader from {0}, log: {1}", filename, GL.GetShaderInfoLog(address));
        }

        private void UpdateAaLevel()
        {
            if (antialiasing == 0)
                GL.Disable(EnableCap.Multisample);
            else
                GL.Enable(EnableCap.Multisample);

            FramebufferRenderer.MSAA_LEVEL = 1 << antialiasing;
        }

        protected void OnResize()
        {
            if (!initialized) return;
            GL.Viewport(0, 0, width, height);
            camera.ComputeProjectionMatrix();
            view = camera.GetViewMatrix();

            renderer?.Dispose();
            renderer = new FramebufferRenderer(width, height);
            UpdateAaLevel();
        }

        public LevelObject GetObjectAtScreenPosition(Vector2 pos)
        {
            if (xLock || yLock || zLock) return null;

            int hit = 0;
            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.ReadPixels((int) pos.X, height - (int) pos.Y, 1, 1, PixelFormat.RedInteger, PixelType.Int, ref hit);

            if (hit == 0) return null;

            RenderedObjectType hitType = (RenderedObjectType) (hit >> 24);
            int hitId = hit & 0xffffff;

            switch (hitType)
            {
                case RenderedObjectType.Null:
                    return null;
                case RenderedObjectType.Terrain:
                    return terrains[hitId];
                case RenderedObjectType.Shrub:
                    return level.shrubs[hitId];
                case RenderedObjectType.Tie:
                    return level.ties[hitId];
                case RenderedObjectType.Moby:
                    return level.mobs[hitId];
                case RenderedObjectType.Spline:
                    return level.splines[hitId];
                case RenderedObjectType.Cuboid:
                    return level.cuboids[hitId];
                case RenderedObjectType.Sphere:
                    return level.spheres[hitId];
                case RenderedObjectType.Cylinder:
                    return level.cylinders[hitId];
                case RenderedObjectType.Type0C:
                    return level.type0Cs[hitId];
                case RenderedObjectType.Tool:
                    switch (hitId)
                    {
                        case 0: xLock = true; break;
                        case 1: yLock = true; break;
                        case 2: zLock = true; break;
                    }
                    InvalidateView();
                    return null;
            }

            return null;
        }


        public void InvalidateView()
        {
            invalidate = true;
        }

        public bool TryRpcs3Hook()
        {
            if (level == null || level.game == null) return false;

            hook = new MemoryHook.MemoryHook(level.game.num);

            return hook.hookWorking;
        }

        public bool Rpcs3HookStatus()
        {
            if (hook != null && hook.hookWorking) return true;

            return false;
        }

        public void RemoveRpcs3Hook()
        {
            hook = null;
        }

        /// <summary>
        /// Export all distinct values of a select property found in a list of objects as a text file.
        /// </summary>
        private void ExportListOfProperties<T>(string path, IEnumerable<T> list, Func<T, string> selector)
        {
            if (path.Length <= 0) return;

            using (StreamWriter sw = File.CreateText(path))
            {
                HashSet<string> distinctItems = new HashSet<string>();
                foreach (T item in list)
                {
                    distinctItems.Add(selector(item));
                }

                foreach (string item in distinctItems)
                {
                    sw.WriteLine(item);
                }
            }
        }

        private void RenderBuffer(RenderableBuffer buffer)
        {
            buffer.UpdateVars();
            buffer.ComputeCulling(camera, enableDistanceCulling, enableFrustumCulling);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, lightsBufferObject);
            buffer.Select(selectedObjects);
            buffer.Render();
        }

        protected void OnPaint()
        {
            worldView = camera.GetViewMatrix() * camera.GetProjectionMatrix();

            camera.ComputeFrustum();

            if (level != null && level.levelVariables != null)
                GL.ClearColor(level.levelVariables.fogColor);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            UpdateDirectionalLights(level.lights);

            GL.UseProgram(shaderIDTable.shaderColor);
            GL.Uniform4(shaderIDTable.uniformColor, new Vector4(1, 1, 1, 1));
            GL.UniformMatrix4(shaderIDTable.uniformColorWorldToViewMatrix, false, ref worldView);
            GL.UseProgram(shaderIDTable.shaderMain);
            GL.UniformMatrix4(shaderIDTable.uniformWorldToViewMatrix, false, ref worldView);

            if (enableSkybox)
            {
                GL.Uniform1(shaderIDTable.uniformLevelObjectType, (int) RenderedObjectType.Skybox);
                GL.Uniform1(shaderIDTable.uniformUseFog, 0);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);
                Matrix4 mvp = view.ClearTranslation() * camera.GetProjectionMatrix();
                GL.UniformMatrix4(shaderIDTable.uniformWorldToViewMatrix, false, ref mvp);
                Matrix4 modelWorld = Matrix4.Identity;
                GL.UniformMatrix4(shaderIDTable.uniformModelToWorldMatrix, false, ref modelWorld);
                ActivateBuffersForModel(level.skybox);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
                foreach (TextureConfig conf in level.skybox.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.id > 0) ? textureIds[level.textures[conf.id]] : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.UniformMatrix4(shaderIDTable.uniformWorldToViewMatrix, false, ref worldView);
            }

            if (level != null && level.levelVariables != null)
            {
                GL.Uniform4(shaderIDTable.uniformFogColor, level.levelVariables.fogColor);
                GL.Uniform1(shaderIDTable.uniformFogNearDist, level.levelVariables.fogNearDistance);
                GL.Uniform1(shaderIDTable.uniformFogFarDist, level.levelVariables.fogFarDistance);
                GL.Uniform1(shaderIDTable.uniformFogNearIntensity, level.levelVariables.fogNearIntensity / 255.0f);
                GL.Uniform1(shaderIDTable.uniformFogFarIntensity, level.levelVariables.fogFarIntensity / 255.0f);
                GL.Uniform1(shaderIDTable.uniformUseFog, (enableFog) ? 1 : 0);
            }

            if (enableTerrain)
            {
                GL.EnableVertexAttribArray(3);
                GL.EnableVertexAttribArray(4);
                GL.Uniform1(shaderIDTable.uniformLevelObjectType, (int) RenderedObjectType.Terrain);
                foreach (RenderableBuffer buffer in terrainBuffers)
                    RenderBuffer(buffer);
                GL.DisableVertexAttribArray(4);
                GL.DisableVertexAttribArray(3);
            }

            if (enableShrub)
            {
                GL.Uniform1(shaderIDTable.uniformLevelObjectType, (int) RenderedObjectType.Shrub);
                foreach (RenderableBuffer buffer in shrubsBuffers)
                    RenderBuffer(buffer);
            }

            if (enableTie)
            {
                GL.EnableVertexAttribArray(3);
                GL.Uniform1(shaderIDTable.uniformLevelObjectType, (int) RenderedObjectType.Tie);
                foreach (RenderableBuffer buffer in tiesBuffers)
                    RenderBuffer(buffer);
                GL.DisableVertexAttribArray(3);
            }

            if (enableMoby)
            {
                if (hook != null) hook.UpdateMobys(level.mobs, level.mobyModels);

                GL.Uniform1(shaderIDTable.uniformLevelObjectType, (int) RenderedObjectType.Moby);

                if (enableTransparency)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }

                foreach (RenderableBuffer buffer in mobiesBuffers)
                    RenderBuffer(buffer);

                GL.Disable(EnableCap.Blend);
            }

            GL.UseProgram(shaderIDTable.shaderColor);

            if (enableSpline)
            {
                GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Spline);
                for (int i = 0; i < level.splines.Count; i++)
                {
                    Spline spline = level.splines[i];
                    GL.Uniform1(shaderIDTable.uniformColorLevelObjectNumber, i);
                    GL.UniformMatrix4(shaderIDTable.uniformColorModelToWorldMatrix, false, ref spline.modelMatrix);
                    GL.Uniform4(shaderIDTable.uniformColor, selectedObjects.Contains(spline) ? SELECTED_COLOR : NORMAL_COLOR);
                    ActivateBuffersForModel(spline);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
                }
            }


            if (enableCuboid)
            {
                GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Cuboid);
                for (int i = 0; i < level.cuboids.Count; i++)
                {
                    Cuboid cuboid = level.cuboids[i];
                    GL.Uniform1(shaderIDTable.uniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.uniformColorModelToWorldMatrix, false, ref cuboid.modelMatrix);
                    GL.Uniform4(shaderIDTable.uniformColor, selectedObjects.Contains(cuboid) ? SELECTED_COLOR : NORMAL_COLOR);
                    ActivateBuffersForModel(cuboid);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Cuboid.CUBE_ELEMENTS.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableSpheres)
            {
                GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Sphere);
                for (int i = 0; i < level.spheres.Count; i++)
                {
                    Sphere sphere = level.spheres[i];
                    GL.Uniform1(shaderIDTable.uniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.uniformColorModelToWorldMatrix, false, ref sphere.modelMatrix);
                    GL.Uniform4(shaderIDTable.uniformColor, selectedObjects.Contains(sphere) ? SELECTED_COLOR : NORMAL_COLOR);
                    ActivateBuffersForModel(sphere);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Sphere.SPHERE_TRIS.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableCylinders)
            {
                GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Cylinder);
                for (int i = 0; i < level.cylinders.Count; i++)
                {
                    Cylinder cylinder = level.cylinders[i];
                    GL.Uniform1(shaderIDTable.uniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.uniformColorModelToWorldMatrix, false, ref cylinder.modelMatrix);
                    GL.Uniform4(shaderIDTable.uniformColor, selectedObjects.Contains(cylinder) ? SELECTED_COLOR : NORMAL_COLOR);
                    ActivateBuffersForModel(cylinder);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Cylinder.CYLINDER_TRIS.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableType0C)
            {
                GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Type0C);
                for (int i = 0; i < level.type0Cs.Count; i++)
                {
                    Type0C type0C = level.type0Cs[i];
                    GL.Uniform1(shaderIDTable.uniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.uniformColorModelToWorldMatrix, false, ref type0C.modelMatrix);
                    GL.Uniform4(shaderIDTable.uniformColor, selectedObjects.Contains(type0C) ? SELECTED_COLOR : NORMAL_COLOR);

                    ActivateBuffersForModel(type0C);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                    GL.DrawElements(PrimitiveType.Triangles, Type0C.CUBE_ELEMENTS.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableCollision)
            {
                GL.Uniform1(shaderIDTable.uniformColorLevelObjectType, (int) RenderedObjectType.Null);
                GL.LineWidth(5.0f);

                GL.UseProgram(shaderIDTable.shaderColor);
                GL.Uniform4(shaderIDTable.uniformColor, new Vector4(1, 1, 1, 1));
                GL.UniformMatrix4(shaderIDTable.uniformColorWorldToViewMatrix, false, ref worldView);
                Matrix4 modelWorld = Matrix4.Identity;
                GL.UniformMatrix4(shaderIDTable.uniformColorModelToWorldMatrix, false, ref modelWorld);
                GL.UseProgram(shaderIDTable.shaderCollision);
                GL.UniformMatrix4(shaderIDTable.uniformCollisionWorldToViewMatrix, false, ref worldView);

                for (int i = 0; i < collisions.Count; i++)
                {
                    Collision col = (Collision) collisions[i].Item1;
                    int vbo = collisions[i].Item2;
                    int ibo = collisions[i].Item3;

                    if (col.indBuff.Length == 0) continue;

                    GL.UseProgram(shaderIDTable.shaderColor);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
                    GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.DrawElements(PrimitiveType.Triangles, col.indBuff.Length, DrawElementsType.UnsignedInt, 0);
                    GL.UseProgram(shaderIDTable.shaderCollision);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    GL.DrawElements(PrimitiveType.Triangles, col.indBuff.Length, DrawElementsType.UnsignedInt, 0);
                }

                GL.LineWidth(1.0f);
            }

            RenderTool();

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
        }

        public void AddSubFrame(Frame frame)
        {
            if (!subFrames.Contains(frame)) subFrames.Add(frame);
        }
    }

}
