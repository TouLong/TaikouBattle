using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
#if UNITY_EDITOR
public class MapObjectGenerator
{
    class MapObjectData
    {
        public float radius;
        public float regionMin;
        public float regionMax;
        public List<GameObject> mapObjects;
        public Transform parent;
        public MapObjectData(float radius)
        {
            this.radius = radius;
        }
        public MapObjectData(float radius, Vector2 heightMask, List<GameObject> mapObjects, Transform parent)
        {
            this.radius = radius;
            regionMin = heightMask.x;
            regionMax = heightMask.y;
            this.mapObjects = mapObjects;
            this.parent = parent;
        }
    }
    struct SpawnCircle
    {
        public float radius;
        public Vector2 center;
    }
    static public void Generate(Vector2 region, float regionHeight, Transform parent, List<MapSetting.ObjectDistribution> objectsDistribution)
    {
        List<MapObjectData> objectDatas = CreateObjectData(parent, objectsDistribution);
        if (objectDatas.Count == 0) return;
        float minRadius = objectDatas.Min(a => a.radius);
        float maxRadius = objectDatas.Max(a => a.radius);
        float cellSize = minRadius / Mathf.Sqrt(2);
        List<int>[,] grid = new List<int>[Mathf.CeilToInt(region.x / cellSize), Mathf.CeilToInt(region.y / cellSize)];
        List<SpawnCircle> circles = new List<SpawnCircle>();

        Vector2 startPoint = new Vector2(Random.Range(0, region.x), Random.Range(0, region.y));
        Physics.Raycast(new Vector3(startPoint.x, regionHeight + 1, startPoint.y), Vector3.down, out RaycastHit startHit);
        List<SpawnCircle> candidates = new List<SpawnCircle>
        {   new SpawnCircle(){center = startPoint, radius = FilterObjectDatassRandom(startHit.point.y, minRadius, objectDatas).radius  } };

        while (candidates.Count > 0)
        {
            int spawnIndex = Random.Range(0, candidates.Count);
            SpawnCircle candidate = candidates[spawnIndex];
            bool stop = false;
            for (int n = 0; n < 10; n++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidatePoint = candidate.center + dir * Random.Range(candidate.radius / 2 + maxRadius / 2, candidate.radius + maxRadius);
                Physics.Raycast(new Vector3(candidatePoint.x, regionHeight + 1, candidatePoint.y), Vector3.down, out RaycastHit hit);
                MapObjectData objectData = FilterObjectDatassRandom(hit.point.y, minRadius, objectDatas);
                if (IsValid(grid, circles, candidatePoint, objectData.radius, region, cellSize))
                {
                    SpawnCircle spawnCircle = new SpawnCircle() { radius = objectData.radius, center = candidatePoint };
                    circles.Add(spawnCircle);
                    candidates.Add(spawnCircle);
                    MarkCircle(ref grid, spawnCircle, cellSize, circles.Count);
                    stop = true;
                    if (objectData.mapObjects != null)
                        GenerateObject(objectData, new Vector3(candidatePoint.x, hit.point.y, candidatePoint.y));
                    break;
                }
            }
            if (!stop)
            {
                candidates.RemoveAt(spawnIndex);
            }
        }
    }
    static List<MapObjectData> CreateObjectData(Transform parent, List<MapSetting.ObjectDistribution> objectsDistribution)
    {
        List<MapObjectData> objectDatas = new List<MapObjectData>();
        foreach (MapSetting.ObjectDistribution objectDistribution in objectsDistribution)
        {
            foreach (MapSetting.ObjectDistribution.Distribution distribution in objectDistribution.distributions)
            {
                if (distribution.radius == 0 || objectDistribution.objects.Count == 0)
                    continue;
                Transform group = parent.Find(objectDistribution.groupName);
                if (group == null)
                {
                    group = new GameObject(objectDistribution.groupName).transform;
                    group.parent = parent;
                }
                objectDatas.Add(new MapObjectData(distribution.radius, distribution.region, objectDistribution.objects, group));
            }
        }
        return objectDatas;
    }
    static MapObjectData FilterObjectDatassRandom(float height, float minRadius, List<MapObjectData> objectDatas)
    {
        List<MapObjectData> vaildObjectDatass = objectDatas.FindAll(a => a.regionMin <= height && a.regionMax >= height);
        if (vaildObjectDatass.Any())
            return vaildObjectDatass[Random.Range(0, vaildObjectDatass.Count)];
        else
            return new MapObjectData(minRadius);
    }
    static bool IsValid(List<int>[,] grid, List<SpawnCircle> circles, Vector2 center, float radius, Vector2 region, float cellSize)
    {
        if (center.x >= 0 && center.x < region.x && center.y >= 0 && center.y < region.y)
        {
            int cellX = (int)(center.x / cellSize);
            int cellY = (int)(center.y / cellSize);
            int offset = Mathf.CeilToInt(radius / cellSize);
            int searchStartX = Mathf.Max(0, cellX - offset);
            int searchEndX = Mathf.Min(cellX + offset, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - offset);
            int searchEndY = Mathf.Min(cellY + offset, grid.GetLength(1) - 1);
            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    if (grid[x, y] == null) continue;
                    foreach (int i in grid[x, y])
                    {
                        int pointIndex = i - 1;
                        if (pointIndex != -1)
                        {
                            float sqrDst = Vector2.Distance(center, circles[pointIndex].center);
                            float dist = circles[pointIndex].radius / 2 + radius / 2;
                            if (sqrDst < dist)
                                return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
    static void MarkCircle(ref List<int>[,] grid, SpawnCircle circle, float cellSize, int mark)
    {
        int cellX = (int)(circle.center.x / cellSize);
        int cellY = (int)(circle.center.y / cellSize);
        int offset = (int)(circle.radius / cellSize);
        int markStartX = Mathf.Max(0, cellX - offset);
        int markEndX = Mathf.Min(cellX + offset, grid.GetLength(0) - 1);
        int markStartY = Mathf.Max(0, cellY - offset);
        int markEndY = Mathf.Min(cellY + offset, grid.GetLength(1) - 1);
        for (int x = markStartX; x <= markEndX; x++)
        {
            for (int y = markStartY; y <= markEndY; y++)
            {
                float dist = Vector2.Distance(new Vector2(cellX, cellY), new Vector2(x, y));
                if (dist <= circle.radius / 2 / cellSize)
                {
                    if (grid[x, y] == null)
                        grid[x, y] = new List<int>() { mark };
                    else
                        grid[x, y].Add(mark);
                }
            }
        }
    }
    static void GenerateObject(MapObjectData objectData, Vector3 position)
    {
        GameObject prefab = objectData.mapObjects[Random.Range(0, objectData.mapObjects.Count)];
        if (prefab != null)
        {
            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity, objectData.parent);
            go.name = string.Format("{0}-{1}", objectData.parent.name, objectData.parent.childCount);
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        }
    }
}
#endif