using System;
using UnityEngine;


class Vector
{
    static public Vector3 Xz(Vector3 v) => new Vector3(v.x, 0, v.z);
    static public Vector3 XyXz(Vector2 v) => new Vector3(v.x, 0, v.y);
    static public Vector2 Xy(Vector3 v) => new Vector2(v.x, v.z);
    public static Vector3 DegToXz(float degree) => new Vector3(Mathf.Sin(degree * Mathf.Deg2Rad), 0, Mathf.Cos(degree * Mathf.Deg2Rad));
    public static Vector3 Forward(Vector3 self, Vector3 target) => (target - self).normalized;
    public static Vector3 Backward(Vector3 self, Vector3 target) => (self - target).normalized;
    public static float ForwardAngle(Transform from, Vector3 to) => Vector3.Angle(from.forward, Forward(from.position, to));
    public static float ForwardSignedAngle(Transform from, Vector3 to) =>
        Vector3.SignedAngle(from.forward, Forward(from.position, to), Vector3.up);

}

