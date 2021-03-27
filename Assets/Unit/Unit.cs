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
        Nothing = 0b00001,
        Moving = 0b00010,
        Turning = 0b00100,
        Attack = 0b01000,
        Arrow = 0b10000,
        All = 0b11110,
    }
    public Weapon weapon;

    public const float maxAp = 1;
    public const float maxMoving = 1f;
    public const float maxTurning = 360f;
    [HideInInspector]
    public Transform model;
    [HideInInspector]
    public int maxHp, hp;
    [HideInInspector]
    public float ap = maxAp;
    [HideInInspector]
    public float moveConsume, turnConsume;
    int roundOfHurt;

    Transform mainHold;
    Transform subHold;
    Transform movingRange, attackRange, turningRange, arrow;
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
        model = transform.Find("guy");
        mainHold = model.Find("armature/both.r");
        subHold = model.Find("armature/single.l");
    }
    void Start()
    {
        SetupWeapon();
        maxHp = 3 + weapon.armor;
        hp = maxHp;
        destination.position = transform.position;
        destination.rotation = transform.rotation;
        status = GetComponentInChildren<UnitStatus>();
        status.Setup(this);
        colliders = GetComponentInChildren<UnitColliders>();
        colliders.transform.SetParent(model);
        movingRange = transform.Find("MovingRange");
        turningRange = transform.Find("TurningRange");
        attackRange = transform.Find("AttackRange");
        arrow = transform.Find("Arrow");
        arrow.transform.SetParent(model);
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
        attackRange.gameObject.SetActive(range.HasFlag(Range.Attack));
        turningRange.gameObject.SetActive(range.HasFlag(Range.Turning));
        arrow.gameObject.SetActive(range.HasFlag(Range.Arrow));
    }
    public void ClampMovingRange(float distance)
    {
        movingRange.localScale = new Vector3(distance, 1, distance);
        movingRange.rotation = model.rotation;
    }
    public void ClampTurningRange(float angle)
    {
        Mesh mesh = GeoGenerator.SectorPlane((int)angle * 2, 0.7f, 0.67f, 0);
        turningRange.GetComponent<MeshFilter>().mesh = mesh;
        turningRange.rotation = model.rotation;
        turningRange.position = new Vector3(model.position.x, turningRange.position.y, model.position.z);
    }
    public void ResetAction()
    {
        moveConsume = 0;
        turnConsume = 0;
        model.localPosition = Vector3.zero;
        model.localEulerAngles = Vector3.zero;
        ClampMovingRange(maxAp);
        ClampTurningRange(maxTurning);
        SetAp(maxAp);
    }
    public void StartAction()
    {
        inAction = true;
        transform.DORotateQuaternion(destination.rotation, 4 / 24f);
        float dist = Vector3.Distance(destination.position, transform.position);
        transform.DOJump(destination.position, dist * 0.5f, 1, 4 / 24f)
            .OnComplete(() => { inAction = false; });
    }
    public void StartCombat()
    {
        inCombat = true;
        void reset()
        {
            inCombat = false;
            ResetAction();
        }
        if (weapon.HitDetect(transform, team.enemies, out List<Unit> hits))
        {
            hits.ForEach(x => x.DamageBy(this));
            motion.Attack(reset);
        }
        else
        {
            reset();
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
