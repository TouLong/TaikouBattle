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
            player.MovementShow(false);
    }
    void SetPosition()
    {
        if (Mouse.Hit(out RaycastHit hit))
        {
            player.ClampModelPosition(new Vector3(hit.point.x, 0, hit.point.z));
            if (Mouse.RightDown)
            {
                player.ModelShow(false);
                stateUpdate = Selecting;
            }
            if (Mouse.LeftDown)
            {
                stateUpdate = SetRotation;
            }
        }
    }
    void SetRotation()
    {
        if (Mouse.Hit(out RaycastHit hit))
        {
            player.RotateModel(new Vector3(hit.point.x, 0, hit.point.z));
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
            }
        }
    }
    void Moving()
    {
    }
}
