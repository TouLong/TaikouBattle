using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class UserControl
{
    public static Team team;
    public static Unit unit;
    static Transform selected;
    static Dictionary<Unit, Transform> destUnits = new Dictionary<Unit, Transform>();
    static public void Setup()
    {
        team = null;
        if (Unit.player != null)
        {
            team = Unit.player.team;
            Team.NonUser.Remove(team);
            foreach (Unit unit in team.alives)
            {
                destUnits.Add(unit, Object.Instantiate(unit.model, unit.transform));
                destUnits[unit].gameObject.SetActive(false);
            }
        }
    }
    static public void Select(Unit unit)
    {
        UserControl.unit = unit;
        selected = destUnits[unit];
        unit.GetComponent<Collider>().enabled = false;
        selected.gameObject.SetActive(true);
        unit.status.AttackRange.SetParent(selected, false);
    }
    static public void Deselect()
    {
        selected.gameObject.SetActive(false);
        selected.localPosition = Vector3.zero;
        selected.localEulerAngles = Vector3.zero;
        unit.status.AttackRange.SetParent(unit.transform, false);
        unit.GetComponent<Collider>().enabled = true;
        selected = null;
    }
    static public void Complete()
    {
        unit.GetComponent<Collider>().enabled = true;
        selected = null;
    }
    static public void MoveTo(float x, float z)
    {
        Vector2 newPos =
            Vector2.ClampMagnitude(new Vector2(x, z) - Vector.XZ2XY(unit.transform.position), unit.moveDistance)
            + Vector.XZ2XY(unit.transform.position);
        selected.position = new Vector3(newPos.x, Map.GetHeight(newPos.x, newPos.y), newPos.y);
    }
    static public void LookAt(float x, float z)
    {
        Vector3 forward = new Vector3(x - selected.position.x, 0, z - selected.position.z);
        if (forward != Vector3.zero)
            selected.rotation = Quaternion.LookRotation(forward);
    }
    static public void Action()
    {
        foreach (Unit unit in team.alives)
        {
            Vector3 newPos = destUnits[unit].position;
            newPos.y = unit.transform.position.y;
            Quaternion newRot = destUnits[unit].rotation;
            unit.action = DOTween.Sequence()
                .Append(unit.LookAtTween(newPos))
                .Append(unit.MoveTween(newPos))
                .Append(unit.RotateTween(newRot));
            unit.status.AttackRange.SetParent(unit.transform, false);
            destUnits[unit].gameObject.SetActive(false);
            destUnits[unit].localPosition = Vector3.zero;
            destUnits[unit].localEulerAngles = Vector3.zero;
        }
    }
}
