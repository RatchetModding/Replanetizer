// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Globalization;

namespace LibReplanetizer
{
    public static class ModelWriter
    {
        private static readonly CultureInfo en_US = CultureInfo.CreateSpecificCulture("en-US");
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static void WriteObjectMaterial(StreamWriter mtlfs, string id)
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

        private static void WriteObjectMaterial(StreamWriter mtlfs, Model model, List<int> usedMtls)
        {
            for (int i = 0; i < model.textureConfig.Count; i++)
            {
                int modelTextureID = model.textureConfig[i].id;
                if (usedMtls.Contains(modelTextureID))
                    continue;
                WriteObjectMaterial(mtlfs, modelTextureID.ToString());
                usedMtls.Add(modelTextureID);
            }
        }

        private static int WriteObjectData(StreamWriter objfs, Model model, int faceOffset, Matrix4 modelMatrix)
        {
            var vertexCount = model.vertexBuffer.Length / 8;

            // Vertices
            var vertices = new Vector3[vertexCount];
            for (int vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var px = model.vertexBuffer[vertIdx * 0x08 + 0x0] * model.size;
                var py = model.vertexBuffer[vertIdx * 0x08 + 0x1] * model.size;
                var pz = model.vertexBuffer[vertIdx * 0x08 + 0x2] * model.size;
                var pos = new Vector4(px, py, pz, 1.0f) * modelMatrix;
                vertices[vertIdx] = pos.Xyz;
                objfs.WriteLine($"v {pos.X:F6} {pos.Y:F6} {pos.Z:F6}");
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
                objfs.WriteLine($"vn {normal.X:F6} {normal.Y:F6} {normal.Z:F6}");
            }

            // UVs
            for (var vertIdx = 0; vertIdx < vertexCount; vertIdx++)
            {
                var tu = model.vertexBuffer[(vertIdx * 0x08) + 0x6];
                var tv = 1f - model.vertexBuffer[(vertIdx * 0x08) + 0x7];
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

                objfs.WriteLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
            }

            return vertexCount;
        }

        /// <summary>
        /// Average a tri's vertex normals to get the target face normal, then
        /// check whether the current winding order yields a normal facing
        /// the target face normal
        /// </summary>
        /// <returns>whether to reverse the winding order</returns>
        private static bool ShouldReverseWinding(
            IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3> vertexNormals, int v1, int v2, int v3)
        {
            var targetFaceNormal = FaceNormalFromVertexNormals(vertexNormals, v1, v2, v3);
            var p1 = vertices[v1];
            var p2 = vertices[v2];
            var p3 = vertices[v3];
            return ShouldReverseWinding(p1, p2, p3, targetFaceNormal);
        }

        private static bool ShouldReverseWinding(
            Vector3 p1, Vector3 p2, Vector3 p3, Vector3 targetFaceNormal)
        {
            p2 -= p1;
            p3 -= p1;
            var faceNormal = Vector3.Cross(p2, p3);
            var dot = Vector3.Dot(faceNormal, targetFaceNormal);
            return dot < 0f;
        }

        private static Vector3 FaceNormalFromVertexNormals(
            IReadOnlyList<Vector3> normals, int v1, int v2, int v3)
        {
            var n1 = normals[v1];
            var n2 = normals[v2];
            var n3 = normals[v3];
            return (n1 + n2 + n3) / 3;
        }

