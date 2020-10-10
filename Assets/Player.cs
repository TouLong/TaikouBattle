using UnityEngine;
using System.Collections.Generic;

public class Player
{
    public static Unit unit;
    Transform dest;
    Transform origin;
    Collider collider;
    public Player(Unit unit)
    {
        Player.unit = unit;
        dest = Player.unit.model;
        origin = Object.Instantiate(dest, Player.unit.transform);
        collider = Player.unit.GetComponent<Collider>();
    }
    public void Start()
    {
        origin.gameObject.SetActive(true);
        collider.enabled = false;
        unit.attackMask.SetParent(dest);
    }
    public void Reset()
    {
        dest.localPosition = Vector3.zero;
        dest.localEulerAngles = Vector3.zero;
        unit.attackMask.SetParent(unit.transform);
        collider.enabled = true;
        origin.gameObject.SetActive(false);
    }
    public void MovementMask(bool enable)
    {
        unit.MovementMask(enable);
    }
    public void AttackMask(bool enable)
    {
        unit.AttackMask(enable);
    }
    public void MoveTo(float x, float z)
    {
        Vector2 newPos = Vector2.ClampMagnitude(new Vector2(x, z) - MathHepler.V3ToV2(origin.position), unit.moveDistance) + MathHepler.V3ToV2(origin.position);
        dest.position = new Vector3(newPos.x, Map.GetHeight(newPos.x, newPos.y), newPos.y);
    }
    public void LookAt(float x, float z)
    {
        Vector3 forward = new Vector3(x - dest.position.x, 0, z - dest.position.z);
        if (forward != Vector3.zero)
            dest.rotation = Quaternion.LookRotation(forward);
        foreach (Unit unit in Enemies.InScene)
        {
            unit.HighLight(false);
        }
        if (unit.weapon.HitDetect(dest, Enemies.InScene, out List<Unit> hits))
        {
            foreach (Unit unit in hits)
            {
                unit.HighLight(true);
            }
        }
    }
    public CombatTween Circling()
    {
        Vector3 newPos = dest.position;
        newPos.y = origin.position.y;
        CombatTween tween = new CombatTween
        {
            lookat = unit.LookAtTween(newPos),
            move = unit.MoveTween(newPos),
            rotate = unit.RotateTween(dest.rotation),
        };
        return tween;
    }
    public void Combat()
    {
        if (unit.weapon.HitDetect(unit.transform, Enemies.InScene, out List<Unit> hits))
        {
            AttackMask(true);
            foreach (Unit hit in hits)
            {
                hit.HighLight(true);
            }
            unit.Punch(() =>
            {
                foreach (Unit hit in hits)
                {
                    hit.Damage(1);
                    hit.HighLight(false);
                }
                AttackMask(false);
                unit.Idle();
            });
        }
        else
        {
            unit.Idle();
        }
    }
}
