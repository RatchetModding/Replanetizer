// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;

namespace LibReplanetizer.Serializers.Importers
{
    public class TerrainBuilder
    {
        private const float FragmentCellSize = 32.0f;
        private const int MaxFacesPerFragment = 83;

        private readonly struct TerrainMaterial
        {
            public int textureId { get; }
            public TextureConfig.WrapMode wrapModeS { get; }
            public TextureConfig.WrapMode wrapModeT { get; }

            public TerrainMaterial(int textureId, TextureConfig.WrapMode wrapModeS, TextureConfig.WrapMode wrapModeT)
            {
                this.textureId = textureId;
                this.wrapModeS = wrapModeS;
                this.wrapModeT = wrapModeT;
            }
        }

        public static int ParseTextureId(string materialName)
        {
            if (materialName.StartsWith("RC_", StringComparison.Ordinal))
                return ParseStructuredTextureMaterial(materialName).textureId;

            string[] parts = materialName.Split('_');
            int textureId = 0;
            bool parsed = parts.Length == 2 && string.Equals(parts[0], "mtl", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out textureId);
            if (!parsed)
                throw new FormatException($"Terrain material name '{materialName}' must be mtl_<textureID> or a supported RC_ material name.");
            return textureId;
        }

        private static TerrainMaterial ParseStructuredTextureMaterial(string materialName)
        {
            string[] parts = materialName.Split('_');
            if (parts.Length != 2 && parts.Length != 4)
                throw new FormatException("Terrain material names must be RC_<textureID> or RC_<textureID>_<wrapModeS>_<wrapModeT>.");
            bool parsed = int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int textureId);
            if (!parsed)
                throw new FormatException("Terrain material texture IDs must be unsigned decimal integers.");

            TextureConfig.WrapMode wrapModeS = TextureConfig.WrapMode.Repeat;
            TextureConfig.WrapMode wrapModeT = TextureConfig.WrapMode.Repeat;
            if (parts.Length == 4)
            {
                wrapModeS = ParseWrapMode(parts[2]);
                wrapModeT = ParseWrapMode(parts[3]);
            }

            return new TerrainMaterial(textureId, wrapModeS, wrapModeT);
        }

        private static TextureConfig.WrapMode ParseWrapMode(string value)
        {
            bool repeat = string.Equals(value, "Repeat", StringComparison.OrdinalIgnoreCase) || value == "0";
            bool clampEdge = string.Equals(value, "ClampEdge", StringComparison.OrdinalIgnoreCase) || value == "1";
            if (!repeat && !clampEdge)
                throw new FormatException($"Unknown texture wrap mode '{value}'. Expected Repeat, ClampEdge, 0, or 1.");
            return clampEdge ? TextureConfig.WrapMode.ClampEdge : TextureConfig.WrapMode.Repeat;
        }

        private static TerrainMaterial GetMaterial(ParsedModelGeometry geometry, Face face)
        {
            string? materialName = geometry.GetMaterialName(face.materialIndex);
            if (materialName == null)
                return new TerrainMaterial(0, TextureConfig.WrapMode.Repeat, TextureConfig.WrapMode.Repeat);

            if (!materialName.StartsWith("RC_", StringComparison.Ordinal))
                return new TerrainMaterial(ParseTextureId(materialName), TextureConfig.WrapMode.Repeat, TextureConfig.WrapMode.Repeat);

            return ParseStructuredTextureMaterial(materialName);
        }

        public static Terrain Build(ParsedModelGeometry geometry, ushort levelNumber)
        {
            var cellFaces = new Dictionary<(int x, int y, int z), List<Face>>();
            foreach (var face in geometry.faces)
            {
                Vector3 a, b, c;
                a = geometry.vertices[face.corners[0].pos];
                b = geometry.vertices[face.corners[1].pos];
                c = geometry.vertices[face.corners[2].pos];

                Vector3 centroid = (a + b + c) / 3.0f;

                int x, y, z;
                x = (int) Math.Floor(centroid.X / FragmentCellSize);
                y = (int) Math.Floor(centroid.Y / FragmentCellSize);
                z = (int) Math.Floor(centroid.Z / FragmentCellSize);

                var cell = (x, y, z);

                List<Face>? faces;
                if (!cellFaces.TryGetValue(cell, out faces))
                {
                    faces = new List<Face>();
                    cellFaces[cell] = faces;
                }

                faces.Add(face);

            }

            var fragments = new List<TerrainFragment>();

            foreach (var cell in cellFaces)
            {
                for (int i = 0; i < cell.Value.Count; i += MaxFacesPerFragment)
                {
                    var part = cell.Value.GetRange(i, Math.Min(MaxFacesPerFragment, cell.Value.Count - i));
                    Utilities.DebugAssert(fragments.Count <= ushort.MaxValue, "Terrain fragment count exceeds the limit supported by 16-bit fragment IDs.");
                    fragments.Add(BuildFragment(geometry, part, (ushort) fragments.Count));
                }
            }

            return new Terrain(fragments, levelNumber);
        }

