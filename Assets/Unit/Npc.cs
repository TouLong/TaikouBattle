using System.Linq;
using UnityEngine;

public class Npc : Unit
{
    public void Decision()
    {
        if (enemies.Count == 0)
            return;
        Unit target = enemies.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
        float distance = Distance(target);
        if (weapon.far < distance)
            Chase(target);
        else if (weapon.near > distance)
            KeepAway(target);
        else
            Guess(target);
    }
    public void ChangeMovingRange(float look)
    {
        float moveRange = maxAp - Mathf.Abs(look) / maxTurning;
        movingRange.localScale = new Vector3(moveRange, 1, moveRange);
        movingRange.rotation = Quaternion.Euler(0, euler.y + look, 0);
    }
    void Chase(Unit target)
    {
        float look = Vector.ForwardSignedAngle(transform, target.position + V3Random.DirectionXz());
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
        ChangeMovingRange(look);
        RaycastHit border = HitMoveBorder(destination.forward);
        float randomDist = Distance(target) - Random.Range(weapon.near, weapon.far);
        float moveDist = Mathf.Min(border.distance, randomDist);
        destination.position = position + destination.forward * moveDist;
    }
    void KeepAway(Unit target)
    {
        float look = Vector.ForwardSignedAngle(transform, target.position + V3Random.DirectionXz());
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
        ChangeMovingRange(look);
        destination.position = HitMoveBorder(-destination.forward).point;
    }
    void Guess(Unit target)
    {
        float look = Vector.ForwardSignedAngle(transform, target.position + V3Random.DirectionXz());
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
        ChangeMovingRange(look);
        Vector3 moveDir = V3Random.DirectionXz();
        destination.position = HitMoveBorder(moveDir).point;
    }
}
