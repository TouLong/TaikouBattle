using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public Unit[] users;
    public Unit[] npcs;
    public Unit[] dummys;
    void Start()
    {
        Team userTeam = new Team();
        Team npcTeam = new Team();
        Team dummyTeam = new Team();
        foreach (Unit unit in users)
        {
            userTeam.members.Add(new UnitInfo(userTeam, unit));
            unit.GetComponent<UnitStatus>().Setup(unit);
        }
        foreach (Unit unit in npcs)
        {
            npcTeam.members.Add(new UnitInfo(npcTeam, unit));
            unit.GetComponent<UnitStatus>().Setup(unit);
        }
        foreach (Unit unit in dummys)
        {
            UnitInfo unitInfo = new UnitInfo(dummyTeam, unit);
            unitInfo.name = "Dummy";
            dummyTeam.members.Add(unitInfo);
            unit.GetComponent<UnitStatus>().Setup(unit);
            unit.GetComponent<UnitStatus>().Setup(unitInfo);
        }
        Unit.player = userTeam.members[0].unit;
        userTeam.Setup();
        npcTeam.Setup();
        dummyTeam.Setup();
        Team.NonUser.Remove(dummyTeam);
        Team.Dummy.Add(dummyTeam);
        CombatControl.self.testing = true;
        CombatControl.self.Startup();
    }
}
