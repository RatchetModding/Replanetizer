﻿using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace LibReplanetizer
{
    public static class ModelWriter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static void writeObjectMaterial(StreamWriter MTLfs, string id)
        {
            MTLfs.WriteLine($"newmtl mtl_{id}");
            MTLfs.WriteLine("Ns 1000");
            MTLfs.WriteLine("Ka 1.000000 1.000000 1.000000");
            MTLfs.WriteLine("Kd 1.000000 1.000000 1.000000");
            MTLfs.WriteLine("Ni 1.000000");
            MTLfs.WriteLine("d 1.000000");
            MTLfs.WriteLine("illum 1");
            MTLfs.WriteLine($"map_Kd {id}.png");
        }

        private static void writeObjectMaterial(StreamWriter MTLfs, Model model, List<int> usedMtls)
        {
            for (int i = 0; i < model.textureConfig.Count; i++)
            {
                int modelTextureID = model.textureConfig[i].ID;
                if (usedMtls.Contains(modelTextureID))
                    continue;
                writeObjectMaterial(MTLfs, modelTextureID.ToString());
                usedMtls.Add(modelTextureID);
            }
        }

        private static int writeObjectData(StreamWriter OBJfs, Model model, int faceOffset, Matrix4 modelMatrix)
        {
            var vertexCount = model.vertexBuffer.Length / 8;

            // Vertices
            var vertices = new Vector3[vertexCount];
            for (int vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var px = model.vertexBuffer[vertIdx * 0x08 + 0x0];
                var py = model.vertexBuffer[vertIdx * 0x08 + 0x1];
                var pz = model.vertexBuffer[vertIdx * 0x08 + 0x2];
                var pos = new Vector4(px, py, pz, 1.0f) * modelMatrix;
                vertices[vertIdx] = pos.Xyz;
                OBJfs.WriteLine($"v {pos.X:F6} {pos.Y:F6} {pos.Z:F6}");
            }

            // Normals
            var normals = new Vector3[vertexCount];
            for (var vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var nx = model.vertexBuffer[vertIdx * 0x08 + 0x3];
                var ny = model.vertexBuffer[vertIdx * 0x08 + 0x4];
                var nz = model.vertexBuffer[vertIdx * 0x08 + 0x5];
                var normal = (new Vector4(nx, ny, nz, 0.0f) * modelMatrix).Xyz;
                normal.Normalize();
                normals[vertIdx] = normal;
                OBJfs.WriteLine($"vn {normal.X:F6} {normal.Y:F6} {normal.Z:F6}");
            }

            // UVs
            for (var vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var tu = model.vertexBuffer[(vertIdx * 0x08) + 0x6];
                var tv = 1f - model.vertexBuffer[(vertIdx * 0x08) + 0x7];
                OBJfs.WriteLine($"vt {tu:F6} {tv:F6}");
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
                    string modelId = model.textureConfig[textureNum].ID.ToString();
                    OBJfs.WriteLine("usemtl mtl_" + modelId);
                    OBJfs.WriteLine("g Texture_" + modelId);
                    textureNum++;
                }

                int v1 = model.indexBuffer[vertIdx + 0];
                int v2 = model.indexBuffer[vertIdx + 1];
                int v3 = model.indexBuffer[vertIdx + 2];

                if (shouldReverseWinding(vertices, normals, v1, v2, v3))
                    (v2, v3) = (v3, v2);

                v1 += 1 + faceOffset;
                v2 += 1 + faceOffset;
                v3 += 1 + faceOffset;

                OBJfs.WriteLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
            }

            return vertexCount;
        }

        /// <summary>
        /// Average a tri's vertex normals to get the target face normal, then
        /// check whether the current winding order yields a normal facing
        /// the target face normal
        /// </summary>
        /// <returns>whether to reverse the winding order</returns>
        private static bool shouldReverseWinding(
            IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3> vertexNormals, int v1, int v2, int v3)
        {
            var targetFaceNormal = faceNormalFromVertexNormals(vertexNormals, v1, v2, v3);
            var p1 = vertices[v1];
            var p2 = vertices[v2];
            var p3 = vertices[v3];
            return shouldReverseWinding(p1, p2, p3, targetFaceNormal);
        }

        private static bool shouldReverseWinding(
            Vector3 p1, Vector3 p2, Vector3 p3, Vector3 targetFaceNormal)
        {
            p2 -= p1;
            p3 -= p1;
            var faceNormal = Vector3.Cross(p2, p3);
            var dot = Vector3.Dot(faceNormal, targetFaceNormal);
            return dot < 0f;
        }

        private static Vector3 faceNormalFromVertexNormals(
            IReadOnlyList<Vector3> normals, int v1, int v2, int v3)
        {
            var n1 = normals[v1];
            var n2 = normals[v2];
            var n3 = normals[v3];
            return (n1 + n2 + n3) / 3;
        }

        public static void WriteIqe(string fileName, Level level, Model model)
        {
            Logger.Trace(fileName);

            string filePath = Path.GetDirectoryName(fileName);

            if (!(model is MobyModel mobyModel)) return;

            using (StreamWriter spookyStream = new StreamWriter(fileName))
            {
                spookyStream.WriteLine("# Inter-Quake Export");

                // Binding pose
                for (int i = 0; i < mobyModel.boneDatas.Count; i++)
                {
                    Quaternion quat = mobyModel.boneMatrices[i].mat1.ExtractRotation();

                    Matrix4 mat = mobyModel.boneMatrices[i].mat1;
                    float x = mat.M41 / 1024f;
                    float y = mat.M42 / 1024f;
                    float z = mat.M43 / 1024f;

                    float xx = mobyModel.boneDatas[i].unk1 / 1024f;
                    float yy = mobyModel.boneDatas[i].unk2 / 1024f;
                    float zz = mobyModel.boneDatas[i].unk3 / 1024f;

                    short par = (short) (mobyModel.boneMatrices[i].bb / 0x40);
                    spookyStream.WriteLine("joint h" + i.ToString() + " " + (par == 0 ? "" : par.ToString()));
                    spookyStream.WriteLine("pq " + xx.ToString() + " " + yy.ToString() + " " + zz.ToString());
                }

                List<Animation> anims;

                if (mobyModel.id == 0)
                    anims = level.playerAnimations;
                else
                    anims = mobyModel.animations;

                int idx = 0;
                int animIndex = 0;
                foreach (Animation anim in anims)
                {
                    if (anim.frames.Count == 0) continue;
                    spookyStream.WriteLine("animation " + animIndex.ToString());
                    spookyStream.WriteLine("framerate " + 60f * anim.speed);

                    int frameIndex = 0;
                    foreach (Frame frame in anim.frames)
                    {
                        idx = 0;
                        spookyStream.WriteLine("frame " + frameIndex.ToString());
                        foreach (short[] quat in frame.rotations)
                        {
                            BoneData bd = mobyModel.boneDatas[idx];
                            //Vector3 vec = mat.mat1.ExtractTranslation();

                            /*
                            foreach(short[] tran in frame.translations)
                            {
                                if(tran[3] / 0x100 == idx)
                                {
                                    x *= -tran[0] / 32767f;
                                    y *= -tran[1] / 32767f;
                                    z *= -tran[2] / 32767f;
                                }
                            }*/

                            float xx = mobyModel.boneDatas[idx].unk1 / 1024f;
                            float yy = mobyModel.boneDatas[idx].unk2 / 1024f;
                            float zz = mobyModel.boneDatas[idx].unk3 / 1024f;

                            spookyStream.WriteLine("pq " + xx.ToString() + " " + yy.ToString() + " " + zz.ToString() + " " + quat[0] / 32767f + " " + quat[1] / 32767f + " " + quat[2] / 32767f + " " + -quat[3] / 32767f);
                            idx++;
                        }
                        frameIndex++;
                    }
                    animIndex++;
                }


                //Faces
                int tCnt = 0;
                for (int i = 0; i < model.indexBuffer.Length / 3; i++)
                {
                    if (model.textureConfig != null && tCnt < model.textureConfig.Count)
                    {
                        if (i * 3 >= model.textureConfig[tCnt].start)
                        {
                            spookyStream.WriteLine("mesh " + model.textureConfig[tCnt].ID.ToString(""));
                            if (model.textureConfig[tCnt].ID != -1)
                            {
                                spookyStream.WriteLine("material " + model.textureConfig[tCnt].ID.ToString("x") + ".png");
                                Bitmap bump = level.textures[model.textureConfig[tCnt].ID].getTextureImage();
                                bump.Save(filePath + "/" + model.textureConfig[tCnt].ID.ToString("x") + ".png");
                            }
                            tCnt++;
                        }
                    }
                    int f1 = model.indexBuffer[i * 3 + 0];
                    int f2 = model.indexBuffer[i * 3 + 1];
                    int f3 = model.indexBuffer[i * 3 + 2];
                    spookyStream.WriteLine("fm " + f1 + " " + f2 + " " + f3);
                }

                //Vertices, normals, UV's
                for (int x = 0; x < model.vertexBuffer.Length / 8; x++)
                {
                    float px = model.vertexBuffer[(x * 0x08) + 0x0];
                    float py = model.vertexBuffer[(x * 0x08) + 0x1];
                    float pz = model.vertexBuffer[(x * 0x08) + 0x2];
                    float nx = model.vertexBuffer[(x * 0x08) + 0x3];
                    float ny = model.vertexBuffer[(x * 0x08) + 0x4];
                    float nz = model.vertexBuffer[(x * 0x08) + 0x5];
                    float tu = model.vertexBuffer[(x * 0x08) + 0x6];
                    float tv = model.vertexBuffer[(x * 0x08) + 0x7];
                    spookyStream.WriteLine("vp " + px.ToString("G") + " " + py.ToString("G") + " " + pz.ToString("G"));
                    spookyStream.WriteLine("vn " + nx.ToString("G") + " " + ny.ToString("G") + " " + nz.ToString("G"));
                    spookyStream.WriteLine("vt " + tu.ToString("G") + " " + tv.ToString("G"));

                    byte[] weights = BitConverter.GetBytes(model.weights[x]);
                    byte[] indices = BitConverter.GetBytes(model.ids[x]);

                    spookyStream.WriteLine("vb " + indices[3].ToString() + " " + (weights[3] / 255f).ToString() + " " + indices[2].ToString() + " " + (weights[2] / 255f).ToString() + " " + indices[1].ToString() + " " + (weights[1] / 255f).ToString() + " " + indices[0].ToString() + " " + (weights[0] / 255f).ToString());
                }
            }
        }

        public static void WriteObj(string fileName, Model model)
        {
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            using (StreamWriter MTLfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl"))
            {
                // List used mtls to prevent it from making duplicate entries
                List<int> usedMtls = new List<int>();
                writeObjectMaterial(MTLfs, model, usedMtls);
            }

            using (StreamWriter OBJfs = new StreamWriter(fileName))
            {
                OBJfs.WriteLine("o Object_" + model.id.ToString("X4"));
                if (model.textureConfig != null)
                    OBJfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                writeObjectData(OBJfs, model, 0, Matrix4.Identity);
            }
        }

        private static List<TerrainFragment> collectTerrainFragments(Level level, WriterLevelSettings settings)
        {
            List<TerrainFragment> terrain = new List<TerrainFragment>();

            if (level.terrainChunks.Count == 0)
            {
                if (settings.chunksSelected[0])
                {
                    terrain.AddRange(level.terrainEngine.fragments);
                }
            }
            else
            {
                for (int i = 0; i < level.terrainChunks.Count; i++)
                {
                    if (settings.chunksSelected[i])
                    {
                        terrain.AddRange(level.terrainChunks[i].fragments);
                    }
                }
            }

            return terrain;
        }

        private static void WriteObjSeparate(string fileName, Level level, WriterLevelSettings settings)
        {
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = collectTerrainFragments(level, settings);

            StreamWriter MTLfs = null;

            if (settings.exportMTLFile) MTLfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter OBJfs = new StreamWriter(fileName))
            {
                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                if (settings.exportMTLFile)
                    OBJfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                foreach (TerrainFragment t in terrain)
                {
                    OBJfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                    faceOffset += writeObjectData(OBJfs, t.model, faceOffset, Matrix4.Identity);
                    if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                }

                if (settings.writeTies)
                {
                    foreach (Tie t in level.ties)
                    {
                        OBJfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }

                if (settings.writeShrubs)
                {
                    foreach (Shrub t in level.shrubs)
                    {
                        OBJfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }

                if (settings.writeMobies)
                {
                    foreach (Moby t in level.mobs)
                    {
                        OBJfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }
            }

            if (settings.exportMTLFile) MTLfs.Dispose();
        }

        private static void WriteObjCombined(string fileName, Level level, WriterLevelSettings settings)
        {
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = collectTerrainFragments(level, settings);

            StreamWriter MTLfs = null;

            if (settings.exportMTLFile) MTLfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter OBJfs = new StreamWriter(fileName))
            {
                if (settings.exportMTLFile)
                    OBJfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                OBJfs.WriteLine("o Object_CombinedLevel");
                foreach (TerrainFragment t in terrain)
                {
                    faceOffset += writeObjectData(OBJfs, t.model, faceOffset, Matrix4.Identity);
                    if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                }

                if (settings.writeTies)
                {
                    foreach (Tie t in level.ties)
                    {
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }

                if (settings.writeShrubs)
                {
                    foreach (Shrub t in level.shrubs)
                    {
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }

                if (settings.writeMobies)
                {
                    foreach (Moby t in level.mobs)
                    {
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }
            }

            if (settings.exportMTLFile) MTLfs.Dispose();
        }

        private static void WriteObjTypewise(string fileName, Level level, WriterLevelSettings settings)
        {
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = collectTerrainFragments(level, settings);

            StreamWriter MTLfs = null;

            if (settings.exportMTLFile) MTLfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter OBJfs = new StreamWriter(fileName))
            {
                if (settings.exportMTLFile)
                    OBJfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                if (terrain.Count != 0)
                {
                    OBJfs.WriteLine("o Object_Terrain");
                }

                foreach (TerrainFragment t in terrain)
                {
                    faceOffset += writeObjectData(OBJfs, t.model, faceOffset, Matrix4.Identity);
                    if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                }

                if (settings.writeTies)
                {
                    OBJfs.WriteLine("o Object_Ties");

                    foreach (Tie t in level.ties)
                    {
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }

                if (settings.writeShrubs)
                {
                    OBJfs.WriteLine("o Object_Shrubs");

                    foreach (Shrub t in level.shrubs)
                    {
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }

                if (settings.writeMobies)
                {
                    OBJfs.WriteLine("o Object_Mobies");

                    foreach (Moby t in level.mobs)
                    {
                        faceOffset += writeObjectData(OBJfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMTLFile) writeObjectMaterial(MTLfs, t.model, usedMtls);
                    }
                }
            }

            if (settings.exportMTLFile) MTLfs.Dispose();
        }

        private static int SeparateModelObjectByMaterial(ModelObject t, List<Tuple<int, int, int>>[] faces, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, int faceOffset)
        {
            Model model = t.model;

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

                    materialID = model.textureConfig[textureNum].ID;
                }

                if (materialID >= faces.Length || materialID < 0)
                {
                    materialID = 0;
                }

                var v1 = model.indexBuffer[triIndex + 0];
                var v2 = model.indexBuffer[triIndex + 1];
                var v3 = model.indexBuffer[triIndex + 2];

                if (shouldReverseWinding(thisVertices, thisNormals, v1, v2, v3))
                    (v2, v3) = (v3, v2);

                faces[materialID].Add(new Tuple<int, int, int>(
                    v1 + 1 + faceOffset,
                    v2 + 1 + faceOffset,
                    v3 + 1 + faceOffset));
            }

            return vertexCount;
        }

        private static void WriteObjMaterialwise(string fileName, Level level, WriterLevelSettings settings)
        {
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = collectTerrainFragments(level, settings);

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

            if (settings.writeTies)
            {
                foreach (Tie t in level.ties)
                {
                    faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
                }
            }

            if (settings.writeShrubs)
            {
                foreach (Shrub t in level.shrubs)
                {
                    faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
                }
            }

            if (settings.writeMobies)
            {
                foreach (Moby t in level.mobs)
                {
                    faceOffset += SeparateModelObjectByMaterial(t, faces, vertices, uvs, normals, faceOffset);
                }
            }

            if (settings.exportMTLFile)
            {
                using (StreamWriter MTLfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl"))
                {
                    for (int i = 0; i < materialCount; i++)
                    {
                        if (faces[i].Count != 0)
                            writeObjectMaterial(MTLfs, i.ToString());
                    }
                }
            }


            using (StreamWriter OBJfs = new StreamWriter(fileName))
            {
                if (settings.exportMTLFile)
                    OBJfs.WriteLine($"mtllib {fileNameNoExtension}.mtl");

                foreach (Vector3 v in vertices)
                {
                    OBJfs.WriteLine($"v {v.X:F6} {v.Y:F6} {v.Z:F6}");
                }

                foreach (Vector3 vn in normals)
                {
                    OBJfs.WriteLine($"vn {vn.X:F6} {vn.Y:F6} {vn.Z:F6}");
                }

                foreach (Vector2 vt in uvs)
                {
                    OBJfs.WriteLine($"vt {vt.X:F6} {vt.Y:F6}");
                }

                for (int i = 0; i < materialCount; i++)
                {
                    List<Tuple<int, int, int>> list = faces[i];

                    if (list.Count == 0) continue;

                    OBJfs.WriteLine("o Object_Material_" + i);

                    if (settings.exportMTLFile)
                    {
                        OBJfs.WriteLine("usemtl mtl_" + i);
                        OBJfs.WriteLine("g Texture_" + i);
                    }

                    foreach (var (v1, v2, v3) in list)
                    {
                        OBJfs.WriteLine(
                            $"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}"
                        );
                    }
                }
            }
        }

        public static void WriteObj(string fileName, Level level, WriterLevelSettings settings)
        {
            switch (settings.mode)
            {
                case WriterLevelMode.Separate:
                    WriteObjSeparate(fileName, level, settings);
                    return;
                case WriterLevelMode.Combined:
                    WriteObjCombined(fileName, level, settings);
                    return;
                case WriterLevelMode.Typewise:
                    WriteObjTypewise(fileName, level, settings);
                    return;
                case WriterLevelMode.Materialwise:
                    WriteObjMaterialwise(fileName, level, settings);
                    return;
            }
        }

        /*
         * Different Modes for Level Export:
         * - Separate: Exports every ModelObject as is
         * - Combined: Combines all ModelObjects into one mesh
         * - Typewise: Combines all ModelObjects of the same type (Tie, Shrubs etc.) into one mesh
         * - Materialwise: Combines all faces using the same material/texture into one mesh
         */
        public enum WriterLevelMode
        {
            Separate,
            Combined,
            Typewise,
            Materialwise
        };

        public class WriterLevelSettings
        {
            public WriterLevelMode mode = WriterLevelMode.Combined;
            public bool writeTies = true;
            public bool writeShrubs = true;
            public bool writeMobies = true;
            public bool[] chunksSelected = new bool[5];
            public bool exportMTLFile = true;

            public WriterLevelSettings()
            {
                for (int i = 0; i < 5; i++)
                {
                    chunksSelected[i] = false;
                }
            }
        }
    }
}
