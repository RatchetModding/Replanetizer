using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Drawing;
using ImGuiNET;
using LibReplanetizer.LevelObjects;

namespace Replanetizer.Frames
{
    public class PropertyFrame : Frame
    {
        protected sealed override string frameName { get; set; } = "Properties";
        private object selectedObject;
        private bool listenToCallbacks;
        private bool hideCallbackButton = false;
        private LevelFrame levelFrame;

        private Dictionary<string, Dictionary<string, PropertyInfo>> properties;
        
        public PropertyFrame(Window wnd, LevelFrame levelFrame = null, object selectedObject = null, string overrideFrameName = null, bool listenToCallbacks = false, bool hideCallbackButton = false) : base(wnd)
        {
            if (overrideFrameName != null && overrideFrameName.Length > 0)
            {
                frameName = overrideFrameName;
            }

            this.levelFrame = levelFrame;
            this.selectedObject = selectedObject;
            this.listenToCallbacks = listenToCallbacks;
            this.hideCallbackButton = hideCallbackButton;
            if (selectedObject != null) RecomputeProperties();
        }

        public void SelectionCallback(object o)
        {
            if (!listenToCallbacks) return;
            if (o == null)
            {
                isOpen = false;
                return;
            }
            selectedObject = o;
            RecomputeProperties();
        }

        private void UpdateLevelFrame()
        {
            if (levelFrame != null) levelFrame.InvalidateView();
        }

        private void RecomputeProperties()
        {
            properties = new Dictionary<string, Dictionary<string, PropertyInfo>>();
            var objProps = selectedObject.GetType().GetProperties();
            foreach (var prop in objProps)
            {
                string category = prop.GetCustomAttribute<CategoryAttribute>()?.Category;
                if (category == null) category = "Unknowns";
                
                string displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                if (displayName == null) displayName = prop.Name;
                
                if (!properties.ContainsKey(category))
                {
                    properties[category] = new Dictionary<string, PropertyInfo>();
                }
                
                properties[category][displayName] = prop;
            }
        }

