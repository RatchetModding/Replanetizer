// Copyright (C) 2018-2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hexa.NET.ImGui;
using LibReplanetizer.LevelObjects;
using static LibReplanetizer.DataFunctions;

namespace Replanetizer.Frames
{
    public class PvarInspectorFrame : Frame
    {
        protected sealed override string frameName { get; set; } = "pVars";
        public LevelFrame? levelFrame { get; set; }
        public PvarInspectorFrame(Window wnd) : base(wnd) { }

        public Moby? moby { get; set; }

        private static readonly Dictionary<string, (int Size, Func<byte[], int, object> Read)> Types = new()
        {
            ["char"] = (1, (data, offset) => data[offset]),

            ["short"] = (2, (data, offset) => ReadShort(data, offset)),
            ["ushort"] = (2, (data, offset) => ReadUshort(data, offset)),

            ["int"] = (4, (data, offset) => ReadInt(data, offset)),
            ["uint"] = (4, (data, offset) => ReadUint(data, offset)),

            ["float"] = (4, (data, offset) => ReadFloat(data, offset)),
            ["cuboid"] = (4, (data, offset) => ReadInt(data, offset)),
        };
        private List<(string name, string type, int offset)>? Load(int id)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "PvarConfigs", "moby" + id + ".h");
            if (!File.Exists(path))
                return null;

            int offset = 0;

            Regex regex = new(@"(\w+)(?:\s+int)?\s+(\w+)\s*;");

            return regex.Matches(File.ReadAllText(path))
                .Select(m => {
                    string type = m.Groups[1].Value;
                    string name = m.Groups[2].Value;

                    var field = (name, type, offset);
                    offset += Types[type].Size;

                    return field;
                }).ToList();
        }

        private void RenderField(byte[] data, string name, string type, int offset)
        {
            object value = Types[type].Read(data, offset);
            ImGui.LabelText(name, value.ToString());

            // Show select button next to the label if it's a valid cuboid
            // Could probably add this for other types. But for now we have just cuboids
            if (type == "cuboid")
            {
                int index = (int)value;

                List<Cuboid>? cuboids = levelFrame?.level.cuboids;
                if (index >= 0 && index < cuboids?.Count)
                {
                    ImGui.SameLine();

                    if (ImGui.Button("Select"))
                        levelFrame?.HandleSelect(cuboids[index], true, true);
                }
            }
        }

        public override void Render(float deltaTime)
        {
            if (moby == null || moby.pVars.Length == 0)
                return;

            // Check if corresponding moby[n].h file exists
            var fields = Load(moby.modelID);
            if (fields == null)
                return;

            foreach (var (name, type, offset) in fields)
            {
                ImGui.PushID(offset);
                RenderField(moby.pVars, name, type, offset);
                ImGui.PopID();
            }

        }
    }
}
