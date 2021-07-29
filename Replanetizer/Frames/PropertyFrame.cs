using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks.Dataflow;
using ImGuiNET;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;

namespace Replanetizer.Frames
{
    public class PropertyFrame : LevelSubFrame
    {
        protected override string frameName { get; set; } = "Properties";
        private Level level => levelFrame.level;
        private bool followObject = true;

        private LevelObject selectedObject;

        private Dictionary<string, Dictionary<string, PropertyInfo>> properties;
        
        public PropertyFrame(Window wnd, LevelFrame levelFrame, bool followObject = true, LevelObject selectedObject = null) : base(wnd, levelFrame)
        {
            this.followObject = followObject;
            if (followObject) levelFrame.RegisterCallback(this.SelectionCallback);

            if (selectedObject == null) this.selectedObject = levelFrame.selectedObject;
            else this.selectedObject = selectedObject;
            RecomputeProperties();
        }

        private class PropertyDetails
        {
            public Type Type;
            public object Object;
            
            public PropertyDetails(Type type, object _object)
            {
                Type = type;
                Object = _object;
            }
        }
        
        private void SelectionCallback(LevelObject o)
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
                if (category == null) continue;
                
                string displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                if (displayName == null) continue;
                
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
