// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Globalization;

namespace LibReplanetizer
{
    public abstract class Exporter
    {
        protected static readonly CultureInfo en_US = CultureInfo.CreateSpecificCulture("en-US");
        protected static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Average a tri's vertex normals to get the target face normal, then
        /// check whether the current winding order yields a normal facing
        /// the target face normal
        /// </summary>
        /// <returns>whether to reverse the winding order</returns>
        protected static bool ShouldReverseWinding(
            IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3>? vertexNormals, int v1, int v2, int v3)
        {
            if (vertexNormals == null) return false;

            var targetFaceNormal = FaceNormalFromVertexNormals(vertexNormals, v1, v2, v3);
            var p1 = vertices[v1];
            var p2 = vertices[v2];
            var p3 = vertices[v3];
            return ShouldReverseWinding(p1, p2, p3, targetFaceNormal);
        }

        protected static bool ShouldReverseWinding(
            Vector3 p1, Vector3 p2, Vector3 p3, Vector3 targetFaceNormal)
        {
            p2 -= p1;
            p3 -= p1;
            var faceNormal = Vector3.Cross(p2, p3);
            var dot = Vector3.Dot(faceNormal, targetFaceNormal);
            return dot < 0f;
        }

        protected static Vector3 FaceNormalFromVertexNormals(
            IReadOnlyList<Vector3> normals, int v1, int v2, int v3)
        {
            var n1 = normals[v1];
            var n2 = normals[v2];
            var n3 = normals[v3];
            return (n1 + n2 + n3) / 3;
        }

        /// <summary>
        /// Export a model to a file.
        /// </summary>
        public abstract void ExportModel(string fileName, Level level, Model model);

        /// <summary>
        /// Returns the default exporter.
        /// </summary>
        public static Exporter? GetExporter()
        {
            return new ColladaExporter();
        }

        /// <summary>
        /// Returns an exporter for model exporting based on the settings.
        /// </summary>
        public static Exporter? GetExporter(ExporterModelSettings settings)
        {
            switch (settings.format)
            {
                case ExporterModelSettings.Format.Wavefront:
                    return new WavefrontExporter(settings);
                case ExporterModelSettings.Format.Collada:
                    return new ColladaExporter(settings);
            }

            return null;
        }

        /// <summary>
        /// Returns the default exporter for level exporting.
        /// </summary>
        public static Exporter? GetExporter(ExporterLevelSettings settings)
        {
            return new WavefrontExporter(settings);
        }

        /// <summary>
        /// Get the file ending associated with the respective exporter.
        /// </summary>
        public abstract string GetFileEnding();
    }


    public class ExporterLevelSettings
    {

        /*
         * Different Modes for Level Export:
         * - Separate: Exports every ModelObject as is
         * - Combined: Combines all ModelObjects into one mesh
         * - Typewise: Combines all ModelObjects of the same type (Tie, Shrubs etc.) into one mesh
         * - Materialwise: Combines all faces using the same material/texture into one mesh
         */
        public enum Mode
        {
            Separate = 0,
            Combined = 1,
            Typewise = 2,
            Materialwise = 3
        };

        public static readonly string[] MODE_STRINGS = { "Separate", "Combined", "Typewise", "Materialwise" };

        public Mode mode = Mode.Combined;
        public bool writeColors = false;
        public bool writeTies = true;
        public bool writeShrubs = true;
        public bool writeMobies = true;
        public bool[] chunksSelected = new bool[5] { true, true, true, true, true };
        public bool exportMtlFile = true;

        public ExporterLevelSettings()
        {
        }
    }

    public class ExporterModelSettings
    {
        public enum Format
        {
            Wavefront = 0,
            Collada = 1
        };


        public enum AnimationChoice
        {
            None = 0,
            All = 1,
            AllSeparate = 2,
            AllSequential = 3
        };

        public static readonly string[] FORMAT_STRINGS = { "Wavefront (*.obj)", "Collada (*.dae)" };
        public static readonly string[] ANIMATION_CHOICE_STRINGS = { "No Animations", "All Animations", "Separate File for each Animation", "Concatenate Animations" };

        public ExporterModelSettings.Format format = ExporterModelSettings.Format.Collada;
        public ExporterModelSettings.AnimationChoice animationChoice = ExporterModelSettings.AnimationChoice.None;
        public bool exportMtlFile = true;
        public bool extendedFeatures = false;

        public ExporterModelSettings()
        {
        }

        public ExporterModelSettings(ExporterModelSettings startValues)
        {
            format = startValues.format;
            animationChoice = startValues.animationChoice;
            exportMtlFile = startValues.exportMtlFile;
        }
    }
}
