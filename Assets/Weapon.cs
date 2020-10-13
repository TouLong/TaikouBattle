using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Range(0.5f, 1f), SerializeField]
    float far = 1f;
    [Range(0.1f, 0.49f), SerializeField]
    float near = 0.1f;
    [Range(1, 180)]
    public int angle;
    [Range(1, 20)]
    public int length;
    [HideInInspector]
    public float farLength, nearLength;
    public Material mask;
    public Texture2D maskTexture;
    public bool HitDetect(Transform owner, Unit target)
    {
        float distance = Vector3.Distance(target.transform.position, owner.position);
        if (distance > nearLength && distance < farLength)
        {
            float angle = Vector3.Angle(owner.forward, target.transform.position - owner.position) * 2;
            return angle <= this.angle;
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
        Texture2D texture = SectorGenerator.GenerateTexture(angle / 2, far, near);
        texture.name = maskTexture.name;
        EditorUtility.CopySerialized(texture, maskTexture);
    }
}
