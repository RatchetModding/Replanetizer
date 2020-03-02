using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using RatchetEdit.Models;
using RatchetEdit.Models.Animations;

namespace RatchetEdit
{
    public static class ModelWriter
    {
        public static void WriteIqe(string fileName, Level level, Model model)
        {
            Console.WriteLine(fileName);

            string filePath = Path.GetDirectoryName(fileName);

            if (!(model is MobyModel mobyModel)) return;

            using(StreamWriter spookyStream = new StreamWriter(fileName))
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

                    short par = (short)(mobyModel.boneMatrices[i].bb / 0x40);
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

                for (int i = 0; i < model.textureConfig.Count; i++)
                {
                    int modelTextureID = model.textureConfig[0].ID;
                    if (!usedMtls.Contains(modelTextureID))
                    {
                        MTLfs.WriteLine("newmtl mtl_" + modelTextureID);
                        MTLfs.WriteLine("Ns 1000");
                        MTLfs.WriteLine("Ka 1.000000 1.000000 1.000000");
                        MTLfs.WriteLine("Kd 1.000000 1.000000 1.000000");
                        MTLfs.WriteLine("Ni 1.000000");
                        MTLfs.WriteLine("d 1.000000");
                        MTLfs.WriteLine("illum 1");
                        MTLfs.WriteLine("map_Kd tex_" + model.textureConfig[i].ID + ".png");
                        usedMtls.Add(modelTextureID);
                    }
                }
            }

            using(StreamWriter OBJfs = new StreamWriter(fileName))
            {
                OBJfs.WriteLine("o Object_" + model.id.ToString("X4"));
                if (model.textureConfig != null)
                    OBJfs.WriteLine("mtllib " + fileNameNoExtension + ".mtl");
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
                    float tv = 1f - model.vertexBuffer[(x * 0x08) + 0x7];
                    OBJfs.WriteLine("v " + px.ToString("G") + " " + py.ToString("G") + " " + pz.ToString("G"));
                    OBJfs.WriteLine("vn " + nx.ToString("G") + " " + ny.ToString("G") + " " + nz.ToString("G"));
                    OBJfs.WriteLine("vt " + tu.ToString("G") + " " + tv.ToString("G"));
                }


                //Faces
                int textureNum = 0;
                for (int i = 0; i < model.indexBuffer.Length / 3; i++)
                {
                    int triIndex = i * 3;
                    if ((model.textureConfig != null) && (textureNum < model.textureConfig.Count) && (triIndex >= model.textureConfig[textureNum].start))
                    {
                        string modelId = model.textureConfig[textureNum].ID.ToString();
                        OBJfs.WriteLine("usemtl mtl_" + modelId);
                        OBJfs.WriteLine("g Texture_" + modelId);
                        textureNum++;
                    }

                    int f1 = model.indexBuffer[triIndex + 0] + 1;
                    int f2 = model.indexBuffer[triIndex + 1] + 1;
                    int f3 = model.indexBuffer[triIndex + 2] + 1;
                    OBJfs.WriteLine("f " + (f1 + "/" + f1 + "/" + f1) + " " + (f2 + "/" + f2 + "/" + f2) + " " + (f3 + "/" + f3 + "/" + f3));
                }
            }

        }
    }
}
