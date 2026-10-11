// Copyright (C) 2018-2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using LibReplanetizer.Models;
using ObjVector3 = OpenTK.Mathematics.Vector3;

namespace LibReplanetizer.Serializers.Importers
{
    public static class CollisionBuilder
    {
        public static Collision Build(ParsedModelGeometry geometry, byte collisionType = 0x1F)
        {
            CollisionType type = new CollisionType(collisionType);
            return Build(geometry, _ => type, false);
        }

        public static Collision BuildPerFace(ParsedModelGeometry geometry)
        {
            return Build(geometry, face =>
            {
                string? materialName = geometry.GetMaterialName(face.materialIndex);
                return materialName == null ? new CollisionType(0x1F) : ParseCollisionMaterial(materialName);
            }, true);
        }

        private static CollisionType ParseCollisionMaterial(string materialName)
        {
            Utilities.DebugAssert(materialName!.StartsWith("RC_", StringComparison.Ordinal), "Collision material names must start with 'RC_'.");

            string[] parts = materialName.Split('_');
            Utilities.DebugAssert(parts.Length >= 2 && parts.Length <= 4, "Collision material names must be RC_<materialID>, RC_<materialID>_<groupID>, or RC_<materialID>_<groupID>_<IgnoreCamera>.");

            int materialID = ParseUnsignedPart(parts[1], "material ID");
            Utilities.DebugAssert(materialID <= 0x1F, "Collision material IDs must be between 0 and 31.");

            int groupID = parts.Length >= 3 ? ParseUnsignedPart(parts[2], "group ID") : 0;
            Utilities.DebugAssert(groupID <= 0x03, "Collision group IDs must be between 0 and 3.");

            bool ignoreCamera = parts.Length == 4 && ParseIgnoreCamera(parts[3]);
            byte data = (byte) (materialID | (groupID << 5) | (ignoreCamera ? 0x80 : 0));
            return new CollisionType(data);
        }

        private static int ParseUnsignedPart(string value, string fieldName)
        {
            bool parsed = int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int result);
            Utilities.DebugAssert(parsed, $"Collision {fieldName} must be an unsigned decimal integer.");
            return result;
        }

        private static bool ParseIgnoreCamera(string value)
        {
            bool ignoreCamera = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
            bool allowCamera = string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) || value == "0";
            Utilities.DebugAssert(ignoreCamera || allowCamera, "IgnoreCamera must be true, false, 1, or 0.");
            return ignoreCamera;
        }

        private static Collision Build(ParsedModelGeometry geometry, Func<Face, CollisionType> collisionTypeSelector, bool skipNegativeCoordinates)
        {
            var triangles = new List<CollisionTriangle>(geometry.faces.Count);

            foreach (Face face in geometry.faces)
            {
                Utilities.DebugAssert(face != null && face.corners != null && face.corners.Length == 3,
                    "Collision faces must contain exactly three corners.");
                (int pos, int uv, int norm)[] corners = face!.corners!;

                ObjVector3 a = GetPosition(geometry, corners[0].pos);
                ObjVector3 b = GetPosition(geometry, corners[1].pos);
                ObjVector3 c = GetPosition(geometry, corners[2].pos);
                if (skipNegativeCoordinates &&
                    (a.X < 0 || a.Y < 0 || a.Z < 0 ||
                     b.X < 0 || b.Y < 0 || b.Z < 0 ||
                     c.X < 0 || c.Y < 0 || c.Z < 0))
                    continue;

                triangles.Add(new CollisionTriangle(
                    new System.Numerics.Vector3(a.X, a.Y, a.Z),
                    new System.Numerics.Vector3(b.X, b.Y, b.Z),
                    new System.Numerics.Vector3(c.X, c.Y, c.Z),
                    collisionTypeSelector(face)));
            }

            return Collision.FromTriangles(triangles);
        }

        private static ObjVector3 GetPosition(ParsedModelGeometry geometry, int index)
        {
            Utilities.DebugAssert(index >= 0 && index < geometry.vertices.Count,
                $"Collision face references missing position {index}.");
            return geometry.vertices[index];
        }
    }
}
