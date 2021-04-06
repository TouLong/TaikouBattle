using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Arena : MonoBehaviour
{
    public enum ButtonType
    {
        Start = 0b000001,
        Change = 0b000010,
        Skip = 0b000100,
        View = 0b001000,
        End = 0b010000,
        Quit = 0b100000,
    }
    struct User
    {
        public int id;
        public Group group;
    }
    static public int minUnit = 4;
    static public int maxUnit = 16;
    static public int maxGroup = 4;
    static public int maxTeam = 6;
    static public Arena self;
    static public ArenaMenu menu;
    static public Schedule schedule;
    static User user;
    static GameObject unitObjects;

    public GameObject unitPrefab;
    public Weapon[] weapons;
    public Weapon testWeapon;
    public Sprite[] icons;
    public Color[] teamColors = new Color[] { };
    public Transform teamLayout;
    public Transform unitLayout;

    void Awake()
    {
        if (self == null)
            self = this;
    }
    void Start()
    {
        menu = ArenaMenu.self;
        menu.ui.start.onClick.AddListener(ContestStart);
        menu.ui.change.onClick.AddListener(RandomRule);
        menu.ui.skip.onClick.AddListener(ContestComplete);
        menu.ui.view.onClick.AddListener(ContestStart);
        menu.ui.end.onClick.AddListener(End);
        menu.ui.quit.onClick.AddListener(End);
        user.id = -1;
        RandomRule();
    }
    static public void Display(ButtonType button)
    {
        menu.ui.start.gameObject.SetActive(button.HasFlag(ButtonType.Start));
        menu.ui.change.gameObject.SetActive(button.HasFlag(ButtonType.Change));
        menu.ui.skip.gameObject.SetActive(button.HasFlag(ButtonType.Skip));
        menu.ui.view.gameObject.SetActive(button.HasFlag(ButtonType.View));
        menu.ui.end.gameObject.SetActive(button.HasFlag(ButtonType.End));
        menu.ui.quit.gameObject.SetActive(button.HasFlag(ButtonType.Quit));
    }
    static public void ContestStart()
    {
        Transform RandomTake(Transform poseList, List<int> idList)
        {
            int id = ListRandom.In(idList);
            idList.Remove(id);
            return poseList.GetChild(id);
        }
        unitObjects = new GameObject("Units");
        Unit.player = null;
        Group group = schedule.CurrentGroup;
        int teamCount = 0;
        float angleStep = 360 / group.Count;
        foreach (Team team in group)
        {
            Transform teamPose = self.teamLayout.GetChild(teamCount);
            teamPose.position = Vector.DegToXz(teamCount * angleStep) * (4f + 0.1f * group.eachTeamMember);
            teamPose.eulerAngles = new Vector3(0, 180 + teamCount * angleStep, 0);
            teamCount++;
            List<int> unitSpawnIDs = Enumerable.Range(0, self.unitLayout.childCount).ToList();
            bool isPlayerTeam = team.unitInfos.Find(x => x.id == user.id) != null;
            List<Unit> units = new List<Unit>();
            foreach (UnitInfo unitInfo in team.unitInfos)
            {
                Transform unitPose = RandomTake(self.unitLayout, unitSpawnIDs);
                GameObject go = Instantiate(self.unitPrefab,
                    teamPose.position + unitPose.localPosition, teamPose.rotation, unitObjects.transform);
                Unit unit;
                if (isPlayerTeam)
                    unit = go.AddComponent<Player>();
                else
                    unit = go.AddComponent<Npc>();
                if (unitInfo.id == user.id)
                {
                    Unit.player = unit;
                    unit.weapon = self.testWeapon;
                }
                else
                {
                    unit.weapon = ListRandom.In(self.weapons);
                }
                unit.info = unitInfo;
                unit.gameObject.name = string.Format("{0}-{1}", unitInfo.id, unitInfo.name);
                unit.team = team;
                units.Add(unit);
            }
            team.Setup(units);
        }
        DelayEvent.Create(0.2f, () =>
        {
            CombatControl.self.Setup();
            menu.gameObject.SetActive(false);
        });
    }
    static public void ContestComplete(Team winTeam)
    {
        Destroy(unitObjects);
        menu.gameObject.SetActive(true);
        Next(winTeam);
    }
    static public void ContestComplete()
    {
        Next(ListRandom.In(schedule.CurrentGroup));
    }
    static public void Next(Team winTeam)
    {
        menu.UpdateWinTeam(schedule.CurrentGroup, winTeam);
        schedule.Next(winTeam);
        if (schedule.IsEndGame)
        {
            Display(ButtonType.End);
            return;
        }
        if (schedule.IsEndRound)
        {
            user.group = schedule.CurrentRound.GetGroup(user.id);
            menu.UpdateSchedule(schedule.units);
        }
        if (schedule.CurrentGroup == user.group)
            Display(ButtonType.Start | ButtonType.Quit);
        else
            Display(ButtonType.Skip | ButtonType.View | ButtonType.Quit);
    }
    static public void End()
    {
        RandomRule();
    }
    static public void RandomRule()
    {
        RandomRule(Random.Range(minUnit, maxUnit + 1));
        if (schedule.CurrentGroup == user.group)
        {
            Display(ButtonType.Start | ButtonType.Change);
        }
        else
        {
            Display(ButtonType.Skip | ButtonType.View | ButtonType.Change);
        }
    }
    static public void RandomRule(int unitCount)
    {
        schedule = new Schedule(unitCount, maxTeam, maxGroup);
        user.id = Random.Range(0, unitCount);
        int count = 0;
        schedule.units = new List<UnitInfo>();
        foreach (Group group in schedule.CurrentRound)
        {
            int teamCount = 0;
            foreach (Team team in group)
            {
                team.color = self.teamColors[teamCount++];

                for (int i = 0; i < group.eachTeamMember; i++)
                {
                    Sprite icon = ListRandom.In(self.icons);
                    string name;
                    if (count != user.id)
                        name = string.Format("參賽者 {0}號", count + 1);
                    else
                        name = "玩家";
                    UnitInfo info = new UnitInfo
                    {
                        id = count,
                        name = name,
                        icon = icon
                    };
                    schedule.units.Add(info);
                    info.team = team;
                    team.unitInfos.Add(info);
                    count++;
                }
            }
        }
        foreach (Round round in schedule)
        {
            foreach (Group group in round)
            {
                int teamCount = 0;
                foreach (Team team in group)
                {
                    team.color = self.teamColors[teamCount++];
                }
            }
        }
        user.group = schedule.CurrentRound.GetGroup(user.id);
        menu.CreateMain(schedule);
        menu.UpdateSchedule(schedule.units);
    }
}

[CustomEditor(typeof(Arena))]
public class ArenaEditor : Editor
{
    int teamCount;
    float spacing;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Arena arena = target as Arena;
        teamCount = EditorGUILayout.IntSlider("Team Count", teamCount, 1, 6);
        spacing = EditorGUILayout.Slider("Spacing", spacing, 1f, 10f);
        if (GUILayout.Button("Team layout"))
        {
            float step = 360 / teamCount;
            for (int i = 0; i < arena.teamLayout.childCount; i++)
            {
                Transform layout = arena.teamLayout.GetChild(i);
                layout.position = Vector.DegToXz(step * i) * spacing;
                layout.eulerAngles = new Vector3(0, step * i + 180, 0);
                layout.gameObject.SetActive(i < teamCount);
            }
        }
    }
}
