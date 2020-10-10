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
    public Unit playerUnit;
    Player player;
    Action stateUpdate;
    void Awake()
    {
        Enemies.Layer = LayerMask.GetMask("Enemy");
        player = new Player(playerUnit);
    }
    void Start()
    {
        stateUpdate = Selecting;
    }
    void Update()
    {
        stateUpdate();
    }
    void Selecting()
    {
        if (Mouse.HitPlayer())
        {
            player.MovementMask(true);
            if (Mouse.LeftDown)
            {
                player.Start();
                stateUpdate = SetPosition;
            }
        }
        else
        {
            player.MovementMask(false);
        }
    }
    void SetPosition()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            player.MoveTo(hit.point.x, hit.point.z);
            if (Mouse.LeftDown)
            {
                stateUpdate = SetRotation;
                player.AttackMask(true);
            }
        }
        if (Mouse.RightDown)
        {
            player.AttackMask(false);
            player.Reset();
            stateUpdate = Selecting;
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            player.LookAt(hit.point.x, hit.point.z);
            if (Mouse.RightDown)
            {
                player.Reset();
                stateUpdate = SetPosition;
            }
            else if (Mouse.LeftDown)
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
                player.Reset();
                player.MovementMask(false);
                player.AttackMask(false);
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
        player.Combat();
        Enemies.InScene.Combat();
        player.Reset();
        stateUpdate = Selecting;
    }
}
