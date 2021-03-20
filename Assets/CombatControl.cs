using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CombatControl : MonoBehaviour
{
    static public CombatControl self;
    [HideInInspector]
    public bool testing;
    Action stateUpdate;
    List<Action> controlSeq = new List<Action>();
    Action control;
    void Awake()
    {
        if (self == null)
            self = this;
    }
    public void Setup()
    {
        Team.All.ForEach(x => x.Update());
        UserControl.Setup();
        if (UserControl.team != null)
            Camera.main.transform.LookAt(UserControl.team.center);
        else
            Camera.main.transform.LookAt(Team.All[0].center);
        Camera.main.GetComponent<TrackballCamera>().Start();
        controlSeq = new List<Action> { SetPosition, SetRotation };
        control = controlSeq.First();
        stateUpdate = Selecting;
    }
    void Update()
    {
        stateUpdate?.Invoke();
    }
    void Selecting()
    {
        if (Mouse.Hit(out RaycastHit hit, LayerMask.GetMask("Unit")))
        {
            Unit unit = hit.transform.parent.parent.GetComponent<Unit>();
            UserControl.Highlight(unit);
            if (Mouse.LeftDown && UserControl.Select(unit))
                stateUpdate = Control;
        }
        else
        {
            UserControl.Highlight();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UserControl.Action();
            Team.NonUser.ForEach(x => x.PlanA());
            Team.Dummy.ForEach(x => x.NotPlan());
            stateUpdate = InAction;
        }
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            controlSeq.Reverse();
            control = controlSeq.First();
            UserControl.ReControl();
            return;
        }
        control();
        int controlId = controlSeq.IndexOf(control);
        bool end = false;
        if (Mouse.LeftDown)
        {
            end = ++controlId >= controlSeq.Count;
            if (end)
                UserControl.Complete();
        }
        else if (Mouse.RightDown)
        {
            end = --controlId < 0;
            if (end)
                UserControl.Deselect();
        }
        if (end)
        {
            control = controlSeq.First();
            stateUpdate = Selecting;
        }
        else
            control = controlSeq[controlId];
    }
    void SetPosition()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                UserControl.MoveBack();
            else
                UserControl.MoveTo(hit.point);
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                UserControl.LookOrigin();
            else
                UserControl.LookAt(hit.point);
        }
    }
    void InAction()
    {
        if (!Unit.Alive.FindAll(x => x.inAction).Any())
        {
            Unit.Alive.ForEach(x => x.StartCombat());
            stateUpdate = InCombat;
        }
    }
    void InCombat()
    {
        if (!Unit.Alive.FindAll(x => x.inCombat).Any())
        {
            Unit.Alive.ForEach(x => x.EndCombat());
            Team.All.RemoveAll(x => x.alives.Count == 0);
            Team.NonUser.RemoveAll(x => x.alives.Count == 0);
            if (Team.All.Count == 1 && !testing)
            {
                stateUpdate = null;
                Timer.Set(2f, () =>
                {
                    Arena.ContestComplete(Team.All.First());
                    Team.All.Clear();
                    Team.NonUser.Clear();
                    Unit.All.Clear();
                    Unit.Alive.Clear();
                });
            }
            else
            {
                Team.All.ForEach(x => x.Update());
                stateUpdate = Selecting;
            }
        }
    }
}
