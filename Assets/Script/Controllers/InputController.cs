using Assets.Script.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    private bool isDraging = false;
    public PlayerController TargetPlayer;

    private PlayerCommand command;
    public PlayerCommand CurCommand
    {
        get
        {
            if (command == null)
                command = new PlayerCommand();
            return command;
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
        if (command != null)
        {
            TargetPlayer.Command = command;
            command = null;
        }
    }

    void Update()
    {
        if (TargetPlayer == null)
            return;

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

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && !isDraging)
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
