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
        public Level level { get; set; }

        private List<TerrainFragment> terrains = new List<TerrainFragment>();
        private List<Tuple<Model,int,int>> collisions = new List<Tuple<Model, int, int>>();

        private static Vector4 normalColor = new Vector4(1, 1, 1, 1); // White
        private static Vector4 selectedColor = new Vector4(1, 0, 1, 1); // Purple

        public Matrix4 worldView { get; set; }

        public int shaderID { get; set; }
        public int colorShaderID { get; set; }
        public int collisionShaderID { get; set; }
        public int matrixID { get; set; }
        public int colorID { get; set; }

        private int uniformFogColorID;
        private int uniformFogNearDistID;
        private int uniformFogFarDistID;
        private int uniformFogNearIntensityID;
        private int uniformFogFarIntensityID;
        private int uniformUseFogID;

        private Matrix4 projection { get; set; }
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
            enableFog = true, enableCameraInfo = true;

        public Camera camera;
        private Tool currentTool;
        public Tool translateTool, rotationTool, scalingTool, vertexTranslator;

        private ConditionalWeakTable<IRenderable, BufferContainer> bufferTable;
        public Dictionary<Texture, int> textureIds;

        MemoryHook.MemoryHook hook;

        private List<int> collisionVbo = new List<int>();
        private List<int> collisionIbo = new List<int>();

        private int Width, Height;
        private int targetTexture;

        private List<Frame> subFrames;
        private List<Action<LevelObject>> selectionCallbacks;

        public LevelFrame(Window wnd) : base(wnd)
        {
            subFrames = new List<Frame>();
            selectionCallbacks = new List<Action<LevelObject>>();
            bufferTable = new ConditionalWeakTable<IRenderable, BufferContainer>();
            camera = new Camera();
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
                    if (ImGui.MenuItem("Save"))
                    {
                        level.Save();
                    }

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
                        if (ImGui.MenuItem("Level Model"))
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
                                level.collBytesEngine = ReadBlock(fs, 0, (int)fs.Length);
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
                        subFrames.Add(new ModelFrame(this.wnd, this));
                    }
                    if (ImGui.MenuItem("Texture viewer"))
                    {
                        subFrames.Add(new TextureFrame(this.wnd, this));
                    }
                    if (ImGui.MenuItem("Light config"))
                    {
                        subFrames.Add(new PropertyFrame(this.wnd, this, level.lightConfig, "Light config"));
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
                    if (ImGui.Checkbox("Transparency", ref enableTransparency)) InvalidateView();
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

        private void RenderTextOverlay()
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

            if (Width <= 0 || Height <= 0) return;

            if (Width != prevWidth || Height != prevHeight)
            {
                InvalidateView();
                OnResize();
            }

            System.Numerics.Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowZero = new Vector2(windowPos.X + vMin.X, windowPos.Y + vMin.Y);
            mousePos = wnd.MousePosition - windowZero;
            contentRegion = new Rectangle((int)windowZero.X, (int)windowZero.Y, Width, Height);
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
            RenderTextOverlay();
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

            //Setup general shader
            shaderID = GL.CreateProgram();
            LoadShader(Path.Join(shaderFolder, "vs.glsl"), ShaderType.VertexShader, shaderID);
            LoadShader(Path.Join(shaderFolder, "fs.glsl"), ShaderType.FragmentShader, shaderID);
            GL.LinkProgram(shaderID);

            //Setup color shader
            colorShaderID = GL.CreateProgram();
            LoadShader(Path.Join(shaderFolder, "colorshadervs.glsl"), ShaderType.VertexShader, colorShaderID);
            LoadShader(Path.Join(shaderFolder, "colorshaderfs.glsl"), ShaderType.FragmentShader, colorShaderID);
            GL.LinkProgram(colorShaderID);

            //Setup color shader
            collisionShaderID = GL.CreateProgram();
            LoadShader(Path.Join(shaderFolder, "collisionshadervs.glsl"), ShaderType.VertexShader, collisionShaderID);
            LoadShader(Path.Join(shaderFolder, "collisionshaderfs.glsl"), ShaderType.FragmentShader, collisionShaderID);
            GL.LinkProgram(collisionShaderID);

            matrixID = GL.GetUniformLocation(shaderID, "MVP");
            colorID = GL.GetUniformLocation(colorShaderID, "incolor");

            uniformFogColorID = GL.GetUniformLocation(shaderID, "fogColor");
            uniformFogNearDistID = GL.GetUniformLocation(shaderID, "fogNearDistance");
            uniformFogFarDistID = GL.GetUniformLocation(shaderID, "fogFarDistance");
            uniformFogNearIntensityID = GL.GetUniformLocation(shaderID, "fogNearIntensity");
            uniformFogFarIntensityID = GL.GetUniformLocation(shaderID, "fogFarIntensity");
            uniformUseFogID = GL.GetUniformLocation(shaderID, "useFog");

            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)Width / Height, 0.1f, 10000.0f);
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
                LoadSingleCollisionBO((Collision)level.collisionEngine);
            } else
            {
                foreach (Model collisionModel in level.collisionChunks)
                {
                    LoadSingleCollisionBO((Collision)collisionModel);
                }
            }
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
            setSelectedChunks();

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
            InvalidateView();
        }

        public void setSelectedChunks()
        {
            if (level.terrainChunks.Count == 0)
            {
                terrains.Clear();
                terrains.AddRange(level.terrainEngine.fragments);
                collisions.Clear();
                collisions.Add(new Tuple<Model,int,int>(level.collisionEngine, collisionVbo[0], collisionIbo[0]));
            } else
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
            if (!mouseGrabHandler.TryGrabMouse(wnd, allowNewGrab))
                return false;

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
            Point absoluteMousePos = new Point((int)wnd.MousePosition.X, (int)wnd.MousePosition.Y);
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

            Vector3 mouseRay = MouseToWorldRay(projection, view, new Size(Width, Height), mousePos);

            if (wnd.IsMouseButtonDown(MouseButton.Left))
            {
                LevelObject obj = null;
                Vector3 direction = Vector3.Zero;
                int tempTextureId = 0;

                FramebufferRenderer.ToTexture(Width, Height, ref tempTextureId, () =>
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

        public void FakeDrawSplines(List<Spline> splines, int offset)
        {
            for (int i = 0; i < splines.Count; i++)
            {
                Spline spline = splines[i];
                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                Matrix4 worldView = this.worldView;
                GL.UniformMatrix4(matrixID, false, ref worldView);
                this.worldView = worldView;

                byte[] cols = BitConverter.GetBytes(i + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));

                ActivateBuffersForModel(spline);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
                GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
            }
        }


        public void FakeDrawCuboids(List<Cuboid> cuboids, int offset)
        {
            for (int i = 0; i < cuboids.Count; i++)
            {
                Cuboid cuboid = cuboids[i];

                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                Matrix4 mvp = cuboid.modelMatrix * worldView;
                GL.UniformMatrix4(matrixID, false, ref mvp);

                byte[] cols = BitConverter.GetBytes(i + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));

                ActivateBuffersForModel(cuboid);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawElements(PrimitiveType.Triangles, Cuboid.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public void FakeDrawSpheres(List<Sphere> spheres, int offset)
        {
            for (int i = 0; i < spheres.Count; i++)
            {
                Sphere sphere = spheres[i];

                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                Matrix4 mvp = sphere.modelMatrix * worldView;
                GL.UniformMatrix4(matrixID, false, ref mvp);

                byte[] cols = BitConverter.GetBytes(i + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));

                ActivateBuffersForModel(sphere);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawElements(PrimitiveType.Triangles, Sphere.sphereTris.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public void FakeDrawCylinders(List<Cylinder> cylinders, int offset)
        {
            for (int i = 0; i < cylinders.Count; i++)
            {
                Cylinder cylinder = cylinders[i];

                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                Matrix4 mvp = cylinder.modelMatrix * worldView;
                GL.UniformMatrix4(matrixID, false, ref mvp);

                byte[] cols = BitConverter.GetBytes(i + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));

                ActivateBuffersForModel(cylinder);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawElements(PrimitiveType.Triangles, Cylinder.cylinderTris.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public void FakeDrawType0Cs(List<Type0C> type0Cs, int offset)
        {
            for (int i = 0; i < type0Cs.Count; i++)
            {
                Type0C type0C = type0Cs[i];

                GL.UseProgram(colorShaderID);
                GL.EnableVertexAttribArray(0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                Matrix4 mvp = type0C.modelMatrix * worldView;
                GL.UniformMatrix4(matrixID, false, ref mvp);

                byte[] cols = BitConverter.GetBytes(i + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));

                ActivateBuffersForModel(type0C);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawElements(PrimitiveType.Triangles, Cuboid.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public void FakeDrawObjects(List<ModelObject> levelObjects, int offset)
        {
            for (int i = 0; i < levelObjects.Count; i++)
            {
                ModelObject levelObject = levelObjects[i];

                if (levelObject.model == null || levelObject.model.vertexBuffer == null)
                    continue;

                Matrix4 mvp = levelObject.modelMatrix * worldView;  //Has to be done in this order to work correctly
                GL.UniformMatrix4(matrixID, false, ref mvp);

                ActivateBuffersForModel(levelObject.model);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);

                byte[] cols = BitConverter.GetBytes(i + offset);
                GL.Uniform4(colorID, new Vector4(cols[0] / 255f, cols[1] / 255f, cols[2] / 255f, 1));
                GL.DrawElements(PrimitiveType.Triangles, levelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);

            }
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
        }

        void LoadShader(string filename, ShaderType type, int program)
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
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)Width / Height, 0.1f, 10000.0f);
            view = camera.GetViewMatrix();
        }

        public LevelObject GetObjectAtScreenPosition(Vector2 pos)
        {
            LevelObject returnObject = null;

            int mobyOffset = 0, tieOffset = 0, shrubOffset = 0, splineOffset = 0, cuboidOffset = 0, sphereOffset = 0, cylinderOffset = 0, type0COffset = 0, tfragOffset = 0;

            GL.Viewport(0, 0, Width, Height);
            GL.Enable(EnableCap.DepthTest);
            GL.LineWidth(5.0f);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(colorShaderID);
            GL.EnableVertexAttribArray(0);

            worldView = view * projection;

            int offset = 0;


            if (enableMoby)
            {
                mobyOffset = offset;
                FakeDrawObjects(level.mobs.Cast<ModelObject>().ToList(), mobyOffset);
                offset += level.mobs.Count;
            }

            if (enableTie)
            {
                tieOffset = offset;
                FakeDrawObjects(level.ties.Cast<ModelObject>().ToList(), tieOffset);
                offset += level.ties.Count;
            }

            if (enableShrub)
            {
                shrubOffset = offset;
                FakeDrawObjects(level.shrubs.Cast<ModelObject>().ToList(), shrubOffset);
                offset += level.shrubs.Count;
            }

            if (enableSpline)
            {
                splineOffset = offset;
                FakeDrawSplines(level.splines, splineOffset);
                offset += level.splines.Count;
            }

            if (enableCuboid)
            {
                cuboidOffset = offset;
                FakeDrawCuboids(level.cuboids, cuboidOffset);
                offset += level.cuboids.Count;
            }

            if (enableSpheres)
            {
                sphereOffset = offset;
                FakeDrawSpheres(level.spheres, sphereOffset);
                offset += level.spheres.Count;
            }

            if (enableCylinders)
            {
                cylinderOffset = offset;
                FakeDrawCylinders(level.cylinders, cylinderOffset);
                offset += level.cylinders.Count;
            }

            if (enableType0C)
            {
                type0COffset = offset;
                FakeDrawType0Cs(level.type0Cs, type0COffset);
                offset += level.type0Cs.Count;
            }

            if (enableTerrain)
            {
                tfragOffset = offset;
                FakeDrawObjects(terrains.Cast<ModelObject>().ToList(), tfragOffset);
            }

            RenderTool();

            Pixel pixel = new Pixel();
            GL.ReadPixels((int) pos.X, Height - (int) pos.Y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);

            if (level != null && level.levelVariables != null)
                GL.ClearColor(level.levelVariables.fogColor);

            // Some GPU's put the alpha at 0, others at 255
            if (pixel.A == 255 || pixel.A == 0)
            {
                pixel.A = 0;

                if (!xLock && !yLock && !zLock)
                {
                    bool hitTool = false;
                    if (pixel.R == 255 && pixel.G == 0 && pixel.B == 0)
                    {
                        hitTool = true;
                        xLock = true;
                    }
                    else if (pixel.R == 0 && pixel.G == 255 && pixel.B == 0)
                    {
                        hitTool = true;
                        yLock = true;
                    }
                    else if (pixel.R == 0 && pixel.G == 0 && pixel.B == 255)
                    {
                        hitTool = true;
                        zLock = true;
                    }

                    if (hitTool)
                    {
                        InvalidateView();
                        return null;
                    }
                }

                int id = (int)pixel.ToUInt32();
                if (enableMoby && id < level.mobs?.Count)
                {
                    returnObject = level.mobs[id];
                }
                else if (enableTie && id - tieOffset < level.ties.Count)
                {
                    returnObject = level.ties[id - tieOffset];
                }
                else if (enableShrub && id - shrubOffset < level.shrubs.Count)
                {
                    returnObject = level.shrubs[id - shrubOffset];
                }
                else if (enableSpline && id - splineOffset < level.splines.Count)
                {
                    returnObject = level.splines[id - splineOffset];
                }
                else if (enableCuboid && id - cuboidOffset < level.cuboids.Count)
                {
                    returnObject = level.cuboids[id - cuboidOffset];
                }
                else if (enableSpheres && id - sphereOffset < level.spheres.Count)
                {
                    returnObject = level.spheres[id - sphereOffset];
                }
                else if (enableCylinders && id - cylinderOffset < level.cylinders.Count)
                {
                    returnObject = level.cylinders[id - cylinderOffset];
                }
                else if (enableType0C && id - type0COffset < level.type0Cs.Count)
                {
                    returnObject = level.type0Cs[id - type0COffset];
                }
                else if (enableTerrain && id - tfragOffset < terrains.Count)
                {
                    returnObject = terrains[id - tfragOffset];
                }
            }

            return returnObject;
        }


        public void InvalidateView()
        {
            invalidate = true;
        }

        void RenderModelObject(ModelObject modelObject, bool selected)
        {
            if (modelObject.model == null || modelObject.model.vertexBuffer == null || modelObject.model.textureConfig.Count == 0) return;
            Matrix4 mvp = modelObject.modelMatrix * worldView;  //Has to be done in this order to work correctly
            GL.UniformMatrix4(matrixID, false, ref mvp);
            ActivateBuffersForModel(modelObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);

            //Bind textures one by one, applying it to the relevant vertices based on the index array
            foreach (TextureConfig conf in modelObject.model.textureConfig)
            {
                GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? textureIds[level.textures[conf.ID]] : 0);
                GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
            }

            if (selected)
            {
                bool switchBlends = enableTransparency && (modelObject is Moby);

                if (switchBlends)
                    GL.Disable(EnableCap.Blend);

                GL.UseProgram(colorShaderID);
                GL.Uniform4(colorID, new Vector4(1, 1, 1, 1));
                GL.UniformMatrix4(matrixID, false, ref mvp);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, modelObject.model.indexBuffer.Length, DrawElementsType.UnsignedShort, 0);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.UseProgram(shaderID);

                if (switchBlends)
                    GL.Enable(EnableCap.Blend);
            }
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

        protected void OnPaint()
        {
            worldView = view * projection;

            if (level != null && level.levelVariables != null)
                GL.ClearColor(level.levelVariables.fogColor);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.UseProgram(shaderID);
            if (level != null && level.levelVariables != null)
            {
                GL.Uniform4(uniformFogColorID, level.levelVariables.fogColor);
                GL.Uniform1(uniformFogNearDistID, level.levelVariables.fogNearDistance);
                GL.Uniform1(uniformFogFarDistID, level.levelVariables.fogFarDistance);
                GL.Uniform1(uniformFogNearIntensityID, level.levelVariables.fogNearIntensity / 255.0f);
                GL.Uniform1(uniformFogFarIntensityID, level.levelVariables.fogFarIntensity / 255.0f);
                GL.Uniform1(uniformUseFogID, (enableFog) ? 1 : 0);
            }

            if (enableSkybox)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);
                Matrix4 mvp = view.ClearTranslation() * projection;
                GL.UniformMatrix4(matrixID, false, ref mvp);
                ActivateBuffersForModel(level.skybox);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);
                foreach (TextureConfig conf in level.skybox.textureConfig)
                {
                    GL.BindTexture(TextureTarget.Texture2D, (conf.ID > 0) ? textureIds[level.textures[conf.ID]] : 0);
                    GL.DrawElements(PrimitiveType.Triangles, conf.size, DrawElementsType.UnsignedShort, conf.start * sizeof(ushort));
                }
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
            }

            if (enableTerrain)
                foreach (TerrainFragment tFrag in terrains)
                    RenderModelObject(tFrag, tFrag == selectedObject);

            if (enableShrub)
                foreach (Shrub shrub in level.shrubs)
                    RenderModelObject(shrub, shrub == selectedObject);

            if (enableTie)
                foreach (Tie tie in level.ties)
                    RenderModelObject(tie, tie == selectedObject);


            if (enableMoby)
            {
                if (hook != null) hook.UpdateMobys(level.mobs, level.mobyModels);

                if (enableTransparency)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }

                foreach (Moby mob in level.mobs)
                {
                    RenderModelObject(mob, mob == selectedObject);
                }

                GL.Disable(EnableCap.Blend);
            }

            GL.UseProgram(colorShaderID);

            if (enableSpline)
                foreach (Spline spline in level.splines)
                {
                    var worldView = this.worldView;
                    GL.UniformMatrix4(matrixID, false, ref worldView);
                    GL.Uniform4(colorID, spline == selectedObject ? selectedColor : normalColor);
                    ActivateBuffersForModel(spline);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, spline.vertexBuffer.Length / 3);
                }

            if (enableCuboid)
                foreach (Cuboid cuboid in level.cuboids)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    Matrix4 mvp = cuboid.modelMatrix * worldView;
                    GL.UniformMatrix4(matrixID, false, ref mvp);
                    GL.Uniform4(colorID, selectedObject == cuboid ? selectedColor : normalColor);
                    ActivateBuffersForModel(cuboid);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Cuboid.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }

            if (enableSpheres)
                foreach (Sphere sphere in level.spheres)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    Matrix4 mvp = sphere.modelMatrix * worldView;
                    GL.UniformMatrix4(matrixID, false, ref mvp);
                    GL.Uniform4(colorID, selectedObject == sphere ? selectedColor : normalColor);
                    ActivateBuffersForModel(sphere);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Sphere.sphereTris.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }

            if (enableCylinders)
                foreach (Cylinder cylinder in level.cylinders)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    Matrix4 mvp = cylinder.modelMatrix * worldView;
                    GL.UniformMatrix4(matrixID, false, ref mvp);
                    GL.Uniform4(colorID, selectedObject == cylinder ? selectedColor : normalColor);
                    ActivateBuffersForModel(cylinder);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.DrawElements(PrimitiveType.Triangles, Cylinder.cylinderTris.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }

            if (enableType0C)
                foreach (Type0C type0c in level.type0Cs)
                {
                    GL.UseProgram(colorShaderID);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    Matrix4 mvp = type0c.modelMatrix * worldView;
                    GL.UniformMatrix4(matrixID, false, ref mvp);
                    GL.Uniform4(colorID, type0c == selectedObject ? selectedColor : normalColor);

                    ActivateBuffersForModel(type0c);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                    GL.DrawElements(PrimitiveType.Triangles, Type0C.cubeElements.Length, DrawElementsType.UnsignedShort, 0);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }

            if (enableCollision)
            {
                for (int i = 0; i < collisions.Count; i++)
                {
                    Collision col = (Collision)collisions[i].Item1;
                    int vbo = collisions[i].Item2;
                    int ibo = collisions[i].Item3;

                    if (col.indBuff.Length == 0) continue;

                    GL.UseProgram(colorShaderID);
                    Matrix4 worldView = this.worldView;
                    GL.UniformMatrix4(matrixID, false, ref worldView);
                    GL.Uniform4(colorID, new Vector4(1, 1, 1, 1));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
                    GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float) * 4, sizeof(float) * 3);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.DrawElements(PrimitiveType.Triangles, col.indBuff.Length, DrawElementsType.UnsignedInt, 0);
                    GL.UseProgram(collisionShaderID);
                    GL.UniformMatrix4(matrixID, false, ref worldView);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    GL.DrawElements(PrimitiveType.Triangles, col.indBuff.Length, DrawElementsType.UnsignedInt, 0);
                }
            }

            RenderTool();

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
        }

        public void AddSubFrame(Frame frame)
        {
            if (!subFrames.Contains(frame)) subFrames.Add(frame);
        }

    }

}
