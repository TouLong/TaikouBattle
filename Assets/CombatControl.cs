using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CombatControl : MonoBehaviour
{
    static public CombatControl self;
    Action stateUpdate;
    void Awake()
    {
        if (self == null)
            self = this;
    }
    public void Startup()
    {
        Team.InScene.ForEach(x => x.Update());
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
            if (Mouse.LeftDown && UserControl.team.alives.Contains(unit))
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
            Unit.Alive.ForEach(x => x.status.Display(RangeDisplayType.Attack));
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
            else if (Mouse.RightDown)
            {
                UserControl.Deselect();
                stateUpdate = Selecting;
            }
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            UserControl.LookAt(hit.point.x, hit.point.z);
            if (Mouse.LeftDown)
            {
                stateUpdate = Selecting;
                UserControl.Complete();
            }
            else if (Mouse.RightDown)
            {
                stateUpdate = SetPosition;
            }
        }
    }
    void InAction()
    {
        if (!Unit.Alive.FindAll(x => x.action.IsPlaying()).Any())
        {
            Unit.Alive.ForEach(x => x.Combat());
            stateUpdate = InCombat;
        }
    }
    void InCombat()
    {
        if (!Unit.Alive.FindAll(x => x.inCombat).Any())
        {
            Team.InScene.RemoveAll(x => x.alives.Count == 0);
            Team.NonUser.RemoveAll(x => x.alives.Count == 0);
            if (Team.InScene.Count == 1)
            {
                stateUpdate = null;
                Timer.Set(2f, () =>
                {
                    Arena.WinTeam(Team.InScene[0]);
                    Team.InScene.Clear();
                    Team.NonUser.Clear();
                    ArenaMenu.self.UpdateWinTeam();
                    Arena.NextContest();
                    ArenaMenu.self.UpdaheRound();
                    ArenaMenu.self.gameObject.SetActive(true);
                    while (Unit.All.Count != 0)
                    {
                        Destroy(Unit.All[0].gameObject);
                        Unit.All.RemoveAt(0);
                    }
                    Unit.Alive.Clear();
                });
            }
            else
            {
                Team.InScene.ForEach(x => x.Update());
                stateUpdate = Selecting;
            }
        }
    }
}
