using UnityEngine;

public class Player : Unit
{
    void ChangeAp(float value)
    {
        ap = Mathf.Clamp(value, 0, maxAp);
        status.actionBar.Set(ap);
    }
    void ChangeTurningRange(float angle)
    {
        Mesh mesh = GeoGenerator.SectorPlane((int)angle * 2, 0.7f, 0.67f, 0);
        turningRange.GetComponent<MeshFilter>().mesh = mesh;
        turningRange.rotation = model.rotation;
        turningRange.position = new Vector3(model.position.x, turningRange.position.y, model.position.z);
    }
    public void ResetStatus()
    {
        moveConsume = 0;
        turnConsume = 0;
        ScaleMovingRange(maxAp);
        ChangeTurningRange(maxTurning);
        ChangeAp(maxAp);
        model.localPosition = Vector3.zero;
        model.localEulerAngles = Vector3.zero;
    }
    public void StartControl()
    {
        ResetStatus();
        colliders.Enable(false);
    }
    public void CancelControl()
    {
        ResetStatus();
        colliders.Enable(true);
    }
    public void CompleteControl()
    {
        colliders.Enable(true);
        destination.position = model.position;
        destination.rotation = model.rotation;
        model.localPosition = Vector3.zero;
        model.localEulerAngles = Vector3.zero;
    }
    public void MoveBack()
    {
        model.localPosition = Vector3.zero;
        Display(Range.Attack);
        moveConsume = 0;
        ChangeTurningRange(Unit.maxTurning);
        ChangeAp(maxAp - turnConsume);
    }
    public void MoveTo(Vector3 to)
    {
        if (turnConsume > 0)
            Display(Range.Moving | Range.Attack);
        else
            Display(Range.All & ~Range.Arrow);
        float remain = maxAp - turnConsume;
        Vector3 from = position;
        RaycastHit hit = HitMoveBorder(to - from, remain);
        float dist = Vector3.Distance(from, to);
        if (dist > hit.distance)
            model.position = hit.point;
        else
            model.position = to;
        moveConsume = dist / hit.distance * remain;
        ChangeTurningRange(ap * maxTurning);
        ChangeAp(remain - moveConsume);
    }
    public void LookOrigin()
    {
        Display(Range.Attack);
        model.localEulerAngles = Vector3.zero;
        turnConsume = 0;
        ChangeAp(maxAp - moveConsume);
    }
    public void LookAt(Vector3 to)
    {
        if (moveConsume > 0)
            Display(Range.Attack | Range.Turning | Range.Arrow);
        else
            Display(Range.Attack);
        float remain = maxAp - moveConsume;
        if (remain > 0)
        {
            float maxAngle = remain * Unit.maxTurning;
            float angle = Vector.ForwardSignedAngle(transform, to);
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            model.localEulerAngles = new Vector3(0, angle, 0);
            turnConsume = Mathf.Abs(angle / maxAngle) * remain;
            ChangeAp(remain - turnConsume);
        }
    }
}
