// Copyright (C) 2018-2023, The Replanetizer Contributors.
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
        /// Converts world space vectors from the game's Z Up format into the
        /// specified orientation.
        /// </summary>
        protected static void ChangeOrientation(ref Vector3[] v, ExporterModelSettings.Orientation orientation)
        {
            for (int i = 0; i < v.Length; i++)
            {
                ChangeOrientation(ref v[i], orientation);
            }
        }

        protected static void ChangeOrientation(ref Vector3 v, ExporterModelSettings.Orientation orientation)
        {
            switch (orientation)
            {
                case ExporterModelSettings.Orientation.Y_UP:
                    {
                        float temp = v.Y;
                        v.Y = v.Z;
                        v.Z = -temp;
                        return;
                    }
                case ExporterModelSettings.Orientation.X_UP:
                    {
                        float temp = v.X;
                        v.X = v.Z;
                        v.Z = -temp;
                        return;
                    }
                case ExporterModelSettings.Orientation.Z_UP:
                default:
                    return;
            }
        }

        protected static void ChangeOrientation(ref Quaternion q, ExporterModelSettings.Orientation orientation)
        {
            switch (orientation)
            {
                case ExporterModelSettings.Orientation.Y_UP:
                    {
                        float temp = q.Y;
                        q.Y = q.Z;
                        q.Z = -temp;
                        return;
                    }
                case ExporterModelSettings.Orientation.X_UP:
                    {
                        float temp = q.X;
                        q.X = q.Z;
                        q.Z = -temp;
                        return;
                    }
                case ExporterModelSettings.Orientation.Z_UP:
                default:
                    return;
            }
        }

        /// <summary>
        /// Converts world space transformations from the game's Z Up format into the
        /// specified orientation.
        /// </summary>
        protected static void ChangeOrientation(ref Matrix4[] m, ExporterModelSettings.Orientation orientation)
        {
            for (int i = 0; i < m.Length; i++)
            {
                ChangeOrientation(ref m[i], orientation);
            }
        }

        protected static void ChangeOrientation(ref Matrix4 m, ExporterModelSettings.Orientation orientation)
        {
            switch (orientation)
            {
                case ExporterModelSettings.Orientation.Y_UP:
                    {
                        // Z-UP -> Y-UP:
                        // 1  0  0  0 (X->X)
                        // 0  0  1  0 (Z->Y)
                        // 0 -1  0  0 (Y->-Z)
                        // 0  0  0  1 (W->W)

                        // Y-UP -> Z-UP:
                        // 1  0  0  0 (X->X)
                        // 0  0 -1  0 (Z->-Y)
                        // 0  1  0  0 (Y->Z)
                        // 0  0  0  1 (W->W)

                        Matrix4 ZToY = new Matrix4();
                        ZToY.M11 = 1.0f;
                        ZToY.M23 = 1.0f;
                        ZToY.M32 = -1.0f;
                        ZToY.M44 = 1.0f;

                        Matrix4 YToZ = new Matrix4();
                        YToZ.M11 = 1.0f;
                        YToZ.M23 = -1.0f;
                        YToZ.M32 = 1.0f;
                        YToZ.M44 = 1.0f;

                        m = ZToY * m * YToZ;
                        return;
                    }
                case ExporterModelSettings.Orientation.X_UP:
                    {
                        // Z-UP -> X-UP:
                        // 0  0  1  0 (Z->X)
                        // 0  1  0  0 (Y->Y)
                        //-1  0  0  0 (X->-Z)
                        // 0  0  0  1 (W->W)

                        // X-UP -> Z-UP:
                        // 0  0 -1  0 (X->X)
                        // 0  1  0  0 (Z->-Y)
                        // 1  0  0  0 (Y->Z)
                        // 0  0  0  1 (W->W)

                        Matrix4 ZToX = new Matrix4();
                        ZToX.M13 = 1.0f;
                        ZToX.M22 = 1.0f;
                        ZToX.M31 = -1.0f;
                        ZToX.M44 = 1.0f;

                        Matrix4 XToZ = new Matrix4();
                        XToZ.M13 = -1.0f;
                        XToZ.M22 = 1.0f;
                        XToZ.M31 = 1.0f;
                        XToZ.M44 = 1.0f;

                        m = ZToX * m * XToZ;
                        return;
                    }
                case ExporterModelSettings.Orientation.Z_UP:
                default:
                    return;
            }
        }

        /// <summary>
        /// Export a model to a file.
        /// </summary>
        public abstract void ExportModel(string fileName, Level level, Model model, List<Texture>? textures = null);

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
                case ExporterModelSettings.Format.glTF:
                    return new GLTFExporter(settings);
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
            glTF = 0,
            Wavefront = 1,
            Collada = 2
        };


        public enum AnimationChoice
        {
            None = 0,
            AllSeparate = 1,
            AllSequential = 2,
            All = 3
        };

        public enum Orientation
        {
            Z_UP = 0,
            Y_UP = 1,
            X_UP = 2
        }

        public static readonly string[] FORMAT_STRINGS = { "glTF 2.0 (*.gltf)", "Wavefront (*.obj)", "Collada (*.dae)" };
        public static readonly string[] ANIMATION_CHOICE_STRINGS = { "No Animations", "Separate File for each Animation", "Concatenate Animations", "All Animations" };
        public static readonly string[] ORIENTATION_STRINGS = { "Z Up", "Y Up", "X Up" };

        public ExporterModelSettings.Format format = ExporterModelSettings.Format.glTF;
        public ExporterModelSettings.AnimationChoice animationChoice = ExporterModelSettings.AnimationChoice.None;
        public ExporterModelSettings.Orientation orientation = ExporterModelSettings.Orientation.Z_UP;
        public bool exportMtlFile = true;
        public bool extendedFeatures = false;
        public bool embedTextures = false;

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
