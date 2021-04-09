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
        TextUI.defualtFontSize = 8;
        TextUI.defualtPopTime = 0.3f;
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
        if (!testing)
        {
            Camera.main.transform.position = lookTeam.center + (Vector3.up - lookTeam.lookat) * 5;
            Camera.main.transform.LookAt(lookTeam.center);
            Camera.main.GetComponent<TrackballCamera>().Start();
        }
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
        bool showInfo = Input.GetKey(KeyCode.LeftAlt);
        bool action = Input.GetKeyDown(KeyCode.Space);
        bool selectPlayer = Mouse.LeftDown && onPlayer;
        foreach (Unit unit in Unit.All)
        {
            if (action)
                unit.Display(Unit.Highlight.Nothing);
            else if (unit == highlight)
            {
                Unit.Highlight state = Unit.Highlight.Attack | Unit.Highlight.Outline | Unit.Highlight.Info;
                if (onPlayer)
                    state |= Unit.Highlight.Moving;
                unit.Display(state);
            }
            else
            {
                Unit.Highlight state = Unit.Highlight.Nothing;
                if (showInfo)
                    state |= Unit.Highlight.Info | Unit.Highlight.Attack;
                unit.Display(state);
            }
        }
        Hint.self.space.SetActive(!selectPlayer && !action);
        Hint.self.alt.SetActive(!showInfo && !action);
        Hint.self.control.SetActive(onPlayer && !selectPlayer && !action);
        Hint.self.shift.SetActive(false);
        Hint.self.ctrl.SetActive(selectPlayer && !action);
        Hint.self.right.SetActive(selectPlayer && !action);
        Hint.self.confirm.SetActive(selectPlayer && !action);
        if (selectPlayer)
        {
            selected = highlight as Player;
            selected.ControlStart();
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
                Unit.Alive.RemoveAll(x => x.isDie);
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
        bool showInfo = Input.GetKey(KeyCode.LeftAlt);
        foreach (Unit unit in Unit.All)
        {
            if (unit != selected)
            {
                if (showInfo)
                    unit.Display(Unit.Highlight.Info | Unit.Highlight.Attack);
                else
                    unit.Display(Unit.Highlight.Nothing);
            }
        }
        Hint.self.shift.SetActive(!Input.GetKey(KeyCode.LeftShift));
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