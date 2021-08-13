using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using ImGuiNET;


namespace Replanetizer.Frames
{
    public class PropertyFrame : Frame
    {
        protected sealed override string frameName { get; set; } = "Properties";
        private object selectedObject;
        private bool listenToCallbacks;
        private bool hideCallbackButton = false;

        private Dictionary<string, Dictionary<string, PropertyInfo>> properties;
        
        public PropertyFrame(Window wnd, object selectedObject = null, string overrideFrameName = null, bool listenToCallbacks = false, bool hideCallbackButton = false) : base(wnd)
        {
            if (overrideFrameName != null && overrideFrameName.Length > 0)
            {
                frameName = overrideFrameName;
            }
            
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
                            }
                        }
                        else if (type == typeof(int))
                        {
                            var v = (int) val;
                            if (ImGui.InputInt(key, ref v))
                            {
                                value.SetValue(selectedObject, v);
                            }
                        }
                        else if (type == typeof(float))
                        {
                            var v = (float) val;
                            if (ImGui.InputFloat(key, ref v))
                            {
                                value.SetValue(selectedObject, v);
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
