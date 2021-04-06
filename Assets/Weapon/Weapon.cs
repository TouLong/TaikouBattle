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
    [Range(0, 10)]
    public float far = 1;
    [Range(1, 5)]
    public int attack = 1;
    [Range(0, 2)]
    public int armor = 1;
    public GameObject main;
    public GameObject sub;
    [HideInInspector]
    public string motionAnim => handleType.ToString() + "-" + attackType.ToString();
    public Mesh rangeMesh => GeoGenerator.SectorPlane(angle, far, near);
    public bool IsContain(float distance) => (distance >= near && distance <= far);
    public bool HitDetect(Unit owner, Unit target)
    {
        float distance = owner.Distance(target);
        if (IsContain(distance))
        {
            float angle = owner.Angle(target) * 2;
            return (int)angle <= this.angle;
        }
        return false;
    }
    public bool HitDetect(Unit owner, List<Unit> targets, out List<Unit> hits)
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

