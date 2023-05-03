using UnityEngine;


public class CameraController : MonoBehaviour
{
    Transform target;
    float moveSpeed = 20f;
    float verticalRotateSpeed = 1f;
    float horizontalRotateSpeed = 1f;

    private void Start()
    {
        
    }

    public void SetTarget(Transform trans)
    {
        target = trans;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }
}
