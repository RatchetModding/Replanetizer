// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using ImGuiNET;
using LibReplanetizer.LevelObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Replanetizer.Frames
{
    public class LightsFrame : Frame
    {
        protected sealed override string frameName { get; set; } = "Lights";
        private List<Light> lights;
        private LightConfig lightConfig;
        private LevelFrame levelFrame;

        private Dictionary<string, Dictionary<string, PropertyInfo>> configProperties;
        private List<Tuple<Light, Dictionary<string, Dictionary<string, PropertyInfo>>>> lightsProperties;

        public LightsFrame(Window wnd, LevelFrame levelFrame, List<Light> lights, LightConfig lightConfig) : base(wnd)
        {
            this.levelFrame = levelFrame;
            this.lights = lights;
            this.lightConfig = lightConfig;

            lightsProperties = new List<Tuple<Light, Dictionary<string, Dictionary<string, PropertyInfo>>>>();

            RecomputeProperties();
        }

        private Dictionary<string, Dictionary<string, PropertyInfo>> RecomputeSinglePropertiesSet(object o)
        {
            Dictionary<string, Dictionary<string, PropertyInfo>> properties = new Dictionary<string, Dictionary<string, PropertyInfo>>();
            var objProps = o.GetType().GetProperties();
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

            return properties;
        }

        private void RecomputeProperties()
        {
            configProperties = RecomputeSinglePropertiesSet(lightConfig);

            foreach (Light light in lights)
            {
                lightsProperties.Add(new Tuple<Light, Dictionary<string, Dictionary<string, PropertyInfo>>>(light, RecomputeSinglePropertiesSet(light)));
            }
        }

        private void UpdateLevelFrame()
        {
            if (levelFrame != null) levelFrame.InvalidateView();
        }

        public override void Render(float deltaTime)
        {
            if (ImGui.CollapsingHeader("Light Config"))
            {
                foreach (var categoryPair in configProperties)
                {
                    foreach (var (key, value) in categoryPair.Value)
                    {
                        float v = (float) value.GetValue(lightConfig);
                        if (ImGui.InputFloat(key, ref v))
                        {
                            value.SetValue(lightConfig, v);
                            UpdateLevelFrame();
                        }
                    }
                }
            }

            if (ImGui.CollapsingHeader("Lights"))
            {
                for (int i = 0; i < lightsProperties.Count; i++)
                {
                    var t = lightsProperties[i];

                    if (ImGui.TreeNode("Light " + i))
                    {
                        foreach (var categoryPair in t.Item2)
                        {
                            var value1 = categoryPair.Value["Color 1"];
                            OpenTK.Mathematics.Vector4 v1 = (OpenTK.Mathematics.Vector4) value1.GetValue(t.Item1);

                            Vector3 color1 = new Vector3(v1.X, v1.Y, v1.Z);

                            if (ImGui.ColorEdit3("Color 1", ref color1))
                            {
                                v1.X = color1.X;
                                v1.Y = color1.Y;
                                v1.Z = color1.Z;

                                value1.SetValue(t.Item1, v1);
                                UpdateLevelFrame();
                            }

                            var value2 = categoryPair.Value["Direction 1"];
                            OpenTK.Mathematics.Vector4 v2 = (OpenTK.Mathematics.Vector4) value2.GetValue(t.Item1);

                            Vector3 dir1 = new Vector3(v2.X, v2.Y, v2.Z);

                            if (ImGui.SliderFloat3("Direction 1", ref dir1, -1.0f, 1.0f))
                            {
                                v2.X = dir1.X;
                                v2.Y = dir1.Y;
                                v2.Z = dir1.Z;

                                value2.SetValue(t.Item1, v2);
                                UpdateLevelFrame();
                            }

                            var value3 = categoryPair.Value["Color 2"];
                            OpenTK.Mathematics.Vector4 v3 = (OpenTK.Mathematics.Vector4) value3.GetValue(t.Item1);

                            Vector3 color2 = new Vector3(v3.X, v3.Y, v3.Z);

                            if (ImGui.ColorEdit3("Color 2", ref color2))
                            {
                                v3.X = color2.X;
                                v3.Y = color2.Y;
                                v3.Z = color2.Z;

                                value3.SetValue(t.Item1, v3);
                                UpdateLevelFrame();
                            }

                            var value4 = categoryPair.Value["Direction 2"];
                            OpenTK.Mathematics.Vector4 v4 = (OpenTK.Mathematics.Vector4) value4.GetValue(t.Item1);

                            Vector3 dir2 = new Vector3(v4.X, v4.Y, v4.Z);

                            if (ImGui.SliderFloat3("Direction 2", ref dir2, -1.0f, 1.0f))
                            {
                                v4.X = dir2.X;
                                v4.Y = dir2.Y;
                                v4.Z = dir2.Z;

                                value4.SetValue(t.Item1, v4);
                                UpdateLevelFrame();
                            }
                        }

                        ImGui.TreePop();
                    }
                }
            }
        }
    }
}
