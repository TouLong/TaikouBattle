using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemies : List<Unit>
{
    static public int Layer;
    static public Enemies InScene = new Enemies();
    public Enemies()
    {

    }
    public void HighLight(bool enable)
    {
        ForEach(x => x.HighLight(enable));
    }
    public void Damage(int attackPoint)
    {
        ForEach(x => x.Damage(attackPoint));
    }
    public void Combat()
    {
        foreach (Unit unit in this)
        {
            if (unit.weapon.HitDetect(unit.transform, Player.unit))
            {
                unit.Punch(() =>
                {
                    Player.unit.Damage(1);
                    unit.Idle();
                });
            }
            else
            {
                unit.Idle();
            }
        }
    }
    public List<CombatTween> Circling()
    {
        List<CombatTween> tweens = new List<CombatTween>();
        List<Vector3[]> paths = new List<Vector3[]>();
        Vector3 playerPosXZ = MathHepler.GetXZ(Player.unit.transform.position);
        float playerMoveRange = Player.unit.moveDistance;
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
        foreach (Enemy enemy in this)
        {
            Vector3 startPosXZ = MathHepler.GetXZ(enemy.transform.position);
            Vector3 endPosXZ = startPosXZ;
            Quaternion newRot = Quaternion.LookRotation(playerPosXZ - endPosXZ);
            if (Vector3.Distance(startPosXZ, playerPosXZ) < enemy.moveDistance + enemy.weapon.length + playerMoveRange)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector3 movePosXZ = V3Random.RangeXZ(-enemy.moveDistance, enemy.moveDistance) + startPosXZ;
                    Vector3 guessPosXZ = V3Random.RangeXZ(-playerMoveRange, playerMoveRange) + playerPosXZ;
                    if (Vector3.Distance(movePosXZ, guessPosXZ) > enemy.weapon.length)
                    {
                        continue;
                    }
                    if (!IsPathBlocking(startPosXZ, movePosXZ))
                    {
                        endPosXZ = movePosXZ;
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
            tweens.Add(new CombatTween
            {
                lookat = enemy.LookAtTween(endPosXZ),
                move = enemy.MoveTween(endPosXZ),
                rotate = enemy.RotateTween(newRot),
            });
        }
        return tweens;
    }
}
