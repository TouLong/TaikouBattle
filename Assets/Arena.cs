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
        public int GetWinnerCount()
        {
            int count = 0;
            foreach (Teams teams in this)
            {
                count += teams[0].members;
            }
            return count;
        }
    }
    public class Teams : List<Team>
    {
        public Teams() : base() { }
        public Teams(IEnumerable<Team> teams) : base(teams) { }
    }
    public class Team
    {
        public int members;
        public Team(int members)
        {
            this.members = members;
        }
    }
    public const int minGuy = 8;
    public const int maxGuy = 24;
    public const int maxGroup = 3;
    public const int maxTeam = 6;
    [Range(minGuy, maxGuy)]
    public int guyCount;
    public List<Groups> rule;
    static public void RandomGenerate()
    {
        int guyCount = Random.Range(minGuy, maxGuy + 1);
        GameObject.Find("Game").GetComponent<Arena>().guyCount = guyCount;
        Generate(guyCount);
    }
    static public Rounds Generate(int guyCount)
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
        Groups RandomGroups(int guys)
        {
            List<int> guysCF = CommonFactor(guys);
            int groupCount = 1;
            if (guys >= 4)
            {
                int minGroup = guysCF.Count > 1 && guys <= maxTeam ? 1 : 2;
                groupCount = Random.Range(minGroup, maxGroup + 1);
            }
            int groupRemain = guys;
            Groups groups = new Groups();
            bool done = false;
            int repeatCount = 0;
            while (!done && repeatCount++ < 200)
            {
                int groupGuys = Random.Range(2, groupRemain + 1);
                List<int> groupGuysCF = CommonFactor(groupGuys);
                if (groupGuysCF.Any())
                {
                    int teamCount = ListRandom.In(groupGuysCF);
                    groups.Add(new Teams(Enumerable.Repeat(new Team(groupGuys / teamCount), teamCount)));
                    groupRemain -= groupGuys;
                }
                if (groupRemain == 1 || groups.Count > groupCount)
                {
                    groups.Clear();
                    groupRemain = guys;
                }
                done = groups.Count <= groupCount && groupRemain == 0;
            }
            if (repeatCount > 200)
                print("fail");
            return groups;
        }
        int remain = guyCount;
        Rounds rounds = new Rounds();
        while (remain != 1)
        {
            Groups groups = new Groups(RandomGroups(remain));
            remain = groups.GetWinnerCount();
            rounds.Add(groups);
        }
        GameObject.Find("UI").GetComponent<ArenaMenu>().UpdateUI(rounds);
        return rounds;
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(Arena))]
public class ArenaRuleEditor : Editor
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
#endif