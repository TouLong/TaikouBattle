using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu()]
public class Weapon : ScriptableObject
{
    public enum HandleType
    {
        single,
        both,
        pole,
        shield,
    }
    public enum AttackType
    {
        hack,
        stab,
    }
    public HandleType handleType;
    public AttackType attackType;
    [Range(1, 180)]
    public int angle = 90;
    [Range(0, 3)]
    public float near = 0;
    [Range(0, 3)]
    public float far = 1;
    [HideInInspector]
    public float size;
    [Range(1, 3)]
    public int attack = 1;
    public GameObject main;
    public GameObject sub;
    [HideInInspector]
    public string motionAnim;
    void Awake()
    {
        motionAnim = handleType.ToString() + "-" + attackType.ToString();
    }
    public Mesh GetRangeMesh()
    {
        return GeoGenerator.SectorPlane(angle, far, near, 0);
    }
    public bool HitDetect(Transform owner, Unit target)
    {
        Pose ownerXZ = new Pose(Vector.Xz(owner.position), owner.rotation);
        Vector3 targetXZ = Vector.Xz(target.transform.position);
        float distance = Vector3.Distance(targetXZ, ownerXZ.position);
        if (distance >= near && distance <= far)
        {
            float angle = Vector3.Angle(ownerXZ.forward, targetXZ - ownerXZ.position) * 2;
            return (int)angle <= this.angle;
        }
        return false;
    }
    public bool HitDetect(Transform owner, List<Unit> targets, out List<Unit> hits)
    {
        hits = new List<Unit>();
        foreach (Unit target in targets)
        {
            if (HitDetect(owner, target))
            {
                hits.Add(target);
            }
        }
        return hits.Any();
    }
}

