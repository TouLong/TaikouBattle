using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Arena : MonoBehaviour
{
    public class Schedule : List<Round>
    {
        int roundId = 0;
        int groupId = 0;
        public int userId = 0;
        public int winTeamId = -1;
        public List<UnitInfo> present = new List<UnitInfo>();
        public List<UnitInfo> promoted = new List<UnitInfo>();
        public Schedule() : base() { }
        public bool NextRound()
        {
            if (roundId + 1 < Count)
            {
                present.Clear();
                present.AddRange(promoted);
                promoted.Clear();
                roundId++;
                groupId = 0;
                CurrentRound.RandomAssign(present);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool NextGroup()
        {
            return ++groupId < this[roundId].Count;
        }
        public Round CurrentRound => this[roundId];
        public Group CurrentGroup => this[roundId][groupId];
        public Team CurrentWinTeam => winTeamId < 0 ? null : this[roundId][groupId][winTeamId];
    }
    public class Round : List<Group>
    {
        public Round() : base() { }
        public Round(IEnumerable<Group> group) : base(group) { }
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
    public class UnitInfo
    {
        public int id;
        public string name;
        public Sprite icon;
        public Team team;
        public Unit unit;
        public UnitInfo(int id, string name, Sprite icon)
        {
            this.id = id;
            this.name = name;
            this.icon = icon;
        }
    }
    static public int minGuy = 4;
    static public int maxGuy = 8;
    static public int maxGroup = 2;
    static public int maxTeam = 6;
    static public Arena self;
    static public Schedule schedule;

    public Unit unitPrefab;
    public Weapon[] weaponPrefab;
    public Sprite[] iconPrefab;
    public Transform teamSpawn;
    public Transform guySpawn;

    void Awake()
    {
        if (self == null)
            self = this;
    }
    public void Spawn()
    {
        Transform Take(Transform poseList, List<int> idList)
        {
            int id = ListRandom.In(idList);
            idList.Remove(id);
            return poseList.GetChild(id);
        }
        Unit.player = null;
        Group group = schedule.CurrentGroup;
        List<int> teamSpawnIDs = Enumerable.Range(0, teamSpawn.childCount).ToList();
        foreach (Team team in group)
        {
            Transform teamPose = Take(teamSpawn, teamSpawnIDs);
            List<int> guySpawnIDs = Enumerable.Range(0, guySpawn.childCount).ToList();
            foreach (UnitInfo unitInfo in team.members)
            {
                Transform pose = Take(guySpawn, guySpawnIDs);
                Unit unit = Instantiate(unitPrefab, pose.localPosition + teamPose.position, teamPose.rotation);
                if (unitInfo.id == 0)
                {
                    unit.weapon = weaponPrefab.Last();
                    Unit.player = unit;
                }
                else
                {
                    //unit.weapon = ListRandom.In(weaponPrefab);
                    unit.weapon = weaponPrefab[0];
                }
                unitInfo.unit = unit;
            }
            team.Create();
        }
        Timer.Set(0.2f, CombatControl.self.Startup);
    }
    static public void WinTeam(Team team)
    {
        Group group = schedule.CurrentGroup;
        schedule.promoted.AddRange(team.members);
        schedule.winTeamId = group.IndexOf(team);
    }
    static public void RandomJudge()
    {
        Group group = schedule.CurrentGroup;
        int win = Random.Range(0, group.Count);
        Team winOfTeam = group[win];
        schedule.promoted.AddRange(winOfTeam.members);
        schedule.winTeamId = win;
    }
    static public void NextContest()
    {
        if (schedule.NextGroup())
        {
        }
        else
        {
            if (schedule.NextRound())
            {
            }
            else
            {
            }
        }
    }
    static public Schedule RandomRule()
    {
        return RandomRule(Random.Range(minGuy, maxGuy + 1));
    }
    static public Schedule RandomRule(int guyCount)
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
        Round RandomGroups(int guys)
        {
            List<int> guysCF = CommonFactor(guys);
            int groupCount = 1;
            if (guys >= 4)
            {
                int minGroup = guysCF.Count > 1 && guys <= maxTeam ? 1 : 2;
                groupCount = Random.Range(minGroup, maxGroup + 1);
            }
            int groupRemain = guys;
            Round round = new Round();
            bool done = false;
            int repeatCount = 0;
            while (!done && repeatCount++ < 200)
            {
                int groupGuys = Random.Range(2, groupRemain + 1);
                List<int> groupGuysCF = CommonFactor(groupGuys);
                if (groupGuysCF.Any())
                {
                    int teamCount = ListRandom.In(groupGuysCF);
                    Group group = new Group();
                    for (int i = 0; i < teamCount; i++)
                    {
                        group.Add(new Team());
                    }
                    group.eachTeamMember = groupGuys / teamCount;
                    round.Add(group);
                    groupRemain -= groupGuys;
                }
                if (groupRemain == 1 || round.Count > groupCount)
                {
                    round.Clear();
                    groupRemain = guys;
                }
                done = round.Count <= groupCount && groupRemain == 0;
            }
            if (repeatCount > 200)
                print("fail");
            return round;
        }
        int remain = guyCount;
        Schedule newSchedule = new Schedule();
        while (remain != 1)
        {
            Round round = new Round(RandomGroups(remain));
            remain = round.Sum(x => x.eachTeamMember);
            newSchedule.Add(round);
        }
        newSchedule.present = new List<UnitInfo>();
        for (int i = 0; i < guyCount; i++)
        {
            UnitInfo info = new UnitInfo(i, string.Format("參賽者{0}號", i + 1), ListRandom.In(self.iconPrefab));
            newSchedule.present.Add(info);
        }
        newSchedule.CurrentRound.Assign(newSchedule.present);
        schedule = newSchedule;
        return newSchedule;
    }
}