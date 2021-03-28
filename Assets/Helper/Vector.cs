using System;
using UnityEngine;


class Vector
{
    static public Vector3 Xz(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    static public Vector3 XyXz(Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
    static public Vector2 Xy(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
    public static Vector3 DegToXz(float degree)
    {
        return new Vector3(Mathf.Sin(degree * Mathf.Deg2Rad), 0, Mathf.Cos(degree * Mathf.Deg2Rad));
    }
    public static float ForwardAngle(Transform from, Vector3 to)
    {
        Vector3 dir = (to - from.position).normalized;
        return Vector3.Angle(from.forward, dir);
    }
    public static float ForwardSignedAngle(Transform from, Vector3 to)
    {
        Vector3 dir = (to - from.position).normalized;
        return Vector3.SignedAngle(from.forward, dir, Vector3.up);
    }
}

