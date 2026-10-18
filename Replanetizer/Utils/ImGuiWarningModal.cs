using System;
using System.Numerics;
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

            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowSize(new Vector2(MathF.Min(560f, viewport.Size.X * 0.8f), 0f), ImGuiCond.Appearing);

            if (!ImGui.BeginPopupModal(popupId, ref isOpen, ImGuiWindowFlags.None | ImGuiWindowFlags.NoDocking))
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
