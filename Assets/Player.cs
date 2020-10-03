﻿using System;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Player : Unit
{
    static public Player self;
    public Transform model;
    new Collider collider;
    [HideInInspector]
    public Transform indicator;
    void Awake()
    {
        if (self == null)
        {
            self = this;
        }
    }
    new void Start()
    {
        base.Start();
        collider = GetComponent<Collider>();
        indicator = Instantiate(model, transform);
        ResetIndicator();
    }
    public CombatTween Circling()
    {
        Vector3 newPos = indicator.position;
        newPos.y = transform.position.y;
        CombatTween tween = new CombatTween
        {
            lookat = LookAtTween(newPos),
            move = MoveTween(newPos),
            rotate = RotateTween(indicator.rotation),
        };
        return tween;
    }
    public void ResetIndicator()
    {
        indicator.localPosition = Vector3.zero;
        indicator.localEulerAngles = Vector3.zero;
        attackMask.transform.SetParent(transform);
    }
    public void RotateIndicator(Vector3 point)
    {
        Vector3 forward = point - indicator.position;
        forward.y = 0;
        if (forward != Vector3.zero)
            indicator.rotation = Quaternion.LookRotation(forward);
    }
    public void MoveIndicator(Vector3 newPosition)
    {
        Vector3 newPos = Vector3.ClampMagnitude(newPosition - transform.position, moveRange) + transform.position;
        newPos.y = Map.GetHeight(newPos.x, newPos.z);
        indicator.transform.position = newPos;

    }
    public void ShowIndicator(bool show)
    {
        indicator.gameObject.SetActive(show);
        collider.enabled = !show;
        if (show)
        {
            attackMask.transform.SetParent(indicator);
        }
        else
        {
            ResetIndicator();
        }
    }
    public bool HitDetect(out Enemies hitEnemies)
    {
        float distance;
        float angle;
        hitEnemies = new Enemies();
        foreach (Enemy enemy in Enemies.InScene)
        {
            distance = Vector3.Distance(enemy.transform.position, indicator.position);
            if (distance > attackMask.nearLength && distance < attackMask.farLength)
            {
                angle = Vector3.Angle(indicator.forward, enemy.transform.position - indicator.position);

                if (angle <= attackMask.range / 2)
                {
                    hitEnemies.Add(enemy);
                }
            }
        }
        return hitEnemies.Any();
    }
}
