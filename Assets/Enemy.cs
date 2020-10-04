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
        AttackMask(true);
    }
    public bool HitDetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position) * 2;
        if (distance > weapon.nearLength && distance < weapon.farLength)
        {
            return angle <= weapon.angle;
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
