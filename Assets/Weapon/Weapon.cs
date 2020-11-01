using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type
    {
        sword1,
        sword2,
        spear,
    }
    public Type type;
    [Range(0.1f, 1f), SerializeField]
    float far = 1f;
    [Range(0f, 0.9f), SerializeField]
    float near = 0.1f;
    [Range(1, 180)]
    public int angle;
    [Range(0.1f, 10f)]
    public float length;
    [Range(0f, 10f)]
    public float weight;
    [Range(1, 5)]
    public int attack;
    [HideInInspector]
    public float farLength, nearLength, size;
    [HideInInspector]
    public Material mask;

    public bool HitDetect(Transform owner, Unit target)
    {
        Pose ownerXZ = new Pose(Vector.XZ(owner.position), owner.rotation);
        Vector3 targetXZ = Vector.XZ(target.transform.position);
        float distance = Vector3.Distance(targetXZ, ownerXZ.position);
        if (distance >= nearLength && distance <= farLength)
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
    void OnValidate()
    {
        farLength = length * far;
        nearLength = length * near;
        size = farLength - nearLength;
        Texture texture = SectorGenerator.GenerateTexture(angle / 2, far, near, Color.gray, Color.red);
        if (mask == null)
        {
            mask = new Material(Shader.Find("Universal Render Pipeline/NiloCat Extension/Screen Space Decal/Unlit"));
        }
        mask.SetTexture("_MainTex", texture);
        mask.SetFloat("_DstBlend", 1);
    }
}
