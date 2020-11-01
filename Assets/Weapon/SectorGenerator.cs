﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SectorGenerator : MonoBehaviour
{
    [Range(1, 180)]
    public int range = 45;
    [Range(0.51f, 0.99f)]
    public float far = 0.8f;
    [Range(0.01f, 0.5f)]
    public float near = 0.1f;
    static public Mesh GenerateMesh(int range, float far, float near, float scale)
    {
        Mesh mesh = new Mesh();
        int angle = range * 2;
        float nearSide = scale * near;
        float farSide = scale * far;
        Vector3[] vertices = new Vector3[(angle + 1) * 2];
        int[] triangles = new int[angle * 6];
        float rad = (range + 90) * Mathf.Deg2Rad;
        int vIndex = 0;
        int tIndex = 0;
        void Vertices()
        {
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            vertices[vIndex++] = dir * nearSide;
            vertices[vIndex++] = dir * farSide;
            rad -= Mathf.Deg2Rad;
        }
        Vertices();
        for (int i = 0; i < angle; i++)
        {
            Vertices();
            triangles[tIndex++] = vIndex - 4;
            triangles[tIndex++] = vIndex - 3;
            triangles[tIndex++] = vIndex - 2;
            triangles[tIndex++] = vIndex - 3;
            triangles[tIndex++] = vIndex - 1;
            triangles[tIndex++] = vIndex - 2;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = new Vector2[vertices.Length];
        return mesh;
    }
    static public Texture2D GenerateTexture(int range, float far, float near, Color mainColor, Color borderColor)
    {
        int size = 512;
        float radius = size / 2;
        float nearSide = radius * near;
        float farSide = radius * far;
        int border = 2;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(radius, radius);
        for (int w = 0; w < size; ++w)
        {
            for (int h = 0; h < size; ++h)
            {
                Vector2 point = new Vector2(w, h);
                float angle = Vector2.Angle(Vector2.up, point - center);
                float dist = Vector2.Distance(center, point);
                if (angle >= 0 && angle <= range && dist >= nearSide && dist <= farSide)
                {
                    if (dist <= nearSide + border || dist >= farSide - border || angle >= range - border / 2)
                    {
                        texture.SetPixel(w, h, borderColor);
                    }
                    else
                    {
                        texture.SetPixel(w, h, mainColor);
                    }
                }
                else
                {
                    texture.SetPixel(w, h, Color.clear);
                }
            }
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear;
        texture.Apply();
        return texture;
    }
    public static void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
    void OnValidate()
    {
        //Texture2D texture = GenerateTexture(range, far, near);
        //SaveTextureAsPNG(texture, "Assets/sector.png");
    }
}