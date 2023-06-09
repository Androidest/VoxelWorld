using Assets.Script.Models;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;

    public PlayerCommand Command;
    private bool isAttacking = false;
    private Quaternion targetDirQuaternion = Quaternion.Euler(0, 0, 0);
    private Vector3 velocity = new Vector3(0, 0, 0);


    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
    }

    void Update()
    {
        var dir = Vector3.zero;
        ApplyGravity();

        if (Command != null)
        {
            dir = Command.Dir;

            if (Command.IsJump && characterController.isGrounded)
                ApplyJump();

            if (Command.IsAttack && !isAttacking)
                ApplyAttack(dir);

            Command = null;
        }

        ApplyRotation(dir);
        ApplyMovement(dir);
        Rotate();
        if (!isAttacking)
            Move();
    }

    private void ApplyGravity()
    {
        if (velocity.y < 0f && characterController.isGrounded)
        {
            velocity.y = -1f;
        }
        else
        {
            velocity.y += Consts.Gravity * Time.deltaTime;
        }
    }

    void ApplyJump()
    {
        velocity.y = Mathf.Sqrt(Consts.PlayerJumpHeight * 2 * -Consts.Gravity);
    }

    private void ApplyMovement(Vector3 dir)
    {
        // apply animation
        if (dir.x == 0 && dir.z == 0)
        {
            animator.SetBool("Walk", false);
        }
        else
        {
            animator.SetBool("Walk", true);
        }
        velocity.x = dir.x * Consts.PlayerMoveSpeed;
        velocity.z = dir.z * Consts.PlayerMoveSpeed;
    }

    private void ApplyRotation(Vector3 dir)
    {
        if (dir.x == 0 && dir.z == 0)
            return;

        targetDirQuaternion = Quaternion.LookRotation(dir);
    }

    private void Rotate()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetDirQuaternion, Time.deltaTime * 10f);
    }

    private void Move()
    {
        characterController.Move(velocity * Time.deltaTime);
    }

    private void ApplyAttack(Vector3 dir)
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
    }

    void StartHit()
    {
        //Debug.Log("StatHit");
    }

    void StopHit()
    {
        isAttacking = false;
        //Debug.Log("StopHit");
    }
}
