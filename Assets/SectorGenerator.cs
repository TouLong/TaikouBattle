using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorGenerator : MonoBehaviour
{
    [Range(1, 60)]
    public int range = 18;
    [Range(1, 6)]
    public int resolution = 3;
    [Range(0.6f, 1)]
    public float far = 0.8f;
    [Range(0, 0.5f)]
    public float near = 0.1f;
    Mesh GenerateMesh(int range, int resolution, float far, float near, int size)
    {
        Mesh mesh = new Mesh();
        int angle = range * 6;
        int sub = angle / (6 + (6 - resolution) * 3);
        float delta = angle / sub * Mathf.Deg2Rad;
        float nearSide = size * near;
        float farSide = size * far;
        Vector3[] vertices = new Vector3[(sub + 1) * 2];
        int[] triangles = new int[sub * 6];
        Vector3 origin = Vector3.zero;
        float rad = 0;
        int vIndex = 0;
        int tIndex = 0;
        void Vertices()
        {
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            Vector3 vertex1 = origin + dir * farSide;
            Vector3 vertex2 = origin + dir * nearSide;
            vertices[vIndex++] = vertex2;
            vertices[vIndex++] = vertex1;
            rad -= delta;
        }
        Vertices();
        for (int i = 0; i < sub; i++)
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
        //mesh.uv = new Vector2[vertices.Length];
        return mesh;
    }
    Texture2D GenerateTexture(int range, float far, float near)
    {
        int size = 256;
        float radius = size / 2;
        float nearSide = size * near;
        float farSide = size * far;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(radius, 0);
        float leftLimit = 90 - range / 2;
        float rightLimit = 90 + range / 2;
        for (int w = 0; w < size; ++w)
        {
            for (int h = 0; h < size; ++h)
            {
                Vector2 point = new Vector2(w, h) - center;
                float angle = Vector2.Angle(center, point);
                float dist = Vector2.Distance(point, Vector2.zero);
                float alpha = 1;
                float width = (Mathf.Sin(range * Mathf.Deg2Rad / 2) * point.y);
                Color color = Color.white;
                if (angle > leftLimit && angle < rightLimit && dist > nearSide && dist < farSide)
                {
                    float farRate = (1 - Mathf.InverseLerp(nearSide, farSide, dist)) * 1.5f;
                    float nearRate = Mathf.InverseLerp(nearSide, farSide, dist) * 30f;
                    float xRate = (width - Mathf.Abs(point.x)) / 6f;
                    alpha = Mathf.Min(farRate, nearRate, xRate, 1);
                }
                else
                {
                    color = Color.black;
                }
                texture.SetPixel(w, h, color * alpha);
            }
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear;
        texture.Apply();
        return texture;
    }
    void OnValidate()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = GenerateMesh(range, resolution, far, near, 10);

        Projector projector = GetComponent<Projector>();
        projector.material.SetTexture("_ShadowTex", GenerateTexture(range, far, near));
    }
}
