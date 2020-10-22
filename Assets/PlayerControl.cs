using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class PlayerControl
{
    public static Unit unit;
    static Transform dest;
    static Transform origin;

    static public void Setup(Unit unit)
    {
        PlayerControl.unit = unit;
        dest = PlayerControl.unit.model;
        origin = Object.Instantiate(dest, PlayerControl.unit.transform);
        origin.name = "origin";
        origin.gameObject.SetActive(false);
    }
    static public void Start()
    {
        origin.gameObject.SetActive(true);
        unit.GetComponent<Collider>().enabled = false;
        unit.status.attackRange.transform.SetParent(dest);
    }
    static public void Reset()
    {
        dest.localPosition = Vector3.zero;
        dest.localEulerAngles = Vector3.zero;
        unit.GetComponent<Collider>().enabled = true;
        origin.gameObject.SetActive(false);
        unit.status.attackRange.transform.SetParent(unit.transform);
    }
    static public void MoveTo(float x, float z)
    {
        Vector2 newPos = Vector2.ClampMagnitude(new Vector2(x, z) - MathHepler.V3ToV2(origin.position), unit.moveDistance) + MathHepler.V3ToV2(origin.position);
        dest.position = new Vector3(newPos.x, Map.GetHeight(newPos.x, newPos.y), newPos.y);
    }
    static public void LookAt(float x, float z)
    {
        Vector3 forward = new Vector3(x - dest.position.x, 0, z - dest.position.z);
        if (forward != Vector3.zero)
            dest.rotation = Quaternion.LookRotation(forward);
    }
    static public Sequence Action()
    {
        Vector3 newPos = dest.position;
        newPos.y = origin.position.y;
        unit.action = DOTween.Sequence()
            .Append(unit.LookAtTween(newPos))
            .Append(unit.MoveTween(newPos))
            .Append(unit.RotateTween(dest.rotation))
            .AppendCallback(() => { unit.Combat(Enemy.InScene.Cast<Unit>().ToList()); });
        return unit.action;
    }
}
