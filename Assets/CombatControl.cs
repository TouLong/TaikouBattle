using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatControl : MonoBehaviour
{
    static public CombatControl self;
    static public Team team;
    static public Player selected;
    static public Unit highlight;
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
        if (Unit.player != null)
        {
            team = Unit.player.team;
            Team.NonUser.Remove(team);
        }
        else
        {
            team = null;
        }
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
            unit.Display(Unit.Range.Moving);
            highlight = unit;
            if (Mouse.LeftDown && team.alives.Contains(unit))
            {
                selected = unit as Player;
                selected.StartControl();
                stateUpdate = Control;
            }
        }
        else if (highlight)
        {
            highlight.Display(Unit.Range.Attack);
            highlight = null;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Team team in Team.NonUser)
            {
                foreach (Npc npc in team.alives)
                {
                    npc.Decision();
                }
            }
            foreach (Player player in team.alives)
            {
                player.ResetStatus();
            }
            foreach (Unit unit in Unit.Alive)
            {
                unit.StartAction();
            }
            stateUpdate = InAction;
        }
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            controlSeq.Reverse();
            control = controlSeq.First();
            selected.ResetStatus();
            return;
        }
        control();
        int controlId = controlSeq.IndexOf(control);
        bool end = false;
        if (Mouse.LeftDown)
        {
            end = ++controlId >= controlSeq.Count;
            if (end)
                selected.CompleteControl();
        }
        else if (Mouse.RightDown)
        {
            end = --controlId < 0;
            if (end)
                selected.CancelControl();
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
                selected.MoveBack();
            else
                selected.MoveTo(hit.point);
        }
    }
    void SetRotation()
    {
        if (Mouse.HitGround(out RaycastHit hit))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                selected.LookOrigin();
            else
                selected.LookAt(hit.point);
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
            Unit.Alive.RemoveAll(x => x.hp <= 0);
            Team.All.RemoveAll(x => x.alives.Count == 0);
            Team.NonUser.RemoveAll(x => x.alives.Count == 0);
            if (Team.All.Count == 1 && !testing)
            {
                stateUpdate = null;
                DelayEvent.Create(2f, () =>
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
