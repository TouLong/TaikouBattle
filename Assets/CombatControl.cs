using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public struct CombatTween
{
    public Tween lookat;
    public Tween move;
    public Tween rotate;
}
public class CombatControl : MonoBehaviour
{
    Player player;
    Enemies hitEnemies = new Enemies();
    Action stateUpdate;
    public Transform a;
    public Transform b;
    public Transform c;
    public Transform d;
    void Awake()
    {
        Enemies.Layer = LayerMask.GetMask("Enemy");
        player = Player.self;
    }
    void Start()
    {
        stateUpdate = Selecting;
    }
    void Update()
    {
        stateUpdate();
        //print(MathHepler.IntersectXZ(a.position, b.position, c.position, d.position));
    }
    void Selecting()
    {
        if (Mouse.HitPlayer())
        {
            player.MovementShow(true);
            if (Mouse.LeftDown)
            {
                player.ShowIndicator(true);
                stateUpdate = SetPosition;
            }
        }
        else
        {
            player.MovementShow(false);
        }
    }
    void SetPosition()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            player.MoveIndicator(new Vector3(hit.point.x, player.transform.position.y, hit.point.z));
            if (Mouse.LeftDown)
            {
                stateUpdate = SetRotation;
                player.AttackShow(true);
            }
        }
        if (Mouse.RightDown)
        {
            player.AttackShow(false);
            player.ShowIndicator(false);
            stateUpdate = Selecting;
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            player.RotateIndicator(new Vector3(hit.point.x, player.transform.position.y, hit.point.z));
            if (player.HitDetect(out hitEnemies))
            {
                foreach (Enemy enemy in Enemies.InScene)
                {
                    if (hitEnemies.Contains(enemy))
                    {
                        enemy.HighLight(true);
                    }
                    else
                    {
                        enemy.HighLight(false);
                    }
                }
            }
            else
            {
                Enemies.InScene.HighLight(false);
            }
            if (Mouse.RightDown)
            {
                player.ResetIndicator();
                stateUpdate = SetPosition;
            }
            if (Mouse.LeftDown)
            {
                CombatTween playerTween = player.Circling();
                List<CombatTween> combatTween = Enemies.InScene.Circling();
                Sequence sequence = DOTween.Sequence();
                Sequence lookatSeq = DOTween.Sequence().Append(playerTween.lookat);
                Sequence moveSeq = DOTween.Sequence().Append(playerTween.move);
                Sequence rotateSeq = DOTween.Sequence().Append(playerTween.rotate);
                foreach (CombatTween tween in combatTween)
                {
                    lookatSeq.Join(tween.lookat);
                    moveSeq.Join(tween.move);
                    rotateSeq.Join(tween.rotate);
                }
                sequence.SetEase(Ease.Linear);
                sequence.Append(lookatSeq).Append(moveSeq).Append(rotateSeq);
                sequence.OnComplete(() => { stateUpdate = Combat; });
                player.ShowIndicator(false);
                player.MovementShow(false);
                player.AttackShow(false);
                Enemies.InScene.HighLight(false);
                stateUpdate = Moving;
            }
        }
    }
    void Moving()
    {

    }
    void Combat()
    {
        if (player.HitDetect(out hitEnemies))
        {
            hitEnemies.HighLight(true);
            player.AttackShow(true);
            player.Punch(() =>
            {
                hitEnemies.HighLight(false);
                player.AttackShow(false);
                hitEnemies.Damage(1);
            });
        }
        foreach (Enemy enemy in Enemies.InScene)
        {
            if (enemy.IsPlayerInFront())
            {
                enemy.Punch(() =>
                {
                    player.Damage(1);
                });
            }
        }
        player.ResetIndicator();
        stateUpdate = Selecting;
    }
}
