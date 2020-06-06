using System;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    MeshOutline outline;
    Action stateUpdate;
    NavMeshAgent navAgent;
    public float moveSpeed;
    public float rotateSpeed;
    public AttackRange attackRange;
    AnimateBehaviour animator;

    void Start()
    {
        outline = GetComponent<MeshOutline>();
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<AnimateBehaviour>();
        stateUpdate = Freezing;
    }
    void Update()
    {
        stateUpdate();
    }
    void Freezing()
    {
        navAgent.SetDestination(transform.position);
    }
    public void Punch(Action onCompleted)
    {
        animator.Play("Punch", onCompleted);
        stateUpdate = Freezing;
    }
    public void Freeze()
    {
        stateUpdate = Freezing;
    }
    public void TrackPlayer()
    {
        navAgent.SetDestination(Player.self.transform.position);
        stateUpdate = TrackPlayer;
        if (HitDetect.Math(this))
        {
            Punch(() =>
            {
                Player.self.Damage(1);
            });
        }
    }
    public void Damage(int attackPoint)
    {
        TextUI.Pop(attackPoint.ToString(), Color.red, transform.position);
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
