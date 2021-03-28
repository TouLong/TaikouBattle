using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatus : MonoBehaviour
{
    public Bar healthBar, actionBar;
    public Image icon;
    public Text nameText;
    public void Setup(Unit unit)
    {
        healthBar.Setup(unit.hp);
        healthBar.Set(unit.hp);
        actionBar.Setup(unit.ap);
        actionBar.Set(unit.ap);
    }
    public void Set(UnitInfo info)
    {
        icon.sprite = info.icon;
        icon.color = info.team.color;
        nameText.text = info.name;
        nameText.color = info.team.color;
    }
}
