using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColliders : MonoBehaviour
{
    [HideInInspector]
    public float radius, height;
    [HideInInspector]
    public CapsuleCollider model;
    void Start()
    {
        model = GetComponent<CapsuleCollider>();
        radius = model.radius;
        height = model.height;
    }
    public bool IsTouch()
    {
        return Physics.CapsuleCast(transform.position, transform.position + Vector3.up * height, radius, transform.forward
           , out RaycastHit hit, radius / 2f, LayerMask.GetMask("Unit"));
    }
}
