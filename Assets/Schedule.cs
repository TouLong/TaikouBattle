using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Schedule : List<Round>
{
    int roundId = 0;
    int groupId = 0;
    public List<UnitInfo> units = new List<UnitInfo>();
    public Schedule(int unitCount, int maxTeam, int maxGroup) : base()
    {
        int remain = unitCount;
        while (remain != 1)
        {
            Round round = new Round(remain, maxTeam, maxGroup);
            remain = round.Sum(x => x.eachTeamMember);
            Add(round);
        }
    }
    public bool Next(Team winTeam)
    {
        if (groupId == 0)
            units.Clear();
        units.AddRange(winTeam.members);
        if (++groupId >= CurrentRound.Count)
        {
            if (++roundId < Count)
            {
                CurrentRound.RandomAssign(units);
                groupId = 0;
            }
            return true;
        }
        return false;
    }
    public bool IsEndGame => roundId >= Count;
    public bool IsEndRound => groupId == 0;
    public Round CurrentRound => this[roundId];
    public Group CurrentGroup => this[roundId][groupId];
}

public class Round : List<Group>
{
    public Round(int unitCount, int maxTeam, int maxGroup) : base()
    {
        List<int> CommonFactor(int number)
        {
            List<int> cf = new List<int>();
            for (int i = 2; i <= maxTeam; i++)
            {
                if (number % i == 0)
                    cf.Add(i);
            }
            return cf;
        }
        List<int> unitsCF = CommonFactor(unitCount);
        int groupCount = 1;
        if (unitCount >= 4)
        {
            int minGroup = unitsCF.Count > 1 && unitCount <= maxTeam ? 1 : 2;
            groupCount = Random.Range(minGroup, maxGroup + 1);
        }
        int groupRemain = unitCount;
        bool done = false;
        int repeatCount = 0;
        while (!done && repeatCount++ < 200)
        {
            int groupUnits = Random.Range(2, groupRemain + 1);
            List<int> groupUnitsCF = CommonFactor(groupUnits);
            if (groupUnitsCF.Any())
            {
                int teamCount = ListRandom.In(groupUnitsCF);
                Group group = new Group();
                for (int i = 0; i < teamCount; i++)
                {
                    group.Add(new Team());
                }
                group.eachTeamMember = groupUnits / teamCount;
                Add(group);
                groupRemain -= groupUnits;
            }
            if (groupRemain == 1 || Count > groupCount)
            {
                Clear();
                groupRemain = unitCount;
            }
            done = Count <= groupCount && groupRemain == 0;
        }
        if (repeatCount > 200)
            Debug.Log("fail");
    }
    public void RandomAssign(List<UnitInfo> units)
    {
        List<int> Ids = Enumerable.Range(0, units.Count).ToList();
        foreach (Group group in this)
        {
            foreach (Team team in group)
            {
                for (int i = 0; i < group.eachTeamMember; i++)
                {
                    int id = ListRandom.In(Ids);
                    Ids.Remove(id);
                    UnitInfo unit = units[id];
                    unit.group = group;
                    unit.team = team;
                    team.members.Add(unit);
                }
            }
        }
    }
    public void Assign(List<UnitInfo> units)
    {
        int count = 0;
        foreach (Group group in this)
        {
            foreach (Team team in group)
            {
                for (int i = 0; i < group.eachTeamMember; i++)
                {
                    UnitInfo unit = units[count++];
                    unit.group = group;
                    unit.team = team;
                    team.members.Add(unit);
                }
            }
        }
    }
}
public class Group : List<Team>
{
    public int eachTeamMember;
    public Group() : base() { }
}
public class Team
{
    public static List<Team> All = new List<Team>();
    public static List<Team> NonUser = new List<Team>();
    public static List<Team> Dummy = new List<Team>();
    public List<UnitInfo> members = new List<UnitInfo>();
    public Color color = Color.black;
    public List<Unit> alives = new List<Unit>();
    public List<Unit> enemies = new List<Unit>();
    public Vector3 center;
    public void Setup()
    {
        All.Add(this);
        NonUser.Add(this);
        alives.AddRange(members.Select(x => x.unit));
    }
    public void Update()
    {
        center = Vector3.zero;
        enemies.Clear();
        enemies.AddRange(Unit.Alive);
        foreach (Unit unit in alives)
        {
            unit.team = this;
            enemies.Remove(unit);
            center += unit.transform.position / alives.Count;
        }
    }
}
public class UnitInfo
{
    public int id;
    public string name;
    public Sprite icon;
    public Group group;
    public Team team;
    public Unit unit;
    public UnitInfo(Team team, Unit unit)
    {
        this.team = team;
        this.unit = unit;
    }
    public UnitInfo(int id, string name, Sprite icon)
    {
        this.id = id;
        this.name = name;
        this.icon = icon;
    }
}