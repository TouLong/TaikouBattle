using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Setting : MonoBehaviour
{
    public Color[] colors;
    public Material[] materials;
    static public Setting self;
    public void Awake()
    {
        if (self == null)
            self = this;
    }
}
