using System;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Enemy : Unit
{
    MeshOutline outline;

    new void Start()
    {
        base.Start();
        outline = GetComponent<MeshOutline>();
    }

    public void TrackPlayer()
    {
        Vector3 newPos = Vector3.ClampMagnitude(Player.InScene.transform.position - transform.position, moveRangeSize) + transform.position;
        float moveAngleOffset = Vector3.Angle(transform.forward, newPos - transform.position);
        Vector3 toward = new Vector3(newPos.x, transform.position.y, newPos.z);
        Sequence sequence = DOTween.Sequence();
        sequence.SetEase(Ease.Linear);
        sequence.Append(transform.DOLookAt(toward, moveAngleOffset * Mathf.Deg2Rad / rotateSpeed));
        sequence.Append(transform.DOMove(newPos, Vector3.Distance(newPos, transform.position) / moveSpeed).OnUpdate(OnGround));
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
