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
    public float maxMoving, maxTurning, moving, turning, ap;
    [Range(1, 5)]
    public int maxHp, hp;
    AnimatorLayer anim;
    public Sequence action;
    public Pose destination;
    [HideInInspector]
    public bool inCombat;
    [HideInInspector]
    public UnitStatus status;
    [HideInInspector]
    public UnitColliders colliders;
    void Awake()
    {
        All.Add(this);
        Alive.Add(this);
    }
    void Start()
    {
        weapon = Instantiate(weapon, holdWeapon, false);
        destination.position = transform.position;
        destination.rotation = transform.rotation;
        maxMoving = 1.6f - weapon.weight * 4 / 10;
        maxTurning = 180f;
        ap = 1;
        status = GetComponent<UnitStatus>();
        colliders = GetComponentInChildren<UnitColliders>();
        colliders.transform.SetParent(model);
        status.Setup(this);
        anim = new AnimatorLayer(GetComponent<Animator>(), 0);
        OnGround();
        Standing();
    }
    public void MoveModel(Vector3 point)
    {
        Vector3 newPos = Vector3.ClampMagnitude(point - transform.position, maxMoving) + transform.position;
        moving = Vector3.Distance(transform.position, newPos);
        if (ApEstimate())
            model.position = newPos;
    }
    public void RotateModel(Vector3 point)
    {
        Vector3 newDir = new Vector3(point.x - transform.position.x, 0, point.z - transform.position.z);
        turning = Vector3.Angle(transform.forward, newDir);
        if (ApEstimate())
            model.rotation = Quaternion.LookRotation(newDir);
    }
    bool ApEstimate()
    {
        float moveConsume = moving / maxMoving;
        float rotateConsume = turning / maxTurning;
        ap = 1 - moveConsume - rotateConsume;
        if (ap > 0)
            status.SetActionBar(ap);
        else
            status.SetActionBar(0);
        return ap > 0;
    }
    public void StartAction()
    {
        string animation = weapon.type.ToString();
        if (Vector3.Angle(transform.forward, destination.position - transform.position) < 90)
            animation += "-front";
        else
            animation += "-back";
        action = DOTween.Sequence()
            .Append(transform.DORotateQuaternion(destination.rotation, 4 / 24f)
            .OnStart(() => { anim.Play(animation + "1"); })
            .OnKill(() => { anim.Play(animation + "3"); }))
            .Append(transform.DOMove(destination.position, 4 / 24f)
            .OnStart(() => { anim.Play(animation + "2"); })
            .OnUpdate(() =>
            {
                OnGround();
                if (colliders.IsTouch())
                    action.Kill();
            })
            .OnComplete(() => { anim.Play(animation + "3"); })
            .OnKill(() => { anim.Play(animation + "3"); }));
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
                //Idle();
            });
        }
        else
        {
            Idle();
        }
    }
    public void Idle()
    {
        inCombat = false;
        ap = 1;
        moving = 0;
        turning = 0;
        status.SetActionBar(ap);
        Standing();
    }
    public void DamageBy(Unit unit)
    {
        TextUI.Pop(unit.weapon.attack, Color.red, transform.position);
        hp = Mathf.Max(hp - unit.weapon.attack, 0);
        status.SetHealthBar(hp);
        if (hp <= 0 && Alive.Contains(this))
        {
            Alive.Remove(this);
            team.alives.Remove(this);
            weapon.transform.SetParent(null);
            weapon.gameObject.AddComponent<Rigidbody>();
            weapon.gameObject.AddComponent<BoxCollider>();
            status.Disable();
            anim.Play("empty");
            GetComponentsInChildren<SphereCollider>().ToList().ForEach(x => x.enabled = true);
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.AddForceAtPosition(unit.transform.forward, transform.position + Vector3.up * colliders.height / 2f, ForceMode.Impulse);
            gameObject.AddComponent<OnTriggerGorund>().Setup(() =>
            {
                Destroy(rigidbody);
                GetComponent<Animator>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponentsInChildren<SphereCollider>().ToList().ForEach(x => x.enabled = false);
            }, 1.5f);
        }
    }
    public void Standing()
    {
        anim.Play(weapon.type.ToString() + "-idle");
    }
    public void Attack(Action onCompleted)
    {
        anim.Play(weapon.type.ToString() + "-attack", onCompleted, 0.5f);
    }
    void OnGround()
    {
        transform.position = new Vector3(transform.position.x,
            Map.GetHeight(transform.position.x, transform.position.z),
            transform.position.z);
    }
}
