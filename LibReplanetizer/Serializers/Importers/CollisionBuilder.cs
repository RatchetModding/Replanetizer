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
            return Build(geometry, _ => (type, CollisionGeometryCategory.Standard), false);
        }

        public static Collision BuildPerFace(ParsedModelGeometry geometry)
        {
            if (geometry.HasBounds &&
                (geometry.BoundsMin.X < 0 || geometry.BoundsMin.Y < 0 || geometry.BoundsMin.Z < 0))
            {
                throw new InvalidOperationException(
                    $"OBJ collision mesh bounds contain negative coordinates (min: {geometry.BoundsMin}, max: {geometry.BoundsMax}); the collision grid cannot represent negative coordinates.");
            }

            return Build(geometry, face =>
            {
                string? materialName = geometry.GetMaterialName(face.materialIndex);
                return materialName == null
                    ? (new CollisionType(0x1F), CollisionGeometryCategory.Standard)
                    : ParseCollisionMaterial(materialName);
            }, !geometry.HasBounds);
        }

        private static (CollisionType type, CollisionGeometryCategory category) ParseCollisionMaterial(string materialName)
        {
            if (!materialName.StartsWith("RC_", StringComparison.Ordinal))
                return (new CollisionType(0x1F), CollisionGeometryCategory.Standard);

            string[] parts = materialName.Split('_');
            if (parts.Length < 2 || parts.Length > 5)
                throw new FormatException("Collision material names must be RC_<materialID>[_<groupID>[_<IgnoreCamera>]][_<category>].");

            int materialID = ParseUnsignedPart(parts[1], "material ID");
            if (materialID > 0x1F)
                throw new FormatException("Collision material IDs must be between 0 and 31.");

            int groupID = parts.Length >= 3 ? ParseUnsignedPart(parts[2], "group ID") : 0;
            if (groupID > 0x03)
                throw new FormatException("Collision group IDs must be between 0 and 3.");

            bool ignoreCamera = parts.Length >= 4 && ParseIgnoreCamera(parts[3]);
            CollisionGeometryCategory category = parts.Length == 5 ? ParseCategory(parts[4]) : CollisionGeometryCategory.Standard;
            byte data = (byte) (materialID | (groupID << 5) | (ignoreCamera ? 0x80 : 0));
            return (new CollisionType(data), category);
        }

        private static CollisionGeometryCategory ParseCategory(string value)
        {
            if (string.Equals(value, nameof(CollisionGeometryCategory.Standard), StringComparison.OrdinalIgnoreCase))
                return CollisionGeometryCategory.Standard;
            if (string.Equals(value, nameof(CollisionGeometryCategory.Hero), StringComparison.OrdinalIgnoreCase))
                return CollisionGeometryCategory.Hero;
            if (string.Equals(value, nameof(CollisionGeometryCategory.Unknown), StringComparison.OrdinalIgnoreCase))
                return CollisionGeometryCategory.Unknown;
            throw new FormatException($"Unsupported collision geometry category '{value}'.");
        }

        private static int ParseUnsignedPart(string value, string fieldName)
        {
            bool parsed = int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int result);
            if (!parsed)
                throw new FormatException($"Collision {fieldName} must be an unsigned decimal integer.");
            return result;
        }

        private static bool ParseIgnoreCamera(string value)
        {
            bool ignoreCamera = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
            bool allowCamera = string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) || value == "0";
            if (!ignoreCamera && !allowCamera)
                throw new FormatException("IgnoreCamera must be true, false, 1, or 0.");
            return ignoreCamera;
        }

        private static Collision Build(ParsedModelGeometry geometry, Func<Face, (CollisionType type, CollisionGeometryCategory category)> collisionMetadataSelector, bool rejectNegativeCoordinates)
        {
            var standardFaces = new List<CollisionFace>(geometry.faces.Count);
            var heroTriangles = new List<CollisionTriangle>();
            var unknownTriangles = new List<CollisionTriangle>();

            System.Numerics.Vector3 ConvertPosition(int index)
            {
                ObjVector3 position = GetPosition(geometry, index);
                if (rejectNegativeCoordinates && (position.X < 0 || position.Y < 0 || position.Z < 0))
                    throw new InvalidOperationException("OBJ collision geometry contains negative coordinates, which the collision grid cannot represent.");
                return new System.Numerics.Vector3(position.X, position.Y, position.Z);
            }

            foreach (Face face in geometry.faces)
            {
                if (face == null || face.corners == null || face.corners.Length is < 3 or > 4)
                    throw new InvalidOperationException("Collision faces must contain three or four corners.");
                (int pos, int uv, int norm)[] corners = face!.corners!;

                (CollisionType type, CollisionGeometryCategory category) = collisionMetadataSelector(face);
                if (category != CollisionGeometryCategory.Standard && type.data != 0x80)
                    throw new FormatException($"Collision category {category} requires collision type 0x80.");

                System.Numerics.Vector3 a = ConvertPosition(corners[0].pos);
                System.Numerics.Vector3 b = ConvertPosition(corners[1].pos);
                System.Numerics.Vector3 c = ConvertPosition(corners[2].pos);
                CollisionFace collisionFace = corners.Length == 4
                    ? new CollisionFace(a, b, c, ConvertPosition(corners[3].pos), type)
                    : new CollisionFace(a, b, c, type);
                switch (category)
                {
                    case CollisionGeometryCategory.Standard:
                        standardFaces.Add(collisionFace);
                        break;
                    case CollisionGeometryCategory.Hero:
                        AddSpecialTriangles(heroTriangles, collisionFace);
                        break;
                    case CollisionGeometryCategory.Unknown:
                        AddSpecialTriangles(unknownTriangles, collisionFace);
                        break;
                    default:
                        throw new FormatException($"Unsupported collision geometry category {category}.");
                }
            }

            return Collision.FromFaces(standardFaces, heroTriangles, unknownTriangles);
        }

        private static void AddSpecialTriangles(List<CollisionTriangle> triangles, CollisionFace face)
        {
            triangles.Add(new CollisionTriangle(face.A, face.B, face.C, face.Type));
            if (face.IsQuad)
                triangles.Add(new CollisionTriangle(face.A, face.C, face.D, face.Type));
        }

        private static ObjVector3 GetPosition(ParsedModelGeometry geometry, int index)
        {
            Utilities.DebugAssert(index >= 0 && index < geometry.vertices.Count,
                $"Collision face references missing position {index}.");
            return geometry.vertices[index];
        }
    }
}
