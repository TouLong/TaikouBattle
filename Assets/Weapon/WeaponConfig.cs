using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WeaponConfig : MonoBehaviour
{
    public Weapon weapon;
}
#if UNITY_EDITOR
[CustomEditor(typeof(WeaponConfig))]
public class WeaponConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WeaponConfig config = target as WeaponConfig;
        Weapon weapon = config.weapon;
        if (weapon != null)
        {
            weapon.far = EditorGUILayout.Slider("far", weapon.far, 0f, 3f);
            weapon.near = EditorGUILayout.Slider("near", weapon.near, 0f, 3f);
            weapon.angle = EditorGUILayout.IntSlider("angle", weapon.angle, 0, 180);
            if (GUILayout.Button("Config"))
            {
                Mesh mesh = GeoGenerator.SectorPlane(weapon.angle, weapon.far, weapon.near, 0);
                config.GetComponent<MeshFilter>().mesh = mesh;
                EditorUtility.SetDirty(weapon);
            }
        }

    }
}
#endif