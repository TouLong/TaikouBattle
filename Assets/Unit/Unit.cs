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

    public enum Range
    {
        Nothing = 0b001,
        Moving = 0b010,
        Attack = 0b100,
        All = 0b110,
    }
    public Transform model;
    public Weapon weapon;
    Transform mainHold;
    Transform subHold;

    public const int maxHp = 5;
    public const float maxAp = 1;
    public const float maxMoving = 1f;
    public const float maxTurning = 180f;
    [Range(1, maxHp)]
    public int hp = maxHp;
    [Range(0, maxAp)]
    public float ap = maxAp;
    [HideInInspector]
    public float moveConsume, turnConsume;
    int roundOfHurt;

    [HideInInspector]
    public MovingRange movingRange;
    GameObject attackRange;
    UnitStatus status;
    [HideInInspector]
    public Pose destination;
    [HideInInspector]
    public Team team;
    UnitMotion motion;
    [HideInInspector]
    public bool inCombat, inAction;
    [HideInInspector]
    public UnitColliders colliders;
    void Awake()
    {
        All.Add(this);
        Alive.Add(this);
        mainHold = model.Find("armature/both.r");
        subHold = model.Find("armature/single.l");
    }
    void Start()
    {
        SetupWeapon();
        destination.position = transform.position;
        destination.rotation = transform.rotation;
        status = GetComponentInChildren<UnitStatus>();
        status.Setup(this);
        colliders = GetComponentInChildren<UnitColliders>();
        colliders.transform.SetParent(model);
        movingRange = GetComponentInChildren<MovingRange>();
        movingRange.Setup(maxMoving);
        attackRange = transform.Find("AttackRange").gameObject;
        attackRange.GetComponent<MeshFilter>().mesh = weapon.GetRangeMesh();
        attackRange.transform.SetParent(model);
        motion = GetComponent<UnitMotion>();
        motion.Setup(this);
        motion.IdlePose();
        Display(Range.Attack);
    }
    public void SetupWeapon()
    {
        Transform mainWeapon = Instantiate(weapon.main, mainHold).transform;
        mainWeapon.localPosition = Vector3.zero;
        mainWeapon.localEulerAngles = Vector3.zero;
        mainWeapon.localScale = Vector3.one;
        if (weapon.sub != null)
        {
            Transform subWeapon = Instantiate(weapon.sub, subHold).transform;
            subWeapon.localPosition = Vector3.zero;
            subWeapon.localEulerAngles = Vector3.zero;
            subWeapon.localScale = Vector3.one;
        }
    }
    public void SetInfo(UnitInfo info)
    {
        status.Set(info);
    }
    public void Display(Range range)
    {
        movingRange.gameObject.SetActive(range.HasFlag(Range.Moving));
        attackRange.SetActive(range.HasFlag(Range.Attack));
    }
    public void ResetAction()
    {
        moveConsume = 0;
        turnConsume = 0;
        model.localPosition = Vector3.zero;
        model.localEulerAngles = Vector3.zero;
        movingRange.transform.localScale = Vector3.one;
        movingRange.transform.rotation = transform.rotation;
        SetAp(maxAp);
    }
    public void StartAction()
    {
        inAction = true;
        motion.PlayReady();
        transform.DORotateQuaternion(destination.rotation, 5 / 24f)
            .OnStart(() => {; });
        float dist = Vector3.Distance(destination.position, transform.position);
        transform.DOJump(destination.position, dist * 0.5f, 1, 5 / 24f)
            .OnComplete(() => { inAction = false; });
    }
    public void StartCombat()
    {
        inCombat = true;
        void action()
        {
            inCombat = false;
            ResetAction();
        }
        if (weapon.HitDetect(transform, team.enemies, out List<Unit> hits))
        {
            hits.ForEach(x => x.DamageBy(this));
            motion.Attack(action);
        }
        else
        {
            motion.ToIdle(action);
        }
    }
    public void EndCombat()
    {
        motion.IdlePose();
        if (roundOfHurt == 0)
            return;
        TextUI.Pop(roundOfHurt, Color.red, transform.position);
        hp = Mathf.Max(hp - roundOfHurt, 0);
        status.healthBar.Set(hp);
        roundOfHurt = 0;
        if (hp <= 0 && Alive.Contains(this))
        {
            Alive.Remove(this);
            team.alives.Remove(this);
            gameObject.SetActive(false);
        }
    }
    public void SetAp(float value)
    {
        ap = Mathf.Clamp(value, 0, maxAp);
        status.actionBar.Set(ap);
    }
    public void DamageBy(Unit unit)
    {
        roundOfHurt += unit.weapon.attack;
    }
}
