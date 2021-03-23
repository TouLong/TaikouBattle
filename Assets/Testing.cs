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
            if (unit.gameObject.activeSelf)
                userTeam.members.Add(new UnitInfo(userTeam, unit));
        }
        foreach (Unit unit in npcs)
        {
            if (unit.gameObject.activeSelf)
                npcTeam.members.Add(new UnitInfo(npcTeam, unit));
        }
        foreach (Unit unit in dummys)
        {
            if (unit.gameObject.activeSelf)
            {
                UnitInfo unitInfo = new UnitInfo(dummyTeam, unit);
                unitInfo.name = "Dummy";
                dummyTeam.members.Add(unitInfo);
                unit.SetInfo(unitInfo);
            }
        }
        Unit.player = userTeam.members[0].unit;
        userTeam.Setup();
        npcTeam.Setup();
        dummyTeam.Setup();
        Team.NonUser.Remove(dummyTeam);
        Team.Dummy.Add(dummyTeam);
        CombatControl.self.testing = true;
        CombatControl.self.Setup();
    }
}
