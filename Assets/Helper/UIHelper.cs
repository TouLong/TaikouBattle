using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
public class UIHelper
{
    static public void Line()
    {
        EditorGUILayout.Space();
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
        EditorGUILayout.Space();
    }
    static public void Horizontal(Action layout, int labelWidth = 0, int fieldWidth = 0)
    {
        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUIUtility.fieldWidth = fieldWidth;
        layout();
        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;
        GUILayout.EndHorizontal();
    }

    static public void Button(string text, Action action, float width)
    {

        if (GUILayout.Button(text, GUILayout.MaxWidth(width)))
            action();
    }
    static public void Button(string text, Action action)
    {

        if (GUILayout.Button(text))
            action();
    }
    static public void Width(float label, float field)
    {
        EditorGUIUtility.labelWidth = label;
        EditorGUIUtility.fieldWidth = field;
    }
    static public void Width()
    {
        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;
    }
}
#endif