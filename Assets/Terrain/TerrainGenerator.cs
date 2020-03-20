using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    public MapSetting setting;
    [HideInInspector] public Material terrainMaterial;
    [HideInInspector] public Material lakeMaterial;
    public float[,] noise;
    readonly string terrainGroup = "Terrain";
    readonly string waterGroup = "Water";
    Transform terrain;
    List<Transform> chunks;
    Transform water;
    void Start()
    {
        TerrainNavMesh.Load();
    }
    void Update()
    {
        if (setting != null)
            UpdateMaterial();
    }
    bool GetTerrain()
    {
        terrain = transform.Find(terrainGroup);
        return terrain != null;
    }
    bool GetChunks()
    {
        if (GetTerrain())
        {
            chunks = new List<Transform>();
            for (int i = 0; i < terrain.childCount; i++)
                chunks.Add(terrain.GetChild(i));
            return true;
        }
        else
        {
            return false;
        }
    }
    bool GetWater()
    {
        water = transform.Find(waterGroup);
        return water != null;
    }
    public void GenerateAll()
    {
        GenerateChunks();
        GenerateWater();
        GenerateMapObject();
        GenerateNavMesh();
        UpdateMaterial();
    }
    public void GenerateChunks()
    {
        if (!GetTerrain())
        {
            terrain = new GameObject(terrainGroup).transform;
            terrain.transform.parent = transform;
        }
        chunks = new List<Transform>();
        TerrainHeight.Noise(out noise,
            setting.MapSideVertices, setting.seed, setting.octaves, setting.persistance, setting.lacunarity, setting.noiseScale, setting.offset);
        for (int x = 0; x < setting.mapDimension; x++)
        {
            for (int y = 0; y < setting.mapDimension; y++)
            {
                GameObject chunk = new GameObject("Terrain Chunk");
                chunk.transform.parent = terrain.transform;
                chunk.transform.localPosition = new Vector3(x * setting.ChunkSize, 0, y * setting.ChunkSize);
                Vector2Int offset = new Vector2Int(x * setting.chunkMesh, y * setting.chunkMesh);
                TerrainHeight.Evaluate(out float[,] heights, ref noise,
                     offset, setting.ChunkVertices, setting.mapScale, setting.mapHeight, setting.heightCurve);
                Mesh mesh = MeshGenerator.Generate(heights, setting.mapScale);
                chunk.AddComponent<MeshFilter>().mesh = mesh;
                chunk.AddComponent<MeshCollider>().sharedMesh = mesh;
                chunk.AddComponent<MeshRenderer>().material = terrainMaterial;
                GameObjectUtility.SetStaticEditorFlags(chunk, StaticEditorFlags.NavigationStatic);
                chunks.Add(chunk.transform);
            }
        }
        EditorUtility.SetDirty(this);
    }
    public void GenerateWater()
    {
        if (lakeMaterial == null || setting.waterLayer == -1) return;
        water = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
        water.name = "Water";
        water.transform.parent = transform;
        water.transform.localScale = new Vector3(setting.MapSideLength / 10f, 1, setting.MapSideLength / 10f);
        water.transform.localPosition = new Vector3(setting.MapSideLength / 2f, setting.WaterHeight, setting.MapSideLength / 2f);
        water.transform.GetComponent<Renderer>().sharedMaterial = lakeMaterial;
        DestroyImmediate(water.transform.GetComponent<Collider>());
    }
    public void GenerateMapObject()
    {
        float regionX = setting.MapSideLength + transform.localPosition.x;
        float regionY = setting.MapSideLength + transform.localPosition.z;
        Vector2 regionSize = new Vector2(regionX, regionY);
        MapObjectGenerator.Generate(regionSize, setting.MapHeight, transform, setting.objectsDistribution);
    }
    public void ClearAll()
    {
        ClearChunks();
        ClearWater();
        ClearMapObjects();
        ClearNav();
    }
    public void ClearChunks()
    {
        if (GetTerrain())
        {
            while (terrain.childCount > 0)
                DestroyImmediate(terrain.GetChild(0).gameObject);
        }
        if (terrain != null)
            DestroyImmediate(terrain.gameObject);
    }
    public void ClearMapObjects()
    {
        foreach (MapSetting.ObjectDistribution objectDistribution in setting.objectsDistribution)
        {
            Transform delTransform = transform.Find(objectDistribution.groupName);
            if (delTransform != null)
                DestroyImmediate(delTransform.gameObject);
        }
    }
    public void ClearWater()
    {
        if (GetWater())
            DestroyImmediate(water.gameObject);
    }
    public void CreateMaterial()
    {
        terrainMaterial = new Material(Shader.Find("Custom/Terrain"));
    }
    public void UpdateMaterial()
    {
        if (GetTerrain() && setting.layers.Count > 0)
        {
            if (terrainMaterial == null)
                CreateMaterial();
            terrainMaterial.SetInt("layerCount", setting.layers.Count);
            terrainMaterial.SetColorArray("baseColors", setting.layers.Select(x => x.color).ToArray());
            terrainMaterial.SetFloatArray("baseStartHeights", setting.layers.Select(x => x.height).ToArray());
            terrainMaterial.SetFloatArray("baseBlends", setting.layers.Select(x => x.blendStrength).ToArray());
            terrainMaterial.SetFloat("minHeight", terrain.position.y);
            terrainMaterial.SetFloat("maxHeight", setting.MapHeight + terrain.position.y);
        }
    }
    public void GenerateNavMesh()
    {
        if (GetChunks())
        {
            terrain.position -= Vector3.one * 0.5f;
            Bounds bounds = new Bounds
            {
                min = new Vector3(0, setting.WaterHeight > 0 ? setting.WaterHeight - 2f : setting.WaterHeight, 0),
                max = new Vector3(setting.MapSideLength, setting.MountainHeight, setting.MapSideLength)
            };
            TerrainNavMesh.Generate(bounds, chunks.Select(a => a.GetComponent<MeshFilter>()).ToList());
            terrain.position += Vector3.one * 0.5f;
        }
    }
    public void ClearNav()
    {
        TerrainNavMesh.Clear();
    }
}