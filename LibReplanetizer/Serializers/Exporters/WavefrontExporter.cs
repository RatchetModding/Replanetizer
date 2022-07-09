// Copyright (C) 2018-2022, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibReplanetizer
{
    public class WavefrontExporter : Exporter
    {

        private ExporterModelSettings modelSettings = new ExporterModelSettings();
        private ExporterLevelSettings levelSettings = new ExporterLevelSettings();

        public WavefrontExporter()
        {
        }

        public WavefrontExporter(ExporterModelSettings settings)
        {
            this.modelSettings = settings;
        }

        public WavefrontExporter(ExporterLevelSettings settings)
        {
            this.levelSettings = settings;
        }

        public WavefrontExporter(ExporterModelSettings modelSettings, ExporterLevelSettings levelSettings)
        {
            this.modelSettings = modelSettings;
            this.levelSettings = levelSettings;
        }

        public override string GetFileEnding()
        {
            return ".obj";
        }

        private void WriteMaterial(StreamWriter mtlfs, string id)
        {
            mtlfs.WriteLine($"newmtl mtl_{id}");
            mtlfs.WriteLine("Ns 1000");
            mtlfs.WriteLine("Ka 1.000000 1.000000 1.000000");
            mtlfs.WriteLine("Kd 1.000000 1.000000 1.000000");
            mtlfs.WriteLine("Ni 1.000000");
            mtlfs.WriteLine("d 1.000000");
            mtlfs.WriteLine("illum 1");
            mtlfs.WriteLine($"map_Kd {id}.png");
        }

        private void WriteMaterial(StreamWriter? mtlfs, Model? model, List<int> usedMtls)
        {
            if (mtlfs == null || model == null) return;

            for (int i = 0; i < model.textureConfig.Count; i++)
            {
                int modelTextureID = model.textureConfig[i].id;
                if (usedMtls.Contains(modelTextureID))
                    continue;
                WriteMaterial(mtlfs, modelTextureID.ToString());
                usedMtls.Add(modelTextureID);
            }
        }

        /// <summary>
        /// Writes a collision model into a stream using the .obj file format.
        /// Every vertex contains 6 floats specifying position and color.
        /// No normals or uvs are written.
        /// </summary>
        /// <returns>The number of vertices that were written to the stream.</returns>
        private int WriteDataCollision(StreamWriter objfs, Collision coll, int faceOffset)
        {
            // Vertices
            var vertexCount = coll.vertexBuffer.Length / 4;
            for (int vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var px = coll.vertexBuffer[vertIdx * 0x04 + 0x0] * coll.size;
                var py = coll.vertexBuffer[vertIdx * 0x04 + 0x1] * coll.size;
                var pz = coll.vertexBuffer[vertIdx * 0x04 + 0x2] * coll.size;
                FloatColor fc = new FloatColor { r = 255, g = 0, b = 255, a = 255 }; ;
                fc.value = coll.vertexBuffer[vertIdx * 0x04 + 0x3];
                float red = fc.r / 255.0f;
                float green = fc.g / 255.0f;
                float blue = fc.b / 255.0f;
                objfs.WriteLine($"v {px:F6} {py:F6} {pz:F6} {red:F6} {green:F6} {blue:F6}");
            }

            // Faces
            var faceCount = coll.indBuff.Length / 3;
            for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
            {
                int vertIdx = faceIdx * 3;

                uint v1 = coll.indBuff[vertIdx + 0];
                uint v2 = coll.indBuff[vertIdx + 1];
                uint v3 = coll.indBuff[vertIdx + 2];

                v1 += 1 + (uint) faceOffset;
                v2 += 1 + (uint) faceOffset;
                v3 += 1 + (uint) faceOffset;

                objfs.WriteLine($"f {v1} {v2} {v3}");
            }

            return vertexCount;
        }

        private int WriteData(StreamWriter objfs, Model? model, int faceOffset, Matrix4 modelMatrix)
        {
            if (model == null) return 0;

            // skybox model has no normals and does the vertex buffer has a different layout
            // if we see other cases like this, it may be advisable to generalize this
            bool skyboxModel = (model is SkyboxModel);

            int bufferStride = (skyboxModel) ? 0x06 : 0x08;
            int vOffset = 0x00;
            int vnOffset = 0x03;
            int vtOffset = (skyboxModel) ? 0x03 : 0x06;

            int vertexCount = model.vertexBuffer.Length / bufferStride;

            // Vertices
            Vector3[] vertices = new Vector3[vertexCount];
            for (int vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var px = model.vertexBuffer[vertIdx * bufferStride + vOffset + 0x00];
                var py = model.vertexBuffer[vertIdx * bufferStride + vOffset + 0x01];
                var pz = model.vertexBuffer[vertIdx * bufferStride + vOffset + 0x02];
                var pos = new Vector4(px, py, pz, 1.0f) * modelMatrix;
                vertices[vertIdx] = pos.Xyz;
                objfs.WriteLine($"v {pos.X:F6} {pos.Y:F6} {pos.Z:F6}");
            }

            // Normals
            Vector3[]? normals = (skyboxModel) ? null : new Vector3[vertexCount];

            if (normals != null)
            {
                for (var vertIdx = 0; vertIdx < vertexCount; vertIdx++)
                {
                    var nx = model.vertexBuffer[vertIdx * bufferStride + vnOffset + 0x00];
                    var ny = model.vertexBuffer[vertIdx * bufferStride + vnOffset + 0x01];
                    var nz = model.vertexBuffer[vertIdx * bufferStride + vnOffset + 0x02];
                    var normal = (new Vector4(nx, ny, nz, 0.0f) * modelMatrix).Xyz;
                    normal.Normalize();
                    normals[vertIdx] = normal;
                    objfs.WriteLine($"vn {normal.X:F6} {normal.Y:F6} {normal.Z:F6}");
                }
            }

            // UVs
            for (var vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var tu = model.vertexBuffer[(vertIdx * bufferStride) + vtOffset + 0x00];
                var tv = 1f - model.vertexBuffer[(vertIdx * bufferStride) + vtOffset + 0x01];
                objfs.WriteLine($"vt {tu:F6} {tv:F6}");
            }

            // Faces
            int textureNum = 0;
            var faceCount = model.indexBuffer.Length / 3;
            for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
            {
                int vertIdx = faceIdx * 3;
                if (model.textureConfig != null && textureNum < model.textureConfig.Count &&
                    vertIdx >= model.textureConfig[textureNum].start)
                {
                    string modelId = model.textureConfig[textureNum].id.ToString();
                    objfs.WriteLine("usemtl mtl_" + modelId);
                    objfs.WriteLine("g Texture_" + modelId);
                    textureNum++;
                }

                int v1 = model.indexBuffer[vertIdx + 0];
                int v2 = model.indexBuffer[vertIdx + 1];
                int v3 = model.indexBuffer[vertIdx + 2];

                if (ShouldReverseWinding(vertices, normals, v1, v2, v3))
                    (v2, v3) = (v3, v2);

                v1 += 1 + faceOffset;
                v2 += 1 + faceOffset;
                v3 += 1 + faceOffset;

                if (skyboxModel)
                {
                    objfs.WriteLine($"f {v1}/{v1} {v2}/{v2} {v3}/{v3}");

                }
                else
                {
                    objfs.WriteLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
                }

            }

            return vertexCount;
        }

        public override void ExportModel(string fileName, Level level, Model model)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            if (modelSettings.exportMtlFile)
            {
                using (StreamWriter mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl"))
                {
                    // List used mtls to prevent it from making duplicate entries
                    List<int> usedMtls = new List<int>();
                    WriteMaterial(mtlfs, model, usedMtls);
                }
            }


            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                objfs.WriteLine("o Object_" + model.id.ToString("X4"));
                if (model.textureConfig != null && modelSettings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                Matrix4 scale = Matrix4.CreateScale(model.size);

                WriteData(objfs, model, 0, scale);
            }
        }

        private List<TerrainFragment> CollectTerrainFragments(Level level)
        {
            List<TerrainFragment> terrain = new List<TerrainFragment>();

            if (level.terrainChunks.Count == 0)
            {
                if (levelSettings.chunksSelected[0])
                {
                    terrain.AddRange(level.terrainEngine.fragments);
                }
            }
            else
            {
                for (int i = 0; i < level.terrainChunks.Count; i++)
                {
                    if (levelSettings.chunksSelected[i])
                    {
                        terrain.AddRange(level.terrainChunks[i].fragments);
                    }
                }
            }

            return terrain;
        }

        private void ExportLevelSeparate(string fileName, Level level)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level);

            StreamWriter? mtlfs = null;

            if (levelSettings.exportMtlFile) mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                if (levelSettings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                foreach (TerrainFragment t in terrain)
                {
                    objfs.WriteLine("o Object_" + t.model?.id.ToString("X4"));
                    faceOffset += WriteData(objfs, t.model, faceOffset, Matrix4.Identity);
                    if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                }

                if (levelSettings.writeTies)
                {
                    foreach (Tie t in level.ties)
                    {
                        objfs.WriteLine("o Object_" + t.model?.id.ToString("X4"));
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (levelSettings.writeShrubs)
                {
                    foreach (Shrub t in level.shrubs)
                    {
                        objfs.WriteLine("o Object_" + t.model?.id.ToString("X4"));
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (levelSettings.writeMobies)
                {
                    foreach (Moby t in level.mobs)
                    {
                        objfs.WriteLine("o Object_" + t.model?.id.ToString("X4"));
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }
            }

            if (levelSettings.exportMtlFile) mtlfs?.Dispose();
        }

        private void ExportLevelCombined(string fileName, Level level)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level);

            StreamWriter? mtlfs = null;

            if (levelSettings.exportMtlFile) mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (levelSettings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                objfs.WriteLine("o Object_CombinedLevel");
                foreach (TerrainFragment t in terrain)
                {
                    faceOffset += WriteData(objfs, t.model, faceOffset, Matrix4.Identity);
                    if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                }

                if (levelSettings.writeTies)
                {
                    foreach (Tie t in level.ties)
                    {
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (levelSettings.writeShrubs)
                {
                    foreach (Shrub t in level.shrubs)
                    {
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (levelSettings.writeMobies)
                {
                    foreach (Moby t in level.mobs)
                    {
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }
            }

            if (levelSettings.exportMtlFile) mtlfs?.Dispose();
        }

        private void ExportLevelTypewise(string fileName, Level level)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level);

            StreamWriter? mtlfs = null;

            if (levelSettings.exportMtlFile) mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (levelSettings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                if (terrain.Count != 0)
                {
                    objfs.WriteLine("o Object_Terrain");
                }

                foreach (TerrainFragment t in terrain)
                {
                    faceOffset += WriteData(objfs, t.model, faceOffset, Matrix4.Identity);
                    if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                }

                if (levelSettings.writeTies)
                {
                    objfs.WriteLine("o Object_Ties");

                    foreach (Tie t in level.ties)
                    {
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (levelSettings.writeShrubs)
                {
                    objfs.WriteLine("o Object_Shrubs");

                    foreach (Shrub t in level.shrubs)
                    {
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (levelSettings.writeMobies)
                {
                    objfs.WriteLine("o Object_Mobies");

                    foreach (Moby t in level.mobs)
                    {
                        faceOffset += WriteData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (levelSettings.exportMtlFile) WriteMaterial(mtlfs, t.model, usedMtls);
                    }
                }
            }

            if (levelSettings.exportMtlFile) mtlfs?.Dispose();
        }

        private static int SeparateModelObjectByMaterial(ModelObject t, List<Tuple<int, int, int>>[] faces, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, int faceOffset)
        {
            Model? model = t.model;

            if (model == null) return 0;

            int vertexCount = model.vertexBuffer.Length / 8;
            var modelMatrix = t.modelMatrix;
            // For correcting winding order we need proper indexing
            var thisVertices = new Vector3[vertexCount];
            var thisNormals = new Vector3[vertexCount];

            for (int x = 0; x < vertexCount; x++)
            {
                Vector4 v = new Vector4(
                    model.vertexBuffer[(x * 0x08) + 0x0],
                    model.vertexBuffer[(x * 0x08) + 0x1],
                    model.vertexBuffer[(x * 0x08) + 0x2],
                    1.0f);
                v *= modelMatrix;
                vertices.Add(v.Xyz);
                thisVertices[x] = v.Xyz;

                var normal = new Vector4(
                    model.vertexBuffer[(x * 0x08) + 0x3],
                    model.vertexBuffer[(x * 0x08) + 0x4],
                    model.vertexBuffer[(x * 0x08) + 0x5],
                    0.0f);
                normal *= modelMatrix;
                normal.Normalize();
                normals.Add(normal.Xyz);
                thisNormals[x] = normal.Xyz;

                uvs.Add(new Vector2(
                    model.vertexBuffer[(x * 0x08) + 0x6],
                    1f - model.vertexBuffer[(x * 0x08) + 0x7]));
            }

            int textureNum = 0;
            for (int i = 0; i < model.indexBuffer.Length / 3; i++)
            {
                int triIndex = i * 3;
                int materialID = 0;

                if ((model.textureConfig != null) && (textureNum < model.textureConfig.Count))
                {
                    if ((textureNum + 1 < model.textureConfig.Count) && (triIndex >= model.textureConfig[textureNum + 1].start))
                    {
                        textureNum++;
                    }

                    materialID = model.textureConfig[textureNum].id;
                }

                if (materialID >= faces.Length || materialID < 0)
                {
                    materialID = 0;
                }

                var v1 = model.indexBuffer[triIndex + 0];
                var v2 = model.indexBuffer[triIndex + 1];
                var v3 = model.indexBuffer[triIndex + 2];

                if (ShouldReverseWinding(thisVertices, thisNormals, v1, v2, v3))
                    (v2, v3) = (v3, v2);

                faces[materialID].Add(new Tuple<int, int, int>(
                    v1 + 1 + faceOffset,
                    v2 + 1 + faceOffset,
                    v3 + 1 + faceOffset));
            }

            return vertexCount;
        }

        private void ExportLevelMaterialwise(string fileName, Level level)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level);

            int materialCount = level.textures.Count;

            List<Tuple<int, int, int>>[] faces = new List<Tuple<int, int, int>>[materialCount];

            for (int i = 0; i < materialCount; i++)
            {
                faces[i] = new List<Tuple<int, int, int>>();
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            int faceOffset = 0;

            foreach (TerrainFragment t in terrain)
            {
                faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
            }

            if (levelSettings.writeTies)
            {
                foreach (Tie t in level.ties)
                {
                    faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
                }
            }

            if (levelSettings.writeShrubs)
            {
                foreach (Shrub t in level.shrubs)
                {
                    faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
                }
            }

            if (levelSettings.writeMobies)
            {
                foreach (Moby t in level.mobs)
                {
                    faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
                }
            }

            if (levelSettings.exportMtlFile)
            {
                using (StreamWriter mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl"))
                {
                    for (int i = 0; i < materialCount; i++)
                    {
                        if (faces[i].Count != 0)
                            WriteMaterial(mtlfs, i.ToString());
                    }
                }
            }


            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (levelSettings.exportMtlFile)
                    objfs.WriteLine($"mtllib {fileNameNoExtension}.mtl");

                foreach (Vector3 v in vertices)
                {
                    objfs.WriteLine($"v {v.X:F6} {v.Y:F6} {v.Z:F6}");
                }

                foreach (Vector3 vn in normals)
                {
                    objfs.WriteLine($"vn {vn.X:F6} {vn.Y:F6} {vn.Z:F6}");
                }

                foreach (Vector2 vt in uvs)
                {
                    objfs.WriteLine($"vt {vt.X:F6} {vt.Y:F6}");
                }

                for (int i = 0; i < materialCount; i++)
                {
                    List<Tuple<int, int, int>> list = faces[i];

                    if (list.Count == 0) continue;

                    objfs.WriteLine("o Object_Material_" + i);

                    if (levelSettings.exportMtlFile)
                    {
                        objfs.WriteLine("usemtl mtl_" + i);
                        objfs.WriteLine("g Texture_" + i);
                    }

                    foreach (var (v1, v2, v3) in list)
                    {
                        objfs.WriteLine(
                            $"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}"
                        );
                    }
                }
            }
        }

        public void ExportLevel(string fileName, Level level)
        {
            switch (levelSettings.mode)
            {
                case ExporterLevelSettings.Mode.Separate:
                    ExportLevelSeparate(fileName, level);
                    return;
                case ExporterLevelSettings.Mode.Combined:
                    ExportLevelCombined(fileName, level);
                    return;
                case ExporterLevelSettings.Mode.Typewise:
                    ExportLevelTypewise(fileName, level);
                    return;
                case ExporterLevelSettings.Mode.Materialwise:
                    ExportLevelMaterialwise(fileName, level);
                    return;
            }
        }


        public void ExportCollision(string fileName, Level level)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (level.collisionChunks.Count == 0)
                {
                    WriteDataCollision(objfs, (Collision) level.collisionEngine, 0);
                }
                else
                {
                    int faceOffset = 0;

                    foreach (Collision col in level.collisionChunks)
                    {
                        faceOffset += WriteDataCollision(objfs, col, faceOffset);
                    }
                }
            }
        }
    }
}
