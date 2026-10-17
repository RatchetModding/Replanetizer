// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.IO;
using Hexa.NET.ImGui;
using LibReplanetizer.Models;
using LibReplanetizer.Serializers.Importers;
using Replanetizer.Utils;

namespace Replanetizer.Frames
{
    public enum CollisionImportFormat
    {
        Rcc,
        Obj
    }

    public class CollisionImportFrame : LevelSubFrame
    {
        protected override string frameName { get; set; } = "Collision Import";

        private static readonly string[] FORMAT_STRINGS = { "R&C Internal (.rcc)", "Wavefront OBJ (.obj)" };

        private CollisionImportFormat format = CollisionImportFormat.Rcc;
        private string path = string.Empty;

        public CollisionImportFrame(Window wnd, LevelFrame levelFrame)
            : base(wnd, levelFrame)
        {
        }

        public void Open()
        {
            isOpen = true;
        }

        public override void Render(float deltaTime)
        {
            int formatIndex = (int)format;
            if (ImGui.Combo("File Type", ref formatIndex, FORMAT_STRINGS, FORMAT_STRINGS.Length))
            {
                format = (CollisionImportFormat)formatIndex;
                path = string.Empty;
            }

            ImGui.InputText("Path", ref path, 4096);
            ImGui.SameLine();
            if (ImGui.Button("Browse..."))
            {
                string filter = format == CollisionImportFormat.Rcc ? ".rcc" : ".obj";
                string selectedPath = CrossFileDialog.OpenFile("Choose a collision file", filter);
                if (!string.IsNullOrEmpty(selectedPath))
                    path = selectedPath;
            }

            ImGui.Separator();
            bool hasPath = !string.IsNullOrWhiteSpace(path);
            if (!hasPath)
                ImGui.BeginDisabled();

            bool importClicked = ImGui.Button("Import");

            if (!hasPath)
                ImGui.EndDisabled();

            if (importClicked && hasPath && ImportCollision())
                isOpen = false;
        }

        private bool ImportCollision()
        {
            Collision importedCollision;
            try
            {
                if (format == CollisionImportFormat.Rcc)
                {
                    using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                        importedCollision = new Collision(stream, 0, import: true);
                }
                else
                {
                    var geometry = new WavefrontImporter().Import(path, preserveQuads: true);
                    importedCollision = CollisionBuilder.BuildPerFace(geometry);

                    if (importedCollision.indBuff.Length == 0)
                    {
                        const string warning = "The OBJ produced no collision triangles. Ensure it contains faces and that all collision vertex coordinates are non-negative.";
                        levelFrame.QueueCollisionImportWarning(warning);
                        return false;
                    }
                }
            }
            catch (Exception exception)
            {
                string formatName = format == CollisionImportFormat.Rcc ? "RCC" : "OBJ";
                levelFrame.QueueCollisionImportWarning($"{formatName} collision import failed: {exception.Message}");
                return false;
            }

            levelFrame.level.collisionEngine = importedCollision;
            if (levelFrame.level.collisionChunks.Count > 0)
                levelFrame.level.collisionChunks[0] = importedCollision;

            levelFrame.RebuildLevelRenderer();
            levelFrame.InvalidateView();
            return true;
        }
    }
}