        private static TerrainFragment BuildFragment(ParsedModelGeometry geometry, List<Face> faces, ushort idx)
        {
            var groups = faces
                .GroupBy(face => GetMaterial(geometry, face))
                .OrderBy(group => group.Key.textureId)
                .ThenBy(group => group.Key.wrapModeS)
                .ThenBy(group => group.Key.wrapModeT)
                .ToList();

            var vertexBufferList = new List<float>();
            var indexBufferList = new List<ushort>();
            var textureConfig = new List<TextureConfig>();
            var vertexColorList = new List<byte>();

            var vertexLookup = new Dictionary<(int pos, int uv, int norm, int generatedNormalFace), ushort>();

            ushort indCnt = 0;
            int generatedNormalFace = 0;

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var group in groups)
            {
                ushort start = indCnt;

                foreach (var face in group)
                {
                    Vector3 faceNormal = GetFaceNormal(geometry, face);
                    int faceNormalId = generatedNormalFace++;
                    foreach (var corner in face.corners)
                    {
                        var key = (corner.pos, corner.uv, corner.norm, corner.norm < 0 ? faceNormalId : -1);

                        if (!vertexLookup.TryGetValue(key, out ushort vertIndex))
                        {
                            Vector3 pos = geometry.vertices[corner.pos];
                            Vector3 colors = geometry.colors[corner.pos];
                            Vector3 normal = corner.norm >= 0 ? geometry.normals[corner.norm] : faceNormal;
                            Vector2 uv = corner.uv >= 0 ? geometry.uvs[corner.uv] : Vector2.Zero;

                            vertIndex = (ushort) (vertexBufferList.Count / 8);

                            vertexBufferList.Add(pos.X);
                            vertexBufferList.Add(pos.Y);
                            vertexBufferList.Add(pos.Z);

                            vertexBufferList.Add(normal.X);
                            vertexBufferList.Add(normal.Y);
                            vertexBufferList.Add(normal.Z);

                            vertexBufferList.Add(uv.X);
                            vertexBufferList.Add(1.0f - uv.Y);

                            vertexColorList.Add((byte) (colors.X * 255f));
                            vertexColorList.Add((byte) (colors.Y * 255f));
                            vertexColorList.Add((byte) (colors.Z * 255f));
                            vertexColorList.Add(128);

                            min = Vector3.ComponentMin(min, pos);
                            max = Vector3.ComponentMax(max, pos);

                            vertexLookup[key] = vertIndex;
                        }

                        indexBufferList.Add(vertIndex);
                        indCnt++;
                    }
                }

                var config = new TextureConfig
                {
                    id = group.Key.textureId,
                    mode = 0x05000000,
                    start = start,
                    size = (ushort) (indCnt - start)
                };
                config.wrapModeS = group.Key.wrapModeS;
                config.wrapModeT = group.Key.wrapModeT;
                textureConfig.Add(config);
            }
            float[] vertexBuffer = vertexBufferList.ToArray();
            ushort[] indexBuffer = indexBufferList.ToArray();
            byte[] vertexColors = vertexColorList.ToArray();

            TerrainModel terrainModel = new TerrainModel(vertexBuffer, indexBuffer, vertexColors, textureConfig);

            Vector3 cullingCenter = (min + max) * 0.5f;
            float cullingSize = (max - cullingCenter).Length;

            return new TerrainFragment(terrainModel, cullingCenter, cullingSize, idx);
        }

        private static Vector3 GetFaceNormal(ParsedModelGeometry geometry, Face face)
        {
            Vector3 a = geometry.vertices[face.corners[0].pos];
            Vector3 b = geometry.vertices[face.corners[1].pos];
            Vector3 c = geometry.vertices[face.corners[2].pos];
            Vector3 normal = Vector3.Cross(b - a, c - a);
            float lengthSquared = normal.LengthSquared;
            return normal / MathF.Sqrt(lengthSquared);
        }

    }
}
