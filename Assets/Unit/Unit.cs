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
    public Transform holdWeapon;
    public Weapon weapon;

    public const int maxHp = 5;
    public const float maxAp = 1;
    [Range(1, maxHp)]
    public int hp = maxHp;
    [Range(0, maxAp)]
    public float ap = maxAp;
    [HideInInspector]
    public float maxMoving, maxTurning, moveConsume, turnConsume;
    int roundOfHurt;

    [HideInInspector]
    public MovingRange movingRange;
    [HideInInspector]
    public AttackRange attackRange;
    UnitStatus status;
    [HideInInspector]
    public Pose destination;
    [HideInInspector]
    public Team team;
    AnimatorLayer anim;
    [HideInInspector]
    public bool inCombat, inAction;
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
        //maxMoving = 1.8f - weapon.weight * 0.5f;
        maxMoving = 1f;
        maxTurning = 180f;
        anim = new AnimatorLayer(GetComponent<Animator>(), 0);
        status = GetComponentInChildren<UnitStatus>();
        status.Setup(this);
        colliders = GetComponentInChildren<UnitColliders>();
        colliders.transform.SetParent(model);
        movingRange = GetComponentInChildren<MovingRange>();
        movingRange.Setup(maxMoving);
        attackRange = GetComponentInChildren<AttackRange>();
        attackRange.Setup(weapon);
        attackRange.transform.SetParent(model);
        Standing();
        Display(Range.Nothing);
    }
    public void SetInfo(UnitInfo info)
    {
        status.Set(info);
    }
    public void Display(Range range)
    {
        movingRange.gameObject.SetActive(range.HasFlag(Range.Moving));
        attackRange.gameObject.SetActive(range.HasFlag(Range.Attack));
    }
    public void StartAction()
    {
        inAction = true;
        transform.DORotateQuaternion(destination.rotation, 4 / 24f)
            .OnStart(() => {; });
        float dist = Vector3.Distance(destination.position, transform.position);
        transform.DOJump(destination.position, dist * 0.5f, 1, 4 / 24f)
            .OnComplete(() => { inAction = false; });
    }
    public void StartCombat()
    {
        inCombat = true;
        if (weapon.HitDetect(transform, team.enemies, out List<Unit> hits))
        {
            hits.ForEach(x => x.DamageBy(this));
        }
        Standing();
        inCombat = false;
        ResetModel();
    }
    public void EndCombat()
    {
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
    public void ResetModel()
    {
        moveConsume = 0;
        turnConsume = 0;
        model.localPosition = Vector3.zero;
        model.localEulerAngles = Vector3.zero;
        movingRange.transform.localScale = Vector3.one;
        movingRange.transform.rotation = transform.rotation;
        SetAp(maxAp);
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
    public void Standing()
    {
        anim.Play(weapon.handleType.ToString() + "-" + weapon.attackType.ToString());
    }
}
