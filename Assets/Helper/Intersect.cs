using System;
using UnityEngine;


class Intersect
{
    static public bool V3XZ(Vector3 aForm, Vector3 aTo, Vector3 bForm, Vector3 bTo)
    {
        float d = (aTo.x - aForm.x) * (bTo.z - bForm.z) - (aTo.z - aForm.z) * (bTo.x - bForm.x);
        if (d == 0.0f)
        {
            return false;
        }
        float u = ((bForm.x - aForm.x) * (bTo.z - bForm.z) - (bForm.z - aForm.z) * (bTo.x - bForm.x)) / d;
        float v = ((bForm.x - aForm.x) * (aTo.z - aForm.z) - (bForm.z - aForm.z) * (aTo.x - aForm.x)) / d;
        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }
        return true;
    }

}

