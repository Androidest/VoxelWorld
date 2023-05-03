using Assets.Script.Manager;
using Assets.Script.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    private bool isDraging = false;
    public PlayerController TargetPlayer;
    public CameraController TargetCamera;

    private PlayerCommand playerCommand;
    private CameraCommand mouseCommand;
    private Vector3 camAngle;

    public PlayerCommand CurCommand
    {
        get
        {
            if (playerCommand == null)
                playerCommand = new PlayerCommand();
            return playerCommand;
        }
    }

    private void Awake()
    {

    }

    void Start()
    {
    }

    private void SendCommand()
    {
        if (!IsMouseOverUI && !isDraging)
        {
            TargetCamera.Command = mouseCommand;
        }
        if (playerCommand != null)
        {
            TargetPlayer.Command = playerCommand;
            playerCommand = null;
        }
    }

    public bool IsMouseOverUI => EventSystem.current.IsPointerOverGameObject();

    void Update()
    {
        if (TargetPlayer == null)
            return;

        // mouse
        var mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        camAngle += new Vector3(-mouseDelta.y * ConfigManager.Instance.VertCamSpeed, mouseDelta.x * ConfigManager.Instance.HorzCamSpeed, 0);
        mouseCommand = new CameraCommand
        {
            MouseDelta = mouseDelta,
            CamAngle = camAngle
        };

        if (Input.GetMouseButton(0) && !IsMouseOverUI && !isDraging)
        {
            CurCommand.IsAttack = true;
        }

        // movement
        var v = Input.GetAxisRaw("Vertical");
        var h = Input.GetAxisRaw("Horizontal");

        if (h != 0 || v != 0)
        {
            var angle = camAngle.y * Consts.ToRadianMultiplier;
            var s = Mathf.Sin(angle);
            var c = Mathf.Cos(angle);
            var camForwardDir = new Vector3(s * v, 0, c * v);
            var camRightDir = new Vector3(c * h, 0, -s * h);
            CurCommand.Dir = (camForwardDir + camRightDir).normalized;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CurCommand.IsJump = true;
        }

        SendCommand();
    }
}
