using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
public class MapEditor : EditorWindow
{
    Map map;
    Texture2D mapImage;
    GUILayoutOption[] btnOption = new GUILayoutOption[] { GUILayout.MaxWidth(200), GUILayout.Height(20) };
    MapSetting setting;
    Vector2 scrollPos;
    List<bool> mapObjectExpand = new List<bool>();
    [MenuItem("Window/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditor>("Map Editor");
    }
    public bool GetTerrain()
    {
        map = FindObjectOfType<Map>();
        return map != null;
    }
    public bool GetSetting()
    {
        setting = map.setting;
        return setting != null;
    }
    void OnGUI()
    {
        minSize = new Vector2(450, 100);

        if (!GetTerrain())
        {
            GUILayout.Label("找不到 Map");
            return;
        }
        map.setting = EditorGUILayout.ObjectField("設定檔", map.setting, typeof(MapSetting), true) as MapSetting;
        if (!GetSetting())
        {
            GUILayout.Label("Map沒Setting檔");
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        #region Main And Image
        GUILayout.Label(string.Format("地圖尺寸: {0}x{0}x{1}, 地圖網格: {2}x{2}",
        setting.MapSideLength, setting.MapHeight, setting.MapSideMesh));
        GUILayout.Label(mapImage);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("生成", GUILayout.MaxWidth(50));
        if (GUILayout.Button("全部", btnOption)) GenerateAll();
        if (GUILayout.Button("地形+物件", btnOption)) GenerateChunksAndObjects();
        if (GUILayout.Button("地形", btnOption)) GenerateChunks();
        if (GUILayout.Button("導航", btnOption)) GenerateNav();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("隨機生成", GUILayout.MaxWidth(50));
        if (GUILayout.Button("全部", btnOption)) GenerateRandomAll();
        if (GUILayout.Button("地形+物件", btnOption)) GenerateRandomChunksAndObjects();
        if (GUILayout.Button("地形", btnOption)) GenerateRandomChunks();
        if (GUILayout.Button("物件", btnOption)) GenerateMapObject();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("清除", GUILayout.MaxWidth(50));
        if (GUILayout.Button("全部", btnOption)) ClearAll();
        if (GUILayout.Button("地形", btnOption)) ClearChunks();
        if (GUILayout.Button("物件", btnOption)) ClearMapObject();
        if (GUILayout.Button("導航", btnOption)) ClearNav();
        EditorGUILayout.EndHorizontal();

        #endregion
        GUILine();

        #region Layer
        AnimationCurve heightCurve = setting.heightCurve;
        if (setting.layers.Count < heightCurve.keys.Length - 1)
        {
            for (int i = 0; i < heightCurve.keys.Length - 1; i++)
            {
                if (!setting.layers.Exists(x => x.height == heightCurve.keys[i].value))
                    setting.layers.Add(new MapSetting.Layer(heightCurve.keys[i].value));
            }
            map.CreateMaterial();
            map.UpdateMaterial();
        }
        else if (setting.layers.Count > heightCurve.keys.Length - 1)
        {
            for (int i = 0; i < setting.layers.Count; i++)
            {
                if (!heightCurve.keys.ToList().Exists(x => x.value == setting.layers[i].height))
                    setting.layers.Remove(setting.layers[i]);
            }
            map.UpdateMaterial();
        }
        int[] layerIndeices = new int[setting.layers.Count + 1];
        string[] layerLabel = new string[setting.layers.Count + 1];
        layerIndeices[0] = -1;
        layerLabel[0] = "無";
        for (int i = 0; i < setting.layers.Count; i++)
        {
            MapSetting.Layer layer = setting.layers[i];
            layer.height = heightCurve.keys[i].value;
            float rangeMin = layer.height * setting.MapHeight;
            float rangeMax = i == setting.layers.Count - 1 ? setting.MapHeight : setting.layers[i + 1].height * setting.MapHeight;
            string label;
            if (i >= setting.mountainLayer)
                label = string.Format("地層{0}-山區-高度:{1:0.00}~{2:0.00}", i + 1, rangeMin, rangeMax);
            else if (i <= setting.waterLayer)
                label = string.Format("地層{0}-水域-高度:{1:0.00}~{2:0.00}", i + 1, rangeMin, rangeMax);
            else
                label = string.Format("地層{0}-平地-高度:{1:0.00}~{2:0.00}", i + 1, rangeMin, rangeMax);
            GUILayout.Label(label, GUILayout.MaxWidth(210f));
            EditorGUILayout.BeginHorizontal();
            layer.color = EditorGUILayout.ColorField(layer.color, GUILayout.MinWidth(20f));
            EditorGUILayout.LabelField("漸層", GUILayout.MaxWidth(30));
            layer.blendStrength = EditorGUILayout.Slider(layer.blendStrength, 0, i == 0 ? 0 : 1, GUILayout.MaxWidth(150f));
            EditorGUILayout.EndHorizontal();
            layerIndeices[i + 1] = i;
            layerLabel[i + 1] = label;
        }
        EditorGUILayout.LabelField("高度分佈", GUILayout.MaxWidth(50));
        EditorGUILayout.CurveField(heightCurve, GUILayout.MinHeight(100f));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField("地形材質", map.terrainMaterial, typeof(Material), true);
        map.lakeMaterial = EditorGUILayout.ObjectField("水域材質", map.lakeMaterial, typeof(Material), true) as Material;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        setting.mountainLayer = EditorGUILayout.IntPopup("山區", setting.mountainLayer, layerLabel, layerIndeices);
        setting.waterLayer = EditorGUILayout.IntPopup("水域", setting.waterLayer, layerLabel, layerIndeices);
        EditorGUILayout.EndHorizontal();
        setting.layerMask = EditorGUILayout.LayerField("地塊層", setting.layerMask);
        #endregion
        GUILine();

        #region Objects And Distribution
        layerLabel[0] = "選擇地層";
        if (mapObjectExpand.Count != setting.objectsDistribution.Count)
        {
            mapObjectExpand = new List<bool>();
            while (mapObjectExpand.Count != setting.objectsDistribution.Count)
                mapObjectExpand.Add(true);
        }
        ListField("地圖物件", setting.objectsDistribution.Count,
                    () => { setting.objectsDistribution.Add(new MapSetting.ObjectDistribution()); mapObjectExpand.Add(true); },
                    () => { setting.objectsDistribution.RemoveAt(setting.objectsDistribution.Count - 1); mapObjectExpand.RemoveAt(mapObjectExpand.Count - 1); });
        for (int i = 0; i < setting.objectsDistribution.Count; i++)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            string group = setting.objectsDistribution[i].groupName;
            setting.objectsDistribution[i].groupName = EditorGUILayout.TextField("群組", group != null && group != "" ? group : "Group" + (i + 1).ToString());
            if (GUILayout.Button(mapObjectExpand[i] ? "v" : "<", GUILayout.MaxWidth(20)))
                mapObjectExpand[i] = !mapObjectExpand[i];
            GUILayout.EndHorizontal();
            if (!mapObjectExpand[i])
            {
                GUILayout.EndVertical();
                continue;
            }
            EditorGUI.indentLevel = 1;
            List<GameObject> objects = setting.objectsDistribution[i].objects;
            if (objects != null)
            {
                ListField("物件", objects.Count,
                () => { objects.Add(null); },
                () => { objects.RemoveAt(objects.Count - 1); });
                for (int j = 0; j < objects.Count; j++)
                {
                    objects[j] = EditorGUILayout.ObjectField(objects[j], typeof(GameObject), true) as GameObject;
                }
            }
            List<MapSetting.ObjectDistribution.Distribution> distributions = setting.objectsDistribution[i].distributions;
            if (distributions != null)
            {
                ListField("分佈", distributions.Count,
                        () => { distributions.Add(new MapSetting.ObjectDistribution.Distribution()); },
                        () => { distributions.RemoveAt(distributions.Count - 1); });
                for (int j = 0; j < distributions.Count; j++)
                {
                    EditorGUI.indentLevel = 1;
                    GUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUIUtility.fieldWidth = 30;
                    distributions[j].radius = EditorGUILayout.FloatField("分散度: ", distributions[j].radius);
                    EditorGUI.indentLevel = 0;
                    EditorGUIUtility.labelWidth = 90;
                    EditorGUIUtility.fieldWidth = 40;
                    distributions[j].region.x = (float)Math.Round(EditorGUILayout.FloatField("分佈範圍(高度): ", distributions[j].region.x), 2);
                    EditorGUIUtility.labelWidth = 10;
                    distributions[j].region.y = (float)Math.Round(EditorGUILayout.FloatField("~", distributions[j].region.y), 2);
                    EditorGUIUtility.fieldWidth = 0;
                    EditorGUIUtility.labelWidth = 0;
                    int selectLayer = EditorGUILayout.IntPopup(-1, layerLabel, layerIndeices, GUILayout.MaxWidth(80));
                    if (selectLayer > -1)
                    {
                        distributions[j].region = new Vector2(setting.layers[selectLayer].height * setting.MapHeight,
                            selectLayer == setting.layers.Count - 1 ? setting.MapHeight : setting.layers[selectLayer + 1].height * setting.MapHeight);
                    }
                    //EditorGUILayout.MinMaxSlider(ref distributions[j].region.x, ref distributions[j].region.y, 0, setting.MapHeight);
                    EditorGUILayout.MinMaxSlider(ref distributions[j].region.x, ref distributions[j].region.y, Mathf.Max(0, distributions[j].region.x - setting.MapHeight / 10), Mathf.Min(setting.MapHeight, distributions[j].region.y + setting.MapHeight / 10));
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndVertical();
        }

        EditorGUI.indentLevel = 0;
        if (GUILayout.Button("更新地圖物件", btnOption)) GenerateMapObject();
        #endregion
        GUILine();

        #region Parameter
        setting.mapDimension = EditorGUILayout.IntSlider(string.Format("地塊數量 {1}({0}x{0})", setting.mapDimension, setting.mapDimension * setting.mapDimension), setting.mapDimension, 1, 10);
        setting.chunkMesh = EditorGUILayout.IntSlider("地塊網格數", setting.chunkMesh, 10, 250);
        setting.mapHeight = EditorGUILayout.IntSlider("地形高度", setting.mapHeight, 5, 50);
        setting.mapScale = EditorGUILayout.IntSlider("地形放大", setting.mapScale, 1, 20);
        setting.noiseScale = EditorGUILayout.IntSlider("Noise放大", setting.noiseScale, 25, 250);
        setting.octaves = EditorGUILayout.IntSlider("細緻度", setting.octaves, 2, 5);
        setting.lacunarity = EditorGUILayout.Slider("隙度", setting.lacunarity, 1, 5);
        setting.persistance = EditorGUILayout.Slider("持久度", setting.persistance, 0, 1);
        setting.seed = EditorGUILayout.IntField("隨機種子", setting.seed);
        setting.offset = EditorGUILayout.Vector2Field("位移", setting.offset);
        if (GUILayout.Button("生成地形", btnOption)) GenerateChunks();
        if (GUILayout.Button("隨機生成地形", btnOption)) GenerateRandomChunks();
        #endregion
        GUILine();

        EditorGUILayout.EndScrollView();
        EditorUtility.SetDirty(setting);

    }
    void GenerateAll()
    {
        map.ClearAll();
        map.GenerateAll();
        mapImage = TerrainImage(300);
    }
    void GenerateChunks()
    {
        ClearChunks();
        map.GenerateChunkData(true);
        map.GenerateWater();
        UpdateMaterial();
        mapImage = TerrainImage(300);
    }
    void GenerateChunksAndObjects()
    {
        GenerateChunks();
        GenerateMapObject();
    }
    void GenerateNav()
    {
        map.ClearNav();
        map.GenerateNavMesh();
    }
    void GenerateRandomAll()
    {
        setting.seed = UnityEngine.Random.Range(-1000, 1000);
        GenerateAll();
    }
    void GenerateRandomChunks()
    {
        setting.seed = UnityEngine.Random.Range(-1000, 1000);
        GenerateChunks();
    }
    void GenerateRandomChunksAndObjects()
    {
        GenerateRandomChunks();
        GenerateMapObject();
    }
    void GenerateMapObject()
    {
        map.ClearMapObjects();
        map.GenerateMapObject();
    }
    void ClearAll()
    {
        map.ClearAll();
        mapImage = new Texture2D(0, 0);
    }
    void ClearNav()
    {
        map.ClearNav();
    }
    void ClearMapObject()
    {
        map.ClearMapObjects();
    }
    void ClearChunks()
    {
        map.ClearChunks();
        map.ClearWater();
        mapImage = new Texture2D(0, 0);
    }
    void UpdateMaterial()
    {
        map.UpdateMaterial();
    }
    Texture2D TerrainImage(int textrueSize)
    {
        float scale = (float)textrueSize / map.noiseHeights.GetLength(0);
        Texture2D texture = new Texture2D(textrueSize, textrueSize)
        {
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] colorMap = new Color[textrueSize * textrueSize];

        for (int x = 0; x < textrueSize; x++)
        {
            for (int y = 0; y < textrueSize; y++)
            {
                float height = setting.heightCurve.Evaluate(map.noiseHeights[(int)(x / scale), (int)(y / scale)]);
                for (int i = setting.layers.Count - 1; i >= 0; i--)
                {
                    if (height >= setting.layers[i].height)
                    {
                        Color c = setting.layers[i].color;
                        colorMap[y * textrueSize + x] = new Color(c.r, c.g, c.b, 1);
                        break;
                    }
                }
            }
        }
        texture.SetPixels(0, 0, textrueSize, textrueSize, colorMap);
        texture.Apply();
        return texture;
    }
    void ListField(string label, int count, Action add, Action remove)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label + ": " + count.ToString());
        if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
            add();
        if (GUILayout.Button("-", GUILayout.MaxWidth(20)) && count > 0)
            remove();
        GUILayout.EndHorizontal();
    }
    static void GUILine()
    {
        EditorGUILayout.Space();
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, Color.gray);
        EditorGUILayout.Space();
    }

}
#endif