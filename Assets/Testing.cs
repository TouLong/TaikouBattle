using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public Unit[] users;
    public Unit[] npc1;
    public Unit[] npc2;
    public Unit[] npc3;
    public Unit[] dummys;
    void Start()
    {
        if (CreateTeam(users) != null)
            Unit.player = users[0];
        CreateTeam(npc1);
        CreateTeam(npc2);
        CreateTeam(npc3);
        Team dummyTeam = CreateTeam(dummys);
        if (dummyTeam != null)
        {
            Team.NonUser.Remove(dummyTeam);
            Team.Dummy.Add(dummyTeam);
        }
        CombatControl.self.testing = true;
        CombatControl.self.Setup();
    }
    Team CreateTeam(Unit[] units)
    {
        if (units.Length > 0)
        {
            Team team = new Team();
            foreach (Unit unit in units)
            {
                UnitInfo info = new UnitInfo(team, unit);
                team.members.Add(info);
                unit.info = info;
            }
            team.Setup();
            return team;
        }
        return null;
    }
}
