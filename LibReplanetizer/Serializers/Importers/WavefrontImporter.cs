// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Mathematics;

namespace LibReplanetizer.Serializers.Importers
{
    public class Face
    {
        public (int pos, int uv, int norm)[] corners = new (int, int, int)[3];
        public int texture;
    }
    public class ParsedModelGeometry
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector3> colors = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<Face> faces = new List<Face>();
    }
    public class WavefrontImporter
    {
        public ParsedModelGeometry Import(string path)
        {
            ParsedModelGeometry parsed = new ParsedModelGeometry();

            string? line;
            StreamReader file = new StreamReader(path);
            int currentTexture = 0;

            while ((line = file.ReadLine()) != null)
            {
                string[] split = line.Split(' ');

                switch (split[0])
                {
                    case "v":
                        parsed.vertices.Add(new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3])));

                        if (split.Length >= 7)
                        {
                            parsed.colors.Add(new Vector3(float.Parse(split[4]), float.Parse(split[5]), float.Parse(split[6])));
                        }
                        else
                        {
                            parsed.colors.Add(new Vector3(0.5f, 0.5f, 0.5f));
                        }
                        break;

                    case "vn":
                        // TODO: Should have support for converting Y/Z up direction 
                        parsed.normals.Add(new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3])));
                        break;

                    case "vt":
                        parsed.uvs.Add(new Vector2(float.Parse(split[1]), float.Parse(split[2])));
                        break;

                    case "usemtl":
                        string digits = new string(split[1].Where(char.IsDigit).ToArray());
                        currentTexture = int.Parse(digits);
                        break;

                    case "f":
                        Face face = new Face();
                        face.texture = currentTexture;
                        for (int i = 0; i < 3; i++)
                        {
                            string[] parts = split[i + 1].Split('/');

                            face.corners[i] = (
                                int.Parse(parts[0]) - 1, // Pos
                                int.Parse(parts[1]) - 1, // UV
                                int.Parse(parts[2]) - 1  // Normal
                            );
                        }
                        parsed.faces.Add(face);

                        break;

                }
            }

            return parsed;
        }
    }
}
