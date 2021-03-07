using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class UserControl
{
    public static Team team;
    public static Unit unit;

    static public void Setup()
    {
        team = null;
        if (Unit.player != null)
        {
            team = Unit.player.team;
            Team.NonUser.Remove(team);
        }
    }
    static public void Select(Unit unit)
    {
        UserControl.unit = unit;
        unit.colliders.model.enabled = false;
        unit.status.AttackRange.SetParent(unit.model, false);
    }
    static public void Deselect()
    {
        unit.model.localPosition = Vector3.zero;
        unit.model.localEulerAngles = Vector3.zero;
        unit.colliders.model.enabled = true;
        unit.Idle();
        unit = null;
    }
    static public void Complete()
    {
        unit.colliders.model.enabled = true;
        unit.destination.position = unit.model.position;
        unit.destination.rotation = unit.model.rotation;
        unit.model.localPosition = Vector3.zero;
        unit.model.localEulerAngles = Vector3.zero;
        unit = null;
    }
    static public void Action()
    {
        if (team == null)
            return;
        foreach (Unit unit in team.alives)
        {
            unit.StartAction();
            unit.status.AttackRange.SetParent(unit.transform, false);
            unit.model.localPosition = Vector3.zero;
            unit.model.localEulerAngles = Vector3.zero;
        }
    }
}
