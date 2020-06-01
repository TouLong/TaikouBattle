using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public Transform model;
    public Transform moveRange;
    public Transform attackRange;
    [Range(1f, 20f)]
    public float moveRangeSize;
    [Range(1f, 20f)]
    public float attackRangeSize;
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
    public bool HitDetect(float range, out Transform hitTransform)
    {
        float rad = (range / 2 - indicator.eulerAngles.y + 90) * Mathf.Deg2Rad;
        Vector3 orgin = indicator.position + Vector3.up * 2;
        float far = attackRangeSize * 1f;
        float near = attackRangeSize * 0.2f;
        int layerMask = LayerMask.GetMask("Enemy");
        hitTransform = null;
        for (int i = 0; i < range; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            if (Physics.Raycast(orgin + near * dir, dir, out RaycastHit hit, far, layerMask))
            {
                hitTransform = hit.transform;
                return true;
            }
            rad -= Mathf.Deg2Rad;
        }
        return false;
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
        if (point - indicator.position != Vector3.zero)
            indicator.rotation = Quaternion.LookRotation(point - indicator.position);
    }
    public void ClampModelPosition(Vector3 newPosition)
    {
        indicator.position = Vector3.ClampMagnitude(newPosition - transform.position, moveRangeSize) + transform.position;
    }
    public void MovementShow(bool show)
    {
        moveRange.gameObject.SetActive(show);
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
    void OnValidate()
    {
        if (moveRange)
        {
            moveRange.GetComponentInChildren<Projector>().orthographicSize = moveRangeSize * 1.2f;
        }
        if (attackRange)
        {
            attackRange.GetComponentInChildren<Projector>().orthographicSize = attackRangeSize;
        }
    }

    void OnDrawGizmos()
    {
        if (indicator != null)
        {
            float rad = (45 - indicator.eulerAngles.y + 90) * Mathf.Deg2Rad;
            float far = attackRangeSize * 1f;
            float near = attackRangeSize * 0.2f;
            Vector3 orgin = indicator.position + Vector3.up * 2;
            Gizmos.color = Color.blue;
            for (int i = 0; i < 90; i++)
            {
                Vector3 dir = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
                Gizmos.DrawLine(orgin + near * dir, orgin + far * dir);
                rad -= Mathf.Deg2Rad;
            }
        }

    }
}
