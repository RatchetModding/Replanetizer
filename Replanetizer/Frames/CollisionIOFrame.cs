// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.IO;
using System.Collections.Generic;
using Hexa.NET.ImGui;
using LibReplanetizer;
using LibReplanetizer.Models;
using LibReplanetizer.Serializers.Importers;
using Replanetizer.Utils;

namespace Replanetizer.Frames
{
    public enum CollisionImportFormat
    {
        Obj,
        Rcc
    }

    public class CollisionIOFrame : LevelSubFrame
    {
        protected override string frameName { get; set; } = "Collision Import/Export";

        private static readonly string[] FORMAT_STRINGS = { "Wavefront OBJ (.obj)", "R&C Internal (.rcc)" };

        private CollisionImportFormat format = CollisionImportFormat.Obj;
        private string path = string.Empty;
        private int selectedChunkIndex;

        public CollisionIOFrame(Window wnd, LevelFrame levelFrame)
            : base(wnd, levelFrame)
        {
        }

        public void Open()
        {
            isOpen = true;
        }

        public override void Render(float deltaTime)
        {
            int formatIndex = (int) format;
            if (ImGui.Combo("File Type", ref formatIndex, FORMAT_STRINGS, FORMAT_STRINGS.Length))
            {
                format = (CollisionImportFormat) formatIndex;
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

            int chunkCount = levelFrame.level.collisionChunks.Count;
            if (chunkCount > 0)
            {
                if (selectedChunkIndex < 0 || selectedChunkIndex >= chunkCount)
                    selectedChunkIndex = 0;

                string selectedTarget = $"Chunk {selectedChunkIndex}";
                if (ImGui.BeginCombo("Target Collision", selectedTarget))
                {
                    for (int i = 0; i < chunkCount; i++)
                    {
                        bool selected = selectedChunkIndex == i;
                        if (ImGui.Selectable($"Chunk {i}", selected))
                            selectedChunkIndex = i;

                        if (selected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
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

            ImGui.SameLine();
            if (ImGui.Button("Export"))
                ExportCollision();
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

            SetSelectedCollision(importedCollision);

            levelFrame.RebuildLevelRenderer();
            levelFrame.InvalidateView();
            return true;
        }

        private void ExportCollision()
        {
            string extension = format == CollisionImportFormat.Rcc ? ".rcc" : ".obj";
            try
            {
                string exportPath = CrossFileDialog.SaveFile("Export collision", extension);
                if (string.IsNullOrEmpty(exportPath))
                    return;

                string actualExtension = Path.GetExtension(exportPath);
                if (!string.Equals(actualExtension, extension, StringComparison.OrdinalIgnoreCase))
                    exportPath = Path.ChangeExtension(exportPath, extension);

                Collision collision = ResolveCollisionTarget(
                    levelFrame.level.collisionEngine,
                    levelFrame.level.collisionChunks,
                    selectedChunkIndex);
                if (format == CollisionImportFormat.Rcc)
                {
                    File.WriteAllBytes(exportPath, collision.Serialize());
                }
                else
                {
                    new WavefrontExporter().ExportCollision(exportPath, collision);
                }
            }
            catch (Exception exception)
            {
                string formatName = format == CollisionImportFormat.Rcc ? "RCC" : "OBJ";
                levelFrame.QueueCollisionImportWarning($"{formatName} collision export failed: {exception.Message}");
            }
        }

        internal static Collision ResolveCollisionTarget(
            Collision engineCollision,
            IList<Collision> collisionChunks,
            int selectedChunkIndex)
        {
            if (collisionChunks.Count == 0)
                return engineCollision;

            if (selectedChunkIndex < 0 || selectedChunkIndex >= collisionChunks.Count)
                throw new InvalidOperationException("The selected collision chunk no longer exists.");

            return collisionChunks[selectedChunkIndex];
        }

        internal static Collision SetCollisionTarget(
            Collision engineCollision,
            IList<Collision> collisionChunks,
            int selectedChunkIndex,
            Collision importedCollision)
        {
            if (collisionChunks.Count == 0)
                return importedCollision;

            if (selectedChunkIndex < 0 || selectedChunkIndex >= collisionChunks.Count)
                throw new InvalidOperationException("The selected collision chunk no longer exists.");

            collisionChunks[selectedChunkIndex] = importedCollision;
            return selectedChunkIndex == 0 ? importedCollision : engineCollision;
        }

        private void SetSelectedCollision(Collision collision)
        {
            levelFrame.level.collisionEngine = SetCollisionTarget(
                levelFrame.level.collisionEngine,
                levelFrame.level.collisionChunks,
                selectedChunkIndex,
                collision);
        }
    }
}
