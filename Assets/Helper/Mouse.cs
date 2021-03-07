using UnityEngine;
using UnityEngine.EventSystems;

public static class Mouse
{
    static readonly Camera cam = Camera.main;
    static public Vector3 SnapPosition(float unit)
    {
        Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
        return new Vector3(unit * Mathf.Round(hit.point.x / unit),
                           hit.point.y,
                           unit * Mathf.Round(hit.point.z / unit));
    }
    static public Vector3 HitPoint()
    {
        Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
        return hit.point;
    }
    static public bool Hit(out RaycastHit hit)
    {
        return Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);
    }
    static public bool Hit(out RaycastHit hit, int layer)
    {
        return Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layer);
    }
    static public bool HitGround(out RaycastHit hit)
    {
        return Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 512);
    }
    static public bool Hit<T>(out T obj)
    {
        obj = default;
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity))
        {
            obj = hit.transform.GetComponent<T>();
            return obj != null;
        }
        else
            return false;
    }
    static public bool LeftUp => Input.GetMouseButtonUp(0) && !IsOverUI;
    static public bool LeftDown => Input.GetMouseButtonDown(0) && !IsOverUI;
    static public bool Left => Input.GetMouseButton(0) && !IsOverUI;
    static public bool RightUp => Input.GetMouseButton(1) && !IsOverUI;
    static public bool RightDown => Input.GetMouseButton(1) && !IsOverUI;
    static public bool Right => Input.GetMouseButton(1) && !IsOverUI;
    static public bool IsOverUI => EventSystem.current.IsPointerOverGameObject();
}

