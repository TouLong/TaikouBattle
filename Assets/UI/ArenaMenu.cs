using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System;

public class ArenaMenu : MonoBehaviour
{
    public string[] roundTexts = new string[] { "I", "II", "III", "IV", };
    public string[] groupTexts = new string[] { "I", "II", "III", "IV", };
    public Color[] groupColors = new Color[]
    {
    };
    public Color[] teamColors = new Color[]
    {
        new Color(1f, 0f,0.5f),
        new Color(0f, 0.5f,1f),
        new Color(1f, 1f,0.5f),
        new Color(0.5f, 1f,0f),
        new Color(0.5f, 0f,1f),
        new Color(1f, 0.5f,0f),
    };
    public Sprite[] icons = new Sprite[] { };
    struct Framework
    {
        public Transform main;
        public Transform rule;
        public Transform menu;
    }
    struct Prefab
    {
        public Transform round;
        public Transform roundTitle;
        public Transform roundContent;
        public Transform group;
        public Transform groupTitle;
        public Transform groupContent;
        public Transform team;
        public Transform member;
    }
    Prefab prefab;
    Framework framework;

    public void GetPrefab()
    {
        framework.main = transform.Find("ArenaMenu/Main");
        framework.menu = transform.Find("ArenaMenu/Menu");
        framework.rule = framework.main.GetChild(0);
        prefab.round = framework.rule.GetChild(0);
        prefab.roundTitle = prefab.round.GetChild(0);
        prefab.roundContent = prefab.round.GetChild(1);
        prefab.group = prefab.roundContent.GetChild(0);
        prefab.groupTitle = prefab.group.GetChild(0);
        prefab.groupContent = prefab.group.GetChild(1);
        prefab.team = prefab.groupContent.GetChild(0);
        prefab.member = prefab.team.GetChild(0);
    }
    public void ResetPrefab()
    {
        void RemoveChild(Transform root)
        {
            while (root.childCount != 1)
                DestroyImmediate(root.GetChild(1).gameObject);
        }
        GetPrefab();
        RemoveChild(framework.rule);
        prefab.round.gameObject.SetActive(false);
    }
    public void UpdateUI(Arena.Rounds rounds)
    {
        ResetPrefab();
        rounds.Reverse();
        int roundCount = rounds.Count;
        int memberCount = 0;
        foreach (Arena.Groups arenaGroups in rounds)
        {
            Transform round = InstantiateWithoutChild(prefab.round, framework.rule);
            Transform roundTitle = InstantiateWithoutChild(prefab.roundTitle, round);
            roundTitle.GetComponent<Text>().text = string.Format("回合{0}", roundTexts[--roundCount]);
            Transform roundContent = InstantiateWithoutChild(prefab.roundContent, round);
            int groupCount = 0;
            int maxTeamCount = arenaGroups.Max(x => x.Count);
            int maxTeamMemberCount = arenaGroups.Max(x => x.Max(y => y.members));
            foreach (Arena.Teams arenaTeams in arenaGroups)
            {
                Transform group = InstantiateWithoutChild(prefab.group, roundContent);
                Transform groupTitle = InstantiateWithoutChild(prefab.groupTitle, group);
                groupTitle.GetComponent<Text>().text = string.Format("場次{0}", groupTexts[groupCount++]);
                Transform groupContent = InstantiateWithoutChild(prefab.groupContent, group);
                VerticalLayoutGroup groupLayout = group.GetComponent<VerticalLayoutGroup>();
                GridLayoutGroup groupContentLayout = groupContent.GetComponent<GridLayoutGroup>();
                int maxMember = arenaTeams.Max(x => x.members);
                if (maxMember > 4)
                {
                    groupContentLayout.cellSize *= new Vector2(Mathf.CeilToInt(maxMember / 2f), 2);
                }
                else
                {
                    groupContentLayout.cellSize *= new Vector2(maxMember, 1);
                    if (maxTeamMemberCount >= 3)
                    {
                        groupContentLayout.constraintCount = 2;
                    }
                }
                int teamCount = 0;
                foreach (Arena.Team arenaTeam in arenaTeams)
                {
                    Transform team = InstantiateWithoutChild(prefab.team, groupContent);
                    GridLayoutGroup teamLayout = team.GetComponent<GridLayoutGroup>();
                    if (arenaTeam.members > 4)
                    {
                        teamLayout.constraintCount = 2;
                    }
                    for (int i = 0; i < arenaTeam.members; i++)
                    {
                        Transform member = Instantiate(prefab.member, team);
                        member.GetComponent<Image>().color = teamColors[teamCount];
                        if (roundCount == 0)
                        {
                            member.GetChild(0).GetComponent<Image>().sprite = ListRandom.In(icons);
                            member.GetChild(1).GetComponent<Text>().text = string.Format("參加者{0}", ++memberCount);
                        }
                    }
                    teamCount++;
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

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(ArenaMenu))]
public class RuleUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ArenaMenu ui = target as ArenaMenu;
        if (GUILayout.Button("Reset"))
        {
            ui.ResetPrefab();
        }
    }
}
#endif