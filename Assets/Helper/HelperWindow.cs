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
    static public void GUILine()
    {
        EditorGUILayout.Space();
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, Color.gray);
        EditorGUILayout.Space();
    }
    Vector2 scrollPos;
    string childNewName;
    Transform reNameTransform;
    void OnGUI()
    {
        minSize = new Vector2(300, 100);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        childNewName = EditorGUILayout.TextField("新群組名稱", childNewName);
        reNameTransform = EditorGUILayout.ObjectField("群組", reNameTransform, typeof(Transform), true) as Transform;
        if (GUILayout.Button("改名"))
            ChildReName(reNameTransform, childNewName);
        GUILine();

        EditorGUILayout.EndScrollView();
    }
    public void ChildReName(Transform transform, string newName, string conjunction = "-")
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).name = newName + conjunction + (i + 1);
        }
    }
    static public T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        Type type = original.GetType();
        var dst = destination.GetComponent(type) as T;
        if (!dst) dst = destination.AddComponent(type) as T;
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst as T;
    }

}



