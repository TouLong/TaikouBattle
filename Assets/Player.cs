using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public Transform model;
    public Transform movementRange;
    public Transform attackRange;
    new Collider collider;
    Transform indicator;
    void Start()
    {
        collider = GetComponent<Collider>();
        indicator = Instantiate(model, transform);
        MovementShow(false);
        AttackShow(false);
        ModelShow(false);
        attackRange.transform.SetParent(indicator);
        OnGround();
    }
    void OnGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
        {
            transform.position = hit.point;
        }
    }
    public void MoveToTaget(TweenCallback onCompleted)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLookAt(indicator.position, 1));
        sequence.Append(transform.DOMove(indicator.position, 1).OnUpdate(OnGround));
        sequence.Append(transform.DORotateQuaternion(indicator.rotation, 1));
        sequence.PrependInterval(0.1f);
        sequence.AppendCallback(ResetModel);
        sequence.OnComplete(onCompleted);
    }
    public void ResetModel()
    {
        indicator.localPosition = Vector3.zero;
        indicator.localEulerAngles = Vector3.zero;
    }
    public void RotateModel(Vector3 point)
    {
        indicator.rotation = Quaternion.LookRotation(point - indicator.position);
    }
    public void ClampModelPosition(Vector3 newPosition)
    {
        indicator.position = Vector3.ClampMagnitude(newPosition - transform.position, 1) + transform.position;
    }
    public void MovementShow(bool show)
    {
        movementRange.gameObject.SetActive(show);
    }
    public void AttackShow(bool show)
    {
        attackRange.gameObject.SetActive(show);
    }
    public void ModelShow(bool show)
    {
        indicator.gameObject.SetActive(show);
        collider.enabled = !show;
        if (!show)
        {
            ResetModel();
        }
    }
}
