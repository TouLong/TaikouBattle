using UnityEngine;
using System.Linq;

public class PlayerControl
{
    static public Team team;
    static public Unit unit;
    static Unit highlight;

    static public void Setup()
    {
        team = null;
        if (Unit.player != null)
        {
            team = Unit.player.team;
            Team.NonUser.Remove(team);
        }
    }
    static public void Highlight(Unit unit = null)
    {
        if (unit)
        {
            highlight = unit;
            highlight.Display(Unit.Range.Moving);
        }
        else if (highlight)
        {
            highlight.Display(Unit.Range.Attack);
            highlight = null;
        }
    }
    static public bool Select(Unit unit)
    {
        if (team.alives.Contains(unit))
        {
            PlayerControl.unit = highlight;
            unit.ResetAction();
            unit.colliders.Enable(false);
            return true;
        }
        return false;
    }
    static public void Deselect()
    {
        unit.ResetAction();
        unit.colliders.Enable(true);
        unit = null;
    }
    static public void ReControl()
    {
        unit.ResetAction();
    }
    static public void MoveBack()
    {
        unit.model.localPosition = Vector3.zero;
        unit.Display(Unit.Range.Attack);
        unit.moveConsume = 0;
        unit.SetAp(Unit.maxAp - unit.turnConsume);
        unit.ClampTurningRange(Unit.maxTurning);
    }
    static public void MoveTo(Vector3 to)
    {
        if (unit.turnConsume > 0)
            unit.Display(Unit.Range.Moving | Unit.Range.Attack);
        else
            unit.Display(Unit.Range.All & ~Unit.Range.Arrow);
        float remain = Unit.maxAp - unit.turnConsume;
        Vector3 from = unit.position;
        Vector3 point = unit.GetMaxMovePoint(to - from, remain);
        float dist = Vector3.Distance(from, to);
        float maxDist = Vector3.Distance(from, point);
        if (dist > maxDist)
            unit.model.position = point;
        else
            unit.model.position = to;
        unit.moveConsume = dist / maxDist * remain;
        unit.SetAp(remain - unit.moveConsume);
        unit.ClampTurningRange(unit.ap * Unit.maxTurning);
    }
    static public void LookOrigin()
    {
        unit.Display(Unit.Range.Attack);
        unit.model.localEulerAngles = Vector3.zero;
        unit.turnConsume = 0;
        unit.SetAp(Unit.maxAp - unit.moveConsume);
    }
    static public void LookAt(Vector3 to)
    {
        if (unit.moveConsume > 0)
            unit.Display(Unit.Range.Attack | Unit.Range.Turning | Unit.Range.Arrow);
        else
            unit.Display(Unit.Range.Attack);
        float remain = Unit.maxAp - unit.moveConsume;
        if (remain > 0)
        {
            float maxAngle = remain * Unit.maxTurning;
            float angle = Vector.ForwardSignedAngle(unit.transform, to);
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            unit.model.localEulerAngles = new Vector3(0, angle, 0);
            unit.turnConsume = Mathf.Abs(angle / maxAngle) * remain;
            unit.SetAp(remain - unit.turnConsume);
        }
    }
    static public void Complete()
    {
        unit.colliders.Enable(true);
        unit.destination.position = unit.model.position;
        unit.destination.rotation = unit.model.rotation;
        unit.model.localPosition = Vector3.zero;
        unit.model.localEulerAngles = Vector3.zero;
        unit = null;
    }
    static public void Action()
    {
        foreach (Team team in Team.NonUser)
        {
            foreach (Unit unit in team.alives)
            {
                Unit target = team.enemies.OrderBy(x => Vector3.Distance(unit.transform.position, x.transform.position)).First();
                float distance = unit.Distance(target);
                if (unit.weapon.IsContain(distance))
                    unit.Guess(target);
                else if (unit.weapon.far < distance)
                    unit.Chase(target);
                else if (unit.weapon.near > distance)
                    unit.KeepAway(target);
                unit.StartAction();
            }
        }
        if (team == null)
            return;
        foreach (Unit unit in team.alives)
        {
            unit.StartAction();
        }
    }
}
