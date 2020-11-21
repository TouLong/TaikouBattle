using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatus : MonoBehaviour
{
    public enum Type
    {
        Nothing = 0b001,
        Moving = 0b010,
        Attack = 0b100,
    }
    MeshRenderer movingRangeMesh, attackRangeMesh;
    Healthbar healthBar;
    public Transform MovingRange => movingRangeMesh.transform;
    public Transform AttackRange => attackRangeMesh.transform;
    public void Setup(Unit unit)
    {
        movingRangeMesh = transform.Find("MovingRange").GetComponent<MeshRenderer>();
        attackRangeMesh = transform.Find("AttackRange").GetComponent<MeshRenderer>();
        healthBar = transform.Find("Status").GetComponentInChildren<Healthbar>();
        movingRangeMesh.transform.localScale = (Vector3.right + Vector3.up) * unit.moveDistance * 2.2f + Vector3.forward;
        attackRangeMesh.transform.localScale = (Vector3.right + Vector3.up) * unit.weapon.length * 2 + Vector3.forward;
        attackRangeMesh.material = unit.weapon.mask;
        Display(Type.Attack);
        healthBar.Setup(unit.maxHealth);
        healthBar.Set(unit.maxHealth);
    }
    public void Display(Type type)
    {
        movingRangeMesh.enabled = type.HasFlag(Type.Moving);
        attackRangeMesh.enabled = type.HasFlag(Type.Attack);
    }
    public void SetHealthBar(float health)
    {
        healthBar.Set(health);
    }
    public void Disable()
    {
        Display(Type.Nothing);
        healthBar.gameObject.SetActive(false);
    }
}
