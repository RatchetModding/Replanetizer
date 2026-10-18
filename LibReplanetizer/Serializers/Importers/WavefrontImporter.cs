// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK.Mathematics;

namespace LibReplanetizer.Serializers.Importers
{
    public class Face
    {
        public (int pos, int uv, int norm)[] corners = new (int, int, int)[3];
        public int materialIndex = -1;
    }
    public class ParsedModelGeometry
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector3> colors = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<string> materialNames = new List<string>();
        public List<Face> faces = new List<Face>();
        public Vector3 BoundsMin { get; private set; }
        public Vector3 BoundsMax { get; private set; }
        public bool HasBounds { get; private set; }

        internal void AddVertex(Vector3 vertex)
        {
            vertices.Add(vertex);
            if (!HasBounds)
            {
                BoundsMin = vertex;
                BoundsMax = vertex;
                HasBounds = true;
                return;
            }

            BoundsMin = new Vector3(
                Math.Min(BoundsMin.X, vertex.X),
                Math.Min(BoundsMin.Y, vertex.Y),
                Math.Min(BoundsMin.Z, vertex.Z));
            BoundsMax = new Vector3(
                Math.Max(BoundsMax.X, vertex.X),
                Math.Max(BoundsMax.Y, vertex.Y),
                Math.Max(BoundsMax.Z, vertex.Z));
        }

        internal string? GetMaterialName(int materialIndex)
        {
            Utilities.DebugAssert(materialIndex == -1 || (materialIndex >= 0 && materialIndex < materialNames.Count),
                $"Face material index {materialIndex} is outside the material-name table.");
            return materialIndex == -1 ? null : materialNames[materialIndex];
        }
    }
    public class WavefrontImporter
    {
        public ParsedModelGeometry Import(string path)
        {
            return Import(path, preserveQuads: false);
        }

        public ParsedModelGeometry Import(string path, bool preserveQuads)
        {
            ParsedModelGeometry parsed = new ParsedModelGeometry();
            var materialIndices = new Dictionary<string, int>(StringComparer.Ordinal);

            string? line;
            using StreamReader file = new StreamReader(path);
            int currentMaterialIndex = -1;

            while ((line = file.ReadLine()) != null)
            {
                int commentStart = line.IndexOf('#');
                if (commentStart >= 0)
                    line = line.Substring(0, commentStart);

                string[] split = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 0)
                    continue;

                switch (split[0])
                {
                    case "v":
                        if (split.Length < 4)
                            throw new FormatException("OBJ positions must contain three coordinates.");
                        parsed.AddVertex(new Vector3(ParseFloat(split[1]), ParseFloat(split[2]), ParseFloat(split[3])));

                        if (split.Length >= 7)
                        {
                            parsed.colors.Add(new Vector3(ParseFloat(split[4]), ParseFloat(split[5]), ParseFloat(split[6])));
                        }
                        else
                        {
                            parsed.colors.Add(new Vector3(0.5f, 0.5f, 0.5f));
                        }
                        break;

                    case "vn":
                        // TODO: Should have support for converting Y/Z up direction
                        if (split.Length < 4)
                            throw new FormatException("OBJ normals must contain three coordinates.");
                        parsed.normals.Add(new Vector3(ParseFloat(split[1]), ParseFloat(split[2]), ParseFloat(split[3])));
                        break;

                    case "vt":
                        if (split.Length < 3)
                            throw new FormatException("OBJ texture coordinates must contain two coordinates.");
                        parsed.uvs.Add(new Vector2(ParseFloat(split[1]), ParseFloat(split[2])));
                        break;

                    case "usemtl":
                        if (split.Length < 2)
                            throw new FormatException("A usemtl directive must include a material name.");
                        string materialName = split[1];
                        if (!materialIndices.TryGetValue(materialName, out currentMaterialIndex))
                        {
                            currentMaterialIndex = parsed.materialNames.Count;
                            materialIndices.Add(materialName, currentMaterialIndex);
                            parsed.materialNames.Add(materialName);
                        }
                        break;

                    case "f":
                        int cornerCount = split.Length - 1;
                        if (cornerCount < 3)
                            throw new FormatException("Wavefront faces must contain at least three corners.");
                        if (cornerCount > 4)
                            throw new FormatException("Wavefront faces cannot contain more than four corners.");

                        var corners = new (int pos, int uv, int norm)[cornerCount];
                        for (int i = 0; i < cornerCount; i++)
                        {
                            string[] parts = split[i + 1].Split('/');
                            if (parts.Length is < 1 or > 3 || parts[0].Length == 0)
                                throw new FormatException("Wavefront face corners must contain a position and optional UV and normal indices.");

                            int uv = parts.Length > 1 && parts[1].Length > 0
                                ? ParseIndex(parts[1], "UV", parsed.uvs.Count)
                                : -1;
                            int normal = parts.Length > 2 && parts[2].Length > 0
                                ? ParseIndex(parts[2], "normal", parsed.normals.Count)
                                : -1;

                            corners[i] = (
                                ParseIndex(parts[0], "position", parsed.vertices.Count),
                                uv,
                                normal
                            );
                        }

                        if (preserveQuads && cornerCount == 4)
                        {
                            parsed.faces.Add(new Face
                            {
                                corners = corners,
                                materialIndex = currentMaterialIndex
                            });
                            break;
                        }

                        Face firstFace = new Face();
                        firstFace.materialIndex = currentMaterialIndex;
                        firstFace.corners[0] = corners[0];
                        firstFace.corners[1] = corners[1];
                        firstFace.corners[2] = corners[2];
                        parsed.faces.Add(firstFace);

                        if (cornerCount == 4)
                        {
                            Face secondFace = new Face();
                            secondFace.materialIndex = currentMaterialIndex;
                            secondFace.corners[0] = corners[0];
                            secondFace.corners[1] = corners[2];
                            secondFace.corners[2] = corners[3];
                            parsed.faces.Add(secondFace);
                        }

                        break;

                }
            }

            return parsed;
        }

        private static float ParseFloat(string value)
        {
            bool parsed = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result);
            if (!parsed || !float.IsFinite(result))
                throw new FormatException($"Invalid OBJ floating-point value '{value}'.");
            return result;
        }

        private static int ParseIndex(string value, string indexName, int itemCount)
        {
            bool parsed = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index);
            if (!parsed || index == 0)
                throw new FormatException($"Wavefront {indexName} indices must be non-zero integers.");

            int resolvedIndex = index > 0 ? index - 1 : itemCount + index;
            if (resolvedIndex < 0 || resolvedIndex >= itemCount)
                throw new FormatException($"Wavefront {indexName} index {index} is outside the available range.");
            return resolvedIndex;
        }
    }
}
