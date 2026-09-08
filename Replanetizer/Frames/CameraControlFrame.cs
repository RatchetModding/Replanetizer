using System;
using System.Collections.Generic;
using ImGuiNET;
using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Frames
{
    public class CameraControlFrame : LevelSubFrame
    {
        private enum ControlMode
        {
            Manual,
            Spline,
            Keyframe
        }

        private enum RotationRepresentation
        {
            YawAndPitch,
            EulerAngles,
            Quaternion
        }

        public enum EasingType
        {
            Linear,
            SmoothStep,
            EaseInOut,
            EaseIn,
            EaseOut
        }
        public class CameraKeyframe
        {
            public Vector3 Position { get; set; }
            public Vector2 Rotation { get; set; }
            public string Name { get; set; }
            public float Duration { get; set; }

            public CameraKeyframe(Vector3 position, Vector2 rotation, string name = "", float duration = 2.0f)
            {
                Position = position;
                Rotation = rotation;
                Name = name;
                Duration = duration;
            }

            public CameraKeyframe Clone()
            {
                return new CameraKeyframe(Position, Rotation, Name, Duration);
            }
        }

        private const float DEGREES_PER_RADIAN = 180.0f / MathF.PI;
        private const float RADIANS_PER_DEGREE = MathF.PI / 180.0f;
        private const float MAX_TARGET_PITCH = 89.9f * RADIANS_PER_DEGREE;

        private ControlMode mode = ControlMode.Manual;
        private RotationRepresentation rotationRepresentation = RotationRepresentation.YawAndPitch;
        private int selectedSplineIndex = -1;
        private float splineProgress;
        private float playbackSpeed = 1.0f;
        private bool isPlaying;
        private bool rotateTowardTarget;
        private bool targetPickerArmed;
        private bool splinePickerArmed;
        private LevelObject? targetObject;

        private List<CameraKeyframe> cameraKeyframes = new List<CameraKeyframe>();
        private int currentPlaybackKeyframe = 0;
        private float keyframeProgress = 0f;
        private bool isPlayingKeyframes = false;
        private bool loopKeyframes = false;
        private float defaultKeyframeDuration = 2.0f;
        private EasingType keyframeEasingType = EasingType.Linear;

        private static readonly string[] EASING_NAMES = { "Linear", "Smooth Step", "Ease In Out", "Ease In", "Ease Out" };

        protected override string frameName { get; set; } = "Camera Control";

        public CameraControlFrame(Window wnd, LevelFrame levelFrame) : base(wnd, levelFrame)
        {
            levelFrame.ObjectSelected += LevelFrameOnObjectSelected;
        }
        public bool visible = true;
        private void HandleVisibilityToggle()
        {
            if (ImGui.GetIO().WantTextInput)
                return;

            if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                visible = !visible;
        }

        public override void RenderAsWindow(float deltaTime)
        {
            HandleVisibilityToggle(); // Pressing Esc toggles rendering of the camera control window.

            if (isPlayingKeyframes)
                UpdateKeyframePlayback(deltaTime);

            if (!visible)
                return;

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

            ImGui.Separator();

            switch (mode)
            {
                case ControlMode.Manual:
                    RenderManualControls();
                    break;
                case ControlMode.Spline:
                    RenderSplineControls(deltaTime);
                    break;
                case ControlMode.Keyframe:
                    RenderKeyframeControls(deltaTime);
                    break;
            }
        }
        private static string GetModeName(ControlMode mode)
        {
            return mode switch
            {
                ControlMode.Manual => "Manual",
                ControlMode.Spline => "Spline",
                ControlMode.Keyframe => "Keyframe",
                _ => "Manual"
            };
        }
        private void RenderModeSelector()
        {
            string modeName = GetModeName(mode);
            if (ImGui.BeginCombo("Mode", modeName))
            {
                RenderModeOption(ControlMode.Manual, "Manual");
                RenderModeOption(ControlMode.Spline, "Spline");
                RenderModeOption(ControlMode.Keyframe, "Keyframe");
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
                isPlayingKeyframes = false;
                if (mode != ControlMode.Spline)
                {
                    targetPickerArmed = false;
                    splinePickerArmed = false;
                }
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

            if (ImGui.DragFloat3("Position", ref position, 0.01f))
            {
                levelFrame.camera.SetPosition(position.X, position.Y, position.Z);
                levelFrame.InvalidateView();
            }

            RenderRotationRepresentation();
        }

        private void RenderRotationRepresentation()
        {
            string representationName = GetRotationRepresentationName(rotationRepresentation);
            if (ImGui.BeginCombo("Rotation", representationName))
            {
                RenderRotationRepresentationOption(
                    RotationRepresentation.YawAndPitch, "yaw and pitch");
                RenderRotationRepresentationOption(
                    RotationRepresentation.EulerAngles, "euler angles");
                RenderRotationRepresentationOption(
                    RotationRepresentation.Quaternion, "quaternion");
                ImGui.EndCombo();
            }

            Vector3 cameraRotation = levelFrame.camera.rotation;
            switch (rotationRepresentation)
            {
                case RotationRepresentation.YawAndPitch:
                    RenderYawAndPitch(cameraRotation);
                    break;
                case RotationRepresentation.EulerAngles:
                    RenderEulerAngles(cameraRotation);
                    break;
                case RotationRepresentation.Quaternion:
                    RenderQuaternion(cameraRotation);
                    break;
            }
        }

        private void RenderRotationRepresentationOption(
            RotationRepresentation representation, string label)
        {
            bool selected = rotationRepresentation == representation;
            if (ImGui.Selectable(label, selected))
                rotationRepresentation = representation;

            if (selected)
                ImGui.SetItemDefaultFocus();
        }

        private static string GetRotationRepresentationName(RotationRepresentation representation)
        {
            return representation switch
            {
                RotationRepresentation.YawAndPitch => "yaw and pitch",
                RotationRepresentation.EulerAngles => "euler angles",
                RotationRepresentation.Quaternion => "quaternion",
                _ => "yaw and pitch"
            };
        }

        private void RenderYawAndPitch(Vector3 cameraRotation)
        {
            float pitch = cameraRotation.X * DEGREES_PER_RADIAN;
            float yaw = cameraRotation.Z * DEGREES_PER_RADIAN;

            bool pitchChanged = ImGui.DragFloat("Pitch (degrees)", ref pitch, 0.01f, -89.99f, 89.99f);
            bool yawChanged = ImGui.DragFloat("Yaw (degrees)", ref yaw, 0.01f);
            if (pitchChanged || yawChanged)
            {
                levelFrame.camera.SetRotation(
                    pitch * RADIANS_PER_DEGREE,
                    yaw * RADIANS_PER_DEGREE
                );
                levelFrame.InvalidateView();
            }
        }

        private void RenderEulerAngles(Vector3 cameraRotation)
        {
            Vector3 eulerAngles = Quaternion.FromEulerAngles(cameraRotation).ToEulerAngles();
            System.Numerics.Vector3 degrees = new(
                eulerAngles.X * DEGREES_PER_RADIAN,
                eulerAngles.Y * DEGREES_PER_RADIAN,
                eulerAngles.Z * DEGREES_PER_RADIAN
            );

            if (ImGui.InputFloat3("Euler angles (degrees)", ref degrees))
            {
                Quaternion quaternion = Quaternion.FromEulerAngles(
                    degrees.X * RADIANS_PER_DEGREE,
                    degrees.Y * RADIANS_PER_DEGREE,
                    degrees.Z * RADIANS_PER_DEGREE
                );
                levelFrame.camera.SetRotation(quaternion.ToEulerAngles());
                levelFrame.InvalidateView();
            }
        }

        private void RenderQuaternion(Vector3 cameraRotation)
        {
            Quaternion quaternion = Quaternion.FromEulerAngles(cameraRotation);
            System.Numerics.Vector4 components = new(
                quaternion.X, quaternion.Y, quaternion.Z, quaternion.W
            );

            if (!ImGui.InputFloat4("Quaternion (X, Y, Z, W)", ref components))
                return;

            float lengthSquared = components.X * components.X +
                components.Y * components.Y +
                components.Z * components.Z +
                components.W * components.W;
            if (lengthSquared <= float.Epsilon)
                return;

            float inverseLength = 1.0f / MathF.Sqrt(lengthSquared);
            quaternion = new Quaternion(
                components.X * inverseLength,
                components.Y * inverseLength,
                components.Z * inverseLength,
                components.W * inverseLength
            );
            levelFrame.camera.SetRotation(quaternion.ToEulerAngles());
            levelFrame.InvalidateView();
        }

        private void RenderSplineControls(float deltaTime)
        {
            List<Spline> splines = levelFrame.level.splines;
            NormalizeSelectedSplineIndex(splines);

            if (splines.Count == 0)
            {
                ImGui.Text("This level has no splines.");
                RenderTargetControls();
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
                        SelectSpline(splines[i]);

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            if (ImGui.Button(splinePickerArmed ? "Picking spline..." : "Pick spline"))
            {
                splinePickerArmed = true;
                targetPickerArmed = false;
            }
            if (splinePickerArmed)
                ImGui.Text("Select a spline in the level.");

            ImGui.Separator();

            RenderTargetControls();

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
            float playbackDuration = 1.0f / playbackSpeed;
            ImGui.InputFloat("Playback Duration (seconds)", ref playbackDuration, 1.0f, 3600.0f);
            ImGui.PopItemWidth();
            playbackSpeed = 1.0f / MathF.Max(1.0f, playbackDuration);

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

        private void RenderTargetControls()
        {
            bool trackingChanged = ImGui.Checkbox("Rotate toward target", ref rotateTowardTarget);

            if (rotateTowardTarget)
            {
                if (targetObject == null)
                    ImGui.Text("Target: None");
                else
                    ImGui.Text($"Target: {targetObject.GetType().Name}");

                if (ImGui.Button(targetPickerArmed ? "Picking..." : "Pick target"))
                {
                    targetPickerArmed = true;
                    splinePickerArmed = false;
                }

                if (targetPickerArmed)
                    ImGui.Text("Select an object in the level.");

                if (trackingChanged && GetSelectedSpline() != null)
                    ApplyTargetRotation();
            }
        }
        private void RenderKeyframeControls(float deltaTime)
        {
            ImGui.Text($"Keyframes: {cameraKeyframes.Count}");

            if (ImGui.Button("Add Keyframe"))
                AddCameraKeyframe();

            ImGui.SameLine();
            if (ImGui.Button("Clear All"))
                ClearAllKeyframes();

            ImGui.SliderFloat("Default Duration", ref defaultKeyframeDuration, 0.5f, 50.0f);

            int easingIndex = (int) keyframeEasingType;
            if (ImGui.Combo("Easing", ref easingIndex, EASING_NAMES, EASING_NAMES.Length))
                keyframeEasingType = (EasingType) easingIndex;

            ImGui.Separator();

            if (cameraKeyframes.Count >= 2)
            {
                if (!isPlayingKeyframes)
                {
                    if (ImGui.Button("Play"))
                        PlayKeyframes(false);
                    ImGui.SameLine();
                    if (ImGui.Button("Loop"))
                        PlayKeyframes(true);
                }
                else
                {
                    ImGui.Text($"Playing: {currentPlaybackKeyframe + 1}/{cameraKeyframes.Count}");
                    ImGui.SliderFloat("Progress", ref keyframeProgress, 0.0f, 1.0f);

                    if (ImGui.Button("Stop"))
                        StopKeyframePlayback();
                }

                ImGui.Separator();
            }

            if (cameraKeyframes.Count == 0)
            {
                ImGui.TextWrapped("No keyframes.");
            }
            else
            {
                ImGui.Text("Keyframe List:");
                ImGui.BeginChild("KeyframeList", new System.Numerics.Vector2(0, -1));

                for (int i = 0; i < cameraKeyframes.Count; i++)
                {
                    CameraKeyframe kf = cameraKeyframes[i];

                    ImGui.PushID(i);

                    ImGui.Text($"Pos: ({kf.Position.X:F2}, {kf.Position.Y:F2}, {kf.Position.Z:F2})");

                    if (i != 0)
                    {
                        float duration = kf.Duration;
                        if (ImGui.SliderFloat("Duration", ref duration, 0.1f, 50.0f))
                            kf.Duration = duration;
                    }

                    if (ImGui.Button("Jump To"))
                        JumpToKeyframe(i);

                    ImGui.SameLine();
                    if (ImGui.Button("Update"))
                        UpdateKeyframe(i);

                    ImGui.SameLine();
                    if (ImGui.Button("Delete"))
                    {
                        RemoveKeyframe(i);
                        ImGui.PopID();
                        break;
                    }

                    ImGui.Separator();
                    ImGui.PopID();
                }

                ImGui.EndChild();
            }


        }

        private void UpdateKeyframePlayback(float deltaTime)
        {
            if (!isPlayingKeyframes || cameraKeyframes.Count < 2)
                return;

            CameraKeyframe fromKeyframe = cameraKeyframes[currentPlaybackKeyframe];
            CameraKeyframe toKeyframe = cameraKeyframes[(currentPlaybackKeyframe + 1) % cameraKeyframes.Count];

            keyframeProgress += deltaTime / toKeyframe.Duration;

            if (keyframeProgress >= 1.0f)
            {
                currentPlaybackKeyframe++;
                keyframeProgress = 0f;

                if (currentPlaybackKeyframe >= cameraKeyframes.Count - 1)
                {
                    if (loopKeyframes)
                    {
                        currentPlaybackKeyframe = 0;
                    }
                    else
                    {
                        levelFrame.camera.SetPosition(toKeyframe.Position.X, toKeyframe.Position.Y, toKeyframe.Position.Z);
                        levelFrame.camera.SetRotation(toKeyframe.Rotation.X, toKeyframe.Rotation.Y);
                        isPlayingKeyframes = false;
                        levelFrame.InvalidateView();
                        return;
                    }
                }

                fromKeyframe = cameraKeyframes[currentPlaybackKeyframe];
                toKeyframe = cameraKeyframes[(currentPlaybackKeyframe + 1) % cameraKeyframes.Count];
            }

            float t = ApplyEasing(keyframeProgress, keyframeEasingType);

            Vector3 pos = Vector3.Lerp(fromKeyframe.Position, toKeyframe.Position, t);

            levelFrame.camera.SetPosition(pos.X, pos.Y, pos.Z);

            Vector2 rot = LerpRotation(fromKeyframe.Rotation, toKeyframe.Rotation, t);
            levelFrame.camera.SetRotation(rot.X, rot.Y);

            levelFrame.InvalidateView();
        }

        private static Vector2 LerpRotation(Vector2 from, Vector2 to, float t)
        {
            Vector2 delta = to - from;

            while (delta.Y > MathF.PI) delta.Y -= MathF.PI * 2;
            while (delta.Y < -MathF.PI) delta.Y += MathF.PI * 2;

            return from + delta * t;
        }

        private static float ApplyEasing(float t, EasingType easingType)
        {
            return easingType switch
            {
                EasingType.Linear => t,
                EasingType.SmoothStep => t * t * (3f - 2f * t),
                EasingType.EaseInOut => t < 0.5f
                    ? 2f * t * t
                    : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
                EasingType.EaseIn => t * t,
                EasingType.EaseOut => 1f - (1f - t) * (1f - t),
                _ => t
            };
        }

        private void AddCameraKeyframe(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = $"Keyframe {cameraKeyframes.Count + 1}";

            var keyframe = new CameraKeyframe(
                levelFrame.camera.position,
                new Vector2(levelFrame.camera.rotation.X, levelFrame.camera.rotation.Z),
                name,
                defaultKeyframeDuration
            );

            cameraKeyframes.Add(keyframe);
        }

        private void RemoveKeyframe(int index)
        {
            if (index >= 0 && index < cameraKeyframes.Count)
                cameraKeyframes.RemoveAt(index);
        }

        private void ClearAllKeyframes()
        {
            cameraKeyframes.Clear();
            StopKeyframePlayback();
        }

        private void PlayKeyframes(bool loop)
        {
            if (cameraKeyframes.Count < 2)
                return;

            isPlayingKeyframes = true;
            currentPlaybackKeyframe = 0;
            keyframeProgress = 0f;
            loopKeyframes = loop;

            CameraKeyframe firstKeyframe = cameraKeyframes[0];
            levelFrame.camera.SetPosition(firstKeyframe.Position.X, firstKeyframe.Position.Y, firstKeyframe.Position.Z);
            levelFrame.camera.SetRotation(firstKeyframe.Rotation.X, firstKeyframe.Rotation.Y);
        }

        private void StopKeyframePlayback()
        {
            isPlayingKeyframes = false;
            currentPlaybackKeyframe = 0;
            keyframeProgress = 0f;
        }

        private void JumpToKeyframe(int index)
        {
            if (index < 0 || index >= cameraKeyframes.Count)
                return;

            CameraKeyframe keyframe = cameraKeyframes[index];
            levelFrame.camera.SetPosition(keyframe.Position.X, keyframe.Position.Y, keyframe.Position.Z);
            levelFrame.camera.SetRotation(keyframe.Rotation.X, keyframe.Rotation.Y);
            levelFrame.InvalidateView();
        }

        private void UpdateKeyframe(int index)
        {
            if (index < 0 || index >= cameraKeyframes.Count)
                return;

            cameraKeyframes[index].Position = levelFrame.camera.position;
            cameraKeyframes[index].Rotation = new Vector2(levelFrame.camera.rotation.X, levelFrame.camera.rotation.Z);
        }

        private void LevelFrameOnObjectSelected(LevelObject obj)
        {
            if (splinePickerArmed)
            {
                Spline? spline = GetSplineFromObject(obj);
                if (spline != null)
                {
                    SelectSpline(spline);
                    splinePickerArmed = false;
                }
                return;
            }

            if (targetPickerArmed)
            {
                targetObject = obj;
                targetPickerArmed = false;

                if (mode == ControlMode.Spline && rotateTowardTarget)
                    ApplyTargetRotation();
            }
        }

        private Spline? GetSplineFromObject(LevelObject obj)
        {
            Spline? spline = obj as Spline;
            if (spline == null && obj is GrindPath grindPath)
                spline = grindPath.spline;

            if (spline == null || !levelFrame.level.splines.Contains(spline))
                return null;

            return spline;
        }

        private void SelectSpline(Spline spline)
        {
            int splineIndex = levelFrame.level.splines.IndexOf(spline);
            if (splineIndex < 0)
                return;

            selectedSplineIndex = splineIndex;
            splineProgress = 0.0f;
            isPlaying = false;
            ApplySplinePosition(spline, splineProgress);
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
            ApplyTargetRotation();
            levelFrame.InvalidateView();
        }

        private void ApplyTargetRotation()
        {
            if (!rotateTowardTarget || targetObject == null)
                return;

            Vector3 direction = targetObject.position - levelFrame.camera.position;
            float horizontalDistance = MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (direction.LengthSquared <= 0.0001f)
                return;

            float pitch = MathF.Atan2(direction.Z, horizontalDistance);
            pitch = MathHelper.Clamp(pitch, -MAX_TARGET_PITCH, MAX_TARGET_PITCH);

            Vector3 cameraRotation = levelFrame.camera.rotation;
            float yaw = horizontalDistance <= 0.0001f
                ? cameraRotation.Z
                : MathF.Atan2(-direction.X, direction.Y);

            levelFrame.camera.SetRotation(pitch, yaw);
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

        public override void Dispose()
        {
            levelFrame.ObjectSelected -= LevelFrameOnObjectSelected;
            base.Dispose();
        }
    }
}
