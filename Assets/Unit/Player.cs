using UnityEngine;

public class Player : Unit
{
    float ap = maxAp;
    float moveConsume, turnConsume;
    void ChangeAp(float value)
    {
        ap = Mathf.Clamp(value, 0, maxAp);
    }
    void ChangeTurningRange(float angle)
    {
        Destroy(turningRange.GetComponent<MeshFilter>().mesh);
        angle = Mathf.Min(angle, 180);
        turningRange.GetComponent<MeshFilter>().mesh = GeoGenerator.SectorPlane((int)angle * 2, 0.7f, 0.67f);
        turningRange.rotation = model.rotation;
        turningRange.position = new Vector3(model.position.x, turningRange.position.y, model.position.z);
    }
    void ChangeAttackRange(Transform parent)
    {
        attackRange.position = new Vector3(parent.position.x, attackRange.position.y, parent.position.z);
        attackRange.rotation = parent.rotation;
    }
    public void ChangeMovingRange(float size)
    {
        movingRange.localScale = new Vector3(size, 1, size);
        movingRange.rotation = model.rotation;
    }
    public void StatusReset()
    {
        moveConsume = 0;
        turnConsume = 0;
        model.localPosition = Vector3.zero;
        model.localEulerAngles = Vector3.zero;
        ChangeMovingRange(maxAp);
        ChangeTurningRange(maxTurning);
        ChangeAttackRange(transform);
        ChangeAp(maxAp);
    }
    public void ControlStart()
    {
        chess.gameObject.SetActive(false);
        StatusReset();
    }
    public void ControlComplete()
    {
        destination.position = model.position;
        destination.rotation = model.rotation;
        chess.gameObject.SetActive(true);
        LineRenderer line = chess.GetComponent<LineRenderer>();
        chess.position = destination.position;
        chess.rotation = destination.rotation;
        line.SetPosition(0, position);
        line.SetPosition(1, chess.position);
        StatusReset();
    }
    public void MoveBack()
    {
        model.localPosition = Vector3.zero;
        Display(Highlight.Attack | Highlight.Outline);
        moveConsume = 0;
        ChangeTurningRange(maxTurning);
        ChangeAttackRange(model);
        ChangeAp(maxAp - turnConsume);
    }
    public void MoveTo(Vector3 to)
    {
        if (turnConsume > 0)
            Display(Highlight.Moving | Highlight.Attack | Highlight.Outline);
        else
            Display(Highlight.All & ~Highlight.Arrow);
        float remain = maxAp - turnConsume;
        Vector3 from = position;
        ChangeMovingRange(remain);
        RaycastHit hit = HitMoveBorder(to - from);
        float dist = Vector3.Distance(from, to);
        if (dist > hit.distance)
            model.position = hit.point;
        else
            model.position = to;
        moveConsume = dist / hit.distance * remain;
        ChangeTurningRange(ap * maxTurning);
        ChangeAttackRange(model);
        ChangeAp(remain - moveConsume);
    }
    public void LookOrigin()
    {
        Display(Highlight.Attack | Highlight.Outline);
        model.localEulerAngles = Vector3.zero;
        ChangeAttackRange(model);
        turnConsume = 0;
        ChangeAp(maxAp - moveConsume);
    }
    public void LookAt(Vector3 to)
    {

        if (moveConsume > 0)
            Display(Highlight.All & ~Highlight.Moving);
        else
            Display(Highlight.Attack | Highlight.Outline | Highlight.Moving);
        float remain = maxAp - moveConsume;
        if (remain > 0)
        {
            float maxAngle = remain * maxTurning;
            float angle = Vector.ForwardSignedAngle(transform, to);
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            model.localEulerAngles = new Vector3(0, angle, 0);
            ChangeAttackRange(model);
            turnConsume = Mathf.Abs(angle / maxAngle) * remain;
            ChangeAp(remain - turnConsume);
            ChangeMovingRange(ap);
        }
    }
}
