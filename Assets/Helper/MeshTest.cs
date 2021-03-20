using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
}


[CustomEditor(typeof(MeshTest))]
public class MeshtTestEditor : Editor
{
    int range = 45;
    float far = 0.8f;
    float near = 0.1f;
    Vector2 factor = Vector2.one;
    Vector2 offset = Vector2.zero;
    int step = 10;
    float height = 1f;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MeshTest target = this.target as MeshTest;

        far = EditorGUILayout.Slider("far", far, 0.1f, 1f);
        near = EditorGUILayout.Slider("near", near, 0f, far);
        range = EditorGUILayout.IntSlider("range", range, 1, 180);
        if (GUILayout.Button("SectorPng"))
        {
            Texture2D texture = GeoGenerator.SectorTexture(range, far, near, Color.white, Color.grey);
            string pngName = string.Format("{0}-{1}", near, far);
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes("Assets/Weapon/" + pngName + ".png", bytes);
        }
        if (GUILayout.Button("Sector"))
        {
            Mesh mesh = GeoGenerator.SectorPlane(range, far, near, 0);
            target.GetComponent<MeshFilter>().mesh = mesh;
        }

        factor = EditorGUILayout.Vector2Field("factor", factor);
        offset = EditorGUILayout.Vector2Field("offset", offset);
        step = EditorGUILayout.IntField("step", step);
        if (GUILayout.Button("Ellipse"))
        {
            Mesh mesh = GeoGenerator.EllipsePlane(factor, offset, step);
            target.GetComponent<MeshFilter>().mesh = mesh;
        }
        height = EditorGUILayout.FloatField("height", height);
        if (GUILayout.Button("EllipseWall"))
        {
            Mesh mesh = GeoGenerator.EllipseWall(factor, offset, height, step);
            target.GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}