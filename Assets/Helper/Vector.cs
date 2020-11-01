using System;
using UnityEngine;


class Vector
{
    static public Vector3 XZ(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    static public Vector2 XZ2XY(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
    static public Vector2 YZ2XY(Vector3 v)
    {
        return new Vector2(v.y, v.z);
    }
    static public Vector3 XY2YZ(Vector2 v)
    {
        return new Vector3(0, v.x, v.y);
    }
    static public Vector3 XY2XZ(Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
    static public Vector3 YX2XZ(Vector2 v)
    {
        return new Vector3(v.y, 0, v.x);
    }

    public static Vector3 DegreeToXZ(float degree)
    {
        return new Vector3(Mathf.Sin(degree * Mathf.Deg2Rad), 0, Mathf.Cos(degree * Mathf.Deg2Rad));
    }

}

