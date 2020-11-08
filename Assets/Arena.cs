using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.VersionControl;

public class Arena : MonoBehaviour
{
    public class Rounds : List<Groups>
    {
        public Rounds() : base() { }
        public Rounds(List<Groups> round) : base(round) { }
    }
    public class Groups : List<Teams>
    {
        public Groups() : base() { }
        public Groups(IEnumerable<Teams> group) : base(group) { }
    }
    public class Teams : List<Team>
    {
        public Teams() : base() { }
        public Teams(IEnumerable<Team> teams) : base(teams) { }
    }
    public class Team
    {
        public int members;
    }
    public const int maxGuy = 16;
    public const int maxGroup = 2;
    public const int maxRound = 4;
    public const int maxTeam = 6;
    [Range(4, maxGuy)]
    public int guyCount;
    public List<Groups> rule;
    static public void RandomGenerate()
    {
        int guyCount = Random.Range(4, maxGuy + 1);
        GameObject.Find("Game").GetComponent<Arena>().guyCount = guyCount;
        Generate(guyCount);
    }
    static public Rounds Generate(int guyCount)
    {
        int remain = guyCount;
        int CommonFactorCount(int number)
        {
            int count = 0;
            for (int i = 2; i <= number; i++)
            {
                if (number % i == 0)
                    count++;
            }
            return count;
        }
        Groups RandomGroups(int guys)
        {
            print(guys);
            int guysCFCount = CommonFactorCount(guys);
            print(guysCFCount);
            List<int> vaildGroupCount = new List<int>();
            for (int i = 1; i <= maxGroup; i++)
            {
                if (guysCFCount >= i)
                    vaildGroupCount.Add(i);
            }
            int groupCount = ListRandom.In(vaildGroupCount);
            print(groupCount);
            int groupRemain = guys;
            Groups groups = new Groups();
            remain = 0;
            for (int i = 1; i <= groupCount; i++)
            {
                if (groupRemain == 2 || i == groupCount)
                {
                    groups.Add(new Teams(RandomTeams(groupRemain)));
                    break;
                }
                else
                {
                    int groupGuys = guys / groupCount;
                    groupRemain -= groupGuys;
                    print(groupGuys);
                    print(groupRemain);
                    groups.Add(new Teams(RandomTeams(groupGuys)));
                }
                if (groupRemain == 0)
                    break;
            }
            return groups;
        }
        Teams RandomTeams(int guys)
        {
            List<int> vaildTeamCount = new List<int>();
            int limit = Mathf.Min(guys, maxTeam);
            for (int i = 2; i <= limit; i++)
            {
                if (guys % i == 0)
                    vaildTeamCount.Add(i);
            }
            int teamCount = ListRandom.In(vaildTeamCount);
            int memberCount = guys / teamCount;
            remain += memberCount;
            print(string.Format("{0},{1}", teamCount, memberCount));
            return new Teams(Enumerable.Repeat(new Team() { members = memberCount }, teamCount));
        }
        Rounds rounds = new Rounds();
        for (int i = 1; i <= maxRound; i++)
        {
            if (remain == 1)
                break;
            Groups groups;
            if (i == maxRound)
            {
                groups = new Groups
                {
                    new Teams(Enumerable.Repeat(new Team() { members = 1 }, remain))
                };
            }
            else
            {
                groups = new Groups(RandomGroups(remain));
            }
            rounds.Add(groups);
        }
        GameObject.Find("ArenaUI").GetComponent<RuleUI>().UpdateUI(rounds);
        return rounds;
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(Arena))]
public class LookAtPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Arena arena = target as Arena;
        if (GUILayout.Button("Ramdom"))
        {
            Arena.RandomGenerate();
        }
        if (GUILayout.Button("Custom"))
        {
            Arena.Generate(arena.guyCount);
        }
    }
}
