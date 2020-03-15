using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRange : MonoBehaviour
{
    public Transform mesh;
    public float size;
    void OnValidate()
    {
        if (mesh != null)
            mesh.localScale = new Vector3(size, 0.001f, size);
    }
}
