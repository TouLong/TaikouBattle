using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSelection : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform frame;
    Vector3 startPoint, endPoint;
    static public bool enable = true;
    static public Action<GameObject> onSelect;
    static public Action onDeselect;
    static public List<GameObject> selectebleObjects;
    void Start()
    {
        if (frame != null)
            frame.gameObject.SetActive(false);
    }
    void Update()
    {
        if (!enable) return;
        if (Mouse.LeftDown)
        {
            startPoint = Input.mousePosition;
            if (Mouse.Hit(out RaycastHit hit))
            {
                onDeselect?.Invoke();
                onSelect?.Invoke(hit.transform.gameObject);
            }
        }
        if (frame == null) return;
        if (Mouse.LeftUp)
        {
            frame.gameObject.SetActive(false);
        }
        if (Mouse.Left)
        {
            endPoint = Input.mousePosition;
            Vector2 size = new Vector2(Mathf.Abs(startPoint.x - endPoint.x), Mathf.Abs(startPoint.y - endPoint.y));
            frame.gameObject.SetActive(true);
            frame.position = (startPoint + endPoint) / 2f;
            frame.sizeDelta = canvas.transform.InverseTransformVector(size.x, size.y, 0);
            Rect selectRect = new Rect(Mathf.Min(startPoint.x, endPoint.x), Mathf.Min(startPoint.y, endPoint.y), size.x, size.y);
            if (selectebleObjects == null) return;
            foreach (GameObject selectebleObject in selectebleObjects)
            {
                if (selectRect.Contains(Camera.main.WorldToScreenPoint(selectebleObject.transform.position)))
                {
                    onSelect?.Invoke(selectebleObject);
                }
            }
        }
    }
}
