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
    public const int baseHp = 3;
    public const float maxAp = 1;
    public const float maxMoving = 1f;
    public const float maxTurning = 360f;

    public Weapon weapon;
    Transform movingRange, attackRange, turningRange, arrow;
    [HideInInspector]
    public Transform model;
    [HideInInspector]
    public int maxHp, hp;
    [HideInInspector]
    public float ap = maxAp;
    [HideInInspector]
    public float moveConsume, turnConsume;
    int roundOfHurt;
    [HideInInspector]
    public Pose destination;
    Transform mainHold;
    Transform subHold;
    UnitStatus status;
    [HideInInspector]
    public UnitInfo info;
    [HideInInspector]
    public Team team;
    UnitMotion motion;
    [HideInInspector]
    public bool inCombat, inAction;
    [HideInInspector]
    public UnitColliders colliders;

    public Vector3 position => transform.position;
    public Vector3 euler => transform.eulerAngles;
    public Quaternion rotation => transform.rotation;
    public Vector3 Forward(Unit target) => (target.position - position).normalized;
    public Vector3 Backward(Unit target) => (position - target.position).normalized;
    public float Angle(Unit target) => Vector.ForwardAngle(transform, target.position);
    public float Distance(Unit target) => Vector3.Distance(position, target.position);

    void Awake()
    {
        All.Add(this);
        Alive.Add(this);
        model = transform.Find("guy");
        mainHold = model.Find("armature/both.r");
        subHold = model.Find("armature/single.l");
        movingRange = transform.Find("MovingRange");
        turningRange = transform.Find("TurningRange");
        attackRange = transform.Find("AttackRange");
        arrow = transform.Find("Arrow");
    }
    void Start()
    {
        SetupWeapon();
        attackRange.GetComponent<MeshFilter>().mesh = weapon.GetRangeMesh();
        maxHp = baseHp + weapon.armor;
        hp = maxHp;
        destination.position = transform.position;
        destination.rotation = transform.rotation;
        arrow.transform.SetParent(model);
        attackRange.transform.SetParent(model);
        status = GetComponentInChildren<UnitStatus>();
        status.Setup(this);
        colliders = GetComponentInChildren<UnitColliders>();
        colliders.transform.SetParent(model);
        motion = GetComponent<UnitMotion>();
        motion.Setup(this);
        motion.IdlePose();
        status.icon.color = info.team.color;
        status.icon.sprite = info.icon;
        status.nameText.color = info.team.color;
        status.nameText.text = info.name;
        Display(Range.Attack);
    }
    public void Display(Range range)
    {
        movingRange.GetComponent<MeshRenderer>().enabled = range.HasFlag(Range.Moving);
        attackRange.gameObject.SetActive(range.HasFlag(Range.Attack));
        turningRange.gameObject.SetActive(range.HasFlag(Range.Turning));
        arrow.gameObject.SetActive(range.HasFlag(Range.Arrow));
    }
    public void SetupWeapon()
    {
        Transform mainWeapon = Instantiate(weapon.main, mainHold).transform;
        mainWeapon.localPosition = Vector3.zero;
        mainWeapon.localEulerAngles = Vector3.zero;
        mainWeapon.localScale = Vector3.one;
        if (weapon.sub == null)
            return;
        Transform subWeapon = Instantiate(weapon.sub, subHold).transform;
        subWeapon.localPosition = Vector3.zero;
        subWeapon.localEulerAngles = Vector3.zero;
        subWeapon.localScale = Vector3.one;
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
        transform.DORotateQuaternion(destination.rotation, 8 / 24f);
        float dist = Vector3.Distance(destination.position, transform.position);
        transform.DOJump(destination.position, dist * 0.25f, 1, 8 / 24f)
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
        if (weapon.HitDetect(this, team.enemies, out List<Unit> hits))
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
            gameObject.SetActive(false);
            team.alives.Remove(this);
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
    public void ClampMovingRange(float scale)
    {
        movingRange.localScale = new Vector3(scale, 1, scale);
        movingRange.rotation = model.rotation;
    }
    public void ClampTurningRange(float angle)
    {
        Mesh mesh = GeoGenerator.SectorPlane((int)angle * 2, 0.7f, 0.67f, 0);
        turningRange.GetComponent<MeshFilter>().mesh = mesh;
        turningRange.rotation = model.rotation;
        turningRange.position = new Vector3(model.position.x, turningRange.position.y, model.position.z);
    }
    public Vector3 GetMaxMovePoint(Vector3 direction, float distance)
    {
        ClampMovingRange(distance);
        MeshCollider collider = movingRange.GetComponent<MeshCollider>();
        collider.enabled = true;
        Physics.Raycast(position, direction, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("MovingRange"));
        collider.enabled = false;
        return hit.point;
    }
    public void Chase(Unit target)
    {
        float angle = Angle(target);
        angle += UnityEngine.Random.Range(-weapon.angle, weapon.angle);
        float moveRange = ap - angle / maxTurning;
        Vector3 dir = Forward(target);
        destination.position = GetMaxMovePoint(dir, moveRange);
        destination.rotation = Quaternion.LookRotation(dir);
    }
    public void KeepAway(Unit target)
    {
        float angle = Angle(target);
        angle += UnityEngine.Random.Range(-weapon.angle, weapon.angle);
        float moveRange = ap - angle / maxTurning;
        Vector3 backward = Backward(target);
        Vector3 forward = Forward(target);
        destination.position = GetMaxMovePoint(backward, moveRange);
        destination.rotation = Quaternion.LookRotation(forward);
    }
    public void Guess(Unit target)
    {
        float look = Angle(target);
        look += UnityEngine.Random.Range(-weapon.angle, weapon.angle);
        float moveRange = ap - look / maxTurning;
        Vector3 moveDir = V3Random.DirectionXz();
        destination.position = GetMaxMovePoint(moveDir, moveRange);
        destination.rotation = Quaternion.Euler(0, euler.y + look, 0);
    }
}
