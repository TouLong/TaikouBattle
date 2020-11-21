﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public bool tesing;
    void Awake()
    {
        transform.GetComponent<Arena>().enabled = !tesing;
    }
    void Start()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        Team userTeam = new Team();
        Team npcTeam = new Team();
        int count = 0;
        foreach (Unit unit in units)
        {
            if (count < 1)
                userTeam.members.Add(new UnitInfo(userTeam, units[count]));
            else
                npcTeam.members.Add(new UnitInfo(npcTeam, units[count]));
            count++;
        }
        Unit.player = userTeam.members[0].unit;
        userTeam.Create();
        npcTeam.Create();
        CombatControl.self.Startup();
    }
}