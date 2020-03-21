using System;
using UnityEngine;

public class BattleControl : MonoBehaviour
{
    Player player;
    Action stateUpdate;
    void Start()
    {
        stateUpdate = Selecting;
    }

    void Update()
    {
        stateUpdate();
    }
    void Selecting()
    {
        if (Mouse.Hit(out Player hit))
        {
            hit.MovementShow(true);
            player = hit;
            if (Mouse.LeftDown)
            {
                player.ModelShow(true);
                stateUpdate = SetPosition;
            }
        }
        else if (player != null)
        {
            player.MovementShow(false);
        }
    }
    void SetPosition()
    {
        if (Mouse.Hit(out RaycastHit hit))
        {
            player.ClampModelPosition(new Vector3(hit.point.x, player.transform.position.y, hit.point.z));
            if (Mouse.LeftDown)
            {
                stateUpdate = SetRotation;
                player.AttackShow(true);
            }
        }
        if (Mouse.RightDown)
        {
            player.AttackShow(false);
            player.ModelShow(false);
            stateUpdate = Selecting;
        }
    }
    void SetRotation()
    {
        if (Mouse.Hit(out RaycastHit hit))
        {
            player.RotateModel(new Vector3(hit.point.x, player.transform.position.y, hit.point.z));
            if (Mouse.RightDown)
            {
                player.ResetModel();
                stateUpdate = SetPosition;
            }
            if (Mouse.LeftDown)
            {
                player.MoveToTaget(() => { stateUpdate = Selecting; });
                player.ModelShow(false);
                player.MovementShow(false);
                player.AttackShow(false);
            }
        }
    }
    void Moving()
    {
    }
}
