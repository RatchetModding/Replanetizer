using Hexa.NET.ImGui;

namespace Replanetizer.Utils
{
    public sealed class ImGuiWarningModal
    {
        private readonly string popupId;
        private string message = string.Empty;
        private bool isPending;
        private bool isOpen;

        public ImGuiWarningModal(string popupId)
        {
            this.popupId = popupId;
        }

        public void Queue(string message)
        {
            this.message = message;
            isPending = true;
            isOpen = true;
        }

        public void Render()
        {
            if (isPending)
            {
                ImGui.OpenPopup(popupId);
                isPending = false;
            }

            if (!ImGui.BeginPopupModal(popupId, ref isOpen, ImGuiWindowFlags.AlwaysAutoResize))
                return;

            ImGui.TextWrapped(message);
            ImGui.Spacing();
            if (ImGui.Button("OK") || ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                isOpen = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
}
