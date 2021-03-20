using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingRange : MonoBehaviour
{
    [SerializeField]
    float scale = 1.25f;
    [SerializeField, Range(0, 1f)]
    float offset = 0.25f;
    public void Setup(float max)
    {
        offset = scale * offset;
        Mesh ellipse = GeoGenerator.EllipsePlane(new Vector2(max, max * scale), new Vector2(0, offset), 60);
        Mesh ellipseWall = GeoGenerator.EllipseWall(new Vector2(max, max * scale), new Vector2(0, offset), 10, 10);
        GetComponent<MeshFilter>().mesh = ellipse;
        GetComponent<MeshCollider>().sharedMesh = ellipseWall;
    }
}