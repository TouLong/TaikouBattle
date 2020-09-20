using System;
using UnityEngine;


class MathHepler
{
    static public Vector3 GetXZ(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }
}

