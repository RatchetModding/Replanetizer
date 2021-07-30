using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using ImGuiNET;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;

namespace Replanetizer.Frames
{
    public class PropertyFrame : LevelSubFrame
    {
        protected sealed override string frameName { get; set; } = "Properties";
        private Level level => levelFrame.level;

        private object selectedObject;

        private Dictionary<string, Dictionary<string, PropertyInfo>> properties;
        
        public PropertyFrame(Window wnd, LevelFrame levelFrame, bool followObject = true, object selectedObject = null, string overrideFrameName = null) : base(wnd, levelFrame)
        {
            if (overrideFrameName != null && overrideFrameName.Length > 0)
            {
                frameName = overrideFrameName;
            }
            
            if (followObject) levelFrame.RegisterCallback(this.SelectionCallback);

            if (selectedObject == null) this.selectedObject = levelFrame.selectedObject;
            else this.selectedObject = selectedObject;
            RecomputeProperties();
        }

        private void SelectionCallback(object o)
        {
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

        public override void Render(float deltaTime)
        {
            if (ImGui.Begin(frameName, ref isOpen, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                if (properties == null)
                {
                    ImGui.Text("Select an object");
                    ImGui.End();
                    return;
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

                ImGui.End();
            }
        }
    }
}
