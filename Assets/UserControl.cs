using UnityEngine;

public class UserControl
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
            UserControl.unit = highlight;
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
        float remain = Unit.maxAp - unit.turnConsume;
        unit.movingRange.transform.localScale = new Vector3(remain, 1, remain);
        unit.model.localPosition = Vector3.zero;
        unit.Display(Unit.Range.All);
        unit.moveConsume = 0;
        unit.SetAp(Unit.maxAp - unit.turnConsume);
    }
    static public void MoveTo(Vector3 to)
    {
        float remain = Unit.maxAp - unit.turnConsume;
        if (remain > 0)
        {
            unit.movingRange.transform.localScale = new Vector3(remain, 1, remain);
            unit.movingRange.transform.rotation = unit.model.transform.rotation;
            unit.Display(Unit.Range.All);
            Vector3 from = unit.transform.position;
            Physics.Raycast(from, to - from, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("MovingRange"));
            float dist = Vector3.Distance(from, to);
            float maxDist = hit.distance;
            if (dist > maxDist)
                unit.model.position = hit.point;
            else
                unit.model.position = to;
            unit.moveConsume = dist / maxDist * remain;
            unit.SetAp(remain - unit.moveConsume);
        }
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
        unit.Display(Unit.Range.Attack);
        float remain = Unit.maxAp - unit.moveConsume;
        if (remain > 0)
        {
            float maxAngle = remain * Unit.maxTurning * 3f;
            Vector3 dir = Vector.Xz(to - unit.transform.position).normalized;
            Vector3 from = unit.transform.forward;
            float angle = Vector3.SignedAngle(from, dir, Vector3.up);
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
        if (team == null)
            return;
        foreach (Unit unit in team.alives)
        {
            unit.StartAction();
            unit.model.localPosition = Vector3.zero;
            unit.model.localEulerAngles = Vector3.zero;
        }
    }
}
