using ImGuiNET;

namespace Replanetizer.Frames
{
    public class DemoWindowFrame : Frame
    {
        protected override string frameName { get; set; }

        public DemoWindowFrame(Window wnd) : base(wnd)
        {
        }

        public override void Render(float deltaTime)
        {
            ImGui.ShowDemoWindow(ref isOpen);
        }

        public override void RenderAsWindow(float deltaTime)
        {
            ImGui.ShowDemoWindow(ref isOpen);
        }
    }
}