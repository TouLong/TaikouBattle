using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemies : List<Enemy>
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
    public List<CombatTween> Circling()
    {
        List<CombatTween> tweens = new List<CombatTween>();
        List<Vector3[]> attackPath = new List<Vector3[]>();
        Vector3 playerPosXZ = MathHepler.GetXZ(Player.self.transform.position);
        foreach (Enemy enemy in this)
        {
            Vector3 selfPosXZ = MathHepler.GetXZ(enemy.transform.position);
            Vector3 newPos = Vector3.ClampMagnitude(playerPosXZ - selfPosXZ, enemy.moveRangeSize) + selfPosXZ;
            Quaternion newRot = Quaternion.LookRotation(playerPosXZ - newPos);
            for (int i = 0; i < 10; i++)
            {
                Vector3 guessPosXZ = V3Random.RangeXZ(-Player.self.moveRangeSize, Player.self.moveRangeSize) + playerPosXZ;
                Vector3 attackPosXZ = V3Random.RangeXZ(-enemy.moveRangeSize, enemy.moveRangeSize) + selfPosXZ;

                bool guessTooFar = Vector3.Distance(selfPosXZ, guessPosXZ) > enemy.moveRangeSize + enemy.attackRangeSize;
                bool attackTooFar = Vector3.Distance(attackPosXZ, guessPosXZ) > enemy.attackRangeSize;
                bool pathBlocking = false;
                for (int j = 0; j < attackPath.Count; j++)
                {
                    if (MathHepler.IntersectXZ(attackPath[j][0], attackPath[j][0], selfPosXZ, attackPosXZ))
                    {
                        pathBlocking = true;
                        break;
                    }
                }

                if (!guessTooFar && !attackTooFar && !pathBlocking)
                {
                    attackPath.Add(new Vector3[] { selfPosXZ, attackPosXZ });
                    newPos = attackPosXZ;
                    newRot = Quaternion.LookRotation(guessPosXZ - newPos);
                    break;
                }
            }
            newPos.y = enemy.transform.position.y;
            tweens.Add(new CombatTween
            {
                lookat = enemy.LookAtTween(newPos),
                move = enemy.MoveTween(newPos),
                rotate = enemy.RotateTween(newRot),
            });
        }
        return tweens;
    }
}
