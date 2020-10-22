using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ActionImages : MonoBehaviour
{
    public List<ActionIcon> actionIcons = new List<ActionIcon>();
    string action;
    Image image;
    int id = 0;
    void Start()
    {
        image = GetComponent<Image>();
        action = "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (++id >= actionIcons.Count)
            {
                id = 0;
            }
            image.sprite = actionIcons[id].icon;
        }
    }
}
