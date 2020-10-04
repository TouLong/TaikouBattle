using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Range(0.5f, 1f), SerializeField]
    float far = 1f;
    [Range(0.1f, 0.49f), SerializeField]
    float near = 0.1f;
    [Range(1, 180)]
    public int angle;
    [Range(1, 20)]
    public int length;
    [HideInInspector]
    public float farLength, nearLength;
    public Material mask;
    [SerializeField]
    Texture2D maskTexture;
    void OnValidate()
    {
        farLength = length * far;
        nearLength = length * near;
        Texture2D texture = SectorGenerator.GenerateTexture(angle / 2, far, near);
        texture.name = maskTexture.name;
        EditorUtility.CopySerialized(texture, maskTexture);
    }
}
