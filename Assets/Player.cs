using System;
using UnityEngine;
using DG.Tweening;

public class Player : Unit
{
    static public Player InScene;
    public Transform model;
    new Collider collider;
    [HideInInspector]
    public Transform indicator;
    void Awake()
    {
        if (InScene == null)
        {
            InScene = this;
        }
    }
    new void Start()
    {
        base.Start();
        collider = GetComponent<Collider>();
        indicator = Instantiate(model, transform);
        attackRange.transform.SetParent(indicator);
    }

    public void MoveToIndicator(TweenCallback onCompleted)
    {
        float moveAngleOffset = Vector3.Angle(transform.forward, indicator.position - transform.position);
        Vector3 toward = new Vector3(indicator.position.x, transform.position.y, indicator.position.z);
        Sequence sequence = DOTween.Sequence();
        sequence.SetEase(Ease.Linear);
        sequence.Append(transform.DOLookAt(toward, moveAngleOffset * Mathf.Deg2Rad / rotateSpeed));
        sequence.Append(transform.DOMove(indicator.position, Vector3.Distance(indicator.position, transform.position) / moveSpeed).OnUpdate(OnGround));
        sequence.Append(transform.DORotateQuaternion(indicator.rotation, 0.5f / rotateSpeed));
        sequence.PrependInterval(0.05f);
        sequence.AppendCallback(ResetIndicator);
        sequence.OnComplete(onCompleted);
    }
    public void ResetIndicator()
    {
        indicator.localPosition = Vector3.zero;
        indicator.localEulerAngles = Vector3.zero;
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
        Vector3 newPos = Vector3.ClampMagnitude(newPosition - transform.position, moveRangeSize) + transform.position;
        newPos.y = Map.GetHeight(newPos.x, newPos.z);
        indicator.transform.position = newPos;

    }
    public void ShowIndicator(bool show)
    {
        indicator.gameObject.SetActive(show);
        collider.enabled = !show;
        if (!show)
        {
            ResetIndicator();
        }
    }
}
