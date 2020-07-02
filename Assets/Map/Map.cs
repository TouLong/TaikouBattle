using UnityEngine;
using System.Linq;
using UnityEditor;

[ExecuteInEditMode]
public class Map : MonoBehaviour
{
    public MapSetting setting;
    [HideInInspector] public float[,] noiseHeights;
    [HideInInspector] public Material terrainMaterial;
    [HideInInspector] public Material lakeMaterial;
    readonly string terrainGroup = "Terrain";
    readonly string waterGroup = "Water";
    Transform terrain;
    Transform water;
    static MapChunk[,] chunks;
    static int sideLength;
    static int chunkSize;
    void Awake()
    {
        GenerateChunkData(false);
        MapNavMesh.Load();
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
    bool GetWater()
    {
        water = transform.Find(waterGroup);
        return water != null;
    }
    static public MapChunk GetChunk(float x, float y)
    {
        return chunks[(int)(x / chunkSize), (int)(y / chunkSize)];
    }
    static public float GetHeight(float x, float y)
    {
        if (x > sideLength || y > sideLength || x < 0 || y < 0)
        {
            return 0;
        }
        //https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        MapChunkMesh mesh = GetChunk(x, y).GetMesh(x, y);
        Vector3 vertex = mesh.vertex;
        Vector3 normal = mesh.GetNormal(x, y);
        return (normal.x * (x - vertex.x) + normal.z * (y - vertex.z)) / -normal.y + vertex.y;
    }
    static public Vector3 GetNormal(float x, float y)
    {
        if (x > sideLength || y > sideLength || x < 0 || y < 0)
        {
            return Vector3.up;
        }
        return GetChunk(x, y).GetMesh(x, y).GetNormal(x, y);
    }
    public void GenerateAll()
    {
        GenerateChunkData(true);
        GenerateWater();
        GenerateMapObject();
        GenerateNavMesh();
        UpdateMaterial();
    }
    public void GenerateChunkData(bool genObject)
    {
        sideLength = setting.MapSideLength;
        chunkSize = setting.ChunkSize;
        chunks = new MapChunk[setting.mapDimension, setting.mapDimension];
        TerrainHeight.Noise(ref noiseHeights, setting.MapSideVertices, setting.seed, setting.octaves, setting.persistance, setting.lacunarity, setting.noiseScale, setting.offset);
        for (int x = 0; x < setting.mapDimension; x++)
        {
            for (int y = 0; y < setting.mapDimension; y++)
            {
                Vector2Int offset = new Vector2Int(x * setting.chunkMesh, y * setting.chunkMesh);
                TerrainHeight.Evaluate(out float[,] chunkHeights, ref noiseHeights, offset, setting.ChunkVertices, setting.mapScale, setting.mapHeight, setting.heightCurve);
                chunks[x, y] = new MapChunk(chunkHeights, setting.chunkMesh, offset, setting.mapScale);
                if (genObject)
                {
                    GenerateChunkObject(x, y, chunkHeights);
                }
            }
        }
    }
    public void GenerateChunkObject(int x, int y, float[,] chunkHeights)
    {
        if (!GetTerrain())
        {
            terrain = new GameObject(terrainGroup).transform;
            terrain.transform.parent = transform;
        }
        GameObject chunk = new GameObject(string.Format("{0},{1}", x, y));
        chunk.transform.parent = terrain.transform;
        chunk.transform.localPosition = new Vector3(x * setting.ChunkSize, 0, y * setting.ChunkSize);
        if (setting.layerMask > 0)
            chunk.layer = setting.layerMask;
        Mesh mesh = ChunkMeshGenerator.Generate(chunkHeights, setting.mapScale);
        chunk.AddComponent<MeshFilter>().mesh = mesh;
        chunk.AddComponent<MeshCollider>().sharedMesh = mesh;
        chunk.AddComponent<MeshRenderer>().material = terrainMaterial;
        GameObjectUtility.SetStaticEditorFlags(chunk, StaticEditorFlags.NavigationStatic);
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
        if (GetTerrain())
        {
            terrain.position -= Vector3.one * 0.5f;
            Bounds bounds = new Bounds
            {
                min = new Vector3(0, setting.WaterHeight > 0 ? setting.WaterHeight - 2f : setting.WaterHeight, 0),
                max = new Vector3(setting.MapSideLength, setting.MountainHeight, setting.MapSideLength)
            };
            MeshFilter[] meshFilters = terrain.GetComponentsInChildren<MeshFilter>();
            MapNavMesh.Generate(bounds, meshFilters);
            terrain.position += Vector3.one * 0.5f;
        }
    }
    public void ClearNav()
    {
        MapNavMesh.Clear();
    }
}
public class MapChunkMesh
{
    public Vector3 vertex;
    public Vector3 normalLeft;
    public Vector3 normalRight;
    public Vector3 GetNormal(float x, float y)
    {
        if (x - vertex.x > y - vertex.z)
        {
            return normalRight;
        }
        else
        {
            return normalLeft;
        }
    }
}
public class MapChunk
{
    public MapChunkMesh[,] mesh;
    public Vector2 offset;
    public float scale;
    public MapChunkMesh GetMesh(float x, float y)
    {
        return mesh[(int)((x - offset.x) / scale), (int)((y - offset.y) / scale)];
    }
    public MapChunk(float[,] heightMap, int size, Vector2 offset, float scale)
    {
        this.offset = new Vector2(offset.x * scale, offset.y * scale);
        this.scale = scale;
        mesh = new MapChunkMesh[size + 1, size + 1];
        for (int x = 0; x < size + 1; x++)
        {
            for (int y = 0; y < size + 1; y++)
            {
                mesh[x, y] = new MapChunkMesh
                {
                    vertex = new Vector3((x + offset.x) * scale, heightMap[x + 1, y + 1], (y + offset.y) * scale)
                };
            }
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector3 sideAC = mesh[x + 1, y].vertex - mesh[x, y].vertex;
                Vector3 sideAB = mesh[x, y + 1].vertex - mesh[x, y].vertex;
                Vector3 sideAD = mesh[x + 1, y + 1].vertex - mesh[x, y].vertex;
                mesh[x, y].normalLeft = Vector3.Cross(sideAB, sideAD).normalized;
                mesh[x, y].normalRight = Vector3.Cross(sideAD, sideAC).normalized;
            }
        }
    }
}