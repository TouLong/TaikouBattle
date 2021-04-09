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
        if (CreateTeam(users, 0) != null)
            Unit.player = users[0];
        CreateTeam(npc1, 1);
        CreateTeam(npc2, 2);
        CreateTeam(npc3, 3);

        Team dummyTeam = CreateTeam(dummys, 4);
        if (dummyTeam != null)
        {
            Team.NonUser.Remove(dummyTeam);
            Team.Dummy.Add(dummyTeam);
        }
        CombatControl.self.testing = true;
        CombatControl.self.Setup();
    }
    Team CreateTeam(Unit[] units, int settingId)
    {
        if (units.Length > 0)
        {
            Team team = new Team();
            foreach (Unit unit in units)
            {
                UnitInfo info = new UnitInfo { name = unit.name, team = team };
                unit.info = info;
            }
            team.Setup(units.ToList());
            team.color = Setting.self.colors[settingId];
            team.material = Setting.self.materials[settingId];
            return team;
        }
        return null;
    }
}
