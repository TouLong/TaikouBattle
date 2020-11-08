using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;

public class RuleUI : MonoBehaviour
{
    public struct Prefab
    {
        public Transform round;
        public Transform group;
        public Transform team;
        public Transform member;
    }
    public struct Framework
    {
        public Transform main;
        public Transform rule;
        public Transform menu;
    }
    Prefab prefab;
    Framework framework;

    public void PrefabSetup()
    {
        framework.main = transform.Find("Main");
        framework.rule = framework.main.Find("Rule");
        framework.menu = transform.Find("Menu");
        prefab.round = framework.rule.Find("Round");
        prefab.group = prefab.round.Find("Group");
        prefab.team = prefab.group.Find("Team");
        prefab.member = prefab.team.Find("Member");
    }
    public void UpdateUI(Arena.Rounds rounds)
    {
        PrefabSetup();
        while (framework.rule.childCount != 1)
        {
            DestroyImmediate(framework.rule.GetChild(1).gameObject);
        }
        List<Color> colors = new List<Color>()
        {
            new Color(1f, 0f,0.5f),
            new Color(0f, 0.5f,1f),
            new Color(0.5f, 1f,0f),
            new Color(1f, 0.5f,0f),
            new Color(0.5f, 0f,1f),
            new Color(1f, 1f,0.5f),
        };

        rounds.Reverse();
        foreach (Arena.Groups arenaGroups in rounds)
        {
            Transform round = InstantiateWithoutChild(prefab.round, framework.rule);
            int groupCount = 0;
            foreach (Arena.Teams arenaTeams in arenaGroups)
            {
                Transform group = InstantiateWithoutChild(prefab.group, round);
                group.GetComponent<Image>().color = colors[groupCount++];
                int teamCount = 0;
                foreach (Arena.Team arenaTeam in arenaTeams)
                {
                    Transform team = InstantiateWithoutChild(prefab.team, group);
                    team.GetComponent<Image>().color = colors[teamCount++];
                    for (int i = 0; i < arenaTeam.members; i++)
                    {
                        Transform member = Instantiate(prefab.member, team);
                    }
                }
            }
        }
    }
    public Transform InstantiateWithoutChild(Transform original, Transform parent)
    {
        Transform clone = Instantiate(original, parent);
        clone.gameObject.SetActive(true);
        while (clone.childCount != 0)
        {
            DestroyImmediate(clone.GetChild(0).gameObject);
        }
        return clone;
    }
}

