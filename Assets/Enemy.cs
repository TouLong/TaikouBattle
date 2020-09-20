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
    public CombatTween Circling()
    {
        Vector3 newPos;
        Quaternion newRot;
        Vector3 selfPosXZ = MathHepler.GetXZ(transform.position);
        Vector3 playerPosXZ = MathHepler.GetXZ(player.transform.position);
        Vector3 guessPosXZ = V3Random.RangeXZ(-player.moveRangeSize, player.moveRangeSize) + playerPosXZ;
        if (Vector3.Distance(selfPosXZ, playerPosXZ) > moveRangeSize + player.moveRangeSize)
        {
            newPos = Vector3.ClampMagnitude(playerPosXZ - selfPosXZ, moveRangeSize) + selfPosXZ;
            newRot = Quaternion.LookRotation(playerPosXZ - newPos);
        }
        else if (Vector3.Distance(selfPosXZ, guessPosXZ) > moveRangeSize + attackRangeSize)
        {
            newPos = Vector3.ClampMagnitude(guessPosXZ - selfPosXZ, moveRangeSize) + selfPosXZ;
            newRot = Quaternion.LookRotation(guessPosXZ - newPos);
        }
        else
        {
            do
            {
                newPos = V3Random.RangeXZ(-moveRangeSize, moveRangeSize) + guessPosXZ;
            }
            while (Vector3.Distance(selfPosXZ, newPos) > attackRangeSize);
            newRot = Quaternion.LookRotation(guessPosXZ - newPos);
        }
        newPos.y = transform.position.y;
        CombatTween combat = new CombatTween
        {
            lookat = LookAtTween(newPos),
            move = MoveTween(newPos),
            rotate = RotateTween(newRot),
        };
        return combat;
    }
    public bool IsPlayerInFront()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);
        if (distance > attackRange.nearLength && distance < attackRange.farLength)
        {
            return angle <= attackRange.range / 2;
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
