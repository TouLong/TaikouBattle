using UnityEngine;
using UnityEngine.EventSystems;

public class TrackballCamera : MonoBehaviour
{
    [Range(1, 10)]
    public float rotateSensitivity = 5;
    [Range(1, 10)]
    public float zoomSensitivity = 5;
    [Range(1, 10)]
    public float mouseMoveSensitivity = 5;
    public float keyboradMoveSensitivity = 100;
    public float rayHitDistMax = 200;
    public Bounds bounding;
    float rayHitDist;
    Vector3 position;
    float angleX, angleY;
    Quaternion keyboradMoveDir;
    float keyboradMoveSpeed;

    public void Start()
    {
        angleX = transform.eulerAngles.y;
        angleY = -transform.eulerAngles.x;
        position = transform.position;
        Camera.main.orthographic = false;
    }

    void LateUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        rayHitDist = RayHitDistance();
        keyboradMoveDir = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        keyboradMoveSpeed = rayHitDist / rayHitDistMax * keyboradMoveSensitivity * Time.deltaTime;

        if (Input.mouseScrollDelta.y != 0)
            position += transform.forward * Input.mouseScrollDelta.y * rayHitDist * zoomSensitivity / rayHitDistMax;

        if (Input.GetMouseButton(1))
        {
            angleX += Input.GetAxis("Mouse X") * rotateSensitivity;
            angleY += Input.GetAxis("Mouse Y") * rotateSensitivity;
            angleX %= 360;
            angleY = Mathf.Clamp(angleY, -89, 0);
            transform.rotation = Quaternion.Euler(-angleY, angleX, 0);
        }

        if (Input.GetMouseButton(2))
        {
            position += transform.right * -Input.GetAxis("Mouse X") * rayHitDist * mouseMoveSensitivity / rayHitDistMax;
            position += transform.up * -Input.GetAxis("Mouse Y") * rayHitDist * mouseMoveSensitivity / rayHitDistMax;
        }

        if (Input.GetKey(KeyCode.LeftShift))
            keyboradMoveSpeed *= 2.5f;
        if (Input.GetKey(KeyCode.W))
            position += keyboradMoveDir * Vector3.forward * keyboradMoveSpeed;
        if (Input.GetKey(KeyCode.S))
            position -= keyboradMoveDir * Vector3.forward * keyboradMoveSpeed;
        if (Input.GetKey(KeyCode.D))
            position += keyboradMoveDir * Vector3.right * keyboradMoveSpeed;
        if (Input.GetKey(KeyCode.A))
            position -= keyboradMoveDir * Vector3.right * keyboradMoveSpeed;

        position.x = Mathf.Clamp(position.x, bounding.min.x, bounding.max.x);
        position.y = Mathf.Clamp(position.y, bounding.min.y, bounding.max.y);
        position.z = Mathf.Clamp(position.z, bounding.min.z, bounding.max.z);
        transform.position = position;
    }
    float RayHitDistance()
    {
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit);
        if (hit.transform != null)
            return Mathf.Min(rayHitDistMax, hit.distance);
        else
            return rayHitDist;
    }
}
