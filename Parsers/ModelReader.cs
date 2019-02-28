using System.Collections.Generic;
using System.IO;
using RatchetEdit.Models;

namespace RatchetEdit
{
    public static class ModelReader
    {
        public static void ReadObj(string fileName, Model model)
        {
            var vertexList = new List<float>();
            var nomalList = new List<float>();
            var uvList = new List<float>();

            var vertexBufferList = new List<float>();
            var conf = new List<TextureConfig>();
            var indBuff = new List<ushort>();

            string line;

            using (StreamReader file = new StreamReader(fileName))
            {
                ushort indCnt = 0, prevCnt = 0;
                int mod = model.textureConfig[0].mode;

                while ((line = file.ReadLine()) != null)
                {
                    string[] g = line.Split(' ');
                    switch (g[0])
                    {
                        case "v":
                            vertexList.Add(float.Parse(g[1]));
                            vertexList.Add(float.Parse(g[2]));
                            vertexList.Add(float.Parse(g[3]));
                            break;
                        case "vn":
                            nomalList.Add(float.Parse(g[1]));
                            nomalList.Add(float.Parse(g[2]));
                            nomalList.Add(float.Parse(g[3]));
                            break;
                        case "vt":
                            uvList.Add(float.Parse(g[1]));
                            uvList.Add(float.Parse(g[2]));
                            break;
                        case "usemtl":
                            conf.Add(new TextureConfig
                            {
                                ID = 0x2d8,
                                start = prevCnt,
                                size = indCnt - prevCnt,
                                mode = mod
                            });

                            prevCnt = indCnt;
                            break;

                        case "f":
                            string[] f1 = g[1].Split('/');
                            string[] f2 = g[2].Split('/');
                            string[] f3 = g[3].Split('/');

                            ushort vert1 = (ushort)(ushort.Parse(f1[0]) - 1);
                            ushort vert2 = (ushort)(ushort.Parse(f2[0]) - 1);
                            ushort vert3 = (ushort)(ushort.Parse(f3[0]) - 1);

                            ushort normal1 = (ushort)(ushort.Parse(f1[2]) - 1);
                            ushort normal2 = (ushort)(ushort.Parse(f2[2]) - 1);
                            ushort normal3 = (ushort)(ushort.Parse(f3[2]) - 1);

                            ushort uv1 = (ushort)(ushort.Parse(f1[1]) - 1);
                            ushort uv2 = (ushort)(ushort.Parse(f2[1]) - 1);
                            ushort uv3 = (ushort)(ushort.Parse(f3[1]) - 1);


                            indBuff.Add((ushort)(indCnt + 0));
                            indBuff.Add((ushort)(indCnt + 1));
                            indBuff.Add((ushort)(indCnt + 2));

                            vertexBufferList.Add(vertexList[(vert1) * 3 + 0]);
                            vertexBufferList.Add(vertexList[(vert1) * 3 + 1]);
                            vertexBufferList.Add(vertexList[(vert1) * 3 + 2]);

                            vertexBufferList.Add(nomalList[(normal1) * 3 + 0]);
                            vertexBufferList.Add(nomalList[(normal1) * 3 + 1]);
                            vertexBufferList.Add(nomalList[(normal1) * 3 + 2]);

                            vertexBufferList.Add(uvList[uv1 * 2 + 0]);
                            vertexBufferList.Add(1f - uvList[uv1 * 2 + 1]);


                            vertexBufferList.Add(vertexList[(vert2) * 3 + 0]);
                            vertexBufferList.Add(vertexList[(vert2) * 3 + 1]);
                            vertexBufferList.Add(vertexList[(vert2) * 3 + 2]);

                            vertexBufferList.Add(nomalList[(normal2) * 3 + 0]);
                            vertexBufferList.Add(nomalList[(normal2) * 3 + 1]);
                            vertexBufferList.Add(nomalList[(normal2) * 3 + 2]);

                            vertexBufferList.Add(uvList[uv2 * 2 + 0]);
                            vertexBufferList.Add(1f - uvList[uv2 * 2 + 1]);


                            vertexBufferList.Add(vertexList[(vert3) * 3 + 0]);
                            vertexBufferList.Add(vertexList[(vert3) * 3 + 1]);
                            vertexBufferList.Add(vertexList[(vert3) * 3 + 2]);

                            vertexBufferList.Add(nomalList[(normal3) * 3 + 0]);
                            vertexBufferList.Add(nomalList[(normal3) * 3 + 1]);
                            vertexBufferList.Add(nomalList[(normal3) * 3 + 2]);

                            vertexBufferList.Add(uvList[uv3 * 2 + 0]);
                            vertexBufferList.Add(1f - uvList[uv3 * 2 + 1]);

                            indCnt += 3;
                            break;
                    }
                }

                TextureConfig cc = new TextureConfig
                {
                    ID = 428,
                    start = prevCnt,
                    size = indCnt - prevCnt,
                    mode = mod
                };
                conf.Add(cc);

                prevCnt = indCnt;

                model.vertexBuffer = vertexBufferList.ToArray();
                model.indexBuffer = indBuff.ToArray();

                model.textureConfig = conf;

                model.VBO = 0;
                model.IBO = 0;
            }
        }
    }
}
