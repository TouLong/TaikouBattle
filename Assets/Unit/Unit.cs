using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour
{
    [SerializeField]
    bool showMovement = true, showAttack = false;
    [HideInInspector]
    public Transform movementMask, attackMask;
    public Weapon weapon;
    public Transform model;
    public Transform holdWeapon;
    [Range(1f, 20f)]
    public float moveDistance = 1;
    [Range(1f, 20f)]
    public float moveSpeed;
    [Range(1f, 10f)]
    public float rotateSpeed;
    AnimatorLayer moveAnim;
    AnimatorLayer actionAnim;

    protected void Start()
    {
        ConfigMask();
        MovementMask(false);
        AttackMask(false);
        OnGround();
        moveAnim = new AnimatorLayer(GetComponent<Animator>(), 0);
        actionAnim = new AnimatorLayer(GetComponent<Animator>(), 1);
        Instantiate(weapon, holdWeapon, false);
        Idle();
    }
    protected void OnGround()
    {
        float y = Map.GetHeight(transform.position.x, transform.position.z);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
    public Tween MoveTween(Vector3 newPos)
    {
        return transform.DOMove(newPos, Vector3.Distance(newPos, transform.position) / moveSpeed)
            .OnStart(() =>
            {
                moveAnim.CrossFade("move");
            })
            .OnUpdate(OnGround)
            .OnComplete(() =>
            {
                moveAnim.CrossFade("stand");
            });
    }
    public Tween LookAtTween(Vector3 toward)
    {
        if (toward == transform.position)
        {
            toward = transform.position + transform.forward;
        }
        return transform.DOLookAt(toward, 1.0f / rotateSpeed)
            .OnStart(() =>
            {
                moveAnim.CrossFade("turn");
            })
            .OnComplete(() =>
            {
                moveAnim.CrossFade("stand");
            });
    }
    public Tween RotateTween(Quaternion newRot)
    {
        return transform.DORotateQuaternion(newRot, 1.0f / rotateSpeed);
    }
    public void Damage(int attackPoint)
    {
        TextUI.Pop(attackPoint.ToString(), Color.red, transform.position);
    }
    public void Idle()
    {
        moveAnim.CrossFade("stand");
        actionAnim.CrossFade(weapon.type.ToString() + "_idle");
    }
    public void Attack(Action onCompleted)
    {
        actionAnim.CrossFadeEvent(weapon.type.ToString(), onCompleted, 0.6f);
    }
    public void MovementMask(bool b)
    {
        movementMask.gameObject.SetActive(b);
    }
    public void AttackMask(bool b)
    {
        attackMask.gameObject.SetActive(b);
    }
    public void HighLight(bool enable)
    {
    }
    public void ConfigMask()
    {
        Transform mask = transform.Find("Mask");
        if (mask)
        {
            movementMask = mask.Find("Movement");
            attackMask = mask.Find("Attack");
            if (movementMask)
                movementMask.localScale = (Vector3.right + Vector3.up) * moveDistance * 2.2f + Vector3.forward;
            if (attackMask)
            {
                attackMask.GetComponent<MeshRenderer>().material = weapon.mask;
                attackMask.localScale = (Vector3.right + Vector3.up) * weapon.length * 2 + Vector3.forward;
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