        private static void WriteSkeletonDae(StreamWriter colladaStream, Skeleton skeleton, float size, string indent = "")
        {
            Matrix3x4 mat = skeleton.bone.transformation;

            colladaStream.WriteLine(indent + "<node id=\"Skel" + skeleton.bone.id.ToString() + "\" sid=\"J" + skeleton.bone.id.ToString() + "\" name=\"Skel" + skeleton.bone.id.ToString() + "\" type=\"JOINT\">");
            // It is unclear which version is correct, I think the first one but both make no difference in blender
            /*colladaStream.Write(indent + "\t<rotate sid=\"jointOrientZ\">");
            colladaStream.Write((mat.M31).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M32).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M33).ToString("G", en_US) + " ");
            colladaStream.Write((0.0f).ToString("G", en_US) + " ");
            colladaStream.WriteLine("</rotate>");
            colladaStream.Write(indent + "\t<rotate sid=\"jointOrientY\">");
            colladaStream.Write((mat.M21).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M22).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M23).ToString("G", en_US) + " ");
            colladaStream.Write((0.0f).ToString("G", en_US) + " ");
            colladaStream.WriteLine("</rotate>");
            colladaStream.Write(indent + "\t<rotate sid=\"jointOrientX\">");
            colladaStream.Write((mat.M11).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M12).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M13).ToString("G", en_US) + " ");
            colladaStream.Write((0.0f).ToString("G", en_US) + " ");
            colladaStream.WriteLine("</rotate>");
            colladaStream.Write(indent + "\t<translate sid=\"translate\">");
            colladaStream.Write((mat.M14 * size / 1024.0f).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M24 * size / 1024.0f).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M34 * size / 1024.0f).ToString("G", en_US) + " ");
            colladaStream.WriteLine("</translate>");*/
            colladaStream.Write(indent + "\t<matrix sid=\"transform\">");
            colladaStream.Write((mat.M11).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M12).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M13).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M14 * size / 1024.0f).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M21).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M22).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M23).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M24 * size / 1024.0f).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M31).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M32).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M33).ToString("G", en_US) + " ");
            colladaStream.Write((mat.M34 * size / 1024.0f).ToString("G", en_US) + " ");
            colladaStream.Write("0 ");
            colladaStream.Write("0 ");
            colladaStream.Write("0 ");
            colladaStream.Write("1 ");
            colladaStream.WriteLine("</matrix>");

            colladaStream.WriteLine(indent + "<extra>");
            colladaStream.WriteLine(indent + "\t<technique profile=\"blender\">");
            colladaStream.WriteLine(indent + "\t\t<connect>1</connect>");
            colladaStream.WriteLine(indent + "\t\t<layer>0</layer>");
            colladaStream.WriteLine(indent + "\t\t<roll>0</roll>");
            colladaStream.WriteLine(indent + "\t\t<tip_x>" + (mat.M14 * size / 1024.0f).ToString("G", en_US) + "</tip_x>");
            colladaStream.WriteLine(indent + "\t\t<tip_y>" + (mat.M24 * size / 1024.0f).ToString("G", en_US) + "</tip_y>");
            colladaStream.WriteLine(indent + "\t\t<tip_z>" + (mat.M34 * size / 1024.0f).ToString("G", en_US) + "</tip_z>");
            colladaStream.WriteLine(indent + "\t</technique>");
            colladaStream.WriteLine(indent + "</extra>");

            foreach (Skeleton child in skeleton.children)
            {
                WriteSkeletonDae(colladaStream, child, size, indent + "\t");
            }

            colladaStream.WriteLine(indent + "</node>");
        }

        public static void WriteDae(string fileName, Level level, Model model)
        {
            LOGGER.Trace(fileName);

            string? filePath = Path.GetDirectoryName(fileName);

            using (StreamWriter colladaStream = new StreamWriter(fileName))
            {
                int vertexCount = model.vertexBuffer.Length / 8;

                colladaStream.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                colladaStream.WriteLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">");

                //metadata
                colladaStream.WriteLine("\t<asset>");
                colladaStream.WriteLine("\t\t<contributor>");
                colladaStream.WriteLine("\t\t\t<author>Replanetizer User</author>");
                colladaStream.WriteLine("\t\t\t<authoring_tool>Replanetizer</authoring_tool>");
                colladaStream.WriteLine("\t\t</contributor>");
                colladaStream.WriteLine("\t\t<created>" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "</created>");
                colladaStream.WriteLine("\t\t<modified>" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "</modified>");
                colladaStream.WriteLine("\t\t<unit name=\"meter\" meter=\"1\"/>");
                colladaStream.WriteLine("\t\t<up_axis>Z_UP</up_axis>");
                colladaStream.WriteLine("\t</asset>");

                //image
                colladaStream.WriteLine("\t<library_images>");
                foreach (TextureConfig config in model.textureConfig)
                {
                    colladaStream.WriteLine("\t\t<image id=\"texture_" + config.id + "\">");
                    colladaStream.Write("\t\t\t<init_from>");
                    colladaStream.Write(config.id + ".png");
                    colladaStream.WriteLine("</init_from>");
                    colladaStream.WriteLine("\t\t</image>");
                }
                colladaStream.WriteLine("\t</library_images>");

                //effects
                colladaStream.WriteLine("\t<library_effects>");
                foreach (TextureConfig config in model.textureConfig)
                {
                    colladaStream.WriteLine("\t\t<effect id=\"effect_" + config.id + "\">");
                    colladaStream.WriteLine("\t\t\t<profile_COMMON>");
                    colladaStream.WriteLine("\t\t\t\t<newparam sid=\"surface_" + config.id + "\">");
                    colladaStream.WriteLine("\t\t\t\t\t<surface type=\"2D\">");
                    colladaStream.WriteLine("\t\t\t\t\t\t<init_from>texture_" + config.id + "</init_from>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<format>A8R8G8B8</format>");
                    colladaStream.WriteLine("\t\t\t\t\t</surface>");
                    colladaStream.WriteLine("\t\t\t\t</newparam>");
                    colladaStream.WriteLine("\t\t\t\t<newparam sid=\"sampler_" + config.id + "\">");
                    colladaStream.WriteLine("\t\t\t\t\t<sampler2D>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<source>surface_" + config.id + "</source>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<minfilter>LINEAR_MIPMAP_LINEAR</minfilter>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<magfilter>LINEAR</magfilter>");
                    colladaStream.WriteLine("\t\t\t\t\t</sampler2D>");
                    colladaStream.WriteLine("\t\t\t\t</newparam>");
                    colladaStream.WriteLine("\t\t\t\t<technique sid=\"common\">");
                    colladaStream.WriteLine("\t\t\t\t\t<lambert>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<diffuse>");
                    colladaStream.WriteLine("\t\t\t\t\t\t\t<texture texture=\"sampler_" + config.id + "\" texcoord=\"texcoord_" + config.id + "\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t\t</diffuse>");
                    colladaStream.WriteLine("\t\t\t\t\t</lambert>");
                    colladaStream.WriteLine("\t\t\t\t</technique>");
                    colladaStream.WriteLine("\t\t\t</profile_COMMON>");
                    colladaStream.WriteLine("\t\t</effect>");
                }
                colladaStream.WriteLine("\t</library_effects>");

                //materials
                colladaStream.WriteLine("\t<library_materials>");
                foreach (TextureConfig config in model.textureConfig)
                {
                    colladaStream.WriteLine("\t\t<material id=\"material_" + config.id + "\">");
                    colladaStream.WriteLine("\t\t\t<instance_effect url=\"#effect_" + config.id + "\"/>");
                    colladaStream.WriteLine("\t\t</material>");
                }
                colladaStream.WriteLine("\t</library_materials>");

                //geometry
                colladaStream.WriteLine("\t<library_geometries>");
                colladaStream.WriteLine("\t\t<geometry id=\"Model\">");
                colladaStream.WriteLine("\t\t\t<mesh>");
                colladaStream.WriteLine("\t\t\t\t<source id=\"Model_positions\">");
                colladaStream.Write("\t\t\t\t\t<float_array id=\"Model_positions_array\" count=\"" + 3 * vertexCount + "\"> ");
                Vector3[] vertices = new Vector3[vertexCount];
                for (int x = 0; x < vertexCount; x++)
                {
                    float px = model.vertexBuffer[(x * 0x08) + 0x0] * model.size;
                    float py = model.vertexBuffer[(x * 0x08) + 0x1] * model.size;
                    float pz = model.vertexBuffer[(x * 0x08) + 0x2] * model.size;
                    vertices[x] = new Vector3(px, py, pz);
                    colladaStream.Write(px.ToString("G", en_US) + " " + py.ToString("G", en_US) + " " + pz.ToString("G", en_US) + " ");
                }
                colladaStream.WriteLine("</float_array>");
                colladaStream.WriteLine("\t\t\t\t\t<technique_common>");
                colladaStream.WriteLine("\t\t\t\t\t\t<accessor count=\"" + vertexCount + "\" offset=\"0\" source=\"#Model_positions_array\" stride=\"3\">");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"X\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"Y\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"Z\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t</accessor>");
                colladaStream.WriteLine("\t\t\t\t\t</technique_common>");
                colladaStream.WriteLine("\t\t\t\t</source>");
                colladaStream.WriteLine("\t\t\t\t<source id=\"Model_normals\">");
                colladaStream.Write("\t\t\t\t\t<float_array id=\"Model_normals_array\" count=\"" + 3 * vertexCount + "\"> ");
                Vector3[] normals = new Vector3[vertexCount];
                for (int x = 0; x < vertexCount; x++)
                {
                    float nx = model.vertexBuffer[(x * 0x08) + 0x3];
                    float ny = model.vertexBuffer[(x * 0x08) + 0x4];
                    float nz = model.vertexBuffer[(x * 0x08) + 0x5];
                    normals[x] = new Vector3(nx, ny, nz);
                    colladaStream.Write(nx.ToString("G", en_US) + " " + ny.ToString("G", en_US) + " " + nz.ToString("G", en_US) + " ");
                }
                colladaStream.WriteLine("</float_array>");
                colladaStream.WriteLine("\t\t\t\t\t<technique_common>");
                colladaStream.WriteLine("\t\t\t\t\t\t<accessor count=\"" + vertexCount + "\" offset=\"0\" source=\"#Model_normals_array\" stride=\"3\">");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"X\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"Y\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"Z\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t</accessor>");
                colladaStream.WriteLine("\t\t\t\t\t</technique_common>");
                colladaStream.WriteLine("\t\t\t\t</source>");
                colladaStream.WriteLine("\t\t\t\t<source id=\"Model_uvs\">");
                colladaStream.Write("\t\t\t\t\t<float_array id=\"Model_uvs_array\" count=\"" + 2 * vertexCount + "\"> ");
                for (int x = 0; x < vertexCount; x++)
                {
                    float tu = model.vertexBuffer[(x * 0x08) + 0x6];
                    float tv = 1.0f - model.vertexBuffer[(x * 0x08) + 0x7];
                    colladaStream.Write(tu.ToString("G", en_US) + " " + tv.ToString("G", en_US) + " ");
                }
                colladaStream.WriteLine("</float_array>");
                colladaStream.WriteLine("\t\t\t\t\t<technique_common>");
                colladaStream.WriteLine("\t\t\t\t\t\t<accessor count=\"" + vertexCount + "\" offset=\"0\" source=\"#Model_uvs_array\" stride=\"2\">");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"S\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"T\" type=\"float\"/>");
                colladaStream.WriteLine("\t\t\t\t\t\t</accessor>");
                colladaStream.WriteLine("\t\t\t\t\t</technique_common>");
                colladaStream.WriteLine("\t\t\t\t</source>");
                colladaStream.WriteLine("\t\t\t\t<vertices id=\"Model_vertices\">");
                colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"POSITION\" source=\"#Model_positions\"/>");
                colladaStream.WriteLine("\t\t\t\t</vertices>");
                foreach (TextureConfig config in model.textureConfig)
                {
                    colladaStream.WriteLine("\t\t\t\t<triangles count=\"" + config.size / 3 + "\" material=\"material_symbol_" + config.id + "\">");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"VERTEX\" source=\"#Model_vertices\" offset=\"0\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"NORMAL\" source=\"#Model_normals\" offset=\"0\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"TEXCOORD\" source=\"#Model_uvs\" offset=\"0\" set=\"0\"/>");
                    colladaStream.Write("\t\t\t\t\t<p> ");
                    for (int i = config.start / 3; i < config.start / 3 + config.size / 3; i++)
                    {
                        int f1 = model.indexBuffer[i * 3 + 0];
                        int f2 = model.indexBuffer[i * 3 + 1];
                        int f3 = model.indexBuffer[i * 3 + 2];

                        if (ShouldReverseWinding(vertices, normals, f1, f2, f3))
                            (f2, f3) = (f3, f2);

                        colladaStream.Write(f1 + " " + f2 + " " + f3 + " ");
                    }
                    colladaStream.WriteLine("</p>");
                    colladaStream.WriteLine("\t\t\t\t</triangles>");
                }
                colladaStream.WriteLine("\t\t\t</mesh>");
                colladaStream.WriteLine("\t\t</geometry>");
                colladaStream.WriteLine("\t</library_geometries>");

                if (model is MobyModel)
                {
                    MobyModel moby = (MobyModel) model;

                    //controllers
                    colladaStream.WriteLine("\t<library_controllers>");
                    colladaStream.WriteLine("\t\t<controller id=\"Armature\" name=\"Armature\">");
                    colladaStream.WriteLine("\t\t\t<skin source=\"#Model\">");
                    colladaStream.WriteLine("\t\t\t\t<source id=\"Joints\">");
                    colladaStream.Write("\t\t\t\t\t<Name_array id=\"JointsArray\" count=\"" + moby.boneDatas.Count + "\">");
                    for (int i = 0; i < moby.boneCount; i++)
                    {
                        colladaStream.Write("J" + moby.boneMatrices[i].id.ToString() + " ");
                    }
                    colladaStream.WriteLine("\t\t\t\t\t</Name_array>");
                    colladaStream.WriteLine("\t\t\t\t\t<technique_common>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<accessor source=\"#JointsArray\" count=\"" + moby.boneDatas.Count + "\" stride=\"1\">");
                    colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"JOINT\" type=\"Name\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t\t</accessor>");
                    colladaStream.WriteLine("\t\t\t\t\t</technique_common>");
                    colladaStream.WriteLine("\t\t\t\t</source>");
                    colladaStream.WriteLine("\t\t\t\t<source id=\"Weights\">");
                    colladaStream.Write("\t\t\t\t\t<float_array id=\"WeightsArray\" count=\"" + vertexCount * 4 + "\">");
                    for (int i = 0; i < vertexCount; i++)
                    {
                        byte[] vWeights = BitConverter.GetBytes(model.weights[i]);
                        float a = vWeights[0x00] / 255.0f;
                        float b = vWeights[0x01] / 255.0f;
                        float c = vWeights[0x02] / 255.0f;
                        float d = vWeights[0x03] / 255.0f;

                        colladaStream.Write(a.ToString("G", en_US) + " " + b.ToString("G", en_US) + " " + c.ToString("G", en_US) + " " + d.ToString("G", en_US) + " ");
                    }
                    colladaStream.WriteLine("</float_array>");
                    colladaStream.WriteLine("\t\t\t\t\t<technique_common>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<accessor source=\"#WeightsArray\" count=\"" + 4 * vertexCount + "\" stride=\"1\">");
                    colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"WEIGHT\" type=\"float\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t\t</accessor>");
                    colladaStream.WriteLine("\t\t\t\t\t</technique_common>");
                    colladaStream.WriteLine("\t\t\t\t</source>");
                    colladaStream.WriteLine("\t\t\t\t<source id=\"InvBindMats\">");
                    colladaStream.Write("\t\t\t\t\t<float_array id=\"InvBindMatsArray\" count=\"" + 16 * moby.boneMatrices.Count + "\">");
                    for (int i = 0; i < moby.boneMatrices.Count; i++)
                    {
                        colladaStream.Write("1.0 ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write((moby.boneMatrices[i].cumulativeOffsetX * model.size / 1024f).ToString("G", en_US) + " ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("1.0 ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write((moby.boneMatrices[i].cumulativeOffsetY * model.size / 1024f).ToString("G", en_US) + " ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("1.0 ");
                        colladaStream.Write((moby.boneMatrices[i].cumulativeOffsetZ * model.size / 1024f).ToString("G", en_US) + " ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("0.0 ");
                        colladaStream.Write("1.0 ");
                    }
                    colladaStream.WriteLine("\t\t\t\t\t</float_array>");
                    colladaStream.WriteLine("\t\t\t\t\t<technique_common>");
                    colladaStream.WriteLine("\t\t\t\t\t\t<accessor source=\"#InvBindMatsArray\" count=\"" + moby.boneMatrices.Count + "\" stride=\"16\">");
                    colladaStream.WriteLine("\t\t\t\t\t\t\t<param name=\"TRANSFORM\" type=\"float4x4\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t\t</accessor>");
                    colladaStream.WriteLine("\t\t\t\t\t</technique_common>");
                    colladaStream.WriteLine("\t\t\t\t</source>");
                    colladaStream.WriteLine("\t\t\t\t<joints>");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"JOINT\" source=\"#Joints\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"INV_BIND_MATRIX\" source=\"#InvBindMats\"/>");
                    colladaStream.WriteLine("\t\t\t\t</joints>");
                    colladaStream.WriteLine("\t\t\t\t<vertex_weights count=\"" + vertexCount + "\">");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"JOINT\" source=\"#Joints\" offset=\"0\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t<input semantic=\"WEIGHT\" source=\"#Weights\" offset=\"1\"/>");
                    colladaStream.Write("\t\t\t\t\t<vcount>");
                    for (int i = 0; i < vertexCount; i++)
                    {
                        colladaStream.Write("4 ");
                    }
                    colladaStream.WriteLine("</vcount>");
                    colladaStream.Write("\t\t\t\t\t<v>");
                    for (int i = 0; i < vertexCount; i++)
                    {
                        Byte[] indices = BitConverter.GetBytes(model.ids[i]);

                        colladaStream.Write(indices[0x00] + " " + (i * 0x04 + 0x00) + " ");
                        colladaStream.Write(indices[0x01] + " " + (i * 0x04 + 0x01) + " ");
                        colladaStream.Write(indices[0x02] + " " + (i * 0x04 + 0x02) + " ");
                        colladaStream.Write(indices[0x03] + " " + (i * 0x04 + 0x03) + " ");
                    }
                    colladaStream.WriteLine("</v>");
                    colladaStream.WriteLine("\t\t\t\t</vertex_weights>");
                    colladaStream.WriteLine("\t\t\t</skin>");
                    colladaStream.WriteLine("\t\t</controller>");
                    colladaStream.WriteLine("\t</library_controllers>");

                    //animation
                    // Good luck getting this to work :P
                    /*colladaStream.WriteLine("\t<library_animations>");
                    for (int i = 0; i < moby.animations.Count; i++)
                    {
                        Animation anim = (moby.id == 0) ? level.playerAnimations[i] : moby.animations[i];
                        for (int k = 0; k < moby.boneCount; k++)
                        {
                            colladaStream.WriteLine("\t\t<animation id=\"Anim" + i.ToString() + "_" + k.ToString() + "\" name=\"Anim" + i.ToString() + "\">");
                            colladaStream.WriteLine("\t\t\t<source id=\"Anim" + i.ToString() + "_" + k.ToString() + "Input\">");
                            colladaStream.Write("\t\t\t\t<float_array id=\"Anim" + i.ToString() + "_" + k.ToString() + "InputArray\" count=\"" + anim.frames.Count + "\">");
                            for (int j = 0; j < anim.frames.Count; j++)
                            {
                                colladaStream.Write((j / (60f * anim.speed)).ToString("G", en_US) + " ");
                            }
                            colladaStream.WriteLine("</float_array>");
                            colladaStream.WriteLine("\t\t\t\t<technique_common>");
                            colladaStream.WriteLine("\t\t\t\t\t<accessor source=\"#Anim" + i.ToString() + "_" + k.ToString() + "InputArray\" count=\"" + anim.frames.Count + "\" stride=\"1\">");
                            colladaStream.WriteLine("\t\t\t\t\t\t<param name=\"TIME\" type=\"float\"/>");
                            colladaStream.WriteLine("\t\t\t\t\t</accessor>");
                            colladaStream.WriteLine("\t\t\t\t</technique_common>");
                            colladaStream.WriteLine("\t\t\t</source>");
                            colladaStream.WriteLine("\t\t\t<source id=\"Anim" + i.ToString() + "_" + k.ToString() + "Output\">");
                            colladaStream.Write("\t\t\t\t<float_array id=\"Anim" + i.ToString() + "_" + k.ToString() + "OutputArray\" count=\"" + 16 * anim.frames.Count + "\">");
                            for (int j = 0; j < anim.frames.Count; j++)
                            {
                                Frame frame = anim.frames[j];
                                short[] rots = frame.rotations[k];

                                Quaternion quat = new Quaternion((rots[0] / 32767f) * 180f, (rots[1] / 32767f) * 180f, (rots[2] / 32767f) * 180f, (-rots[3] / 32767f) * 180f);

                                Matrix4 rotation = Matrix4.CreateFromQuaternion(quat);
                                Matrix4 animationMatrix = rotation;

                                colladaStream.Write((animationMatrix.M11).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M12).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M13).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M14).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M21).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M22).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M23).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M24).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M31).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M32).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M33).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M34).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M41).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M42).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M43).ToString("G", en_US) + " ");
                                colladaStream.Write((animationMatrix.M44).ToString("G", en_US) + " ");
                            }
                            colladaStream.WriteLine("</float_array>");
                            colladaStream.WriteLine("\t\t\t\t<technique_common>");
                            colladaStream.WriteLine("\t\t\t\t\t<accessor source=\"#Anim" + i.ToString() + "_" + k.ToString() + "OutputArray\" count=\"" + anim.frames.Count + "\" stride=\"16\">");
                            colladaStream.WriteLine("\t\t\t\t\t\t<param name=\"TRANSFORM\" type=\"float4x4\"/>");
                            colladaStream.WriteLine("\t\t\t\t\t</accessor>");
                            colladaStream.WriteLine("\t\t\t\t</technique_common>");
                            colladaStream.WriteLine("\t\t\t</source>");
                            colladaStream.WriteLine("\t\t\t<source id=\"Anim" + i.ToString() + "_" + k.ToString() + "Interp\">");
                            colladaStream.Write("\t\t\t\t<Name_array id=\"Anim" + i.ToString() + "_" + k.ToString() + "InterpArray\" count=\"" + anim.frames.Count + "\">");
                            for (int j = 0; j < anim.frames.Count; j++)
                            {
                                colladaStream.Write("LINEAR ");
                            }
                            colladaStream.WriteLine("</Name_array>");
                            colladaStream.WriteLine("\t\t\t\t<technique_common>");
                            colladaStream.WriteLine("\t\t\t\t\t<accessor source=\"#Anim" + i.ToString() + "_" + k.ToString() + "InterpArray\" count=\"" + anim.frames.Count + "\" stride=\"1\">");
                            colladaStream.WriteLine("\t\t\t\t\t\t<param name=\"INTERPOLATION\" type=\"Name\"/>");
                            colladaStream.WriteLine("\t\t\t\t\t</accessor>");
                            colladaStream.WriteLine("\t\t\t\t</technique_common>");
                            colladaStream.WriteLine("\t\t\t</source>");
                            colladaStream.WriteLine("\t\t\t<sampler id=\"Anim" + i.ToString() + "_" + k.ToString() + "Sampler\">");
                            colladaStream.WriteLine("\t\t\t\t<input semantic=\"INPUT\" source=\"#Anim" + i.ToString() + "_" + k.ToString() + "Input\"/>");
                            colladaStream.WriteLine("\t\t\t\t<input semantic=\"OUTPUT\" source=\"#Anim" + i.ToString() + "_" + k.ToString() + "Output\"/>");
                            colladaStream.WriteLine("\t\t\t\t<input semantic=\"INTERPOLATION\" source=\"#Anim" + i.ToString() + "_" + k.ToString() + "Interp\"/>");
                            colladaStream.WriteLine("\t\t\t</sampler>");
                            colladaStream.WriteLine("\t\t\t<channel source=\"Anim" + i.ToString() + "_" + k.ToString() + "Sampler\" target=\"VSJ" + k.ToString() + "/transform\"/>");
                            colladaStream.WriteLine("\t\t</animation>");
                        }
                    }
                    colladaStream.WriteLine("\t</library_animations>");*/
                }

                //scene
                colladaStream.WriteLine("\t<library_visual_scenes>");
                colladaStream.WriteLine("\t\t<visual_scene id=\"Scene\" name=\"Scene\">");
                if (model is MobyModel)
                {
                    MobyModel moby = (MobyModel) model;

                    if (moby.skeleton != null)
                        WriteSkeletonDae(colladaStream, moby.skeleton, model.size, "\t\t\t");
                }
                colladaStream.WriteLine("\t\t\t<node id=\"Object\" name=\"Object\" type=\"NODE\">");
                colladaStream.WriteLine("\t\t\t\t<matrix sid=\"transform\">1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</matrix>");
                if (model is MobyModel)
                {
                    colladaStream.WriteLine("\t\t\t\t<instance_controller url=\"#Armature\" name=\"Armature\">");
                    colladaStream.WriteLine("\t\t\t\t\t<skeleton>#Skel0</skeleton>");
                }
                else
                {
                    colladaStream.WriteLine("\t\t\t\t<instance_geometry url=\"#Model\" name=\"Model\">");
                }
                colladaStream.WriteLine("\t\t\t\t\t<bind_material>");
                colladaStream.WriteLine("\t\t\t\t\t\t<technique_common>");
                foreach (TextureConfig config in model.textureConfig)
                {
                    colladaStream.WriteLine("\t\t\t\t\t\t\t<instance_material symbol=\"material_symbol_" + config.id + "\" target=\"#material_" + config.id + "\">");
                    colladaStream.WriteLine("\t\t\t\t\t\t\t\t<bind_vertex_input semantic=\"texcoord_" + config.id + "\" input_semantic=\"TEXCOORD\" input_set=\"0\"/>");
                    colladaStream.WriteLine("\t\t\t\t\t\t\t</instance_material>");
                }
                colladaStream.WriteLine("\t\t\t\t\t\t</technique_common>");
                colladaStream.WriteLine("\t\t\t\t\t</bind_material>");
                if (model is MobyModel)
                {
                    colladaStream.WriteLine("\t\t\t\t</instance_controller>");
                }
                else
                {
                    colladaStream.WriteLine("\t\t\t\t</instance_geometry>");
                }
                colladaStream.WriteLine("\t\t\t</node>");
                colladaStream.WriteLine("\t\t</visual_scene>");
                colladaStream.WriteLine("\t</library_visual_scenes>");

                colladaStream.WriteLine("</COLLADA>");
            }
        }

        public static void WriteIqe(string fileName, Level level, Model model)
        {
            LOGGER.Trace(fileName);

            string? filePath = Path.GetDirectoryName(fileName);

            if (!(model is MobyModel mobyModel)) return;

            using (StreamWriter spookyStream = new StreamWriter(fileName))
            {
                spookyStream.WriteLine("# Inter-Quake Export");

                // Binding pose
                for (int i = 0; i < mobyModel.boneDatas.Count; i++)
                {
                    float xx = mobyModel.boneDatas[i].translationX / 1024f;
                    float yy = mobyModel.boneDatas[i].translationY / 1024f;
                    float zz = mobyModel.boneDatas[i].translationZ / 1024f;

                    short par = (short) (mobyModel.boneMatrices[i].parent / 0x40);
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

                            float xx = mobyModel.boneDatas[idx].translationX / 1024f;
                            float yy = mobyModel.boneDatas[idx].translationY / 1024f;
                            float zz = mobyModel.boneDatas[idx].translationZ / 1024f;

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
                            spookyStream.WriteLine("mesh " + model.textureConfig[tCnt].id.ToString(""));
                            if (model.textureConfig[tCnt].id != -1)
                            {
                                spookyStream.WriteLine("material " + model.textureConfig[tCnt].id.ToString("x") + ".png");
                                Bitmap bump = level.textures[model.textureConfig[tCnt].id].GetTextureImage();
                                bump.Save(filePath + "/" + model.textureConfig[tCnt].id.ToString("x") + ".png");
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
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            using (StreamWriter mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl"))
            {
                // List used mtls to prevent it from making duplicate entries
                List<int> usedMtls = new List<int>();
                WriteObjectMaterial(mtlfs, model, usedMtls);
            }

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                objfs.WriteLine("o Object_" + model.id.ToString("X4"));
                if (model.textureConfig != null)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                WriteObjectData(objfs, model, 0, Matrix4.Identity);
            }
        }

        private static List<TerrainFragment> CollectTerrainFragments(Level level, WriterLevelSettings settings)
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
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level, settings);

            StreamWriter mtlfs = null;

            if (settings.exportMtlFile) mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                if (settings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                foreach (TerrainFragment t in terrain)
                {
                    objfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                    faceOffset += WriteObjectData(objfs, t.model, faceOffset, Matrix4.Identity);
                    if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                }

                if (settings.writeTies)
                {
                    foreach (Tie t in level.ties)
                    {
                        objfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (settings.writeShrubs)
                {
                    foreach (Shrub t in level.shrubs)
                    {
                        objfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (settings.writeMobies)
                {
                    foreach (Moby t in level.mobs)
                    {
                        objfs.WriteLine("o Object_" + t.model.id.ToString("X4"));
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }
            }

            if (settings.exportMtlFile) mtlfs.Dispose();
        }

        private static void WriteObjCombined(string fileName, Level level, WriterLevelSettings settings)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level, settings);

            StreamWriter mtlfs = null;

            if (settings.exportMtlFile) mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (settings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                objfs.WriteLine("o Object_CombinedLevel");
                foreach (TerrainFragment t in terrain)
                {
                    faceOffset += WriteObjectData(objfs, t.model, faceOffset, Matrix4.Identity);
                    if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                }

                if (settings.writeTies)
                {
                    foreach (Tie t in level.ties)
                    {
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (settings.writeShrubs)
                {
                    foreach (Shrub t in level.shrubs)
                    {
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (settings.writeMobies)
                {
                    foreach (Moby t in level.mobs)
                    {
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }
            }

            if (settings.exportMtlFile) mtlfs.Dispose();
        }

        private static void WriteObjTypewise(string fileName, Level level, WriterLevelSettings settings)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level, settings);

            StreamWriter mtlfs = null;

            if (settings.exportMtlFile) mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl");

            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (settings.exportMtlFile)
                    objfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");

                int faceOffset = 0;
                List<int> usedMtls = new List<int>();

                if (terrain.Count != 0)
                {
                    objfs.WriteLine("o Object_Terrain");
                }

                foreach (TerrainFragment t in terrain)
                {
                    faceOffset += WriteObjectData(objfs, t.model, faceOffset, Matrix4.Identity);
                    if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                }

                if (settings.writeTies)
                {
                    objfs.WriteLine("o Object_Ties");

                    foreach (Tie t in level.ties)
                    {
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (settings.writeShrubs)
                {
                    objfs.WriteLine("o Object_Shrubs");

                    foreach (Shrub t in level.shrubs)
                    {
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }

                if (settings.writeMobies)
                {
                    objfs.WriteLine("o Object_Mobies");

                    foreach (Moby t in level.mobs)
                    {
                        faceOffset += WriteObjectData(objfs, t.model, faceOffset, t.modelMatrix);
                        if (settings.exportMtlFile) WriteObjectMaterial(mtlfs, t.model, usedMtls);
                    }
                }
            }

            if (settings.exportMtlFile) mtlfs.Dispose();
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

        private static void WriteObjMaterialwise(string fileName, Level level, WriterLevelSettings settings)
        {
            string? pathName = Path.GetDirectoryName(fileName);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);

            List<TerrainFragment> terrain = CollectTerrainFragments(level, settings);

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

            if (settings.exportMtlFile)
            {
                using (StreamWriter mtlfs = new StreamWriter(pathName + "\\" + fileNameNoExtension + ".mtl"))
                {
                    for (int i = 0; i < materialCount; i++)
                    {
                        if (faces[i].Count != 0)
                            WriteObjectMaterial(mtlfs, i.ToString());
                    }
                }
            }


            using (StreamWriter objfs = new StreamWriter(fileName))
            {
                if (settings.exportMtlFile)
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

                    if (settings.exportMtlFile)
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
            public bool exportMtlFile = true;

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
