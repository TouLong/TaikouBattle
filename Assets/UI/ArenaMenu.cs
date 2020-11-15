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
    public Color[] groupColors = new Color[] { };
    public Color[] teamColors = new Color[] { };
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
    static public ArenaMenu self;
    Dictionary<Arena.Round, Transform> roundDict;
    Dictionary<Arena.Group, Transform> groupDict;
    Dictionary<Team, Transform> teamDict;
    Prefab prefab;
    Framework framework;
    void Awake()
    {
        if (self == null)
            self = this;
    }
    void Start()
    {
        Setup();
        RandomRule();
    }
    public void Setup()
    {
        framework.main = transform.Find("ArenaMenu/Main");
        framework.menu = transform.Find("ArenaMenu/Menu");
        framework.rule = framework.main.GetChild(0);
        Button startButton = framework.menu.Find("Start").GetComponent<Button>();
        Button randomButton = framework.menu.Find("Random").GetComponent<Button>();
        startButton.onClick.AddListener(Confirm);
        randomButton.onClick.AddListener(RandomRule);
        prefab.round = framework.rule.GetChild(0);
        prefab.roundTitle = prefab.round.GetChild(0);
        prefab.roundContent = prefab.round.GetChild(1);
        prefab.group = prefab.roundContent.GetChild(0);
        prefab.groupTitle = prefab.group.GetChild(0);
        prefab.groupContent = prefab.group.GetChild(1);
        prefab.team = prefab.groupContent.GetChild(0);
        prefab.member = prefab.team.GetChild(0);
        ClearMain();
    }
    public void RandomRule()
    {
        Arena.RandomRule();
        CreateMain();
    }
    public void Confirm()
    {
        Arena.self.Spawn();
        gameObject.SetActive(false);
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
    public void ClearMain()
    {
        roundDict = new Dictionary<Arena.Round, Transform>();
        groupDict = new Dictionary<Arena.Group, Transform>();
        teamDict = new Dictionary<Team, Transform>();
        while (framework.rule.childCount != 1)
            DestroyImmediate(framework.rule.GetChild(1).gameObject);
        prefab.round.gameObject.SetActive(false);
    }
    public void CreateMain()
    {
        ClearMain();
        Arena.Schedule schedule = Arena.schedule;
        schedule.Reverse();
        int roundCount = schedule.Count;
        int memberCount = 0;

        foreach (Arena.Round arenaRound in schedule)
        {
            Transform round = InstantiateWithoutChild(prefab.round, framework.rule);
            Transform roundTitle = InstantiateWithoutChild(prefab.roundTitle, round);
            roundTitle.GetComponent<Text>().text = string.Format("回合{0}", roundTexts[--roundCount]);
            Transform roundContent = InstantiateWithoutChild(prefab.roundContent, round);
            roundDict.Add(arenaRound, roundContent);
            int groupCount = 0;
            int maxTeamMemberCount = arenaRound.Max(x => x.eachTeamMember);
            foreach (Arena.Group arenaGroup in arenaRound)
            {
                Transform group = InstantiateWithoutChild(prefab.group, roundContent);
                Transform groupTitle = InstantiateWithoutChild(prefab.groupTitle, group);
                groupTitle.GetComponent<Text>().text = string.Format("場次{0}", groupTexts[groupCount++]);
                Transform groupContent = InstantiateWithoutChild(prefab.groupContent, group);
                groupDict.Add(arenaGroup, groupContent);
                VerticalLayoutGroup groupLayout = group.GetComponent<VerticalLayoutGroup>();
                GridLayoutGroup groupContentLayout = groupContent.GetComponent<GridLayoutGroup>();
                if (arenaGroup.eachTeamMember > 4)
                {
                    groupContentLayout.cellSize *= new Vector2(Mathf.CeilToInt(arenaGroup.eachTeamMember / 2f), 2);
                }
                else
                {
                    groupContentLayout.cellSize *= new Vector2(arenaGroup.eachTeamMember, 1);
                    if (maxTeamMemberCount >= 3)
                    {
                        groupContentLayout.constraintCount = 2;
                    }
                }
                int teamCount = 0;
                foreach (Team arenaTeam in arenaGroup)
                {
                    Transform teamContent = InstantiateWithoutChild(prefab.team, groupContent);
                    teamDict.Add(arenaTeam, teamContent);
                    GridLayoutGroup teamLayout = teamContent.GetComponent<GridLayoutGroup>();
                    if (arenaGroup.eachTeamMember > 4)
                    {
                        teamLayout.constraintCount = 2;
                    }
                    for (int i = 0; i < arenaGroup.eachTeamMember; i++)
                    {
                        Transform unitContent = Instantiate(prefab.member, teamContent);
                        unitContent.GetComponent<Image>().color = teamColors[teamCount];
                    }
                    teamCount++;
                }
            }
        }
        schedule.Reverse();
        UpdaheRound();
    }
    public void UpdateWinTeam()
    {
        Team winTeam = Arena.schedule.CurrentWinTeam;
        if (winTeam != null)
        {
            foreach (Team team in Arena.schedule.CurrentGroup)
            {
                if (winTeam != team)
                {
                    Transform teamContent = teamDict[team];
                    for (int i = 0; i < teamContent.childCount; i++)
                    {
                        teamContent.GetChild(i).GetComponent<Image>().color = Color.gray;
                    }
                }
            }
        }
    }
    public void UpdaheRound()
    {
        foreach (Arena.UnitInfo unit in Arena.schedule.present)
        {
            Transform teamContent = teamDict[unit.team];
            int childID = unit.team.members.IndexOf(unit);
            Transform unitContent = teamContent.GetChild(childID);
            unitContent.GetChild(0).GetComponent<Image>().sprite = unit.icon;
            unitContent.GetChild(1).GetComponent<Text>().text = unit.name;
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ArenaMenu))]
    public class RuleUIEditor : Editor
    {
        int unitCount = Arena.maxGuy;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Arena.self == null)
            {
                Arena.self = FindObjectOfType<Arena>();
            }
            ArenaMenu ui = target as ArenaMenu;
            unitCount = EditorGUILayout.IntSlider("人數", unitCount, Arena.minGuy, Arena.maxGuy);
            if (GUILayout.Button("重置"))
            {
                ui.Setup();
            }
            if (GUILayout.Button("隨機生成"))
            {
                ui.Setup();
                ui.RandomRule();
                unitCount = Arena.schedule.present.Count;
            }
            if (GUILayout.Button("固定生成"))
            {
                ui.Setup();
                Arena.RandomRule(unitCount);
                ui.CreateMain();
            }
            if (GUILayout.Button("下一輪"))
            {
                Arena.RandomJudge();
                ui.UpdateWinTeam();
                Arena.NextContest();
                ui.UpdaheRound();
            }
        }
    }
}
#endif