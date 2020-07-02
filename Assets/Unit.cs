using System;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour
{
    [HideInInspector]
    public Transform moveRange;
    [HideInInspector]
    public AttackRange attackRange;
    [Range(1f, 20f)]
    public float moveRangeSize = 1;
    [Range(1f, 20f)]
    public float attackRangeSize = 1;
    [Range(30, 180)]
    public int attackRangeAngle;
    [Range(1, 20)]
    public float moveSpeed;
    [Range(1, 10)]
    public float rotateSpeed;
    protected AnimateBehaviour animator;
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
        moveRange.gameObject.SetActive(show);
    }
    public void AttackShow(bool show)
    {
        attackRange.gameObject.SetActive(show);
    }
    protected void OnValidate()
    {
        attackRange = GetComponentInChildren<AttackRange>();
        moveRange = transform.Find("MoveRange");
        if (moveRange)
        {
            moveRange.GetComponentInChildren<Projector>().orthographicSize = moveRangeSize * 1.2f;
        }
        if (attackRange)
        {
            attackRange.Config(attackRangeAngle, attackRangeSize);
        }
    }
}
