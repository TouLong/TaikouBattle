using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RangeDisplayType
{
    Nothing = 0b001,
    Moving = 0b010,
    Attack = 0b100,
}
public class UnitStatus : MonoBehaviour
{
    [HideInInspector]
    public MeshRenderer movingRange, attackRange;
    Healthbar healthBar;
    public void Setup(Unit unit)
    {
        movingRange = transform.Find("MovingRange").GetComponent<MeshRenderer>();
        attackRange = transform.Find("AttackRange").GetComponent<MeshRenderer>();
        healthBar = transform.Find("Status").GetComponentInChildren<Healthbar>();
        movingRange.transform.localScale = (Vector3.right + Vector3.up) * unit.moveDistance * 2.2f + Vector3.forward;
        attackRange.transform.localScale = (Vector3.right + Vector3.up) * unit.weapon.length * 2 + Vector3.forward;
        attackRange.material = unit.weapon.mask;
        Display(RangeDisplayType.Attack);
        healthBar.Setup(unit.maxHealth);
        healthBar.Set(unit.maxHealth);
    }
    public void Display(RangeDisplayType type)
    {
        movingRange.enabled = type.HasFlag(RangeDisplayType.Moving);
        attackRange.enabled = type.HasFlag(RangeDisplayType.Attack);
    }
    public void SetHealthBar(float health)
    {
        healthBar.Set(health);
    }
    public void Disable()
    {
        enabled = false;
        Display(RangeDisplayType.Nothing);
        healthBar.gameObject.SetActive(false);
    }
}
