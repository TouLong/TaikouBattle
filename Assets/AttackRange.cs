using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [Range(1, 180)]
    public int range = 45;
    [Range(1, 6)]
    public int resolution = 3;
    [Range(0.6f, 1)]
    public float far = 0.8f;
    [Range(0, 0.5f)]
    public float near = 0.1f;
    void Start()
    {
        Projector projector = FindObjectOfType<Projector>();
        projector.material.SetTexture("_ShadowTex", SectorGenerator.GenerateTexture(range, far, near));
        projector.material.SetColor("_Color", Color.white);
    }
}
