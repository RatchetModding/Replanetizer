using System;
using System.Reflection;
using ImGuiNET;

namespace Replanetizer.Frames
{
    public class AboutFrame : Frame
    {
        private string aboutText;
        protected override string frameName { get; set; } = "About Replanetizer";
        
        public AboutFrame(Window wnd) : base(wnd)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var versionAttr = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            string gitTreeStatus = versionAttr != null ? versionAttr.InformationalVersion : "unknown";
            aboutText = String.Format(@"Replanetizer, a Level Editor for Ratchet & Clank games on the PS3.
Copyright (C) 2018 - 2021, The Replanetizer Contributors.

Replanetizer is free software, licensed under the GNU GPL 3.0.
Please see the LICENSE.md file for more details, or visit 
https://github.com/RatchetModding/Replanetizer/blob/master/LICENSE.md.


Replanetizer version: '{0}'. 
Graphics backend: '{1}'.

Found a bug? Want to help? Please visit our project page: 
https://github.com/RatchetModding/replanetizer

Replanetizer includes icons from the materials icon pack located at 
https://material.io/tools/icons/?style=baseline, 
which is protected under the apache 2.0 license availible at 
https://www.apache.org/licenses/LICENSE-2.0.html

", gitTreeStatus, wnd.OpenGLString);
        }

        public override void Render(float deltaTime)
        {
            ImGui.Text(aboutText);
        }
    }
}