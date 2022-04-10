﻿using System;
using System.Collections.Generic;
using System.IO;

namespace BasicTriangle
{
    public class ObjResource
    {
        //Вершины объекта в виде списка
        public Vertex[] vertices { get; set; }
        //Нормали, соответствующие этим вершинам (в том же порядке)
        public Normal[] normals { get; set; }

        //Треугольники, представленные номерами вершин каждый
        //в виде v11, v12, v13 <- первый треугольник
        //       v21, v22, v23 <- следующий
        //       ...........
        //       vk1, vk2, vk3 <- последний треугольник
        public int[] triangles { get; set; }

        public ObjResource(Vertex[] vertices, Normal[] normals, int[] triangles)
        {
            this.vertices = vertices;
            this.normals = normals;
            this.triangles = triangles;
        }

        //Получение всех точек в формате:
        //x1, y1, z1, w1,
        //x2, y2, z2, w2,
        //...........
        //xm, ym, zm, wm
        public float[] GetFloatPoints()
        {
            float[] points = new float[vertices.Length * 4];
            for (int i = 0, j = 0; i < vertices.Length; i++, j += 4)
            {
                points[j] = vertices[i].x;
                points[j + 1] = vertices[i].y;
                points[j + 2] = vertices[i].z;
                points[j + 3] = vertices[i].w;
            }
            return points;
        }
    }

    public class ObjReader
    {
        public ObjResource ReadObj(string path)
        {
            List<Vertex> objVertices = new List<Vertex>();
            List<Normal> objNormals = new List<Normal>();

            string fileText = File.ReadAllText(path);
            string[] fileLines = fileText.Split('\n');

            //Проход по каждой строке .obj-файла
            int i = 0;
            for (; i < fileLines.Length; i++)
            {
                string line = fileLines[i];
                string[] lineParts = line.Split(' ');

                //Считывание нормали
                if (line.StartsWith("vn"))
                {
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    float z = float.Parse(lineParts[3].Replace('.', ','));
                    objNormals.Add(new Normal(x, y, z));
                }

                //Считывание вершины
                else if (line.StartsWith("v"))
                {
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    float z = float.Parse(lineParts[3].Replace('.', ','));
                    objVertices.Add(new Vertex(x, y, z));
                }
                //Начало треугольников в файле (конец перечисления вершин и нормалей)
                else if (line.StartsWith("f")) break;
            }

            //Массив нормалей к ним (уже отсортированный, каждой вершине нормаль)
            Normal[] normalsSorted = new Normal[objNormals.Count];
            //Список треугольников
            //Треугольники представлены списками вершин
            //в виде v11, v12, v13, <- треугольник 1
            //       v21, v22, v23, <- треугольник 2
            //       ...........
            //       vk1, vk2, vk3  <- треугольник k (последний)
            List<int> objTriangles = new List<int>();

            //Считывание треугольников
            for (; i < fileLines.Length; i++)
            {
                string line = fileLines[i];
                string[] lineParts = line.Split(' ');

                if (line.StartsWith("f"))
                {
                    string[] s1 = lineParts[1]
                        .Split(new string[] { "//" }, StringSplitOptions.None);
                    string[] s2 = lineParts[2]
                        .Split(new string[] { "//" }, StringSplitOptions.None);
                    string[] s3 = lineParts[3]
                        .Split(new string[] { "//" }, StringSplitOptions.None);

                    int v1 = int.Parse(s1[0]) - 1;
                    int vn1 = int.Parse(s1[1]) - 1;

                    int v2 = int.Parse(s2[0]) - 1;
                    int vn2 = int.Parse(s2[1]) - 1;

                    int v3 = int.Parse(s3[0]) - 1;
                    int vn3 = int.Parse(s3[1]) - 1;

                    normalsSorted[v1] = objNormals[vn1];
                    normalsSorted[v2] = objNormals[vn2];
                    normalsSorted[v3] = objNormals[vn3];

                    objTriangles.Add(v1);
                    objTriangles.Add(v2);
                    objTriangles.Add(v3);
                }
            }
            return new ObjResource(
                objVertices.ToArray(),
                normalsSorted,
                objTriangles.ToArray());
        }
    }


}