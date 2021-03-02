using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Unit : MonoBehaviour
{
    static public List<Unit> All = new List<Unit>();
    static public List<Unit> Alive = new List<Unit>();
    static public Unit player;

    public Transform model;
    public Transform holdWeapon;
    [HideInInspector]
    public Team team;
    public Weapon weapon;

    [HideInInspector]
    public float moveDistance;
    [Range(1, 100)]
    public int maxHealth, health;
    float moveSpeed;
    float rotateSpeed;
    [HideInInspector]
    public float radius, height;

    AnimatorLayer moveAnim, actionAnim;
    public Sequence action;
    [HideInInspector]
    public bool inCombat;
    [HideInInspector]
    public UnitStatus status;
    void Awake()
    {
        All.Add(this);
        Alive.Add(this);
    }
    void Start()
    {
        weapon = Instantiate(weapon, holdWeapon, false);
        rotateSpeed = 5f - weapon.weight / 10;
        moveDistance = 2f - weapon.weight * 4 / 10;
        moveSpeed = moveDistance * 2f;
        radius = GetComponent<CapsuleCollider>().radius;
        height = GetComponent<CapsuleCollider>().height;
        status = GetComponent<UnitStatus>();
        status.Setup(this);
        moveAnim = new AnimatorLayer(GetComponent<Animator>(), 0);
        actionAnim = new AnimatorLayer(GetComponent<Animator>(), 1);
        OnGround();
        Idle();
    }
    public Tween MoveTween(Vector3 newPos)
    {
        newPos.Set(newPos.x, Map.GetHeight(newPos.x, newPos.z), newPos.z);
        return transform.DOMove(newPos, Vector3.Distance(newPos, transform.position) / moveSpeed)
            .OnStart(Moving)
            .OnUpdate(OnGround)
            .OnUpdate(TouchCheck)
            .OnComplete(Standing)
            .OnKill(Standing);
    }
    public Tween LookAtTween(Vector3 toward)
    {
        if (toward == transform.position)
        {
            toward = transform.position + transform.forward;
        }
        return transform.DOLookAt(toward, 1.0f / rotateSpeed)
            .OnStart(Turning)
            .OnComplete(Standing)
            .OnKill(Standing);
    }
    public Tween RotateTween(Quaternion newRot)
    {
        return transform.DORotateQuaternion(newRot, 1.0f / rotateSpeed);
    }
    public void Combat()
    {
        inCombat = true;
        if (weapon.HitDetect(transform, team.enemies, out List<Unit> hits))
        {
            Attack(() =>
            {
                hits.ForEach(x => x.DamageBy(this));
                inCombat = false;
                Idle();
            });
        }
        else
        {
            inCombat = false;
            Idle();
        }
    }
    public void DamageBy(Unit unit)
    {
        TextUI.Pop(unit.weapon.attack, Color.red, transform.position);
        health = Mathf.Max(health - unit.weapon.attack, 0);
        status.SetHealthBar(health);
        if (health <= 0 && Alive.Contains(this))
        {
            Alive.Remove(this);
            team.alives.Remove(this);
            weapon.transform.SetParent(null);
            weapon.gameObject.AddComponent<Rigidbody>();
            weapon.gameObject.AddComponent<BoxCollider>();
            status.Disable();
            moveAnim.Play("none");
            actionAnim.Play("none");
            GetComponentsInChildren<SphereCollider>().ToList().ForEach(x => x.enabled = true);
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.AddForceAtPosition(unit.transform.forward, transform.position + Vector3.up * height / 2f, ForceMode.Impulse);
            gameObject.AddComponent<OnTriggerGorund>().Setup(() =>
            {
                Destroy(rigidbody);
                GetComponent<Animator>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponentsInChildren<SphereCollider>().ToList().ForEach(x => x.enabled = false);
            }, 1.5f);
        }
    }
    public void Idle()
    {
        moveAnim.Play("stand");
        actionAnim.Play(weapon.type.ToString() + "-idle");
    }
    public void Standing()
    {
        moveAnim.Play("stand");
    }
    public void Moving()
    {
        moveAnim.Play("move");
    }
    public void Turning()
    {
        moveAnim.Play("turn");
    }
    public void Attack(Action onCompleted)
    {
        actionAnim.Play(weapon.type.ToString() + "-attack", onCompleted, 0.5f);
    }
    void OnGround()
    {
        transform.position = new Vector3(transform.position.x,
            Map.GetHeight(transform.position.x, transform.position.z),
            transform.position.z);
    }
    void TouchCheck()
    {
        if (Physics.CapsuleCast(transform.position, transform.position + Vector3.up * height, radius, transform.forward
            , out RaycastHit hit, radius / 2f, LayerMask.GetMask("Unit")))
        {
            action.Kill();
        }
    }
}
