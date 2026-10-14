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

        private CollisionImportFormat format;
        private string path = string.Empty;

        public CollisionImportFrame(Window wnd, LevelFrame levelFrame, CollisionImportFormat format)
            : base(wnd, levelFrame)
        {
            this.format = format;
        }

        public void SetFormat(CollisionImportFormat format)
        {
            if (this.format != format)
                path = string.Empty;

            this.format = format;
            isOpen = true;
        }

        public override void Render(float deltaTime)
        {
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
            if (format == CollisionImportFormat.Rcc)
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                    importedCollision = new Collision(stream, 0, import: true);
            }
            else
            {
                var geometry = new WavefrontImporter().Import(path);
                try
                {
                    importedCollision = CollisionBuilder.BuildPerFace(geometry);
                }
                catch (InvalidOperationException exception)
                {
                    levelFrame.QueueCollisionImportWarning($"OBJ collision import failed: {exception.Message}");
                    return false;
                }

                if (importedCollision.indBuff.Length == 0)
                {
                    const string warning = "The OBJ produced no collision triangles. Ensure it contains faces and that all collision vertex coordinates are non-negative.";
                    levelFrame.QueueCollisionImportWarning(warning);
                    return false;
                }
            }

            MergeImportedCollision(importedCollision);
            levelFrame.InvalidateView();
            return true;
        }

        private void MergeImportedCollision(Collision importedCollision)
        {
            Collision mergedEngineCollision = Collision.Concatenate(levelFrame.level.collisionEngine, importedCollision);
            Collision? mergedFirstChunk = levelFrame.level.collisionChunks.Count > 0
                ? Collision.Concatenate(levelFrame.level.collisionChunks[0], importedCollision)
                : null;

            levelFrame.level.collisionEngine = mergedEngineCollision;
            if (mergedFirstChunk != null)
                levelFrame.level.collisionChunks[0] = mergedFirstChunk;

            levelFrame.RebuildLevelRenderer();
        }
    }
}
