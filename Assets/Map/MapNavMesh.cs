using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class MapNavMesh
{
    static readonly string path = "Assets/Map/NavMesh.asset";
    public static void Load()
    {
        NavMeshData navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if (navMeshData != null)
        {
            Clear();
            NavMesh.AddNavMeshData(navMeshData);
        }
    }
    public static void Generate(Bounds bounds, MeshFilter[] meshFilters)
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
