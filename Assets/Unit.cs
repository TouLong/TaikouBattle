using System;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour
{
    [HideInInspector]
    public Transform moveMask;
    [HideInInspector]
    public AttackRange attackMask;
    [Range(1f, 20f), Min(1f)]
    public float moveRange = 1;
    [Range(1f, 20f), Min(1f)]
    public float attackRange = 1;
    [Range(30, 180), Min(30)]
    public int attackAngle;
    [Range(1, 20)]
    public float moveSpeed;
    [Range(1, 10)]
    public float rotateSpeed;
    protected AnimateBehaviour animator;
    [Range(1, 10)]
    public int HP;
    protected void Start()
    {
        MovementShow(false);
        AttackShow(false);
        OnGround();
        animator = GetComponent<AnimateBehaviour>();
    }
    protected void OnGround()
    {
        float y = Map.GetHeight(transform.position.x, transform.position.z);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
    public Tween MoveTween(Vector3 newPos)
    {
        return transform.DOMove(newPos, Vector3.Distance(newPos, transform.position) / moveSpeed).OnUpdate(OnGround);
    }
    public Tween LookAtTween(Vector3 toward)
    {
        if (toward == transform.position)
        {
            toward = transform.position + transform.forward;
        }
        return transform.DOLookAt(toward, 1.0f / rotateSpeed);
    }
    public Tween RotateTween(Quaternion newRot)
    {
        return transform.DORotateQuaternion(newRot, 1.0f / rotateSpeed);
    }
    public void Damage(int attackPoint)
    {
        TextUI.Pop(attackPoint.ToString(), Color.red, transform.position);
    }
    public void Punch(Action onCompleted)
    {
        animator.Play("Punch", onCompleted);
    }
    public void MovementShow(bool show)
    {
        moveMask.gameObject.SetActive(show);
    }
    public void AttackShow(bool show)
    {
        attackMask.gameObject.SetActive(show);
    }
    protected void OnValidate()
    {
        attackMask = transform.Find("AttackRange").GetComponent<AttackRange>();
        moveMask = transform.Find("MoveRange");
        if (moveMask)
        {
            moveMask.GetComponentInChildren<Projector>().orthographicSize = moveRange * 1.2f;
        }
        if (attackMask)
        {
            attackMask.Config(attackAngle, attackRange);
        }
    }
}
