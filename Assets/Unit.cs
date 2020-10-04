using System;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour
{
    [SerializeField]
    bool showMovement = true, showAttack = false;
    [HideInInspector]
    public Transform movementMask, attackMask;
    public Weapon weapon;
    [Range(1f, 20f)]
    public float moveDistance = 1;
    [Range(1f, 20f)]
    public float moveSpeed;
    [Range(1f, 10f)]
    public float rotateSpeed;
    [Range(1, 10)]
    public int HP;
    protected AnimateBehaviour animator;
    protected void Start()
    {
        ConfigMask();
        MovementMask(false);
        AttackMask(false);
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
    public void MovementMask(bool b)
    {
        movementMask.gameObject.SetActive(b);
    }
    public void AttackMask(bool b)
    {
        attackMask.gameObject.SetActive(b);
    }
    public void ConfigMask()
    {
        Transform mask = transform.Find("Mask");
        if (mask)
        {
            movementMask = mask.Find("Movement");
            attackMask = mask.Find("Attack");
            if (movementMask)
                movementMask.GetComponent<Projector>().orthographicSize = moveDistance * 1.2f;
            if (attackMask)
            {
                attackMask.GetComponent<Projector>().orthographicSize = weapon.length;
                attackMask.GetComponent<Projector>().material = weapon.mask;
            }
        }
    }
    protected void OnValidate()
    {
        ConfigMask();
        MovementMask(showMovement);
        AttackMask(showAttack);

    }
}
