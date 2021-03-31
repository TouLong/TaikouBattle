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
    protected Transform movingRange, attackRange, turningRange, arrow;
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
    protected UnitStatus status;
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
    public float SignelAngle(Unit target) => Vector.ForwardSignedAngle(transform, target.position);
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
        if (info != null)
        {
            status.icon.color = info.team.color;
            status.icon.sprite = info.icon;
            status.nameText.color = info.team.color;
            status.nameText.text = info.name;
        }
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
        if (weapon.HitDetect(this, team.enemies, out List<Unit> hits))
        {
            hits.ForEach(x => x.DamageBy(this));
            motion.Attack(() =>
            {
                inCombat = false;
            });
        }
        else
        {
            inCombat = false;
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
    public void DamageBy(Unit unit)
    {
        roundOfHurt += unit.weapon.attack;
    }
    public void ScaleMovingRange(float size)
    {
        movingRange.localScale = new Vector3(size, 1, size);
        movingRange.rotation = model.rotation;
    }
    public RaycastHit HitMoveBorder(Vector3 direction, float range)
    {
        ScaleMovingRange(range);
        MeshCollider collider = movingRange.GetComponent<MeshCollider>();
        collider.enabled = true;
        Physics.Raycast(position, direction, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("MovingRange"));
        collider.enabled = false;
        return hit;
    }
}
