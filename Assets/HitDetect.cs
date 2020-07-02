using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitDetect : MonoBehaviour
{
    static public bool RayCast(out Enemies hitEnemies)
    {
        Player player = Player.InScene;
        float rad = (player.attackRange.range / 2 - player.indicator.eulerAngles.y + 90) * Mathf.Deg2Rad;
        Vector3 orgin = player.indicator.position + Vector3.up * 2;
        RaycastHit[] hits;
        hitEnemies = new Enemies();
        for (int i = 0; i < player.attackRange.range; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            hits = Physics.RaycastAll(orgin + player.attackRange.nearLength * dir, dir, player.attackRange.farLength, Enemies.Layer);
            if (hits.Any())
            {
                hitEnemies.AddRange(hits.Select(x => x.transform.GetComponent<Enemy>()));
            }
            rad -= Mathf.Deg2Rad;
        }
        return hitEnemies.Any();
    }
    static public bool Math(out Enemies hitEnemies)
    {
        float distance;
        float angle;
        hitEnemies = new Enemies();
        Player player = Player.InScene;
        foreach (Enemy enemy in Enemies.InScene)
        {
            distance = Vector3.Distance(enemy.transform.position, player.indicator.position);
            if (distance > player.attackRange.nearLength && distance < player.attackRange.farLength)
            {
                angle = Vector3.Angle(player.indicator.forward, enemy.transform.position - player.indicator.position);

                if (angle <= player.attackRange.range / 2)
                {
                    hitEnemies.Add(enemy);
                }
            }
        }
        return hitEnemies.Any();
    }
    static public bool Math(Enemy enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, Player.InScene.transform.position);
        if (distance > enemy.attackRange.nearLength && distance < enemy.attackRange.farLength)
        {
            float angle = Vector3.Angle(enemy.transform.forward, Player.InScene.transform.position - enemy.transform.position);
            return angle <= enemy.attackRange.range / 2;
        }
        return false;
    }
}
