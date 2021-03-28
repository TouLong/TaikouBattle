using System.Collections.Generic;
using UnityEngine;
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
        public UnitInfo info;
        public bool isLose;
    }
    static public int minUnit = 4;
    static public int maxUnit = 8;
    static public int maxGroup = 2;
    static public int maxTeam = 6;
    static public Arena self;
    static public ArenaMenu menu;
    static public Schedule schedule;
    static User user;
    static GameObject unitObjects;

    public Unit unitPrefab;
    public Weapon[] weaponPrefab;
    public Sprite[] iconPrefab;
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
        user.info = new UnitInfo(-1, "", null);
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
        Transform Take(Transform poseList, List<int> idList)
        {
            int id = ListRandom.In(idList);
            idList.Remove(id);
            return poseList.GetChild(id);
        }
        unitObjects = new GameObject("Units");
        Unit.player = null;
        Group group = schedule.CurrentGroup;
        List<int> teamSpawnIDs = Enumerable.Range(0, self.teamLayout.childCount).ToList();
        foreach (Team team in group)
        {
            Transform teamPose = Take(self.teamLayout, teamSpawnIDs);
            List<int> unitSpawnIDs = Enumerable.Range(0, self.unitLayout.childCount).ToList();
            foreach (UnitInfo unitInfo in team.members)
            {
                Transform unitPose = Take(self.unitLayout, unitSpawnIDs);
                Unit unit = Instantiate(self.unitPrefab, unitPose.localPosition + teamPose.position, teamPose.rotation, unitObjects.transform);
                unit.weapon = ListRandom.In(self.weaponPrefab);
                unit.info = unitInfo;
                unitInfo.unit = unit;
                if (unitInfo.id == user.id)
                    Unit.player = unit;
                unit.gameObject.name = string.Format("{0}-{1}", unitInfo.id, unitInfo.name);
            }
            team.Setup();
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
        if (!user.isLose)
            user.isLose = user.info.group == schedule.CurrentGroup && user.info.team != winTeam;
        menu.UpdateWinTeam(schedule.CurrentGroup, winTeam);
        schedule.Next(winTeam);
        if (schedule.IsEndGame)
        {
            Display(ButtonType.End);
            return;
        }
        if (schedule.IsEndRound)
            menu.UpdateSchedule(schedule.units);
        if (schedule.CurrentGroup == user.info.group)
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
        if (schedule.CurrentGroup == user.info.group)
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
        user.isLose = false;
        int count = 0;
        schedule.units = new List<UnitInfo>();
        for (int i = 0; i < unitCount; i++)
        {
            if (i != user.id)
                schedule.units.Add(new UnitInfo(i, string.Format("參賽者 {0}號", ++count), ListRandom.In(self.iconPrefab)));
            else
            {
                user.info = new UnitInfo(user.id, "玩家", ListRandom.In(self.iconPrefab));
                schedule.units.Add(user.info);
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
        schedule.CurrentRound.Assign(schedule.units);
        menu.CreateMain(schedule);
        menu.UpdateSchedule(schedule.units);
    }
}