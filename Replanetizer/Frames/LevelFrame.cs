using System;
using System.Collections.Generic;
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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override string frameName { get; set; } = "Level";

        private FramebufferRenderer renderer;
        public Level level { get; set; }

        private List<TerrainFragment> terrains = new List<TerrainFragment>();
        private List<Tuple<Model, int, int>> collisions = new List<Tuple<Model, int, int>>();

        private static Vector4 normalColor = new Vector4(1, 1, 1, 1); // White
        private static Vector4 selectedColor = new Vector4(1, 0, 1, 1); // Purple

        public Matrix4 worldView;

        public ShaderIDTable shaderIDTable;

        private int lightsBufferObject;
        private float[][] lightsData;

        private int alignmentUBO = GL.GetInteger(GetPName.UniformBufferOffsetAlignment);

        private float movingAvgFrametime = 1.0f;

        private Matrix4 view { get; set; }

        private int currentSplineVertex;
        public LevelObject selectedObject;

        private Vector2 mousePos;
        private Vector3 prevMouseRay;
        private Rectangle contentRegion;
        private int lastMouseX, lastMouseY;
        private bool xLock = false, yLock = false, zLock = false;
        private MouseGrabHandler mouseGrabHandler = new()
        {
            MouseButton = MouseButton.Right
        };

        public bool initialized, invalidate;
        public bool[] selectedChunks;
        public bool enableMoby = true, enableTie = true, enableShrub = true, enableSpline = false,
            enableCuboid = false, enableSpheres = false, enableCylinders = false, enableType0C = false,
            enableSkybox = true, enableTerrain = true, enableCollision = false, enableTransparency = true,
            enableDistanceCulling = true, enableFrustumCulling = true, enableFog = true, enableCameraInfo = true;

        public Camera camera;
        private Tool currentTool;
        public Tool translateTool, rotationTool, scalingTool, vertexTranslator;

        private ConditionalWeakTable<IRenderable, BufferContainer> bufferTable;
        public Dictionary<Texture, int> textureIds;

        public List<RenderableBuffer> mobiesBuffers, tiesBuffers, shrubsBuffers, terrainBuffers;

        MemoryHook.MemoryHook hook;

        private List<int> collisionVbo = new List<int>();
        private List<int> collisionIbo = new List<int>();

        private int Width, Height;

        private List<Frame> subFrames;
        private List<Action<LevelObject>> selectionCallbacks;

        public LevelFrame(Window wnd) : base(wnd)
        {
            subFrames = new List<Frame>();
            selectionCallbacks = new List<Action<LevelObject>>();
            bufferTable = new ConditionalWeakTable<IRenderable, BufferContainer>();
            camera = new Camera();
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
                        var newFrame = new PropertyFrame(this.wnd, this, selectedObject, listenToCallbacks: true);
                        RegisterCallback(newFrame.SelectionCallback);
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
                    if (ImGui.MenuItem("Delete object  [Del]")) DeleteObject(selectedObject);
                    if (ImGui.MenuItem("Deselect object")) SelectObject(null);

                    ImGui.EndMenu();
                }

                if (selectedChunks.Length > 0)
                {
                    if (ImGui.BeginMenu("Chunks"))
                    {
                        for (int i = 0; i < selectedChunks.Length; i++)
                        {
                            if (ImGui.Checkbox("Chunk " + i, ref selectedChunks[i])) setSelectedChunks();
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

            const float pad = 10f;
            const ImGuiWindowFlags windowFlags =
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
                workPos.X + pad,
                workPos.Y + workSize.Y - pad
            );
            SysVector2 windowPosPivot = new(0f, 1f);
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);

            ImGui.SetNextWindowBgAlpha(0.35f);

            if (ImGui.Begin("Info overlay", windowFlags))
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
                var camRotX = fToDegrees(camera.rotation.X);
                var camRotZ = fToDegrees(camera.rotation.Z);
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
            int prevWidth = Width, prevHeight = Height;

            System.Numerics.Vector2 vMin = ImGui.GetWindowContentRegionMin();
            System.Numerics.Vector2 vMax = ImGui.GetWindowContentRegionMax();

            Width = (int) (vMax.X - vMin.X);
            Height = (int) (vMax.Y - vMin.Y);

            camera.aspect = (float) Width / Height;

            if (Width <= 0 || Height <= 0) return;

            if (Width != prevWidth || Height != prevHeight)
            {
                InvalidateView();
                OnResize();
            }

            System.Numerics.Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowZero = new Vector2(windowPos.X + vMin.X, windowPos.Y + vMin.Y);
            mousePos = wnd.MousePosition - windowZero;
            contentRegion = new Rectangle((int) windowZero.X, (int) windowZero.Y, Width, Height);
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
                    GL.Viewport(0, 0, Width, Height);

                    OnPaint();
                });
                invalidate = false;
            }
            ImGui.Image((IntPtr) renderer.targetTexture, new System.Numerics.Vector2(Width, Height),
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
            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);

            //Setup openGL variables
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.LineWidth(5.0f);

            string applicationFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string shaderFolder = Path.Join(applicationFolder, "Shaders");

            shaderIDTable = new ShaderIDTable();

            shaderIDTable.ShaderMain = LinkShader(shaderFolder, "vs.glsl", "fs.glsl");
            shaderIDTable.ShaderColor = LinkShader(shaderFolder, "colorshadervs.glsl", "colorshaderfs.glsl");
            shaderIDTable.ShaderCollision = LinkShader(shaderFolder, "collisionshadervs.glsl", "collisionshaderfs.glsl");

            shaderIDTable.UniformWorldToViewMatrix = GL.GetUniformLocation(shaderIDTable.ShaderMain, "WorldToView");
            shaderIDTable.UniformModelToWorldMatrix = GL.GetUniformLocation(shaderIDTable.ShaderMain, "ModelToWorld");
            shaderIDTable.UniformColorWorldToViewMatrix = GL.GetUniformLocation(shaderIDTable.ShaderColor, "WorldToView");
            shaderIDTable.UniformColorModelToWorldMatrix = GL.GetUniformLocation(shaderIDTable.ShaderColor, "ModelToWorld");
            shaderIDTable.UniformCollisionWorldToViewMatrix = GL.GetUniformLocation(shaderIDTable.ShaderCollision, "WorldToView");

            shaderIDTable.UniformColor = GL.GetUniformLocation(shaderIDTable.ShaderColor, "incolor");

            shaderIDTable.UniformFogColor = GL.GetUniformLocation(shaderIDTable.ShaderMain, "fogColor");
            shaderIDTable.UniformFogNearDist = GL.GetUniformLocation(shaderIDTable.ShaderMain, "fogNearDistance");
            shaderIDTable.UniformFogFarDist = GL.GetUniformLocation(shaderIDTable.ShaderMain, "fogFarDistance");
            shaderIDTable.UniformFogNearIntensity = GL.GetUniformLocation(shaderIDTable.ShaderMain, "fogNearIntensity");
            shaderIDTable.UniformFogFarIntensity = GL.GetUniformLocation(shaderIDTable.ShaderMain, "fogFarIntensity");
            shaderIDTable.UniformUseFog = GL.GetUniformLocation(shaderIDTable.ShaderMain, "useFog");

            shaderIDTable.UniformLevelObjectType = GL.GetUniformLocation(shaderIDTable.ShaderMain, "levelObjectType");
            shaderIDTable.UniformLevelObjectNumber = GL.GetUniformLocation(shaderIDTable.ShaderMain, "levelObjectNumber");
            shaderIDTable.UniformColorLevelObjectType = GL.GetUniformLocation(shaderIDTable.ShaderColor, "levelObjectType");
            shaderIDTable.UniformColorLevelObjectNumber = GL.GetUniformLocation(shaderIDTable.ShaderColor, "levelObjectNumber");

            shaderIDTable.UniformAmbientColor = GL.GetUniformLocation(shaderIDTable.ShaderMain, "staticColor");
            shaderIDTable.UniformLightIndex = GL.GetUniformLocation(shaderIDTable.ShaderMain, "lightIndex");

            RenderableBuffer.shaderIDTable = shaderIDTable;

            LoadDirectionalLights(level.lights);

            camera.ComputeProjectionMatrix();
            view = camera.GetViewMatrix();

            translateTool = new TranslationTool();
            rotationTool = new RotationTool();
            scalingTool = new ScalingTool();
            vertexTranslator = new VertexTranslationTool();

            initialized = true;
        }

        private void loadTexture(Texture t)
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
                loadTexture(t);
            }

            foreach (List<Texture> list in level.armorTextures)
            {
                foreach (Texture t in list)
                {
                    loadTexture(t);
                }
            }

            foreach (Texture t in level.gadgetTextures)
            {
                loadTexture(t);
            }

            foreach (Mission mission in level.missions)
            {
                foreach (Texture t in mission.textures)
                {
                    loadTexture(t);
                }
            }
        }

        private void LoadSingleCollisionBO(Collision col)
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
                LoadSingleCollisionBO((Collision) level.collisionEngine);
            }
            else
            {
                foreach (Model collisionModel in level.collisionChunks)
                {
                    LoadSingleCollisionBO((Collision) collisionModel);
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
            int loc = GL.GetUniformBlockIndex(shaderIDTable.ShaderMain, "lights");
            GL.UniformBlockBinding(shaderIDTable.ShaderMain, loc, 0);

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
            SelectObject(null);
            setSelectedChunks();
            shrubsBuffers = GetRenderableBuffer(level.shrubs, RenderedObjectType.Shrub);
            tiesBuffers = GetRenderableBuffer(level.ties, RenderedObjectType.Tie);
            mobiesBuffers = GetRenderableBuffer(level.mobs, RenderedObjectType.Moby);

            InvalidateView();
        }

        public void setSelectedChunks()
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

        private void TriggerSelectionCallbacks()
        {
            foreach (Delegate callback in selectionCallbacks)
            {
                callback.DynamicInvoke(selectedObject);
            }
        }

        public void RegisterCallback(Action<LevelObject> callback)
        {
            selectionCallbacks.Add(callback);
        }

        public void SelectObject(LevelObject newObject = null)
        {
            if (newObject != null)
            {
                if ((selectedObject is Spline) && !(newObject is Spline))
                {
                    //Previous object was spline, new isn't
                    if (currentTool is VertexTranslationTool) SelectTool(null);
                }
            }

            selectedObject = newObject;
            InvalidateView();
            TriggerSelectionCallbacks();
        }

        public void DeleteObject(LevelObject levelObject)
        {
            if (levelObject == null) return;
            SelectObject(null);
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
            if (!(selectedObject is Spline spline)) return;
            if (!(currentTool is VertexTranslationTool)) return;

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
            SelectObject(newMoby);
            InvalidateView();
        }

        private void HandleKeyboardShortcuts()
        {
            if (wnd.IsKeyPressed(Keys.D1)) SelectTool(translateTool);
            if (wnd.IsKeyPressed(Keys.D2)) SelectTool(rotationTool);
            if (wnd.IsKeyPressed(Keys.D3)) SelectTool(scalingTool);
            if (wnd.IsKeyPressed(Keys.D4)) SelectTool(vertexTranslator);
            if (wnd.IsKeyPressed(Keys.D5)) SelectTool();
            if (wnd.IsKeyPressed(Keys.Delete)) DeleteObject(selectedObject);
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
            float moveSpeed = wnd.IsKeyDown(Keys.LeftShift) ? 40 : 10;
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

            switch (currentTool)
            {
                case TranslationTool t:
                    selectedObject.Translate(direction * magnitude);
                    break;
                case RotationTool t:
                    selectedObject.Rotate(direction * magnitude);
                    break;
                case ScalingTool t:
                    selectedObject.Scale(direction * magnitude + Vector3.One);
                    break;
                case VertexTranslationTool t:
                    if (selectedObject is Spline spline)
                    {
                        spline.TranslateVertex(currentSplineVertex, direction * magnitude);
                        if (hook != null && hook.hookWorking)
                        {
                            hook.HandleSplineTranslation(level, spline, currentSplineVertex);
                        }
                    }
                    break;
            }

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

            Vector3 mouseRay = MouseToWorldRay(camera.GetProjectionMatrix(), view, new Size(Width, Height), mousePos);

            if (wnd.IsMouseButtonDown(MouseButton.Left))
            {
                LevelObject obj = null;
                Vector3 direction = Vector3.Zero;

                renderer.ExposeFramebuffer(() =>
                {
                    obj = GetObjectAtScreenPosition(mousePos);
                });

                if (xLock)
                {
                    direction = Vector3.UnitX;
                }
                else if (yLock)
                {
                    direction = Vector3.UnitY;
                }
                else if (zLock)
                {
                    direction = Vector3.UnitZ;
                }

                if (xLock || yLock || zLock)
                {
                    HandleToolUpdates(mouseRay, direction);
                }
                else if (!wnd.MouseState.WasButtonDown(MouseButton.Left))
                {
                    SelectObject(obj);
                }
            }
            else
            {
                xLock = false;
                yLock = false;
                zLock = false;
            }

            lastMouseX = (int) wnd.MousePosition.X;
            lastMouseY = (int) wnd.MousePosition.Y;
            prevMouseRay = mouseRay;
        }

        private Vector3 GetInputAxes()
        {
            float xAxis = 0, yAxis = 0, zAxis = 0;

            if (wnd.KeyboardState.IsKeyDown(Keys.W)) yAxis++;
            if (wnd.KeyboardState.IsKeyDown(Keys.S)) yAxis--;
            if (wnd.KeyboardState.IsKeyDown(Keys.A)) xAxis--;
            if (wnd.KeyboardState.IsKeyDown(Keys.D)) xAxis++;
            if (wnd.KeyboardState.IsKeyDown(Keys.Q)) zAxis--;
            if (wnd.KeyboardState.IsKeyDown(Keys.E)) zAxis++;

            return new Vector3(xAxis, yAxis, zAxis);
        }

        public void ActivateBuffersForModel(IRenderable renderable)
        {
            BufferContainer container = bufferTable.GetValue(renderable, BufferContainer.FromRenderable);
            container.Bind();
        }

        public void RenderTool()
        {
            // Render tool on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Tool);
            GL.LineWidth(5.0f);

            if ((selectedObject != null) && (currentTool != null))
            {
                if ((currentTool is VertexTranslationTool) && (selectedObject is Spline spline))
                {
                    currentTool.Render(spline.GetVertex(currentSplineVertex), this);
                }
                else
                {
                    currentTool.Render(selectedObject.position, this);
                }
            }

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
            Logger.Debug("Compiled shader from {0}, log: {1}", filename, GL.GetShaderInfoLog(address));
        }

        protected void OnResize()
        {
            if (!initialized) return;
            GL.Viewport(0, 0, Width, Height);
            camera.ComputeProjectionMatrix();
            view = camera.GetViewMatrix();

            renderer?.Dispose();
            renderer = new FramebufferRenderer(Width, Height);
        }

        public LevelObject GetObjectAtScreenPosition(Vector2 pos)
        {
            if (xLock || yLock || zLock) return null;

            int hit = 0;
            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.ReadPixels((int) pos.X, Height - (int) pos.Y, 1, 1, PixelFormat.RedInteger, PixelType.Int, ref hit);

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

        public bool TryRPCS3Hook()
        {
            if (level == null || level.game == null) return false;

            hook = new MemoryHook.MemoryHook(level.game.num);

            return hook.hookWorking;
        }

        public bool RPCS3HookStatus()
        {
            if (hook != null && hook.hookWorking) return true;

            return false;
        }

        public void RemoveRPCS3Hook()
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
            buffer.Select(selectedObject);
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

            GL.UseProgram(shaderIDTable.ShaderColor);
            GL.Uniform4(shaderIDTable.UniformColor, new Vector4(1, 1, 1, 1));
            GL.UniformMatrix4(shaderIDTable.UniformColorWorldToViewMatrix, false, ref worldView);
            GL.UseProgram(shaderIDTable.ShaderMain);
            GL.UniformMatrix4(shaderIDTable.UniformWorldToViewMatrix, false, ref worldView);

            if (enableSkybox)
            {
                GL.Uniform1(shaderIDTable.UniformLevelObjectType, (int) RenderedObjectType.Skybox);
                GL.Uniform1(shaderIDTable.UniformUseFog, 0);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);
                Matrix4 mvp = view.ClearTranslation() * camera.GetProjectionMatrix();
                GL.UniformMatrix4(shaderIDTable.UniformWorldToViewMatrix, false, ref mvp);
                Matrix4 modelWorld = Matrix4.Identity;
                GL.UniformMatrix4(shaderIDTable.UniformModelToWorldMatrix, false, ref modelWorld);
                ActivateBuffersForModel(level.skybox);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
                foreach (TextureConfig conf in level.skybox.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? textureIds[level.textures[conf.ID]] : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.UniformMatrix4(shaderIDTable.UniformWorldToViewMatrix, false, ref worldView);
            }

            if (level != null && level.levelVariables != null)
            {
                GL.Uniform4(shaderIDTable.UniformFogColor, level.levelVariables.fogColor);
                GL.Uniform1(shaderIDTable.UniformFogNearDist, level.levelVariables.fogNearDistance);
                GL.Uniform1(shaderIDTable.UniformFogFarDist, level.levelVariables.fogFarDistance);
                GL.Uniform1(shaderIDTable.UniformFogNearIntensity, level.levelVariables.fogNearIntensity / 255.0f);
                GL.Uniform1(shaderIDTable.UniformFogFarIntensity, level.levelVariables.fogFarIntensity / 255.0f);
                GL.Uniform1(shaderIDTable.UniformUseFog, (enableFog) ? 1 : 0);
            }

            if (enableTerrain)
            {
                GL.EnableVertexAttribArray(3);
                GL.EnableVertexAttribArray(4);
                GL.Uniform1(shaderIDTable.UniformLevelObjectType, (int) RenderedObjectType.Terrain);
                foreach (RenderableBuffer buffer in terrainBuffers)
                    RenderBuffer(buffer);
                GL.DisableVertexAttribArray(4);
                GL.DisableVertexAttribArray(3);
            }

            if (enableShrub)
            {
                GL.Uniform1(shaderIDTable.UniformLevelObjectType, (int) RenderedObjectType.Shrub);
                foreach (RenderableBuffer buffer in shrubsBuffers)
                    RenderBuffer(buffer);
            }

            if (enableTie)
            {
                GL.EnableVertexAttribArray(3);
                GL.Uniform1(shaderIDTable.UniformLevelObjectType, (int) RenderedObjectType.Tie);
                foreach (RenderableBuffer buffer in tiesBuffers)
                    RenderBuffer(buffer);
                GL.DisableVertexAttribArray(3);
            }

            if (enableMoby)
            {
                if (hook != null) hook.UpdateMobys(level.mobs, level.mobyModels);

                GL.Uniform1(shaderIDTable.UniformLevelObjectType, (int) RenderedObjectType.Moby);

                if (enableTransparency)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }

                foreach (RenderableBuffer buffer in mobiesBuffers)
                    RenderBuffer(buffer);

                GL.Disable(EnableCap.Blend);
            }

            GL.UseProgram(shaderIDTable.ShaderColor);

            if (enableSpline)
            {
                GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Spline);
                for (int i = 0; i < level.splines.Count; i++)
                {
                    Spline spline = level.splines[i];
                    GL.Uniform1(shaderIDTable.UniformColorLevelObjectNumber, i);
                    GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref spline.modelMatrix);
                    GL.Uniform4(shaderIDTable.UniformColor, spline == selectedObject ? selectedColor : normalColor);
                    ActivateBuffersForModel(spline);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
                }
            }


            if (enableCuboid)
            {
                GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Cuboid);
                for (int i = 0; i < level.cuboids.Count; i++)
                {
                    Cuboid cuboid = level.cuboids[i];
                    GL.Uniform1(shaderIDTable.UniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref cuboid.modelMatrix);
                    GL.Uniform4(shaderIDTable.UniformColor, selectedObject == cuboid ? selectedColor : normalColor);
                    ActivateBuffersForModel(cuboid);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Cuboid.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableSpheres)
            {
                GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Sphere);
                for (int i = 0; i < level.spheres.Count; i++)
                {
                    Sphere sphere = level.spheres[i];
                    GL.Uniform1(shaderIDTable.UniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref sphere.modelMatrix);
                    GL.Uniform4(shaderIDTable.UniformColor, selectedObject == sphere ? selectedColor : normalColor);
                    ActivateBuffersForModel(sphere);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Sphere.sphereTris.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableCylinders)
            {
                GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Cylinder);
                for (int i = 0; i < level.cylinders.Count; i++)
                {
                    Cylinder cylinder = level.cylinders[i];
                    GL.Uniform1(shaderIDTable.UniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref cylinder.modelMatrix);
                    GL.Uniform4(shaderIDTable.UniformColor, selectedObject == cylinder ? selectedColor : normalColor);
                    ActivateBuffersForModel(cylinder);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Cylinder.cylinderTris.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableType0C)
            {
                GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Type0C);
                for (int i = 0; i < level.type0Cs.Count; i++)
                {
                    Type0C type0c = level.type0Cs[i];
                    GL.Uniform1(shaderIDTable.UniformColorLevelObjectNumber, i);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref type0c.modelMatrix);
                    GL.Uniform4(shaderIDTable.UniformColor, type0c == selectedObject ? selectedColor : normalColor);

                    ActivateBuffersForModel(type0c);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                    GL.DrawElements(PrimitiveType.Triangles, Type0C.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }


            if (enableCollision)
            {
                GL.Uniform1(shaderIDTable.UniformColorLevelObjectType, (int) RenderedObjectType.Null);
                GL.LineWidth(5.0f);

                GL.UseProgram(shaderIDTable.ShaderColor);
                GL.Uniform4(shaderIDTable.UniformColor, new Vector4(1, 1, 1, 1));
                GL.UniformMatrix4(shaderIDTable.UniformColorWorldToViewMatrix, false, ref worldView);
                Matrix4 modelWorld = Matrix4.Identity;
                GL.UniformMatrix4(shaderIDTable.UniformColorModelToWorldMatrix, false, ref modelWorld);
                GL.UseProgram(shaderIDTable.ShaderCollision);
                GL.UniformMatrix4(shaderIDTable.UniformCollisionWorldToViewMatrix, false, ref worldView);

                for (int i = 0; i < collisions.Count; i++)
                {
                    Collision col = (Collision) collisions[i].Item1;
                    int vbo = collisions[i].Item2;
                    int ibo = collisions[i].Item3;

                    if (col.indBuff.Length == 0) continue;

                    GL.UseProgram(shaderIDTable.ShaderColor);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
                    GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.DrawElements(PrimitiveType.Triangles, col.indBuff.Length, DrawElementsType.UnsignedInt, 0);
                    GL.UseProgram(shaderIDTable.ShaderCollision);
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
