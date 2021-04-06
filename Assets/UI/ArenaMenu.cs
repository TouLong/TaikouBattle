using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System;

public class ArenaMenu : MonoBehaviour
{
    [Serializable]
    public struct UI
    {
        public RectTransform main;

        public RectTransform schedule;
        public RectTransform round;
        public Text roundTitle;
        public RectTransform roundContent;
        public RectTransform group;
        public Text groupTitle;
        public RectTransform groupContent;
        public RectTransform team;
        public RectTransform member;

        public RectTransform action;
        public Button change;
        public Button start;
        public Button skip;
        public Button view;
        public Button end;
        public Button quit;
    }
    public UI ui;
    public string[] roundTexts = new string[] { "I", "II", "III", "IV", };
    public string[] groupTexts = new string[] { "I", "II", "III", "IV", };
    public Color disableColor;
    static public ArenaMenu self;
    Dictionary<Round, Transform> roundDict;
    Dictionary<Group, Transform> groupDict;
    Dictionary<Team, Transform> teamDict;
    void Awake()
    {
        if (self == null)
            self = this;
    }
    public T InstantiateWithoutChild<T>(T original, Transform parent) where T : Component
    {
        T clone = Instantiate(original, parent);
        clone.gameObject.SetActive(true);
        while (clone.transform.childCount != 0)
        {
            DestroyImmediate(clone.transform.GetChild(0).gameObject);
        }
        return clone;
    }
    public void ClearMain()
    {
        roundDict = new Dictionary<Round, Transform>();
        groupDict = new Dictionary<Group, Transform>();
        teamDict = new Dictionary<Team, Transform>();
        while (ui.schedule.childCount != 1)
            DestroyImmediate(ui.schedule.GetChild(1).gameObject);
        ui.round.gameObject.SetActive(false);
    }
    public void CreateMain(Schedule schedule)
    {
        ClearMain();
        schedule.Reverse();
        int roundCount = schedule.Count;
        foreach (Round arenaRound in schedule)
        {
            Transform round = InstantiateWithoutChild(ui.round, ui.schedule);
            Text roundTitle = InstantiateWithoutChild(ui.roundTitle, round);
            roundTitle.text = string.Format("回合{0}", roundTexts[--roundCount]);
            Transform roundContent = InstantiateWithoutChild(ui.roundContent, round);
            roundDict.Add(arenaRound, roundContent);
            int groupCount = 0;
            int maxTeamMemberCount = arenaRound.Max(x => x.eachTeamMember);
            foreach (Group arenaGroup in arenaRound)
            {
                Transform group = InstantiateWithoutChild(ui.group, roundContent);
                Text groupTitle = InstantiateWithoutChild(ui.groupTitle, group);
                groupTitle.text = string.Format("場次{0}", groupTexts[groupCount++]);
                Transform groupContent = InstantiateWithoutChild(ui.groupContent, group);
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
                    Transform teamContent = InstantiateWithoutChild(ui.team, groupContent);
                    teamDict.Add(arenaTeam, teamContent);
                    GridLayoutGroup teamLayout = teamContent.GetComponent<GridLayoutGroup>();
                    if (arenaGroup.eachTeamMember > 4)
                    {
                        teamLayout.constraintCount = 2;
                    }
                    for (int i = 0; i < arenaGroup.eachTeamMember; i++)
                    {
                        Transform unitContent = Instantiate(ui.member, teamContent);
                        unitContent.GetComponent<Image>().color = arenaTeam.color;
                    }
                    teamCount++;
                }
            }
        }
        schedule.Reverse();
    }
    public void UpdateWinTeam(List<Team> teams, Team winTeam)
    {
        foreach (Team team in teams)
        {
            if (winTeam != team)
            {
                Transform teamContent = teamDict[team];
                for (int i = 0; i < teamContent.childCount; i++)
                {
                    teamContent.GetChild(i).GetComponent<Image>().color = disableColor;
                }
            }
        }
    }
    public void UpdateSchedule(List<UnitInfo> units)
    {
        foreach (UnitInfo unit in units)
        {
            Transform teamContent = teamDict[unit.team];
            int childID = unit.team.unitInfos.IndexOf(unit);
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
        int unitCount = Arena.maxUnit;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ArenaMenu ui = target as ArenaMenu;
            if (Arena.self == null)
            {
                Arena.self = FindObjectOfType<Arena>();
                Arena.menu = ui;
            }
            unitCount = EditorGUILayout.IntSlider("人數", unitCount, Arena.minUnit, Arena.maxUnit);
            if (GUILayout.Button("重置"))
            {
                ui.ClearMain();
            }
            if (GUILayout.Button("隨機生成"))
            {
                Arena.RandomRule();
                unitCount = Arena.schedule.units.Count;
            }
            if (GUILayout.Button("固定生成"))
            {
                Arena.RandomRule(unitCount);
            }
            if (GUILayout.Button("下一輪"))
            {
                Arena.ContestComplete();
            }
        }
    }
#endif
}