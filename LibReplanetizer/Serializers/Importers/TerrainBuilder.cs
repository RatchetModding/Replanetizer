// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;

namespace LibReplanetizer.Serializers.Importers
{
    public class TerrainBuilder
    {
        public static Terrain Build(ParsedModelGeometry geometry, ushort levelNumber)
        {
            float fragmentSize = 32.0f;

            var cellFaces = new Dictionary<(int x, int y, int z), List<Face>>();
            foreach (var face in geometry.faces)
            {
                Vector3 a, b, c;
                a = geometry.vertices[face.corners[0].pos];
                b = geometry.vertices[face.corners[1].pos];
                c = geometry.vertices[face.corners[2].pos];

                Vector3 centroid = (a + b + c) / 3.0f;

                int x, y, z;
                x = (int)Math.Floor(centroid.X / fragmentSize);
                y = (int)Math.Floor(centroid.Y / fragmentSize);
                z = (int)Math.Floor(centroid.Z / fragmentSize);

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
                int maxFaces = 250 / 3; // This is totally arbitrary and probably doesn't match up at all with how it works in-game. 
                for (int i = 0; i < cell.Value.Count; i += maxFaces)
                {
                    var part = cell.Value.GetRange(i, Math.Min(maxFaces, cell.Value.Count - i));
                    fragments.Add(BuildFragment(geometry, part, (ushort) fragments.Count));
                }
            }

            return new Terrain(fragments, levelNumber);
        }
        private static TerrainFragment BuildFragment(ParsedModelGeometry geometry, List<Face> faces, ushort idx)
        {
            var groups = faces.GroupBy(f => f.texture).OrderBy(g => g.Key).ToList();

            var vertexBufferList = new List<float>();
            var indexBufferList = new List<ushort>();
            var textureConfig = new List<TextureConfig>();
            var vertexColorList = new List<byte>();

            var vertexLookup = new Dictionary<(int pos, int uv, int norm), ushort>();

            ushort indCnt = 0;

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var group in groups)
            {
                ushort start = indCnt;

                foreach (var face in group)
                {
                    foreach (var corner in face.corners)
                    {
                        var key = (corner.pos, corner.uv, corner.norm);

                        if (!vertexLookup.TryGetValue(key, out ushort vertIndex))
                        {
                            Vector3 pos = geometry.vertices[corner.pos];
                            Vector3 colors = geometry.colors[corner.pos];
                            Vector3 normal = geometry.normals[corner.norm];
                            Vector2 uv = geometry.uvs[corner.uv];

                            vertIndex = (ushort) (vertexBufferList.Count / 8);

                            vertexBufferList.Add(pos.X);
                            vertexBufferList.Add(pos.Y);
                            vertexBufferList.Add(pos.Z);

                            vertexBufferList.Add(normal.X);
                            vertexBufferList.Add(normal.Y);
                            vertexBufferList.Add(normal.Z);

                            vertexBufferList.Add(uv.X);
                            vertexBufferList.Add(1.0f - uv.Y);

                            vertexColorList.Add((byte)(colors.X * 255f));
                            vertexColorList.Add((byte)(colors.Y * 255f));
                            vertexColorList.Add((byte)(colors.Z * 255f));
                            vertexColorList.Add(128);

                            min = Vector3.ComponentMin(min, pos);
                            max = Vector3.ComponentMax(max, pos);

                            vertexLookup[key] = vertIndex;
                        }

                        indexBufferList.Add(vertIndex);
                        indCnt++;
                    }
                }

                textureConfig.Add(new TextureConfig
                {
                    id = group.Key,
                    mode = 0x5000000,// hardcoded as Wrapmode.Repeat.
                                     // I imagine if people wanted to use clamp edge they could just change it manually.
                    start = start,
                    size = (ushort) (indCnt - start)
                });
            }
            float[] vertexBuffer = vertexBufferList.ToArray();
            ushort[] indexBuffer = indexBufferList.ToArray();
            byte[] vertexColors = vertexColorList.ToArray();

            TerrainModel terrainModel = new TerrainModel(vertexBuffer, indexBuffer, vertexColors, textureConfig);

            Vector3 cullingCenter = (min + max) * 0.5f;
            float cullingSize = (max - cullingCenter).Length;

            return new TerrainFragment(terrainModel, cullingCenter, cullingSize, idx);
        }

    }
}
