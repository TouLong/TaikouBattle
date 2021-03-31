using System.Linq;
using UnityEngine;

public class Npc : Unit
{
    public void Decision()
    {
        Unit target = team.enemies.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
        float distance = Distance(target);
        if (weapon.far < distance)
            Chase(target);
        else if (weapon.near > distance)
            KeepAway(target);
        else
            Guess(target);
    }
    public void Chase(Unit target)
    {
        float look = Vector.ForwardSignedAngle(transform, target.position + V3Random.DirectionXz());
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
        float moveRange = ap - Mathf.Abs(look) / maxTurning;
        RaycastHit border = HitMoveBorder(destination.forward, moveRange);
        float randomDist = Distance(target) - Random.Range(weapon.near, weapon.far);
        float moveDist = Mathf.Min(border.distance, randomDist);
        destination.position = position + destination.forward * moveDist;
    }
    public void KeepAway(Unit target)
    {
        float look = Vector.ForwardSignedAngle(transform, target.position + V3Random.DirectionXz());
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
        float moveRange = ap - Mathf.Abs(look) / maxTurning;
        destination.position = HitMoveBorder(-destination.forward, moveRange).point;
    }
    public void Guess(Unit target)
    {
        float look = Vector.ForwardSignedAngle(transform, target.position + V3Random.DirectionXz());
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
        float moveRange = ap - Mathf.Abs(look) / maxTurning;
        Vector3 moveDir = V3Random.DirectionXz();
        destination.position = HitMoveBorder(-destination.forward, moveRange).point;
    }
}
