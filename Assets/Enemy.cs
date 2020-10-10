using System.Linq;
using UnityEngine;

public class Enemy : Unit
{
    new void Start()
    {
        base.Start();
        AttackMask(true);
    }
    void OnDestroy()
    {
        Enemies.InScene.Remove(this);
    }
    void OnEnable()
    {
        Enemies.InScene.Add(this);
    }
    void OnDisable()
    {
        Enemies.InScene.Remove(this);
    }
}
