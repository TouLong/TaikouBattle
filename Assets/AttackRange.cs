using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [Range(0.5f, 1f)]
    public float far;
    [Range(0.01f, 0.49f)]
    public float near;
    [Range(1, 180)]
    public int range;

    public float farLength;
    public float nearLength;
    public void Config(int angle, float size)
    {
        farLength = size * far;
        nearLength = size * near;
        range = angle;
        GetComponentInChildren<Projector>().orthographicSize = size;
    }
}
