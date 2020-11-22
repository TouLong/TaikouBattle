using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatus : MonoBehaviour
{
    public enum Type
    {
        Nothing = 0b001,
        Moving = 0b010,
        Attack = 0b100,
        Info = 0b1000,
    }
    [SerializeField]
    MeshRenderer movingRangeMesh, attackRangeMesh;
    [SerializeField]
    Healthbar healthBar;
    [SerializeField]
    Image icon;
    [SerializeField]
    Text nameText;
    public Transform MovingRange => movingRangeMesh.transform;
    public Transform AttackRange => attackRangeMesh.transform;
    public void Setup(Unit unit)
    {
        movingRangeMesh.transform.localScale = (Vector3.right + Vector3.up) * unit.moveDistance * 2.2f + Vector3.forward;
        attackRangeMesh.transform.localScale = (Vector3.right + Vector3.up) * unit.weapon.length * 2 + Vector3.forward;
        attackRangeMesh.material = unit.weapon.mask;
        Display(Type.Attack);
        healthBar.Setup(unit.maxHealth);
        healthBar.Set(unit.maxHealth);
    }
    public void Setup(UnitInfo info)
    {
        icon.sprite = info.icon;
        icon.color = info.team.color;
        nameText.text = info.name;
        nameText.color = info.team.color;
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
