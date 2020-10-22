using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit : MonoBehaviour
{
    static public List<Unit> InScene = new List<Unit>();

    public Weapon weapon;
    public Transform model;
    public Transform holdWeapon;

    [HideInInspector]
    public float moveDistance;
    [Range(1, 5)]
    public int maxHealth, health;
    float moveSpeed;
    float rotateSpeed;

    AnimatorLayer moveAnim;
    AnimatorLayer actionAnim;
    public Sequence action;
    [HideInInspector]
    public UnitStatus status;

    void Start()
    {
        weapon = Instantiate(weapon, holdWeapon, false);
        rotateSpeed = 5f - weapon.weight / 10;
        moveDistance = 2f - weapon.weight * 4 / 10;
        moveSpeed = moveDistance * 2f;
        status = GetComponent<UnitStatus>();
        status.Setup(this);
        moveAnim = new AnimatorLayer(GetComponent<Animator>(), 0);
        actionAnim = new AnimatorLayer(GetComponent<Animator>(), 1);
        OnGround();
        Idle();
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
    public void Combat(List<Unit> units)
    {
        if (weapon.HitDetect(transform, units, out List<Unit> hits))
        {
            Attack(() =>
            {
                hits.ForEach(x => x.DamageBy(this));
                Idle();
            });
        }
        else
        {
            Idle();
        }
    }
    public void DamageBy(Unit unit)
    {
        TextUI.Pop(unit.weapon.attack, Color.red, transform.position);
        health = Mathf.Max(health - unit.weapon.attack, 0);
        status.SetHealthBar(health);
        if (health <= 0)
        {
            weapon.transform.SetParent(null);
            weapon.gameObject.AddComponent<Rigidbody>();
            weapon.gameObject.AddComponent<BoxCollider>();
            enabled = false;
            status.Disable();
            moveAnim.CrossFade("none");
            actionAnim.CrossFade("none");
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.AddForceAtPosition(unit.transform.forward, transform.position, ForceMode.Impulse);
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.center = Vector3.up * 2;
            gameObject.AddComponent<OnTriggerGorund>().Setup(() =>
            {
                Destroy(rigidbody);
                Destroy(sphereCollider);
                GetComponent<Animator>().enabled = false;
                GetComponent<Collider>().enabled = false;
            }, 1.5f);
        }
    }
    public void Idle()
    {
        moveAnim.CrossFade("stand");
        actionAnim.CrossFade(weapon.type.ToString() + "-idle");
    }
    public void Attack(Action onCompleted)
    {
        actionAnim.CrossFadeEvent(weapon.type.ToString() + "-attack", onCompleted, 0.5f);
    }
    void OnGround()
    {
        transform.position.Set(transform.position.x,
            Map.GetHeight(transform.position.x, transform.position.z),
            transform.position.z);
    }
    void OnEnable()
    {
        InScene.Add(this);
    }
    void OnDisable()
    {
        InScene.Remove(this);
    }
    void OnDestroy()
    {
        InScene.Remove(this);
    }
}