        public override void RenderAsWindow(float deltaTime)
        {
            if (ImGui.Begin(frameName, ref isOpen, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                Render(deltaTime);
                ImGui.End();
            }
        }

        public override void Render(float deltaTime)
        {
            if (properties == null)
            {
                ImGui.Text("Select an object");
                return;
            }

            if (!hideCallbackButton && listenToCallbacks)
            {
                if (ImGui.Button("Stop following object selection"))
                {
                    listenToCallbacks = false;
                }
            }
            foreach (var categoryPair in properties)
            {
                if (ImGui.CollapsingHeader(categoryPair.Key, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    foreach (var (key, value) in categoryPair.Value)
                    {
                        var val = value.GetValue(selectedObject);
                        var type = value.PropertyType;
                        if (value.GetSetMethod() == null) type = null;
                        if (type == typeof(string))
                        {
                            var v = Encoding.ASCII.GetBytes((string)val ?? string.Empty);
                            if (ImGui.InputText(key, v, (uint)v.Length))
                            {
                                value.SetValue(selectedObject, Encoding.ASCII.GetString(v));
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(int))
                        {
                            int v = (int) val;
                            if (ImGui.InputInt(key, ref v))
                            {
                                value.SetValue(selectedObject, v);
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(uint))
                        {
                            int v = unchecked((int)(uint)val);
                            if (ImGui.InputInt(key, ref v))
                            {
                                value.SetValue(selectedObject, unchecked((uint)v));
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(short))
                        {
                            int v = Convert.ToInt32(val);
                            if (ImGui.InputInt(key, ref v))
                            {
                                value.SetValue(selectedObject, (short)(v & 0xffff));
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(ushort))
                        {
                            int v = unchecked((ushort)val);
                            if (ImGui.InputInt(key, ref v))
                            {
                                value.SetValue(selectedObject, unchecked((ushort)(v & 0xffff)));
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(float))
                        {
                            var v = (float) val;
                            if (ImGui.InputFloat(key, ref v))
                            {
                                value.SetValue(selectedObject, v);
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(Color))
                        {
                            Color c = (Color)val;
                            Vector3 v = new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
                            if (ImGui.ColorEdit3(key, ref v))
                            {
                                Color newColor = Color.FromArgb((int)(v.X * 255.0f), (int)(v.Y * 255.0f), (int)(v.Z * 255.0f));
                                value.SetValue(selectedObject, newColor);
                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(OpenTK.Mathematics.Vector3))
                        {
                            OpenTK.Mathematics.Vector3 origV = (OpenTK.Mathematics.Vector3)val;
                            Vector3 v = new Vector3(origV.X, origV.Y, origV.Z);
                            if (ImGui.InputFloat3(key, ref v))
                            {
                                origV.X = v.X;
                                origV.Y = v.Y;
                                origV.Z = v.Z;
                                value.SetValue(selectedObject, origV);

                                if (selectedObject is LevelObject)
                                {
                                    ((LevelObject)selectedObject).UpdateTransformMatrix();
                                }

                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(OpenTK.Mathematics.Quaternion))
                        {
                            OpenTK.Mathematics.Vector3 origRot = ((OpenTK.Mathematics.Quaternion)val).ToEulerAngles();
                            Vector3 v = new Vector3(origRot.X, origRot.Y, origRot.Z);
                            if (ImGui.InputFloat3(key, ref v))
                            {
                                origRot.X = v.X;
                                origRot.Y = v.Y;
                                origRot.Z = v.Z;
                                value.SetValue(selectedObject, new OpenTK.Mathematics.Quaternion(origRot.X, origRot.Y, origRot.Z));

                                if (selectedObject is LevelObject)
                                {
                                    ((LevelObject)selectedObject).UpdateTransformMatrix();
                                }

                                UpdateLevelFrame();
                            }
                        }
                        else if (type == typeof(OpenTK.Mathematics.Matrix4))
                        {
                            OpenTK.Mathematics.Matrix4 mat = (OpenTK.Mathematics.Matrix4)val;
                            Vector4 v1 = new Vector4(mat.M11, mat.M12, mat.M13, mat.M14);
                            Vector4 v2 = new Vector4(mat.M21, mat.M22, mat.M23, mat.M24);
                            Vector4 v3 = new Vector4(mat.M31, mat.M32, mat.M33, mat.M34);
                            Vector4 v4 = new Vector4(mat.M41, mat.M42, mat.M43, mat.M44);

                            bool change = false;

                            if (ImGui.InputFloat4(key + " Row 1", ref v1))
                            {
                                change = true;
                                mat.M11 = v1.X;
                                mat.M12 = v1.Y;
                                mat.M13 = v1.Z;
                                mat.M14 = v1.W;
                            }

                            if (ImGui.InputFloat4(key + " Row 2", ref v2))
                            {
                                change = true;
                                mat.M21 = v2.X;
                                mat.M22 = v2.Y;
                                mat.M23 = v2.Z;
                                mat.M24 = v2.W;
                            }

                            if (ImGui.InputFloat4(key + " Row 3", ref v3))
                            {
                                change = true;
                                mat.M31 = v3.X;
                                mat.M32 = v3.Y;
                                mat.M33 = v3.Z;
                                mat.M34 = v3.W;
                            }

                            if (ImGui.InputFloat4(key + " Row 4", ref v4))
                            {
                                change = true;
                                mat.M41 = v4.X;
                                mat.M42 = v4.Y;
                                mat.M43 = v4.Z;
                                mat.M44 = v4.W;
                            }

                            if (change)
                            {
                                value.SetValue(selectedObject, mat);

                                if (selectedObject is LevelObject)
                                {
                                    ((LevelObject)selectedObject).UpdateTransformMatrix();
                                }

                                UpdateLevelFrame();
                            }
                        }
                        else if (type.IsArray)
                        {
                            if (ImGui.CollapsingHeader(key))
                            {
                                Array array = (Array)val;

                                foreach(object o in array)
                                {
                                    ImGui.Text(Convert.ToString(o));
                                }
                            }
                        }
                        else if (type.IsEnum)
                        {
                            Array values = Enum.GetValues(type);

                            string[] strings = new string[values.Length];

                            for (int i = 0; i < values.Length; i++)
                            {
                                strings[i] = Convert.ToString(values.GetValue(i));
                            }

                            int index = (int)val;

                            if (index < values.Length)
                            {
                                if (ImGui.Combo(key, ref index, strings, values.Length))
                                {
                                    value.SetValue(selectedObject, index);
                                    UpdateLevelFrame();
                                }
                            } else
                            {
                                ImGui.LabelText(key, "[Out of Range] " + Convert.ToString(index));
                            }
                        }
                        else
                        {
                            ImGui.LabelText(key, Convert.ToString(val));
                        }
                    }
                }
            }
        }
    }
}
