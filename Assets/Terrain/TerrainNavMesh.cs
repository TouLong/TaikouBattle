using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class TerrainNavMesh
{
    static readonly string path = "Assets/Terrain/NavMesh.asset";
    public static void Load()
    {
        NavMeshData navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(path) as NavMeshData;
        if (navMeshData != null)
        {
            Clear();
            NavMesh.AddNavMeshData(navMeshData);
        }
    }
    public static void Generate(Bounds bounds, List<MeshFilter> meshFilters)
    {
        List<NavMeshBuildSource> meshBuildSources = new List<NavMeshBuildSource>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            meshBuildSources.Add(new NavMeshBuildSource()
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            });
        }
        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(0);
        NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(settings, meshBuildSources, bounds, bounds.min, Quaternion.identity);
        NavMesh.AddNavMeshData(navMeshData);
        AssetDatabase.CreateAsset(navMeshData, path);
    }
    public static void Clear()
    {
        NavMesh.RemoveAllNavMeshData();
    }
}
