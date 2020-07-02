using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(fileName = "MapSetting", menuName = "Map Setting")]
public class MapSetting : ScriptableObject
{
    [Range(1, 10)]
    public int mapDimension = 1;
    [Range(10, 250)]
    public int chunkMesh = 100;
    [Range(5, 50)]
    public int mapHeight = 25;
    [Range(1, 20)]
    public int mapScale = 1;
    [Range(25, 250)]
    public int noiseScale = 25;
    [Range(2, 5)]
    public int octaves = 3;
    [Range(1, 5)]
    public float lacunarity = 2.5f;
    [Range(0, 1)]
    public float persistance = 0.25f;
    public int seed;
    public Vector2 offset;
    public AnimationCurve heightCurve;
    public LayerMask layerMask = 0;
    public int waterLayer = -1;
    public int mountainLayer = -1;
    public List<Layer> layers;
    public List<ObjectDistribution> objectsDistribution;
    public int ChunkVertices => chunkMesh + 3;
    public int ChunkSize => chunkMesh * mapScale;
    public int MapSideVertices => chunkMesh * mapDimension + 3;
    public int MapSideMesh => chunkMesh * mapDimension;
    public int MapSideLength => chunkMesh * mapDimension * mapScale;
    public int MapHeight => mapScale * mapHeight;
    public float WaterHeight
    {
        get
        {
            if (layers.Count > 0 && waterLayer != -1)
                return layers[Mathf.Min(waterLayer + 1, layers.Count - 1)].height * MapHeight;
            else
                return 0;
        }
    }
    public float MountainHeight
    {
        get
        {
            if (layers.Count > 0 && mountainLayer != -1)
                return layers[mountainLayer].height * MapHeight;
            else
                return MapHeight;
        }
    }
    [System.Serializable]
    public class Layer
    {
        public Color color;
        public float height;
        public float blendStrength;
        public Layer(float height)
        {
            this.height = height;
        }
    }
    [System.Serializable]
    public class ObjectDistribution
    {
        [System.Serializable]
        public class Distribution
        {
            public Vector2 region;
            public float radius = 100;
        }
        public string groupName;
        public List<GameObject> objects = new List<GameObject>();
        public List<Distribution> distributions = new List<Distribution>();
    }
}
