using Assets.Script.Models;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private Transform target;
    private const float moveSpeed = 10f;
    
    private Transform visualCenter;
    private Transform mainCamera;
    private Vector3 eulerAngle;
    private Vector3 camLocalPos;

    public CameraCommand Command;

    private void Start()
    {
        visualCenter = transform.Find("VisualCenter");
        mainCamera = transform.Find("VisualCenter/MainCamera");
        eulerAngle = Vector3.zero;
        camLocalPos = mainCamera.localPosition;
        transform.position = target.position;
    }

    public void SetTarget(Transform trans)
    {
        target = trans;
    }

    void ApplyRotation()
    {
        eulerAngle = Command.CamAngle;
        eulerAngle.x = Mathf.Clamp(eulerAngle.x, -Consts.CameraAngleLimitX, Consts.CameraAngleLimitX);
        visualCenter.eulerAngles = eulerAngle;
    }

    void FollowTarget()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
    }

    void ApplyThirdPersonCameraCollision()
    {
        var dir = -mainCamera.forward;
        var layerMask = 1 << LayerMask.NameToLayer("Default");
        if (Physics.Raycast(visualCenter.position, dir, out RaycastHit hit, Consts.MaxCamDistance, layerMask)) // Camera layer is 6
        {
            var distance = Mathf.Max(0, hit.distance - 0.5f);
            camLocalPos.z = -distance;
        }
        mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, camLocalPos, moveSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (target != null && Command != null)
        {
            ApplyRotation();
            FollowTarget();
            ApplyThirdPersonCameraCollision();
        }
    }
}
