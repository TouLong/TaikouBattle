using System;
using UnityEngine;
using DG.Tweening;

public class Enemy : Unit
{
    MeshOutline outline;
    Player player;
    new void Start()
    {
        base.Start();
        outline = GetComponent<MeshOutline>();
        player = Player.self;
        AttackShow(true);
    }
    public bool IsPlayerInFront()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);
        if (distance > attackMask.nearLength && distance < attackMask.farLength)
        {
            return angle <= attackMask.range / 2;
        }
        return false;
    }
    public void HighLight(bool enable)
    {
        outline.enabled = enable;
    }
    void OnDestroy()
    {
        Enemies.InScene.Remove(this);
    }
    void OnEnable()
    {
        Enemies.InScene.Add(this);
    }
    void OnDisable()
    {
        Enemies.InScene.Remove(this);
    }
}
