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
                        Utilities.DebugAssert(split.Length >= 4, "OBJ positions must contain three coordinates.");
                        parsed.vertices.Add(new Vector3(ParseFloat(split[1]), ParseFloat(split[2]), ParseFloat(split[3])));

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
                        Utilities.DebugAssert(split.Length >= 4, "OBJ normals must contain three coordinates.");
                        parsed.normals.Add(new Vector3(ParseFloat(split[1]), ParseFloat(split[2]), ParseFloat(split[3])));
                        break;

                    case "vt":
                        Utilities.DebugAssert(split.Length >= 3, "OBJ texture coordinates must contain two coordinates.");
                        parsed.uvs.Add(new Vector2(ParseFloat(split[1]), ParseFloat(split[2])));
                        break;

                    case "usemtl":
                        Utilities.DebugAssert(split.Length >= 2, "A usemtl directive must include a material name.");
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
                        Utilities.DebugAssert(cornerCount >= 3 && cornerCount <= 4,
                            "Wavefront faces must contain three or four corners.");
                        if (cornerCount < 3)
                            continue;

                        int parsedCornerCount = cornerCount > 4 ? 3 : cornerCount;
                        var corners = new (int pos, int uv, int norm)[parsedCornerCount];
                        for (int i = 0; i < parsedCornerCount; i++)
                        {
                            string[] parts = split[i + 1].Split('/');
                            Utilities.DebugAssert(parts.Length > 0 && parts[0].Length > 0,
                                "Wavefront face corner is missing a position index.");

                            int uv = parts.Length > 1 && parts[1].Length > 0 ? ParseIndex(parts[1], "UV") : -1;
                            int normal = parts.Length > 2 && parts[2].Length > 0 ? ParseIndex(parts[2], "normal") : -1;

                            corners[i] = (
                                ParseIndex(parts[0], "position"),
                                uv,
                                normal
                            );
                        }

                        for (int triangleIndex = 0; triangleIndex < parsedCornerCount - 2; triangleIndex++)
                        {
                            Face face = new Face();
                            face.materialIndex = currentMaterialIndex;
                            face.corners[0] = corners[0];
                            face.corners[1] = corners[triangleIndex + 1];
                            face.corners[2] = corners[triangleIndex + 2];
                            parsed.faces.Add(face);
                        }

                        break;

                }
            }

            return parsed;
        }

        private static float ParseFloat(string value)
        {
            bool parsed = float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float result);
            Utilities.DebugAssert(parsed, $"Invalid OBJ floating-point value '{value}'.");
            return result;
        }

        private static int ParseIndex(string value, string indexName)
        {
            bool parsed = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int oneBasedIndex);
            Utilities.DebugAssert(parsed && oneBasedIndex > 0, $"Wavefront {indexName} indices must be positive 1-based integers.");
            return oneBasedIndex - 1;
        }
    }
}
