using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleControl : MonoBehaviour
{
    Player player;
    Enemies hitEnemies = new Enemies();
    Action stateUpdate;
    void Awake()
    {
        Enemies.Layer = LayerMask.GetMask("Enemy");
        player = Player.InScene;
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
            //if (HitDetect.RayCast(player, out hightLightEnemies))
            if (HitDetect.Math(out hitEnemies))
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
                player.MoveToIndicator(() =>
                {
                    stateUpdate = Battle;
                });
                player.ShowIndicator(false);
                player.MovementShow(false);
                player.AttackShow(false);
                Enemies.InScene.HighLight(false);
                Enemies.InScene.TrackPlayer();
                stateUpdate = Moving;
            }
        }
    }
    void Moving()
    {
    }
    void Battle()
    {
        if (HitDetect.Math(out hitEnemies))
        {
            player.Punch(() =>
            {
                hitEnemies.Damage(1);
            });
        }
        stateUpdate = Selecting;
    }
}
