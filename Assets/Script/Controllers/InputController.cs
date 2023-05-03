using Assets.Script.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    private bool isDraging = false;
    public PlayerController TargetPlayer;
    public CameraController TargetCamera;

    private PlayerCommand playerCommand;
    private MouseCommand mouseCommand;

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

        mouseCommand = new MouseCommand
        {
            deltaX = Input.GetAxis("Mouse X"),
            deltaY = Input.GetAxis("Mouse Y"),
        };

        var h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            CurCommand.DirX = h;
        }

        var v = Input.GetAxisRaw("Vertical");
        if (v != 0)
        {
            CurCommand.DirZ = v;
        }

        if (Input.GetMouseButton(0) && !IsMouseOverUI && !isDraging)
        {
            CurCommand.IsAttack = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CurCommand.IsJump = true;
        }

        SendCommand();
    }
}
