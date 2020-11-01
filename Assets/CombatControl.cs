using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CombatControl : MonoBehaviour
{
    Action stateUpdate;
    void Start()
    {
        UserControl.Setup();
        if (UserControl.team != null)
        {
            stateUpdate = Selecting;
        }
    }
    void Update()
    {
        stateUpdate?.Invoke();
    }
    void Selecting()
    {
        if (Mouse.Hit(out Unit unit))
        {
            if (Mouse.LeftDown && UserControl.team.members.Contains(unit))
            {
                UserControl.Select(unit);
                stateUpdate = SetPosition;
            }
            else
            {
                unit.status.Display(RangeDisplayType.Moving);
            }
        }
        else
        {
            Unit.InScene.ForEach(x => x.status.Display(RangeDisplayType.Attack));
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UserControl.Action();
            Team.NonUser.ForEach(x => x.Action());
            stateUpdate = InAction;
        }
    }
    void SetPosition()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            UserControl.MoveTo(hit.point.x, hit.point.z);
            if (Mouse.LeftDown)
            {
                UserControl.unit.status.Display(RangeDisplayType.Attack);
                stateUpdate = SetRotation;
            }
        }
        if (Mouse.RightDown)
        {
            UserControl.Deselect();
            stateUpdate = Selecting;
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            UserControl.LookAt(hit.point.x, hit.point.z);
            if (Mouse.RightDown)
            {
                stateUpdate = SetPosition;
            }
            else if (Mouse.LeftDown)
            {
                stateUpdate = Selecting;
                UserControl.Complete();
            }
        }
    }
    void InAction()
    {
        if (!Unit.InScene.FindAll(x => x.action.IsPlaying()).Any())
        {
            Unit.InScene.ForEach(x => x.Combat());
            stateUpdate = InCombat;
        }
    }
    void InCombat()
    {
        if (!Unit.InScene.FindAll(x => x.inCombat).Any())
        {
            Team.All.ForEach(x => x.UpdateTeam());
            stateUpdate = Selecting;
        }
    }
}
