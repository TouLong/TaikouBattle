using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class CombatControl : MonoBehaviour
{
    static public CombatControl self;
    static public Team playerTeam;
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
        Team lookTeam;
        Team.All.Update();
        if (Unit.player != null)
        {
            playerTeam = Unit.player.team;
            Team.NonUser.Remove(playerTeam);
            lookTeam = playerTeam;
        }
        else
        {
            playerTeam = null;
            lookTeam = Team.All.First();
        }
        Camera.main.transform.position = lookTeam.center + (Vector3.up - lookTeam.lookat) * 5;
        Camera.main.transform.LookAt(lookTeam.center);
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
        highlight = null;
        bool onPlayer = false;
        bool hasPlayer = Unit.player != null;
        if (Mouse.Hit(out RaycastHit hit, LayerMask.GetMask("Unit")))
        {
            highlight = hit.transform.GetComponent<Unit>();
            if (hasPlayer)
                onPlayer = playerTeam.memebers.Contains(highlight);
        }
        bool showAttack = Input.GetKey(KeyCode.LeftAlt);
        bool action = Input.GetKeyDown(KeyCode.Space);
        bool selectPlayer = Mouse.LeftDown && onPlayer;
        foreach (Team team in Team.NonUser)
        {
            foreach (Unit unit in team.memebers)
            {
                if (action)
                    unit.Display(Unit.Highlight.Nothing);
                else if (showAttack)
                    unit.Display(Unit.Highlight.Info | Unit.Highlight.Attack);
                else if (unit == highlight)
                    unit.Display(Unit.Highlight.Attack | Unit.Highlight.Outline | Unit.Highlight.Info);
                else
                    unit.Display(Unit.Highlight.Nothing);
            }
        }
        if (hasPlayer)
        {
            foreach (Unit unit in playerTeam.memebers)
            {
                if (action)
                    unit.Display(Unit.Highlight.Nothing);
                else if (showAttack)
                    unit.Display(Unit.Highlight.Info | Unit.Highlight.Attack);
                else if (unit == highlight)
                    unit.Display(Unit.Highlight.Attack | Unit.Highlight.Info | Unit.Highlight.Outline | Unit.Highlight.Moving);
                else
                    unit.Display(Unit.Highlight.Info);
            }
        }
        if (selectPlayer)
        {
            selected = highlight as Player;
            selected.StatusReset();
            stateUpdate = Control;
        }
        else if (action)
        {
            stateUpdate = null;
            Team.NonUser.NpcDecision();
            if (hasPlayer)
                playerTeam.memebers.ForEach(x => (x as Player).StatusReset());
            Unit.Action(() =>
            {
                Unit.Alive.RemoveAll(x => x.hp <= 0);
                Team.All.RemoveAll(x => x.memebers.Count == 0);
                Team.NonUser.RemoveAll(x => x.memebers.Count == 0);
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
            });
        }
    }
    void Control()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            controlSeq.Reverse();
            control = controlSeq.First();
            selected.StatusReset();
            return;
        }
        control();
        int controlId = controlSeq.IndexOf(control);
        bool end = false;
        if (Mouse.LeftDown)
        {
            end = ++controlId >= controlSeq.Count;
            if (end)
                selected.ControlComplete();
        }
        else if (Mouse.RightDown)
        {
            end = --controlId < 0;
            if (end)
                selected.StatusReset();
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
}