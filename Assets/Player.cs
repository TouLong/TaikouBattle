using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public Transform playerPrefab;
    new Collider collider;
    MovementRange movementRange;
    AttackRange attackRange;
    Transform indicator;
    void Start()
    {
        movementRange = FindObjectOfType<MovementRange>();
        attackRange = FindObjectOfType<AttackRange>();
        collider = GetComponent<Collider>();
        indicator = Instantiate(playerPrefab, transform);
        MovementShow(false);
        AttackShow(false);
        ModelShow(false);
        attackRange.transform.SetParent(indicator);
    }
    void Update()
    {

    }
    public void MoveToTaget(TweenCallback onCompleted)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLookAt(indicator.position, 1));
        sequence.Append(transform.DOMove(indicator.position, 1));
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
        indicator.position = Vector3.ClampMagnitude(newPosition - transform.position, movementRange.size / 2) + transform.position;
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
