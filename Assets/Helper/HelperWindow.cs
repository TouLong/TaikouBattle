using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class HelperWindow : EditorWindow
{
    [MenuItem("Window/Helper")]
    public static void ShowWindow()
    {
        GetWindow<HelperWindow>("Helper");
    }
    Vector2 scrollPos;
    string childNewName;
    Transform reNameTransform;
    MeshFilter meshFilter;
    int range = 45;
    float far = 0.8f;
    float near = 0.1f;
    float height = 1;
    float scale = 1;
    void OnGUI()
    {
        minSize = new Vector2(300, 100);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        childNewName = EditorGUILayout.TextField("新群組名稱", childNewName);
        reNameTransform = EditorGUILayout.ObjectField("群組", reNameTransform, typeof(Transform), true) as Transform;
        UIHelper.Button("改名", () =>
         {
             ChildReName(reNameTransform, childNewName);
         });
        UIHelper.Line();
        meshFilter = EditorGUILayout.ObjectField("Mesh", meshFilter, typeof(MeshFilter), true) as MeshFilter;
        range = EditorGUILayout.IntField("range", range);
        far = EditorGUILayout.FloatField("far", far);
        near = EditorGUILayout.FloatField("near", near);
        scale = EditorGUILayout.FloatField("size", scale);
        UIHelper.Button("Gen Mesh", () =>
        {
            meshFilter.mesh = SectorGenerator.GenerateMesh(range, far, near, scale);
        });
        UIHelper.Line();

        EditorGUILayout.EndScrollView();
    }
    public void ChildReName(Transform transform, string newName, string conjunction = "-")
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).name = newName + conjunction + (i + 1);
        }
    }
}



