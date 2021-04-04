using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoGenerator
{
    static public Mesh EllipseWall(Vector2 factor, Vector2 offset, float height, int step)
    {
        Mesh mesh = new Mesh();
        int resolution = 360 / step;
        Vector3[] vertices = new Vector3[(step + 1) * 2];
        int[] triangles = new int[step * 6];
        int angle = 0;
        int vIndex = 0;
        int tIndex = 0;
        void AddVertices()
        {
            Vector3 dir = Vector.DegToXz(angle);
            angle += resolution;
            dir.x = dir.x * factor.x + offset.x;
            dir.z = dir.z * factor.y + offset.y;
            dir.y = 0;
            vertices[vIndex++] = dir;
            dir.y = height;
            vertices[vIndex++] = dir;
        }
        AddVertices();
        for (int i = 0; i < step; i++)
        {
            AddVertices();
            triangles[tIndex++] = vIndex - 1;
            triangles[tIndex++] = vIndex - 4;
            triangles[tIndex++] = vIndex - 3;
            triangles[tIndex++] = vIndex - 1;
            triangles[tIndex++] = vIndex - 2;
            triangles[tIndex++] = vIndex - 4;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    static public Mesh EllipsePlane(Vector2 factor, Vector2 offset, int step)
    {
        Mesh mesh = new Mesh();
        int resolution = 360 / step;
        Vector3[] vertices = new Vector3[step + 1];
        int[] triangles = new int[step * 3];
        int angle = 0;
        int vIndex = 0;
        int tIndex = 0;
        void AddVertex()
        {
            Vector3 dir = Vector.DegToXz(angle);
            angle += resolution;
            dir.x = dir.x * factor.x + offset.x;
            dir.z = dir.z * factor.y + offset.y;
            vertices[vIndex++] = dir;
        }
        vertices[vIndex++] = Vector3.zero;
        AddVertex();
        for (int i = 0; i < step - 1; i++)
        {
            AddVertex();
            triangles[tIndex++] = 0;
            triangles[tIndex++] = vIndex - 2;
            triangles[tIndex++] = vIndex - 1;
        }
        triangles[tIndex++] = 0;
        triangles[tIndex++] = vIndex - 1;
        triangles[tIndex++] = 1;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    static public Mesh SectorPlane(int range, float far, float near, float offset)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(range + 1) * 2];
        int[] triangles = new int[range * 6];
        int angle = -range / 2;
        Vector3 offsetZ = Vector3.forward * offset;
        int vIndex = 0;
        int tIndex = 0;
        void AddVertices()
        {
            Vector3 dir = Vector.DegToXz(angle++);
            vertices[vIndex++] = dir * near + offsetZ;
            vertices[vIndex++] = dir * far + offsetZ;
        }
        AddVertices();
        for (int i = 0; i < range; i++)
        {
            AddVertices();
            triangles[tIndex++] = vIndex - 4;
            triangles[tIndex++] = vIndex - 3;
            triangles[tIndex++] = vIndex - 2;
            triangles[tIndex++] = vIndex - 3;
            triangles[tIndex++] = vIndex - 1;
            triangles[tIndex++] = vIndex - 2;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    static public Texture2D SectorTexture(int range, float far, float near, Color mainColor, Color borderColor)
    {
        int size = 512;
        int border = 2;
        float radius = size / 2;
        float nearSide = radius * near;
        float farSide = radius * far;
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
                        texture.SetPixel(w, h, borderColor);
                    else
                        texture.SetPixel(w, h, mainColor);
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
}