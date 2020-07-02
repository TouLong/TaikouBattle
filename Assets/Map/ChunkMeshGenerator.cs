using UnityEngine;

public static class ChunkMeshGenerator
{
    public static Mesh Generate(float[,] heightMap, float scale)
    {
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2;
        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;
        ChunkMeshData meshData = new ChunkMeshData(meshSize);

        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }
        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                Vector3 vertexPosition = new Vector3((x - 1) * scale, heightMap[x, y], (y - 1) * scale);
                meshData.AddVertex(vertexPosition, vertexIndicesMap[x, y]);
                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x, y + 1];
                    int c = vertexIndicesMap[x + 1, y];
                    int d = vertexIndicesMap[x + 1, y + 1];
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }
        return meshData.CreateMesh();
    }
    class ChunkMeshData
    {
        readonly Vector3[] vertices;
        readonly int[] triangles;
        readonly Vector3[] borderVertices;
        readonly int[] borderTriangles;
        int triangleIndex;
        int borderTriangleIndex;

        public ChunkMeshData(int verticesPerLine)
        {
            vertices = new Vector3[verticesPerLine * verticesPerLine];
            triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

            borderVertices = new Vector3[verticesPerLine * 4 + 4];
            borderTriangles = new int[24 * verticesPerLine];
        }
        public void AddVertex(Vector3 vertexPosition, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                borderVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
            }
        }
        public void AddTriangle(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                borderTriangles[borderTriangleIndex] = a;
                borderTriangles[borderTriangleIndex + 1] = b;
                borderTriangles[borderTriangleIndex + 2] = c;
                borderTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }
        }
        Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
            Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
            Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC).normalized;
        }
        Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            int borderTriangleCount = borderTriangles.Length / 3;
            for (int i = 0; i < borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = borderTriangles[normalTriangleIndex];
                int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
                int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                if (vertexIndexA >= 0)
                {
                    vertexNormals[vertexIndexA] += triangleNormal;
                }
                if (vertexIndexB >= 0)
                {
                    vertexNormals[vertexIndexB] += triangleNormal;
                }
                if (vertexIndexC >= 0)
                {
                    vertexNormals[vertexIndexC] += triangleNormal;
                }
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals;

        }
        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                normals = CalculateNormals()
            };
            return mesh;
        }
    }
}