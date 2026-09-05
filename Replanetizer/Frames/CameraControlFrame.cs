using System;
using System.Collections.Generic;
using ImGuiNET;
using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;

namespace Replanetizer.Frames
{
    public class CameraControlFrame : LevelSubFrame
    {
        private enum ControlMode
        {
            Manual,
            Spline
        }

        private const float DEGREES_PER_RADIAN = 180.0f / MathF.PI;
        private const float RADIANS_PER_DEGREE = MathF.PI / 180.0f;

        private ControlMode mode = ControlMode.Manual;
        private int selectedSplineIndex = -1;
        private float splineProgress;
        private float playbackSpeed = 1.0f;
        private bool isPlaying;

        protected override string frameName { get; set; } = "Camera Control";

        public CameraControlFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
        }

        public override void RenderAsWindow(float deltaTime)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(360, 0), ImGuiCond.FirstUseEver);
            if (ImGui.Begin(frameName, ref isOpen))
            {
                Render(deltaTime);
                ImGui.End();
            }
        }

        public override void Render(float deltaTime)
        {
            RenderModeSelector();

            if (mode == ControlMode.Manual)
                RenderManualControls();
            else
                RenderSplineControls(deltaTime);
        }

        private void RenderModeSelector()
        {
            string modeName = mode == ControlMode.Manual ? "Manual" : "Spline";
            if (ImGui.BeginCombo("Mode", modeName))
            {
                RenderModeOption(ControlMode.Manual, "Manual");
                RenderModeOption(ControlMode.Spline, "Spline");
                ImGui.EndCombo();
            }
        }

        private void RenderModeOption(ControlMode option, string label)
        {
            bool selected = mode == option;
            if (ImGui.Selectable(label, selected))
            {
                mode = option;
                isPlaying = false;
                if (mode == ControlMode.Spline)
                    ApplySplinePosition(GetSelectedSpline(), splineProgress);
            }

            if (selected)
                ImGui.SetItemDefaultFocus();
        }

        private void RenderManualControls()
        {
            Vector3 cameraPosition = levelFrame.camera.position;
            System.Numerics.Vector3 position = new(cameraPosition.X, cameraPosition.Y, cameraPosition.Z);

            if (ImGui.InputFloat3("Position", ref position))
            {
                levelFrame.camera.SetPosition(position.X, position.Y, position.Z);
                levelFrame.InvalidateView();
            }

            Vector3 cameraRotation = levelFrame.camera.rotation;
            float pitch = cameraRotation.X * DEGREES_PER_RADIAN;
            float yaw = cameraRotation.Z * DEGREES_PER_RADIAN;

            if (ImGui.InputFloat("Pitch (degrees)", ref pitch))
            {
                levelFrame.camera.SetRotation(pitch * RADIANS_PER_DEGREE, cameraRotation.Z);
                levelFrame.InvalidateView();
            }

            if (ImGui.InputFloat("Yaw (degrees)", ref yaw))
            {
                levelFrame.camera.SetRotation(cameraRotation.X, yaw * RADIANS_PER_DEGREE);
                levelFrame.InvalidateView();
            }
        }

        private void RenderSplineControls(float deltaTime)
        {
            List<Spline> splines = levelFrame.level.splines;
            NormalizeSelectedSplineIndex(splines);

            if (splines.Count == 0)
            {
                ImGui.Text("This level has no splines.");
                return;
            }

            string selectedLabel = selectedSplineIndex >= 0
                ? GetSplineLabel(splines[selectedSplineIndex])
                : "Select a spline";

            if (ImGui.BeginCombo("Spline", selectedLabel))
            {
                for (int i = 0; i < splines.Count; i++)
                {
                    bool selected = selectedSplineIndex == i;
                    if (ImGui.Selectable(GetSplineLabel(splines[i]), selected))
                    {
                        selectedSplineIndex = i;
                        splineProgress = 0.0f;
                        isPlaying = false;
                        ApplySplinePosition(splines[i], splineProgress);
                    }

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            Spline? spline = GetSelectedSpline();
            if (spline == null)
                return;

            int vertexCount = spline.GetVertexCount();
            if (vertexCount == 0)
            {
                ImGui.Text("Selected spline has no vertices.");
                return;
            }

            bool progressChanged = ImGui.SliderFloat("Progress", ref splineProgress, 0.0f, 1.0f);
            if (progressChanged)
            {
                isPlaying = false;
                ApplySplinePosition(spline, splineProgress);
            }

            ImGui.PushItemWidth(120.0f);
            ImGui.InputFloat("Speed", ref playbackSpeed, 0.1f, 1.0f);
            ImGui.PopItemWidth();
            playbackSpeed = MathF.Max(0.0f, playbackSpeed);

            if (ImGui.Button(isPlaying ? "Pause" : "Play"))
                isPlaying = !isPlaying;
            ImGui.SameLine();
            if (ImGui.Button("Reset"))
            {
                splineProgress = 0.0f;
                isPlaying = false;
                ApplySplinePosition(spline, splineProgress);
            }

            if (isPlaying)
            {
                splineProgress += deltaTime * playbackSpeed;
                splineProgress %= 1.0f;
                ApplySplinePosition(spline, splineProgress);
            }
        }

        private void NormalizeSelectedSplineIndex(IReadOnlyList<Spline> splines)
        {
            if (selectedSplineIndex >= splines.Count)
            {
                selectedSplineIndex = -1;
                splineProgress = 0.0f;
                isPlaying = false;
            }
        }

        private Spline? GetSelectedSpline()
        {
            if (selectedSplineIndex < 0 || selectedSplineIndex >= levelFrame.level.splines.Count)
                return null;

            return levelFrame.level.splines[selectedSplineIndex];
        }

        private static string GetSplineLabel(Spline spline)
        {
            return $"Spline {spline.id} ({spline.GetVertexCount()} vertices)";
        }

        private void ApplySplinePosition(Spline? spline, float progress)
        {
            if (spline == null || spline.GetVertexCount() == 0)
                return;

            levelFrame.camera.SetPosition(EvaluateSpline(spline, progress));
            levelFrame.InvalidateView();
        }

        private static Vector3 EvaluateSpline(Spline spline, float progress)
        {
            int vertexCount = spline.GetVertexCount();
            if (vertexCount == 1)
                return spline.GetVertex(0);

            progress = MathHelper.Clamp(progress, 0.0f, 1.0f);
            float vertexPosition = progress * (vertexCount - 1);
            int firstVertex = (int) MathF.Floor(vertexPosition);
            int secondVertex = Math.Min(firstVertex + 1, vertexCount - 1);
            float interpolation = vertexPosition - firstVertex;

            return Vector3.Lerp(
                spline.GetVertex(firstVertex),
                spline.GetVertex(secondVertex),
                interpolation
            );
        }
    }
}
