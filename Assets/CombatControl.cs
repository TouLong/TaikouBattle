using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CombatControl : MonoBehaviour
{
    public Unit playerUnit;
    Action stateUpdate;
    void Awake()
    {
        if (playerUnit != null)
        {
            PlayerControl.Setup(playerUnit);
        }
    }
    void Start()
    {
        if (playerUnit != null)
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
            unit.status.Display(RangeDisplayType.Moving);
            if (Mouse.LeftDown && unit.gameObject.layer == 8)//Player
            {
                PlayerControl.Start();
                stateUpdate = SetPosition;
            }
        }
        else
        {
            Unit.InScene.ForEach(x => x.status.Display(RangeDisplayType.Attack));
        }
    }
    void SetPosition()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            PlayerControl.MoveTo(hit.point.x, hit.point.z);
            if (Mouse.LeftDown)
            {
                stateUpdate = SetRotation;
                PlayerControl.unit.status.Display(RangeDisplayType.Attack);
            }
        }
        if (Mouse.RightDown)
        {
            PlayerControl.unit.status.Display(RangeDisplayType.Attack);
            PlayerControl.Reset();
            stateUpdate = Selecting;
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            PlayerControl.LookAt(hit.point.x, hit.point.z);
            if (Mouse.RightDown)
            {
                PlayerControl.Reset();
                stateUpdate = SetPosition;
            }
            else if (Mouse.LeftDown)
            {
                Unit.InScene.ForEach(x => x.status.Display(RangeDisplayType.Attack));
                PlayerControl.Action();
                Enemy.InSceneAction();
                PlayerControl.Reset();
                stateUpdate = Action;
            }
        }
    }
    void Action()
    {
        if (!Unit.InScene.All(x => x.action.IsPlaying()))
        {
            stateUpdate = Selecting;
        }
    }
}
