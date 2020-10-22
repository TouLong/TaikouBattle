using DG.Tweening;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class Enemy : Unit
{
    static public new List<Enemy> InScene = new List<Enemy>();
    static public List<Sequence> InSceneAction()
    {
        List<Sequence> tweens = new List<Sequence>();
        List<Vector3[]> paths = new List<Vector3[]>();
        Vector3 playerPosXZ = MathHepler.GetXZ(PlayerControl.unit.transform.position);
        float playerMoveRange = PlayerControl.unit.moveDistance;
        bool IsPathBlocking(Vector3 start, Vector3 end)
        {
            for (int j = 0; j < paths.Count; j++)
            {
                if (Vector3.Distance(paths[j][1], end) < 4)
                {
                    return true;
                }
                if (MathHepler.IntersectXZ(paths[j][0], paths[j][1], start, end))
                {
                    return true;
                }
            }
            return false;
        }
        foreach (Enemy enemy in InScene)
        {
            Vector3 startPosXZ = MathHepler.GetXZ(enemy.transform.position);
            Vector3 endPosXZ = startPosXZ;
            Quaternion newRot = Quaternion.LookRotation(playerPosXZ - endPosXZ);
            if (Vector3.Distance(startPosXZ, playerPosXZ) < enemy.moveDistance + enemy.weapon.farLength + playerMoveRange)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector3 guessPosXZ = V3Random.RangeXZ(-playerMoveRange, playerMoveRange) + playerPosXZ;
                    Vector3 attackPosXZ = V3Random.RangeXZ(-enemy.moveDistance, enemy.moveDistance) + startPosXZ;
                    float dist = Vector3.Distance(attackPosXZ, guessPosXZ);
                    if (dist > enemy.weapon.farLength || dist < enemy.weapon.nearLength)
                    {
                        continue;
                    }
                    if (!IsPathBlocking(startPosXZ, attackPosXZ))
                    {
                        endPosXZ = attackPosXZ;
                        newRot = Quaternion.LookRotation(guessPosXZ - endPosXZ);
                        break;
                    }
                }
            }
            else
            {
                Vector3 movePosXZ = Vector3.ClampMagnitude(playerPosXZ - startPosXZ, enemy.moveDistance) + startPosXZ;
                if (!IsPathBlocking(startPosXZ, movePosXZ))
                {
                    endPosXZ = movePosXZ;
                    newRot = Quaternion.LookRotation(playerPosXZ - endPosXZ);
                }
            }

            paths.Add(new Vector3[] { startPosXZ, endPosXZ });
            endPosXZ.y = enemy.transform.position.y;
            enemy.action = DOTween.Sequence()
                .Append(enemy.LookAtTween(endPosXZ))
                .Append(enemy.MoveTween(endPosXZ))
                .Append(enemy.RotateTween(newRot))
                .AppendCallback(enemy.Combat);
            tweens.Add(enemy.action);
        }
        return tweens;
    }
    public void Combat()
    {
        if (weapon.HitDetect(transform, PlayerControl.unit))
        {
            Attack(() =>
            {
                PlayerControl.unit.DamageBy(this);
                Idle();
            });
        }
        else
        {
            Idle();
        }
    }
    void OnEnable()
    {
        Unit.InScene.Add(this);
        InScene.Add(this);
    }
    void OnDisable()
    {
        Unit.InScene.Remove(this);
        InScene.Remove(this);
    }
    void OnDestroy()
    {
        Unit.InScene.Remove(this);
        InScene.Remove(this);
    }
}
