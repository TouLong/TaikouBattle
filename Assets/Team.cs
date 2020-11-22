using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Team
{
    public class ActionPath
    {
        public Vector3 start;
        public Vector3 end;
        public ActionPath(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }
    }
    public static List<Team> All = new List<Team>();
    public static List<Team> NonUser = new List<Team>();
    public List<UnitInfo> members = new List<UnitInfo>();
    public Color color = Color.black;
    [HideInInspector]
    public List<Unit> alives = new List<Unit>();
    [HideInInspector]
    public List<Unit> enemies = new List<Unit>();
    [HideInInspector]
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
    public void Action()
    {
        List<ActionPath> paths = new List<ActionPath>();
        foreach (Unit unit in alives)
        {
            Unit target = enemies.OrderBy(x => Vector3.Distance(unit.transform.position, x.transform.position)).First();
            Pose dest;
            Vector3 originXZ = Vector.XZ(unit.transform.position);
            Vector3 guessPosXZ = V3Random.RangeXZ(-target.moveDistance, target.moveDistance) + Vector.XZ(target.transform.position);
            float guessDist = Vector3.Distance(originXZ, guessPosXZ);
            bool isBlocking;
            int blockingCount = -1;
            do
            {
                if (guessDist < unit.moveDistance + unit.weapon.farLength)
                {
                    Vector3 guessDir = (guessPosXZ - originXZ).normalized;
                    float guessAngle = Vector3.SignedAngle(Vector3.forward, guessDir, Vector3.up);
                    float randomMoveAngle = Random.Range(-unit.weapon.angle / 2, unit.weapon.angle / 2);
                    float randomMoveDist = Mathf.Clamp(guessDist - Random.Range(unit.weapon.nearLength, unit.weapon.farLength), -unit.moveDistance, unit.moveDistance);
                    Vector3 randomMoveDir = Vector.DegreeToXZ(guessAngle + randomMoveAngle);
                    dest.position = randomMoveDir * randomMoveDist + originXZ;
                    float randomPoseAngle = Random.Range(-unit.weapon.angle / 2, unit.weapon.angle / 2);
                    Vector3 randomPoseDir = Vector.DegreeToXZ(guessAngle + randomPoseAngle);
                    dest.rotation = Quaternion.LookRotation(randomPoseDir);
                }
                else
                {
                    dest.position = Vector3.ClampMagnitude(guessPosXZ - originXZ, unit.moveDistance) + originXZ;
                    dest.rotation = Quaternion.LookRotation(guessPosXZ - originXZ);
                }
                isBlocking = false;
                foreach (ActionPath path in paths)
                {
                    isBlocking |= Intersect.V3XZ(originXZ, dest.position, path.start, path.end);
                }
                blockingCount++;
            }
            while (isBlocking && blockingCount < 100);
            if (blockingCount == 100)
                Debug.Log(unit.name + "blocking" + blockingCount);
            paths.Add(new ActionPath(originXZ, dest.position));
            dest.position.y = unit.transform.position.y;
            unit.action = DOTween.Sequence()
               .Append(unit.LookAtTween(dest.position))
               .Append(unit.MoveTween(dest.position))
               .Append(unit.RotateTween(dest.rotation));
        }
    }
}
public class UnitInfo
{
    public int id;
    public string name;
    public Sprite icon;
    public Color color;
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