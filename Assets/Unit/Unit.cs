using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public enum Highlight
    {
        Nothing = 0b000001,
        Moving = 0b0000010,
        Turning = 0b0000100,
        Attack = 0b0001000,
        Arrow = 0b0010000,
        Outline = 0b0100000,
        Info = 0b1000000,
        All = 0b1111110,
    }
    public const int baseHp = 3;
    public const float maxAp = 1;
    public const float maxMoving = 1f;
    public const float maxTurning = 360f;

    public Weapon weapon;
    protected Transform model, mainHold, subHold, movingRange, attackRange, turningRange, arrow;
    protected Pose destination;
    [HideInInspector]
    public int hp;
    int roundOfHurt;
    Bar healthBar;
    Image icon;
    Text nameText;
    UnitMotion motion;
    Outline outline;
    public UnitInfo info;
    public Team team;
    Rigidbody rig;
    public List<Unit> enemies => team.enemies;
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
        if (movingClamp == null)
            movingClamp = GameObject.Find("Game/MovingClamp").GetComponent<MeshCollider>();
        All.Add(this);
        Alive.Add(this);
        model = transform.Find("guy");
        mainHold = model.Find("armature/both.r");
        subHold = model.Find("armature/single.l");
        movingRange = transform.Find("MovingRange");
        turningRange = transform.Find("TurningRange");
        attackRange = transform.Find("AttackRange");
        arrow = transform.Find("Arrow");
        healthBar = transform.Find("Status/Health").GetComponent<Bar>();
        icon = transform.Find("Status/Info/Icon").GetComponent<Image>();
        nameText = transform.Find("Status/Info/Name").GetComponent<Text>();
    }
    void Start()
    {
        SetupWeapon();
        hp = baseHp + weapon.armor;
        destination.position = transform.position;
        destination.rotation = transform.rotation;
        attackRange.GetComponent<MeshFilter>().mesh = weapon.rangeMesh;
        arrow.transform.SetParent(model);
        healthBar.Setup(hp);
        motion = GetComponent<UnitMotion>();
        motion.Setup(this);
        motion.IdlePose();
        outline = model.GetComponent<Outline>();
        rig = GetComponent<Rigidbody>();
        icon.color = team.color;
        icon.sprite = info.icon;
        outline.OutlineColor = team.color;
        if (info != null)
        {
            nameText.color = team.color;
            nameText.text = info.name;
        }
        Display(Highlight.Attack);
    }
    public void Display(Highlight range)
    {
        movingRange.gameObject.SetActive(range.HasFlag(Highlight.Moving));
        attackRange.gameObject.SetActive(range.HasFlag(Highlight.Attack));
        turningRange.gameObject.SetActive(range.HasFlag(Highlight.Turning));
        arrow.gameObject.SetActive(range.HasFlag(Highlight.Arrow));
        outline.OutlineWidth = range.HasFlag(Highlight.Outline) ? 10f : 5f;
        nameText.enabled = range.HasFlag(Highlight.Info);
        icon.enabled = range.HasFlag(Highlight.Info);
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
    public void DamageBy(Unit unit)
    {
        roundOfHurt += unit.weapon.attack;
    }
    public RaycastHit HitMoveBorder(Vector3 direction)
    {
        movingClamp.transform.position = movingRange.position;
        movingClamp.transform.rotation = movingRange.rotation;
        movingClamp.transform.localScale = movingRange.localScale;
        movingClamp.enabled = true;
        Physics.Raycast(position, direction, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("MovingRange"));
        movingClamp.enabled = false;
        return hit;
    }

    static public List<Unit> All = new List<Unit>();
    static public List<Unit> Alive = new List<Unit>();
    static public Unit player;
    static public MeshCollider movingClamp;
    static public void Action(System.Action onCompleted)
    {
        Sequence action = DOTween.Sequence();
        foreach (Unit unit in Alive)
        {
            Pose destination = unit.destination;
            float dist = Vector3.Distance(destination.position, unit.position);
            action.Join(unit.transform.DORotateQuaternion(destination.rotation, 8 / 24f));
            action.Join(unit.transform.DOJump(destination.position, dist * 0.25f, 1, 8 / 24f));
            unit.rig.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        action.AppendInterval(0.1f);
        action.AppendCallback(() =>
        {
            float sec = 0;
            foreach (Unit unit in Alive)
            {
                unit.rig.constraints = RigidbodyConstraints.FreezeAll;
                if (unit.weapon.HitDetect(unit, unit.enemies, out List<Unit> hits))
                {
                    unit.Display(Highlight.Attack);
                    hits.ForEach(x => x.DamageBy(unit));
                    sec = Mathf.Max(sec, unit.motion.Attack());
                }
            }
            Sequence combat = DOTween.Sequence();
            combat.AppendInterval(sec);
            combat.AppendCallback(() =>
            {
                foreach (Unit unit in Alive)
                {
                    unit.motion.IdlePose();
                    if (unit.roundOfHurt == 0)
                        continue;
                    TextUI.Pop(unit.roundOfHurt, Color.red, unit.position);
                    unit.hp = Mathf.Max(unit.hp - unit.roundOfHurt, 0);
                    unit.healthBar.Set(unit.hp);
                    unit.roundOfHurt = 0;
                    if (unit.hp <= 0 && Alive.Contains(unit))
                    {
                        unit.gameObject.SetActive(false);
                        unit.team.memebers.Remove(unit);
                    }
                }
            });
            combat.OnComplete(() => onCompleted());
        });
    }
}
