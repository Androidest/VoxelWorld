using Assets.Script.Models;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private Transform target;
    private float moveSpeed = 20f;
    private float verticalRotateSpeed = 5f;
    private float horizontalRotateSpeed = 5f;
    private Transform visualCenter;
    private Transform mainCamera;
    public MouseCommand Command;

    private void Start()
    {
        visualCenter = transform.Find("VisualCenter");
        mainCamera = transform.Find("VisualCenter/MainCamera");
    }

    public void SetTarget(Transform trans)
    {
        target = trans;
    }

    private void LateUpdate()
    {
        if (target != null && Command != null)
        {
            visualCenter.eulerAngles = visualCenter.eulerAngles + new Vector3(-Command.deltaY * verticalRotateSpeed, Command.deltaX * horizontalRotateSpeed, 0);
            transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }
}
